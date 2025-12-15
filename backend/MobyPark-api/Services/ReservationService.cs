using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Reservation;

public class ReservationService : IReservationService
{
    private AppDbContext _db;
    public ReservationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReadReservationDto[]> GetAll()
    {
        var reservations = await _db.Reservations.ToArrayAsync();
        ReadReservationDto[] dtos = reservations.Select(ReservationToReadDto).ToArray()!;
        return dtos;
    }

    public async Task<ReadReservationDto?> GetById(string guid)
    {
        var reservation = await _db.Reservations.FindAsync(new Guid(guid));
        var dto = ReservationToReadDto(reservation);
        return dto;
    }

    public async Task<ReadReservationDto?> Post(WriteReservationDto dto)
    {
        var parkinglot = await _db.ParkingLots.FindAsync(dto.ParkingLotId);
        Reservation reservation = new Reservation()
        {
            LicensePlate = dto.LicensePlate,
            ParkingLotId = dto.ParkingLotId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            CreatedAt = DateTime.UtcNow,
            Cost = CalculateReservationCost(dto.StartTime, dto.EndTime, parkinglot)
        };
       await _db.Reservations.AddAsync(reservation);
       await _db.SaveChangesAsync();
       return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Put(string guid, WriteReservationDto dto)
    {
        var parkinglot = await _db.ParkingLots.FindAsync(dto.ParkingLotId);
        Reservation? reservation = await _db.Reservations.FindAsync(new Guid(guid));
        if (reservation == null) return null;
        reservation.LicensePlate = dto.LicensePlate;
        reservation.ParkingLotId = dto.ParkingLotId;
        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Cost = CalculateReservationCost(dto.StartTime, dto.EndTime, parkinglot);
        await _db.SaveChangesAsync();

        return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Delete(string guid)
    {
        Reservation? reservation = await _db.Reservations.FindAsync(new Guid(guid));
        if (reservation == null) return null;
        _db.Reservations.Remove(reservation);
        await _db.SaveChangesAsync();
        return ReservationToReadDto(reservation);
    }

    public ReadReservationDto? ReservationToReadDto(Reservation? reservation)
    {
        if (reservation == null) return null;

        return new ReadReservationDto()
        {
            Id = reservation.Id.ToString(),
            ParkingLotId = reservation.ParkingLotId,
            LicensePlate = reservation.LicensePlate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            CreatedAt = reservation.CreatedAt,
            Cost = reservation.Cost
        };
    }

    public async Task<(bool, string)> IsReservationAllowed(WriteReservationDto reservation)
    {
        if (reservation.StartTime.AddMinutes(30) < DateTime.UtcNow)
            return (false, "Reservation cannot start in the past or within 30 minutes from now");
        if (reservation.StartTime >= reservation.EndTime)
            return (false, "EndTime is smaller than StartTime");
        if (reservation.StartTime + new TimeSpan(0, 15, 0) > reservation.EndTime)
            return (false, "Reservation must be at least 15 minutes");
        if (string.IsNullOrEmpty(reservation.LicensePlate))
            return (false, "licenseplate is empty");
        if (reservation.LicensePlate.Length > 20)
            return (false, "numberplate contains too many characters");
        if (await _db.ParkingLots.FindAsync(reservation.ParkingLotId) == null)
            return (false, "Parkinglot ID does not exist in the database");
        if (await WillParkingLotOverflow(reservation.StartTime, reservation.EndTime, reservation.ParkingLotId))
            return (false, "Parkinglot is full");
        return (true, "reservation allowed");
    }

    public static bool DoTimesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }

    public async Task<bool> WillParkingLotOverflow(DateTime start, DateTime end, long parkingLotId, int baseLoad = 0)
    {
        (DateTime start, DateTime end)[] intersections = _db.Reservations
            .Where(r => r.ParkingLotId == parkingLotId && start < r.EndTime && r.StartTime < end)
            .Where(r => r.Status == ReservationStatus.UnUsed)
            .AsEnumerable()
            .Select(r => (r.StartTime, r.EndTime))
            .ToArray();

        int maxOverlap = CountMaxOverlap(start, end, intersections);
        var parkinglot = await _db.ParkingLots.FindAsync(parkingLotId);

        if (parkinglot == null) 
            throw new ArgumentException("Parkinglot does not exist in database");

        if (maxOverlap + 1 + baseLoad > parkinglot.Capacity)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Counts the highest number of overlaps beween the start and end time span and de ordered interval collection
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="OrderedIntervalCollection">a collection of start and end time with which a intersection might take place.</param>
    /// <returns></returns>
    public static int CountMaxOverlap(DateTime start, DateTime end, (DateTime start, DateTime end)[] intervals)
    {
        List<(DateTime start, DateTime end)> overlappingIntervals = new();
        foreach (var interval in intervals)
        {
            if (DoTimesOverlap(start, end, interval.start, interval.end))
            {
                overlappingIntervals.Add(interval);
            }
        }
        (DateTime start, DateTime end)[] intervalsStart = overlappingIntervals.OrderBy(interval => interval.start).ToArray();
        List<(DateTime start, DateTime end)> sessions = new();
        int parked = 0;
        int max = 0;

        for (int startI = 0; startI < intervalsStart.Length; startI++)
        { // start session
            parked++;
            sessions.Add(intervalsStart[startI]);
            //stop sessions
            for (int endI = 0; endI < sessions.Count;)
            {
                var interval = sessions[endI];
                if (interval.end < intervalsStart[startI].start)
                {
                    sessions.RemoveAt(endI);
                    parked--;
                }
                endI++;
            }
            if (parked > max)
            {
                max = parked;
            }
        }
        return max;
    }

    public float CalculateReservationCost(DateTime start, DateTime end, ParkingLot lot)
    {
        TimeSpan totalTime = end - start;
        if (totalTime >= TimeSpan.FromDays(1) && lot.DayTariff != null)
        {
            return (totalTime.Days * lot.DayTariff.Value);
        }
        return (int)totalTime.TotalHours * lot.Tariff!.Value;
    }

    public async Task<ReadReservationDto?> GetActiveReservation(string licensePlate, DateTime time)
    {
        var reservation = await _db.Reservations
            .Where(
                r => r.LicensePlate == licensePlate 
                && r.StartTime < time 
                && r.EndTime > time)
            .FirstOrDefaultAsync();

        return ReservationToReadDto(reservation);
    }

    public async Task ConsumeReservation(string guid)
    {
        var reservation = await _db.Reservations.FindAsync(guid);
        if (reservation == null) throw new ArgumentException("reservation does not exist");
        reservation.Status = ReservationStatus.Used;
        await _db.SaveChangesAsync();
    }
}


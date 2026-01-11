using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Reservation;
using MobyPark_api.Enums;


public sealed class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IParkingLotRepository _parkingLots;
    private readonly IPricingService _pricingService;
    private readonly IPaymentService _payments;

    public ReservationService(IReservationRepository reservations, IParkingLotRepository parkingLots, IPricingService pricing, IPaymentService payments)
        => (_reservations, _parkingLots, _pricingService) = (reservations, parkingLots, pricing, payments);


    public async Task<ReadReservationDto[]> GetAll()
    {
        var entities = await _reservations.GetAllNoTrackingAsync();
        return entities.Select(ReservationToReadDto)!.ToArray()!;
    }

    public async Task<ReadReservationDto?> GetById(string guid)
    {
        var id = Guid.Parse(guid);
        var entity = await _reservations.GetByIdNoTrackingAsync(id);
        return ReservationToReadDto(entity);
    }

    public async Task<List<ReadReservationDto>?> Post(WriteMultiReservationDto dto)
    {
        List<ReadReservationDto> results = new();

        // Reuse existing single-reservation post per vehicle
        foreach (var plate in dto.LicensePlates)
        {
            var singleDto = new WriteReservationDto
            {
                LicensePlate = plate,
                ParkingLotId = dto.ParkingLotId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            var postedReservation = await Post(singleDto);
            if (postedReservation != null)
                results.Add(postedReservation);
        }

        return results;
    }

    public async Task<ReadReservationDto?> Post(WriteReservationDto dto)
    {
        var lot = await _parkingLots.GetByIdAsync(dto.ParkingLotId);
        if (lot == null) return null;

        var reservation = new Reservation
        {
            LicensePlate = dto.LicensePlate,
            ParkingLotId = dto.ParkingLotId,
            StartTime = dto.StartTime.ToUniversalTime(),
            EndTime = dto.EndTime.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow.ToUniversalTime(),
            Status = ReservationStatus.UnUsed,
            Cost = await CalculateReservationCost(dto.StartTime, dto.EndTime, lot)
        };

        await _reservations.AddAsync(reservation);
        await _reservations.SaveChangesAsync();

        var payment = new AddPaymentDto
        {
            Amount = (decimal)reservation.Cost,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Complete,
            Hash = null,
            Transaction = new TransactionDataDto
            {
                Amount = (decimal)reservation.Cost,
                Date = DateTime.UtcNow,
                Method = "ideal",
                Issuer = "XYY910HH",
                Bank = "ABN-NL"
            }
        };

        await _payments.AddPaymentAsync(payment);
        return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Put(string guid, WriteReservationDto dto)
    {
        var id = Guid.Parse(guid);

        var lot = await _parkingLots.GetByIdAsync(dto.ParkingLotId);
        if (lot == null) return null;

        var reservation = await _reservations.GetByIdAsync(id);
        if (reservation == null) return null;

        reservation.LicensePlate = dto.LicensePlate;
        reservation.ParkingLotId = dto.ParkingLotId;
        reservation.StartTime = dto.StartTime.ToUniversalTime();
        reservation.EndTime = dto.EndTime.ToUniversalTime();
        reservation.Cost = await CalculateReservationCost(dto.StartTime, dto.EndTime, lot);

        await _reservations.SaveChangesAsync();
        return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Delete(string guid)
    {
        var id = Guid.Parse(guid);
        var reservation = await _reservations.GetByIdAsync(id);
        if (reservation == null) return null;

        _reservations.Remove(reservation);
        await _reservations.SaveChangesAsync();
        return ReservationToReadDto(reservation);
    }

    public ReadReservationDto? ReservationToReadDto(Reservation? reservation)
    {
        if (reservation == null) return null;
        return new ReadReservationDto
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

    public async Task<(bool, string)> IsReservationAllowed(WriteMultiReservationDto dto)
    {
        if (dto.LicensePlates == null || dto.LicensePlates.Length == 0)
            return (false, "At least one vehicle is required");

        // Validate parking lot once
        var lot = await _parkingLots.GetByIdAsync(dto.ParkingLotId);
        if (lot == null)
            return (false, "Parkinglot ID does not exist in the database");

        // Capacity check INCLUDING number of vehicles
        if (await WillParkingLotOverflow(
                dto.StartTime,
                dto.EndTime,
                dto.ParkingLotId,
                baseLoad: dto.LicensePlates.Length - 1))
        {
            return (false, "Parkinglot does not have enough capacity for all vehicles");
        }

        // Reuse existing single-reservation validation per vehicle
        foreach (var plate in dto.LicensePlates)
        {
            var singleDto = new WriteReservationDto
            {
                LicensePlate = plate,
                ParkingLotId = dto.ParkingLotId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            var result = await IsReservationAllowed(singleDto);
            if (!result.Item1)
                return result;
        }

        return (true, "multi reservation allowed");
    }

    public async Task<(bool, string)> IsReservationAllowed(WriteReservationDto dto)
    {
        if (dto.StartTime.AddMinutes(30) < DateTime.Now)
            return (false, "Reservation cannot start in the past or within 30 minutes from now");
        if (dto.StartTime >= dto.EndTime)
            return (false, "EndTime is smaller than StartTime");
        if (dto.StartTime.AddMinutes(15) > dto.EndTime)
            return (false, "Reservation must be at least 15 minutes");
        if (string.IsNullOrEmpty(dto.LicensePlate))
            return (false, "licenseplate is empty");
        if (dto.LicensePlate.Length > 20)
            return (false, "numberplate contains too many characters");

        var lot = await _parkingLots.GetByIdAsync(dto.ParkingLotId);
        if (lot == null)
            return (false, "Parkinglot ID does not exist in the database");

        if (await WillParkingLotOverflow(dto.StartTime.ToUniversalTime(), dto.EndTime.ToUniversalTime(), dto.ParkingLotId))
            return (false, "Parkinglot is full");

        return (true, "reservation allowed");
    }

    public async Task<bool> WillParkingLotOverflow(DateTime start, DateTime end, long parkingLotId, int baseLoad = 0)
    {
        var intervals = await _reservations.GetOverlappingUnusedIntervalsAsync(parkingLotId, start, end);

        var maxOverlap = CountMaxOverlap(start, end, intervals);

        var lot = await _parkingLots.GetByIdAsync(parkingLotId);
        if (lot == null)
            throw new ArgumentException("Parkinglot does not exist in database");

        return (maxOverlap + 1 + baseLoad) > lot.Capacity;
    }

    public async Task<ReadReservationDto?> GetActiveReservation(string licensePlate, DateTime time)
    {
        time = time.ToUniversalTime();
        var entity = await _reservations.GetActiveReservationEntityAsync(licensePlate, time);
        return ReservationToReadDto(entity);
    }

    public async Task ConsumeReservation(string guid)
    {
        var id = Guid.Parse(guid);
        var reservation = await _reservations.GetByIdAsync(id);
        if (reservation == null) throw new ArgumentException("reservation does not exist");

        reservation.Status = ReservationStatus.Used;
        await _reservations.SaveChangesAsync();
    }

    public static bool DoTimesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        => start1 < end2 && start2 < end1;

    public static int CountMaxOverlap(DateTime start, DateTime end, (DateTime start, DateTime end)[] intervals)
    {
        var overlapping = new List<(DateTime start, DateTime end)>();
        foreach (var interval in intervals)
            if (DoTimesOverlap(start, end, interval.start, interval.end))
                overlapping.Add(interval);

        var ordered = overlapping.OrderBy(i => i.start).ToArray();
        var active = new List<(DateTime start, DateTime end)>();
        var parked = 0;
        var max = 0;

        for (int i = 0; i < ordered.Length; i++)
        {
            parked++;
            active.Add(ordered[i]);

            for (int j = 0; j < active.Count;)
            {
                if (active[j].end < ordered[i].start)
                {
                    active.RemoveAt(j);
                    parked--;
                }
                else j++;
            }
            if (parked > max) max = parked;
        }
        return max;
    }

    public async Task<decimal> CalculateReservationCost(DateTime start, DateTime end, ParkingLot lot)
    {
        return await _pricingService.GetPrice(start, end, lot.Id);
    }
}



using Microsoft.EntityFrameworkCore;
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
        Reservation reservation = new Reservation() 
        { 
            LicensePlate = dto.LicensePlate, 
            ParkingLotId = dto.ParkingLotId,
            StartTime = dto.StartTime, 
            EndTime = dto.EndTime, 
            CreatedAt = DateTime.UtcNow, 
            Cost = 20 // FIX THIS 
        };
       await _db.Reservations.AddAsync(reservation);
       await _db.SaveChangesAsync();
       return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Put(string guid, WriteReservationDto dto)
    {
        Reservation? reservation = await _db.Reservations.FindAsync(new Guid(guid));
        if (reservation == null) return null;
        reservation.LicensePlate = dto.LicensePlate;
        reservation.ParkingLotId = dto.ParkingLotId;
        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Cost = 20;  // FIX THIS
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
        if (reservation.StartTime <= reservation.EndTime)
            return (false, "StartTime is smaller than EndTime");
        if (reservation.StartTime + new TimeSpan(0, 0, 15) < reservation.EndTime)
            return (false, "Reservation must be at least 15 minutes");
        if (reservation.LicensePlate.Length > 20)
            return (false, "numberplate contains too many characters");
        if (await _db.ParkingLots.FindAsync(reservation.ParkingLotId) == null)
            return (false, "Parkinglot ID does not exist in the database");

        return (true, "reservation allowed");
    }

    public static bool DoTimesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }
}


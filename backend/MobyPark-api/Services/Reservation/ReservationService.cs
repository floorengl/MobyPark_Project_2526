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

    public async Task<ReadReservationDto?> GetById(long id)
    {
        var reservation = await _db.Reservations.FindAsync(id);
        var dto = ReservationToReadDto(reservation);
        return dto;
    }

    public async Task<ReadReservationDto?> Post(WriteReservationDto dto)
    {
        Reservation reservation = new Reservation() 
        { 
            LicensePlate = dto.LicensePlate, 
            StartTime = dto.StartTime, 
            EndTime = dto.EndTime, 
            CreatedAt = DateTime.UtcNow, 
            Cost = dto.Cost 
        };
       await _db.Reservations.AddAsync(reservation);
       await _db.SaveChangesAsync();
       return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Put(long id, WriteReservationDto dto)
    {
        Reservation? reservation = await _db.Reservations.FindAsync(id);
        if (reservation == null) return null;
        reservation.LicensePlate = dto.LicensePlate;
        reservation.ParkingLotId = dto.ParkingLotId;
        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Cost = dto.Cost;
        await _db.SaveChangesAsync();

        return ReservationToReadDto(reservation);
    }

    public async Task<ReadReservationDto?> Delete(long id)
    {
        Reservation? reservation = await _db.Reservations.FindAsync(id);
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
            Id = reservation.Id,
            ParkingLotId = reservation.ParkingLotId,
            LicensePlate = reservation.LicensePlate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            CreatedAt = reservation.CreatedAt,
            Cost = reservation.Cost
        };
    }
}


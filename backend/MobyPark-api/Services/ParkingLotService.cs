using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.ParkingLot;
using MobyPark_api.Services.ParkingLot;

public class ParkingLotService : IParkingLotService
{
    private readonly AppDbContext _context;

    public ParkingLotService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ParkingLotDto>> GetAllAsync()
    {
        return await _context.ParkingLots
            .Select(p => new ParkingLotDto
            {
                Id = p.Id,
                Name = p.Name,
                Location = p.Location,
                Address = p.Address,
                Capacity = p.Capacity,
                Tariff = p.Tariff,
                DayTariff = p.DayTariff,
                CreatedAt = p.CreatedAt,
                Coordinates = p.Coordinates
            }).ToListAsync();
    }

    public async Task<ParkingLotDto> GetByIdAsync(long id)
    {
        var p = await _context.ParkingLots.FindAsync(id);
        if (p == null) return null;

        return new ParkingLotDto
        {
            Id = p.Id,
            Name = p.Name,
            Location = p.Location,
            Address = p.Address,
            Capacity = p.Capacity,
            Tariff = p.Tariff,
            DayTariff = p.DayTariff,
            CreatedAt = p.CreatedAt,
            Coordinates = p.Coordinates
        };
    }

    public async Task<ParkingLotDto> CreateAsync(ParkingLotCreateDto dto)
    {
        var parkingLot = new ParkingLot
        {
            Name = dto.Name,
            Location = dto.Location,
            Address = dto.Address,
            Capacity = dto.Capacity,
            Tariff = dto.Tariff,
            DayTariff = dto.DayTariff,
            Coordinates = dto.Coordinates
        };

        _context.ParkingLots.Add(parkingLot);
        await _context.SaveChangesAsync();

        return new ParkingLotDto
        {
            Id = parkingLot.Id,
            Name = parkingLot.Name,
            Location = parkingLot.Location,
            Address = parkingLot.Address,
            Capacity = parkingLot.Capacity,
            Tariff = parkingLot.Tariff,
            DayTariff = parkingLot.DayTariff,
            CreatedAt = parkingLot.CreatedAt,
            Coordinates = parkingLot.Coordinates
        };
    }

    public async Task<ParkingLotDto> UpdateAsync(long id, ParkingLotUpdateDto dto)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(id);
        if (parkingLot == null) return null;

        parkingLot.Name = dto.Name;
        parkingLot.Location = dto.Location;
        parkingLot.Address = dto.Address;
        parkingLot.Capacity = dto.Capacity;
        parkingLot.Tariff = dto.Tariff;
        parkingLot.DayTariff = dto.DayTariff;
        parkingLot.Coordinates = dto.Coordinates;

        await _context.SaveChangesAsync();

        return new ParkingLotDto
        {
            Id = parkingLot.Id,
            Name = parkingLot.Name,
            Location = parkingLot.Location,
            Address = parkingLot.Address,
            Capacity = parkingLot.Capacity,
            Tariff = parkingLot.Tariff,
            DayTariff = parkingLot.DayTariff,
            CreatedAt = parkingLot.CreatedAt,
            Coordinates = parkingLot.Coordinates
        };
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(id);
        if (parkingLot == null) return false;

        _context.ParkingLots.Remove(parkingLot);
        await _context.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.ParkingLot;
using MobyPark_api.Services.ParkingLot;

public class ParkingLotService : IParkingLotService
{
    private readonly IParkingLotRepository _repo;

    public ParkingLotService(IParkingLotRepository repo) => _repo = repo;

    public async Task<IEnumerable<ParkingLotDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();

        // If you want ordering, do it here (or add a custom repo method later)
        return list
            .OrderBy(p => p.Id)
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
            });
    }

    public async Task<ParkingLotDto?> GetByIdAsync(long id)
    {
        var p = await _repo.GetByIdAsync(id);
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
            Coordinates = dto.Coordinates,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(parkingLot);
        await _repo.SaveChangesAsync();

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

    public async Task<ParkingLotDto?> UpdateAsync(long id, ParkingLotUpdateDto dto)
    {
        // FindAsync via generic GetByIdAsync
        var parkingLot = await _repo.GetByIdAsync(id);
        if (parkingLot == null) return null;

        parkingLot.Name = dto.Name;
        parkingLot.Location = dto.Location;
        parkingLot.Address = dto.Address;
        parkingLot.Capacity = dto.Capacity;
        parkingLot.Tariff = dto.Tariff;
        parkingLot.DayTariff = dto.DayTariff;
        parkingLot.Coordinates = dto.Coordinates;

        await _repo.SaveChangesAsync();

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
        var parkingLot = await _repo.GetByIdAsync(id);
        if (parkingLot == null) return false;

        _repo.Remove(parkingLot);
        await _repo.SaveChangesAsync();
        return true;
    }
}

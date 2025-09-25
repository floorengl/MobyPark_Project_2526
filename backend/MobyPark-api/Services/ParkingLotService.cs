using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;
using MobyPark_api.DTOs;

namespace MobyPark_api.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly AppDbContext _db;
        public ParkingLotService(AppDbContext db) => _db = db;

        private static ParkingLotReadDto ToReadDto(ParkingLot x) =>
            new(
                x.Id, x.Name, x.Location, x.Address, x.Capacity, x.Tariff, x.DayTariff,
                x.CreatedAt, new CoordinatesDto(x.Coordinates.Lat, x.Coordinates.Lng)
            );

        public async Task<ParkingLotReadDto> CreateAsync(CreateParkingLotDto dto, CancellationToken ct)
        {
            var entity = new ParkingLot
            {
                Name = dto.Name,
                Location = dto.Location,
                Address = dto.Address,
                Capacity = dto.Capacity,
                Tariff = dto.Tariff,
                DayTariff = dto.DayTariff,
                Coordinates = new Coordinates { Lat = dto.Coordinates.Lat, Lng = dto.Coordinates.Lng }
            };
            _db.ParkingLots.Add(entity);
            await _db.SaveChangesAsync(ct);
            return ToReadDto(entity);
        }

        public async Task<ParkingLotReadDto?> GetAsync(long id, CancellationToken ct)
        {
            var e = await _db.ParkingLots.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
            return e is null ? null : ToReadDto(e);
        }

        public async Task<IReadOnlyList<ParkingLotReadDto>> ListAsync(CancellationToken ct)
        {
            var list = await _db.ParkingLots.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
            return list.Select(ToReadDto).ToList();
        }

        public async Task<bool> UpdateAsync(long id, UpdateParkingLotDto dto, CancellationToken ct)
        {
            var e = await _db.ParkingLots.SingleOrDefaultAsync(p => p.Id == id, ct);
            if (e is null) return false;

            e.Name = dto.Name;
            e.Location = dto.Location;
            e.Address = dto.Address;
            e.Capacity = dto.Capacity;
            e.Tariff = dto.Tariff;
            e.DayTariff = dto.DayTariff;
            e.Coordinates.Lat = dto.Coordinates.Lat;
            e.Coordinates.Lng = dto.Coordinates.Lng;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken ct)
        {
            var e = await _db.ParkingLots.FindAsync(new object[] { id }, ct);
            if (e is null) return false;
            _db.ParkingLots.Remove(e);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

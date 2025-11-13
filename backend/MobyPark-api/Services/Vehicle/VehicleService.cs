using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Vehicle;

namespace MobyPark_api.Services.VehicleService
{
    public sealed class VehicleService : IVehicleService
    {
        private readonly AppDbContext _db;


        public VehicleService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehicles()
        {
            return await _db.Vehicles
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    Make = v.Make,
                    Model = v.Model,
                    Color = v.Color,
                    Year = v.Year,
                    UserId = v.UserId,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<VehicleDto?> GetByIdAsync(long id, long userId)
        {
            var vehicle = await _db.Vehicles.Where(v => v.UserId == userId).Take(Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (vehicle is null) return null;


            return new VehicleDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Color = vehicle.Color,
                Year = vehicle.Year,
                UserId = vehicle.UserId,
                CreatedAt = vehicle.CreatedAt
            };
        }

        public async Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(long userId)
        {
            return await _db.Vehicles
            .Where(v => v.UserId == userId)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Make = v.Make,
                Model = v.Model,
                Color = v.Color,
                Year = v.Year,
                UserId = v.UserId,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesByUsernameAsync(string username)
        {
            return await _db.Vehicles
            .Where(v => v.User != null && v.User.Username == username)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Make = v.Make,
                Model = v.Model,
                Color = v.Color,
                Year = v.Year,
                UserId = v.UserId,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
        }

        public async Task<VehicleDto?> CreateAsync(VehicleDto dto, long userId)
        {
            if (await _db.Vehicles.AnyAsync(v => v.LicensePlate == dto.LicensePlate))
            {
                return null;
            }
            var vehicle = new Vehicle
            {
                LicensePlate = dto.LicensePlate,
                Make = dto.Make,
                Model = dto.Model,
                Color = dto.Color,
                Year = dto.Year,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };


            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();


            return new VehicleDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Color = vehicle.Color,
                Year = vehicle.Year,
                UserId = vehicle.UserId,
                CreatedAt = vehicle.CreatedAt
            };
        }
        public async Task<VehicleDto?> UpdateAsync(long id, VehicleDto dto, long userId)
        {
            var vehicle = await _db.Vehicles.Where(v => v.UserId == userId).Take(Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (vehicle is null) return null;


            vehicle.LicensePlate = dto.LicensePlate;
            vehicle.Make = dto.Make;
            vehicle.Model = dto.Model;
            vehicle.Color = dto.Color;
            vehicle.Year = dto.Year;


            await _db.SaveChangesAsync();


            return new VehicleDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Color = vehicle.Color,
                Year = vehicle.Year,
                UserId = vehicle.UserId,
                CreatedAt = vehicle.CreatedAt
            };
        }

        public async Task<bool> DeleteAsync(long id, long userId)
        {
            //var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            var vehicle = await _db.Vehicles.Where(v => v.UserId == userId).Take(Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (vehicle is null) return false;


            _db.Vehicles.Remove(vehicle);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

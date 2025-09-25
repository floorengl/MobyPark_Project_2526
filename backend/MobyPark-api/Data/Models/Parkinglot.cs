using System;

namespace MobyPark_api.Data.Models
{
    public class ParkingLot
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Location { get; set; }
        public string? Address { get; set; }
        public int Capacity { get; set; }
        public decimal? Tariff { get; set; }
        public decimal? DayTariff { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Coordinates Coordinates { get; set; } = new();
    }

    public class Coordinates
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}

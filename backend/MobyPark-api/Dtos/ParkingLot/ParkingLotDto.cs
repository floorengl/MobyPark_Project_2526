using System.Drawing;

namespace MobyPark_api.Dtos.ParkingLot
{
    public class ParkingLotDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public long Capacity { get; set; }
        public decimal? Tariff { get; set; }
        public decimal? DayTariff { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Coordinates { get; set; }
    }
}

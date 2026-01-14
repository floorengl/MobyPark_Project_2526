using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace MobyPark_api.Dtos.ParkingLot
{
    public class ParkingLotCreateDto
    {
        [Required]
        public string Name { get; set; } = "";
        [Required]
        public string Location { get; set; } = "";
        public string Address { get; set; } = "";
        public long Capacity { get; set; }
        public decimal? Tariff { get; set; }
        public decimal? DayTariff { get; set; }
        public string Coordinates { get; set; }
    }
}

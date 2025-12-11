using System.ComponentModel.DataAnnotations;

namespace MobyPark_api.Dtos
{
    public sealed class CheckInDto
    {
        [Required, MaxLength(10)]
        public required string LicensePlateName { get; set; }
        
        public required long ParkingLotId { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace MobyPark_api.Data.Models
{
    public class Reservation
    {
        [Key]
        public Guid Id { get; set; }
        public long ParkingLotId { get; set; }
        public ParkingLot ParkingLot { get; set; }
        [Required]
        public required string LicensePlate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public float? Cost { get; set; }
        public ReservationStatus Status {get; set; }

    }
}

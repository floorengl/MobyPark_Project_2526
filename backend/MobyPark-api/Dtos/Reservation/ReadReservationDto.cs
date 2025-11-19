using System.ComponentModel.DataAnnotations;

namespace MobyPark_api.Dtos.Reservation
{
    public class ReadReservationDto
    {
        public long Id { get; init; }
        public long ParkingLotId { get; init; }
        public required string LicensePlate { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public DateTime CreatedAt { get; init; }
        public float? Cost { get; init; }
    }
}

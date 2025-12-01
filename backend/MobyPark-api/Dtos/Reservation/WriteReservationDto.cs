using System.ComponentModel.DataAnnotations;

namespace MobyPark_api.Dtos.Reservation
{
    public class WriteReservationDto
    {
        public long ParkingLotId { get; init; }
        public required string LicensePlate { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
    }
}

namespace MobyPark_api.Dtos.Reservation
{
    public class WriteMultiReservationDto
    {
        public long ParkingLotId { get; init; }
        public required string[] LicensePlates { get; init; } = [];
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
    }
}

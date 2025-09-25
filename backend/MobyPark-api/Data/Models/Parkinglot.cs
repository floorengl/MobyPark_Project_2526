namespace MobyPark_api.Data.Models
{
    public class ParkingLot
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Location { get; set; }
        public int Capacity { get; set; }
    }
}

namespace MobyPark_api.Data.Models
{
    public class DiscountParkingLot
    {
        public long Id { get; set; }
        public Discount Discount { get; set; }
        public required long DiscountId { get; set; }
        public ParkingLot ParkingLot { get; set; }
        public required long ParkingLotId { get; set; }
    }
}

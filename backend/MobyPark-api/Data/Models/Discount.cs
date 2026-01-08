using MobyPark_api.Enums;

namespace MobyPark_api.Data.Models
{
    public class Discount
    {
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public Operator Operator { get; set; }
        public DateTime? Start {  get; set; }
        public DateTime? End { get; set; }
        public long? ParkingLotId { get; set; }
        public ParkingLot? ParkingLot { get; set; }
        public DiscountType DiscountType { get; set; }
        public string? TypeSpecificData { get; set; }
    }
}

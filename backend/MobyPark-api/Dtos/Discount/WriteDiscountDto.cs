using MobyPark_api.Dtos.ParkingLot;
using MobyPark_api.Enums;

namespace MobyPark_api.Dtos.Discount
{
    public class WriteDiscountDto
    {
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public Operator Operator { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public long[]? ParkingLotIds { get; set; }
        public DiscountType DiscountType { get; set; }
        public string? TypeSpecificData { get; set; }
    }
}

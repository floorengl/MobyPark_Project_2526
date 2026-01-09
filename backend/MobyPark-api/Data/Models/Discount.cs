using MobyPark_api.Enums;

namespace MobyPark_api.Data.Models
{
    public class Discount
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public required decimal Amount { get; set; }
        public required Operator Operator { get; set; }
        public DateTime? Start {  get; set; }
        public DateTime? End { get; set; }
        public IList<long>? ParkingLotIds { get; set; }
        public IList<ParkingLot>? ParkingLots { get; set; }
        public required DiscountType DiscountType { get; set; }
        public string? TypeSpecificData { get; set; }
    }
}

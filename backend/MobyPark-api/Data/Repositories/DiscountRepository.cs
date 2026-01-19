using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;

namespace MobyPark_api.Data.Repositories
{
    public class DiscountRepository : GenericRepository<Discount, long>, IDiscountRepository
    {
        public DiscountRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<Discount[]> GetAllForSessionAtLot(DateTime start, DateTime end, long lotId)
        {
            return await _db.Discounts.Where(d =>
                 d.ParkingLotIds == null ? true: d.ParkingLotIds.Contains(lotId) &&
                start < (d.End ?? DateTime.MaxValue) && 
                (d.Start ?? DateTime.MinValue) < end)
            .ToArrayAsync();
        }
    }
}

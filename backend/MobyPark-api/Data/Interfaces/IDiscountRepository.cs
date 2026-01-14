using MobyPark_api.Data.Models;
public interface IDiscountRepository: IGenericRepository<Discount, long>
{
    public Task<Discount[]> GetAllForSessionAtLot(DateTime start, DateTime end, long lotId);
}


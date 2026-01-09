using MobyPark_api.Dtos.Discount;


public interface IDiscountService
{
    Task<IEnumerable<ReadDiscountDto>> GetAllAsync();
    Task<ReadDiscountDto?> GetByIdAsync(long id);
    Task<ReadDiscountDto?> CreateAsync(WriteDiscountDto dto);
    Task<ReadDiscountDto?> UpdateAsync(long id, WriteDiscountDto dto);
    Task<bool> DeleteAsync(long id);
}


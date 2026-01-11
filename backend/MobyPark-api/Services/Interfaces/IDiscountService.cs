using MobyPark_api.Dtos.Discount;


public interface IDiscountService
{
    Task<IEnumerable<ReadDiscountDto>> GetAllAsync();
    Task<ReadDiscountDto?> GetByIdAsync(long id);
    Task<ReadDiscountDto?> CreateAsync(WriteDiscountDto dto);
    Task<ReadDiscountDto?> UpdateAsync(long id, WriteDiscountDto dto);
    Task<bool> DeleteAsync(long id);
    (bool isLegal, string reason) IsModelLegal(WriteDiscountDto dto);
}


using MobyPark_api.Dtos.Discount;
using MobyPark_api.Data.Models;
using MobyPark_api.Enums;

namespace MobyPark_api.Services
{
    public class DiscountService : IDiscountService
    {
        private IDiscountRepository _repo;

        public DiscountService(IDiscountRepository repo) => _repo = repo;

        public async Task<ReadDiscountDto?> CreateAsync(WriteDiscountDto dto)
        {
            Discount discount = new Discount()
            {
                Title = dto.Title,
                Amount = dto.Amount,
                Operator = dto.Operator,
                DiscountType = dto.DiscountType,
                End = dto.End?.ToUniversalTime(),
                Start = dto.Start?.ToUniversalTime(),
                ParkingLotIds = dto.ParkingLotIds,
                TypeSpecificData = dto.TypeSpecificData,
            };
            await _repo.AddAsync(discount);
            await _repo.SaveChangesAsync();

            ReadDiscountDto readDiscountDto = new()
            {
                Id = discount.Id,
                Title = discount.Title,
                Amount = discount.Amount,
                Operator = discount.Operator,
                DiscountType = discount.DiscountType,
                End = discount.End,
                Start = discount.Start,
                ParkingLotIds = discount.ParkingLotIds?.ToArray(),
                TypeSpecificData = discount.TypeSpecificData,
            };
            return readDiscountDto;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var parkingLot = await _repo.GetByIdAsync(id);
            if (parkingLot == null) return false;

            _repo.Remove(parkingLot);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReadDiscountDto>> GetAllAsync()
        {
            List<Discount> discounts = await _repo.GetAllAsync();
            return discounts.Select(discount => new ReadDiscountDto()
            {
                Id = discount.Id,
                Title = discount.Title,
                Amount = discount.Amount,
                Operator = discount.Operator,
                DiscountType = discount.DiscountType,
                End = discount.End,
                Start = discount.Start,
                ParkingLotIds = discount.ParkingLotIds?.ToArray(),
                TypeSpecificData = discount.TypeSpecificData,
            }
            );
            
        }

        public async Task<ReadDiscountDto?> GetByIdAsync(long id)
        {
            Discount? discount = await _repo.GetByIdAsync(id);
            if (discount == null) return null;
            ReadDiscountDto readDiscountDto = new()
            {
                Id = discount.Id,
                Title = discount.Title,
                Amount = discount.Amount,
                Operator = discount.Operator,
                DiscountType = discount.DiscountType,
                End = discount.End,
                Start = discount.Start,
                ParkingLotIds = discount.ParkingLotIds?.ToArray(),
                TypeSpecificData = discount.TypeSpecificData,
            };
            return readDiscountDto;
        }

        public async Task<ReadDiscountDto?> UpdateAsync(long id, WriteDiscountDto dto)
        {

            Discount? discount = await _repo.GetByIdAsync(id);
            if (discount == null) return null;
            
            discount.Title = dto.Title;
            discount.Amount = dto.Amount;
            discount.Operator = dto.Operator;
            discount.DiscountType = dto.DiscountType;
            discount.End = dto.End?.ToUniversalTime();
            discount.Start = dto.Start?.ToUniversalTime();
            discount.ParkingLotIds = dto.ParkingLotIds;
            discount.TypeSpecificData = dto.TypeSpecificData;

            _repo.Update(discount);
            await _repo.SaveChangesAsync();

            ReadDiscountDto readDiscountDto = new()
            {
                Id = discount.Id,
                Title = discount.Title,
                Amount = discount.Amount,
                Operator = discount.Operator,
                DiscountType = discount.DiscountType,
                End = discount.End,
                Start = discount.Start,
                ParkingLotIds = discount.ParkingLotIds?.ToArray(),
                TypeSpecificData = discount.TypeSpecificData,
            };
            return readDiscountDto;
        }

        public (bool isLegal, string reason) IsModelLegal(WriteDiscountDto dto)
        {
            if (dto == null)
                return (false, "discount is null");

            else if (dto.End <= dto.Start)
                return (false, "end cannot be before start");

            else if (dto.Operator == Operator.Multiply && dto.Amount < 0)
                return (false, "discount cannot be a factor and have a negative amount, this would result in negative prices");

            else if (dto.DiscountType == DiscountType.NoExtraCriteria && dto.TypeSpecificData != null)
                return (false, "without extra criteria the typespecific data must be null. but it has a value");
            
            return (true, "");
        }


    }
}

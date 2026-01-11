using MobyPark_api.Data.Models;


/// <summary>
/// Service to calculate prices. Used to apply discounts. Used by reservations and sessions.
/// </summary>
public class PricingService: IPricingService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IParkingLotRepository _lotRepository;

    public PricingService(IDiscountRepository discountRepository, IParkingLotRepository parkingLotRepository) 
    {
        _discountRepository = discountRepository;
        _lotRepository = parkingLotRepository;
    }

    /// <summary>
    /// Helper to calculate price based on predefined information
    /// This method is seperate to simplefy testing
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="lot"></param>
    /// <param name="discounts"></param>
    /// <returns></returns>
    public static decimal CalculatePrice(DateTime start, DateTime end, ParkingLot lot, Discount[] discounts)
    {
        decimal price = 0;
        TimeSpan differance = end - start;
        decimal tariff = (decimal)(lot.Tariff ?? 999);
        decimal dayTariff = (decimal)(lot.DayTariff ?? 999);

        int hours = (int)differance.TotalHours + 1;

        if (differance.TotalMinutes < 3) // first 3 minutes free
        {
            price = 0; 
            return price;
        }
        if (hours <= 24) // first day in hour tariff
        {
            price = tariff * hours;
        }
        else
        {
            price = dayTariff * (hours / 24 + 1);
        }
        price = ApplyDiscount(price, start, end, lot, discounts);

        if (price < 0)
            price = 0;

        price = Math.Round(price, 2);

        return price;
    }

    /// <summary>
    /// Helper to apply discounts
    /// This method is seperate to simplefy testing
    /// </summary>
    /// <param name="price"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="lot"></param>
    /// <param name="discounts"></param>
    /// <returns></returns>
    public static decimal ApplyDiscount(decimal price, DateTime start, DateTime end, ParkingLot lot, Discount[] discounts)
    {
        var discountsForLot = discounts.Where(d => d.ParkingLotIds == null || d.ParkingLotIds.Contains(lot.Id));
        // spread discounts
        Discount[] PlusDiscounts = discountsForLot.Where(d => d.Operator == MobyPark_api.Enums.Operator.Plus).ToArray();
        Discount[] MultDiscounts = discountsForLot.Where(d => d.Operator == MobyPark_api.Enums.Operator.Multiply).ToArray();

        foreach (Discount discount in PlusDiscounts)
        {
            if (start < (discount.End ?? DateTime.MaxValue) && (discount.Start ?? DateTime.MinValue) < end) // if overlaping
            {
                price += discount.Amount;
            }
        }
        foreach (Discount discount in MultDiscounts)
        {
            if (!(start < (discount.End ?? DateTime.MaxValue) && (discount.Start ?? DateTime.MinValue) < end)) // if overlaping
            {
                continue;
            }
            // Overlap = min(A2, B2) - max(A1, B1) + 1
            DateTime minEnd = end < (discount.End ?? DateTime.MaxValue) ? end : discount.End ?? DateTime.MaxValue;
            DateTime maxStart = start > (discount.Start ?? DateTime.MinValue) ? start : discount.Start ?? DateTime.MinValue;
            TimeSpan overlap = minEnd - maxStart;
            TimeSpan sessionDuration = end - start;
            decimal overlapFactor = (decimal)(overlap / sessionDuration);
            decimal appliedFactor = 1 - (decimal)overlapFactor * discount.Amount;
            price *= appliedFactor;
        }
        return price;
    }

    private async Task<Discount[]> GetRelatedDiscounts(DateTime start, DateTime end, long lotId)
    {
        return await _discountRepository.GetAllForSessionAtLot(start, end, lotId);
    }

    public async Task<decimal> GetPrice(DateTime start, DateTime end, long lotId)
    {
        var discounts = await GetRelatedDiscounts(start, end, lotId);
        var lot = await _lotRepository.GetByIdAsync(lotId);
        if (lot == null) throw new ArgumentException($"lot id is not found in database: {lotId}");
        decimal price = CalculatePrice(start, end, lot, discounts);
        return price;
    }

    public async Task<decimal> GetPrice(Session session)
    {
        if (session.Stopped == null) throw new ArgumentException($"Session hasn't been stopped yet but an atempt was made to calculate the price id:{session.Id}");
        decimal price = await GetPrice(session.Started, session.Stopped.Value, session.ParkingLotId);
        return price;
    }

    public async Task<decimal> GetPrice(Reservation reservation)
    {
        decimal price = await GetPrice(reservation.StartTime, reservation.EndTime, reservation.ParkingLotId);
        return price;
    }
}


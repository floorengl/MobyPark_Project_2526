using MobyPark_api.Data.Models;


/// <summary>
/// Service to calculate prices. Used to apply discounts. Used by reservations and sessions.
/// </summary>
public class PricingService: IPricingService
{

    public PricingService() 
    {
        
    }

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

        return price;
    }

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
            double overlapFactor = overlap / sessionDuration;
            decimal appliedFactor = 1 - (decimal)overlapFactor * discount.Amount;
            price *= appliedFactor;
        }
        return price;
    }

    public async Task CreatePayment()
    {

    }
}


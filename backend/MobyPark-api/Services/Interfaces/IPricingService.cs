
using MobyPark_api.Data.Models;

public interface IPricingService
{
    /// <summary>
    /// Method to find the price of parking a vehicle for a amount of time at a parking lot
    /// </summary>
    Task<decimal> GetPrice(DateTime start, DateTime end, long lotId, string licenseplate);

    /// <summary>
    /// Calculate price for a session
    /// </summary>
    /// <param name="session"></param>
    /// <exception cref="ArgumentException">if session end time is null</exception>
    /// <returns>cost</returns>
    Task<decimal> GetPrice(Session session);

    /// <summary>
    /// Calculate price for a discount
    /// </summary>
    /// <param name="reservation"></param>
    /// <returns>cost</returns>
    Task<decimal> GetPrice(Reservation reservation);
}


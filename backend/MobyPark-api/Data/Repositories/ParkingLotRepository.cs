using MobyPark_api.Data.Models;

public sealed class ParkingLotRepository : GenericRepository<ParkingLot, long>, IParkingLotRepository
{
    public ParkingLotRepository(AppDbContext db) : base(db) { }
}

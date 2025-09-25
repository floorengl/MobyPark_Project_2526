using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobyPark_api.Data.Models;

namespace MobyPark_api.Data.Configurations
{
    public class ParkingLotCfg : IEntityTypeConfiguration<ParkingLot>
    {
        public void Configure(EntityTypeBuilder<ParkingLot> e)
        {
            e.ToTable("parking_lots");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(240);
            e.Property(x => x.Capacity).IsRequired();
            e.Property(x => x.Tariff).HasColumnType("decimal(10,2)");
            e.Property(x => x.DayTariff).HasColumnType("decimal(10,2)");

            // Keep the JSON shape: { "coordinates": { "lat": ..., "lng": ... } }
            e.OwnsOne(x => x.Coordinates, nav =>
            {
                nav.Property(p => p.Lat).HasColumnName("coordinates_lat");
                nav.Property(p => p.Lng).HasColumnName("coordinates_lng");
            });
        }
    }
}

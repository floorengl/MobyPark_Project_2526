using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class LicenseplateConfig : IEntityTypeConfiguration<Licenseplate>
{
    public void Configure(EntityTypeBuilder<Licenseplate> l)
    {
        l.ToTable("licenseplates");
        l.HasKey(x => x.Id);

        l.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint");
        l.Property(x => x.LicensePlateName).HasColumnName("licensePlateNames").HasColumnType("text").IsRequired();

    }
}
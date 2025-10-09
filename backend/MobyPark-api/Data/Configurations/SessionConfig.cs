using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SessionConfig : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> s)
    {
        s.ToTable("sessions");
        s.HasKey(x => x.Id);

        s.Property(x => x.Started).IsRequired();

        s.HasOne(x => x.LicensePlate)
         .WithMany(p => p.Sessions)
         .HasForeignKey(x => x.LicensePlateId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
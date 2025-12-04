using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> e)
    {
        e.ToTable("payments");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id)           .HasColumnName("id")            .HasColumnType("bigint");
        e.Property(x => x.CreatedAt)    .HasColumnName("createdat")     .HasColumnType("timestamp");
        e.Property(x => x.Status)       .HasColumnName("status")        .HasColumnType("int");
        e.Property(x => x.Hash)         .HasColumnName("hash")          .HasColumnType("text");
        e.Property(x => x.TData)        .HasColumnName("tdata")         .HasColumnType("text");

    }
}
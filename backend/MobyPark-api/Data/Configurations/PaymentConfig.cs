using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> e)
    {
        e.ToTable("payments");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("uuid_generate_v4()");

        e.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric");

        e.Property(x => x.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamptz");

        e.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("int");

        e.Property(x => x.Hash)
            .HasColumnName("hash")
            .HasColumnType("text");

        e.Property(x => x.TransactionId)
            .HasColumnName("transaction_id")
            .HasColumnType("uuid");

        e.HasOne(x => x.TransactionData)
            .WithOne()
            .HasForeignKey<Payment>(x => x.TransactionId);

    }
}
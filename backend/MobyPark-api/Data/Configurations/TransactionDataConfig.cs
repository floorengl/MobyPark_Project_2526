using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TransactionDataConfig : IEntityTypeConfiguration<TransactionData>
{
    public void Configure(EntityTypeBuilder<TransactionData> e)
    {
        e.ToTable("transaction_data");
        e.HasKey(x => x.TransactionId);

        e.Property(x => x.TransactionId)
            .HasColumnName("transaction_id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("uuid_generate_v4()");

        e.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric");

        e.Property(x => x.Date)
            .HasColumnName("date")
            .HasColumnType("timestamp");

        e.Property(x => x.Method)
            .HasColumnName("method")
            .HasColumnType("text");

        e.Property(x => x.Issuer)
            .HasColumnName("issuer")
            .HasColumnType("text");

        e.Property(x => x.Bank)
            .HasColumnName("bank")
            .HasColumnType("text");
    }
}

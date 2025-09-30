using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.ToTable("users");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id)          .HasColumnName("id")          .HasColumnType("bigint");
        e.Property(x => x.Username)    .HasColumnName("username")    .HasColumnType("text").IsRequired();
        e.Property(x => x.Password)    .HasColumnName("password")    .HasColumnType("text").IsRequired();
        e.Property(x => x.FullName)    .HasColumnName("name")        .HasColumnType("text");
        e.Property(x => x.Email)       .HasColumnName("email")       .HasColumnType("text");
        e.Property(x => x.Phone)       .HasColumnName("phone")       .HasColumnType("text");
        e.Property(x => x.Role)        .HasColumnName("role")        .HasColumnType("text");
        e.Property(x => x.CreatedAtUtc).HasColumnName("created-at")  .HasColumnType("timestamptz");
        e.Property(x => x.BirthYear)   .HasColumnName("birth-year")  .HasColumnType("smallint");
        e.Property(x => x.Active)      .HasColumnName("active")      .HasColumnType("boolean").HasDefaultValue(true);

        e.HasIndex(x => x.Username).IsUnique().HasDatabaseName("ux_users_username");
        e.HasIndex(x => x.Email).HasDatabaseName("ix_users_email");
    }
}

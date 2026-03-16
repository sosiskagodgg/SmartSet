// FitnessTracker.Infrastructure/Data/Configurations/UserConfiguration.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("telegramid")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(100);

        builder.Property(u => u.SubscriptionEndDate)
            .HasColumnName("subscriptionenddate")
            .HasColumnType("timestamp with time zone")  // ИЗМЕНИЛ!
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(u => u.SubscriptionStatus)
            .HasColumnName("subscriptionstatus")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamp with time zone")  // ИЗМЕНИЛ!
            .HasConversion<UtcDateTimeConverter>()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updatedat")
            .HasColumnType("timestamp with time zone")  // ИЗМЕНИЛ!
            .HasConversion<UtcDateTimeConverter>()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(u => u.Username)
            .HasDatabaseName("idx_users_username");

        builder.HasIndex(u => new { u.SubscriptionStatus, u.SubscriptionEndDate })
            .HasDatabaseName("idx_users_subscription");
    }
}
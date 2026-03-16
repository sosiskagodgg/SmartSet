// FitnessTracker.Infrastructure/Data/Configurations/UserConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

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
            .HasColumnType("timestamp without time zone");  // Явно указываем тип

        builder.Property(u => u.SubscriptionStatus)
            .HasColumnName("subscriptionstatus")
            .IsRequired()
            .HasMaxLength(50);

        // ИСПРАВЛЕНО: добавляем конвертер для DateTime
        builder.Property(u => u.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamp without time zone")
            .HasConversion(
                v => v,  // при записи оставляем как есть
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)  // при чтении устанавливаем Kind=Utc
            )
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updatedat")
            .HasColumnType("timestamp without time zone")
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            )
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(u => u.Username)
            .HasDatabaseName("idx_users_username");

        builder.HasIndex(u => new { u.SubscriptionStatus, u.SubscriptionEndDate })
            .HasDatabaseName("idx_users_subscription");
    }
}
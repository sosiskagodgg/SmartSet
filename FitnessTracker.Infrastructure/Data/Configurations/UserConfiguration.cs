using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.TelegramId);

            builder.Property(u => u.TelegramId)
                .HasColumnName("telegramid")  // ← ДОБАВИЛ!
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(u => u.Name)
                .HasColumnName("name")        // ← ДОБАВИЛ!
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Username)
                .HasColumnName("username")    // ← ДОБАВИЛ!
                .HasMaxLength(100);

            builder.Property(u => u.SubscriptionStatus)
                .HasColumnName("subscriptionstatus")  // ← ДОБАВИЛ!
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.SubscriptionEndDate)
                .HasColumnName("subscriptionenddate")  // ← ДОБАВИЛ!
                .HasColumnType("timestamp");

            builder.HasIndex(u => u.Username)
                .HasDatabaseName("idx_users_username");

            builder.HasIndex(u => new { u.SubscriptionStatus, u.SubscriptionEndDate })
                .HasDatabaseName("idx_users_subscription");
        }
    }
}
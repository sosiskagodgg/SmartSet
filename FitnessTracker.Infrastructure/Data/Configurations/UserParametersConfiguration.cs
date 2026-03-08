using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations
{
    public class UserParametersConfiguration : IEntityTypeConfiguration<UserParameters>
    {
        public void Configure(EntityTypeBuilder<UserParameters> builder)
        {
            builder.ToTable("userparameters");

            builder.HasKey(up => up.TelegramId);

            builder.Property(up => up.TelegramId)
                .HasColumnName("telegramid")  // ← ДОБАВИЛ!
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(up => up.Height)
                .HasColumnName("height")      // ← ДОБАВИЛ!
                .IsRequired(false);

            builder.Property(up => up.Weight)
                .HasColumnName("weight")      // ← ДОБАВИЛ!
                .IsRequired(false)
                .HasPrecision(5, 2);

            builder.Property(up => up.BodyFat)
                .HasColumnName("bodyfat")     // ← ДОБАВИЛ!
                .IsRequired(false)
                .HasPrecision(4, 1);

            builder.Property(up => up.Experience)
                .HasColumnName("experience")  // ← ДОБАВИЛ!
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(up => up.Goals)
                .HasColumnName("goals")       // ← ДОБАВИЛ!
                .IsRequired(false);

            builder.HasOne<User>()
                .WithOne()
                .HasForeignKey<UserParameters>(up => up.TelegramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(up => up.TelegramId)
                .HasDatabaseName("idx_userparameters_telegram");
        }
    }
}
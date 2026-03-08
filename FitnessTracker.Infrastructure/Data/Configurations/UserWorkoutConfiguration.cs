using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations
{
    public class UserWorkoutConfiguration : IEntityTypeConfiguration<UserWorkout>
    {
        public void Configure(EntityTypeBuilder<UserWorkout> builder)
        {
            builder.ToTable("userworkouts");

            builder.HasKey(uw => new { uw.TelegramId, uw.DayNumber });

            builder.Property(uw => uw.TelegramId)
                .HasColumnName("telegramid")  // ← ДОБАВИЛ!
                .IsRequired();

            builder.Property(uw => uw.DayNumber)
                .HasColumnName("daynumber")   // ← ДОБАВИЛ!
                .IsRequired();

            builder.Property(uw => uw.Name)
                .HasColumnName("name")        // ← ДОБАВИЛ!
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(uw => uw.Exercises)
                .HasColumnName("exercises")   // ← ДОБАВИЛ!
                .IsRequired()
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(uw => uw.TelegramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(uw => new { uw.TelegramId, uw.DayNumber })
                .HasDatabaseName("idx_userworkouts_day");
        }
    }
}
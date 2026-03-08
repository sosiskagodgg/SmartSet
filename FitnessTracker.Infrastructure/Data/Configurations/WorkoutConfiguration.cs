using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations
{
    public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
    {
        public void Configure(EntityTypeBuilder<Workout> builder)
        {
            builder.ToTable("workouts");

            builder.HasKey(w => new { w.TelegramId, w.Date });

            builder.Property(w => w.TelegramId)
                .HasColumnName("telegramid")  // ← ДОБАВИЛ!
                .IsRequired();

            builder.Property(w => w.Date)
                .HasColumnName("date")        // ← ДОБАВИЛ!
                .IsRequired()
                .HasColumnType("date");

            builder.Property(w => w.Exercises)
                .HasColumnName("exercises")   // ← ДОБАВИЛ!
                .IsRequired()
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(w => w.TelegramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(w => new { w.TelegramId, w.Date })
                .HasDatabaseName("idx_workouts_date");
        }
    }
}
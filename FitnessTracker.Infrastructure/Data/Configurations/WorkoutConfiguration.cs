// FitnessTracker.Infrastructure/Data/Configurations/WorkoutConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Infrastructure.Data.Converters;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("workouts");

        builder.Ignore(w => w.Id);
        builder.HasKey(w => new { w.TelegramId, w.Date });

        builder.Property(w => w.TelegramId)
            .HasColumnName("telegramid")
            .IsRequired();

        builder.Property(w => w.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Ignore(w => w.Notes);
        builder.Ignore(w => w.Status);
        builder.Ignore(w => w.TotalDuration);
        builder.Ignore(w => w.TotalCaloriesBurned);

        // ИСПОЛЬЗУЕМ ТОТ ЖЕ КОНВЕРТЕР ЧТО И ДЛЯ USERWORKOUT
        builder.Property(w => w.Exercises)
            .HasColumnName("exercises")
            .HasColumnType("jsonb")
            .HasConversion(new ExerciseListConverter());  // ИЗМЕНЕНО!

        builder.HasIndex(w => new { w.TelegramId, w.Date })
            .HasDatabaseName("idx_workouts_telegram_date")
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
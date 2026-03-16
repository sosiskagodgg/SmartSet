// FitnessTracker.Infrastructure/Data/Configurations/WorkoutConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Infrastructure.Data.Converters;
using System.Text.Json;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("workouts");

        builder.HasKey(w => new { w.TelegramId, w.Date });

        builder.Property(w => w.TelegramId)
            .HasColumnName("telegramid")
            .IsRequired();

        builder.Property(w => w.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(w => w.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(w => w.TotalDuration)
            .HasColumnName("total_duration");

        builder.Property(w => w.TotalCaloriesBurned)
            .HasColumnName("total_calories");

        // Настройка JSON сериализации для Exercises
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new ExerciseJsonConverter() },
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        builder.Property(w => w.Exercises)
            .HasColumnName("exercises")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<Exercise>>(v, jsonOptions) ?? new()
            )
            .IsRequired();

        builder.HasIndex(w => new { w.TelegramId, w.Date })
            .HasDatabaseName("idx_workouts_telegram_date")
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
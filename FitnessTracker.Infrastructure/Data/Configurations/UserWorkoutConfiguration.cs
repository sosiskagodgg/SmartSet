// FitnessTracker.Infrastructure/Data/Configurations/UserWorkoutConfiguration.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class UserWorkoutConfiguration : IEntityTypeConfiguration<UserWorkout>
{
    public void Configure(EntityTypeBuilder<UserWorkout> builder)
    {
        builder.ToTable("userworkouts");

        builder.HasKey(uw => uw.Id);

        builder.Property(uw => uw.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(uw => uw.TelegramId)
            .HasColumnName("telegramid")
            .IsRequired();

        builder.Property(uw => uw.DayNumber)
            .HasColumnName("daynumber")
            .IsRequired();

        builder.Property(uw => uw.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        // ИСПРАВЛЕНО: timestamp with time zone
        builder.Property(uw => uw.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamp with time zone")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        // ИСПРАВЛЕНО: timestamp with time zone для nullable поля
        builder.Property(uw => uw.LastModified)
            .HasColumnName("lastmodified")
            .HasColumnType("timestamp with time zone")
            .HasConversion<UtcNullableDateTimeConverter>();

        // Настройка JSON сериализации для Exercises
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new ExerciseJsonConverter() },
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Добавляем ValueComparer для отслеживания изменений в коллекции
        var exercisesComparer = new ValueComparer<List<Exercise>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(uw => uw.Exercises)
            .HasColumnName("exercises")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<Exercise>>(v, jsonOptions) ?? new())
            .Metadata.SetValueComparer(exercisesComparer);

        builder.HasIndex(uw => new { uw.TelegramId, uw.DayNumber })
            .HasDatabaseName("idx_userworkouts_telegram_day")
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(uw => uw.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
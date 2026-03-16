// FitnessTracker.Infrastructure/Data/Configurations/UserWorkoutConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Infrastructure.Data.Converters;
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

        // ИСПРАВЛЕНО: добавляем конвертер для DateTime
        builder.Property(uw => uw.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamp without time zone")
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            )
            .IsRequired();

        builder.Property(uw => uw.LastModified)
            .HasColumnName("lastmodified")
            .HasColumnType("timestamp without time zone")
            .HasConversion(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null
            );

        // Настройка JSON сериализации для Exercises
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new ExerciseJsonConverter() },
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        builder.Property(uw => uw.Exercises)
            .HasColumnName("exercises")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<Exercise>>(v, jsonOptions) ?? new()
            );

        builder.HasIndex(uw => new { uw.TelegramId, uw.DayNumber })
            .HasDatabaseName("idx_userworkouts_telegram_day")
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(uw => uw.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
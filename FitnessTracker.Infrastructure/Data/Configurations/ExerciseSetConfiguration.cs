// FitnessTracker.Infrastructure/Data/Configurations/ExerciseSetConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class ExerciseSetConfiguration : IEntityTypeConfiguration<ExerciseSet>
{
    public void Configure(EntityTypeBuilder<ExerciseSet> builder)
    {
        builder.ToTable("exercise_sets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.WorkoutExerciseId)
            .HasColumnName("workout_exercise_id")
            .IsRequired();

        builder.Property(s => s.SetNumber)
            .HasColumnName("set_number")
            .IsRequired();

        builder.Property(s => s.Reps)
            .HasColumnName("reps");

        builder.Property(s => s.Weight)
            .HasColumnName("weight")
            .HasPrecision(6, 2);

        builder.Property(s => s.DurationSeconds)
            .HasColumnName("duration_seconds");

        builder.Property(s => s.DistanceMeters)
            .HasColumnName("distance_meters")
            .HasPrecision(8, 2);

        builder.Property(s => s.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(true);

        builder.Property(s => s.Notes)
            .HasColumnName("notes");

        builder.Property(s => s.CompletedAt)
            .HasColumnName("completed_at");

        // Индексы
        builder.HasIndex(s => s.WorkoutExerciseId)
            .HasDatabaseName("ix_exercise_sets_workout_exercise_id");

        builder.HasIndex(s => s.CompletedAt)
            .HasDatabaseName("ix_exercise_sets_completed_at");

        // Связи
        builder.HasOne(s => s.WorkoutExercise)
            .WithMany(we => we.Sets)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
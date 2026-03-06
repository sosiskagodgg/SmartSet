// FitnessTracker.Infrastructure/Data/Configurations/WorkoutExerciseConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.ToTable("workout_exercises");

        builder.HasKey(we => we.Id);

        builder.Property(we => we.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(we => we.WorkoutId)
            .HasColumnName("workout_id")
            .IsRequired();

        builder.Property(we => we.ExerciseId)
            .HasColumnName("exercise_id")
            .IsRequired();

        builder.Property(we => we.Order)
            .HasColumnName("order");

        builder.Property(we => we.Notes)
            .HasColumnName("notes");

        // Индексы
        builder.HasIndex(we => we.WorkoutId)
            .HasDatabaseName("ix_workout_exercises_workout_id");

        builder.HasIndex(we => we.ExerciseId)
            .HasDatabaseName("ix_workout_exercises_exercise_id");

        // Связи
        builder.HasOne(we => we.Workout)
            .WithMany(w => w.Exercises)
            .HasForeignKey(we => we.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(we => we.Exercise)
            .WithMany()
            .HasForeignKey(we => we.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(we => we.Sets)
            .WithOne(s => s.WorkoutExercise)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class ProgramDayExerciseConfiguration : IEntityTypeConfiguration<ProgramDayExercise>
{
    public void Configure(EntityTypeBuilder<ProgramDayExercise> builder)
    {
        builder.ToTable("program_day_exercises");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ProgramDayId)
            .HasColumnName("program_day_id")
            .IsRequired();

        builder.Property(x => x.ExerciseId)
            .HasColumnName("exercise_id")
            .IsRequired();

        builder.Property(x => x.Order)
            .HasColumnName("order_number")  // order_number â ÁÄ
            .IsRequired();

        builder.Property(x => x.TargetSets)
            .HasColumnName("sets");  // sets â ÁÄ

        builder.Property(x => x.TargetRepsMin)
            .HasColumnName("reps_min");

        builder.Property(x => x.TargetRepsMax)
            .HasColumnName("reps_max");

        builder.Property(x => x.TargetWeight)
            .HasColumnName("weight")
            .HasPrecision(6, 2);

        builder.Property(x => x.TargetDurationSeconds)
            .HasColumnName("duration");  // duration â ÁÄ

        builder.Property(x => x.TargetDistanceMeters)
            .HasColumnName("distance_m")
            .HasPrecision(10, 2);

        builder.Property(x => x.Notes)
            .HasColumnName("notes");

        // Èíäåêñû
        builder.HasIndex(x => x.ProgramDayId)
            .HasDatabaseName("ix_program_day_exercises_day");

        builder.HasIndex(x => x.ExerciseId)
            .HasDatabaseName("ix_program_day_exercises_exercise");

        // Ñâÿçè
        builder.HasOne(x => x.ProgramDay)
            .WithMany(pd => pd.Exercises)
            .HasForeignKey(x => x.ProgramDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
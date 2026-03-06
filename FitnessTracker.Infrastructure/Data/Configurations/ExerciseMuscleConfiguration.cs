using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class ExerciseMuscleConfiguration : IEntityTypeConfiguration<ExerciseMuscle>
{
    public void Configure(EntityTypeBuilder<ExerciseMuscle> builder)
    {
        builder.ToTable("exercise_muscles");

        builder.HasKey(em => new { em.ExerciseId, em.MuscleId });

        builder.Property(em => em.ExerciseId)
            .HasColumnName("exercise_id");

        builder.Property(em => em.MuscleId)
            .HasColumnName("muscle_id");

        builder.HasOne(em => em.Exercise)
            .WithMany(e => e.ExerciseMuscles)
            .HasForeignKey(em => em.ExerciseId);

        builder.HasOne(em => em.Muscle)
            .WithMany()
            .HasForeignKey(em => em.MuscleId);
    }
}
// FitnessTracker.Infrastructure/Data/Configurations/WorkoutConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("workouts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(w => w.StartedAt)
            .HasColumnName("started_at")
            .HasDefaultValueSql("now()");

        builder.Property(w => w.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(w => w.Notes)
            .HasColumnName("notes");

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(w => w.ProgramDayId)
            .HasColumnName("program_day_id");

        // Индексы
        builder.HasIndex(w => w.UserId)
            .HasDatabaseName("ix_workouts_user_id");

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("ix_workouts_status");

        builder.HasIndex(w => w.StartedAt)
            .HasDatabaseName("ix_workouts_started_at");

        // Связи
        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.ProgramDay)
            .WithMany()
            .HasForeignKey(w => w.ProgramDayId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Exercises)
            .WithOne(e => e.Workout)
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
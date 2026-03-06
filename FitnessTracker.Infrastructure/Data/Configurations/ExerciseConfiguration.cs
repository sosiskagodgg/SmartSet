// FitnessTracker.Infrastructure/Data/Configurations/ExerciseConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.MET)
            .HasColumnName("met")
            .HasPrecision(10, 2);

        builder.Property(e => e.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(e => e.IsCustom)
            .HasColumnName("is_custom")
            .HasDefaultValue(false);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        // Индексы
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("ix_exercises_name");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_exercises_user_id");

        builder.HasIndex(e => e.Category)
            .HasDatabaseName("ix_exercises_category");

        // Связи
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.ExerciseMuscles)
            .WithOne(em => em.Exercise)
            .HasForeignKey(em => em.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
// FitnessTracker.Infrastructure/Data/Configurations/UserParameterConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class UserParameterConfiguration : IEntityTypeConfiguration<UserParameter>
{
    public void Configure(EntityTypeBuilder<UserParameter> builder)
    {
        builder.ToTable("user_parameters");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.WeightKg)
            .HasColumnName("weight_kg")
            .HasPrecision(5, 2);

        builder.Property(p => p.HeightCm)
            .HasColumnName("height_cm")
            .HasPrecision(5, 2);

        builder.Property(p => p.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(p => p.Gender)
            .HasColumnName("gender")
            .HasMaxLength(10);

        builder.Property(p => p.ActivityLevel)
            .HasColumnName("activity_level")
            .HasMaxLength(20);

        builder.Property(p => p.ExperienceLevel)
            .HasColumnName("experience_level")
            .HasMaxLength(20);

        builder.Property(p => p.FitnessGoals)
            .HasColumnName("fitness_goals")
            .HasColumnType("text[]");

        builder.Property(p => p.Notes)
            .HasColumnName("notes");

        builder.Property(p => p.RecordedAt)
            .HasColumnName("recorded_at")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.IsCurrent)
            .HasColumnName("is_current")
            .HasDefaultValue(true);

        // Индексы
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_user_parameters_user_id");

        builder.HasIndex(p => p.RecordedAt)
            .HasDatabaseName("ix_user_parameters_recorded_at");

        builder.HasIndex(p => p.IsCurrent)
            .HasDatabaseName("ix_user_parameters_is_current");

        // Связь с пользователем
        builder.HasOne(p => p.User)
            .WithMany() // У User пока нет коллекции параметров
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
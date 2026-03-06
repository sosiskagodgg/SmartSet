// FitnessTracker.Infrastructure/Data/Configurations/MuscleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class MuscleConfiguration : IEntityTypeConfiguration<Muscle>
{
    public void Configure(EntityTypeBuilder<Muscle> builder)
    {
        builder.ToTable("muscles");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Stamina)
            .HasColumnName("stamina")
            .HasDefaultValue(100);

        builder.Property(m => m.Strength)
            .HasColumnName("strength")
            .HasDefaultValue(100);

        builder.Property(m => m.PercentageOfRecovery)
            .HasColumnName("percentage_of_recovery")
            .HasDefaultValue(100);

        builder.Property(m => m.RecoveryTime)
            .HasColumnName("recovery_time");

        builder.Property(m => m.UserId)
            .HasColumnName("user_id");

        // Индексы
        builder.HasIndex(m => m.UserId)
            .HasDatabaseName("ix_muscles_user_id");

        builder.HasIndex(m => m.Name)
            .HasDatabaseName("ix_muscles_name");

        // Связь с пользователем
        builder.HasOne(m => m.User)
            .WithMany(u => u.Muscles)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
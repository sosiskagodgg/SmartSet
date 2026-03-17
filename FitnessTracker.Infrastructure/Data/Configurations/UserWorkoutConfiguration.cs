// FitnessTracker.Infrastructure/Data/Configurations/UserWorkoutConfiguration.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class UserWorkoutConfiguration : IEntityTypeConfiguration<UserWorkout>
{
    public void Configure(EntityTypeBuilder<UserWorkout> builder)
    {
        builder.ToTable("userworkouts");

        builder.Ignore(uw => uw.Id);

        builder.HasKey(uw => new { uw.TelegramId, uw.DayNumber });

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

        // ДЛЯ List<Exercise>
        builder.Property(uw => uw.Exercises)
            .HasColumnName("exercises")
            .HasColumnType("jsonb")
            .HasConversion(new ExerciseListConverter());

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(uw => uw.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
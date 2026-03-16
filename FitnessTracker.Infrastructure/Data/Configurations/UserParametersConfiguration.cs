// FitnessTracker.Infrastructure/Data/Configurations/UserParametersConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class UserParametersConfiguration : IEntityTypeConfiguration<UserParameters>
{
    public void Configure(EntityTypeBuilder<UserParameters> builder)
    {
        // Имя таблицы в нижнем регистре
        builder.ToTable("userparameters");

        // Первичный ключ
        builder.HasKey(up => up.Id);

        // Настройка колонок - ВСЕ В НИЖНЕМ РЕГИСТРЕ
        builder.Property(up => up.Id)
            .HasColumnName("telegramid")  // нижний регистр
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(up => up.Height)
            .HasColumnName("height")      // нижний регистр
            .HasColumnType("integer");

        builder.Property(up => up.Weight)
            .HasColumnName("weight")      // нижний регистр
            .HasPrecision(5, 2);

        builder.Property(up => up.BodyFat)
            .HasColumnName("bodyfat")     // нижний регистр
            .HasPrecision(4, 1);

        builder.Property(up => up.Experience)
            .HasColumnName("experience")  // нижний регистр
            .HasMaxLength(50);

        builder.Property(up => up.Goals)
            .HasColumnName("goals");      // нижний регистр

        // Связь с users
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserParameters>(up => up.Id)
            .HasConstraintName("fk_userparameters_telegramid")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
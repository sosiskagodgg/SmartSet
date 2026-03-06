using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class ProgramDayConfiguration : IEntityTypeConfiguration<ProgramDay>
{
    public void Configure(EntityTypeBuilder<ProgramDay> builder)
    {
        builder.ToTable("program_days");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ProgramId)
            .HasColumnName("program_id")
            .IsRequired();

        builder.Property(x => x.DayNumber)
            .HasColumnName("day_number")
            .IsRequired();

        builder.Property(x => x.Name)  // ← Name, а не DayName
            .HasColumnName("day_name")
            .HasMaxLength(100);

        builder.Property(x => x.IsRestDay)
            .HasColumnName("is_rest_day")
            .HasDefaultValue(false);

        // Индексы
        builder.HasIndex(x => x.ProgramId)
            .HasDatabaseName("ix_program_days_program_id");

        builder.HasIndex(x => new { x.ProgramId, x.DayNumber })
            .IsUnique()
            .HasDatabaseName("ix_program_days_program_day");

        // Связи
        builder.HasOne(x => x.Program)
            .WithMany(p => p.Days)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Exercises)
            .WithOne(e => e.ProgramDay)
            .HasForeignKey(e => e.ProgramDayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
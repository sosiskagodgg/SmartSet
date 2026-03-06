// FitnessTracker.Infrastructure/Data/Configurations/ProgramTemplateConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data.Configurations;

public class ProgramTemplateConfiguration : IEntityTypeConfiguration<ProgramTemplate>
{
    public void Configure(EntityTypeBuilder<ProgramTemplate> builder)
    {
        builder.ToTable("program_templates");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasColumnName("description");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.StartDate)
            .HasColumnName("start_date");

        builder.Property(p => p.EndDate)
            .HasColumnName("end_date");

        // Индексы
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_program_templates_user_id");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("ix_program_templates_is_active");

        // Связи
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Days)
            .WithOne(d => d.Program)
            .HasForeignKey(d => d.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
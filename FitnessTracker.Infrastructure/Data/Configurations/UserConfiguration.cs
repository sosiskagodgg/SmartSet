using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessTracker.Domain.Entities;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(u => u.Id);

        b.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Не автоинкремент, а вручную

        b.Property(u => u.Username)
            .HasColumnName("username");

        b.Property(u => u.FirstName)
            .HasColumnName("first_name");

        b.Property(u => u.LastName)
            .HasColumnName("last_name");

        b.Property(u => u.RegisteredAt)
            .HasColumnName("registered_at")
            .HasDefaultValueSql("now()");

        b.Property(u => u.LastActivityAt)
            .HasColumnName("last_activity_at");
    }
}
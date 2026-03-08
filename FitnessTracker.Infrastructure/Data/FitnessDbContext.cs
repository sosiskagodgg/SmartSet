using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data;

public class FitnessDbContext : DbContext
{
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
        : base(options)
    {
    }

    // Добавляем DbSet для всех сущностей
    public DbSet<User> Users { get; set; }
    public DbSet<UserParameters> UserParameters { get; set; }
    public DbSet<UserWorkout> UserWorkouts { get; set; }
    public DbSet<Workout> Workouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);
    }
}
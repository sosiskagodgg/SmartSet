using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Infrastructure.Data;

public class FitnessDbContext : DbContext
{
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
        : base(options)
    {
    }

    // Users
    public DbSet<User> Users { get; set; }
    public DbSet<UserParameter> UserParameters { get; set; }
    public DbSet<Muscle> Muscles { get; set; }

    // Exercises
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseMuscle> ExerciseMuscles { get; set; }


    // Workouts - НОВЫЕ
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
    public DbSet<ExerciseSet> ExerciseSets { get; set; }

    // Programs - НОВЫЕ
    public DbSet<ProgramTemplate> ProgramTemplates { get; set; }
    public DbSet<ProgramDay> ProgramDays { get; set; }
    public DbSet<ProgramDayExercise> ProgramDayExercises { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);
    }
}
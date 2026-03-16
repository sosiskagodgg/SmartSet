// FitnessTracker.Infrastructure/Data/FitnessDbContext.cs
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Infrastructure.Data.Configurations;
using FitnessTracker.Infrastructure.Data.Converters;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitnessTracker.Infrastructure.Data;

public class FitnessDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserParameters> UserParameters { get; set; }
    public DbSet<UserWorkout> UserWorkouts { get; set; }
    public DbSet<Workout> Workouts { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new ExerciseJsonConverter() // Добавляем наш конвертер
        }
    };

    public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
        : base(options)
    {
    }

    static FitnessDbContext()
    {
        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем все конфигурации
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);
    }
}
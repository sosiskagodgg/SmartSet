// FitnessTracker.Infrastructure/Data/FitnessDbContext.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Infrastructure.Data.Configurations;
using FitnessTracker.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            new ExerciseJsonConverter()
        }
    };

    public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
        : base(options)
    {
        // Добавим логирование для отладки
        Console.WriteLine($"[DbContext] Constructor called at {DateTime.UtcNow:HH:mm:ss.fff}");
    }

    static FitnessDbContext()
    {
        Console.WriteLine("[DbContext] Static constructor called");
        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

        // Добавим глобальный маппинг для DateTime
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ExerciseType>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Добавим логирование SQL запросов
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        Console.WriteLine("[DbContext] OnModelCreating started");
        base.OnModelCreating(modelBuilder);

        // Применяем все конфигурации
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);
        Console.WriteLine("[DbContext] OnModelCreating completed");
    }
}
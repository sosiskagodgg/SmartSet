using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using FitnessTracker.Infrastructure.Repositories;

namespace FitnessTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Добавляем DbContext (используем PostgreSQL)
        // Вместо AddDbContext используй AddDbContextFactory
        services.AddDbContextFactory<FitnessDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Регистрируем репозитории
        services.AddScoped<IMuscleRepository, MuscleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUserParameterRepository, UserParameterRepository>(); // Добавить

        services.AddScoped<IExerciseLibraryRepository, ExerciseLibraryRepository>();

        // Workout Repositories
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
        services.AddScoped<IExerciseSetRepository, ExerciseSetRepository>();

        // Program Repositories
        services.AddScoped<IProgramTemplateRepository, ProgramTemplateRepository>();
        services.AddScoped<IProgramDayRepository, ProgramDayRepository>();

        // Statistics
        services.AddScoped<IWorkoutStatisticsRepository, WorkoutStatisticsRepository>();

        return services;
    }
}
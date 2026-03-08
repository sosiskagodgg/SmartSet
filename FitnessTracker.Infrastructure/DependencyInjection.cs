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
        // Добавляем DbContext
        services.AddDbContextFactory<FitnessDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Ручная регистрация ВСЕХ репозиториев
        RegisterRepositories(services);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        // Явно и понятно: каждый репозиторий регистрируем руками
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserWorkoutRepository, UserWorkoutRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IUserParametersRepository, UserParametersRepository>();
        // Добавляй сюда новые репозитории по мере создания
        // services.AddScoped<IExerciseRepository, ExerciseRepository>();
        // services.AddScoped<IMuscleRepository, MuscleRepository>();
        // и так далее...
    }
}
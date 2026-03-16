// FitnessTracker.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.Services;

namespace FitnessTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Регистрируем сервисы через интерфейсы
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserParametersService, UserParametersService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IUserWorkoutService, UserWorkoutService>();

        return services;
    }
}
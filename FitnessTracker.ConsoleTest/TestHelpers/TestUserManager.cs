// FitnessTracker.ConsoleTest/TestHelpers/TestUserManager.cs
using FitnessTracker.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessTracker.ConsoleTest.TestHelpers;

public class TestUserManager
{
    private readonly IServiceProvider _services;

    public TestUserManager(IServiceProvider services)
    {
        _services = services;
    }

    public async Task EnsureTestUserExistsAsync(long telegramId)
    {
        using var scope = _services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var user = await userService.GetUserByIdAsync(telegramId);
        if (user == null)
        {
            ConsoleHelper.WriteInfo($"Создаем тестового пользователя с ID {telegramId}...");
            user = await userService.CreateUserAsync(
                telegramId,
                name: "Тестовый Пользователь",
                username: "test_user"
            );
            ConsoleHelper.WriteSuccess($"Пользователь создан: {user.Name}");
        }
        else
        {
            ConsoleHelper.WriteSuccess($"Тестовый пользователь найден: {user.Name}");
        }
    }
}
// FitnessTracker.Application/Services/UserRegistrationService.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class UserRegistrationService
{
    private readonly IUserService _userService;

    public UserRegistrationService(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<User> RegisterIfNotExistsAsync(
        long telegramId,
        string name,
        string? username = null,
        CancellationToken ct = default)
    {
        // Проверяем есть ли пользователь
        var existingUser = await _userService.GetUserByTelegramIdAsync(telegramId, ct);

        if (existingUser != null)
            return existingUser;

        // Если нет - создаем
        return await _userService.CreateUserAsync(telegramId, name, username, ct);
    }
}
// FitnessTracker.Application/Services/UserService.cs
using FitnessTracker.Application.Common.Exceptions;
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services;

/// <summary>
/// Сервис для работы с пользователями
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;

        _logger.LogInformation("📝 UserService конструктор вызван в {Time}", DateTime.Now.ToString("HH:mm:ss.fff"));
        _logger.LogInformation("  userRepository is null: {IsNull}", userRepository == null);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByUsernameAsync(username, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetAllUsersAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAllAsync(limit, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User> CreateUserAsync(long telegramId, string name, string? username = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user with TelegramId: {TelegramId}", telegramId);

        var existing = await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} already exists", telegramId);
            throw new UserAlreadyExistsException(telegramId);
        }

        var user = User.Create(telegramId, name, username);
        await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User created successfully with TelegramId: {TelegramId}", telegramId);
        return user;
    }

    /// <inheritdoc />
    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user with TelegramId: {TelegramId}", telegramId);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            throw new UserNotFoundException(telegramId);
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        _logger.LogInformation("User deleted successfully with TelegramId: {TelegramId}", telegramId);
    }

    /// <inheritdoc />
    public async Task<bool> UserExistsAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.ExistsByTelegramIdAsync(telegramId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ActivateSubscriptionAsync(long telegramId, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating subscription for user {TelegramId} until {EndDate}", telegramId, endDate);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            throw new UserNotFoundException(telegramId);
        }

        user.ActivateSubscription(endDate);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Subscription activated for user {TelegramId}", telegramId);
    }

    /// <inheritdoc />
    public async Task DeactivateSubscriptionAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating subscription for user {TelegramId}", telegramId);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            throw new UserNotFoundException(telegramId);
        }

        user.DeactivateSubscription();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Subscription deactivated for user {TelegramId}", telegramId);
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveSubscriptionAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        return user?.IsSubscriptionActive ?? false;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetActiveSubscribersAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User> RegisterIfNotExistsAsync(long telegramId, string name, string? username = null, CancellationToken cancellationToken = default)
    {
        var existing = await GetUserByIdAsync(telegramId, cancellationToken);
        if (existing != null)
        {
            if (existing.Name != name || existing.Username != username)
            {
                existing.UpdateProfile(name, username);
                await UpdateUserAsync(existing, cancellationToken);
            }
            return existing;
        }

        return await CreateUserAsync(telegramId, name, username, cancellationToken);
    }
}
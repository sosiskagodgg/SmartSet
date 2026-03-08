using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByIdAsync(long id, CancellationToken ct = default)
    {
        return await _userRepository.GetByIdAsync(id, ct);
    }

    public async Task<User?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(int.MaxValue, ct);
        return users.FirstOrDefault(u => u.TelegramId == telegramId);
    }

    public async Task<List<User>> GetAllUsersAsync(int limit = 50, CancellationToken ct = default)
    {
        return await _userRepository.GetAllAsync(limit, ct);
    }

    public async Task<User> CreateUserAsync(long telegramId, string name, string? username = null, CancellationToken ct = default)
    {
        var user = new User
        {
            TelegramId = telegramId,
            Name = name,
            Username = username,
            SubscriptionStatus = "inactive"
        };

        await _userRepository.AddAsync(user, ct);
        return user;
    }

    public async Task UpdateUserAsync(User user, CancellationToken ct = default)
    {
        await _userRepository.UpdateAsync(user, ct);
    }

    public async Task DeleteUserAsync(User user, CancellationToken ct = default)
    {
        await _userRepository.DeleteAsync(user, ct);
    }

    public async Task<bool> UserExistsAsync(long telegramId, CancellationToken ct = default)
    {
        var user = await GetUserByTelegramIdAsync(telegramId, ct);
        return user != null;
    }

    public async Task UpdateSubscriptionAsync(User user, DateTime? endDate, string status, CancellationToken ct = default)
    {
        user.SubscriptionEndDate = endDate;
        user.SubscriptionStatus = status;
        await _userRepository.UpdateAsync(user, ct);
    }

    public async Task<bool> HasActiveSubscriptionAsync(User user, CancellationToken ct = default)
    {
        return user.SubscriptionStatus == "active" &&
               (!user.SubscriptionEndDate.HasValue || user.SubscriptionEndDate > DateTime.UtcNow);
    }
}
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IUserService : IService  // ← Наследуем маркер
{
    Task<User?> GetUserByIdAsync(long id, CancellationToken ct = default);
    Task<User?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<List<User>> GetAllUsersAsync(int limit = 50, CancellationToken ct = default);
    Task<User> CreateUserAsync(long telegramId, string name, string? username = null, CancellationToken ct = default);
    Task UpdateUserAsync(User user, CancellationToken ct = default);
    Task DeleteUserAsync(User user, CancellationToken ct = default);
    Task<bool> UserExistsAsync(long telegramId, CancellationToken ct = default);
    Task UpdateSubscriptionAsync(User user, DateTime? endDate, string status, CancellationToken ct = default);
    Task<bool> HasActiveSubscriptionAsync(User user, CancellationToken ct = default);
}
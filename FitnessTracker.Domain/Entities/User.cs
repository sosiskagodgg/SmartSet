// FitnessTracker.Domain/Entities/User.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Exceptions;

namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Пользователь - корень агрегата.
/// Содержит все правила и инварианты, связанные с пользователем.
/// </summary>
public class User : AggregateRoot<long> // TelegramId = long
{
    // Приватные сеттеры для EF Core
    public string Name { get; private set; }
    public string? Username { get; private set; }
    public DateTime? SubscriptionEndDate { get; private set; }
    public string SubscriptionStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Для EF Core
    private User() : base()
    {
        Name = string.Empty;
        SubscriptionStatus = "inactive";
    }

    private User(long telegramId, string name, string? username) : base(telegramId)
    {
        Name = name;
        Username = username;
        SubscriptionStatus = "inactive";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Фабричный метод для создания пользователя.
    /// Используется вместо конструктора для явного указания намерения.
    /// </summary>
    public static User Create(long telegramId, string name, string? username = null)
    {
        // Валидация входных данных
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId must be positive", nameof(telegramId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new User(telegramId, name.Trim(), username?.Trim());
    }

    /// <summary>
    /// Обновление профиля пользователя
    /// </summary>
    public void UpdateProfile(string name, string? username)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name.Trim();
        Username = username?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Установка статуса подписки
    /// </summary>
    public void SetSubscription(DateTime? endDate, string status)
    {
        // Валидация статуса
        var allowedStatuses = new[] { "active", "inactive", "expired", "cancelled" };
        if (!allowedStatuses.Contains(status.ToLowerInvariant()))
            throw new InvalidSubscriptionStatusException(status);

        // Если статус active, endDate обязателен
        if (status.Equals("active", StringComparison.OrdinalIgnoreCase) && !endDate.HasValue)
            throw new InvalidSubscriptionDateException("End date is required for active subscription");

        // Если endDate указан, он должен быть в будущем для активной подписки
        if (endDate.HasValue && status.Equals("active", StringComparison.OrdinalIgnoreCase) && endDate <= DateTime.UtcNow)
            throw new InvalidSubscriptionDateException("Subscription end date must be in the future");

        SubscriptionEndDate = endDate;
        SubscriptionStatus = status.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активация подписки
    /// </summary>
    public void ActivateSubscription(DateTime endDate)
    {
        SetSubscription(endDate, "active");
    }

    /// <summary>
    /// Деактивация подписки
    /// </summary>
    public void DeactivateSubscription()
    {
        SetSubscription(null, "inactive");
    }

    /// <summary>
    /// Проверка активности подписки
    /// </summary>
    public bool IsSubscriptionActive =>
        SubscriptionStatus == "active" &&
        SubscriptionEndDate > DateTime.UtcNow;
}
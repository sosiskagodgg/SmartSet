
namespace FitnessTracker.Domain.Entities;

public class User
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Telegram ID пользователя
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// Username пользователя в Telegram
    /// </summary>
    public string? Username { get; set; }


    /// <summary>
    /// Дата окончания подписки
    /// </summary>
    public DateTime? SubscriptionEndDate { get; set; }

    /// <summary>
    /// Статус подписки (активна/неактивна)
    /// </summary>
    public string SubscriptionStatus { get; set; }



}
// FitnessTracker.TelegramBot/Abstractions/Models/UserState.cs
namespace FitnessTracker.TelegramBot.Abstractions;

/// <summary>
/// Состояние пользователя в диалоге с ботом
/// </summary>
public class UserState
{
    /// <summary>
    /// Имя команды, которая ждет ввод
    /// </summary>
    public required string CommandName { get; set; }

    /// <summary>
    /// Конкретный шаг внутри команды (для многошаговых процессов)
    /// </summary>
    public string Step { get; set; } = string.Empty;

    /// <summary>
    /// Данные, собранные за время диалога
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Когда состояние было создано
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Проверка, не устарело ли состояние (больше 30 минут)
    /// </summary>
    public bool IsExpired => (DateTime.UtcNow - CreatedAt) > TimeSpan.FromMinutes(30);
}
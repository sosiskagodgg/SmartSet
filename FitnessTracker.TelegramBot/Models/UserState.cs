namespace FitnessTracker.TelegramBot.Models;

public class UserState
{
    /// <summary>
    /// Текущее состояние (null = не ждет ввода)
    /// </summary>
    public string? CurrentState { get; set; }

    /// <summary>
    /// Данные состояния
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Очистить состояние
    /// </summary>
    public void Clear()
    {
        CurrentState = null;
        Data.Clear();
    }
}
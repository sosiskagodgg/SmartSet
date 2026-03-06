namespace FitnessTracker.TelegramBot.Handlers.Base;

public interface IStateHandler
{
    /// <summary>
    /// Тип состояния (например, "waiting_weight")
    /// </summary>
    string StateType { get; }

    /// <summary>
    /// Обработать ввод в состоянии
    /// </summary>
    Task HandleAsync(long userId, string messageText, int messageId, Dictionary<string, object> stateData, CancellationToken ct);
}
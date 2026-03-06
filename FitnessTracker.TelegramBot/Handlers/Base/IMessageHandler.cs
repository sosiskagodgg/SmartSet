namespace FitnessTracker.TelegramBot.Handlers.Base;

public interface IMessageHandler
{
    /// <summary>
    /// Может ли этот хендлер обработать сообщение
    /// </summary>
    bool CanHandle(string messageText);

    /// <summary>
    /// Обработать сообщение
    /// </summary>
    Task HandleAsync(long userId, string messageText, int messageId, CancellationToken ct);
}
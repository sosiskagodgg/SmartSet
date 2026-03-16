// FitnessTracker.TelegramBot/Abstractions/ICommand.cs
namespace FitnessTracker.TelegramBot.Abstractions;

/// <summary>
/// Команда бота, которая может обрабатывать сообщения и колбэки
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Уникальное имя команды (используется в колбэках)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Может ли команда обработать данное сообщение (без состояния)
    /// </summary>
    bool CanHandle(string messageText);

    /// <summary>
    /// Обработать обычное сообщение (когда нет состояния)
    /// </summary>
    Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обработать колбэк (нажатие на inline кнопку)
    /// </summary>
    Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обработать ввод в состоянии (когда пользователь отвечает на вопрос)
    /// </summary>
    Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Установить менеджер состояний (вызывается после создания команды)
    /// </summary>
    void SetStateManager(IUserStateManager stateManager) { } // Пустая реализация по умолчанию
}
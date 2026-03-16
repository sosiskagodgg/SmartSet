// FitnessTracker.TelegramBot/Commands/MainCommand.cs
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Главное меню бота
/// </summary>
public class MainCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly ILogger<MainCommand> _logger;
    private IUserStateManager? _stateManager;

    public string Name => "main";

    private static class Const
    {
        public const string Title = "🏋️ Главное меню\n\nВыберите раздел:";

        public static class Action
        {
            public const string Open = "open";
            public const string Back = "back";
        }

        public static class ButtonText
        {
            public const string Profile = "👤 Профиль";
            public const string Workouts = "💪 Тренировки";
            public const string Questions = "❓ Вопросы";

            public const string ProfileCallback = "profile:main";
            public const string WorkoutsCallback = "workouts:main";
            public const string QuestionsCallback = "questions:main";
        }

        public static Keyboard Menu()
        {
            return Keyboard.FromRows(
                new List<Button>
                {
                    Models.Button.Create(ButtonText.Profile, ButtonText.ProfileCallback),
                    Models.Button.Create(ButtonText.Workouts, ButtonText.WorkoutsCallback),
                    Models.Button.Create(ButtonText.Questions, ButtonText.QuestionsCallback)
                }
            );
        }
    }

    public MainCommand(
        ITelegramBotAdapter adapter,
        ILogger<MainCommand> logger)
    {
        Console.WriteLine($"\n🏠 MainCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  logger is null: {logger == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Console.WriteLine($"✅ MainCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"🏠 MainCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText)
    {
        var lower = messageText.ToLowerInvariant();
        return lower.Contains("меню") || lower.Contains("главное") || lower.Contains("main");
    }

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        await ShowMainMenu(context.UserId, context.MessageId, cancellationToken);
    }

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        switch (context.Action)
        {
            case Const.Action.Open:
            case Const.Action.Back:
                await ShowMainMenu(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            default:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;
        }
    }

    public Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private async Task ShowMainMenu(long userId, int messageId, CancellationToken cancellationToken)
    {
        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Title,
            keyboard: Const.Menu(),
            cancellationToken: cancellationToken);
    }
}
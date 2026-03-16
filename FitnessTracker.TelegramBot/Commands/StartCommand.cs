// FitnessTracker.TelegramBot/Commands/StartCommand.cs
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда /start - приветствие и вход в меню
/// </summary>
public class StartCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly IUserService _userService;
    private readonly ILogger<StartCommand> _logger;
    private IUserStateManager? _stateManager;

    public string Name => "start";

    private static class Const
    {
        public const string Welcome = """
            🏋️ <b>Добро пожаловать в Fitness Tracker Bot!</b>
            
            Этот бот поможет тебе:
            • 📝 Создавать персональные тренировки
            • 📊 Отслеживать свой прогресс
            • 💪 Выполнять тренировки и записывать результаты
            • 📈 Следить за своими параметрами
            
            Нажми кнопку ниже, чтобы войти в меню!
            """;

        public const string ButtonEnterMenu = "🚀 Войти в меню";
        public const string EnterMenuCallback = "main:open";
    }

    public StartCommand(
        ITelegramBotAdapter adapter,
        IUserService userService,
        ILogger<StartCommand> logger)
    {
        Console.WriteLine($"\n📋 StartCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  userService is null: {userService == null}");
        Console.WriteLine($"  logger is null: {logger == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Console.WriteLine($"✅ StartCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"📋 StartCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText)
    {
        return messageText == "/start";
    }

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} started the bot", context.UserId);

        // Проверяем/создаем пользователя
        var user = await _userService.GetUserByIdAsync(context.UserId, cancellationToken);
        if (user == null)
        {
            await _userService.CreateUserAsync(
                telegramId: context.UserId,
                name: "Пользователь",
                username: null,
                cancellationToken: cancellationToken);
        }

        // Очищаем возможное предыдущее состояние пользователя
        _stateManager?.ClearState(context.UserId);

        var keyboard = Keyboard.FromSingleButton(Const.ButtonEnterMenu, Const.EnterMenuCallback);

        await _adapter.SendMessageAsync(
            userId: context.UserId,
            text: Const.Welcome,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        // StartCommand не обрабатывает колбэки
        return Task.CompletedTask;
    }

    public Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        // StartCommand не обрабатывает состояния
        return Task.CompletedTask;
    }
}
// Handlers/Messages/StartHandler.cs
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.TelegramBot.Handlers.Messages;

public class StartHandler : HandlerBase, IMessageHandler
{
    private static class Callback
    {
        public const string EnterMenu = "enter_menu";
        public const string EnterMenuFull = "ft:start:enter_menu";
    }

    private static class Text
    {
        public const string Welcome = """
            🏋️ <b>Добро пожаловать в Fitness Tracker Bot!</b>
            
            Этот бот поможет тебе:
            • 📝 Создавать персональные тренировки
            • 📊 Отслеживать свой прогресс
            • 💪 Выполнять тренировки и записывать результаты
            • 📈 Следить за своими параметрами
            
            <b>Что можно делать:</b>
            • Составлять программы тренировок по дням
            • Записывать выполненные упражнения
            • Отслеживать вес, процент жира и другие параметры
            • Получать статистику прогресса
            
            Нажми кнопку ниже, чтобы войти в меню и начать!
            """;

        public const string ButtonEnterMenu = "🚀 Войти в меню";
    }

    private static class Keyboard
    {
        public static Models.Keyboard WelcomeMenu()
        {
            return Models.Keyboard.FromSingleButton(
                Text.ButtonEnterMenu,
                Callback.EnterMenuFull
            );
        }
    }

    public StartHandler(
        ITelegramBotAdapter telegram,
        ILogger<StartHandler> logger,
        UserStateService stateService,
        IUserService userService)  // ← передаем в base
        : base(telegram, logger, stateService, userService)
    {
    }

    public bool CanHandle(string messageText)
    {
        return messageText == "/start";
    }

    public async Task HandleAsync(
        long userId,
        string messageText,
        int messageId,
        CancellationToken ct)
    {
        _logger.LogInformation("User {UserId} started the bot", userId);

        // Проверяем/создаем пользователя
        // Но username пока не знаем - получим через колбэк или позже
        await EnsureUserExistsAsync(userId, ct: ct);

        await SendMessage(userId, Text.Welcome, Keyboard.WelcomeMenu(), ct);
    }
}
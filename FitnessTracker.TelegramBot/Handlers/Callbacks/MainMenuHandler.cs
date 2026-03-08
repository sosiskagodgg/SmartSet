// Handlers/Callbacks/MainMenuHandler.cs
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;
using FitnessTracker.Application.Interfaces;  // ← добавить!

namespace FitnessTracker.TelegramBot.Handlers.Callbacks;

public class MainMenuHandler : HandlerBase, ICallbackHandler
{
    public string CallbackPrefix => "main";

    private static class Callback
    {
        public const string Profile = "profile";
        public const string ProfileFull = "ft:profile:main";  // ← для перехода в профиль

        public const string Workouts = "workouts";
        public const string WorkoutsFull = "ft:workouts:main";

        public const string Questions = "questions";
        public const string QuestionsFull = "ft:questions:main";

        public const string OpenMenu = "open";
        public const string OpenMenuFull = "ft:main:open";

        public const string FromStart = "enter_menu";
        public const string FromStartFull = "ft:start:enter_menu";

        // ДОБАВЛЯЕМ это для кнопки "Назад"
        public const string BackToMain = "back";
        public const string BackToMainFull = "ft:main:back";
    }

    private static class Text
    {
        public const string Title = "🏋️ Главное меню\n\nВыберите раздел:";
        public const string ButtonProfile = "👤 Профиль";
        public const string ButtonWorkouts = "💪 Тренировки";
        public const string ButtonQuestions = "❓ Вопросы";
    }

    private static class Keyboard
    {
        public static Models.Keyboard MainMenu()
        {
            return Models.Keyboard.FromRows(
                new List<Button>
                {
                    Button.Create(Text.ButtonProfile, Callback.ProfileFull),
                    Button.Create(Text.ButtonWorkouts, Callback.WorkoutsFull),
                    Button.Create(Text.ButtonQuestions, Callback.QuestionsFull)
                }
            );
        }
    }

    public MainMenuHandler(
        ITelegramBotAdapter telegram,
        ILogger<MainMenuHandler> logger,
        UserStateService stateService,
        IUserService userService)  // ← ДОБАВИЛ!
        : base(telegram, logger, stateService, userService)
    {
    }

    public async Task HandleAsync(
        long userId,
        CallbackInfo callback,
        int messageId,
        string callbackQueryId,
        CancellationToken ct)
    {
        switch (callback.Action)
        {
            case Callback.Profile:
                // Переходим в профиль
                await ShowMainMenu(userId, messageId, ct); // ← временно, пока нет ProfileHandler
                break;

            case Callback.Workouts:
            case Callback.Questions:
                // Пока просто показываем меню
                await ShowMainMenu(userId, messageId, ct);
                break;

            case Callback.OpenMenu:
            case Callback.FromStart:
                await ShowMainMenu(userId, messageId, ct);
                break;

            // ДОБАВЛЯЕМ обработку кнопки "Назад"
            case Callback.BackToMain:
                await ShowMainMenu(userId, messageId, ct);
                break;
        }

        await AnswerCallback(callbackQueryId, ct: ct);
    }

    private async Task ShowMainMenu(long userId, int messageId, CancellationToken ct)
    {
        await EditMessage(userId, messageId, Text.Title, Keyboard.MainMenu(), ct);
    }
}
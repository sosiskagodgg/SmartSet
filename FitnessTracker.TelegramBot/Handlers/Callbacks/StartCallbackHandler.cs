using FitnessTracker.Application.Interfaces;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;

public class StartCallbackHandler : HandlerBase, ICallbackHandler
{
    public string CallbackPrefix => "start";

    private static class Callback
    {
        public const string EnterMenu = "enter_menu";
        public const string EnterMenuFull = "ft:start:enter_menu";

        // Колбэки для главного меню
        public const string Profile = "profile";
        public const string ProfileFull = "ft:profile:main";

        public const string Workouts = "workouts";
        public const string WorkoutsFull = "ft:workouts:main";

        public const string Questions = "questions";
        public const string QuestionsFull = "ft:questions:main";
    }

    private static class Text
    {
        public const string MainMenu = "🏋️ Главное меню\n\nВыберите раздел:";
        public const string ButtonProfile = "👤 Профиль";
        public const string ButtonWorkouts = "💪 Тренировки";
        public const string ButtonQuestions = "❓ Вопросы";
    }

    private static class Keyboard
    {
        public static FitnessTracker.TelegramBot.Models.Keyboard MainMenu()  // ← полное имя
        {
            return FitnessTracker.TelegramBot.Models.Keyboard.FromRows(      // ← полное имя
                new List<Button>
                {
                    Button.Create(Text.ButtonProfile, Callback.ProfileFull),
                    Button.Create(Text.ButtonWorkouts, Callback.WorkoutsFull),
                    Button.Create(Text.ButtonQuestions, Callback.QuestionsFull)
                }
            );
        }
    }

    public StartCallbackHandler(
        ITelegramBotAdapter telegram,
        ILogger<StartCallbackHandler> logger,
        UserStateService stateService,
        IUserService userService)  // ← добавил!
        : base(telegram, logger, stateService, userService)  // ← 4 параметра
    {
    }

    public async Task HandleAsync(
        long userId,
        CallbackInfo callback,
        int messageId,
        string callbackQueryId,
        CancellationToken ct)
    {
        if (callback.Action == Callback.EnterMenu)
        {
            await DeleteMessage(userId, messageId, ct);
            await SendMessage(userId, Text.MainMenu, Keyboard.MainMenu(), ct);
        }

        await AnswerCallback(callbackQueryId, ct: ct);
    }
}
// FitnessTracker.TelegramBot/Commands/QuestionsCommand.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.AI.PublicServices;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для раздела вопросов
/// </summary>
public class QuestionsCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly QuestionsAIService _questionsAI;
    private readonly ILogger<QuestionsCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IUserStateManager? _stateManager;

    public string Name => "questions";

    private static class Const
    {
        public const string Title = "❓ <b>Раздел вопросов</b>\n\nВыберите тему или задайте свой вопрос:";

        public static class Action
        {
            public const string Main = "main";
            public const string AskFree = "ask_free";
            public const string AboutBot = "about_bot";
            public const string AboutNutrition = "about_nutrition";
            public const string AboutWorkouts = "about_workouts";
        }

        public static class State
        {
            public const string Asking = "asking";
        }

        public static class Text
        {
            public const string AskFreePrompt = """
                💬 <b>Задайте любой вопрос</b>
                
                Напишите в чат ваш вопрос о тренировках, питании или работе бота.
                
                <i>Примеры:</i>
                • "Как часто нужно тренироваться?"
                • "Сколько белка нужно в день?"
                • "Как накачать грудь?"
                • "Как изменить вес в профиле?"
                
                Или нажмите кнопку "Назад" для возврата в меню.
                """;

            public const string ButtonAboutBot = "🤖 О боте";
            public const string ButtonAboutNutrition = "🥗 Питание";
            public const string ButtonAboutWorkouts = "💪 Тренировки";
            public const string ButtonAskFree = "💬 Задать вопрос";
            public const string ButtonBack = "◀️ Назад";
            public const string ButtonBackToQuestions = "◀️ К вопросам";
            public const string ButtonAskAnother = "💬 Задать ещё вопрос";
        }

        public static Keyboard MainMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonAboutBot, $"{commandName}:{Action.AboutBot}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonAboutNutrition, $"{commandName}:{Action.AboutNutrition}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonAboutWorkouts, $"{commandName}:{Action.AboutWorkouts}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonAskFree, $"{commandName}:{Action.AskFree}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonBack, "main:back")
                }
            );
        }

        public static Keyboard QuestionMenu(string commandName)
        {
            return Keyboard.FromSingleButton(Text.ButtonBackToQuestions, $"{commandName}:{Action.Main}");
        }

        public static Keyboard ContinueMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonAskAnother, $"{commandName}:{Action.AskFree}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonBackToQuestions, $"{commandName}:{Action.Main}")
                }
            );
        }
    }

    public QuestionsCommand(
        ITelegramBotAdapter adapter,
        QuestionsAIService questionsAI,
        ILogger<QuestionsCommand> logger,
        IServiceProvider serviceProvider)
    {
        Console.WriteLine($"\n❓ QuestionsCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  questionsAI is null: {questionsAI == null}");
        Console.WriteLine($"  logger is null: {logger == null}");
        Console.WriteLine($"  serviceProvider is null: {serviceProvider == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _questionsAI = questionsAI ?? throw new ArgumentNullException(nameof(questionsAI));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        Console.WriteLine($"✅ QuestionsCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"❓ QuestionsCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText)
    {
        var lower = messageText.ToLowerInvariant();
        return lower.Contains("вопрос") || lower.Contains("помощь") || lower.Contains("help");
    }

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        await ShowMainMenu(context.UserId, context.MessageId, cancellationToken);
    }

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("QuestionsCommand received: {Action}", context.Action);

        switch (context.Action)
        {
            case Const.Action.Main:
                await ShowMainMenu(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.AskFree:
                await StartAsking(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.AboutBot:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                await ShowAboutBot(context.UserId, context.MessageId, cancellationToken);
                break;

            case Const.Action.AboutNutrition:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                await ShowAboutNutrition(context.UserId, context.MessageId, cancellationToken);
                break;

            case Const.Action.AboutWorkouts:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                await ShowAboutWorkouts(context.UserId, context.MessageId, cancellationToken);
                break;

            default:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing free question from user {UserId}: {Message}", context.UserId, context.Text);

        try
        {
            await _adapter.SendChatActionAsync(context.UserId, BotChatAction.Typing, cancellationToken);

            var processingMsg = await _adapter.SendMessageAsync(
                context.UserId,
                "🤔 Ищу ответ...",
                cancellationToken: cancellationToken);

            var answer = await _questionsAI.AnswerAsync(context.UserId, context.Text, cancellationToken);

            await _adapter.DeleteMessageAsync(context.UserId, processingMsg, cancellationToken);

            await _adapter.SendMessageAsync(
                context.UserId,
                answer,
                Const.ContinueMenu(Name),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question for user {UserId}", context.UserId);
            await _adapter.SendMessageAsync(
                context.UserId,
                "❌ Ошибка при обработке вопроса. Попробуйте еще раз.",
                Const.QuestionMenu(Name),
                cancellationToken: cancellationToken);
            _stateManager?.ClearState(context.UserId);
        }
    }

    private async Task ShowMainMenu(long userId, int messageId, CancellationToken cancellationToken)
    {
        _stateManager?.ClearState(userId);
        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Title,
            keyboard: Const.MainMenu(Name),
            cancellationToken: cancellationToken);
    }

    private async Task StartAsking(long userId, int messageId, CancellationToken cancellationToken)
    {
        if (_stateManager == null)
        {
            _logger.LogError("StateManager not initialized for QuestionsCommand");
            return;
        }

        _stateManager.SetState(userId, new UserState
        {
            CommandName = Name,
            Step = Const.State.Asking,
            Data = new Dictionary<string, object>()
        });

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.AskFreePrompt,
            keyboard: Const.QuestionMenu(Name),
            cancellationToken: cancellationToken);
    }

    private async Task ShowAboutBot(long userId, int messageId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuestionsAboutBotCommand>>();

        var aboutBotCommand = new QuestionsAboutBotCommand(
            _adapter,
            _questionsAI,
            logger,
            _serviceProvider);

        await aboutBotCommand.HandleCallbackAsync(
            new CallbackContext
            {
                UserId = userId,
                MessageId = messageId,
                CallbackQueryId = "",
                Action = "main",
                Parameters = Array.Empty<string>(),
                RawData = "questions_about_bot:main"
            },
            cancellationToken);
    }

    private async Task ShowAboutNutrition(long userId, int messageId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuestionsNutritionCommand>>();

        var nutritionCommand = new QuestionsNutritionCommand(
            _adapter,
            _questionsAI,
            logger,
            _serviceProvider);

        await nutritionCommand.HandleCallbackAsync(
            new CallbackContext
            {
                UserId = userId,
                MessageId = messageId,
                CallbackQueryId = "",
                Action = "main",
                Parameters = Array.Empty<string>(),
                RawData = "questions_nutrition:main"
            },
            cancellationToken);
    }

    private async Task ShowAboutWorkouts(long userId, int messageId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuestionsWorkoutsCommand>>();

        var workoutsCommand = new QuestionsWorkoutsCommand(
            _adapter,
            _questionsAI,
            logger,
            _serviceProvider);

        await workoutsCommand.HandleCallbackAsync(
            new CallbackContext
            {
                UserId = userId,
                MessageId = messageId,
                CallbackQueryId = "",
                Action = "main",
                Parameters = Array.Empty<string>(),
                RawData = "questions_workouts:main"
            },
            cancellationToken);
    }
}
// FitnessTracker.TelegramBot/Commands/QuestionsWorkoutsCommand.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.AI.Data;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для раздела "Тренировки" - FAQ и вопросы о тренировках
/// </summary>
public class QuestionsWorkoutsCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly QuestionsAIService _questionsAI;
    private readonly ILogger<QuestionsWorkoutsCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IUserStateManager? _stateManager;

    public string Name => "questions_workouts";

    private static class Const
    {
        public const string Title = "💪 <b>Тренировки</b>\n\nВыберите тему или задайте свой вопрос:";

        public static class Action
        {
            public const string Main = "main";
            public const string Ask = "ask";
            public const string Back = "back";

            public const string FaqFrequency = "faq_frequency";
            public const string FaqProgram = "faq_program";
            public const string FaqWarmup = "faq_warmup";
            public const string FaqCardio = "faq_cardio";
            public const string FaqChest = "faq_chest";
            public const string FaqBack = "faq_back";
            public const string FaqLegs = "faq_legs";
        }

        public static class State
        {
            public const string Asking = "asking";
        }

        public static class Text
        {
            public const string AskPrompt = """
                💬 <b>Задайте вопрос о тренировках</b>
                
                Напишите ваш вопрос, и AI поможет вам.
                
                <i>Примеры:</i>
                • "Как накачать грудь?"
                • "Сколько раз в неделю тренироваться?"
                • "Как правильно приседать?"
                • "Нужно ли делать кардио?"
                
                Или нажмите кнопку "Назад" для возврата в меню.
                """;

            public const string ButtonFaqFrequency = "📅 Частота";
            public const string ButtonFaqProgram = "📋 Программа";
            public const string ButtonFaqWarmup = "🔥 Разминка";
            public const string ButtonFaqCardio = "❤️ Кардио";
            public const string ButtonFaqChest = "🏋️ Грудь";
            public const string ButtonFaqBack = "🔱 Спина";
            public const string ButtonFaqLegs = "🦵 Ноги";
            public const string ButtonAskQuestion = "💬 Задать вопрос";
            public const string ButtonBack = "◀️ Назад";
            public const string ButtonAskAnother = "💬 Задать ещё вопрос";
        }

        public static Keyboard MainMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqFrequency, $"{commandName}:{Action.FaqFrequency}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqProgram, $"{commandName}:{Action.FaqProgram}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqWarmup, $"{commandName}:{Action.FaqWarmup}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqCardio, $"{commandName}:{Action.FaqCardio}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqChest, $"{commandName}:{Action.FaqChest}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqBack, $"{commandName}:{Action.FaqBack}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqLegs, $"{commandName}:{Action.FaqLegs}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonAskQuestion, $"{commandName}:{Action.Ask}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonBack, "questions:main")
                }
            );
        }

        public static Keyboard FaqMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonAskQuestion, $"{commandName}:{Action.Ask}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonBack, $"{commandName}:{Action.Main}")
                }
            );
        }

        public static Keyboard QuestionMenu(string commandName)
        {
            return Keyboard.FromSingleButton(Text.ButtonBack, $"{commandName}:{Action.Main}");
        }

        public static Keyboard ContinueMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonAskAnother, $"{commandName}:{Action.Ask}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonBack, $"{commandName}:{Action.Main}")
                }
            );
        }
    }

    public QuestionsWorkoutsCommand(
        ITelegramBotAdapter adapter,
        QuestionsAIService questionsAI,
        ILogger<QuestionsWorkoutsCommand> logger,
        IServiceProvider serviceProvider)
    {
        Console.WriteLine($"\n💪 QuestionsWorkoutsCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  questionsAI is null: {questionsAI == null}");
        Console.WriteLine($"  logger is null: {logger == null}");
        Console.WriteLine($"  serviceProvider is null: {serviceProvider == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _questionsAI = questionsAI ?? throw new ArgumentNullException(nameof(questionsAI));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        Console.WriteLine($"✅ QuestionsWorkoutsCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"💪 QuestionsWorkoutsCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText) => false;

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
        => await Task.CompletedTask;

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("QuestionsWorkoutsCommand received: {Action}", context.Action);

        try
        {
            switch (context.Action)
            {
                case Const.Action.Main:
                    await ShowMainMenu(context.UserId, context.MessageId, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.Ask:
                    await StartAsking(context.UserId, context.MessageId, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqFrequency:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "frequency", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqProgram:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "program", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqWarmup:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "warmup", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqCardio:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "cardio", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqChest:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "chest", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqBack:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "back", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqLegs:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "legs", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.Back:
                    _stateManager?.ClearState(context.UserId);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    await ShowQuestionsMainMenu(context.UserId, context.MessageId, cancellationToken);
                    break;

                default:
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QuestionsWorkoutsCommand");
            await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing workouts question from user {UserId}: {Message}", context.UserId, context.Text);

        try
        {
            await _adapter.SendChatActionAsync(context.UserId, BotChatAction.Typing, cancellationToken);

            var processingMsg = await _adapter.SendMessageAsync(
                context.UserId,
                "🤔 Ищу ответ...",
                cancellationToken: cancellationToken);

            var answer = await _questionsAI.AnswerInCategoryAsync(context.UserId, context.Text, "workouts", cancellationToken);

            await _adapter.DeleteMessageAsync(context.UserId, processingMsg, cancellationToken);

            await _adapter.SendMessageAsync(
                context.UserId,
                answer,
                Const.ContinueMenu(Name),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing workouts question for user {UserId}", context.UserId);
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

    private async Task ShowFaqAnswer(long userId, int messageId, string topic, CancellationToken cancellationToken)
    {
        var answer = QuestionAnswers.GetAnswer("workouts", topic)
            ?? "❌ Информация по данному вопросу временно недоступна.";

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: answer,
            keyboard: Const.FaqMenu(Name),
            cancellationToken: cancellationToken);
    }

    private async Task StartAsking(long userId, int messageId, CancellationToken cancellationToken)
    {
        if (_stateManager == null)
        {
            _logger.LogError("StateManager not initialized for QuestionsWorkoutsCommand");
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
            text: Const.Text.AskPrompt,
            keyboard: Const.QuestionMenu(Name),
            cancellationToken: cancellationToken);
    }

    private async Task ShowQuestionsMainMenu(long userId, int messageId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuestionsCommand>>();

        var questionsCommand = new QuestionsCommand(
            _adapter,
            _questionsAI,
            logger,
            _serviceProvider);

        await questionsCommand.HandleCallbackAsync(
            new CallbackContext
            {
                UserId = userId,
                MessageId = messageId,
                CallbackQueryId = "",
                Action = "main",
                Parameters = Array.Empty<string>(),
                RawData = "questions:main"
            },
            cancellationToken);
    }
}
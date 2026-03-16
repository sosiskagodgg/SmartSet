// FitnessTracker.TelegramBot/Commands/QuestionsNutritionCommand.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.AI.Data;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для раздела "Питание" - FAQ и вопросы о питании
/// </summary>
public class QuestionsNutritionCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly QuestionsAIService _questionsAI;
    private readonly ILogger<QuestionsNutritionCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IUserStateManager? _stateManager;

    public string Name => "questions_nutrition";

    private static class Const
    {
        public const string Title = "🥗 <b>Питание</b>\n\nВыберите тему или задайте свой вопрос:";

        public static class Action
        {
            public const string Main = "main";
            public const string Ask = "ask";
            public const string Back = "back";

            public const string FaqCalories = "faq_calories";
            public const string FaqProtein = "faq_protein";
            public const string FaqWater = "faq_water";
            public const string FaqCarbs = "faq_carbs";
            public const string FaqFat = "faq_fat";
        }

        public static class State
        {
            public const string Asking = "asking";
        }

        public static class Text
        {
            public const string AskPrompt = """
                💬 <b>Задайте вопрос о питании</b>
                
                Напишите ваш вопрос, и AI поможет вам.
                
                <i>Примеры:</i>
                • "Сколько углеводов нужно?"
                • "Что есть до тренировки?"
                • "Как похудеть?"
                • "Сколько воды пить?"
                
                Или нажмите кнопку "Назад" для возврата в меню.
                """;

            public const string ButtonFaqCalories = "🔥 Калории";
            public const string ButtonFaqProtein = "🥩 Белок";
            public const string ButtonFaqWater = "💧 Вода";
            public const string ButtonFaqCarbs = "🍚 Углеводы";
            public const string ButtonFaqFat = "🥑 Жиры";
            public const string ButtonAskQuestion = "💬 Задать вопрос";
            public const string ButtonBack = "◀️ Назад";
            public const string ButtonAskAnother = "💬 Задать ещё вопрос";
        }

        public static Keyboard MainMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqCalories, $"{commandName}:{Action.FaqCalories}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqProtein, $"{commandName}:{Action.FaqProtein}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqWater, $"{commandName}:{Action.FaqWater}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqCarbs, $"{commandName}:{Action.FaqCarbs}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqFat, $"{commandName}:{Action.FaqFat}")
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

    public QuestionsNutritionCommand(
        ITelegramBotAdapter adapter,
        QuestionsAIService questionsAI,
        ILogger<QuestionsNutritionCommand> logger,
        IServiceProvider serviceProvider)
    {
        Console.WriteLine($"\n🥗 QuestionsNutritionCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  questionsAI is null: {questionsAI == null}");
        Console.WriteLine($"  logger is null: {logger == null}");
        Console.WriteLine($"  serviceProvider is null: {serviceProvider == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _questionsAI = questionsAI ?? throw new ArgumentNullException(nameof(questionsAI));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        Console.WriteLine($"✅ QuestionsNutritionCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"🥗 QuestionsNutritionCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText) => false;

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
        => await Task.CompletedTask;

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("QuestionsNutritionCommand received: {Action}", context.Action);

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

                case Const.Action.FaqCalories:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "calories", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqProtein:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "protein", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqWater:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "water", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqCarbs:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "carbs", cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqFat:
                    await ShowFaqAnswer(context.UserId, context.MessageId, "fat", cancellationToken);
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
            _logger.LogError(ex, "Error in QuestionsNutritionCommand");
            await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing nutrition question from user {UserId}: {Message}", context.UserId, context.Text);

        try
        {
            await _adapter.SendChatActionAsync(context.UserId, BotChatAction.Typing, cancellationToken);

            var processingMsg = await _adapter.SendMessageAsync(
                context.UserId,
                "🤔 Ищу ответ...",
                cancellationToken: cancellationToken);

            var answer = await _questionsAI.AnswerInCategoryAsync(context.UserId, context.Text, "nutrition", cancellationToken);

            await _adapter.DeleteMessageAsync(context.UserId, processingMsg, cancellationToken);

            await _adapter.SendMessageAsync(
                context.UserId,
                answer,
                Const.ContinueMenu(Name),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing nutrition question for user {UserId}", context.UserId);
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
        var answer = QuestionAnswers.GetAnswer("nutrition", topic)
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
            _logger.LogError("StateManager not initialized for QuestionsNutritionCommand");
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
// FitnessTracker.TelegramBot/Commands/QuestionsAboutBotCommand.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.AI.Data;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для раздела "О боте" - FAQ и вопросы о боте
/// </summary>
public class QuestionsAboutBotCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly QuestionsAIService _questionsAI;
    private readonly ILogger<QuestionsAboutBotCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IUserStateManager? _stateManager;

    public string Name => "questions_about_bot";

    private static class Const
    {
        public const string Title = """
            🤖 <b>О боте</b>
            
            Здесь вы можете узнать информацию о возможностях бота или задать свой вопрос.
            """;

        public static class Action
        {
            public const string Main = "main";
            public const string Ask = "ask";
            public const string Back = "back";

            public const string FaqWhatCan = "faq_what_can";
            public const string FaqWhyNeed = "faq_why_need";
            public const string FaqIsFree = "faq_is_free";
            public const string FaqCreator = "faq_creator";
        }

        public static class State
        {
            public const string Asking = "asking";
        }

        public static class Text
        {
            public const string AskPrompt = """
                💬 <b>Задайте свой вопрос о боте</b>
                
                Напишите в чат любой вопрос о функциях или работе бота.
                
                <i>Примеры вопросов:</i>
                • "Как изменить вес в профиле?"
                • "Как создать тренировку?"
                • "Где посмотреть статистику?"
                • "Как работает AI-помощник?"
                
                Или нажмите кнопку "Назад" для возврата в меню.
                """;

            public const string ButtonFaqWhatCan = "❓ Что умеет бот?";
            public const string ButtonFaqWhyNeed = "❓ Для чего нужен?";
            public const string ButtonFaqIsFree = "❓ Это бесплатно?";
            public const string ButtonFaqCreator = "❓ Кто создатель?";
            public const string ButtonAskQuestion = "💬 Задать вопрос";
            public const string ButtonBack = "◀️ К вопросам";
            public const string ButtonAskAnother = "💬 Задать ещё вопрос";
        }

        public static Keyboard MainMenu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqWhatCan, $"{commandName}:{Action.FaqWhatCan}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqWhyNeed, $"{commandName}:{Action.FaqWhyNeed}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqIsFree, $"{commandName}:{Action.FaqIsFree}")
                },
                new List<Button> {
                    Models.Button.Create(Text.ButtonFaqCreator, $"{commandName}:{Action.FaqCreator}")
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

        // FAQ ответы
        public const string FaqWhatCanAnswer = """
            <b>❓ Что умеет этот бот?</b>
            
            Fitness Tracker Bot помогает вам:
            
            📝 <b>Создавать тренировки</b>
            • Индивидуальные программы
            • Разбивка по дням недели
            • Различные упражнения
            
            📊 <b>Отслеживать прогресс</b>
            • Статистика тренировок
            • История результатов
            
            💪 <b>Выполнять тренировки</b>
            • Пошаговое выполнение
            • Запись результатов
            
            📈 <b>Следить за параметрами</b>
            • Вес и процент жира
            • Динамика изменений
            
            🤖 <b>AI-помощник</b>
            • Ответы на вопросы
            • Советы по тренировкам
            • Рекомендации по питанию
            """;

        public const string FaqWhyNeedAnswer = """
            <b>❓ Для чего нужен этот бот?</b>
            
            Бот создан для того, чтобы сделать ваши тренировки:
            
            🎯 <b>Эффективнее</b>
            • Четкий план тренировок
            • Контроль прогресса
            • Достижение целей быстрее
            
            📱 <b>Удобнее</b>
            • Всегда под рукой в Telegram
            • Не нужно бумажных дневников
            • Быстрый доступ к статистике
            
            📊 <b>Понятнее</b>
            • Наглядная статистика
            • Анализ результатов
            • Понимание прогресса
            """;

        public const string FaqIsFreeAnswer = """
            <b>❓ Это бесплатно?</b>
            
            ✅ <b>Да, бот полностью бесплатный!</b>
            
            Все функции доступны без ограничений:
            • Создание тренировок
            • Отслеживание прогресса
            • Статистика и графики
            • AI-консультации
            
            Никаких планов по введению платы — проект развивается для сообщества.
            """;

        public const string FaqCreatorAnswer = """
            <b>❓ Кто создатель?</b>
            
            👨‍💻 <b>О разработчике</b>
            
            Бот создан как pet-проект для изучения современных технологий и помощи людям в их фитнес-пути.
            
            🛠 <b>Технологии:</b>
            • C# / .NET 8
            • Clean Architecture
            • Telegram Bot API
            • GigaChat AI
            • PostgreSQL
            
            🌟 <b>Цель проекта:</b>
            Создать удобного и бесплатного помощника для тренировок, доступного каждому.
            """;
    }

    public QuestionsAboutBotCommand(
        ITelegramBotAdapter adapter,
        QuestionsAIService questionsAI,
        ILogger<QuestionsAboutBotCommand> logger,
        IServiceProvider serviceProvider)
    {
        Console.WriteLine($"\n🤖 QuestionsAboutBotCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  questionsAI is null: {questionsAI == null}");
        Console.WriteLine($"  logger is null: {logger == null}");
        Console.WriteLine($"  serviceProvider is null: {serviceProvider == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _questionsAI = questionsAI ?? throw new ArgumentNullException(nameof(questionsAI));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        Console.WriteLine($"✅ QuestionsAboutBotCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"🤖 QuestionsAboutBotCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText) => false;

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
        => await Task.CompletedTask;

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("QuestionsAboutBotCommand received: {Action}", context.Action);

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

                case Const.Action.FaqWhatCan:
                    await ShowFaqAnswer(context.UserId, context.MessageId, Const.FaqWhatCanAnswer, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqWhyNeed:
                    await ShowFaqAnswer(context.UserId, context.MessageId, Const.FaqWhyNeedAnswer, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqIsFree:
                    await ShowFaqAnswer(context.UserId, context.MessageId, Const.FaqIsFreeAnswer, cancellationToken);
                    await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                    break;

                case Const.Action.FaqCreator:
                    await ShowFaqAnswer(context.UserId, context.MessageId, Const.FaqCreatorAnswer, cancellationToken);
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
            _logger.LogError(ex, "Error in QuestionsAboutBotCommand");
            await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing question about bot from user {UserId}: {Message}", context.UserId, context.Text);

        try
        {
            await _adapter.SendChatActionAsync(context.UserId, BotChatAction.Typing, cancellationToken);

            var processingMsg = await _adapter.SendMessageAsync(
                context.UserId,
                "🤔 Ищу ответ...",
                cancellationToken: cancellationToken);

            var answer = await _questionsAI.AnswerInCategoryAsync(context.UserId, context.Text, "bot", cancellationToken);

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

    private async Task ShowFaqAnswer(long userId, int messageId, string answer, CancellationToken cancellationToken)
    {
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
            _logger.LogError("StateManager not initialized for QuestionsAboutBotCommand");
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
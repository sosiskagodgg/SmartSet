// FitnessTracker.TelegramBot/Commands/ProfileCommand.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.TelegramBot.Commands;

/// <summary>
/// Команда для управления профилем пользователя
/// </summary>
public class ProfileCommand : ICommand
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly IUserService _userService;
    private readonly IUserParametersService _userParametersService;
    private readonly UserParametersAIService _userParametersAI;
    private readonly ILogger<ProfileCommand> _logger;
    private IUserStateManager? _stateManager;

    public string Name => "profile";

    private static class Const
    {
        public const string Title = "👤 <b>Ваш профиль</b>";

        public static class Action
        {
            public const string Main = "main";
            public const string Edit = "edit";
            public const string Back = "back";
        }

        public static class State
        {
            public const string Editing = "editing";
        }

        public static class Text
        {
            public const string EditPrompt = """
                ✏️ <b>Режим редактирования</b>
                
                Напишите в свободной форме, что хотите изменить.
                
                Например:
                • "мой рост 180 см"
                • "вес 75 кг"
                • "процент жира 15"
                • "опыт средний"
                • "хочу набрать массу"
                
                Или нажмите кнопку "Назад" для отмены.
                """;

            public const string ButtonEdit = "✏️ Изменить";
            public const string ButtonBack = "◀️ Назад";
        }

        public static Keyboard Menu(string commandName)
        {
            return Keyboard.FromRows(
                new List<Button> { Models.Button.Create(Text.ButtonEdit, $"{commandName}:{Action.Edit}") },
                new List<Button> { Models.Button.Create(Text.ButtonBack, "main:back") }
            );
        }

        public static Keyboard EditMenu(string commandName)
        {
            return Keyboard.FromSingleButton(Text.ButtonBack, $"{commandName}:{Action.Main}");
        }
    }

    public ProfileCommand(
        ITelegramBotAdapter adapter,
        IUserService userService,
        IUserParametersService userParametersService,
        UserParametersAIService userParametersAI,
        ILogger<ProfileCommand> logger)
    {
        Console.WriteLine($"\n👤 ProfileCommand КОНСТРУКТОР {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"  adapter is null: {adapter == null}");
        Console.WriteLine($"  userService is null: {userService == null}");
        Console.WriteLine($"  userParametersService is null: {userParametersService == null}");
        Console.WriteLine($"  userParametersAI is null: {userParametersAI == null}");
        Console.WriteLine($"  logger is null: {logger == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _userParametersService = userParametersService ?? throw new ArgumentNullException(nameof(userParametersService));
        _userParametersAI = userParametersAI ?? throw new ArgumentNullException(nameof(userParametersAI));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Console.WriteLine($"✅ ProfileCommand конструктор завершен успешно");
    }

    public void SetStateManager(IUserStateManager stateManager)
    {
        _stateManager = stateManager;
        Console.WriteLine($"👤 ProfileCommand получил StateManager: {stateManager != null}");
    }

    public bool CanHandle(string messageText)
    {
        var lower = messageText.ToLowerInvariant();
        return lower.Contains("профиль") || lower.Contains("параметры") || lower.Contains("мои данные");
    }

    public async Task HandleMessageAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        await ShowProfile(context.UserId, context.MessageId, cancellationToken);
    }

    public async Task HandleCallbackAsync(CallbackContext context, CancellationToken cancellationToken = default)
    {
        switch (context.Action)
        {
            case Const.Action.Main:
                await ShowProfile(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            case Const.Action.Edit:
                await StartEditing(context.UserId, context.MessageId, cancellationToken);
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;

            default:
                await _adapter.AnswerCallbackAsync(context.CallbackQueryId, cancellationToken: cancellationToken);
                break;
        }
    }

    public async Task HandleStateInputAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing profile edit for user {UserId}: {Message}",
            context.UserId, context.Text);

        try
        {
            await _adapter.SendChatActionAsync(context.UserId, BotChatAction.Typing, cancellationToken);

            var result = await _userParametersAI.UpdateParametersDirectAsync(
                context.UserId,
                context.Text,
                cancellationToken);

            _stateManager?.ClearState(context.UserId);

            await _adapter.SendMessageAsync(
                userId: context.UserId,
                text: result,
                keyboard: Const.Menu(Name),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", context.UserId);
            await _adapter.SendMessageAsync(
                userId: context.UserId,
                text: "❌ Ошибка при обновлении. Попробуйте еще раз.",
                keyboard: Const.EditMenu(Name),
                cancellationToken: cancellationToken);
        }
    }

    private async Task ShowProfile(long userId, int messageId, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            await _adapter.EditMessageAsync(
                userId: userId,
                messageId: messageId,
                text: "❌ Пользователь не найден",
                cancellationToken: cancellationToken);
            return;
        }

        var parameters = await _userParametersService.GetUserParametersAsync(userId, cancellationToken);
        var text = FormatProfileText(user, parameters);

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: text,
            keyboard: Const.Menu(Name),
            cancellationToken: cancellationToken);
    }

    private async Task StartEditing(long userId, int messageId, CancellationToken cancellationToken)
    {
        if (_stateManager == null)
        {
            _logger.LogError("StateManager not initialized for ProfileCommand");
            return;
        }

        var state = new UserState
        {
            CommandName = Name,
            Step = Const.State.Editing,
            Data = new Dictionary<string, object>()
        };
        _stateManager.SetState(userId, state);

        await _adapter.EditMessageAsync(
            userId: userId,
            messageId: messageId,
            text: Const.Text.EditPrompt,
            keyboard: Const.EditMenu(Name),
            cancellationToken: cancellationToken);
    }

    private string FormatProfileText(User user, UserParameters? parameters)
    {
        var height = parameters?.Height?.ToString() ?? "не указано";
        var weight = parameters?.Weight?.ToString() ?? "не указано";
        var bodyFat = parameters?.BodyFat?.ToString() ?? "не указано";

        var experience = parameters?.Experience?.ToLowerInvariant() switch
        {
            "beginner" => "начинающий",
            "intermediate" => "средний",
            "advanced" => "продвинутый",
            _ => "не указан"
        };

        var goals = string.IsNullOrEmpty(parameters?.Goals) ? "не указаны" : parameters.Goals;

        return $"""
            {Const.Title}
            
            <b>Параметры:</b>
            • Рост: {height} см
            • Вес: {weight} кг
            • Процент жира: {bodyFat}%
            • Опыт тренировок: {experience}
            
            <b>Цели:</b>
            {goals}
            """;
    }
}
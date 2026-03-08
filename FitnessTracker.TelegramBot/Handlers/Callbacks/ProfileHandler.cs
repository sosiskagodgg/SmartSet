// FitnessTracker.TelegramBot/Handlers/ProfileHandler.cs

using FitnessTracker.AI.PublicServices;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.TelegramBot.Handlers;

/// <summary>
/// Один класс для всего, что связано с профилем
/// </summary>
public class ProfileHandler : HandlerBase, ICallbackHandler, IStateHandler
{
    private readonly IUserParametersService _userParametersService;
    private readonly UserParametersAIService _userParametersAI;

    // ===== Интерфейсы =====
    public string CallbackPrefix => "profile";
    public string StateType => "waiting_profile_edit";

    // ===== Конструктор =====
    public ProfileHandler(
        ITelegramBotAdapter telegram,
        ILogger<ProfileHandler> logger,
        UserStateService stateService,
        IUserService userService,
        IUserParametersService userParametersService,
        UserParametersAIService userParametersAI)
        : base(telegram, logger, stateService, userService)
    {
        _userParametersService = userParametersService;
        _userParametersAI = userParametersAI;
    }

    // ===== КОНСТАНТЫ =====
    private static class Callback
    {
        public const string Main = "main";
        public const string Edit = "edit";
        public const string Back = "back";

        // Полные колбэки
        public const string MainFull = "ft:profile:main";
        public const string EditFull = "ft:profile:edit";
        public const string BackFull = "ft:profile:back";
    }

    private static class Text
    {
        public const string Title = "👤 Ваш профиль";
        public const string ButtonEdit = "✏️ Изменить";
        public const string ButtonBack = "◀️ Назад";
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
    }

    // ===== ОБРАБОТКА КНОПОК (ICallbackHandler) =====
    public async Task HandleAsync(
        long userId,
        CallbackInfo callback,
        int messageId,
        string callbackQueryId,
        CancellationToken ct)
    {
        switch (callback.Action)
        {
            case Callback.Main:
                await ShowProfile(userId, messageId, ct);
                break;

            case Callback.Edit:
                await StartEditing(userId, messageId, ct);
                break;

            case Callback.Back:
                await CancelEditing(userId, messageId, ct);
                break;
        }

        await AnswerCallback(callbackQueryId, ct: ct);
    }

    // ===== ОБРАБОТКА ТЕКСТА В СОСТОЯНИИ (IStateHandler) =====
    public async Task HandleAsync(
        long userId,
        string messageText,
        int messageId,
        Dictionary<string, object> stateData,
        CancellationToken ct)
    {
        _logger.LogInformation("Processing profile edit for user {UserId}: {Message}",
            userId, messageText);

        try
        {
            // Показываем что обрабатываем
            var processingMsg = await SendMessage(userId, "🤔 Обновляю параметры...", ct: ct);

            // AI обновляет параметры
            await _userParametersAI.UpdateUserParametersFromMessageAsync(
                userId,
                messageText,
                ct);

            // Удаляем сообщение "обработка"
            await DeleteMessage(userId, processingMsg, ct);

            // Очищаем состояние
            await ClearState(userId);

            // Показываем обновленный профиль
            await ShowProfile(userId, null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            await SendMessage(userId, "❌ Ошибка при обновлении. Попробуйте еще раз.", ct: ct);
            await ClearState(userId);
        }
    }

    // ===== ОСНОВНАЯ ЛОГИКА (приватные методы) =====

    /// <summary>
    /// Показать профиль
    /// </summary>
    private async Task ShowProfile(long userId, int? messageId = null, CancellationToken ct = default)
    {
        var user = await _userService.GetUserByTelegramIdAsync(userId, ct);
        if (user == null)
        {
            await SendMessage(userId, "❌ Пользователь не найден", ct: ct);
            return;
        }

        var parameters = await _userParametersService.GetUserParametersAsync(userId, ct);
        var text = FormatProfileText(user, parameters);
        var keyboard = GetProfileKeyboard();

        if (messageId.HasValue)
            await EditMessage(userId, messageId.Value, text, keyboard, ct);
        else
            await SendMessage(userId, text, keyboard, ct);
    }

    /// <summary>
    /// Начать редактирование
    /// </summary>
    private async Task StartEditing(long userId, int messageId, CancellationToken ct)
    {
        await SetState(userId, StateType);
        await EditMessage(userId, messageId, Text.EditPrompt,
            Keyboard.FromSingleButton(Text.ButtonBack, Callback.BackFull), ct);
    }

    /// <summary>
    /// Отменить редактирование
    /// </summary>
    private async Task CancelEditing(long userId, int messageId, CancellationToken ct)
    {
        await ClearState(userId);
        await ShowProfile(userId, messageId, ct);
    }

    /// <summary>
    /// Клавиатура профиля
    /// </summary>
    private Keyboard GetProfileKeyboard()
    {
        return Keyboard.FromRows(
            new List<Button>
            {
                Button.Create(Text.ButtonEdit, Callback.EditFull),
                Button.Create(Text.ButtonBack, "ft:main:back")
            }
        );
    }


    /// <summary>
    /// Форматирование текста профиля
    /// </summary>
    private string FormatProfileText(Domain.Entities.User user, Domain.Entities.UserParameters? parameters)
    {
        var height = parameters?.Height?.ToString() ?? "не указано";
        var weight = parameters?.Weight?.ToString() ?? "не указано";
        var bodyFat = parameters?.BodyFat?.ToString() ?? "не указано";

        // Опыт может быть как на русском, так и на английском
        var experience = parameters?.Experience?.ToLowerInvariant() switch
        {
            "beginner" or "новичок" or "начинающий" => "начинающий",
            "intermediate" or "средний" => "средний",
            "advanced" or "продвинутый" or "профи" => "продвинутый",
            null => "не указан",
            _ => parameters.Experience // если что-то другое - показываем как есть
        };

        var goals = string.IsNullOrEmpty(parameters?.Goals) ? "не указаны" : parameters.Goals;

        return $"""
        {Text.Title}
        
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
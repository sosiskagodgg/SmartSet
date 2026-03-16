// FitnessTracker.TelegramBot/Bot.cs
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FitnessTracker.TelegramBot.Abstractions;

namespace FitnessTracker.TelegramBot;

/// <summary>
/// Главный класс бота. Получает все апдейты и маршрутизирует их командам.
/// </summary>
public class Bot : IUserStateManager
{
    private readonly ITelegramBotAdapter _adapter;
    private readonly IEnumerable<ICommand> _allCommands;
    private readonly ILogger<Bot> _logger;

    // Словарь команд для быстрого доступа по имени
    private readonly Dictionary<string, ICommand> _commands;

    // Состояния пользователей (в памяти)
    private readonly ConcurrentDictionary<long, UserState> _userStates = new();

    public Bot(
        ITelegramBotAdapter adapter,
        IEnumerable<ICommand> commands,
        ILogger<Bot> logger)
    {
        Console.WriteLine($"\n🤖 Bot КОНСТРУКТОР НАЧАЛСЯ! {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"🤖 adapter is null: {adapter == null}");
        Console.WriteLine($"🤖 commands is null: {commands == null}");
        Console.WriteLine($"🤖 logger is null: {logger == null}");

        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _allCommands = commands ?? throw new ArgumentNullException(nameof(commands));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Подсчитываем команды
        var commandsList = commands.ToList();
        Console.WriteLine($"🤖 Команд получено: {commandsList.Count}");

        // Выводим все команды для диагностики
        foreach (var cmd in commandsList)
        {
            Console.WriteLine($"🤖   - {cmd.GetType().Name} (Name: {cmd.Name})");
        }

        // Строим словарь команд по имени
        Console.WriteLine("🤖 Построение словаря команд...");
        try
        {
            _commands = commandsList.ToDictionary(c => c.Name, c => c);
            Console.WriteLine($"🤖 Словарь построен. Команд в словаре: {_commands.Count}");

            // Устанавливаем StateManager для каждой команды
            Console.WriteLine("🤖 Установка StateManager для команд...");
            foreach (var command in _commands.Values)
            {
                command.SetStateManager(this);
                Console.WriteLine($"🤖   StateManager установлен для {command.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🤖 ОШИБКА при построении словаря: {ex.Message}");
            Console.WriteLine($"🤖 StackTrace: {ex.StackTrace}");
            throw;
        }

        // Проверяем уникальность имен
        if (_commands.Count != commandsList.Count)
        {
            var duplicateNames = commandsList
                .GroupBy(c => c.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            Console.WriteLine($"🤖 Найдены дубликаты команд: {string.Join(", ", duplicateNames)}");
            throw new InvalidOperationException(
                $"Duplicate command names found: {string.Join(", ", duplicateNames)}");
        }

        _logger.LogInformation("Bot initialized with {Count} commands", _commands.Count);
        Console.WriteLine($"🤖 Bot КОНСТРУКТОР ЗАВЕРШЕН! {DateTime.Now:HH:mm:ss.fff}\n");
    }

    /// <summary>
    /// Точка входа для всех апдейтов от Telegram
    /// </summary>
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessageAsync(update.Message!, cancellationToken);
                    break;

                case UpdateType.CallbackQuery:
                    await HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken);
                    break;

                default:
                    _logger.LogDebug("Ignoring update type: {UpdateType}", update.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in bot");
        }
    }

    /// <summary>
    /// Обработка входящего сообщения
    /// </summary>
    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var userId = message.Chat.Id;
        var text = message.Text ?? string.Empty;
        var messageId = message.MessageId;

        _logger.LogInformation("Message from user {UserId}: {Text}", userId, text);

        // ШАГ 1: Проверяем, есть ли состояние у пользователя
        if (_userStates.TryGetValue(userId, out var state))
        {
            // Проверяем, не устарело ли состояние
            if (state.IsExpired)
            {
                _userStates.TryRemove(userId, out _);
                await _adapter.SendMessageAsync(
                    userId: userId,
                    text: "⏰ Время ожидания истекло. Начните заново.",
                    keyboard: null,
                    cancellationToken: cancellationToken);
                return;
            }

            // Ищем команду, которая создала это состояние
            if (_commands.TryGetValue(state.CommandName, out var command))
            {
                var context = new MessageContext
                {
                    UserId = userId,
                    Text = text,
                    MessageId = messageId,
                    State = state
                };

                await command.HandleStateInputAsync(context, cancellationToken);
                return;
            }
            else
            {
                // Странная ситуация: команда есть в состоянии, но не найдена
                _logger.LogWarning("Command {CommandName} not found for user {UserId} in state",
                    state.CommandName, userId);
                _userStates.TryRemove(userId, out _);
            }
        }

        // ШАГ 2: Если состояния нет, ищем команду по тексту
        foreach (var command in _allCommands)
        {
            if (command.CanHandle(text))
            {
                var context = new MessageContext
                {
                    UserId = userId,
                    Text = text,
                    MessageId = messageId,
                    State = null
                };

                await command.HandleMessageAsync(context, cancellationToken);
                return;
            }
        }

        // ШАГ 3: Ничего не нашли
        await _adapter.SendMessageAsync(
            userId: userId,
            text: "🤔 Я не понял. Напишите /start или выберите команду из меню.",
            keyboard: null,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка колбэка (нажатия на кнопку)
    /// </summary>
    private async Task HandleCallbackQueryAsync(CallbackQuery callback, CancellationToken cancellationToken)
    {
        var userId = callback.From.Id;
        var callbackData = callback.Data ?? string.Empty;
        var messageId = callback.Message?.MessageId ?? 0;

        _logger.LogInformation("Callback from user {UserId}: {Data}", userId, callbackData);

        // Парсим колбэк (формат: "commandName:action:param1:param2")
        var parts = callbackData.Split(':');
        if (parts.Length < 2)
        {
            _logger.LogWarning("Invalid callback format: {Data}", callbackData);
            await _adapter.AnswerCallbackAsync(
                callbackQueryId: callback.Id,
                text: null,
                showAlert: false,
                cancellationToken: cancellationToken);
            return;
        }

        var commandName = parts[0];
        var action = parts[1];
        var parameters = parts.Skip(2).ToArray();

        // Ищем команду
        if (!_commands.TryGetValue(commandName, out var command))
        {
            _logger.LogWarning("Command {CommandName} not found for callback", commandName);
            await _adapter.AnswerCallbackAsync(
                callbackQueryId: callback.Id,
                text: "❌ Команда не найдена",
                showAlert: true,
                cancellationToken: cancellationToken);
            return;
        }

        var context = new CallbackContext
        {
            UserId = userId,
            CallbackQueryId = callback.Id,
            MessageId = messageId,
            Action = action,
            Parameters = parameters,
            RawData = callbackData
        };

        await command.HandleCallbackAsync(context, cancellationToken);
    }

    /// <summary>
    /// Установить состояние для пользователя
    /// </summary>
    public void SetState(long userId, UserState state)
    {
        _userStates[userId] = state;
        _logger.LogDebug("State set for user {UserId}: {CommandName}/{Step}",
            userId, state.CommandName, state.Step);
    }

    /// <summary>
    /// Получить состояние пользователя
    /// </summary>
    public UserState? GetState(long userId)
    {
        return _userStates.TryGetValue(userId, out var state) ? state : null;
    }

    /// <summary>
    /// Очистить состояние пользователя
    /// </summary>
    public void ClearState(long userId)
    {
        _userStates.TryRemove(userId, out _);
        _logger.LogDebug("State cleared for user {UserId}", userId);
    }
}
// FitnessTracker.TelegramBot/Services/BotBackgroundService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FitnessTracker.TelegramBot.Services;

public class BotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly Bot _bot;
    private readonly ILogger<BotBackgroundService> _logger;

    public BotBackgroundService(
        ITelegramBotClient botClient,
        Bot bot,
        ILogger<BotBackgroundService> logger)
    {
        _botClient = botClient;
        _bot = bot;
        _logger = logger;

        // МАКСИМАЛЬНАЯ ДИАГНОСТИКА
        Console.WriteLine("🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥");
        Console.WriteLine("🔥 BotBackgroundService КОНСТРУКТОР ВЫЗВАН! 🔥");
        Console.WriteLine($"🔥 Время: {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"🔥 botClient is null: {botClient == null}");
        Console.WriteLine($"🔥 bot is null: {bot == null}");
        Console.WriteLine($"🔥 logger is null: {logger == null}");
        Console.WriteLine("🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥");

        _logger.LogInformation("✅ BotBackgroundService constructor executed");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯");
        Console.WriteLine("🎯 BotBackgroundService.ExecuteAsync ЗАПУЩЕН! 🎯");
        Console.WriteLine($"🎯 Время: {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine("🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯");

        _logger.LogInformation("🎯 BotBackgroundService.ExecuteAsync started");

        try
        {
            Console.WriteLine("📡 Connecting to Telegram API...");
            var me = await _botClient.GetMe(stoppingToken);
            Console.WriteLine($"✅ Connected! Bot @{me.Username}");
            _logger.LogInformation("✅ Connected! Bot @{Username}", me.Username);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            Console.WriteLine("✅ Bot is listening for updates");
            _logger.LogInformation("✅ Bot is listening for updates");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error starting bot: {ex.Message}");
            Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            _logger.LogError(ex, "❌ Error starting bot");
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
        Console.WriteLine($"\n📨 Update received: {update.Type} at {DateTime.Now:HH:mm:ss.fff}");
        _logger.LogInformation("📨 Update received: {UpdateType}", update.Type);
        await _bot.HandleUpdateAsync(update, token);
    }

    private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
    {
        Console.WriteLine($"❌ Error in bot: {exception.Message}");
        Console.WriteLine($"❌ Source: {source}");
        _logger.LogError(exception, "❌ Error in bot");
        return Task.CompletedTask;
    }
}
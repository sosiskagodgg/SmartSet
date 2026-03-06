using Telegram.Bot;
using Telegram.Bot.Polling;

namespace FitnessTracker.TelegramBot.Services;

public class BotService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private readonly ILogger<BotService> _logger;

    public BotService(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        ILogger<BotService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting bot...");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [] // получать все типы
        };

        _botClient.StartReceiving(
            _updateHandler.HandleUpdateAsync,
            _updateHandler.HandleErrorAsync,
            receiverOptions,
            stoppingToken
        );

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Bot @{Username} started", me.Username);
    }
}
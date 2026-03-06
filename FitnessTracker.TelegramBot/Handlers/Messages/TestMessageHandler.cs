using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;

namespace FitnessTracker.TelegramBot.Handlers.Messages;

public class TestMessageHandler : HandlerBase, IMessageHandler
{
    // Константы прямо в классе
    private static class Text
    {
        public const string HelloResponse = "Привет! Я тестовый бот. Нажми на кнопку ниже:";
        public const string HowAreYouResponse = "Отлично, а у тебя как?";
        public const string DefaultResponse = "Я понимаю только:\n- привет\n- как дела";
    }

    private static class Callback
    {
        public const string StartFull = "ft:test:start";
    }

    public TestMessageHandler(
        ITelegramBotAdapter telegram,
        ILogger<TestMessageHandler> logger,
        UserStateService stateService)
        : base(telegram, logger, stateService)
    {
    }

    public bool CanHandle(string messageText)
    {
        var lowerMessage = messageText.ToLowerInvariant();
        return lowerMessage.Contains("привет") ||
               lowerMessage.Contains("здравствуй") ||
               lowerMessage.Contains("как дела");
    }

    public async Task HandleAsync(
        long userId,
        string messageText,
        int messageId,
        CancellationToken ct)
    {
        _logger.LogInformation("TestMessageHandler handling message: {Message} from user {UserId}", messageText, userId);

        var lowerMessage = messageText.ToLowerInvariant();

        if (lowerMessage.Contains("привет") || lowerMessage.Contains("здравствуй"))
        {
            await HandleGreeting(userId, ct);
        }
        else if (lowerMessage.Contains("как дела"))
        {
            await HandleHowAreYou(userId, ct);
        }
        else
        {
            await HandleDefault(userId, ct);
        }
    }

    private async Task HandleGreeting(long userId, CancellationToken ct)
    {
        var keyboard = Keyboard.FromRows(
            new List<Button>
            {
                Button.Create("🔁 Тестовый callback", Callback.StartFull)
            }
        );

        // ⚠️ ВНИМАНИЕ: здесь SendMessage (без Async)
        await SendMessage(userId, Text.HelloResponse, keyboard, ct);
    }

    private async Task HandleHowAreYou(long userId, CancellationToken ct)
    {
        // ⚠️ ВНИМАНИЕ: здесь SendMessage (без Async)
        await SendMessage(userId, Text.HowAreYouResponse, ct: ct);
    }

    private async Task HandleDefault(long userId, CancellationToken ct)
    {
        // ⚠️ ВНИМАНИЕ: здесь SendMessage (без Async)
        await SendMessage(userId, Text.DefaultResponse, ct: ct);
    }
}
// FitnessTracker.AI/PublicServices/QuestionsAIService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Data;
using System.Text.Json;
using FitnessTracker.AI.Core.Models;
namespace FitnessTracker.AI.PublicServices;

/// <summary>
/// AI сервис для ответов на вопросы
/// </summary>
public class QuestionsAIService
{
    private readonly ILogger<QuestionsAIService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public QuestionsAIService(
        ILogger<QuestionsAIService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Полная классификация (для главного меню)
    /// </summary>
    public async Task<string> AnswerAsync(
        long userId,
        string question,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        _logger.LogInformation("Processing free question from user {UserId}: {Question}", userId, question);

        try
        {
            // Пробуем найти ответ в локальной базе знаний
            var localAnswer = await TryFindLocalAnswer(question, cancellationToken);
            if (!string.IsNullOrEmpty(localAnswer))
            {
                return localAnswer;
            }

            // Если нет локального ответа - используем AI
            var prompt = $"""
                Ты - профессиональный фитнес-тренер. Ответь на вопрос пользователя.

                Вопрос: {question}

                Требования к ответу:
                - Используй HTML теги для форматирования: <b>, <i>
                - Для списков используй •
                - Ответ должен быть полезным и информативным
                - На русском языке
                - С эмодзи для наглядности
                """;

            var response = await aiProvider.AskAsync(
                prompt,
                new AiOptions
                {
                    SystemPrompt = "Ты - профессиональный фитнес-тренер. Отвечай подробно и структурированно.",
                    Temperature = 0.3,
                    MaxTokens = 1000
                },
                cancellationToken);

            if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
            {
                return response.Content + "\n\n<i>Ответ сгенерирован AI</i>";
            }

            return "❌ Не удалось получить ответ. Попробуйте позже.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question for user {UserId}", userId);
            return "❌ Произошла ошибка при обработке вопроса.";
        }
    }

    /// <summary>
    /// Упрощенная классификация (когда категория известна)
    /// </summary>
    public async Task<string> AnswerInCategoryAsync(
        long userId,
        string question,
        string category, // nutrition, workouts, bot
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        _logger.LogInformation("Processing {Category} question from user {UserId}: {Question}",
            category, userId, question);

        try
        {
            // Пробуем найти тему вопроса
            var topic = await ExtractTopicAsync(question, category, aiProvider, cancellationToken);

            if (!string.IsNullOrEmpty(topic))
            {
                var localAnswer = QuestionAnswers.GetAnswer(category, topic);
                if (!string.IsNullOrEmpty(localAnswer))
                {
                    return localAnswer;
                }
            }

            // Если нет локального ответа - используем AI
            var prompt = $"""
                Ты - профессиональный фитнес-тренер. Ответь на вопрос пользователя в категории "{category}".

                Вопрос: {question}

                Требования к ответу:
                - Используй HTML теги для форматирования: <b>, <i>
                - Для списков используй •
                - Ответ должен быть полезным и информативным
                - На русском языке
                - С эмодзи для наглядности
                """;

            var response = await aiProvider.AskAsync(
                prompt,
                new AiOptions
                {
                    SystemPrompt = "Ты - профессиональный фитнес-тренер. Отвечай подробно и структурированно.",
                    Temperature = 0.3,
                    MaxTokens = 1000
                },
                cancellationToken);

            if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
            {
                return response.Content + "\n\n<i>Ответ сгенерирован AI</i>";
            }

            return "❌ Не удалось получить ответ. Попробуйте позже.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {Category} question for user {UserId}", category, userId);
            return "❌ Произошла ошибка при обработке вопроса.";
        }
    }

    private async Task<string?> TryFindLocalAnswer(string question, CancellationToken cancellationToken)
    {
        // Простой поиск по ключевым словам
        var lowerQuestion = question.ToLowerInvariant();

        // Питание
        if (lowerQuestion.Contains("белок") || lowerQuestion.Contains("протеин"))
            return QuestionAnswers.GetAnswer("nutrition", "protein");

        if (lowerQuestion.Contains("жир") || lowerQuestion.Contains("жиры"))
            return QuestionAnswers.GetAnswer("nutrition", "fat");

        if (lowerQuestion.Contains("углевод"))
            return QuestionAnswers.GetAnswer("nutrition", "carbs");

        if (lowerQuestion.Contains("калори"))
            return QuestionAnswers.GetAnswer("nutrition", "calories");

        if (lowerQuestion.Contains("вод"))
            return QuestionAnswers.GetAnswer("nutrition", "water");

        // Тренировки
        if (lowerQuestion.Contains("груд") || lowerQuestion.Contains("жим") && lowerQuestion.Contains("лежа"))
            return QuestionAnswers.GetAnswer("workouts", "chest");

        if (lowerQuestion.Contains("спин") || lowerQuestion.Contains("подтягиван"))
            return QuestionAnswers.GetAnswer("workouts", "back");

        if (lowerQuestion.Contains("ног") || lowerQuestion.Contains("приседа"))
            return QuestionAnswers.GetAnswer("workouts", "legs");

        if (lowerQuestion.Contains("пресс") || lowerQuestion.Contains("живот"))
            return QuestionAnswers.GetAnswer("workouts", "abs");

        if (lowerQuestion.Contains("бицепс"))
            return QuestionAnswers.GetAnswer("workouts", "biceps");

        if (lowerQuestion.Contains("трицепс"))
            return QuestionAnswers.GetAnswer("workouts", "triceps");

        if (lowerQuestion.Contains("плеч") || lowerQuestion.Contains("дельт"))
            return QuestionAnswers.GetAnswer("workouts", "shoulders");

        if (lowerQuestion.Contains("кардио"))
            return QuestionAnswers.GetAnswer("workouts", "cardio");

        // О боте
        if (lowerQuestion.Contains("профил"))
            return QuestionAnswers.GetAnswer("bot", "profile");

        if (lowerQuestion.Contains("параметр") || lowerQuestion.Contains("вес") || lowerQuestion.Contains("рост"))
            return QuestionAnswers.GetAnswer("bot", "parameters");

        if (lowerQuestion.Contains("тренировк"))
            return QuestionAnswers.GetAnswer("bot", "workout");

        if (lowerQuestion.Contains("статистик") || lowerQuestion.Contains("прогресс"))
            return QuestionAnswers.GetAnswer("bot", "stats");

        if (lowerQuestion.Contains("созда") && lowerQuestion.Contains("тренировк"))
            return QuestionAnswers.GetAnswer("bot", "create");

        if (lowerQuestion.Contains("измен") && (lowerQuestion.Contains("вес") || lowerQuestion.Contains("рост")))
            return QuestionAnswers.GetAnswer("bot", "update");

        return null;
    }

    private async Task<string?> ExtractTopicAsync(string question, string category, IAiProvider aiProvider, CancellationToken cancellationToken)
    {
        var prompt = $"""
            Извлеки тему вопроса из сообщения пользователя.

            Категория: {category}
            Сообщение: {question}

            Верни ТОЛЬКО одно слово - тему вопроса (например: protein, chest, profile и т.д.)
            """;

        var response = await aiProvider.AskAsync(prompt, new AiOptions { Temperature = 0.1, MaxTokens = 10 }, cancellationToken);

        if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
        {
            return response.Content.Trim().ToLowerInvariant();
        }

        return null;
    }
}
// FitnessTracker.AI/PublicServices/UserParametersAIService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using System.Text.Json;
using FitnessTracker.AI.Core.Models;
namespace FitnessTracker.AI.PublicServices;

/// <summary>
/// AI сервис для изменения параметров пользователя через естественный язык
/// </summary>
public class UserParametersAIService
{
    private readonly IUserParametersService _userParametersService;
    private readonly IUserService _userService;
    private readonly ILogger<UserParametersAIService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UserParametersAIService(
        IUserParametersService userParametersService,
        IUserService userService,
        ILogger<UserParametersAIService> logger,
        IServiceProvider serviceProvider)
    {
        _userParametersService = userParametersService;
        _userService = userService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// ✨ БЫСТРЫЙ МЕТОД: сразу обновить параметры, минуя классификацию
    /// </summary>

    public async Task<string> UpdateParametersDirectAsync(
        long telegramId,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        _logger.LogInformation("🚀 Быстрое обновление параметров для пользователя {UserId}: {Message}",
            telegramId, message);

        try
        {
            // Извлекаем параметры через AI
            var extractionPrompt = $@"
Извлеки параметры пользователя из сообщения на русском языке.

Сообщение: {message}

Доступные параметры и их возможные значения:
- weight: вес в кг (только число, например 85)
- height: рост в см (только число, например 190)
- bodyFat: процент жира (только число, например 15)
- experience: уровень опыта (beginner/intermediate/advanced)
- goals: цель (lose_weight/gain_muscle/maintain/endurance/strength)

ПРИМЕРЫ:
Сообщение: ""мой рост 190 вес 85""
Ответ: [{{""type"":""height"",""value"":""190""}}, {{""type"":""weight"",""value"":""85""}}]

Сообщение: ""опыт продвинутый и цель набрать массу""
Ответ: [{{""type"":""experience"",""value"":""advanced""}}, {{""type"":""goals"",""value"":""gain_muscle""}}]

Сообщение: ""процент жира 15""
Ответ: [{{""type"":""bodyFat"",""value"":""15""}}]

Верни ТОЛЬКО JSON массив, без пояснений.
Если параметр не указан, не включай его в массив.
";

            var extractionResponse = await aiProvider.AskStructuredAsync<List<Dictionary<string, string>>>(
                extractionPrompt,
                new AiOptions { Temperature = 0.1, MaxTokens = 200 },
                cancellationToken);

            if (!extractionResponse.IsSuccess || extractionResponse.Data == null || !extractionResponse.Data.Any())
            {
                _logger.LogWarning("Failed to extract parameters from message: {Message}", message);
                return "❌ Не удалось распознать параметры. Попробуйте: 'вес 85', 'рост 190' или 'процент жира 15'";
            }

            _logger.LogInformation("Extracted {Count} parameters", extractionResponse.Data.Count);

            var changes = new List<ExtractedEntity>();
            foreach (var item in extractionResponse.Data)
            {
                if (item.TryGetValue("type", out var type) && item.TryGetValue("value", out var value))
                {
                    changes.Add(new ExtractedEntity { Type = type, Value = value });
                    _logger.LogDebug("Extracted: {Type} = {Value}", type, value);
                }
            }

            if (!changes.Any())
            {
                return "❌ Не удалось распознать параметры. Попробуйте указать их явно.";
            }

            var updatedParams = await ApplyChangesAsync(telegramId, changes, cancellationToken);
            return FormatSuccessResponse(updatedParams, changes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parameters for user {UserId}", telegramId);
            return "❌ Ошибка при обновлении параметров.";
        }
    }

    /// <summary>
    /// Изменить вес
    /// </summary>
    public async Task<string> UpdateWeightAsync(long telegramId, decimal weight, CancellationToken ct = default)
    {
        await _userParametersService.UpdateWeightAsync(telegramId, weight, ct);
        return $"✅ Вес обновлен: {weight} кг";
    }

    /// <summary>
    /// Изменить рост
    /// </summary>
    public async Task<string> UpdateHeightAsync(long telegramId, int height, CancellationToken ct = default)
    {
        await _userParametersService.UpdateHeightAsync(telegramId, height, ct);
        return $"✅ Рост обновлен: {height} см";
    }

    /// <summary>
    /// Изменить процент жира
    /// </summary>
    public async Task<string> UpdateBodyFatAsync(long telegramId, decimal bodyFat, CancellationToken ct = default)
    {
        await _userParametersService.UpdateBodyFatAsync(telegramId, bodyFat, ct);
        return $"✅ Процент жира обновлен: {bodyFat}%";
    }

    /// <summary>
    /// Изменить опыт
    /// </summary>
    public async Task<string> UpdateExperienceAsync(long telegramId, string experience, CancellationToken ct = default)
    {
        await _userParametersService.UpdateExperienceAsync(telegramId, experience, ct);
        var expText = FormatExperience(experience);
        return $"✅ Уровень опыта обновлен: {expText}";
    }

    /// <summary>
    /// Изменить цели
    /// </summary>
    public async Task<string> UpdateGoalsAsync(long telegramId, string goals, CancellationToken ct = default)
    {
        await _userParametersService.UpdateGoalsAsync(telegramId, goals, ct);
        var goalsText = FormatGoals(goals);
        return $"✅ Цели обновлены: {goalsText}";
    }

    private async Task<UserParameters> ApplyChangesAsync(long telegramId, List<ExtractedEntity> changes, CancellationToken cancellationToken)
    {
        var currentParams = await _userParametersService.GetUserParametersAsync(telegramId, cancellationToken);

        if (currentParams == null)
        {
            currentParams = await _userParametersService.CreateOrUpdateUserParametersAsync(
                telegramId,
                cancellationToken: cancellationToken);
        }

        foreach (var change in changes)
        {
            switch (change.Type.ToLower())
            {
                case "weight":
                case "вес":
                    if (decimal.TryParse(change.Value, out var weight))
                        await _userParametersService.UpdateWeightAsync(telegramId, weight, cancellationToken);
                    break;

                case "height":
                case "рост":
                    if (int.TryParse(change.Value, out var height))
                        await _userParametersService.UpdateHeightAsync(telegramId, height, cancellationToken);
                    break;

                case "bodyfat":
                case "body_fat":
                case "жир":
                    if (decimal.TryParse(change.Value, out var fat))
                        await _userParametersService.UpdateBodyFatAsync(telegramId, fat, cancellationToken);
                    break;

                case "experience":
                case "опыт":
                    await _userParametersService.UpdateExperienceAsync(telegramId, change.Value, cancellationToken);
                    break;

                case "goals":
                case "цели":
                case "цель":
                    await _userParametersService.UpdateGoalsAsync(telegramId, change.Value, cancellationToken);
                    break;
            }
        }

        var updated = await _userParametersService.GetUserParametersAsync(telegramId, cancellationToken);
        if (updated == null)
            throw new InvalidOperationException("Failed to get updated parameters");

        return updated;
    }

    private string FormatSuccessResponse(UserParameters updatedParams, List<ExtractedEntity> changes)
    {
        var response = "✅ Параметры обновлены:\n\n";

        foreach (var change in changes)
        {
            var displayName = change.Type.ToLower() switch
            {
                "weight" or "вес" => "Вес",
                "height" or "рост" => "Рост",
                "bodyfat" or "body_fat" or "жир" => "Процент жира",
                "experience" or "опыт" => "Опыт",
                "goals" or "цели" or "цель" => "Цели",
                _ => change.Type
            };

            var unit = change.Type.ToLower() switch
            {
                "weight" or "вес" => " кг",
                "height" or "рост" => " см",
                "bodyfat" or "body_fat" or "жир" => "%",
                _ => ""
            };

            response += $"• {displayName}: {change.Value}{unit}\n";
        }

        response += $"\n📊 Текущие параметры:\n";
        response += $"• Рост: {updatedParams.Height?.ToString() ?? "не указан"} см\n";
        response += $"• Вес: {updatedParams.Weight?.ToString() ?? "не указан"} кг\n";
        response += $"• Жир: {updatedParams.BodyFat?.ToString() ?? "не указан"}%\n";
        response += $"• Опыт: {FormatExperience(updatedParams.Experience)}\n";
        response += $"• Цели: {FormatGoals(updatedParams.Goals)}";

        return response;
    }

    private string FormatExperience(string? experience) => experience switch
    {
        "beginner" => "новичок",
        "intermediate" => "средний",
        "advanced" => "продвинутый",
        _ => experience ?? "не указан"
    };

    private string FormatGoals(string? goals) => goals switch
    {
        "lose_weight" => "похудение",
        "gain_muscle" => "набор массы",
        "maintain" => "поддержание формы",
        "endurance" => "повышение выносливости",
        "strength" => "увеличение силы",
        _ => goals ?? "не указаны"
    };

    private class ExtractedEntity
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
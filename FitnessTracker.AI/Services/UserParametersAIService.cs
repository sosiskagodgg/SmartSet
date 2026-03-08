// FitnessTracker.AI/PublicServices/UserParametersAIService.cs

using FitnessTracker.AI.Core.Base;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Directions;
using FitnessTracker.AI.Recognition.Classifiers;
using FitnessTracker.AI.Recognition.Recognizers;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
    /// Изменить параметры пользователя с автоматическим определением направления (полная классификация)
    /// </summary>
    public async Task<string> UpdateUserParametersFromMessageAsync(
        long telegramId,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();

        var classifier = scope.ServiceProvider.GetRequiredService<DirectionClassifier>();
        var recognizer = scope.ServiceProvider.GetRequiredService<GigaChatEntityRecognizer>();

        // 1. Определяем направление
        var direction = await classifier.ClassifyDirectionAsync(message);
        if (direction == null)
        {
            return "❌ Не удалось определить направление запроса";
        }

        // 2. Определяем поднаправление
        var subDirection = await classifier.ClassifySubDirectionAsync(message, direction);
        if (subDirection == null)
        {
            return $"❌ Не удалось определить поднаправление в {direction.Name}";
        }

        // 3. Проверяем, что это нужное нам поднаправление
        if (direction.Name != "profile" || subDirection.Name != "update_parameters")
        {
            return $"⏳ Это запрос на {direction.Name}/{subDirection.Name}, а я умею только обновлять параметры";
        }

        // 4. Извлекаем сущности с учетом контекста поднаправления
        var entitiesPrompt = BuildEntitiesPrompt(message, subDirection);
        var entitiesJson = await recognizer.AskAsync(entitiesPrompt, cancellationToken);

        // 5. Парсим и применяем сущности
        var changes = ParseEntitiesFromJson(entitiesJson);
        var updatedParams = await ApplyChangesAsync(telegramId, changes, cancellationToken);

        return FormatSuccessResponse(updatedParams, changes);
    }

    /// <summary>
    /// ✨ БЫСТРЫЙ МЕТОД: сразу обновить параметры, минуя классификацию
    /// Использует заранее известное поднаправление UpdateParametersSubDirection
    /// </summary>
    public async Task<string> UpdateParametersDirectAsync(
        long telegramId,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();

        var recognizer = scope.ServiceProvider.GetRequiredService<GigaChatEntityRecognizer>();

        // Создаем поднаправление напрямую (без классификации)
        var subDirection = new UpdateParametersSubDirection();

        _logger.LogInformation("🚀 Быстрое обновление параметров для пользователя {UserId}: {Message}",
            telegramId, message);

        // Строим промпт с описанием сущностей из поднаправления
        var entitiesPrompt = BuildEntitiesPrompt(message, subDirection);
        var entitiesJson = await recognizer.AskAsync(entitiesPrompt, cancellationToken);

        // Парсим и применяем изменения
        var changes = ParseEntitiesFromJson(entitiesJson);

        if (!changes.Any())
        {
            return "❌ Не удалось распознать параметры. Попробуйте: 'вес 85' или 'процент жира 15'";
        }

        var updatedParams = await ApplyChangesAsync(telegramId, changes, cancellationToken);

        return FormatSuccessResponse(updatedParams, changes);
    }

    /// <summary>
    /// Построение промпта для извлечения сущностей на основе поднаправления
    /// </summary>
    private string BuildEntitiesPrompt(string message, SubDirectionBase subDirection)
    {
        var entitiesDesc = string.Join("\n", subDirection.RequiredEntities.Select(e =>
            $"- {e.Type}: {e.DisplayName} {e.Unit} (примеры: {string.Join(", ", e.Examples)})"));

        return $@"
Извлеки параметры из сообщения для действия '{subDirection.Description}'.

Сообщение: {message}

Доступные параметры:
{entitiesDesc}

Верни JSON массив с type и value.
Пример: [{{""type"":""weight"",""value"":""85""}}, {{""type"":""bodyFat"",""value"":""15""}}]";
    }

    /// <summary>
    /// Парсинг JSON ответа от нейросети
    /// </summary>
    private List<ParameterChange> ParseEntitiesFromJson(string json)
    {
        var changes = new List<ParameterChange>();

        try
        {
            _logger.LogInformation("Парсим JSON от нейросети: {Json}", json);

            var entities = JsonSerializer.Deserialize<List<JsonEntity>>(json);
            if (entities == null)
            {
                _logger.LogWarning("Не удалось распарсить JSON как массив");
                return changes;
            }

            foreach (var entity in entities)
            {
                _logger.LogInformation("Найдена сущность: {Type} = {Value}", entity.type, entity.value);

                switch (entity.type.ToLower())
                {
                    case "weight":
                    case "вес":
                        if (decimal.TryParse(entity.value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var weight))
                            changes.Add(new ParameterChange("weight", weight, "Вес", "кг"));
                        else
                            _logger.LogWarning("Не удалось распарсить вес: {Value}", entity.value);
                        break;

                    case "height":
                    case "рост":
                        if (int.TryParse(entity.value, out var height))
                            changes.Add(new ParameterChange("height", height, "Рост", "см"));
                        else
                            _logger.LogWarning("Не удалось распарсить рост: {Value}", entity.value);
                        break;

                    case "bodyfat":
                    case "жир":
                    case "body_fat":
                        if (decimal.TryParse(entity.value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var fat))
                            changes.Add(new ParameterChange("bodyFat", fat, "Процент жира", "%"));
                        else
                            _logger.LogWarning("Не удалось распарсить процент жира: {Value}", entity.value);
                        break;

                    case "experience":
                    case "опыт":
                        changes.Add(new ParameterChange("experience", entity.value, "Опыт", ""));
                        break;

                    case "goals":
                    case "цели":
                    case "цель":
                        changes.Add(new ParameterChange("goals", entity.value, "Цели", ""));
                        break;

                    default:
                        _logger.LogWarning("Неизвестный тип сущности: {Type}", entity.type);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка парсинга JSON: {Json}", json);
        }

        _logger.LogInformation("Всего изменений: {Count}", changes.Count);
        return changes;
    }

    /// <summary>
    /// Применение изменений к параметрам пользователя
    /// </summary>
    private async Task<UserParameters> ApplyChangesAsync(
        long telegramId,
        List<ParameterChange> changes,
        CancellationToken cancellationToken)
    {
        var currentParams = await _userParametersService.GetUserParametersAsync(telegramId, cancellationToken);

        if (currentParams == null)
        {
            _logger.LogInformation("Creating new parameters for user {UserId}", telegramId);
            currentParams = await _userParametersService.CreateOrUpdateUserParametersAsync(
                telegramId,
                height: null,
                weight: null,
                bodyFat: null,
                experience: null,
                goals: null,
                ct: cancellationToken);
        }

        _logger.LogInformation("Before update - Height: {Height}, Weight: {Weight}, BodyFat: {BodyFat}",
            currentParams.Height, currentParams.Weight, currentParams.BodyFat);

        foreach (var change in changes)
        {
            switch (change.Type)
            {
                case "height":
                    if (change.IntValue.HasValue)
                    {
                        await _userParametersService.UpdateHeightAsync(telegramId, change.IntValue.Value, cancellationToken);
                        _logger.LogInformation("Updated height to: {Height}", change.IntValue.Value);
                    }
                    break;

                case "weight":
                    if (change.DecimalValue.HasValue)
                    {
                        await _userParametersService.UpdateWeightAsync(telegramId, change.DecimalValue.Value, cancellationToken);
                        _logger.LogInformation("Updated weight to: {Weight}", change.DecimalValue.Value);
                    }
                    break;

                case "bodyFat":
                    if (change.DecimalValue.HasValue)
                    {
                        await _userParametersService.UpdateBodyFatAsync(telegramId, change.DecimalValue.Value, cancellationToken);
                        _logger.LogInformation("Updated body fat to: {BodyFat}", change.DecimalValue.Value);
                    }
                    break;

                case "experience":
                    if (!string.IsNullOrEmpty(change.StringValue))
                    {
                        await _userParametersService.UpdateExperienceAsync(telegramId, change.StringValue, cancellationToken);
                        _logger.LogInformation("Updated experience to: {Experience}", change.StringValue);
                    }
                    break;

                case "goals":
                    if (!string.IsNullOrEmpty(change.StringValue))
                    {
                        await _userParametersService.UpdateGoalsAsync(telegramId, change.StringValue, cancellationToken);
                        _logger.LogInformation("Updated goals to: {Goals}", change.StringValue);
                    }
                    break;
            }
        }

        var updated = await _userParametersService.GetUserParametersAsync(telegramId, cancellationToken);

        _logger.LogInformation("After update - Height: {Height}, Weight: {Weight}, BodyFat: {BodyFat}",
            updated?.Height, updated?.Weight, updated?.BodyFat);

        if (updated == null)
        {
            throw new InvalidOperationException("Failed to get updated parameters");
        }

        return updated;
    }

    /// <summary>
    /// Форматирование успешного ответа
    /// </summary>
    private string FormatSuccessResponse(UserParameters updatedParams, List<ParameterChange> changes)
    {
        var response = "✅ Параметры обновлены:\n\n";

        foreach (var change in changes)
        {
            if (change.Type == "height" && change.IntValue.HasValue)
            {
                response += $"• {change.DisplayName}: {change.IntValue} {change.Unit}\n";
            }
            else if ((change.Type == "weight" || change.Type == "bodyFat") && change.DecimalValue.HasValue)
            {
                response += $"• {change.DisplayName}: {change.DecimalValue} {change.Unit}\n";
            }
            else if (!string.IsNullOrEmpty(change.StringValue))
            {
                var valueText = change.Type switch
                {
                    "experience" => change.StringValue switch
                    {
                        "beginner" => "новичок",
                        "intermediate" => "средний",
                        "advanced" => "продвинутый",
                        _ => change.StringValue
                    },
                    "goals" => change.StringValue switch
                    {
                        "lose_weight" => "похудение",
                        "gain_muscle" => "набор массы",
                        "maintain" => "поддержание формы",
                        "endurance" => "выносливость",
                        "strength" => "сила",
                        _ => change.StringValue
                    },
                    _ => change.StringValue
                };

                response += $"• {change.DisplayName}: {valueText}\n";
            }
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

    // ===== Прямые методы для изменения конкретных параметров =====
    public async Task<string> UpdateHeightAsync(long telegramId, int height, CancellationToken ct = default)
    {
        await _userParametersService.UpdateHeightAsync(telegramId, height, ct);
        return $"✅ Рост обновлен: {height} см";
    }

    public async Task<string> UpdateWeightAsync(long telegramId, decimal weight, CancellationToken ct = default)
    {
        await _userParametersService.UpdateWeightAsync(telegramId, weight, ct);
        return $"✅ Вес обновлен: {weight} кг";
    }

    public async Task<string> UpdateBodyFatAsync(long telegramId, decimal bodyFat, CancellationToken ct = default)
    {
        await _userParametersService.UpdateBodyFatAsync(telegramId, bodyFat, ct);
        return $"✅ Процент жира обновлен: {bodyFat}%";
    }

    public async Task<string> UpdateExperienceAsync(long telegramId, string experience, CancellationToken ct = default)
    {
        await _userParametersService.UpdateExperienceAsync(telegramId, experience, ct);
        var expText = FormatExperience(experience);
        return $"✅ Уровень опыта обновлен: {expText}";
    }

    public async Task<string> UpdateGoalsAsync(long telegramId, string goals, CancellationToken ct = default)
    {
        await _userParametersService.UpdateGoalsAsync(telegramId, goals, ct);
        var goalsText = FormatGoals(goals);
        return $"✅ Цели обновлены: {goalsText}";
    }

    // Вспомогательный класс для JSON
    private class JsonEntity
    {
        public string type { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Внутренний класс для хранения изменений
    /// </summary>
    private class ParameterChange
    {
        public string Type { get; set; }
        public int? IntValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public string? StringValue { get; set; }
        public string DisplayName { get; set; }
        public string Unit { get; set; }

        public ParameterChange(string type, int value, string displayName, string unit)
        {
            Type = type;
            IntValue = value;
            DisplayName = displayName;
            Unit = unit;
        }

        public ParameterChange(string type, decimal value, string displayName, string unit)
        {
            Type = type;
            DecimalValue = value;
            DisplayName = displayName;
            Unit = unit;
        }

        public ParameterChange(string type, string value, string displayName, string unit)
        {
            Type = type;
            StringValue = value;
            DisplayName = displayName;
            Unit = unit;
        }
    }
}
// FitnessTracker.AI/Commands/User/SetUserParameterCommand.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Commands.Base;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
namespace FitnessTracker.AI.Commands.User;

public class SetUserParameterCommand : BaseCommand
{
    private readonly IUserParameterService _parameterService;
    private readonly ILogger<SetUserParameterCommand> _logger;

    public override string Name => "SetUserParameter";
    public override string Description => "Сохраняет ваши параметры (вес, рост, пол, активность, опыт, цели)";
    public override string Category => "User";
    public override string Group => "Profile";
    public override double ConfidenceThreshold => 0.6;

    // Одна сущность - команда поняла что это запрос на изменение параметров
    public override List<EntityDefinition> RequiredEntities { get; } = new();

    // Только ключевые фразы для распознавания намерения
    public override List<string> TrainingPhrases { get; } = new()
    {
        "сохрани параметры",
        "мои параметры",
        "измени параметры",
        "обнови параметры",
        "запиши мои данные",
        "set my parameters",
        "update my info"
    };

    public SetUserParameterCommand(
        IUserParameterService parameterService,
        ILogger<SetUserParameterCommand> logger) : base(logger)
    {
        _parameterService = parameterService;
        _logger = logger;
    }

    public override async Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        LogExecution(context);

        try
        {
            var message = context.OriginalMessage.ToLowerInvariant();

            // ПОЛУЧАЕМ ТЕКУЩИЕ ПАРАМЕТРЫ
            var currentParams = await _parameterService.GetCurrentAsync(context.UserId);

            // СОЗДАЕМ НОВУЮ ЗАПИСЬ (для истории)
            var newParams = new Domain.Entities.UserParameter
            {
                IsCurrent = true // Новая запись будет текущей
            };

            // КОПИРУЕМ СУЩЕСТВУЮЩИЕ ПАРАМЕТРЫ ЕСЛИ ОНИ ЕСТЬ
            if (currentParams != null)
            {
                newParams.WeightKg = currentParams.WeightKg;
                newParams.HeightCm = currentParams.HeightCm;
                newParams.BirthDate = currentParams.BirthDate; // Добавить!
                newParams.Gender = currentParams.Gender;
                newParams.ActivityLevel = currentParams.ActivityLevel;
                newParams.ExperienceLevel = currentParams.ExperienceLevel;
                newParams.FitnessGoals = currentParams.FitnessGoals?.ToArray();
                newParams.Notes = currentParams.Notes;
            }

            var updated = false;
            var changes = new List<string>();

            // Парсим сообщение - ищем все возможные параметры
            updated |= TryParseWeight(message, newParams, changes);
            updated |= TryParseHeight(message, newParams, changes);
            updated |= TryParseGender(message, newParams, changes);
            updated |= TryParseActivity(message, newParams, changes);
            updated |= TryParseExperience(message, newParams, changes);
            updated |= TryParseGoals(message, newParams, changes);
            updated |= TryParseBirthDate(message, newParams, changes); // ДОБАВИТЬ!

            if (!updated)
            {
                return Success(GetHelpMessage());
            }

            // Сохраняем - создаст НОВУЮ запись, старая автоматически станет is_current = false
            if (newParams.BirthDate.HasValue && newParams.BirthDate.Value.Kind != DateTimeKind.Utc)
            {
                newParams.BirthDate = DateTime.SpecifyKind(newParams.BirthDate.Value, DateTimeKind.Utc);
            }

            var saved = await _parameterService.AddOrUpdateAsync(context.UserId, newParams);

            if (saved == null)
            {
                return Error("Не удалось сохранить параметры");
            }

            var response = $"✅ Обновлено:\n" + string.Join("\n", changes.Select(c => $"• {c}"));
            return Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user parameter");
            return Error("Ошибка при сохранении параметра");
        }
    }

    // ДОБАВИТЬ ЭТОТ МЕТОД
    private bool TryParseBirthDate(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        // Ищем дату в формате ДД.ММ.ГГГГ или ДД/ММ/ГГГГ
        var datePatterns = new[]
        {
        @"(\d{2})[.\-](\d{2})[.\-](\d{4})", // 05.10.2008 или 05-10-2008
        @"(\d{4})[.\-](\d{2})[.\-](\d{2})"  // 2008.10.05 или 2008-10-05
    };

        foreach (var pattern in datePatterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, pattern);
            if (match.Success && DateTime.TryParse(match.Value, out var birthDate))
            {
                // КОНВЕРТИРУЕМ В UTC
                parameters.BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);
                changes.Add($"Дата рождения: {birthDate:dd.MM.yyyy}");
                return true;
            }
        }

        // Ищем возраст "N лет"
        var ageMatch = System.Text.RegularExpressions.Regex.Match(message, @"\b(\d+)\s*(?:лет|год|года)\b");
        if (ageMatch.Success && int.TryParse(ageMatch.Groups[1].Value, out var age) && age > 0 && age < 150)
        {
            var birthDate = DateTime.Today.AddYears(-age);
            // КОНВЕРТИРУЕМ В UTC
            parameters.BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);
            changes.Add($"Возраст: {age} лет");
            return true;
        }

        return false;
    }
    private bool TryParseWeight(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        // Ищем паттерны типа: вес 75, weight 75, 75kg, 75 кг
        var patterns = new[]
        {
            @"вес[:\s]*(\d+(?:[.,]\d+)?)",
            @"weight[:\s]*(\d+(?:[.,]\d+)?)",
            @"(\d+(?:[.,]\d+)?)\s*кг",
            @"(\d+(?:[.,]\d+)?)\s*kg"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, pattern);
            if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace('.', ','), out var weight))
            {
                parameters.WeightKg = weight;
                changes.Add($"Вес: {weight} кг");
                return true;
            }
        }
        return false;
    }

    private bool TryParseHeight(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        var patterns = new[]
        {
            @"рост[:\s]*(\d+(?:[.,]\d+)?)",
            @"height[:\s]*(\d+(?:[.,]\d+)?)",
            @"(\d+(?:[.,]\d+)?)\s*см",
            @"(\d+(?:[.,]\d+)?)\s*cm"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, pattern);
            if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace('.', ','), out var height))
            {
                parameters.HeightCm = height;
                changes.Add($"Рост: {height} см");
                return true;
            }
        }
        return false;
    }

    private bool TryParseGender(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        // Ищем ТОЛЬКО явные указания пола
        var genderPatterns = new (string pattern, string value, string display)[]
        {
        (@"\b(?:пол|gender)[:\s]*(муж|male|м)\b", "male", "мужской"),
        (@"\b(?:пол|gender)[:\s]*(жен|female|ж)\b", "female", "женский"),
        (@"\bмужской\b", "male", "мужской"),
        (@"\bженский\b", "female", "женский"),
        (@"\bmale\b", "male", "мужской"),
        (@"\bfemale\b", "female", "женский"),
        (@"\bмуж\b", "male", "мужской"),
        (@"\bжен\b", "female", "женский")
        };

        foreach (var (pattern, value, display) in genderPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(message, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                parameters.Gender = value;
                changes.Add($"Пол: {display}");
                return true;
            }
        }

        return false;
    }

    private bool TryParseActivity(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        var activityMap = new Dictionary<string, (string value, string display)>
        {
            { "сидяч", ("sedentary", "сидячий") },
            { "sedentary", ("sedentary", "сидячий") },
            { "легк", ("light", "легкий") },
            { "light", ("light", "легкий") },
            { "умерен", ("moderate", "умеренный") },
            { "moderate", ("moderate", "умеренный") },
            { "высок", ("very", "высокий") },
            { "very", ("very", "высокий") },
            { "экстра", ("extra", "экстра") },
            { "extra", ("extra", "экстра") }
        };

        foreach (var kv in activityMap)
        {
            if (message.Contains(kv.Key))
            {
                parameters.ActivityLevel = kv.Value.value;
                changes.Add($"Активность: {kv.Value.display}");
                return true;
            }
        }
        return false;
    }

    private bool TryParseExperience(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        var expMap = new Dictionary<string, (string value, string display)>
        {
            { "нович", ("beginner", "новичок") },
            { "beginner", ("beginner", "новичок") },
            { "средн", ("intermediate", "средний") },
            { "intermediate", ("intermediate", "средний") },
            { "продвин", ("advanced", "продвинутый") },
            { "advanced", ("advanced", "продвинутый") }
        };

        foreach (var kv in expMap)
        {
            if (message.Contains(kv.Key))
            {
                parameters.ExperienceLevel = kv.Value.value;
                changes.Add($"Опыт: {kv.Value.display}");
                return true;
            }
        }
        return false;
    }

    private bool TryParseGoals(string message, Domain.Entities.UserParameter parameters, List<string> changes)
    {
        var goals = new List<string>();
        var goalMap = new Dictionary<string, (string value, string display)>
    {
        { "похуде", ("lose_weight", "похудение") },
        { "lose", ("lose_weight", "похудение") },
        { "weight loss", ("lose_weight", "похудение") },
        { "масс", ("build_muscle", "набор массы") },
        { "muscle", ("build_muscle", "набор массы") },
        { "выносл", ("endurance", "выносливость") },
        { "endurance", ("endurance", "выносливость") },
        { "сил", ("strength", "сила") },
        { "strength", ("strength", "сила") }
    };

        // Если есть существующие цели - копируем их
        if (parameters.FitnessGoals != null)
        {
            goals.AddRange(parameters.FitnessGoals);
        }

        var found = false;
        foreach (var kv in goalMap)
        {
            if (message.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
            {
                if (!goals.Contains(kv.Value.value))
                {
                    goals.Add(kv.Value.value);
                    changes.Add($"Цель: {kv.Value.display}");
                    found = true;
                }
            }
        }

        if (found)
        {
            parameters.FitnessGoals = goals.ToArray();
            return true;
        }

        return false;
    }

    private string GetHelpMessage()
    {
        return @"📝 **Как заполнить параметры:**

Напишите что хотите изменить, например:

⚖️ **Вес:** `вес 75` или `75кг`
📏 **Рост:** `рост 180` или `180см`
👤 **Пол:** `пол мужской` или `пол female`
📊 **Активность:** `активность умеренная` или `activity light`
💪 **Опыт:** `опыт новичок` или `experience advanced`
🎯 **Цели:** `цель похудение, сила`

Можно несколько сразу:
`вес 75, рост 180, пол мужской, цель похудение`";
    }
}
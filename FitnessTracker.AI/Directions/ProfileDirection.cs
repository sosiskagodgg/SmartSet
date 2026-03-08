// FitnessTracker.AI/Directions/ProfileDirection.cs

using FitnessTracker.AI.Core.Base;
using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Directions;

/// <summary>
/// Направление: Управление профилем
/// </summary>
public class ProfileDirection : DirectionBase
{
    public override string Name => "profile";
    public override string Description => "Управление профилем и параметрами пользователя";
    
    public override List<string> TrainingPhrases => new()
    {
        "профиль", "мои данные", "параметры", "личная информация",
        "profile", "my data", "settings", "parameters",
        "изменить вес", "обновить рост", "мой вес", "мой рост",
        "вес", "рост", "жир", "процент жира"
    };

    public override List<SubDirectionBase> SubDirections => new()
    {
        new UpdateParametersSubDirection()
    };
}

/// <summary>
/// Поднаправление: Обновление параметров
/// </summary>
public class UpdateParametersSubDirection : SubDirectionBase
{
    public override string Name => "update_parameters";
    public override string Description => "Изменение веса, роста и других параметров";

    public override List<string> TrainingPhrases => new()
    {
        "изменить вес", "обновить рост", "новый вес", "процент жира",
        "change weight", "update height", "new weight", "body fat",
        "мой вес", "мой рост", "мои параметры", "опыт", "цели"
    };

    public override List<EntityDefinition> RequiredEntities => new()
    {
        new EntityDefinition
        {
            Type = "weight",
            DisplayName = "Вес",
            Unit = "кг",
            Examples = new List<string> { "85", "75.5", "90.2" }
        },
        new EntityDefinition
        {
            Type = "height",
            DisplayName = "Рост",
            Unit = "см",
            Examples = new List<string> { "180", "175", "190" }
        },
        new EntityDefinition
        {
            Type = "bodyFat",
            DisplayName = "Процент жира",
            Unit = "%",
            Examples = new List<string> { "15", "12.5", "20" }
        },
        new EntityDefinition  // ← ДОБАВЛЯЕМ
        {
            Type = "experience",
            DisplayName = "Опыт",
            Examples = new List<string> { "beginner", "новичок", "intermediate", "средний", "advanced", "профи", "продвинутый" }
        },
        new EntityDefinition  // ← ДОБАВЛЯЕМ
        {
            Type = "goals",
            DisplayName = "Цели",
            Examples = new List<string> { "lose_weight", "похудеть", "gain_muscle", "набрать массу", "maintain", "поддержание" }
        }
    };
}
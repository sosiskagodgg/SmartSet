// FitnessTracker.AI.Core/Base/DirectionBase.cs

using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Base;

/// <summary>
/// Базовый класс для всех направлений
/// Просто наследуйся и заполняй свойства
/// </summary>
public abstract class DirectionBase
{
    /// <summary>
    /// Название направления (уникальный ID)
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Описание для чего это направление
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Фразы для определения направления (нейросеть их использует)
    /// </summary>
    public abstract List<string> TrainingPhrases { get; }

    /// <summary>
    /// Поднаправления (intents) внутри этого направления
    /// </summary>
    public virtual List<SubDirectionBase> SubDirections => new();
}

/// <summary>
/// Базовый класс для поднаправлений
/// </summary>
public abstract class SubDirectionBase
{
    /// <summary>
    /// Название поднаправления
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Описание
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Фразы для определения поднаправления
    /// </summary>
    public abstract List<string> TrainingPhrases { get; }

    /// <summary>
    /// Сущности, которые нужно извлекать для этого поднаправления
    /// </summary>
    public virtual List<EntityDefinition> RequiredEntities => new();
}
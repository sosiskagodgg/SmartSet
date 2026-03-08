// FitnessTracker.AI.Core/Models/EntityDefinition.cs

namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Описание сущности для извлечения
/// </summary>
public class EntityDefinition
{
    /// <summary>
    /// Тип сущности (weight, height, bodyFat)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Человеко-читаемое название
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Единица измерения
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Примеры значений (для обучения нейросети)
    /// </summary>
    public List<string> Examples { get; set; } = new();

    /// <summary>
    /// Обязательная ли сущность
    /// </summary>
    public bool IsRequired { get; set; } = false;
}
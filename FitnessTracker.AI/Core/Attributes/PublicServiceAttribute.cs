// FitnessTracker.AI.Core/Attributes/PublicServiceAttribute.cs

namespace FitnessTracker.AI.Core.Attributes;

/// <summary>
/// Помечай этим атрибутом свои публичные сервисы
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PublicServiceAttribute : Attribute
{
    /// <summary>
    /// Имя сервиса для вызова
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание что делает сервис
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
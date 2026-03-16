// FitnessTracker.AI/Core/Interfaces/IFitnessPlugin.cs
using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Interfaces;

/// <summary>
/// Плагин (команда) для обработки специфических запросов без участия AI
/// </summary>
public interface IFitnessPlugin
{
    /// <summary>
    /// Уникальный идентификатор плагина
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Отображаемое имя
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Описание того, что делает плагин
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Может ли этот плагин обработать данное сообщение
    /// </summary>
    Task<bool> CanHandleAsync(AiMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполнить плагин
    /// </summary>
    Task<PluginResult> ExecuteAsync(AiMessageContext context, CancellationToken cancellationToken = default);
}
// FitnessTracker.AI/Core/Registry/IPluginRegistry.cs
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;  // Добавляем using

namespace FitnessTracker.AI.Core.Registry;

/// <summary>
/// Реестр всех доступных плагинов
/// </summary>
public interface IPluginRegistry
{
    /// <summary>
    /// Зарегистрировать плагин
    /// </summary>
    void Register(IFitnessPlugin plugin);

    /// <summary>
    /// Получить плагин по ID
    /// </summary>
    IFitnessPlugin? Get(string id);

    /// <summary>
    /// Получить все плагины
    /// </summary>
    IEnumerable<IFitnessPlugin> GetAll();

    /// <summary>
    /// Найти плагин, который может обработать сообщение
    /// </summary>
    Task<IFitnessPlugin?> FindForMessageAsync(AiMessageContext context, CancellationToken cancellationToken = default);
}
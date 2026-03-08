// FitnessTracker.AI.Core/Registry/DirectionRegistry.cs

using System.Reflection;
using FitnessTracker.AI.Core.Base;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.AI.Core.Registry;

/// <summary>
/// Реестр, который сам находит все направления в сборках
/// </summary>
public class DirectionRegistry
{
    private readonly List<DirectionBase> _directions = new();
    private readonly ILogger<DirectionRegistry> _logger;

    public DirectionRegistry(ILogger<DirectionRegistry> logger)
    {
        _logger = logger;
        LoadDirectionsFromAssemblies();
    }

    private void LoadDirectionsFromAssemblies()
    {
        var directionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DirectionBase)))
            .ToList();

        _logger.LogInformation("Найдено направлений: {Count}", directionTypes.Count);

        foreach (var type in directionTypes)
        {
            try
            {
                var direction = Activator.CreateInstance(type) as DirectionBase;
                if (direction != null)
                {
                    _directions.Add(direction);
                    _logger.LogInformation("  ✅ Загружено направление: {Name} ({Description})",
                        direction.Name, direction.Description);

                    foreach (var sub in direction.SubDirections)
                    {
                        _logger.LogInformation("    ↳ Поднаправление: {Name} (сущностей: {Count})",
                            sub.Name, sub.RequiredEntities.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки направления {Type}", type.Name);
            }
        }
    }

    /// <summary>
    /// Получить все направления
    /// </summary>
    public List<DirectionBase> GetAllDirections() => _directions;

    /// <summary>
    /// Получить направление по имени
    /// </summary>
    public DirectionBase? GetDirection(string name)
        => _directions.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Получить поднаправление по имени направления и поднаправления
    /// </summary>
    public SubDirectionBase? GetSubDirection(string directionName, string subDirectionName)
    {
        var direction = GetDirection(directionName);
        return direction?.SubDirections
            .FirstOrDefault(s => s.Name.Equals(subDirectionName, StringComparison.OrdinalIgnoreCase));
    }
}
// FitnessTracker.AI/Registry/PluginRegistry.cs
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;  // Важно: используем AiMessageContext
using FitnessTracker.AI.Core.Registry;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.AI.Registry;

public class PluginRegistry : IPluginRegistry
{
    private readonly Dictionary<string, IFitnessPlugin> _plugins = new();
    private readonly ILogger<PluginRegistry> _logger;

    public PluginRegistry(ILogger<PluginRegistry> logger)
    {
        _logger = logger;
    }

    public void Register(IFitnessPlugin plugin)
    {
        if (_plugins.ContainsKey(plugin.Id))
        {
            _logger.LogWarning("Plugin with ID {PluginId} already registered", plugin.Id);
            return;
        }

        _plugins[plugin.Id] = plugin;
        _logger.LogInformation("Registered plugin: {PluginId}", plugin.Id);
    }

    public IFitnessPlugin? Get(string id)
    {
        return _plugins.TryGetValue(id, out var plugin) ? plugin : null;
    }

    public IEnumerable<IFitnessPlugin> GetAll()
    {
        return _plugins.Values;
    }

    public async Task<IFitnessPlugin?> FindForMessageAsync(AiMessageContext context, CancellationToken cancellationToken = default)
    {
        foreach (var plugin in _plugins.Values)
        {
            try
            {
                if (await plugin.CanHandleAsync(context, cancellationToken))
                {
                    _logger.LogDebug("Plugin {PluginId} can handle message", plugin.Id);
                    return plugin;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking plugin {PluginId}", plugin.Id);
            }
        }

        return null;
    }
}
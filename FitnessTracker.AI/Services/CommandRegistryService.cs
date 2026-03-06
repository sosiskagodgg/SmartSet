using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Services;

public class CommandRegistryService : ICommandRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRegistryService> _logger;
    private readonly List<Type> _commandTypes;
    private List<ICommand>? _cachedCommands;

    public CommandRegistryService(
        IServiceProvider serviceProvider,
        ILogger<CommandRegistryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _commandTypes = new List<Type>();
    }
    public void RegisterCommand(Type commandType)
    {
        if (!_commandTypes.Contains(commandType))
        {
            _commandTypes.Add(commandType);
            _cachedCommands = null;
            _logger.LogInformation("Registered command: {CommandName}", commandType.Name);
        }
    }
    public IEnumerable<ICommand> GetCommandsByGroup(string group)
    {
        return GetAllCommands().Where(c => c.Group.Equals(group, StringComparison.OrdinalIgnoreCase));
    }
    public void Register<T>() where T : class, ICommand
    {
        var type = typeof(T);
        if (!_commandTypes.Contains(type))
        {
            _commandTypes.Add(type);
            _cachedCommands = null; // Сбрасываем кэш
            _logger.LogInformation("Registered command: {CommandName}", type.Name);
        }
    }

    public void RegisterAllFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t));

        foreach (var type in types)
        {
            if (!_commandTypes.Contains(type))
            {
                _commandTypes.Add(type);
            }
        }

        _cachedCommands = null;
        _logger.LogInformation("Registered {Count} commands from assembly {Assembly}",
            types.Count(), assembly.GetName().Name);
    }

    public IEnumerable<ICommand> GetAllCommands()
    {
        if (_cachedCommands != null)
            return _cachedCommands;

        var commands = new List<ICommand>();

        foreach (var commandType in _commandTypes)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var command = scope.ServiceProvider.GetService(commandType) as ICommand;
                if (command != null)
                {
                    commands.Add(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating command {CommandType}", commandType.Name);
            }
        }

        _cachedCommands = commands;
        return commands;
    }

    public async Task<ICommand?> FindBestMatchAsync(string message, CancellationToken cancellationToken = default)
    {
        var commands = GetAllCommands().ToList();

        if (!commands.Any())
        {
            return null;
        }

        var bestMatch = (ICommand?)null;
        var bestConfidence = 0.0;

        foreach (var command in commands)
        {
            if (await command.CanHandleAsync(message, cancellationToken))
            {
                // TODO: Добавить более сложную логику ранжирования
                // Сейчас просто берем первую подошедшую команду
                bestMatch = command;
                break;
            }
        }

        return bestMatch;
    }

    public ICommand? GetCommandByName(string name)
    {
        return GetAllCommands().FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

}
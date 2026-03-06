using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Interfaces;

public interface ICommandRegistry
{
    void Register<T>() where T : class, ICommand;
    void RegisterAllFromAssembly(System.Reflection.Assembly assembly);
    IEnumerable<ICommand> GetAllCommands();
    Task<ICommand?> FindBestMatchAsync(string message, CancellationToken cancellationToken = default);
    IEnumerable<ICommand> GetCommandsByGroup(string group); // НОВЫЙ
    ICommand? GetCommandByName(string name);
    void RegisterCommand(Type commandType);
}
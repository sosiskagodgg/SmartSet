using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Interfaces;

public interface IIntentClassifier
{
    Task<Intent?> ClassifyAsync(string message, CancellationToken cancellationToken = default);
    Task<Intent?> ClassifyAsync(string message, List<ICommand> commands, CancellationToken cancellationToken = default); // НОВЫЙ
}
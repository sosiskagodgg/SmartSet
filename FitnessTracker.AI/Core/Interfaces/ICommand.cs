using FitnessTracker.AI.Core.Models;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Category { get; } // Это уже есть - переименуем или оставим?
    string Group { get; } // НОВОЕ: Workout, Profile, Stats, System
    double ConfidenceThreshold { get; }

    List<string> TrainingPhrases { get; }
    List<EntityDefinition> RequiredEntities { get; }

    Task<bool> CanHandleAsync(string message, CancellationToken cancellationToken = default);
    Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(CommandContext context, CancellationToken cancellationToken = default);

    Dictionary<string, object> Metadata { get; }
}
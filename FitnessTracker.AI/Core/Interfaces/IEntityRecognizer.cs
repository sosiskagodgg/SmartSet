using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Interfaces;

public interface IEntityRecognizer
{
    int Priority { get; } // Для порядка выполнения
    Task<List<Entity>> RecognizeAsync(string message, Intent? intent = null, CancellationToken cancellationToken = default);
}
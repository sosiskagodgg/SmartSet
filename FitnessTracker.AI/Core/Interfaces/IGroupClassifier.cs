namespace FitnessTracker.AI.Core.Interfaces;

public interface IGroupClassifier
{
    Task<string?> ClassifyGroupAsync(string message);
}
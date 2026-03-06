namespace FitnessTracker.AI.Core.Interfaces;

public interface IGigaChatTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
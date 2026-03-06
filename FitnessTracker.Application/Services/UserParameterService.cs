// FitnessTracker.Application/Services/UserParameterService.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class UserParameterService : IUserParameterService
{
    private readonly IUserParameterRepository _repository;
    private readonly ILogger<UserParameterService> _logger;

    public UserParameterService(
        IUserParameterRepository repository,
        ILogger<UserParameterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserParameter?> GetCurrentAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting current parameters for user {UserId}", userId);
            return await _repository.GetCurrentByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current parameters for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<UserParameter>> GetHistoryAsync(long userId, int limit = 10)
    {
        try
        {
            _logger.LogDebug("Getting parameters history for user {UserId}", userId);
            var parameters = await _repository.GetByUserIdAsync(userId, onlyCurrent: false);
            return parameters.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parameters history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserParameter?> AddOrUpdateAsync(long userId, UserParameter parameter)
    {
        try
        {
            _logger.LogInformation("Adding/updating parameters for user {UserId}", userId);

            parameter.UserId = userId;
            parameter.RecordedAt = DateTime.UtcNow;
            parameter.IsCurrent = true;

            var result = await _repository.AddAsync(parameter);
            return result ? parameter : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating parameters for user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            _logger.LogInformation("Deleting parameter {Id}", id);
            return await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting parameter {Id}", id);
            return false;
        }
    }
}
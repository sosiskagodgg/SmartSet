// FitnessTracker.Application/Services/UserParametersService.cs
using FitnessTracker.Application.Common.Exceptions;
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services;

/// <summary>
/// Сервис для работы с параметрами пользователя
/// </summary>
public class UserParametersService : IUserParametersService
{
    private readonly IUserParametersRepository _parametersRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserParametersService> _logger;

    public UserParametersService(
        IUserParametersRepository parametersRepository,
        IUserRepository userRepository,
        ILogger<UserParametersService> logger)
    {
        _parametersRepository = parametersRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserParameters?> GetUserParametersAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _parametersRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserParameters> CreateOrUpdateUserParametersAsync(
        long telegramId,
        int? height = null,
        decimal? weight = null,
        decimal? bodyFat = null,
        string? experience = null,
        string? goals = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating parameters for user {TelegramId}", telegramId);

        var userExists = await _userRepository.ExistsByTelegramIdAsync(telegramId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            throw new UserNotFoundException(telegramId);
        }

        var parameters = await _parametersRepository.GetByTelegramIdAsync(telegramId, cancellationToken);

        if (parameters == null)
        {
            parameters = UserParameters.Create(telegramId);
            ApplyUpdates(parameters, height, weight, bodyFat, experience, goals);
            await _parametersRepository.AddAsync(parameters, cancellationToken);
            _logger.LogInformation("Created new parameters for user {TelegramId}", telegramId);
        }
        else
        {
            ApplyUpdates(parameters, height, weight, bodyFat, experience, goals);
            await _parametersRepository.UpdateAsync(parameters, cancellationToken);
            _logger.LogInformation("Updated parameters for user {TelegramId}", telegramId);
        }

        return parameters;
    }

    /// <inheritdoc />
    public async Task UpdateHeightAsync(long telegramId, int height, CancellationToken cancellationToken = default)
    {
        await CreateOrUpdateUserParametersAsync(telegramId, height: height, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateWeightAsync(long telegramId, decimal weight, CancellationToken cancellationToken = default)
    {
        await CreateOrUpdateUserParametersAsync(telegramId, weight: weight, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateBodyFatAsync(long telegramId, decimal bodyFat, CancellationToken cancellationToken = default)
    {
        await CreateOrUpdateUserParametersAsync(telegramId, bodyFat: bodyFat, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateExperienceAsync(long telegramId, string experience, CancellationToken cancellationToken = default)
    {
        await CreateOrUpdateUserParametersAsync(telegramId, experience: experience, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateGoalsAsync(long telegramId, string goals, CancellationToken cancellationToken = default)
    {
        await CreateOrUpdateUserParametersAsync(telegramId, goals: goals, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteUserParametersAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting parameters for user {TelegramId}", telegramId);

        var parameters = await _parametersRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        if (parameters != null)
        {
            await _parametersRepository.DeleteAsync(parameters, cancellationToken);
            _logger.LogInformation("Deleted parameters for user {TelegramId}", telegramId);
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserParametersExistsAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _parametersRepository.ExistsAsync(telegramId, cancellationToken);
    }

    private static void ApplyUpdates(
        UserParameters parameters,
        int? height,
        decimal? weight,
        decimal? bodyFat,
        string? experience,
        string? goals)
    {
        parameters.UpdatePhysicalMetrics(height, weight, bodyFat);

        if (experience != null)
            parameters.UpdateExperience(experience);

        if (goals != null)
            parameters.UpdateGoals(goals);
    }
}
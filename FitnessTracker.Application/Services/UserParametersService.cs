// FitnessTracker.Application/Services/UserParametersService.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class UserParametersService : IUserParametersService
{
    private readonly IUserParametersRepository _userParametersRepository;

    public UserParametersService(IUserParametersRepository userParametersRepository)
    {
        _userParametersRepository = userParametersRepository;
    }

    public async Task<UserParameters?> GetUserParametersAsync(long telegramId, CancellationToken ct = default)
    {
        // TelegramId и есть Id в таблице
        return await _userParametersRepository.GetByIdAsync(telegramId, ct);
    }

    public async Task<UserParameters> CreateOrUpdateUserParametersAsync(
        long telegramId,
        int? height = null,
        decimal? weight = null,
        decimal? bodyFat = null,
        string? experience = null,
        string? goals = null,
        CancellationToken ct = default)
    {
        var existing = await GetUserParametersAsync(telegramId, ct);

        if (existing != null)
        {
            // Обновляем только те поля, которые переданы
            if (height.HasValue)
                existing.Height = height.Value;
            if (weight.HasValue)
                existing.Weight = weight.Value;
            if (bodyFat.HasValue)
                existing.BodyFat = bodyFat.Value;
            if (experience != null)
                existing.Experience = experience;
            if (goals != null)
                existing.Goals = goals;

            await _userParametersRepository.UpdateAsync(existing, ct);
            return existing;
        }

        // Создаем новые параметры
        var newParameters = new UserParameters
        {
            TelegramId = telegramId,
            Height = height,
            Weight = weight,
            BodyFat = bodyFat,
            Experience = experience,
            Goals = goals
        };

        await _userParametersRepository.AddAsync(newParameters, ct);
        return newParameters;
    }

    public async Task UpdateHeightAsync(long telegramId, int height, CancellationToken ct = default)
    {
        var parameters = await GetUserParametersAsync(telegramId, ct);
        if (parameters == null)
        {
            parameters = new UserParameters { TelegramId = telegramId, Height = height };
            await _userParametersRepository.AddAsync(parameters, ct);
        }
        else
        {
            parameters.Height = height;
            await _userParametersRepository.UpdateAsync(parameters, ct);
        }
    }

    public async Task UpdateWeightAsync(long telegramId, decimal weight, CancellationToken ct = default)
    {
        var parameters = await GetUserParametersAsync(telegramId, ct);
        if (parameters == null)
        {
            parameters = new UserParameters { TelegramId = telegramId, Weight = weight };
            await _userParametersRepository.AddAsync(parameters, ct);
        }
        else
        {
            parameters.Weight = weight;
            await _userParametersRepository.UpdateAsync(parameters, ct);
        }
    }

    public async Task UpdateBodyFatAsync(long telegramId, decimal bodyFat, CancellationToken ct = default)
    {
        var parameters = await GetUserParametersAsync(telegramId, ct);
        if (parameters == null)
        {
            parameters = new UserParameters { TelegramId = telegramId, BodyFat = bodyFat };
            await _userParametersRepository.AddAsync(parameters, ct);
        }
        else
        {
            parameters.BodyFat = bodyFat;
            await _userParametersRepository.UpdateAsync(parameters, ct);
        }
    }

    public async Task UpdateExperienceAsync(long telegramId, string experience, CancellationToken ct = default)
    {
        var parameters = await GetUserParametersAsync(telegramId, ct);
        if (parameters == null)
        {
            parameters = new UserParameters { TelegramId = telegramId, Experience = experience };
            await _userParametersRepository.AddAsync(parameters, ct);
        }
        else
        {
            parameters.Experience = experience;
            await _userParametersRepository.UpdateAsync(parameters, ct);
        }
    }

    public async Task UpdateGoalsAsync(long telegramId, string goals, CancellationToken ct = default)
    {
        var parameters = await GetUserParametersAsync(telegramId, ct);
        if (parameters == null)
        {
            parameters = new UserParameters { TelegramId = telegramId, Goals = goals };
            await _userParametersRepository.AddAsync(parameters, ct);
        }
        else
        {
            parameters.Goals = goals;
            await _userParametersRepository.UpdateAsync(parameters, ct);
        }
    }

    public async Task<bool> UserParametersExistsAsync(long telegramId, CancellationToken ct = default)
    {
        return await _userParametersRepository.ExistsAsync(telegramId, ct);
    }

    public async Task DeleteUserParametersAsync(long telegramId, CancellationToken ct = default)
    {
        var parameters = await GetUserParametersAsync(telegramId, ct);
        if (parameters != null)
        {
            await _userParametersRepository.DeleteAsync(parameters, ct);
        }
    }
}
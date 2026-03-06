using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.DTOs;

namespace FitnessTracker.Application.Services;

public class ProgramService : IProgramService
{
    private readonly IProgramTemplateRepository _programRepository;
    private readonly IProgramDayRepository _dayRepository;
    private readonly IExerciseLibraryRepository _exerciseRepository;
    private readonly ILogger<ProgramService> _logger;

    public ProgramService(
        IProgramTemplateRepository programRepository,
        IProgramDayRepository dayRepository,
        IExerciseLibraryRepository exerciseRepository,
        ILogger<ProgramService> logger)
    {
        _programRepository = programRepository;
        _dayRepository = dayRepository;
        _exerciseRepository = exerciseRepository;
        _logger = logger;
    }

    public async Task<ProgramTemplate?> CreateProgramAsync(long userId, string name, string? description, List<ProgramDayDto> days)
    {
        try
        {
            _logger.LogInformation("Creating program {Name} for user {UserId}", name, userId);

            // Проверяем, есть ли уже активная программа
            var activeProgram = await _programRepository.GetUserActiveProgramAsync(userId);
            if (activeProgram != null)
            {
                _logger.LogWarning("User {UserId} already has an active program {ProgramId}", userId, activeProgram.Id);
                return null; // Возвращаем null - программа не создана
            }

            var program = new ProgramTemplate
            {
                UserId = userId,
                Name = name,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var programResult = await _programRepository.AddAsync(program);
            if (!programResult) return null;

            foreach (var dayDto in days)
            {
                var day = new ProgramDay
                {
                    ProgramId = program.Id,
                    DayNumber = dayDto.DayNumber,
                    Name = dayDto.Name,
                    IsRestDay = dayDto.IsRestDay
                };

                var dayResult = await _dayRepository.AddAsync(day);
                if (!dayResult) continue;

                foreach (var exDto in dayDto.Exercises)
                {
                    var exercise = await _exerciseRepository.GetByIdAsync(exDto.ExerciseId);
                    if (exercise == null) continue;

                    var dayExercise = new ProgramDayExercise
                    {
                        ProgramDayId = day.Id,
                        ExerciseId = exDto.ExerciseId,
                        Order = exDto.Order,
                        TargetSets = exDto.TargetSets,
                        TargetRepsMin = exDto.TargetRepsMin,
                        TargetRepsMax = exDto.TargetRepsMax,
                        TargetWeight = exDto.TargetWeight
                    };

                    // TODO: добавить репозиторий для ProgramDayExercise
                }
            }

            return program;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating program for user {UserId}", userId);
            return null;
        }
    }

    public async Task<ProgramTemplate?> GetActiveProgramAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting active program for user {UserId}", userId);
            return await _programRepository.GetUserActiveProgramAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active program for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ProgramTemplate>> GetUserProgramsAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting programs for user {UserId}", userId);
            return await _programRepository.GetUserProgramsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting programs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ActivateProgramAsync(long programId)
    {
        try
        {
            _logger.LogInformation("Activating program {ProgramId}", programId);
            var program = await _programRepository.GetByIdAsync(programId);
            if (program == null) return false;

            // Деактивируем предыдущую активную программу
            var active = await _programRepository.GetUserActiveProgramAsync(program.UserId);
            if (active != null)
            {
                active.IsActive = false;
                await _programRepository.UpdateAsync(active);
            }

            program.IsActive = true;
            return await _programRepository.UpdateAsync(program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating program {ProgramId}", programId);
            return false;
        }
    }

    public async Task<ProgramDay?> GetTodayProgramAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting today's program for user {UserId}", userId);
            var program = await _programRepository.GetUserActiveProgramAsync(userId);
            if (program == null) return null;

            var dayNumber = (int)DateTime.UtcNow.DayOfWeek;
            if (dayNumber == 0) dayNumber = 7; // Воскресенье = 7

            return await _dayRepository.GetDayByNumberAsync(program.Id, dayNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's program for user {UserId}", userId);
            throw;
        }
    }

    // НОВЫЙ МЕТОД: GetProgramDayAsync
    public async Task<ProgramDay?> GetProgramDayAsync(long programId, int dayNumber)
    {
        try
        {
            _logger.LogDebug("Getting day {DayNumber} for program {ProgramId}", dayNumber, programId);
            return await _dayRepository.GetDayByNumberAsync(programId, dayNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting program day");
            throw;
        }
    }

    public async Task<bool> UpdateProgramAsync(ProgramTemplate program)
    {
        try
        {
            _logger.LogInformation("Updating program {ProgramId}", program.Id);
            return await _programRepository.UpdateAsync(program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating program {ProgramId}", program.Id);
            return false;
        }
    }

    public async Task<bool> DeleteProgramAsync(long programId)
    {
        try
        {
            _logger.LogInformation("Deleting program {ProgramId}", programId);
            return await _programRepository.DeleteAsync(programId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting program {ProgramId}", programId);
            return false;
        }
    }
}
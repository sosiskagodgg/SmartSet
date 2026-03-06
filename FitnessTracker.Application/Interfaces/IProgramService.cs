using FitnessTracker.Domain.Entities;
using FitnessTracker.Application.DTOs;
namespace FitnessTracker.Application.Interfaces;

public interface IProgramService
{
    Task<ProgramTemplate?> CreateProgramAsync(long userId, string name, string? description, List<ProgramDayDto> days);
    Task<ProgramTemplate?> GetActiveProgramAsync(long userId);
    Task<List<ProgramTemplate>> GetUserProgramsAsync(long userId);
    Task<bool> ActivateProgramAsync(long programId);
    Task<ProgramDay?> GetTodayProgramAsync(long userId);
    Task<ProgramDay?> GetProgramDayAsync(long programId, int dayNumber);
    Task<bool> UpdateProgramAsync(ProgramTemplate program);
    Task<bool> DeleteProgramAsync(long programId);
}
using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;

public interface IProgramDayRepository
{
    Task<ProgramDay?> GetByIdAsync(long id);
    Task<List<ProgramDay>> GetProgramDaysAsync(long programId);
    Task<ProgramDay?> GetDayByNumberAsync(long programId, int dayNumber);
    Task<bool> AddAsync(ProgramDay day);
    Task<bool> UpdateAsync(ProgramDay day);
    Task<bool> DeleteAsync(long id);
}
using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;

public interface IProgramTemplateRepository
{
    Task<ProgramTemplate?> GetByIdAsync(long id);
    Task<ProgramTemplate?> GetUserActiveProgramAsync(long userId);
    Task<List<ProgramTemplate>> GetUserProgramsAsync(long userId);
    Task<bool> AddAsync(ProgramTemplate program);
    Task<bool> UpdateAsync(ProgramTemplate program);
    Task<bool> DeleteAsync(long id);
}
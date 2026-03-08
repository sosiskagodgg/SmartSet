using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с ежедневными тренировками
    /// </summary>
    public interface IWorkoutRepository : IBaseRepository<Workout>
    {
    }
}
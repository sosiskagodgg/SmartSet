using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с тренировками пользователя (по дням)
    /// </summary>
    public interface IUserWorkoutRepository : IBaseRepository<UserWorkout>
    {
    }
}
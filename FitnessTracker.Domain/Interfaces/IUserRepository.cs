using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public interface IUserRepository : IBaseRepository<User>
    {
    }
}
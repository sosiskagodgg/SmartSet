// FitnessTracker.Domain/Interfaces/IUserParametersRepository.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с параметрами пользователя
    /// </summary>
    public interface IUserParametersRepository : IBaseRepository<UserParameters>
    {
        // Ничего лишнего, GetByIdAsync уже есть в базовом интерфейсе
        // TelegramId и так является Id (Primary Key)
    }
}
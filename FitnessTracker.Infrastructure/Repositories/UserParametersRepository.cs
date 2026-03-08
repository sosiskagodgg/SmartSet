// FitnessTracker.Infrastructure/Repositories/UserParametersRepository.cs
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с параметрами пользователя
/// </summary>
public class UserParametersRepository : BaseRepository<UserParameters>, IUserParametersRepository
{
    public UserParametersRepository(FitnessDbContext context) : base(context)
    {
    }

    // Специфичные методы можно добавить позже при необходимости
    // Например, получение истории изменений параметров, если будет отдельная таблица
}
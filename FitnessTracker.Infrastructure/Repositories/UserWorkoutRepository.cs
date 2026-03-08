using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с тренировками пользователя (по дням)
/// </summary>
public class UserWorkoutRepository : BaseRepository<UserWorkout>, IUserWorkoutRepository
{
    public UserWorkoutRepository(FitnessDbContext context) : base(context)
    {
    }

}
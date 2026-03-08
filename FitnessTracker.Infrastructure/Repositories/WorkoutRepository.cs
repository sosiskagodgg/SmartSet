using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с ежедневными тренировками
/// </summary>
public class WorkoutRepository : BaseRepository<Workout>, IWorkoutRepository
{
    public WorkoutRepository(FitnessDbContext context) : base(context)
    {
    }


}
// FitnessTracker.Infrastructure/Repositories/ProgramDayRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class ProgramDayRepository : IProgramDayRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<ProgramDayRepository> _logger;

    public ProgramDayRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<ProgramDayRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<ProgramDay?> GetByIdAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProgramDays
                .Include(d => d.Exercises)
                    .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting program day by id {Id}", id);
            throw;
        }
    }

    public async Task<List<ProgramDay>> GetProgramDaysAsync(long programId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProgramDays
                .Include(d => d.Exercises)
                    .ThenInclude(e => e.Exercise)
                .Where(d => d.ProgramId == programId)
                .OrderBy(d => d.DayNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting days for program {ProgramId}", programId);
            throw;
        }
    }

    public async Task<ProgramDay?> GetDayByNumberAsync(long programId, int dayNumber)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProgramDays
                .Include(d => d.Exercises)
                    .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(d => d.ProgramId == programId && d.DayNumber == dayNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting day {DayNumber} for program {ProgramId}", dayNumber, programId);
            throw;
        }
    }

    public async Task<bool> AddAsync(ProgramDay day)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.ProgramDays.AddAsync(day);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding day to program {ProgramId}", day.ProgramId);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(ProgramDay day)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.ProgramDays.Update(day);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating program day {Id}", day.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var day = await context.ProgramDays.FindAsync(id);
            if (day == null)
                return false;

            context.ProgramDays.Remove(day);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting program day {Id}", id);
            return false;
        }
    }
}
// FitnessTracker.Infrastructure/Repositories/ProgramTemplateRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class ProgramTemplateRepository : IProgramTemplateRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<ProgramTemplateRepository> _logger;

    public ProgramTemplateRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<ProgramTemplateRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<ProgramTemplate?> GetByIdAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProgramTemplates
                .Include(p => p.Days)
                    .ThenInclude(d => d.Exercises)
                        .ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting program by id {Id}", id);
            throw;
        }
    }

    public async Task<ProgramTemplate?> GetUserActiveProgramAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProgramTemplates
                .Include(p => p.Days)
                    .ThenInclude(d => d.Exercises)
                        .ThenInclude(e => e.Exercise)
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active program for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ProgramTemplate>> GetUserProgramsAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProgramTemplates
                .Include(p => p.Days)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting programs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddAsync(ProgramTemplate program)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            program.CreatedAt = DateTime.UtcNow;

            // Деактивируем предыдущую активную программу
            if (program.IsActive)
            {
                var activeProgram = await context.ProgramTemplates
                    .FirstOrDefaultAsync(p => p.UserId == program.UserId && p.IsActive);

                if (activeProgram != null)
                    activeProgram.IsActive = false;
            }

            await context.ProgramTemplates.AddAsync(program);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding program for user {UserId}", program.UserId);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(ProgramTemplate program)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.ProgramTemplates.Update(program);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating program {Id}", program.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var program = await context.ProgramTemplates.FindAsync(id);
            if (program == null)
                return false;

            context.ProgramTemplates.Remove(program);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting program {Id}", id);
            return false;
        }
    }
}
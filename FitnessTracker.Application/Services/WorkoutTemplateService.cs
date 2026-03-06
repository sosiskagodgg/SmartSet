using Microsoft.Extensions.Logging;
using System.Text.Json;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Services;

public class WorkoutTemplateService : IWorkoutTemplateService
{
    private readonly List<WorkoutTemplate> _templates;
    private readonly ILogger<WorkoutTemplateService> _logger;
    private readonly string _templatesPath;

    public WorkoutTemplateService(ILogger<WorkoutTemplateService> logger)
    {
        _logger = logger;
        _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        _templates = LoadTemplates();
    }

    private List<WorkoutTemplate> LoadTemplates()
    {
        var templates = new List<WorkoutTemplate>();

        try
        {
            if (!Directory.Exists(_templatesPath))
            {
                _logger.LogWarning("Templates directory not found: {Path}", _templatesPath);
                return templates;
            }

            var jsonFiles = Directory.GetFiles(_templatesPath, "*.json");
            _logger.LogInformation("Found {Count} template files", jsonFiles.Length);

            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var template = JsonSerializer.Deserialize<WorkoutTemplate>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (template != null)
                    {
                        templates.Add(template);
                        _logger.LogDebug("Loaded template: {Name} from {File}", template.Name, Path.GetFileName(file));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading template from file: {File}", file);
                }
            }

            _logger.LogInformation("Successfully loaded {Count} workout templates", templates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading templates from directory: {Path}", _templatesPath);
        }

        return templates;
    }

    public List<WorkoutTemplate> GetAllTemplates()
    {
        return _templates;
    }

    public WorkoutTemplate? GetTemplateById(string id)
    {
        return _templates.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public List<WorkoutTemplate> GetTemplatesByLevel(string level)
    {
        return _templates
            .Where(t => t.Level.Equals(level, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<WorkoutTemplate> GetTemplatesByGoal(string goal)
    {
        return _templates
            .Where(t => t.Goal.Equals(goal, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<WorkoutTemplate> GetTemplatesByLevelAndGoal(string? level, string? goal)
    {
        var query = _templates.AsEnumerable();

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(t => t.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(goal))
        {
            query = query.Where(t => t.Goal.Equals(goal, StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    public WorkoutTemplate? FindBestMatch(string? level, string? goal, int? daysPerWeek)
    {
        var candidates = _templates.AsEnumerable();

        if (!string.IsNullOrEmpty(level))
        {
            candidates = candidates.Where(t => t.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(goal))
        {
            candidates = candidates.Where(t => t.Goal.Equals(goal, StringComparison.OrdinalIgnoreCase));
        }

        if (daysPerWeek.HasValue)
        {
            // Ищем точное совпадение или ближайшее
            candidates = candidates
                .OrderBy(t => Math.Abs(t.DaysPerWeek - daysPerWeek.Value));
        }

        return candidates.FirstOrDefault();
    }

    public async Task ReloadTemplatesAsync()
    {
        _templates.Clear();
        _templates.AddRange(LoadTemplates());
        await Task.CompletedTask;
    }
}
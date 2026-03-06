using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Recognition.Recognizers;

public class RegexEntityRecognizer : IEntityRecognizer
{
    private readonly ILogger<RegexEntityRecognizer> _logger;
    private readonly Dictionary<string, List<RegexPattern>> _patterns;

    public int Priority => 50;

    public RegexEntityRecognizer(ILogger<RegexEntityRecognizer> logger)
    {
        _logger = logger;
        _patterns = new Dictionary<string, List<RegexPattern>>();
    }

    /// <summary>
    /// Регистрирует regex паттерн для определенного типа сущности
    /// </summary>
    public void RegisterPattern(string entityType, string pattern, double confidence = 0.9, string? description = null)
    {
        if (!_patterns.ContainsKey(entityType))
        {
            _patterns[entityType] = new List<RegexPattern>();
        }

        _patterns[entityType].Add(new RegexPattern
        {
            Pattern = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled),
            Confidence = confidence,
            Description = description ?? pattern
        });

        _logger.LogDebug("Registered regex pattern for {EntityType}: {Pattern}", entityType, pattern);
    }

    /// <summary>
    /// Регистрирует несколько паттернов для типа сущности
    /// </summary>
    public void RegisterPatterns(string entityType, IEnumerable<string> patterns, double confidence = 0.9)
    {
        foreach (var pattern in patterns)
        {
            RegisterPattern(entityType, pattern, confidence);
        }
    }

    public Task<List<Entity>> RecognizeAsync(string message, Intent? intent = null, CancellationToken cancellationToken = default)
    {
        var entities = new List<Entity>();

        foreach (var kvp in _patterns)
        {
            foreach (var pattern in kvp.Value)
            {
                var matches = pattern.Pattern.Matches(message);
                foreach (Match match in matches)
                {
                    // Берем первую группу или весь match
                    var value = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;

                    entities.Add(new Entity
                    {
                        Type = kvp.Key,
                        Value = value.Trim(),
                        Confidence = pattern.Confidence,
                        Metadata = new Dictionary<string, object>
                        {
                            ["pattern"] = pattern.Description,
                            ["match"] = match.Value
                        }
                    });
                }
            }
        }

        _logger.LogDebug("Regex extracted {Count} entities", entities.Count);
        return Task.FromResult(entities);
    }

    private class RegexPattern
    {
        public Regex Pattern { get; set; } = null!;
        public double Confidence { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
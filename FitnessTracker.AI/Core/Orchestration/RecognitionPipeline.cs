using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Core.Orchestration;

public class RecognitionPipeline
{
    private readonly IEnumerable<IIntentClassifier> _classifiers;
    private readonly IEnumerable<IEntityRecognizer> _recognizers;
    private readonly ILogger<RecognitionPipeline> _logger;

    public RecognitionPipeline(
        IEnumerable<IIntentClassifier> classifiers,
        IEnumerable<IEntityRecognizer> recognizers,
        ILogger<RecognitionPipeline> logger)
    {
        _classifiers = classifiers.OrderBy(c => c.GetType().Name); // Можно добавить Priority
        _recognizers = recognizers.OrderBy(r => r.Priority);
        _logger = logger;
    }

    public async Task<RecognitionResult> ProcessAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var result = new RecognitionResult
        {
            OriginalMessage = message
        };

        // Распознаем intent
        foreach (var classifier in _classifiers)
        {
            try
            {
                var intent = await classifier.ClassifyAsync(message, cancellationToken);
                if (intent != null && intent.Confidence > 0.5)
                {
                    result.Intent = intent;
                    result.Metadata["classifier"] = classifier.GetType().Name;
                    _logger.LogDebug("Intent classified by {Classifier}: {IntentType} ({Confidence})",
                        classifier.GetType().Name, intent.Type, intent.Confidence);
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in classifier {Classifier}", classifier.GetType().Name);
            }
        }

        // Извлекаем сущности
        var allEntities = new List<Entity>();
        foreach (var recognizer in _recognizers)
        {
            try
            {
                var entities = await recognizer.RecognizeAsync(message, result.Intent, cancellationToken);
                if (entities.Any())
                {
                    // Добавляем только новые сущности или с более высокой уверенностью
                    foreach (var entity in entities)
                    {
                        var existing = allEntities.FirstOrDefault(e => e.Type == entity.Type);
                        if (existing == null || entity.Confidence > existing.Confidence)
                        {
                            if (existing != null) allEntities.Remove(existing);
                            allEntities.Add(entity);
                        }
                    }

                    result.Metadata[$"recognizer_{recognizer.GetType().Name}"] = entities.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in recognizer {Recognizer}", recognizer.GetType().Name);
            }
        }

        result.Entities = allEntities;
        _logger.LogDebug("Total entities found: {Count}", result.Entities.Count);

        return result;
    }
}
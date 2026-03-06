// FitnessTracker.AI/Commands/Workout/GenerateProgramCommand.cs
using FitnessTracker.AI.Commands.Base;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.Application.DTOs;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.Services;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
namespace FitnessTracker.AI.Commands.Workout;

public class GenerateProgramCommand : BaseCommand
{
    private readonly IWorkoutTemplateService _templateService;
    private readonly IUserService _userService;
    private readonly IUserParameterService _parameterService;
    private readonly IProgramService _programService;
    private readonly IGigaChatTokenService _tokenService;
    private readonly ILogger<GenerateProgramCommand> _logger;

    public override string Name => "GenerateProgram";
    public override string Description => "Создает программу тренировок на неделю";
    public override string Category => "Workout";
    public override string Group => "Workout";
    public override double ConfidenceThreshold => 0.7;

    public override List<string> TrainingPhrases { get; } = new()
    {
        "создай программу",
        "составь план тренировок",
        "нужна программа",
        "хочу программу тренировок",
        "подбери программу",
        "create workout plan",
        "generate program"
    };

    private readonly IExerciseService _exerciseService;

    public GenerateProgramCommand(
        IWorkoutTemplateService templateService,
        IUserService userService,
        IUserParameterService parameterService,
        IProgramService programService,
        IExerciseService exerciseService, // ДОБАВИТЬ
        IGigaChatTokenService tokenService,
        ILogger<GenerateProgramCommand> logger) : base(logger)
    {
        _templateService = templateService;
        _userService = userService;
        _parameterService = parameterService;
        _programService = programService;
        _exerciseService = exerciseService; // СОХРАНИТЬ
        _tokenService = tokenService;
        _logger = logger;
    }

    public override async Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        LogExecution(context);

        try
        {
            var userId = context.UserId;
            var message = context.OriginalMessage;

            // Получаем данные пользователя
            var user = await _userService.GetByIdAsync(userId);
            var parameters = await _parameterService.GetCurrentAsync(userId);

            // Шаг 1: AI анализирует запрос и определяет параметры программы
            var (level, goal, daysPerWeek) = await AnalyzeUserRequest(message, parameters);

            // Шаг 2: Находим подходящий шаблон
            var template = _templateService.FindBestMatch(level, goal, daysPerWeek);
            if (template == null)
            {
                return Error("Не удалось найти подходящий шаблон программы");
            }

            // Шаг 3: Адаптируем программу под пользователя
            var adaptedProgram = await AdaptProgramToUser(template, user, parameters);

            // Шаг 4: Сохраняем программу в БД
            // Шаг 4: Проверяем, есть ли уже активная программа
            var existingProgram = await _programService.GetActiveProgramAsync(userId);
            if (existingProgram != null)
            {
                return Success(
                    "❌ *У вас уже есть активная программа*\n\n" +
                    $"Текущая программа: *{existingProgram.Name}*\n\n" +
                    "Если хотите создать новую, сначала удалите старую командой:\n" +
                    "• `удалить программу`\n" +
                    "• `деактивировать программу`"
                );
            }
            var savedProgram = await SaveProgramToDatabase(userId, adaptedProgram);

            if (savedProgram == null)
            {
                return Error("Не удалось сохранить программу");
            }

            // Шаг 5: Формируем красивый ответ
            var response = FormatProgramResponse(adaptedProgram);

            return Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating program for user {UserId}", context.UserId);
            return Error("Произошла ошибка при создании программы");
        }
    }

    private async Task<(string? level, string? goal, int? daysPerWeek)> AnalyzeUserRequest(string message, UserParameter? parameters)
    {
        try
        {
            // Получаем все возможные значения из шаблонов
            var availableLevels = _templateService.GetAllTemplates()
                .Select(t => t.Level)
                .Distinct()
                .ToList();

            var availableGoals = _templateService.GetAllTemplates()
                .Select(t => t.Goal)
                .Distinct()
                .ToList();

            var token = await _tokenService.GetAccessTokenAsync();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var userInfo = parameters != null
                ? $"Уровень: {parameters.ExperienceLevel ?? "unknown"}, Цель: {string.Join(", ", parameters.FitnessGoals ?? Array.Empty<string>())}"
                : "Нет данных о пользователе";

            var prompt = $@"
Ты - фитнес-тренер. Проанализируй запрос пользователя и выбери параметры из списка доступных.

Информация о пользователе: {userInfo}

Запрос: '{message}'

Доступные уровни: {string.Join(", ", availableLevels)}
Доступные цели: {string.Join(", ", availableGoals)}
Допустимые дни в неделю: 1-6

Верни ТОЛЬКО JSON в формате:
{{ ""level"": ""один_из_доступных"", ""goal"": ""одна_из_доступных"", ""days"": число }}";

            var requestBody = new
            {
                model = "GigaChat-2",
                messages = new[]
                {
                    new { role = "system", content = "Ты - классификатор. Отвечай только JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1,
                max_tokens = 100
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("GigaChat response: {Response}", responseBody); // ДОБАВЬ

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GigaChat error: {StatusCode} - {Error}", response.StatusCode, responseBody);
                return (null, null, null);
            }

            var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(responseBody);
            var resultJson = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? "{}";

            var result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);

            return (result?.level, result?.goal, result?.days);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user request");

            // Fallback на основе параметров пользователя
            if (parameters != null)
            {
                return (parameters.ExperienceLevel ?? "beginner",
                       parameters.FitnessGoals?.FirstOrDefault() ?? "hypertrophy",
                       3);
            }

            return ("beginner", "hypertrophy", 3);
        }
    }

    private async Task<ProgramTemplate> AdaptProgramToUser(WorkoutTemplate template, FitnessTracker.Domain.Entities.User? user, UserParameter? parameters)
    {
        var program = new ProgramTemplate
        {
            Name = template.Name,
            Description = template.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Days = new List<ProgramDay>()
        };

        foreach (var templateDay in template.Days)
        {
            var day = new ProgramDay
            {
                DayNumber = templateDay.DayNumber,
                Name = templateDay.Name,
                IsRestDay = false,
                Exercises = new List<ProgramDayExercise>()
            };

            foreach (var ex in templateDay.Exercises)
            {
                // ПОЛУЧАЕМ УПРАЖНЕНИЕ ИЗ БД ЧТОБЫ БЫЛО ИМЯ
                var exercise = await _exerciseService.GetExerciseByNameAsync(ex.Name);
                if (exercise == null)
                {
                    _logger.LogWarning("Exercise not found: {ExerciseName}", ex.Name);
                    continue;
                }

                day.Exercises.Add(new ProgramDayExercise
                {
                    ExerciseId = exercise.Id,
                    Exercise = exercise, // ← ВАЖНО! Заполняем навигационное свойство
                    Order = day.Exercises.Count + 1,
                    TargetSets = ex.Sets,
                    TargetRepsMin = ex.RepsMin,
                    TargetRepsMax = ex.RepsMax
                });
            }

            program.Days.Add(day);
        }

        return program;
    }

    private async Task<int> GetExerciseIdByName(string exerciseName)
    {
        // TODO: Найти ID упражнения в БД или создать новое
        // Пока возвращаем 1 (заглушка)
        return 1;
    }

    private async Task<ProgramTemplate?> SaveProgramToDatabase(long userId, ProgramTemplate program)
    {
        program.UserId = userId;

        var result = await _programService.CreateProgramAsync(
            userId,
            program.Name,
            program.Description,
            program.Days.Select(d => new ProgramDayDto
            {
                DayNumber = d.DayNumber,
                Name = d.Name,
                IsRestDay = d.IsRestDay,
                Exercises = d.Exercises.Select(e => new ProgramDayExerciseDto
                {
                    ExerciseId = (int)e.ExerciseId,  // ← явное приведение к int
                    Order = e.Order,
                    TargetSets = e.TargetSets,
                    TargetRepsMin = e.TargetRepsMin,
                    TargetRepsMax = e.TargetRepsMax
                }).ToList()
            }).ToList());

        return result;
    }

    private string FormatProgramResponse(ProgramTemplate program)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"🏋️ *{program.Name}*\n");
        sb.AppendLine($"{program.Description ?? ""}\n");
        sb.AppendLine($"📅 *Программа на {program.Days.Count} дней*\n");

        foreach (var day in program.Days.OrderBy(d => d.DayNumber))
        {
            sb.AppendLine($"*День {day.DayNumber}: {day.Name ?? ""}*");

            foreach (var ex in day.Exercises.OrderBy(e => e.Order))
            {
                var exerciseName = ex.Exercise?.Name ?? "Упражнение";
                var reps = ex.TargetRepsMax.HasValue
                    ? $"{ex.TargetRepsMin}-{ex.TargetRepsMax}"  // ← НЕ экранируем!
                    : ex.TargetRepsMin.ToString();

                sb.AppendLine($"  {ex.Order}. {exerciseName} - {ex.TargetSets} x {reps}");
            }
            sb.AppendLine("");
        }

        sb.AppendLine("💪 *Удачи в тренировках!*");

        return sb.ToString();
    }


    private class AnalysisResult
    {
        public string? level { get; set; }
        public string? goal { get; set; }
        public int? days { get; set; }
    }
}
// FitnessTracker.ConsoleTest/TestScenarios/AiTestScenario.cs
using FitnessTracker.AI.PublicServices;
using Microsoft.Extensions.Logging;
using FitnessTracker.ConsoleTest.TestHelpers;
namespace FitnessTracker.ConsoleTest.TestScenarios;

public class AiTestScenario
{
    private readonly QuestionsAIService _questionsAI;
    private readonly ILogger<AiTestScenario> _logger;

    public AiTestScenario(
        QuestionsAIService questionsAI,
        ILogger<AiTestScenario> logger)
    {
        _questionsAI = questionsAI;
        _logger = logger;
    }

    public async Task RunAsync(long userId)
    {
        ConsoleHelper.WriteHeader("ТЕСТИРОВАНИЕ AI ВОПРОСОВ");

        while (true)
        {
            Console.WriteLine("\nВыберите тип вопроса:");
            Console.WriteLine("1. ❓ Свободный вопрос (AI сам определит категорию)");
            Console.WriteLine("2. 🥗 Вопрос о питании");
            Console.WriteLine("3. 💪 Вопрос о тренировках");
            Console.WriteLine("4. 🤖 Вопрос о боте");
            Console.WriteLine("5. 📋 Тест предустановленных вопросов");
            Console.WriteLine("0. 🔙 Назад");

            var choice = ConsoleHelper.ReadInput("Ваш выбор");

            switch (choice)
            {
                case "1":
                    await TestFreeQuestion(userId);
                    break;
                case "2":
                    await TestCategoryQuestion(userId, "nutrition");
                    break;
                case "3":
                    await TestCategoryQuestion(userId, "workouts");
                    break;
                case "4":
                    await TestCategoryQuestion(userId, "bot");
                    break;
                case "5":
                    await TestPredefinedQuestions(userId);
                    break;
                case "0":
                    return;
                default:
                    ConsoleHelper.WriteError("Неверный выбор");
                    break;
            }

            ConsoleHelper.WaitForKey();
        }
    }

    private async Task TestFreeQuestion(long userId)
    {
        ConsoleHelper.WriteHeader("СВОБОДНЫЙ ВОПРОС");

        var question = ConsoleHelper.ReadInput("Введите ваш вопрос");

        if (string.IsNullOrWhiteSpace(question))
        {
            ConsoleHelper.WriteError("Вопрос не может быть пустым");
            return;
        }

        ConsoleHelper.WriteInfo("🤔 Думаю...");

        try
        {
            var startTime = DateTime.Now;
            var answer = await _questionsAI.AnswerAsync(userId, question);
            var duration = DateTime.Now - startTime;

            ConsoleHelper.WriteSuccess($"Ответ получен за {duration.TotalSeconds:F1} сек:");
            Console.WriteLine();
            ConsoleHelper.WriteInfo(answer);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            _logger.LogError(ex, "Error in AI test");
        }
    }

    private async Task TestCategoryQuestion(long userId, string category)
    {
        string categoryName = category switch
        {
            "nutrition" => "ПИТАНИЕ",
            "workouts" => "ТРЕНИРОВКИ",
            "bot" => "О БОТЕ",
            _ => category
        };

        ConsoleHelper.WriteHeader($"ВОПРОС В КАТЕГОРИИ: {categoryName}");

        var question = ConsoleHelper.ReadInput("Введите ваш вопрос");

        if (string.IsNullOrWhiteSpace(question))
        {
            ConsoleHelper.WriteError("Вопрос не может быть пустым");
            return;
        }

        ConsoleHelper.WriteInfo("🤔 Думаю...");

        try
        {
            var startTime = DateTime.Now;
            var answer = await _questionsAI.AnswerInCategoryAsync(userId, question, category);
            var duration = DateTime.Now - startTime;

            ConsoleHelper.WriteSuccess($"Ответ получен за {duration.TotalSeconds:F1} сек:");
            Console.WriteLine();
            ConsoleHelper.WriteInfo(answer);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            _logger.LogError(ex, "Error in AI test");
        }
    }

    private async Task TestPredefinedQuestions(long userId)
    {
        ConsoleHelper.WriteHeader("ТЕСТ ПРЕДУСТАНОВЛЕННЫХ ВОПРОСОВ");

        var testQuestions = new[]
        {
            ("nutrition", "Сколько белка нужно в день?"),
            ("nutrition", "Сколько воды пить?"),
            ("workouts", "Как накачать грудные мышцы?"),
            ("workouts", "Сколько раз в неделю тренироваться?"),
            ("bot", "Как изменить вес в профиле?"),
            ("bot", "Как создать тренировку?"),
            ("free", "Расскажи про правильное питание"),
            ("free", "Какие упражнения лучше для спины?")
        };

        foreach (var (category, question) in testQuestions)
        {
            ConsoleHelper.WriteInfo($"\n[{category}] {question}");

            try
            {
                string answer;
                if (category == "free")
                {
                    answer = await _questionsAI.AnswerAsync(userId, question);
                }
                else
                {
                    answer = await _questionsAI.AnswerInCategoryAsync(userId, question, category);
                }

                ConsoleHelper.WriteSuccess("✓ Ответ получен");
                ConsoleHelper.WriteJson(answer.Length > 200
                    ? answer[..200] + "...\n(ответ сокращен)"
                    : answer);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }

            await Task.Delay(500); // Небольшая пауза между запросами
        }
    }

    // Метод для комплексного тестирования
    public async Task<string?> TestSingleQuestion(long userId, string question)
    {
        try
        {
            return await _questionsAI.AnswerAsync(userId, question);
        }
        catch
        {
            return null;
        }
    }
}
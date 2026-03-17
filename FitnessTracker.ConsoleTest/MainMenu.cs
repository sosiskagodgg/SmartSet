// FitnessTracker.ConsoleTest/MainMenu.cs
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.ConsoleTest.TestScenarios;
using FitnessTracker.ConsoleTest.TestHelpers;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.AI.PublicServices;

namespace FitnessTracker.ConsoleTest;

public class MainMenu
{
    private readonly IServiceProvider _services;
    private readonly long _testUserId;

    public MainMenu(IServiceProvider services, long testUserId)
    {
        _services = services;
        _testUserId = testUserId;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.WriteHeader($"ГЛАВНОЕ МЕНЮ (User ID: {_testUserId})");

            Console.WriteLine("\nВыберите сценарий тестирования:");
            Console.WriteLine("1. 🤖 Тестирование AI (вопросы)");
            Console.WriteLine("2. 📊 Тестирование параметров пользователя");
            Console.WriteLine("3. 🏋️ Тестирование генерации тренировок");
            Console.WriteLine("4. ✅ Тестирование выполнения тренировок");
            Console.WriteLine("5. 🔄 Полный цикл (параметры → генерация → выполнение)");
            Console.WriteLine("6. 🧪 Комплексный тест всех сценариев");
            Console.WriteLine("0. 🚪 Выход");

            var choice = ConsoleHelper.ReadInput("Ваш выбор");

            using var scope = _services.CreateScope();

            switch (choice)
            {
                case "1":
                    var aiScenario = scope.ServiceProvider.GetRequiredService<AiTestScenario>();
                    await aiScenario.RunAsync(_testUserId);
                    break;

                case "2":
                    var paramsScenario = scope.ServiceProvider.GetRequiredService<UserParametersTestScenario>();
                    await paramsScenario.RunAsync(_testUserId);
                    break;

                case "3":
                    var workoutGenService = scope.ServiceProvider.GetRequiredService<WorkoutGenerationService>();
                    var userWorkoutService = scope.ServiceProvider.GetRequiredService<IUserWorkoutService>();
                    var userParamsService = scope.ServiceProvider.GetRequiredService<IUserParametersService>();

                    var workoutGenScenario = new WorkoutGenerationScenario(
                        workoutGenService,
                        userWorkoutService,
                        userParamsService
                    );
                    await workoutGenScenario.RunAsync(_testUserId);
                    break;

                case "4":
                    var workoutExecService = scope.ServiceProvider.GetRequiredService<IWorkoutService>();
                    var userWorkoutExecService = scope.ServiceProvider.GetRequiredService<IUserWorkoutService>();

                    var workoutExecScenario = new WorkoutExecutionScenario(
                        userWorkoutExecService,
                        workoutExecService
                    );
                    await workoutExecScenario.RunAsync(_testUserId);
                    break;

                case "5":
                    await RunFullCycleTestAsync(scope);
                    break;

                case "6":
                    await RunAllTestsAsync(scope);
                    break;

                case "0":
                    ConsoleHelper.WriteInfo("Выход из программы...");
                    return;

                default:
                    ConsoleHelper.WriteError("Неверный выбор");
                    ConsoleHelper.WaitForKey();
                    break;
            }
        }
    }

    private async Task RunFullCycleTestAsync(IServiceScope scope)
    {
        ConsoleHelper.WriteHeader("ПОЛНЫЙ ЦИКЛ: ПАРАМЕТРЫ → ГЕНЕРАЦИЯ → ВЫПОЛНЕНИЕ");

        var paramsService = scope.ServiceProvider.GetRequiredService<IUserParametersService>();
        var workoutGenService = scope.ServiceProvider.GetRequiredService<WorkoutGenerationService>();
        var userWorkoutService = scope.ServiceProvider.GetRequiredService<IUserWorkoutService>();
        var workoutExecService = scope.ServiceProvider.GetRequiredService<IWorkoutService>();

        // Шаг 1: Устанавливаем параметры
        ConsoleHelper.WriteInfo("\n📌 ШАГ 1: Настройка параметров пользователя");

        await paramsService.CreateOrUpdateUserParametersAsync(
            _testUserId,
            height: 180,
            weight: 75,
            bodyFat: 15,
            experience: "intermediate",
            goals: "strength"
        );
        ConsoleHelper.WriteSuccess("Параметры установлены: рост 180см, вес 75кг, опыт средний, цель сила");

        // Шаг 2: Генерируем программу
        ConsoleHelper.WriteInfo("\n📌 ШАГ 2: Генерация программы тренировок");

        var success = await workoutGenService.CreateWorkoutProgramAsync(
            _testUserId,
            daysPerWeek: 4,
            goal: "strength",
            experience: "intermediate",
            location: "gym",
            durationMinutes: 60,
            gender: "male"
        );

        if (success)
        {
            ConsoleHelper.WriteSuccess("✅ Программа успешно создана!");

            // Показываем созданные тренировки
            var workouts = await userWorkoutService.GetAllUserWorkoutsAsync(_testUserId);
            ConsoleHelper.WriteInfo($"Создано тренировок: {workouts.Count}");

            foreach (var w in workouts.OrderBy(w => w.DayNumber))
            {
                Console.WriteLine($"  День {w.DayNumber}: {w.Name} ({w.Exercises.Count} упражнений)");
            }
        }
        else
        {
            ConsoleHelper.WriteError("❌ Не удалось создать программу");
        }

        // Шаг 3: Предлагаем выполнить тренировку
        ConsoleHelper.WriteInfo("\n📌 ШАГ 3: Выполнение тренировки");
        ConsoleHelper.WriteInfo("Теперь вы можете перейти в меню выполнения (пункт 4) и начать тренировку на сегодня.");

        ConsoleHelper.WaitForKey();
    }

    private async Task RunAllTestsAsync(IServiceScope scope)
    {
        ConsoleHelper.WriteHeader("КОМПЛЕКСНОЕ ТЕСТИРОВАНИЕ");

        var aiScenario = scope.ServiceProvider.GetRequiredService<AiTestScenario>();
        var paramsScenario = scope.ServiceProvider.GetRequiredService<UserParametersTestScenario>();

        // Тест 1: Сначала зададим параметры
        ConsoleHelper.WriteInfo("\n📌 ШАГ 1: Установка параметров пользователя");
        await SetTestParameters(_testUserId, scope);

        // Тест 2: Зададим вопросы AI
        ConsoleHelper.WriteInfo("\n📌 ШАГ 2: Тестирование AI вопросов");
        await TestAiQuestions(_testUserId, aiScenario);

        ConsoleHelper.WriteSuccess("\n✅ Комплексное тестирование завершено!");
        ConsoleHelper.WaitForKey();
    }

    private async Task SetTestParameters(long userId, IServiceScope scope)
    {
        var parametersService = scope.ServiceProvider.GetRequiredService<IUserParametersService>();

        await parametersService.CreateOrUpdateUserParametersAsync(
            userId,
            height: 180,
            weight: 75,
            bodyFat: 15,
            experience: "intermediate",
            goals: "gain_muscle"
        );

        ConsoleHelper.WriteSuccess("Параметры установлены");
    }

    private async Task TestAiQuestions(long userId, AiTestScenario aiScenario)
    {
        var questions = new[]
        {
            "Сколько белка нужно для набора массы?",
            "Как правильно приседать?",
            "Как часто нужно тренироваться?"
        };

        foreach (var question in questions)
        {
            ConsoleHelper.WriteInfo($"\nВопрос: {question}");
            try
            {
                var answer = await aiScenario.TestSingleQuestion(userId, question);
                if (answer != null)
                {
                    var preview = answer.Length > 100 ? answer[..100] + "..." : answer;
                    ConsoleHelper.WriteInfo($"Ответ: {preview}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка: {ex.Message}");
            }
            await Task.Delay(1000);
        }
    }
}
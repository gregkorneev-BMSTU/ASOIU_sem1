using System;
using System.Globalization;
using System.IO;

Console.WriteLine("=== АНАЛИЗ КВАЛИФИКАЦИИ ГРАН-ПРИ (версия 2) ===");
Console.WriteLine();

// Выбор режима ввода
Console.WriteLine("Выберите способ ввода данных:");
Console.WriteLine("1 - ввод с клавиатуры");
Console.WriteLine("2 - чтение из файла");

int inputMode;
while (true)
{
    inputMode = ReadPositiveInt("Ваш выбор: ");
    if (inputMode == 1 || inputMode == 2)
    {
        break;
    }

    Console.WriteLine("Ошибка: введите 1 или 2.");
}

Console.WriteLine();

string[] teams;
double[] avgSpeeds;
int n;

if (inputMode == 1)
{
    n = ReadPositiveInt("Введите количество участников: ");
    Console.WriteLine();

    teams = new string[n];
    avgSpeeds = new double[n];

    InputData(teams, avgSpeeds, n);
}
else
{
string filePath = "input.txt";

Console.WriteLine("Чтение данных из файла input.txt...");
Console.WriteLine();

ReadDataFromFile(filePath, out teams, out avgSpeeds, out n);
}

// Вычисление статистики
Console.WriteLine("--- СТАТИСТИКА КВАЛИФИКАЦИИ ---");
CalculateStatistics(teams, avgSpeeds, n);
Console.WriteLine();

// Вывод исходного порядка
Console.WriteLine("--- ИСХОДНЫЙ ПОРЯДОК ---");
PrintTable(teams, avgSpeeds, n, false);
Console.WriteLine();

// Копирование массивов для сортировки
string[] sortedTeams = new string[n];
double[] sortedSpeeds = new double[n];
CopyArrays(teams, avgSpeeds, sortedTeams, sortedSpeeds, n);

// Сортировка
BubbleSort(sortedTeams, sortedSpeeds, n);

// Вывод отсортированного протокола
Console.WriteLine("--- ИТОГОВЫЙ ПРОТОКОЛ КВАЛИФИКАЦИИ ---");
PrintTable(sortedTeams, sortedSpeeds, n, true);
Console.WriteLine();

// Фильтр по скорости
FilterBySpeed(sortedTeams, sortedSpeeds, n);
Console.WriteLine();

Console.Write("Нажмите любую клавишу для выхода...");
Console.ReadKey();

// ================= ФУНКЦИИ =================

static void InputData(string[] teams, double[] speeds, int n)
{
    for (int i = 0; i < n; i++)
    {
        Console.WriteLine($"Участник #{i + 1}");
        teams[i] = ReadNonEmptyString("Команда: ");
        speeds[i] = ReadDouble("Средняя скорость (км/ч): ");
        Console.WriteLine();
    }
}

static void ReadDataFromFile(string filePath, out string[] teams, out double[] speeds, out int n)
{
    while (true)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Ошибка: файл не найден.");
                filePath = ReadNonEmptyString("Введите путь к файлу снова: ");
                continue;
            }

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                Console.WriteLine("Ошибка: файл пуст.");
                filePath = ReadNonEmptyString("Введите путь к файлу снова: ");
                continue;
            }

            if (!int.TryParse(lines[0], out n) || n <= 0)
            {
                Console.WriteLine("Ошибка: первая строка файла должна содержать положительное целое число.");
                filePath = ReadNonEmptyString("Введите путь к файлу снова: ");
                continue;
            }

            if (lines.Length != 1 + n * 2)
            {
                Console.WriteLine("Ошибка: количество строк в файле не соответствует числу участников.");
                filePath = ReadNonEmptyString("Введите путь к файлу снова: ");
                continue;
            }

            teams = new string[n];
            speeds = new double[n];

            int lineIndex = 1;

            for (int i = 0; i < n; i++)
            {
                string team = lines[lineIndex].Trim();
                string speedText = lines[lineIndex + 1].Trim().Replace(',', '.');

                if (string.IsNullOrWhiteSpace(team))
                {
                    throw new Exception($"Ошибка: пустое название команды у участника #{i + 1}.");
                }

                if (!double.TryParse(speedText, NumberStyles.Float, CultureInfo.InvariantCulture, out double speed))
                {
                    throw new Exception($"Ошибка: некорректная скорость у участника #{i + 1}.");
                }

                teams[i] = team;
                speeds[i] = speed;

                lineIndex += 2;
            }

            Console.WriteLine("Данные успешно считаны из файла.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            filePath = ReadNonEmptyString("Введите путь к файлу снова: ");
        }
    }
}

static void CalculateStatistics(string[] teams, double[] speeds, int n)
{
    double sum = 0;

    for (int i = 0; i < n; i++)
    {
        sum += speeds[i];
    }

    double average = sum / n;

    double maxSpeed = speeds[0];
    double minSpeed = speeds[0];
    string fastestTeam = teams[0];
    string slowestTeam = teams[0];

    for (int i = 1; i < n; i++)
    {
        if (speeds[i] > maxSpeed)
        {
            maxSpeed = speeds[i];
            fastestTeam = teams[i];
        }

        if (speeds[i] < minSpeed)
        {
            minSpeed = speeds[i];
            slowestTeam = teams[i];
        }
    }

    Console.WriteLine($"Средняя скорость: {average:F2} км/ч");
    Console.WriteLine($"Лидер: {fastestTeam} ({maxSpeed:F2} км/ч)");
    Console.WriteLine($"Самый медленный: {slowestTeam} ({minSpeed:F2} км/ч)");
    Console.WriteLine($"Разница темпа: {maxSpeed - minSpeed:F2} км/ч");
}

static void PrintTable(string[] teams, double[] speeds, int n, bool showPosition)
{
    Console.WriteLine("--------------------------------------------------");

    if (showPosition)
    {
        Console.WriteLine("| Поз. | Команда              | Скорость        |");
    }
    else
    {
        Console.WriteLine("| Команда              | Скорость (км/ч)     |");
    }

    Console.WriteLine("--------------------------------------------------");

    for (int i = 0; i < n; i++)
    {
        if (showPosition)
        {
            Console.WriteLine($"| {i + 1,4} | {teams[i],-20} | {speeds[i],14:F2} |");
        }
        else
        {
            Console.WriteLine($"| {teams[i],-20} | {speeds[i],20:F2} |");
        }
    }

    Console.WriteLine("--------------------------------------------------");
}

static void BubbleSort(string[] teams, double[] speeds, int n)
{
    for (int i = 0; i < n - 1; i++)
    {
        for (int j = 0; j < n - i - 1; j++)
        {
            if (speeds[j] < speeds[j + 1])
            {
                double tempSpeed = speeds[j];
                speeds[j] = speeds[j + 1];
                speeds[j + 1] = tempSpeed;

                string tempTeam = teams[j];
                teams[j] = teams[j + 1];
                teams[j + 1] = tempTeam;
            }
        }
    }
}

static void CopyArrays(string[] srcTeams, double[] srcSpeeds, string[] dstTeams, double[] dstSpeeds, int n)
{
    for (int i = 0; i < n; i++)
    {
        dstTeams[i] = srcTeams[i];
        dstSpeeds[i] = srcSpeeds[i];
    }
}

static void FilterBySpeed(string[] teams, double[] speeds, int n)
{
    Console.WriteLine("--- ДОПОЛНИТЕЛЬНО: ФИЛЬТР ПО СКОРОСТИ ---");
    double minSpeed = ReadDouble("Введите минимальную скорость для отбора (км/ч): ");

    Console.WriteLine();
    Console.WriteLine($"Команды со скоростью >= {minSpeed:F2} км/ч:");

    int count = 0;

    for (int i = 0; i < n; i++)
    {
        if (speeds[i] >= minSpeed)
        {
            Console.WriteLine($"- {teams[i]} ({speeds[i]:F2} км/ч)");
            count++;
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Отобрано команд: {count}");
}

static int ReadPositiveInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int value) && value > 0)
        {
            return value;
        }

        Console.WriteLine("Ошибка: введите целое положительное число.");
    }
}

static double ReadDouble(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Ошибка: ввод не должен быть пустым.");
            continue;
        }

        input = input.Replace(',', '.');

        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        Console.WriteLine("Ошибка: введите корректное число.");
    }
}

static string ReadNonEmptyString(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        Console.WriteLine("Ошибка: строка не должна быть пустой.");
    }
}

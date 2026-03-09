## AurumSoftTask

[![.NET Build and Test](https://github.com/Maksonjenu/AurumSoftTask/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Maksonjenu/AurumSoftTask/actions/workflows/dotnet.yml)

Библиотека для разбора, валидации и анализа данных по скважинам из CSV-файлов. Решение тестового задания.

Основной сценарий:

- прочитать CSV с интервалами по скважинам
- отфильтровать некорректные строки с понятными ошибками
- посчитать агрегированные показатели по каждой скважине
- Экспорт **только валидных** данных в JSON (невалидные данные выводятся в отдельную таблицу)

Формат входных данных (разделитель `;`):

```text
WellId;X;Y;DepthFrom;DepthTo;Rock;Porosity
A-001;82.10;55.20;0;10;Sandstone;0.18
A-001;82.10;55.20;10;25;Limestone;0.07
A-002;90.00;60.00;0;15;Shale;0.04
...
```

## Структура решения

- `AurumSoftTask.Core` — базовые модели:
  - `CsvRow` — одна строка исходного CSV;
  - `ValidationError` — описание проблемной строки и типа ошибки;
  - `WellSummary` — агрегированная статистика по скважине;
  - `Interval`, `Well` — вспомогательные доменные модели.
- `AurumSoftTask.Services` — доменные сервисы:
  - `ICsvParser` / `CsvParser` — разбирает CSV-файл и возвращает:
    - `Rows` — список корректно распарсенных строк;
    - `ParseErrors` — ошибки формата (кол-во колонок, парсинг чисел и т.п.).
  - `IWellValidator` / `WellValidator` — проверяет бизнес-правила:
    - глубина не отрицательная, `DepthFrom < DepthTo`;
    - `Porosity` в диапазоне \[0;1\];
    - порода (`Rock`) заполнена;
    - интервалы по одной скважине не пересекаются.
  - `IWellAnalyzer` / `WellAnalyzer` — строит `WellSummary` по валидным строкам:
    - максимальная глубина скважины;
    - количество интервалов;
    - средневзвешенная пористость по толщине интервалов;
    - порода с максимальной суммарной толщиной.
- `AurumSoftTask.Services.Tests` — NUnit‑тесты для сервисов.

## Быстрый старт

Требуется .NET 8 SDK.

```bash
dotnet restore
dotnet build
```

### Пример использования сервисов

```csharp
using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Implementation;
using AurumSoftTask.Services.Interfaces;

ICsvParser parser = new CsvParser();
IWellValidator validator = new WellValidator();
IWellAnalyzer analyzer = new WellAnalyzer();

var (rows, parseErrors) = parser.Parse("input.csv");

// обработка ошибок формата
foreach (var error in parseErrors)
{
    Console.WriteLine($"Parse error in line {error.LineWithError}: {error.ErrorDetails}");
}

// валидация бизнес-правил
var (validRows, validationErrors) = validator.Validate(rows);

foreach (var error in validationErrors)
{
    Console.WriteLine($"Validation error in well {error.WellId}, line {error.LineWithError}: {error.ErrorDetails}");
}

// расчёт сводок для валидных строк
List<WellSummary> summaries = analyzer.CalculateSummary(validRows);

foreach (var summary in summaries)
{
    Console.WriteLine($"{summary.WellId}: depth={summary.TotalDepth}, intervals={summary.IntervalCount}, " +
                      $"avg φ={summary.AveragePorosity:F3}, top rock={summary.TopRockType}");
}
```

## Тесты

Для сервисов есть отдельный проект с тестами на NUnit:

- `CsvParserTests` — базовые сценарии разбора CSV (валидный файл, неверное число колонок, ошибки парсинга чисел, пустые строки, заголовок);
- `WellValidatorTests` — проверки диапазонов глубин, пористости, заполненности породы и пересечения интервалов;
- `WellAnalyzerTests` — расчёт сводок по примерам и порядок скважин по `WellId`.

Запуск тестов:

```bash
dotnet test AurumSoftTask.Services.Tests/AurumSoftTask.Services.Tests.csproj
```



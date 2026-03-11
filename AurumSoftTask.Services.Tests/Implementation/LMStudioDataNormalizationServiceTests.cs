using AurumSoftTask.Services.Interfaces;
using NUnit.Framework;

namespace AurumSoftTask.Services.Tests.Implementation;

[TestFixture]
public class LMStudioDataNormalizationServiceTests
{
    // Интеграционные тесты: требуют запущенный LM Studio на http://localhost:1234/api/v1/chat.
    // Помечены Explicit, чтобы не падать на CI/без локального LLM.

    [Test, Explicit("Requires LM Studio running on http://localhost:1234/api/v1/chat")]
    public async Task NormalizeRockNamesAsync_ValidJson_ReturnsDictionary()
    {
        var rockNames = new[] { "sndstn", "Песчаник", "Limestone" };

        IDataNormalizationService service = new LMStudioDataNormalizationService();

        var result = await service.NormalizeRockNamesAsync(rockNames);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(rockNames.Length));
        foreach (var name in rockNames)
        {
            Assert.That(result.ContainsKey(name), Is.True, $"Result must contain key for '{name}'.");
            Assert.That(string.IsNullOrWhiteSpace(result[name]), Is.False, $"Normalized value for '{name}' should not be empty.");
        }
    }

    [Test, Explicit("Requires LM Studio running on http://localhost:1234/api/v1/chat")]
    public async Task NormalizeRockNamesAsync_EmptyOutput_ReturnsEmptyDictionary()
    {
        var rockNames = Array.Empty<string>();

        IDataNormalizationService service = new LMStudioDataNormalizationService();

        var result = await service.NormalizeRockNamesAsync(rockNames);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}


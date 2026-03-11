using AurumSoftTask.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

public class LMStudioDataNormalizationService : IDataNormalizationService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _lmStudioEndpoint = "http://localhost:1234/api/v1/chat";

    public async Task<Dictionary<string, string>> NormalizeRockNamesAsync(IEnumerable<string> rockNames)
    {
        var requestPayload = new
        {
            model = "qwen-portal/qwen3-vl-8b",
            input = BuildPrompt(rockNames)
        };

        var response = await _httpClient.PostAsJsonAsync(_lmStudioEndpoint, requestPayload);
        response.EnsureSuccessStatusCode();

        var lmStudioResponse = await response.Content.ReadFromJsonAsync<LMStudioApiResponse>();

        string jsonContent = lmStudioResponse?.output?.FirstOrDefault()?.content ?? "{}";

        return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? new Dictionary<string, string>();
    }

    private string BuildPrompt(IEnumerable<string> rockNames)
    {
        string uniqueNames = string.Join(", ", rockNames.Select(n => $"\"{n}\""));

        return $@"You are an expert geologist API. Your task is to normalize this list of rock formation names to a consistent English standard (e.g., Sandstone, Limestone, Shale, Granite): [{uniqueNames}].
                Your response MUST be ONLY a valid JSON object mapping each original name to its normalized version.
                Do not add any explanation, introductory text, markdown backticks, or anything else. Just the raw JSON.
                Example format: {{""sndstn"": ""Sandstone"", ""Песчаник"": ""Sandstone"", ""Limestone"": ""Limestone""}}";
    }

    private record LMStudioApiResponse(OutputMessage[] output);
    private record OutputMessage(string content);

}
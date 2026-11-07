using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<AiController> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var apiKey = _configuration["Groq:ApiKey"];
            var model = _configuration["Groq:Model"] ?? "openai/gpt-oss-20b";

            _logger.LogInformation("Groq API key present: {HasKey}, length: {Length}, model: {Model}",
                !string.IsNullOrEmpty(apiKey),
                apiKey?.Length ?? 0,
                model);

            if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith("gsk_", StringComparison.Ordinal))
            {
                return BadRequest(new { error = "مفتاح Groq غير صحيح" });
            }

            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { error = "الرسالة فارغة" });
            }

            const string systemPrompt = """
                أنت مساعد ذكي محترف لنظام إدارة الموارد البشرية. أجب بالعربية دائماً بشكل ودود ومختصر.

                السياسات:
                1. الإجازات: السنوية 30 يوم (دائمين) أو 15 يوم (تحت التجربة). المرضية 30 يوم بأجر كامل + 60 يوم بنصف أجر. الأمومة 10 أسابيع. الحج 15 يوم. الأبوة 5 أيام.
                2. الحضور: من 8 صباحاً إلى 5 مساءً، راحة من 12 إلى 1 ظهراً.
                3. العمل الإضافي: 150% من أجر الساعة، بحد أقصى 3 ساعات يومياً.
                4. مكافأة نهاية الخدمة: نصف شهر عن كل سنة لأول 5 سنوات، وشهر كامل بعد ذلك.
                5. فترة التجربة: 3 أشهر قابلة للتمديد شهراً.
                """;

            var payload = new GroqChatCompletionRequest
            {
                Model = model,
                Messages =
                [
                    new GroqMessage { Role = "system", Content = systemPrompt },
                    new GroqMessage { Role = "user", Content = request.Message }
                ],
                Temperature = 0.7,
                MaxCompletionTokens = 2048,
                ReasoningEffort = "low"
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            _logger.LogInformation("Groq request payload: {Payload}", json);

            var client = _httpClientFactory.CreateClient("Groq");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Groq response status {StatusCode} ({Status}): {Body}",
                (int)response.StatusCode,
                response.StatusCode,
                responseString);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new
                {
                    error = $"فشل الاتصال بـ Groq: {(int)response.StatusCode} {response.StatusCode}",
                    details = responseString
                });
            }

            var text = ExtractAssistantText(responseString);
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Groq returned an empty assistant message. Body: {Body}", responseString);
                return StatusCode(502, new { error = "الرد من النموذج فارغ.", details = responseString });
            }

            return Ok(new { reply = text });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Groq chat request failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static string? ExtractAssistantText(string responseString)
    {
        using var doc = JsonDocument.Parse(responseString);
        var root = doc.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
        {
            return null;
        }

        var message = choices[0].GetProperty("message");
        if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
        {
            var text = content.GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        if (message.TryGetProperty("reasoning", out var reasoning) && reasoning.ValueKind == JsonValueKind.String)
        {
            return reasoning.GetString();
        }

        return null;
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}

internal sealed class GroqChatCompletionRequest
{
    public string Model { get; set; } = string.Empty;
    public List<GroqMessage> Messages { get; set; } = [];
    public double Temperature { get; set; }

    [JsonPropertyName("max_completion_tokens")]
    public int MaxCompletionTokens { get; set; }

    [JsonPropertyName("reasoning_effort")]
    public string? ReasoningEffort { get; set; }
}

internal sealed class GroqMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

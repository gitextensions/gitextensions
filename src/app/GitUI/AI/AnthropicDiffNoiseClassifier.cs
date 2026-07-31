using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using GitCommands;

namespace GitUI.AI;

/// <summary>
/// Raised when diff classification cannot be performed (e.g. missing configuration or an API error).
/// The <see cref="Exception.Message"/> is safe to show to the user.
/// </summary>
public sealed class DiffNoiseClassifierException : Exception
{
    public DiffNoiseClassifierException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Classifies diffs by calling the Anthropic Messages API directly with an API key.
/// </summary>
public sealed class AnthropicDiffNoiseClassifier : IDiffNoiseClassifier
{
    // The API handles concurrent requests well, so several batches can run in parallel.
    private const int _maxConcurrency = 6;

    private static readonly HttpClient _httpClient = new(new HttpClientHandler { UseProxy = true, DefaultProxyCredentials = CredentialCache.DefaultCredentials })
    {
        Timeout = TimeSpan.FromMinutes(2)
    };

    private readonly string _endpoint;
    private readonly string _model;
    private readonly string _apiKey;
    private readonly string _anthropicVersion;
    private readonly int _maxCharsPerFile;

    public AnthropicDiffNoiseClassifier()
    {
        _endpoint = AppSettings.AiFilterEndpoint.Value;
        _model = AppSettings.AiFilterModel.Value;
        _anthropicVersion = AppSettings.AiFilterAnthropicVersion.Value;
        _maxCharsPerFile = Math.Max(1000, AppSettings.AiFilterMaxDiffCharsPerFile.Value);

        string configuredKey = AppSettings.AiFilterApiKey.Value;
        _apiKey = !string.IsNullOrWhiteSpace(configuredKey)
            ? configuredKey.Trim()
            : Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")?.Trim() ?? "";
    }

    public async Task<IReadOnlyList<DiffNoiseClassification>> ClassifyAsync(
        IReadOnlyList<DiffFileContent> files,
        DiffNoiseFilterOptions options,
        IProgress<DiffNoiseProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (!options.AnyEnabled || files.Count == 0)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new DiffNoiseClassifierException("No Anthropic API key configured. Set it in Settings > Diff AI filter (or the ANTHROPIC_API_KEY environment variable).");
        }

        return await DiffNoiseBatchRunner.RunAsync(
            files,
            _maxCharsPerFile,
            _maxConcurrency,
            (batch, ct) => ClassifyBatchAsync(batch, options, ct),
            progress,
            cancellationToken);
    }

    private async Task<IReadOnlyList<DiffNoiseClassification>> ClassifyBatchAsync(
        IReadOnlyList<DiffFileContent> batch,
        DiffNoiseFilterOptions options,
        CancellationToken cancellationToken)
    {
        string requestJson = BuildRequestJson(batch, options);

        using HttpRequestMessage request = new(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("x-api-key", _apiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", _anthropicVersion);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DiffNoiseClassifierException($"Failed to contact the AI service: {ex.Message}", ex);
        }

        using (response)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new DiffNoiseClassifierException($"AI service returned {(int)response.StatusCode} {response.StatusCode}: {Truncate(body, 500)}");
            }

            string text = ExtractResponseText(body);
            return DiffNoisePrompt.ParseClassifications(text, batch);
        }
    }

    private string BuildRequestJson(IReadOnlyList<DiffFileContent> batch, DiffNoiseFilterOptions options)
    {
        Dictionary<string, object> payload = new()
        {
            ["model"] = _model,
            ["max_tokens"] = Math.Min(8000, 200 + (batch.Count * 120)),
            ["system"] = DiffNoisePrompt.SystemPrompt,
            ["messages"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = DiffNoisePrompt.BuildUserPrompt(batch, options, _maxCharsPerFile)
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string ExtractResponseText(string body)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("content", out JsonElement content) && content.ValueKind == JsonValueKind.Array)
            {
                StringBuilder sb = new();
                foreach (JsonElement block in content.EnumerateArray())
                {
                    if (block.TryGetProperty("type", out JsonElement type)
                        && type.GetString() == "text"
                        && block.TryGetProperty("text", out JsonElement text))
                    {
                        sb.Append(text.GetString());
                    }
                }

                if (sb.Length > 0)
                {
                    return sb.ToString();
                }
            }
        }
        catch (JsonException ex)
        {
            throw new DiffNoiseClassifierException($"Could not parse the AI service response: {ex.Message}", ex);
        }

        throw new DiffNoiseClassifierException("The AI service response did not contain any text content.");
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "…";

    internal static TestAccessor GetTestAccessor() => new();

    internal readonly struct TestAccessor
    {
        internal string ExtractResponseText(string body) => AnthropicDiffNoiseClassifier.ExtractResponseText(body);
    }
}

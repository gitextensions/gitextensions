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
/// Classifies diffs by calling the Anthropic Messages API.
/// </summary>
public sealed class AnthropicDiffNoiseClassifier : IDiffNoiseClassifier
{
    // Bounds per request so a large diff set is split into several calls.
    private const int _maxFilesPerBatch = 25;
    private const int _maxCharsPerBatch = 120_000;

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
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new DiffNoiseClassifierException("No Anthropic API key configured. Set it in Settings > Diff AI filter (or the ANTHROPIC_API_KEY environment variable).");
        }

        if (!options.AnyEnabled || files.Count == 0)
        {
            return [];
        }

        List<DiffNoiseClassification> results = [];
        foreach (List<DiffFileContent> batch in CreateBatches(files))
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.AddRange(await ClassifyBatchAsync(batch, options, cancellationToken));
        }

        return results;
    }

    private IEnumerable<List<DiffFileContent>> CreateBatches(IReadOnlyList<DiffFileContent> files)
    {
        List<DiffFileContent> current = [];
        int currentChars = 0;
        foreach (DiffFileContent file in files)
        {
            int size = Math.Min(file.Diff.Length, _maxCharsPerFile);
            if (current.Count > 0 && (current.Count >= _maxFilesPerBatch || currentChars + size > _maxCharsPerBatch))
            {
                yield return current;
                current = [];
                currentChars = 0;
            }

            current.Add(file);
            currentChars += size;
        }

        if (current.Count > 0)
        {
            yield return current;
        }
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
            return ParseClassifications(text, batch);
        }
    }

    private string BuildRequestJson(IReadOnlyList<DiffFileContent> batch, DiffNoiseFilterOptions options)
    {
        StringBuilder user = new();
        user.AppendLine("Classify each of the following file diffs into exactly one category.");
        user.AppendLine("Return your answer by calling nothing and instead responding with ONLY a JSON object, no prose, of the form:");
        user.AppendLine("""{"files":[{"path":"<path>","category":"<Category>","reason":"<short reason>"}]}""");
        user.AppendLine();
        user.AppendLine("Valid categories:");
        user.AppendLine("- None: the file contains at least one substantive change (default when unsure).");
        AppendEnabledCategory(user, "Imports", options.Imports, "ALL changes only add, remove or reorder import statements (e.g. C# `using` directives).");
        AppendEnabledCategory(user, "CallerSiteRename", options.CallerSiteRename, "ALL changes only update references to a renamed symbol at its call sites; the declaration/definition of the symbol is NOT in this diff.");
        AppendEnabledCategory(user, "SyncToAsync", options.SyncToAsync, "ALL changes only convert code between synchronous and asynchronous form (async/await, Task/ValueTask return types, *Async name suffixes) without altering behavior.");
        AppendEnabledCategory(user, "StyleOnly", options.StyleOnly, "ALL changes are formatting/whitespace/style only and do not change behavior.");
        user.AppendLine();
        user.AppendLine("Rules:");
        user.AppendLine("- Assign a non-None category ONLY when EVERY change in the file fits that single category. If a file mixes real changes with noise, use None.");
        user.AppendLine("- Only use a category that is listed above. If a change is noise of a category not listed, treat it as a real change (None).");
        user.AppendLine("- Be conservative: when in doubt, use None so real changes are never hidden.");
        user.AppendLine("- Output exactly one entry per file, echoing the given path verbatim.");
        user.AppendLine();

        for (int i = 0; i < batch.Count; i++)
        {
            DiffFileContent file = batch[i];
            user.AppendLine($"===== FILE {i + 1}: {file.Path} =====");
            user.AppendLine(TruncateDiff(file.Diff));
            user.AppendLine();
        }

        Dictionary<string, object> payload = new()
        {
            ["model"] = _model,
            ["max_tokens"] = Math.Min(8000, 200 + (batch.Count * 120)),
            ["system"] = "You are a precise code-review assistant that classifies git diffs. You only ever respond with the requested JSON object.",
            ["messages"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = user.ToString()
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private static void AppendEnabledCategory(StringBuilder sb, string name, bool enabled, string description)
    {
        if (enabled)
        {
            sb.AppendLine($"- {name}: {description}");
        }
    }

    private string TruncateDiff(string diff)
        => diff.Length <= _maxCharsPerFile
            ? diff
            : diff[.._maxCharsPerFile] + $"{Environment.NewLine}... [diff truncated after {_maxCharsPerFile} characters] ...";

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

    private static IReadOnlyList<DiffNoiseClassification> ParseClassifications(string text, IReadOnlyList<DiffFileContent> batch)
    {
        string json = ExtractJsonObject(text);
        if (json.Length == 0)
        {
            return [];
        }

        Dictionary<string, DiffNoiseClassification> byPath = [];
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("files", out JsonElement filesElement) || filesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            foreach (JsonElement entry in filesElement.EnumerateArray())
            {
                if (!entry.TryGetProperty("path", out JsonElement pathElement))
                {
                    continue;
                }

                string? path = pathElement.GetString();
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string? categoryName = entry.TryGetProperty("category", out JsonElement categoryElement) ? categoryElement.GetString() : null;
                string? reason = entry.TryGetProperty("reason", out JsonElement reasonElement) ? reasonElement.GetString() : null;

                DiffNoiseCategory category = Enum.TryParse(categoryName, ignoreCase: true, out DiffNoiseCategory parsed)
                    ? parsed
                    : DiffNoiseCategory.None;

                byPath[path] = new DiffNoiseClassification(path, category, reason);
            }
        }
        catch (JsonException)
        {
            // Malformed JSON from the model: treat as "nothing classified" rather than failing the whole operation.
            return [];
        }

        // Correlate back to the requested files, so paths the model altered are ignored.
        List<DiffNoiseClassification> results = [];
        foreach (DiffFileContent file in batch)
        {
            if (byPath.TryGetValue(file.Path, out DiffNoiseClassification? classification))
            {
                results.Add(classification);
            }
        }

        return results;
    }

    private static string ExtractJsonObject(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : "";
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "…";

    internal static TestAccessor GetTestAccessor() => new();

    internal readonly struct TestAccessor
    {
        internal string ExtractResponseText(string body) => AnthropicDiffNoiseClassifier.ExtractResponseText(body);

        internal IReadOnlyList<DiffNoiseClassification> ParseClassifications(string text, IReadOnlyList<DiffFileContent> batch)
            => AnthropicDiffNoiseClassifier.ParseClassifications(text, batch);
    }
}

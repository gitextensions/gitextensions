using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GitExtensions.Extensibility;

namespace GitCommands;

/// <summary>
///  Generates a commit message from a staged diff using a user-configured OpenAI-compatible
///  Chat Completions endpoint (<c>{baseUrl}/chat/completions</c>). The same request/response shape is
///  accepted by OpenAI, Azure OpenAI, OpenRouter, Groq, and local servers such as Ollama, so one code
///  path covers cloud and local models. All configuration is read from <see cref="AppSettings"/>.
/// </summary>
public static class AiCommitMessageGenerator
{
    public const string DefaultSystemPrompt =
        """
        You are a senior software engineer writing a git commit message for the staged diff.

        Write the message in this exact shape:

        1. Subject line: imperative mood, at most 50 characters, no trailing period.
           Use a Conventional Commits prefix when it fits the change, one of:
           feat, fix, refactor, docs, test, chore, perf, build, ci
           (example: "fix: prevent crash when staging an empty file").
        2. Then exactly one blank line.
        3. Body: explain WHAT changed and, above all, WHY it changed. Hard-wrap
           every body line at 72 characters. Use "- " bullet points when there
           are several distinct changes.

        Guidelines:
        - Infer the intent from the diff; never invent changes that aren't there.
        - For a small, self-explanatory change, a subject line alone is fine.
        - Be concise and specific; avoid filler like "updated some code".

        Output ONLY the raw commit message text: no markdown, no code fences, no
        surrounding quotes, and no commentary before or after it.
        """;

    // Reused across calls; HttpClient is designed to be long-lived (avoids per-call socket churn).
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(90) };

    /// <summary>
    ///  Sends <paramref name="diff"/> to the configured endpoint and returns the suggested commit message.
    /// </summary>
    public static async Task<string> GenerateAsync(string diff, CancellationToken cancellationToken = default)
    {
        string baseUrl = AppSettings.AiCommitMessageApiBaseUrl.Value.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("The AI commit message API base URL is not configured (Settings > Commit dialog).");
        }

        string apiKey = AppSettings.AiCommitMessageApiKey.Trim();
        string model = AppSettings.AiCommitMessageModel.Value;
        string systemPrompt = AppSettings.AiCommitMessageSystemPrompt.Value;

        int maxChars = AppSettings.AiCommitMessageMaxDiffLength.Value;
        if (maxChars > 0 && diff.Length > maxChars)
        {
            diff = diff[..maxChars] + "\n\n[diff truncated to fit the configured limit]";
        }

        // Note: no "temperature" is sent. Newer models (e.g. the GPT-5 family) reject any
        // non-default temperature, and omitting it lets every OpenAI-compatible endpoint apply its
        // own default - the system prompt already constrains the output tightly.
        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = "Here is the staged git diff. Write the commit message for it:\n\n" + diff }
            }
        };

        string json = JsonSerializer.Serialize(requestBody);

        using HttpRequestMessage request = new(HttpMethod.Post, baseUrl + "/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrEmpty(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string details = $"The AI provider returned {(int)response.StatusCode} ({response.ReasonPhrase}).{Environment.NewLine}{Environment.NewLine}{Truncate(responseText, 1000)}";
            throw new UserExternalOperationException(
                "AI commit message generation failed.",
                new ExternalOperationException(command: request.RequestUri?.ToString(), exitCode: (int)response.StatusCode, innerException: new Exception(details)));
        }

        return ExtractContent(responseText);
    }

    /// <summary>
    ///  Extracts the generated commit message from a Chat Completions JSON response.
    /// </summary>
    public static string ExtractContent(string responseText)
    {
        using JsonDocument doc = JsonDocument.Parse(responseText);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("choices", out JsonElement choices)
            && choices.ValueKind == JsonValueKind.Array
            && choices.GetArrayLength() > 0)
        {
            JsonElement first = choices[0];
            if (first.TryGetProperty("message", out JsonElement message)
                && message.TryGetProperty("content", out JsonElement content))
            {
                string? text = content.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }
        }

        throw new UserExternalOperationException(
            "AI commit message generation failed.",
            new ExternalOperationException(innerException: new Exception($"The AI response did not contain a commit message (no choices[0].message.content):{Environment.NewLine}{Environment.NewLine}{Truncate(responseText, 1000)}")));
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : $"{value[..maxLength]}…";
}

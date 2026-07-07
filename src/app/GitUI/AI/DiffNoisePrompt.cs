using System.Text;
using System.Text.Json;

namespace GitUI.AI;

/// <summary>
/// Builds the prompts sent to Claude and parses the classification response.
/// Shared by all <see cref="IDiffNoiseClassifier"/> implementations so the two backends behave identically.
/// </summary>
internal static class DiffNoisePrompt
{
    // Bounds per request so a large diff set is split into several calls.
    // Kept moderate so many batches can run concurrently and each call stays fast.
    internal const int MaxFilesPerBatch = 15;
    internal const int MaxCharsPerBatch = 60_000;

    internal const string SystemPrompt =
        "You are a precise code-review assistant that classifies git diffs. You only ever respond with the requested JSON object.";

    internal static IEnumerable<List<DiffFileContent>> CreateBatches(IReadOnlyList<DiffFileContent> files, int maxCharsPerFile)
    {
        List<DiffFileContent> current = [];
        int currentChars = 0;
        foreach (DiffFileContent file in files)
        {
            int size = Math.Min(file.Diff.Length, maxCharsPerFile);
            if (current.Count > 0 && (current.Count >= MaxFilesPerBatch || currentChars + size > MaxCharsPerBatch))
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

    internal static string BuildUserPrompt(IReadOnlyList<DiffFileContent> batch, DiffNoiseFilterOptions options, int maxCharsPerFile)
    {
        StringBuilder user = new();
        user.AppendLine("Classify each of the following file diffs into exactly one category.");
        user.AppendLine("Respond with ONLY a JSON object, no prose, of the form:");
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
            user.AppendLine(TruncateDiff(file.Diff, maxCharsPerFile));
            user.AppendLine();
        }

        return user.ToString();
    }

    internal static string TruncateDiff(string diff, int maxCharsPerFile)
        => diff.Length <= maxCharsPerFile
            ? diff
            : diff[..maxCharsPerFile] + $"{Environment.NewLine}... [diff truncated after {maxCharsPerFile} characters] ...";

    internal static IReadOnlyList<DiffNoiseClassification> ParseClassifications(string text, IReadOnlyList<DiffFileContent> batch)
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

    internal static string ExtractJsonObject(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : "";
    }

    private static void AppendEnabledCategory(StringBuilder sb, string name, bool enabled, string description)
    {
        if (enabled)
        {
            sb.AppendLine($"- {name}: {description}");
        }
    }
}

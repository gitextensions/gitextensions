using System.Text.RegularExpressions;

namespace GitUI.AI;

/// <summary>
/// Cheap, language-aware detection of "noise" diffs that can be classified without calling the AI.
/// Everything here is intentionally conservative: it only returns a positive result when it is certain,
/// so undecided files fall through to the AI classifier (no risk of hiding real changes).
/// </summary>
internal static partial class DeterministicDiffNoise
{
    // Extensions where whitespace/indentation is NOT semantically significant, so a change that
    // disappears under `git diff -w` is safe to treat as style-only.
    // Deliberately excludes whitespace-significant languages (Python, YAML, Makefiles, F#, Haskell, ...).
    private static readonly HashSet<string> _whitespaceInsignificantExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".java", ".js", ".jsx", ".ts", ".tsx", ".mjs", ".cjs",
        ".c", ".h", ".cpp", ".hpp", ".cc", ".cxx", ".hxx",
        ".go", ".rs", ".kt", ".kts", ".swift", ".php",
        ".css", ".scss", ".less", ".json", ".xml", ".html", ".htm",
        ".sql", ".gradle", ".groovy", ".dart", ".scala",
    };

    /// <summary>
    /// Whether whitespace-only changes in this file can be treated as style noise.
    /// Returns false for whitespace-significant or unknown languages.
    /// </summary>
    internal static bool IsWhitespaceInsignificant(string path)
        => _whitespaceInsignificantExtensions.Contains(Path.GetExtension(path));

    /// <summary>
    /// Whether the unified diff contains any added/removed content lines (ignoring the file headers).
    /// Used against a <c>git diff -w</c> patch: if it has no content changes, the original change was whitespace-only.
    /// </summary>
    internal static bool DiffHasContentChanges(string? patchText)
    {
        if (string.IsNullOrEmpty(patchText))
        {
            return false;
        }

        foreach (string rawLine in patchText.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.Length == 0)
            {
                continue;
            }

            char first = line[0];
            if ((first == '+' || first == '-') && !line.StartsWith("+++", StringComparison.Ordinal) && !line.StartsWith("---", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether every added/removed line in the diff is an import statement (or a blank line),
    /// for a language with unambiguous single-line import syntax. Returns false for unsupported languages.
    /// </summary>
    internal static bool IsImportOnly(string path, string diffText)
    {
        Func<string, bool>? isImport = GetImportMatcher(path);
        if (isImport is null)
        {
            return false;
        }

        bool sawImportChange = false;
        foreach (string rawLine in diffText.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.Length == 0)
            {
                continue;
            }

            char first = line[0];
            if (first != '+' && first != '-')
            {
                continue;
            }

            if (line.StartsWith("+++", StringComparison.Ordinal) || line.StartsWith("---", StringComparison.Ordinal))
            {
                continue;
            }

            string content = line[1..];
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            if (!isImport(content))
            {
                return false;
            }

            sawImportChange = true;
        }

        return sawImportChange;
    }

    private static Func<string, bool>? GetImportMatcher(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".cs" => static line => CSharpUsingRegex().IsMatch(line),
            ".java" => static line => JavaImportRegex().IsMatch(line),
            ".py" or ".pyi" => static line => PythonImportRegex().IsMatch(line),
            ".js" or ".jsx" or ".ts" or ".tsx" or ".mjs" or ".cjs" => IsJavaScriptImport,
            _ => null
        };

    private static bool IsJavaScriptImport(string line)
        => JsImportFromRegex().IsMatch(line)
            || JsSideEffectImportRegex().IsMatch(line)
            || JsExportFromRegex().IsMatch(line)
            || JsRequireRegex().IsMatch(line);

    // C# using directive: `using X.Y;`, `using static X.Y;`, `global using X;`, `using Alias = X.Y;`.
    // Rejects using statements/declarations like `using (x)` or `using var x = ...;`.
    [GeneratedRegex(@"^\s*(global\s+)?using\s+(static\s+)?[A-Za-z_][\w.]*(\s*=\s*[A-Za-z_][\w.<>,\s]*)?\s*;\s*$")]
    private static partial Regex CSharpUsingRegex();

    [GeneratedRegex(@"^\s*import\s+(static\s+)?[A-Za-z_][\w.]*(\.\*)?\s*;\s*$")]
    private static partial Regex JavaImportRegex();

    // Python: `import x`, `import x as y`, `import x, y`, `from x import y`, `from .x import y`.
    [GeneratedRegex(@"^\s*(import\s+[A-Za-z_][\w., ]*(\s+as\s+\w+)?|from\s+[.\w]+\s+import\s+\S.*)$")]
    private static partial Regex PythonImportRegex();

    // import ... from '...';
    [GeneratedRegex("""^\s*import\s+[^;(]*\sfrom\s+['"][^'"]+['"]\s*;?\s*$""")]
    private static partial Regex JsImportFromRegex();

    // import '...'; (side-effect import)
    [GeneratedRegex("""^\s*import\s+['"][^'"]+['"]\s*;?\s*$""")]
    private static partial Regex JsSideEffectImportRegex();

    // export ... from '...';
    [GeneratedRegex("""^\s*export\s+[^;]*\sfrom\s+['"][^'"]+['"]\s*;?\s*$""")]
    private static partial Regex JsExportFromRegex();

    // const/let/var x = require('...');
    [GeneratedRegex("""^\s*(export\s+)?(const|let|var)\s+[^=]+=\s*require\(\s*['"][^'"]+['"]\s*\)\s*;?\s*$""")]
    private static partial Regex JsRequireRegex();
}

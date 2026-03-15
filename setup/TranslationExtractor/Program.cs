namespace TranslationExtractor;

/// <summary>
///  Extracts translatable strings from Git Extensions source code and produces
///  English.xlf and English.Plugins.xlf in the custom XLIFF 1.0 format,
///  replacing the runtime-reflection-based TranslationApp.
///
///  Usage: TranslationExtractor &lt;repoRoot&gt; [outputDir]
///    repoRoot  - Path to the Git Extensions repository root.
///    outputDir - Optional output directory; defaults to repoRoot/src/app/GitUI/Translation.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: TranslationExtractor <repoRoot> [outputDir]");
            return 1;
        }

        string repoRoot = Path.GetFullPath(args[0]);
        string outputDir = args.Length >= 2
            ? Path.GetFullPath(args[1])
            : Path.Combine(repoRoot, "src", "app", "GitUI", "Translation");

        if (!Directory.Exists(repoRoot))
        {
            Console.Error.WriteLine($"Repository root not found: {repoRoot}");
            return 1;
        }

        Console.WriteLine($"Repository root: {repoRoot}");
        Console.WriteLine($"Output directory: {outputDir}");

        List<TranslationEntry> mainEntries = [];
        List<TranslationEntry> pluginEntries = [];

        // Process main app source files (Externals are excluded, matching the reflection tool's UnTranslatableDLLs)
        string[] appDirs =
        [
            Path.Combine(repoRoot, "src", "app"),
        ];

        foreach (string appDir in appDirs)
        {
            if (!Directory.Exists(appDir))
            {
                continue;
            }

            ProcessDirectory(appDir, mainEntries, isPlugin: false);
        }

        // Process plugin source files
        string pluginsDir = Path.Combine(repoRoot, "src", "plugins");
        if (Directory.Exists(pluginsDir))
        {
            ProcessDirectory(pluginsDir, pluginEntries, isPlugin: true);
        }

        // Deduplicate entries (same category + id should only appear once)
        mainEntries = DeduplicateEntries(mainEntries);
        pluginEntries = DeduplicateEntries(pluginEntries);

        Console.WriteLine($"Main app entries (extracted): {mainEntries.Count} across {mainEntries.Select(e => e.Category).Distinct().Count()} categories");
        Console.WriteLine($"Plugin entries (extracted): {pluginEntries.Count} across {pluginEntries.Select(e => e.Category).Distinct().Count()} categories");

        // Write XLIFF files, merging with existing to preserve entries the static extractor can't discover
        string mainXlf = Path.Combine(outputDir, "English.xlf");
        string pluginsXlf = Path.Combine(outputDir, "English.Plugins.xlf");

        mainEntries = MergeWithExisting(mainEntries, mainXlf);
        pluginEntries = MergeWithExisting(pluginEntries, pluginsXlf);

        Console.WriteLine($"Main app entries (after merge): {mainEntries.Count} across {mainEntries.Select(e => e.Category).Distinct().Count()} categories");
        Console.WriteLine($"Plugin entries (after merge): {pluginEntries.Count} across {pluginEntries.Select(e => e.Category).Distinct().Count()} categories");

        XliffWriter.Write(mainEntries, mainXlf);
        Console.WriteLine($"Wrote: {mainXlf}");

        if (pluginEntries.Count > 0)
        {
            XliffWriter.Write(pluginEntries, pluginsXlf);
            Console.WriteLine($"Wrote: {pluginsXlf}");
        }

        return 0;
    }

    /// <summary>
    ///  Merges extracted entries with entries from the existing XLIFF file.
    ///  Any entry present in the existing file but missing from the extracted set is preserved.
    ///  Extracted entries always take precedence (their source text may have changed).
    /// </summary>
    private static List<TranslationEntry> MergeWithExisting(List<TranslationEntry> extracted, string existingXlfPath)
    {
        List<TranslationEntry> existing = XliffReader.Read(existingXlfPath);
        if (existing.Count == 0)
        {
            return extracted;
        }

        HashSet<(string Category, string Id)> extractedKeys = new(
            extracted.Select(e => (e.Category, e.Id)));

        int merged = 0;
        foreach (TranslationEntry entry in existing)
        {
            if (!extractedKeys.Contains((entry.Category, entry.Id)))
            {
                extracted.Add(entry);
                merged++;
            }
        }

        if (merged > 0)
        {
            Console.WriteLine($"  Preserved {merged} entries from existing file that static analysis could not discover");
        }

        return extracted;
    }

    private static void ProcessDirectory(string directory, List<TranslationEntry> entries, bool isPlugin)
    {
        foreach (string file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
        {
            // Skip obj and bin directories
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string sourceText;
            try
            {
                sourceText = File.ReadAllText(file);
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Warning: Could not read {file}: {ex.Message}");
                continue;
            }

            if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            {
                // Extract from Designer.cs - need the class name from the corresponding .cs file
                string? className = InferClassNameFromDesignerFile(file, sourceText);
                if (className is not null)
                {
                    entries.AddRange(DesignerExtractor.Extract(file, sourceText, className));
                }
            }
            else
            {
                // Extract TranslationString fields from regular .cs files
                entries.AddRange(TranslationStringExtractor.Extract(file, sourceText));

                // Extract translatable properties from object initializers (dynamic control creation)
                entries.AddRange(ObjectInitializerExtractor.Extract(file, sourceText));

                // Extract plugin-specific entries
                if (isPlugin)
                {
                    entries.AddRange(PluginExtractor.Extract(file, sourceText));
                }
            }
        }
    }

    private static string? InferClassNameFromDesignerFile(string designerPath, string sourceText)
    {
        // The Designer.cs file contains a partial class declaration.
        // Extract the class name from the "partial class ClassName" declaration.
        int idx = sourceText.IndexOf("partial class ", StringComparison.Ordinal);
        if (idx < 0)
        {
            return null;
        }

        idx += "partial class ".Length;
        int end = idx;
        while (end < sourceText.Length && (char.IsLetterOrDigit(sourceText[end]) || sourceText[end] == '_'))
        {
            end++;
        }

        return end > idx ? sourceText[idx..end] : null;
    }

    private static List<TranslationEntry> DeduplicateEntries(List<TranslationEntry> entries)
    {
        // Keep first occurrence of each (Category, Id) pair
        HashSet<(string, string)> seen = [];
        List<TranslationEntry> result = [];
        foreach (TranslationEntry entry in entries)
        {
            if (seen.Add((entry.Category, entry.Id)))
            {
                result.Add(entry);
            }
        }

        return result;
    }
}

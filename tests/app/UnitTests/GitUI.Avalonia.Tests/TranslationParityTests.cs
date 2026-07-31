using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Translations;

namespace GitExtensionsTests;

// parity-scaffolding: Sweeps every portable translation emitter against the WinForms English catalog.
[TestFixture]
[NonParallelizable]
public sealed class TranslationParityTests
{
    private const string EvidencePathEnvironmentVariable = "GITEXT_TRANSLATION_PARITY_REPORT";

    [AvaloniaTest]
    [Category("P1_6")]
    public void AddTranslationItems_should_match_English_xlf_for_every_twin_view()
    {
        string repositoryRoot = FindRepositoryRoot();
        IReadOnlyDictionary<TranslationKey, string> catalog = ReadCatalog(
            Path.Combine(repositoryRoot, "src", "app", "GitUI", "Translation", "English.xlf"));
        IReadOnlyList<Type> types = GetTwinViewTypes(repositoryRoot);
        TranslationCollector collector = new();
        List<string> constructionFailures = [];
        bool originalDesignMode = Design.IsDesignMode;
        SetDesignMode(true);
        try
        {
            foreach (Type type in types)
            {
                ITranslate? instance = null;
                try
                {
                    instance = Activator.CreateInstance(type) as ITranslate
                        ?? throw new InvalidOperationException($"Could not construct translatable twin '{type.FullName}'.");
                    instance.AddTranslationItems(collector);
                }
                catch (Exception exception)
                {
                    Exception root = Unwrap(exception);
                    constructionFailures.Add($"{type.FullName}: {root.GetType().Name}: {root.Message}");
                }
                finally
                {
                    if (instance is Window window)
                    {
                        window.Close();
                    }

                    instance?.Dispose();
                }
            }
        }
        finally
        {
            SetDesignMode(originalDesignMode);
        }

        TranslationItem[] emittedItems = collector.Items
            .OrderBy(item => item.Key.Category, StringComparer.Ordinal)
            .ThenBy(item => item.Key.Id, StringComparer.Ordinal)
            .ThenBy(item => item.NeutralValue, StringComparer.Ordinal)
            .ToArray();
        string[] duplicateConflicts = emittedItems
            .GroupBy(item => item.Key)
            .Where(group => group.Select(item => NormalizeText(item.NeutralValue)).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(group => $"{group.Key}: {string.Join(" | ", group.Select(item => Quote(item.NeutralValue)).Distinct(StringComparer.Ordinal))}")
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] missingKeys = emittedItems
            .Select(item => item.Key)
            .Distinct()
            .Where(key => !catalog.ContainsKey(key))
            .Select(key => key.ToString())
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] sourceMismatches = emittedItems
            .DistinctBy(item => item.Key)
            .Where(item => catalog.TryGetValue(item.Key, out string? source)
                           && NormalizeText(source) != NormalizeText(item.NeutralValue))
            .Select(item => $"{item.Key}: emitted {Quote(item.NeutralValue)}, catalog {Quote(catalog[item.Key])}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        WriteEvidence(
            types.Count,
            emittedItems.Select(item => item.Key).Distinct().Count(),
            constructionFailures,
            missingKeys,
            sourceMismatches,
            duplicateConflicts);

        constructionFailures.Should().BeEmpty("every translatable twin view must be inspectable in one sweep");
        missingKeys.Should().BeEmpty("Avalonia must not emit keys absent from the WinForms English catalog");
        sourceMismatches.Should().BeEmpty("an existing key must retain its established neutral source text");
        duplicateConflicts.Should().BeEmpty("one translation key cannot have multiple neutral source strings");
    }

    private static string FindRepositoryRoot([CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from '{startPath}'.");
    }

    private static IReadOnlyList<Type> GetTwinViewTypes(string repositoryRoot)
    {
        string twinRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia");
        string originalRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI");
        string assemblyPath = Path.Combine(AppContext.BaseDirectory, "GitUI.Avalonia.dll");
        Assembly assembly = Assembly.LoadFrom(assemblyPath);
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        return Directory.EnumerateFiles(twinRoot, "*.axaml", SearchOption.AllDirectories)
            .Where(path => File.Exists(Path.Combine(
                originalRoot,
                Path.ChangeExtension(Path.GetRelativePath(twinRoot, path), ".Designer.cs"))))
            .Select(path => XDocument.Load(path, LoadOptions.None).Root?.Attribute(xaml + "Class")?.Value)
            .Where(className => !string.IsNullOrWhiteSpace(className))
            .Select(className => assembly.GetType(className!, throwOnError: true)!)
            .Where(type => typeof(ITranslate).IsAssignableFrom(type))
            .Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeText(string value) =>
        value.ReplaceLineEndings("\n");

    private static string Quote(string value) =>
        JsonSerializer.Serialize(NormalizeText(value));

    private static IReadOnlyDictionary<TranslationKey, string> ReadCatalog(string path)
    {
        XDocument document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        return document.Root!.Elements("file")
            .SelectMany(file => file.Element("body")!.Elements("trans-unit")
                .Select(unit => new KeyValuePair<TranslationKey, string>(
                    new TranslationKey(
                        file.Attribute("original")!.Value,
                        unit.Attribute("id")!.Value),
                    unit.Element("source")!.Value)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private static void SetDesignMode(bool value)
    {
        PropertyInfo property = typeof(Design).GetProperty(
            nameof(Design.IsDesignMode),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Avalonia's design-mode property could not be resolved.");
        property.SetValue(null, value);
    }

    private static Exception Unwrap(Exception exception)
    {
        while (exception is TargetInvocationException { InnerException: not null }
               or TypeInitializationException { InnerException: not null })
        {
            exception = exception.InnerException;
        }

        return exception;
    }

    private static void WriteEvidence(
        int typeCount,
        int emittedKeyCount,
        IReadOnlyCollection<string> constructionFailures,
        IReadOnlyCollection<string> missingKeys,
        IReadOnlyCollection<string> sourceMismatches,
        IReadOnlyCollection<string> duplicateConflicts)
    {
        string? path = Environment.GetEnvironmentVariable(EvidencePathEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(
            fullPath,
            JsonSerializer.Serialize(
                new
                {
                    schemaVersion = 1,
                    typeCount,
                    emittedKeyCount,
                    constructionFailures,
                    missingKeys,
                    sourceMismatches,
                    duplicateConflicts,
                },
                new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
    }

    private sealed class TranslationCollector : ITranslation
    {
        public List<TranslationItem> Items { get; } = [];

        public void AddTranslationItem(string category, string item, string property, string neutralValue)
            => Items.Add(new TranslationItem(new TranslationKey(category, $"{item}.{property}"), neutralValue));

        public string? TranslateItem(
            string category,
            string item,
            string property,
            Func<string?> provideDefaultValue)
            => provideDefaultValue();
    }

    private readonly record struct TranslationItem(TranslationKey Key, string NeutralValue);

    private readonly record struct TranslationKey(string Category, string Id)
    {
        public override string ToString() => $"{Category}/{Id}";
    }
}

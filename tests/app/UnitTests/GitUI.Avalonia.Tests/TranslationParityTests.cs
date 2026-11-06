using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AwesomeAssertions.Execution;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.LeftPanel;
using NSubstitute;

namespace GitExtensionsTests;

// parity-scaffolding: Sweeps every portable translation emitter against the WinForms English catalog.
[TestFixture]
[NonParallelizable]
public sealed class TranslationParityTests
{
    private const string EvidencePathEnvironmentVariable = "GITEXT_TRANSLATION_PARITY_REPORT";

    [AvaloniaTest]
    public void Original_tooltip_and_column_keys_should_receive_existing_German_translations()
    {
        using TranslationTestScope translationScope = new("German");
        using FileViewer viewer = new();
        using FormPush form = new();

        object? toolTip = ToolTip.GetTip(viewer.FindControl<Control>("nextChangeButton")!);
        (toolTip is TextBlock text ? text.Text : toolTip as string).Should().Be("Nächste Änderung");
        form.FindControl<GitUI.Compat.WinFormsControls.ColumnHeader>("ForceColumn")!
            .Header.Should().Be("Erzwingen");
    }

    [AvaloniaTest]
    [TestCase("")]
    [TestCase("English")]
    public void Context_menu_captions_should_render_original_mnemonics_and_literal_ampersands(string language)
    {
        using TranslationTestScope translationScope = new(language);
        using RepoObjectsTree tree = new();
        using FileStatusList files = new();
        Dictionary<string, object> fields = TranslationUtils.GetObjFields(tree, "$this")
            .ToDictionary(item => item.Name, item => item.Item);
        (MenuItem Item, string Expected)[] captions =
        [
            ((MenuItem)fields["copyContextMenuItem"], "Copy to clipboard"),
            ((MenuItem)fields["_sortByContextMenuItem"], "Sort by"),
            ((MenuItem)fields["_sortOrderContextMenuItem"], "Sort order"),
            ((MenuItem)fields["mnubtnFetchCreateBranch"], "Fetch & Create Branch"),
            ((MenuItem)fields["mnubtnFetchRebase"], "Fetch & Rebase"),
            ((MenuItem)fields["mnubtnPullFromRemoteBranch"], "Fetch & Merge (Pull)"),
            ((MenuItem)fields["mnubtnRemoteBranchFetchAndCheckout"], "Fetch & Checkout"),
            (files.GetTestAccessor().SortByContextMenuItem, "Sort and group by"),
        ];
        Button owner = new() { Content = "Menu" };
        Window window = new() { Width = 400, Height = 300, Content = owner };
        MenuFlyout flyout = new();
        List<object> evidence = [];
        List<(string Actual, string Expected)> rendered = [];
        try
        {
            foreach ((MenuItem item, _) in captions)
            {
                if (item.Parent is ItemsControl parent)
                {
                    parent.Items.Remove(item);
                }

                item.IsVisible = true;
                flyout.Items.Add(item);
            }

            window.Show();
            flyout.ShowAt(owner);
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame()?.Dispose();
            flyout.IsOpen.Should().BeTrue();
            foreach ((MenuItem item, string expected) in captions)
            {
                AccessText? text = item.GetVisualDescendants().OfType<AccessText>().SingleOrDefault();
                text?.Measure(Size.Infinity);
                string actual = text is null ? "<not rendered>" : string.Concat(text.TextLayout.TextLines.SelectMany(line => line.TextRuns)
                    .OfType<ShapedTextRun>().Select(run => run.Text.ToString()));
                rendered.Add((actual, expected));
                evidence.Add(new { header = item.Header, actual, expected });
            }

            string? report = Environment.GetEnvironmentVariable("GITEXT_MNEMONIC_REPORT");
            if (!string.IsNullOrEmpty(report))
            {
                File.WriteAllText(report + (language.Length == 0 ? ".neutral.json" : ".English.json"),
                    JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
            }

            using AssertionScope scope = new();
            foreach ((string actual, string expected) in rendered)
            {
                actual.Should().Be(expected, "the opened menu must render the original caption in {0}", language);
            }
        }
        finally
        {
            flyout.Hide();
            window.Close();
        }
    }

    [Test]
    public void Nested_control_root_should_not_publish_the_owner_window_title_key()
    {
        Button nestedControl = new() { Content = "Nested control" };
        ITranslation translation = Substitute.For<ITranslation>();

        AvaloniaTranslationUtils.AddTranslationItemsFromFields("OwnerWindow", nestedControl, translation);

        translation.DidNotReceive().AddTranslationItem(
            "OwnerWindow",
            "$this",
            "Text",
            Arg.Any<string>());
    }

    [AvaloniaTest]
    [Category("P1_6")]
    public void AddTranslationItems_should_match_English_xlf_for_every_twin_view()
    {
        using TranslationTestScope translationScope = new("");
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
            .Distinct()
            .Where(item => !catalog.ContainsKey(item.Key))
            .Select(item => $"{item.Key}: emitted {Quote(item.NeutralValue)}")
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] sourceMismatches = emittedItems
            .Distinct()
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
        // Pin the complete conflict text so new, changed, or resolved conflicts require review.
        using JsonDocument known = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            repositoryRoot, "tests", "app", "UnitTests", "GitUI.Avalonia.Tests", "TranslationParityKnownConflicts.json")));
        missingKeys.Should().BeEquivalentTo(ReadKnown("missingKeys"));
        sourceMismatches.Should().BeEquivalentTo(ReadKnown("sourceMismatches"));
        duplicateConflicts.Should().BeEquivalentTo(ReadKnown("duplicateConflicts"));

        string[] ReadKnown(string name) => known.RootElement.GetProperty(name).EnumerateArray()
            .Select(item => item.GetString()!).ToArray();
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

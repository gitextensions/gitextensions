using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Columns;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Verifies manifest-only factory observations against actual portable Date-column consumers.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CaptureConstructionObservationTests
{
    private const string ConstructionFailureMessage = "The bounded constructor probe failed before returning a control.";
    private static readonly MethodInfo SettingsContainerSetter = typeof(AppSettings).GetProperty(
            nameof(AppSettings.SettingsContainer), BindingFlags.Public | BindingFlags.Static)?.GetSetMethod(nonPublic: true)
        ?? throw new InvalidOperationException("The existing settings container setter must exist.");
    private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))!;
    private static readonly PropertyInfo HelperSettings = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))!;
    private readonly Dictionary<string, string?> _environment = new(StringComparer.Ordinal);
    private DistributedSettings _originalSettings = null!;
    private Lazy<string?> _originalApplicationDataPath = null!;
    private GitExtSettingsCache _settingsCache = null!;
    private string _settingsDirectory = null!;
    private CultureInfo _culture = null!;
    private CultureInfo _uiCulture = null!;
    private ThemeSettings _theme = null!;
    private ThemeSettings _helperTheme = null!;
    private ThemeVariant? _variant;
    private WinFormsShims.SystemColorMode _colorMode;
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
        _culture = CultureInfo.CurrentCulture;
        _uiCulture = CultureInfo.CurrentUICulture;
        _originalSettings = AppSettings.SettingsContainer;
        _originalApplicationDataPath = AppSettings.GetTestAccessor().ApplicationDataPath;
        _theme = ThemeModule.Settings;
        _helperTheme = (ThemeSettings)HelperSettings.GetValue(null)!;
        _variant = Application.Current!.RequestedThemeVariant;
        _colorMode = WinFormsShims.Application.SystemColorMode;
        _settingsDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ConstructionClock-{Guid.NewGuid():N}");
        _environment.Clear();
        try
        {
            Directory.CreateDirectory(_settingsDirectory);
            _settingsCache = new GitExtSettingsCache(Path.Combine(_settingsDirectory, AppSettings.SettingsFileName), autoSave: false);
            // UsingContainer holds its original cache lock across a synchronous callback;
            // this async fixture borrows only the reference and restores its exact identity.
            SettingsContainerSetter.Invoke(null, [new DistributedSettings(null, _settingsCache, SettingLevel.Unknown)]);
            AppSettings.GetTestAccessor().ApplicationDataPath = new Lazy<string?>(() => _settingsDirectory);
            foreach (string key in new[] { "GITEXT_CAPTURE_PARITY_REPOSITORY", "GITEXT_CAPTURE_PARITY_DETERMINISTIC_REPOSITORY" })
            {
                _environment[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, null);
            }

            AppSettings.Font = new WinFormsShims.Font("Segoe UI", 9);
            AppSettings.ShowDateColumn = true;
            AppSettings.ShowAvailableDiffTools = false;
            AppSettings.RevisionGraphShowArtificialCommits = false;
            AppSettings.BranchFilterEnabled.Value = false;
            AppSettings.ShowCurrentBranchOnly.Value = false;
            AppSettings.ThemeId = ThemeId.DefaultLight;
            AppSettings.ThemeVariations = [];
            AppSettings.UseSystemVisualStyle = true;
            AvaloniaThemeSettings.ApplyAppSettings();
        }
        catch
        {
            RestoreFixture();
            throw;
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            RestoreFixture();
        }
    }

    [AvaloniaTest]
    [TestCase("en-US", false, 9, 0)]
    [TestCase("sr-Latn-RS", false, 11, 3)]
    [TestCase("en-US", true, 9, 0)]
    [TestCase("sr-Latn-RS", true, 14, 12)]
    public void Actual_Date_provider_should_retain_observed_width_without_replacing_its_private_clock(
        string culture, bool relativeDate, int points, int styleFlags)
    {
        WinFormsShims.FontStyle style = styleFlags switch
        {
            0 => WinFormsShims.FontStyle.Regular,
            3 => WinFormsShims.FontStyle.Bold | WinFormsShims.FontStyle.Italic,
            12 => WinFormsShims.FontStyle.Underline | WinFormsShims.FontStyle.Strikeout,
            _ => throw new ArgumentOutOfRangeException(nameof(styleFlags)),
        };
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
        AppSettings.Font = new WinFormsShims.Font("Segoe UI", points, style);
        WinFormsShims.Font sourceFont = AppSettings.Font;
        sourceFont.Size.Should().Be(points);
        sourceFont.Bold.Should().Be(style.HasFlag(WinFormsShims.FontStyle.Bold));
        sourceFont.Italic.Should().Be(style.HasFlag(WinFormsShims.FontStyle.Italic));
        if (styleFlags == 12)
        {
            sourceFont.Style.HasFlag(WinFormsShims.FontStyle.Underline).Should().BeFalse(
                "the source settings font serialization retains only bold and italic");
            sourceFont.Style.HasFlag(WinFormsShims.FontStyle.Strikeout).Should().BeFalse(
                "the source settings font serialization retains only bold and italic");
        }

        AppSettings.RelativeDate = relativeDate;
        CaptureConstructionClockSample before = ParityScreenshotTests.SampleConstructionClock();
        RevisionGridControl grid = new();
        CaptureConstructionClockSample after = ParityScreenshotTests.SampleConstructionClock();
        try
        {
            DateColumnProvider provider = grid.ColumnProviders.OfType<DateColumnProvider>().Single();
            IReadOnlyList<CaptureConstructionDateColumn> columns = ParityScreenshotTests.ReadConstructionDateColumns(grid);
            CaptureConstructionDateColumn column = columns.Should().ContainSingle().Subject;
            column.Owner.Should().Be("$");
            column.Provider.Should().Be(typeof(DateColumnProvider).FullName);
            column.ColumnIndex.Should().Be(provider.Index);
            column.Width.Should().Be(provider.Column.Width.Value).And.BeGreaterThan(0);
            column.Unit.Should().Be(CaptureConstructionWidthUnit.DeviceIndependentPixel);
            column.RenderScale.Should().Be(1, "an unattached provider has no captured top-level scale");
            column.Dpi.Should().Be(new CaptureDpi { X = 96, Y = 96 });
            before.Context.CultureName.Should().Be(culture);
            before.Context.UICultureName.Should().Be("en-GB");
            before.Context.RelativeDate.Should().Be(relativeDate);
            before.Context.ConfiguredFont.Should().Be(new CaptureConstructionFont
            {
                Family = sourceFont.Name,
                SizePoints = sourceFont.Size,
                Bold = sourceFont.Bold,
                Italic = sourceFont.Italic,
                Underline = sourceFont.Style.HasFlag(WinFormsShims.FontStyle.Underline),
                Strikeout = sourceFont.Style.HasFlag(WinFormsShims.FontStyle.Strikeout),
            });
            CaptureConstructionObservation observation = CaptureConstructionClock.Create(before, after, columns, []);
            observation.BeforeFactory.Should().BeSameAs(before);
            observation.AfterFactory.Should().BeSameAs(after);
            observation.AfterFactoryDateColumns.Should().Equal(columns);
            observation.BeforeImageDateColumns.Should().BeEmpty();

            // A post-factory header-formatting fixture is not the Date provider's constructor input.
            using CaptureFixtureClock formatterClock = CaptureFixtureClock.Apply([grid], new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc));
            ParityScreenshotTests.ReadConstructionDateColumns(grid).Should().Equal(columns);
        }
        finally
        {
            grid.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public void Date_column_observation_should_keep_outer_owner_identity_and_ignore_other_controls()
    {
        RevisionGridControl grid = new() { Name = "RevisionGrid" };
        Border root = new() { Child = grid };
        try
        {
            CaptureConstructionDateColumn named = ParityScreenshotTests.ReadConstructionDateColumns(root).Should().ContainSingle().Subject;
            named.Owner.Should().Be("RevisionGrid");
            grid.Name = string.Empty;
            CaptureConstructionDateColumn unnamed = ParityScreenshotTests.ReadConstructionDateColumns(root).Should().ContainSingle().Subject;
            unnamed.Owner.Should().Be(typeof(RevisionGridControl).FullName);
            unnamed.Width.Should().Be(named.Width);
            grid.Name = " ";
            ParityScreenshotTests.ReadConstructionDateColumns(root).Should().ContainSingle()
                .Subject.Owner.Should().Be(typeof(RevisionGridControl).FullName);
            ParityScreenshotTests.ReadConstructionDateColumns(new TextBlock()).Should().BeEmpty();
        }
        finally
        {
            grid.CancelBackgroundTasks();
            root.Child = null;
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Actual_capture_consumer_should_emit_manifest_only_constructor_observation(bool unsupported)
    {
        AppSettings.RelativeDate = false;
        CaptureStatePlan state = new()
        {
            Id = unsupported ? "construction.unsupported" : "normal",
            Kind = unsupported ? CaptureStateKind.Focus : CaptureStateKind.Normal,
            TargetField = unsupported ? "missingConstructionProbeTarget" : null,
        };
        string evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_CONSTRUCTION_CLOCK_EVIDENCE")
            ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "ConstructionClockEvidence");
        string output = Path.Combine(Path.GetFullPath(evidenceRoot), $"{TestContext.CurrentContext.Test.ID}-{Guid.NewGuid():N}");
        DateTime fixtureNowUtc = new(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        CaptureSetManifest manifest = await ParityScreenshotTests.CaptureConstructionProbeAsync(state, fixtureNowUtc, output);
        string manifestPath = Path.Combine(output, "manifest.json");
        TestContext.AddTestAttachment(manifestPath);
        await TestContext.Progress.WriteLineAsync($"Construction observation evidence: {manifestPath}");
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
        CaptureSetManifest retained = JsonSerializer.Deserialize<CaptureSetManifest>(File.ReadAllText(manifestPath), options)!;
        retained.Should().BeEquivalentTo(manifest);
        CaptureManifestEntry entry = retained.Captures.Should().ContainSingle().Subject;
        entry.Status.Should().Be(unsupported ? CaptureStateStatus.Unsupported : CaptureStateStatus.Captured, entry.Note ?? string.Empty);
        entry.ConstructionObservation.Should().NotBeNull();
        CaptureConstructionObservation observation = entry.ConstructionObservation!;
        observation.Boundary.Should().Be("rootFactory");
        observation.BeforeFactory.ObservedAtUtc.Should().NotBe(fixtureNowUtc);
        observation.AfterFactory.ObservedAtUtc.Should().NotBe(fixtureNowUtc);
        CaptureConstructionDateColumn afterFactory = observation.AfterFactoryDateColumns.Should().ContainSingle().Subject;
        afterFactory.Owner.Should().Be("$");
        afterFactory.Provider.Should().Be(typeof(DateColumnProvider).FullName);
        afterFactory.Width.Should().BeGreaterThan(0);
        afterFactory.Unit.Should().Be(CaptureConstructionWidthUnit.DeviceIndependentPixel);
        if (unsupported)
        {
            observation.BeforeImageDateColumns.Should().BeEmpty("image acquisition was not reached");
            entry.Note.Should().Contain(state.TargetField!);
            entry.ImageFile.Should().BeNull();
            entry.TreeFile.Should().BeNull();
            return;
        }

        CaptureConstructionDateColumn beforeImage = observation.BeforeImageDateColumns.Should().ContainSingle().Subject;
        beforeImage.Width.Should().Be(afterFactory.Width, "capture must not replace the actual source-created Date width");
        beforeImage.ColumnIndex.Should().Be(afterFactory.ColumnIndex);
        string treePath = Path.Combine(output, entry.TreeFile!);
        string treeJson = File.ReadAllText(treePath);
        CaptureDocument tree = CaptureJson.Deserialize(treeJson);
        CaptureColumn date = Flatten(tree.Surfaces[0].Root).SelectMany(node => node.Columns)
            .Single(column => column.Index == beforeImage.ColumnIndex);
        date.WidthDip.Should().Be(decimal.Round((decimal)beforeImage.Width, 4));
        treeJson.Should().NotContain("constructionObservation").And.NotContain("observedAtUtc").And.NotContain("observedAtLocal");
        CaptureJson.Serialize(tree).Should().Be(treeJson, "manifest-only observations cannot alter the deterministic tree contract");
        File.ReadAllText(treePath).Should().Be(treeJson);
        File.Exists(Path.Combine(output, entry.ImageFile!)).Should().BeTrue();
        TestContext.AddTestAttachment(treePath);
        TestContext.AddTestAttachment(Path.Combine(output, entry.ImageFile!));
    }

    [AvaloniaTest]
    public async Task Failed_factory_attempt_should_retain_real_endpoints_without_inventing_Date_columns()
    {
        string evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_CONSTRUCTION_CLOCK_EVIDENCE")
            ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "ConstructionClockEvidence");
        string output = Path.Combine(Path.GetFullPath(evidenceRoot), $"{TestContext.CurrentContext.Test.ID}-{Guid.NewGuid():N}");
        DateTime fixtureNowUtc = new(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        CaptureSetManifest manifest = await ParityScreenshotTests.CaptureConstructionProbeAsync(
            new CaptureStatePlan { Id = "construction.failed", Kind = CaptureStateKind.Normal },
            fixtureNowUtc, output, typeof(ThrowingCaptureControl));
        string manifestPath = Path.Combine(output, "manifest.json");
        TestContext.AddTestAttachment(manifestPath);
        await TestContext.Progress.WriteLineAsync($"Construction observation evidence: {manifestPath}");
        CaptureManifestEntry entry = manifest.Captures.Should().ContainSingle().Subject;
        entry.Status.Should().Be(CaptureStateStatus.Failed);
        entry.Note.Should().Contain(ConstructionFailureMessage).And.Contain(nameof(TargetInvocationException));
        entry.ImageFile.Should().BeNull();
        entry.TreeFile.Should().BeNull();
        entry.ConstructionObservation.Should().NotBeNull();
        CaptureConstructionObservation observation = entry.ConstructionObservation!;
        observation.BeforeFactory.ObservedAtUtc.Should().NotBe(fixtureNowUtc);
        observation.AfterFactory.ObservedAtUtc.Should().NotBe(fixtureNowUtc);
        observation.AfterFactoryDateColumns.Should().BeEmpty("the factory did not return a Date provider to observe");
        observation.BeforeImageDateColumns.Should().BeEmpty("image acquisition was not reached");
        using JsonDocument retained = JsonDocument.Parse(File.ReadAllText(manifestPath));
        retained.RootElement.GetProperty("captures")[0].GetProperty("constructionObservation")
            .GetProperty("afterFactoryDateColumns").GetArrayLength().Should().Be(0);
        Directory.GetFiles(output, "*.tree.json", SearchOption.AllDirectories).Should().BeEmpty();
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
        => new[] { root }.Concat(root.Children.SelectMany(Flatten));

    private void RestoreFixture()
    {
        try
        {
            foreach ((string key, string? value) in _environment)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiCulture;
            // Restore theme notifications while the disposable settings container is still active.
            Application.Current!.RequestedThemeVariant = _variant;
            ModuleSettings.SetValue(null, _theme);
            WinFormsShims.Application.SystemColorMode = _colorMode;
            AvaloniaThemeResources.Apply(Application.Current!, _theme);
            ColorHelper.ThemeSettings = _helperTheme;
        }
        finally
        {
            SettingsContainerSetter.Invoke(null, [_originalSettings]);
            AppSettings.GetTestAccessor().ApplicationDataPath = _originalApplicationDataPath;
            _settingsCache?.Dispose();
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
            TestDirectory.Delete(_settingsDirectory);
        }
    }

    private sealed class ThrowingCaptureControl : Control
    {
        public ThrowingCaptureControl()
            => throw new InvalidOperationException(ConstructionFailureMessage);
    }
}

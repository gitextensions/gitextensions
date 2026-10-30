using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

/// <summary>
///  Exercises the source's single message-tooltip owner with actual rendered pointer routes.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridMessageToolTipTests
{
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    /// <summary>
    ///  Repeated moves and refreshes preserve truncated messages, bodies and ref details without stealing the cursor.
    /// </summary>
    [AvaloniaTest]
    [TestCase("light", false)]
    [TestCase("light", true)]
    [TestCase("dark", false)]
    [TestCase("dark", true)]
    [TestCase("custom", false)]
    [TestCase("custom", true)]
    public async Task Message_tooltips_should_survive_pointer_reentry_and_real_row_refresh(string theme, bool tooltips)
    {
        using ThemeScope themeScope = new(theme);
        using SettingsScope settings = new(tooltips);
        GridFixture fixture = new();
        List<PointerObservation> observations = [];
        NotesContentObservation? notesContent = null;
        bool completed = false;
        ExceptionDispatchInfo? failure = null;
        try
        {
            MessageColumnProvider provider = fixture.Grid.ColumnProviders.OfType<MessageColumnProvider>().Single();
            TextBlock subject = fixture.Subject(fixture.Plain);
            subject.TextLayout.TextLines.Any(line => line.HasCollapsed || line.HasOverflowed).Should().BeTrue(
                "the plain message must exercise the source's actual truncation fallback");
            provider.TryGetToolTip(fixture.Plain, out _).Should().BeFalse();
            string? plainTip = tooltips ? fixture.Plain.Subject : null;
            MoveSubject(fixture.Plain, "plain-first", plainTip);
            MoveSubject(fixture.Plain, "plain-repeat", plainTip, alternate: true);
            fixture.LeaveMessages();
            MoveSubject(fixture.Plain, "plain-reenter", plainTip);
            fixture.Grid.RefreshRealizedRows();
            MoveSubject(fixture.Plain, "plain-after-refresh", plainTip, alternate: true);

            string? bodyTip = tooltips ? SourceSummary(fixture.Multiline) : null;
            MoveSubject(fixture.Multiline, "multiline-first", bodyTip);
            MoveSubject(fixture.Multiline, "multiline-repeat", bodyTip, alternate: true);
            fixture.Grid.RefreshRealizedRows();
            MoveSubject(fixture.Multiline, "multiline-after-refresh", bodyTip);

            string? refTip = tooltips
                ? "[main]" + Environment.NewLine + GitUI.TranslatedStrings.IsLocalBranch
                    + Environment.NewLine + Environment.NewLine + SourceSummary(fixture.Referenced)
                : null;
            if (refTip is not null)
            {
                refTip.Should().Contain("Full referenced details");
            }

            MoveRef("ref-first", refTip);
            MoveRef("ref-repeat", refTip, alternate: true);
            fixture.LeaveMessages();
            MoveRef("ref-reenter", refTip);
            fixture.Grid.RefreshRealizedRows();
            MoveRef("ref-after-refresh", refTip, alternate: true);
            MoveSubject(fixture.Plain, "plain-after-ref", plainTip);

            // Keep pointer routes in the existing separate-notes mode: inline CR text
            // has an independent SingleLine paint mismatch. Its tooltip content is still covered.
            GitExtSettingsCache cache = AppSettings.SettingsContainer.SettingsCache;
            string key = AppSettings.ShowGitNotesColumn.FullPath;
            string? originalMode = cache.TryGetValue(key, out string? raw) ? raw : null;
            try
            {
                AppSettings.ShowGitNotesColumn.Value = false;
                provider.ApplySettings();
                string ordinaryExpected = SourceSummary(fixture.Referenced, includeNotes: true);
                bool ordinaryFound = provider.TryGetToolTip(fixture.Referenced, out string? ordinaryActual);
                string? highlightedExpected = tooltips
                    ? "[main]" + Environment.NewLine + GitUI.TranslatedStrings.IsLocalBranch
                        + Environment.NewLine + Environment.NewLine + ordinaryExpected
                    : null;
                bool highlightedFound = provider.TryGetToolTip(fixture.Referenced, fixture.Referenced.Refs.Single(), out string? highlightedActual);
                notesContent = new NotesContentObservation("actualMessageColumnProviderWithoutRenderingInlineNotes",
                    ordinaryFound, ordinaryActual, ordinaryExpected, highlightedFound, highlightedActual, highlightedExpected);
                ordinaryFound.Should().BeTrue("the ordinary provider exposes its loaded content independently of the central tooltip gate");
                ordinaryActual.Should().Be(ordinaryExpected).And.Contain("Reference note");
                highlightedFound.Should().Be(tooltips);
                highlightedActual.Should().Be(highlightedExpected);
            }
            finally
            {
                cache.SetValue(key, originalMode);
                provider.ApplySettings();
            }

            completed = true;
        }
        catch (Exception exception)
        {
            failure = ExceptionDispatchInfo.Capture(exception);
        }
        finally
        {
            TryCleanup(() => fixture.Record(theme, tooltips, completed, observations, notesContent), ref failure);
            TryCleanup(fixture.Dispose, ref failure);
            TryCleanup(() => Dispatcher.UIThread.RunJobs(), ref failure);
            try
            {
                using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
                await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
            }
            catch (Exception exception)
            {
                PreserveFailure(exception, ref failure);
            }

            TryCleanup(() => Dispatcher.UIThread.RunJobs(), ref failure);
        }

        failure?.Throw();
        return;

        void MoveSubject(GitRevision revision, string phase, string? expected, bool alternate = false)
        {
            fixture.Settle();
            TextBlock text = fixture.Subject(revision);
            Point point = text.TranslatePoint(new Point(text.Bounds.Width / (alternate ? 3 : 2), text.Bounds.Height / 2), fixture.Window)
                ?? throw new AssertionException("The actual message subject must be attached.");
            Move(revision, phase, point, expected, hand: false);
        }

        void MoveRef(string phase, string? expected, bool alternate = false)
        {
            fixture.Settle();
            RevisionGridRefRenderer.RefLabelControl label = fixture.Message(fixture.Referenced).GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>().Single();
            Point local = new(label.CapsuleBounds.Left + (label.CapsuleBounds.Width / (alternate ? 3 : 2)), label.CapsuleBounds.Center.Y);
            label.Contains(local).Should().BeTrue();
            Point point = label.TranslatePoint(local, fixture.Window)
                ?? throw new AssertionException("The actual ref capsule must be attached.");
            Move(fixture.Referenced, phase, point, expected, hand: true);
        }

        void Move(GitRevision revision, string phase, Point point, string? expected, bool hand)
        {
            Control cell = fixture.Message(revision);
            IInputElement? rawHit = fixture.Window.InputHitTest(point);
            bool ownsCell = rawHit is Visual visual && visual.GetSelfAndVisualAncestors().Contains(cell);
            fixture.Window.MouseMove(point);
            fixture.Settle();
            string? actual = ToolTip.GetTip(cell)?.ToString();
            object cursor = typeof(MessageColumnProvider).GetField("HandCursor", BindingFlags.Static | BindingFlags.NonPublic)!
                .GetValue(null) ?? throw new AssertionException("The original ref cursor must exist.");
            bool expectedCursor = hand ? ReferenceEquals(cell.Cursor, cursor) : cell.Cursor is null;
            observations.Add(new PointerObservation(phase, revision.Guid, point, cell.Bounds,
                rawHit?.GetType().FullName, ownsCell, actual, expected, hand ? "refHand" : "default", expectedCursor));
            ownsCell.Should().BeTrue("the input must traverse the actual realized message cell");
            actual.Should().Be(expected, phase + " must retain the source's tooltip content through the complete pointer route");
            expectedCursor.Should().BeTrue("only actionable ref labels use the original hand cursor");
        }
    }

    private static string SourceSummary(GitRevision revision, bool includeNotes = false)
    {
        string body = includeNotes ? UIExtensions.FormatBodyAndNotes(revision.Body!, revision.Notes) : revision.Body!;
        string summary = new GitRevisionSummaryBuilder().BuildSummary(body)
            ?? throw new AssertionException("The loaded source commit must have a summary.");
        return revision.Refs.Count == 0 ? summary : summary + Environment.NewLine + Environment.NewLine + "[main]" + Environment.NewLine;
    }

    private static void TryCleanup(Action cleanup, ref ExceptionDispatchInfo? failure)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception)
        {
            PreserveFailure(exception, ref failure);
        }
    }

    private static void PreserveFailure(Exception exception, ref ExceptionDispatchInfo? failure)
    {
        if (failure is null)
        {
            failure = ExceptionDispatchInfo.Capture(exception);
        }
        else
        {
            TestContext.Error.WriteLine(exception);
        }
    }

    private sealed record PointerObservation(string Phase, string Revision, Point Point, Rect Bounds,
        string? HitType, bool HitOwnsMessageCell, string? Actual, string? Expected, string Cursor, bool ExpectedCursor);

    private sealed record NotesContentObservation(string CaptureMethod, bool OrdinaryFound, string? OrdinaryActual,
        string OrdinaryExpected, bool HighlightedFound, string? HighlightedActual, string? HighlightedExpected);

    private sealed class GridFixture : IDisposable
    {
        public GridFixture()
        {
            IGitModule module = Substitute.For<IGitModule>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.BrowseRepo.Returns(Substitute.For<IBrowseRepo>());
            commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            Grid = new RevisionGridControl(Substitute.For<ICommitDataManager>()) { UICommandsSource = source };
            Referenced.Refs = [new GitRef(module, Referenced.ObjectId, "refs/heads/main")];
            Grid.GetTestAccessor().SetRevisions([Plain, Multiline, Referenced]);
            DockPanel host = new();
            DockPanel.SetDock(FocusTarget, Dock.Bottom);
            host.Children.Add(FocusTarget);
            host.Children.Add(Grid);
            Window = new Window { Width = 900, Height = 220, Content = host };
            Window.Show();
            Window.Activate();
            Settle();
        }

        public RevisionGridControl Grid { get; }

        public Window Window { get; }

        public Button FocusTarget { get; } = new() { Content = "Outside the message cells" };

        public GitRevision Plain { get; } = Revision(1, new string('W', 300), multiline: false);

        public GitRevision Multiline { get; } = Revision(2, "Multiline commit", multiline: true, "Full multiline details");

        public GitRevision Referenced { get; } = Revision(3, "Referenced commit", multiline: true, "Full referenced details", "Reference note");

        public Control Message(GitRevision revision)
            => Grid.GetVisualDescendants().OfType<Control>().Single(control => control.Classes.Contains("revision-message-cell")
                && ReferenceEquals(control.DataContext, revision));

        public TextBlock Subject(GitRevision revision)
            => Message(revision).GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("revision-subject"));

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual message input scene must render.");
        }

        public void LeaveMessages()
        {
            Point point = FocusTarget.TranslatePoint(new Point(FocusTarget.Bounds.Width / 2, FocusTarget.Bounds.Height / 2), Window)
                ?? throw new AssertionException("The actual outside input target must be attached.");
            Window.MouseMove(point);
            Settle();
        }

        public void Record(string theme, bool tooltips, bool completed, IEnumerable<PointerObservation> observations, NotesContentObservation? notesContent)
        {
            string? evidence = Environment.GetEnvironmentVariable("GITEXT_MESSAGE_TOOLTIP_EVIDENCE");
            if (string.IsNullOrWhiteSpace(evidence))
            {
                return;
            }

            Settle();
            string directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "message-input");
            File.Exists(path + ".png").Should().BeFalse("actual pointer evidence must not overwrite another run");
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual message evidence frame must render.");
            frame.Save(path + ".png", PngBitmapEncoderOptions.Default);
            File.WriteAllText(path + ".json", JsonSerializer.Serialize(new
            {
                test = TestContext.CurrentContext.Test.FullName,
                theme,
                tooltips,
                completed,
                captureMethod = "actualAvaloniaHeadlessRevisionGridMessageWindow",
                frame.PixelSize,
                Window.RenderScaling,
                observations,
                pointerNotesMode = "separateColumn",
                notesContent,
                messageLayout = new[] { Plain, Multiline, Referenced }.Select(revision =>
                {
                    Control cell = Message(revision);
                    Control row = cell.GetVisualAncestors().OfType<Control>().Single(control => control.Classes.Contains("revision-row"));
                    TextBlock body = cell.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("revision-body"));
                    string raw = body.Text ?? string.Empty;
                    Size original = WinFormsTextMeasurer.MeasureSize(body, raw);
                    Size removed = WinFormsTextMeasurer.MeasureSize(body, raw.Replace("\r", string.Empty));
                    Size spaces = WinFormsTextMeasurer.MeasureSize(body, raw.Replace('\r', ' '));
                    return new
                    {
                        revision = revision.Guid,
                        rowBounds = new { row.Bounds.X, row.Bounds.Y, row.Bounds.Width, row.Bounds.Height },
                        rowDesiredSize = new { row.DesiredSize.Width, row.DesiredSize.Height },
                        cellBounds = new { cell.Bounds.X, cell.Bounds.Y, cell.Bounds.Width, cell.Bounds.Height },
                        cellDesiredSize = new { cell.DesiredSize.Width, cell.DesiredSize.Height },
                        sourceRowHeight = RevisionGridControl.GetRowHeight(Grid),
                        body = new
                        {
                            raw,
                            bounds = new { body.Bounds.X, body.Bounds.Y, body.Bounds.Width, body.Bounds.Height },
                            desiredSize = new { body.DesiredSize.Width, body.DesiredSize.Height },
                            body.FontSize,
                            fontFamily = body.FontFamily.Name,
                            body.FontStyle,
                            body.FontWeight,
                            lineCount = body.TextLayout.TextLines.Count,
                            measurementMethod = "WinFormsTextMeasurer.MeasureSize(singleLine:true)",
                            originalSingleLine = new { original.Width, original.Height },
                            withoutCarriageReturns = new { removed.Width, removed.Height },
                            withCarriageReturnSpaces = new { spaces.Width, spaces.Height },
                        },
                        labels = cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>().Select(label => new
                        {
                            label.Label,
                            bounds = new { label.Bounds.X, label.Bounds.Y, label.Bounds.Width, label.Bounds.Height },
                            desiredSize = new { label.DesiredSize.Width, label.DesiredSize.Height },
                            capsuleBounds = new { label.CapsuleBounds.X, label.CapsuleBounds.Y, label.CapsuleBounds.Width, label.CapsuleBounds.Height },
                        }),
                    };
                }),
                scope = "Source-backed pointer, tooltip ownership and content regression; not native desktop or full-parity evidence.",
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void Dispose()
        {
            ExceptionDispatchInfo? failure = null;
            TryCleanup(Grid.CancelBackgroundTasks, ref failure);
            TryCleanup(() => Grid.GetTestAccessor().SetRevisions([]), ref failure);
            TryCleanup(Window.Close, ref failure);
            TryCleanup(() => Dispatcher.UIThread.RunJobs(), ref failure);
            failure?.Throw();
        }

        private static GitRevision Revision(int index, string subject, bool multiline, string detail = "", string notes = "")
            => new(ObjectId.Parse(index.ToString("x8") + index.ToString("x32")))
            {
                Subject = subject,
                Body = multiline ? subject + "\n\n" + detail : subject,
                HasMultiLineMessage = multiline,
                Notes = notes,
                Author = "Author",
                Committer = "Committer",
                AuthorUnixTime = 1_800_000_000,
                CommitUnixTime = 1_800_003_600,
            };
    }

    private sealed class SettingsScope : IDisposable
    {
        private readonly GitExtSettingsCache _cache = AppSettings.SettingsContainer.SettingsCache;
        private readonly Dictionary<string, string?> _original;

        public SettingsScope(bool tooltips)
        {
            AppSettings.SettingsContainer.LowerPriority.Should().BeNull("the headless assembly owns a single-level isolated settings cache");
            string[] keys =
            [
                AppSettings.ShowRevisionGridTooltips.FullPath,
                AppSettings.ShowGitNotesColumn.FullPath,
                "ShowCommitBodyInRevisionGrid",
                "showgitnotes",
                "showrevisiongridgraphcolumn",
                "showrevisiongridauthoravatarcolumn",
                "showrevisiongridauthornamecolumn",
                "showrevisiongriddatecolumn",
                "showids",
            ];
            _original = keys.ToDictionary(key => key,
                key => _cache.TryGetValue(key, out string? value) ? value : null, StringComparer.Ordinal);
            AppSettings.ShowRevisionGridTooltips.Value = tooltips;
            AppSettings.ShowGitNotesColumn.Value = true;
            AppSettings.ShowCommitBodyInRevisionGrid = true;
            AppSettings.ShowGitNotes = true;
            AppSettings.ShowRevisionGridGraphColumn = false;
            AppSettings.ShowAuthorAvatarColumn = false;
            AppSettings.ShowAuthorNameColumn = false;
            AppSettings.ShowDateColumn = false;
            AppSettings.ShowObjectIdColumn = false;
        }

        public void Dispose()
        {
            foreach ((string key, string? value) in _original)
            {
                _cache.SetValue(key, value);
            }
        }
    }

    private sealed class ThemeScope : IDisposable
    {
        private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))!;
        private static readonly PropertyInfo HelperSettings = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))!;
        private readonly Application _application = Application.Current ?? throw new AssertionException("The actual headless application must exist.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly ThemeSettings _originalHelper = (ThemeSettings)HelperSettings.GetValue(null)!;
        private readonly ThemeVariant? _variant;

        public ThemeScope(string theme)
        {
            _variant = _application.RequestedThemeVariant;
            ThemeSettings settings = new(LoadTheme(theme), LoadTheme("invariant"), [], useSystemVisualStyle: theme == "light");
            _application.RequestedThemeVariant = theme == "light" ? ThemeVariant.Light : ThemeVariant.Dark;
            ModuleSettings.SetValue(null, settings);
            ColorHelper.ThemeSettings = settings;
            AvaloniaThemeResources.Apply(_application, settings);
        }

        public void Dispose()
        {
            _application.RequestedThemeVariant = _variant;
            ModuleSettings.SetValue(null, _original);
            ColorHelper.ThemeSettings = _originalHelper;
            AvaloniaThemeResources.Apply(_application, _original);
        }

        private static Theme LoadTheme(string theme, [CallerFilePath] string source = "")
        {
            if (theme == "light")
            {
                return Theme.CreateDefaultTheme();
            }

            DirectoryInfo? directory = new FileInfo(source).Directory;
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props")))
            {
                directory = directory.Parent;
            }

            string repository = directory?.FullName ?? throw new AssertionException("The tracked theme sources must exist.");
            string themes = Path.Combine(repository, "src", "app", "GitUI", "Themes");
            string path = theme == "custom"
                ? Path.Combine(repository, "eng", "tools", "WinFormsParityCapture", "Themes", "parity-custom.css")
                : Path.Combine(themes, theme == "invariant" ? "invariant.css" : "dark.css");
            IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
            resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(themes, call.Arg<string>()));
            return new ThemeLoader(resolver, new ThemeFileReader()).LoadTheme(path,
                theme == "invariant" ? ThemeId.DefaultLight : new ThemeId(theme), []);
        }
    }
}

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using SkiaSharp;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Checks source row foreground roles and lane-dependent tooltips on the realized grid.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridRowStateTests
{
    private static readonly string[] MetadataClasses =
    [
        "revision-author-cell",
        "revision-date-cell",
        "revision-object-id-cell",
        "revision-notes-cell",
    ];

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

    private static IEnumerable<TestCaseData> MetadataStates()
    {
        foreach (string theme in new[] { "light", "dark", "custom" })
        {
            foreach (bool gray in new[] { false, true })
            {
                foreach (bool relative in new[] { false, true })
                {
                    foreach (bool selected in new[] { false, true })
                    {
                        foreach (bool focused in new[] { false, true })
                        {
                            yield return new TestCaseData(theme, gray, relative, selected, focused);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///  All ordinary metadata painters receive the source's common CellStyle foreground.
    /// </summary>
    [AvaloniaTest]
    [TestCaseSource(nameof(MetadataStates))]
    public void Metadata_cells_should_follow_the_source_row_foreground(
        string theme, bool gray, bool relative, bool selected, bool focused)
    {
        using ThemeScope themeScope = new(theme);
        using SettingsScope settings = new(gray, tooltips: true);
        using GridFixture fixture = new();
        GitRevision revision = fixture.Revisions.Single(revision => revision.ObjectId == Id(relative ? 1 : 2));
        int index = fixture.Grid.GetRevisionIndex(revision);
        fixture.Graph.IsRowRelative(index).Should().Be(relative);
        fixture.SetSelection(revision, selected, focused);
        Control row = fixture.Row(revision);
        DrawingColor expected = SourceForeground(gray && !relative, selected, focused);
        TextBlock[] cells = MetadataClasses.Select(name => row.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Classes.Contains(name))).ToArray();
        using Frame frame = new(fixture.Window, "metadata");
        using SKBitmap expectedReference = CaptureForegroundReference(fixture.Window, cells,
            Color.FromArgb(expected.A, expected.R, expected.G, expected.B));
        using SKBitmap transparentReference = CaptureForegroundReference(fixture.Window, cells, Colors.Transparent);
        ColumnObservation[] columns = cells.Select((text, index) =>
        {
            Color actual = text.Foreground.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;
            Point origin = text.TranslatePoint(default, fixture.Window)
                ?? throw new AssertionException("The metadata cell must be attached to its actual window.");
            Rect bounds = new(origin, text.Bounds.Size);
            SKColor foreground = new(actual.R, actual.G, actual.B, actual.A);
            return new ColumnObservation(MetadataClasses[index], text.Text ?? string.Empty, Argb(expected), Argb(actual), bounds,
                CountPixels(frame.Bitmap, bounds, foreground),
                CountDifferentPixels(frame.Bitmap, expectedReference, bounds),
                CountDifferentPixels(frame.Bitmap, transparentReference, bounds),
                DescribeGlyphReference(expectedReference, bounds), DescribeGlyphReference(transparentReference, bounds));
        }).ToArray();
        frame.Record(theme, new { gray, relative, selected, focused, columns });

        foreach (ColumnObservation column in columns)
        {
            column.Text.Should().NotBeEmpty();
            column.ActualArgb.Should().Be(column.ExpectedArgb, column.Column + " uses the common source row foreground");
            column.ExpectedReferenceDifferentPixels.Should().Be(0,
                column.Column + " must paint the source foreground with this actual font's antialias coverage");
            column.ForegroundPaintPixels.Should().BeGreaterThan(0,
                column.Column + " must paint visible foreground rather than merely publish a brush property");
        }
    }

    /// <summary>
    ///  The routed pointer event must retain the lane tooltip, not replace it with a generic cell tip.
    /// </summary>
    [AvaloniaTest]
    [TestCase("light", false)]
    [TestCase("light", true)]
    [TestCase("dark", false)]
    [TestCase("dark", true)]
    [TestCase("custom", false)]
    [TestCase("custom", true)]
    public void Actual_graph_pointer_should_use_its_lane_and_default_cursor(string theme, bool tooltips)
    {
        using ThemeScope themeScope = new(theme);
        using SettingsScope settings = new(gray: true, tooltips);
        using GridFixture fixture = new(longHistory: true);
        RevisionGraphColumnProvider provider = fixture.Grid.ColumnProviders.OfType<RevisionGraphColumnProvider>().Single();
        ListBox list = fixture.Grid.GetTestAccessor().Revisions;
        Dictionary<ListBoxItem, GitRevision> originalContainers = list.GetRealizedContainers().OfType<ListBoxItem>()
            .ToDictionary(container => container, container => (GitRevision)container.Content!);
        List<object> observations = [];
        int maxLanes = Enumerable.Range(0, fixture.Graph.Count)
            .Max(row => fixture.Graph.GetSegmentsForRow(row)!.GetLaneCount());
        int rowIndex = Enumerable.Range(0, fixture.Graph.Count)
            .First(row => fixture.Graph.GetSegmentsForRow(row)!.GetLaneCount() > 1);
        GitRevision revision = fixture.Graph.GetNodeForRow(rowIndex)!.GitRevision!;
        Control cell = fixture.Cell(revision, "revision-graph-cell");
        int lane = fixture.Graph.GetSegmentsForRow(rowIndex)!.GetCurrentRevisionLane();
        AssertLane(cell, revision, lane, "node");
        if (tooltips)
        {
            ToolTip.SetIsOpen(cell, true);
            ToolTip.GetIsOpen(cell).Should().BeTrue();
            fixture.Grid.GetTestAccessor().RaiseOwnerWindowDeactivated();
            Dispatcher.UIThread.RunJobs();
            ToolTip.GetIsOpen(cell).Should().BeFalse("the central owner must track the graph tooltip too");
        }

        int otherLane = Enumerable.Range(0, fixture.Graph.GetSegmentsForRow(rowIndex)!.GetLaneCount())
            .First(candidate => candidate != lane);
        AssertLane(cell, revision, otherLane, "other-lane");

        int narrowRow = Enumerable.Range(0, fixture.Graph.Count)
            .First(row => fixture.Graph.GetSegmentsForRow(row)!.GetLaneCount() < maxLanes);
        GitRevision narrowRevision = fixture.Graph.GetNodeForRow(narrowRow)!.GitRevision!;
        Control narrowCell = fixture.Cell(narrowRevision, "revision-graph-cell");
        AssertLane(narrowCell, narrowRevision, maxLanes - 1, "empty-lane");

        // Virtualization reuses the item container; its content presenter may rebuild the row's cells.
        fixture.ScrollTo(fixture.Revisions[^1]);
        ScrollContentPresenter viewport = list.GetVisualDescendants().OfType<ScrollContentPresenter>().Single();
        ListBoxItem replacementContainer = list.GetRealizedContainers().OfType<ListBoxItem>()
            .First(container => originalContainers.TryGetValue(container, out GitRevision? original)
                && !ReferenceEquals(original, container.Content)
                && container.TranslatePoint(new Point(container.Bounds.Width / 2, container.Bounds.Height / 2), viewport) is Point center
                && center.X >= 0 && center.X < viewport.Bounds.Width
                && center.Y >= 0 && center.Y < viewport.Bounds.Height);
        GitRevision replacement = replacementContainer.Content.Should().BeOfType<GitRevision>().Which;
        Control replacementCell = replacementContainer.GetVisualDescendants().OfType<Control>()
            .Single(control => control.Classes.Contains("revision-graph-cell"));
        replacementCell.DataContext.Should().BeSameAs(replacement);
        int replacementIndex = fixture.Grid.GetRevisionIndex(replacement);
        int replacementLane = fixture.Graph.GetSegmentsForRow(replacementIndex)!.GetCurrentRevisionLane();
        AssertLane(replacementCell, replacement, replacementLane, "recycled-node", originalContainers[replacementContainer]);

        if (tooltips)
        {
            fixture.ScrollTo(fixture.Revisions[0]);
            RevisionGridRefRenderer.RefLabelControl label = fixture.Grid.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>().Single(label => label.GitRef?.CompleteName == "refs/heads/main");
            Point point = label.TranslatePoint(new Point(label.Bounds.Width / 2, label.Bounds.Height / 2), fixture.Window)
                ?? throw new AssertionException("The actual ref label must be attached.");
            fixture.Window.MouseMove(point, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Control messageCell = fixture.Cell(fixture.Revisions.Single(revision => revision.ObjectId == label.GitRef!.ObjectId), "revision-message-cell");
            object hand = typeof(MessageColumnProvider).GetField("HandCursor", BindingFlags.Static | BindingFlags.NonPublic)!
                .GetValue(null) ?? throw new AssertionException("The source-shaped ref hand cursor must exist.");
            messageCell.Cursor.Should().BeSameAs(hand, "the source's actionable ref labels retain their hand cursor");
            observations.Add(new { phase = "ref-label", tip = ToolTip.GetTip(messageCell)?.ToString(), cursor = "refHand" });
        }

        using Frame frame = new(fixture.Window, "graph-input");
        frame.Record(theme, new { tooltips, observations });
        return;

        void AssertLane(Control actualCell, GitRevision actualRevision, int actualLane, string phase, GitRevision? previousContainerRevision = null)
        {
            double x = (actualLane * GraphRenderer.LaneWidth) + (GraphRenderer.LaneWidth / 2);
            x.Should().BeLessThan(actualCell.Bounds.Width, "the actual pointer must remain inside this realized cell");
            string? expected = provider.TryGetToolTip(actualRevision, x, out string? tip) ? tip : null;
            Point point = actualCell.TranslatePoint(new Point(x, actualCell.Bounds.Height / 2), fixture.Window)
                ?? throw new AssertionException("The actual graph cell must be attached.");
            using (WriteableBitmap rendered = fixture.Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual input scene must render."))
            {
                // Complete the retained drawing before querying its actual hit target.
            }

            IInputElement? hit = fixture.Window.InputHitTest(point);
            observations.Add(new
            {
                phase = phase + "-hit",
                revision = actualRevision.Guid,
                lane = actualLane,
                x,
                windowPoint = point,
                bounds = actualCell.Bounds,
                margin = actualCell.Margin,
                hitType = hit?.GetType().FullName,
                hitClasses = (hit as Control)?.Classes.ToArray(),
                hitIsGraphCell = ReferenceEquals(hit, actualCell),
            });
            if (!ReferenceEquals(hit, actualCell))
            {
                using Frame failed = new(fixture.Window, "graph-input");
                failed.Record(theme, new { tooltips, observations });
            }

            hit.Should().BeSameAs(actualCell, "the source grid hit-tests the full graph cell, not only painted lane ink");
            fixture.Window.MouseMove(point, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            string? actual = ToolTip.GetTip(actualCell)?.ToString();
            actual.Should().Be(expected, "the complete routed event must preserve the pixel-aware lane tooltip");
            actualCell.Cursor.Should().BeNull("source graph lanes display information but are not ref links");
            observations.Add(new
            {
                phase,
                revision = actualRevision.Guid,
                lane = actualLane,
                x,
                actual,
                expected,
                cursor = "default",
                reusedContainer = previousContainerRevision is not null,
                previousContainerRevision = previousContainerRevision?.Guid,
            });
        }
    }

    private static DrawingColor SourceForeground(bool nonRelativeGray, bool selected, bool focused)
    {
        bool dark = ThemeModule.Settings.Theme.SystemColorMode == WinFormsShims.SystemColorMode.Dark;
        if (nonRelativeGray)
        {
            return selected && focused
                ? dark ? DrawingColor.FromArgb(235, 235, 215) : DrawingColor.FromArgb(188, 188, 188)
                : dark ? DrawingColor.FromArgb(192, 192, 192) : AvaloniaThemeResources.ResolveNativeSystemColor(dark, KnownColor.GrayText);
        }

        return AvaloniaThemeResources.ResolveNativeSystemColor(dark,
            selected && focused && !dark ? KnownColor.HighlightText : KnownColor.ControlText);
    }

    private static int CountPixels(SKBitmap bitmap, Rect bounds, SKColor color)
    {
        int count = 0;
        for (int y = Math.Max(0, (int)Math.Floor(bounds.Top)); y < Math.Min(bitmap.Height, (int)Math.Ceiling(bounds.Bottom)); y++)
        {
            for (int x = Math.Max(0, (int)Math.Floor(bounds.Left)); x < Math.Min(bitmap.Width, (int)Math.Ceiling(bounds.Right)); x++)
            {
                if (bitmap.GetPixel(x, y) == color)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static SKBitmap CaptureForegroundReference(Window window, IEnumerable<TextBlock> cells, Color color)
    {
        window.RenderScaling.Should().Be(1, "the retained glyph crops use the current native100 headless gate");
        List<IDisposable> overrides = [];
        try
        {
            foreach (TextBlock cell in cells)
            {
                IDisposable? value = cell.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(color), BindingPriority.Animation);
                value.Should().NotBeNull("the temporary reference brush must restore the original resource binding");
                overrides.Add(value!);
            }

            // Repaint the same realized glyphs without assuming fully covered LCD/subpixel texels.
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using WriteableBitmap captured = window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual glyph reference scene must render.");
            using MemoryStream stream = new();
            captured.Save(stream, PngBitmapEncoderOptions.Default);
            return SKBitmap.Decode(stream.ToArray()) ?? throw new AssertionException("The actual glyph reference frame must decode.");
        }
        finally
        {
            foreach (IDisposable value in overrides)
            {
                value.Dispose();
            }

            Dispatcher.UIThread.RunJobs();
        }
    }

    private static int CountDifferentPixels(SKBitmap actual, SKBitmap reference, Rect bounds)
    {
        actual.Width.Should().Be(reference.Width);
        actual.Height.Should().Be(reference.Height);
        PixelRect crop = Crop(actual, bounds);
        int count = 0;
        for (int y = crop.Y; y < crop.Bottom; y++)
        {
            for (int x = crop.X; x < crop.Right; x++)
            {
                if (actual.GetPixel(x, y) != reference.GetPixel(x, y))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static GlyphReference DescribeGlyphReference(SKBitmap bitmap, Rect bounds)
    {
        PixelRect crop = Crop(bitmap, bounds);
        string[] rows = new string[crop.Height];
        for (int y = 0; y < crop.Height; y++)
        {
            List<string> runs = [];
            int x = 0;
            while (x < crop.Width)
            {
                SKColor color = bitmap.GetPixel(crop.X + x, crop.Y + y);
                int count = 1;
                while (x + count < crop.Width && bitmap.GetPixel(crop.X + x + count, crop.Y + y) == color)
                {
                    count++;
                }

                runs.Add($"{count}:{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2}");
                x += count;
            }

            rows[y] = string.Join(';', runs);
        }

        return new GlyphReference(crop.X, crop.Y, crop.Width, crop.Height, rows);
    }

    private static PixelRect Crop(SKBitmap bitmap, Rect bounds)
    {
        int left = Math.Max(0, (int)Math.Floor(bounds.Left));
        int top = Math.Max(0, (int)Math.Floor(bounds.Top));
        int right = Math.Min(bitmap.Width, (int)Math.Ceiling(bounds.Right));
        int bottom = Math.Min(bitmap.Height, (int)Math.Ceiling(bounds.Bottom));
        return new PixelRect(left, top, right - left, bottom - top);
    }

    private static string Argb(DrawingColor color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static string Argb(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static ObjectId Id(int index) => ObjectId.Parse(index.ToString("x8") + index.ToString("x32"));

    private sealed record ColumnObservation(string Column, string Text, string ExpectedArgb, string ActualArgb, Rect Bounds,
        int ExactForegroundPixels, int ExpectedReferenceDifferentPixels, int ForegroundPaintPixels,
        GlyphReference ExpectedForegroundReference, GlyphReference TransparentForegroundReference);

    private sealed record GlyphReference(int X, int Y, int Width, int Height, string[] ArgbRuns);

    private sealed class GridFixture : IDisposable
    {
        private readonly IGitModule _module = Substitute.For<IGitModule>();
        private readonly bool _longHistory;

        public GridFixture(bool longHistory = false)
        {
            _longHistory = longHistory;
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(_module);
            commands.BrowseRepo.Returns(Substitute.For<IBrowseRepo>());
            commands.GetService(typeof(ResourceManager.IHotkeySettingsLoader)).Returns(Substitute.For<ResourceManager.IHotkeySettingsLoader>());
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            Grid = new RevisionGridControl(Substitute.For<ICommitDataManager>()) { UICommandsSource = source };
            Graph = (RevisionGraph)(typeof(RevisionGridControl).GetField("_revisionGraph", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(Grid) ?? throw new AssertionException("The actual grid graph must exist."));
            PublishModel(offset: 0);
            DockPanel host = new();
            DockPanel.SetDock(FocusTarget, Dock.Bottom);
            host.Children.Add(FocusTarget);
            host.Children.Add(Grid);
            Window = new Window { Width = 1100, Height = 260, Content = host };
            Window.Show();
            Window.Activate();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public RevisionGridControl Grid { get; }

        public RevisionGraph Graph { get; }

        public Window Window { get; }

        public Button FocusTarget { get; } = new() { Content = "Other focus target" };

        public GitRevision[] Revisions { get; private set; } = [];

        public Control Row(GitRevision revision)
            => Grid.GetVisualDescendants().OfType<Control>().Single(control => control.Classes.Contains("revision-row")
                && ReferenceEquals(control.DataContext, revision));

        public Control Cell(GitRevision revision, string cellClass)
            => Row(revision).GetVisualDescendants().OfType<Control>().Single(control => control.Classes.Contains(cellClass));

        public void SetSelection(GitRevision revision, bool selected, bool focused)
        {
            ListBox list = Grid.GetTestAccessor().Revisions;
            list.SelectedItem = selected ? revision : Revisions.First(candidate => !ReferenceEquals(candidate, revision));
            (focused ? (Avalonia.Input.InputElement)list : FocusTarget).Focus().Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            Grid.RefreshRealizedRows();
            Window.UpdateLayout();
            list.SelectedItems!.Contains(revision).Should().Be(selected);
            list.IsKeyboardFocusWithin.Should().Be(focused);
        }

        public void ScrollTo(GitRevision revision)
        {
            Grid.GetTestAccessor().Revisions.ScrollIntoView(revision);
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public void Dispose()
        {
            Grid.CancelBackgroundTasks();
            Grid.GetTestAccessor().SetRevisions([]);
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        private void PublishModel(int offset)
        {
            Graph.Clear();
            Graph.HeadId = Id(offset + 1);
            typeof(RevisionGridControl).GetField("_headId", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(Grid, Graph.HeadId);
            List<GitRevision> revisions =
            [
                Revision(1, 3, 4),
                Revision(2, 5),
                Revision(3, 6),
                Revision(4, 6),
                Revision(5, 6),
                Revision(6, _longHistory ? [7] : []),
            ];
            if (_longHistory)
            {
                revisions.AddRange(Enumerable.Range(7, 74).Select(index => Revision(index, index < 80 ? [index + 1] : [])));
            }

            revisions[0].Refs = [new GitRef(_module, revisions[0].ObjectId, "refs/heads/main")];
            foreach (GitRevision revision in revisions)
            {
                Graph.Add(revision);
            }

            Graph.HighlightBranch(Graph.HeadId);
            Graph.LoadingCompleted();
            Graph.CacheTo(Graph.Count - 1, Graph.Count - 1);
            Revisions = [.. Enumerable.Range(0, Graph.Count).Select(index => Graph.GetNodeForRow(index)!.GitRevision!)];
            Grid.GetTestAccessor().SetRevisions(Revisions);
            return;

            GitRevision Revision(int index, params int[] parents)
                => new(Id(offset + index))
                {
                    ParentIds = [.. parents.Select(parent => Id(offset + parent))],
                    Subject = $"Realized revision {offset + index}",
                    Body = $"Realized revision {offset + index}\n\nLoaded body",
                    Notes = "Saved note",
                    Author = $"Author {index}",
                    Committer = $"Committer {index}",
                    AuthorUnixTime = 1_800_000_000 - index,
                    CommitUnixTime = 1_800_003_600 - index,
                };
        }
    }

    private sealed class Frame : IDisposable
    {
        private readonly string _kind;
        private readonly string? _directory;

        public Frame(Window window, string kind)
        {
            _kind = kind;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using WriteableBitmap captured = window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual realized grid must render a frame.");
            using MemoryStream stream = new();
            captured.Save(stream, PngBitmapEncoderOptions.Default);
            Bitmap = SKBitmap.Decode(stream.ToArray()) ?? throw new AssertionException("The actual frame must decode.");
            string? evidence = Environment.GetEnvironmentVariable("GITEXT_ROW_STATE_EVIDENCE");
            if (!string.IsNullOrWhiteSpace(evidence))
            {
                _directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID);
                Directory.CreateDirectory(_directory);
                string path = Path.Combine(_directory, kind + ".png");
                File.Exists(path).Should().BeFalse("an evidence frame must never overwrite a previous run");
                captured.Save(path, PngBitmapEncoderOptions.Default);
            }
        }

        public SKBitmap Bitmap { get; }

        public void Record(string theme, object observations)
        {
            if (_directory is not null)
            {
                File.WriteAllText(Path.Combine(_directory, _kind + ".json"), JsonSerializer.Serialize(new
                {
                    test = TestContext.CurrentContext.Test.FullName,
                    theme,
                    kind = _kind,
                    captureMethod = "actualAvaloniaHeadlessRevisionGridWindow",
                    PixelSize = new { Bitmap.Width, Bitmap.Height },
                    observations,
                    scope = "Source-backed realized-control color and pointer regression; not paired WinForms image or full-parity evidence.",
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        public void Dispose() => Bitmap.Dispose();
    }

    private sealed class SettingsScope : IDisposable
    {
        private readonly GitExtSettingsCache _cache = AppSettings.SettingsContainer.SettingsCache;
        private readonly Dictionary<string, string?> _original;

        public SettingsScope(bool gray, bool tooltips)
        {
            AppSettings.SettingsContainer.LowerPriority.Should().BeNull("the headless assembly owns an isolated, single-level settings cache");
            string[] keys =
            [
                "revisiongraphdrawnonrelativestextgray",
                AppSettings.ShowRevisionGridTooltips.FullPath,
                "showrevisiongridauthornamecolumn",
                "showrevisiongriddatecolumn",
                "showids",
                AppSettings.ShowGitNotesColumn.FullPath,
                "showrevisiongridauthoravatarcolumn",
                "showrevisiongridgraphcolumn",
                "highlightauthoredrevisions",
                "RevisionGraphDrawAlternateBackColor",
                "relativedate",
            ];

            // Preserve absence and raw spelling, not merely each property's effective default.
            _original = keys.ToDictionary(key => key,
                key => _cache.TryGetValue(key, out string? raw) ? raw : null, StringComparer.Ordinal);
            AppSettings.RevisionGraphDrawNonRelativesTextGray = gray;
            AppSettings.ShowRevisionGridTooltips.Value = tooltips;
            AppSettings.ShowAuthorNameColumn = true;
            AppSettings.ShowDateColumn = true;
            AppSettings.ShowObjectIdColumn = true;
            AppSettings.ShowGitNotesColumn.Value = true;
            AppSettings.ShowAuthorAvatarColumn = false;
            AppSettings.ShowRevisionGridGraphColumn = true;
            AppSettings.HighlightAuthoredRevisions = false;
            AppSettings.RevisionGraphDrawAlternateBackColor = false;
            AppSettings.RelativeDate = false;
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
        private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))
            ?? throw new InvalidOperationException("The source-shaped theme settings must exist.");
        private static readonly PropertyInfo HelperSettings = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))
            ?? throw new InvalidOperationException("The shared color settings must exist.");
        private readonly Application _application = Application.Current ?? throw new InvalidOperationException("The headless application must exist.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly ThemeSettings _originalHelper = HelperSettings.GetValue(null) as ThemeSettings
            ?? throw new InvalidOperationException("The shared color settings must be initialized.");
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

            string repository = directory?.FullName ?? throw new AssertionException("The tracked theme source must be available.");
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

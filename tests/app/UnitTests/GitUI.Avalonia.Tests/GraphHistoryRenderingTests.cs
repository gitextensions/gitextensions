using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using SkiaSharp;
using IHotkeySettingsLoader = ResourceManager.IHotkeySettingsLoader;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class GraphHistoryRenderingTests
{
    private const int RowHeight = 26;
    private const int RevisionCount = 144;
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
    public void TearDown()
    {
        ThreadHelper.JoinableTaskContext = _previousContext!;
        _context.Dispose();
    }

    [AvaloniaTest]
    public async Task Loaded_graph_should_publish_on_the_grid_thread_and_replace_the_old_viewport()
    {
        using AppSettingsTestScope settings = new();
        AppSettings.RevisionGraphShowArtificialCommits = false;
        AppSettings.ShowRevisionGridGraphColumn = true;
        using GitModuleTestHelper first = new();
        using GitModuleTestHelper second = new();
        ImportHistory(first.Module);
        second.Module.GitExecutable.GetOutput("commit --allow-empty -m replacement");
        IGitUICommandsSource source = CreateCommandsSource();
        RevisionGridControl grid = new() { UICommandsSource = source };
        Window window = new() { Width = 1_000, Height = 400, Content = grid };

        window.Show();
        try
        {
            await ReloadAsync(first.Module);
            RevisionGraph graph = (RevisionGraph)(typeof(RevisionGridControl)
                .GetField("_revisionGraph", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(grid)!);
            graph.GetCachedCount().Should().Be(graph.Count);
            ScrollContentPresenter viewport = grid._gridView.GetVisualDescendants()
                .OfType<ScrollContentPresenter>().Single();
            ScrollViewer scroll = grid._gridView.GetVisualDescendants().OfType<ScrollViewer>().Single();
            double rowHeight = RevisionGridControl.GetRowHeight(grid);
            RevisionGraphColumnProvider provider = grid.ColumnProviders.OfType<RevisionGraphColumnProvider>().Single();

            foreach (int lastRow in new[] { 5, 15 })
            {
                SetViewportHeight(rowHeight * (lastRow + 0.5));
                int[] rows = GetVisibleRows(graph, viewport);
                rows.Should().Equal(Enumerable.Range(0, lastRow + 1));
                provider.Column.Width.Value.Should().Be(GetSourceWidth(
                    rows.Max(index => graph.GetSegmentsForRow(index)!.GetLaneCount())));
            }

            int threeLaneRow = Enumerable.Range(0, graph.Count)
                .First(index => graph.GetSegmentsForRow(index)!.GetLaneCount() == 3);
            SetViewportHeight(rowHeight / 2);
            scroll.Offset = new Vector(scroll.Offset.X, rowHeight * (threeLaneRow + 0.25));
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            GetVisibleRows(graph, viewport).Should().Equal(threeLaneRow);
            provider.Column.Width.Value.Should().Be(54, "the source reserves a 6-DIP left margin and three 16-DIP lanes");

            window.Height = 400;
            await ReloadAsync(second.Module);
            GitRevision revision = grid._gridView.Items.OfType<GitRevision>().Should().ContainSingle().Which;
            revision.Subject.Should().Be("replacement");
            graph.GetCachedCount().Should().Be(1);
            provider.Column.Width.Value.Should().Be(GetSourceWidth(1));

            void SetViewportHeight(double height)
            {
                window.Height += height - viewport.Bounds.Height;
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }
        }
        finally
        {
            window.Close();
            grid.CancelBackgroundTasks();
        }

        async Task ReloadAsync(GitModule module)
        {
            source.UICommands.Module.Returns(module);
            TaskCompletionSource<bool> loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<RevisionLoadEventArgs> handler = (_, _) => loaded.TrySetResult(Dispatcher.UIThread.CheckAccess());
            grid.RevisionsLoaded += handler;
            try
            {
                grid.ReloadRevisions(module);
                (await loaded.Task.WaitAsync(TimeSpan.FromSeconds(15))).Should().BeTrue();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }
            finally
            {
                grid.RevisionsLoaded -= handler;
            }
        }

        int[] GetVisibleRows(RevisionGraph graph, ScrollContentPresenter viewport)
            => [.. grid.GetVisualDescendants().OfType<Control>()
                .Where(cell => cell.Classes.Contains("revision-graph-cell")
                    && cell.TranslatePoint(default, viewport) is Point origin
                    && origin.Y < viewport.Bounds.Height
                    && origin.Y + cell.Bounds.Height > 0)
                .Select(cell => (GitRevision)cell.DataContext!)
                .Select(revision => graph.TryGetRowIndex(revision.ObjectId, out int index) ? index : -1)
                .Order()];
    }

    [AvaloniaTest]
    public void Graph_width_should_follow_actual_viewport_including_partial_rows_not_virtualization_overscan()
    {
        RevisionGridControl grid = new() { UICommandsSource = CreateCommandsSource() };
        RevisionGraph graph = (RevisionGraph)(typeof(RevisionGridControl)
            .GetField("_revisionGraph", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(grid)!);
        GitRevision[] revisions = [.. Enumerable.Range(1, 80)
            .Select(index => index switch
            {
                < 8 => Revision(index, index + 1),
                < 40 => Revision(index, index + 40),
                < 80 => Revision(index, index + 1),
                _ => Revision(index),
            })];
        graph.HeadId = revisions[0].ObjectId;
        typeof(RevisionGridControl).GetField("_headId", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(grid, graph.HeadId);
        foreach (GitRevision revision in revisions)
        {
            graph.Add(revision);
        }

        graph.HighlightBranch(graph.HeadId);
        graph.LoadingCompleted();
        graph.CacheTo(graph.Count - 1, graph.Count - 1);
        GitRevision[] orderedRevisions = [.. Enumerable.Range(0, graph.Count)
            .Select(index => graph.GetNodeForRow(index)!.GitRevision!)];
        int firstLaneCount = graph.GetSegmentsForRow(0)!.GetLaneCount();
        int firstWiderRowIndex = Enumerable.Range(1, graph.Count - 1)
            .First(index => graph.GetSegmentsForRow(index)!.GetLaneCount() > firstLaneCount);
        grid._gridView.ItemsPanel = new FuncTemplate<Panel?>(
            () => new VirtualizingStackPanel { CacheLength = 1 });
        grid.GetNativeTestAccessor().SetRevisions(orderedRevisions);
        Window window = new() { Width = 1200, Height = 155, Content = grid };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            ScrollContentPresenter viewport = grid._gridView.GetVisualDescendants()
                .OfType<ScrollContentPresenter>().Single();
            double rowHeight = RevisionGridControl.GetRowHeight(grid);
            double targetViewportHeight = (firstWiderRowIndex * rowHeight) - (rowHeight / 2);
            window.Height += targetViewportHeight - viewport.Bounds.Height;
            window.UpdateLayout();
            AssertViewportWidth(requireOverscan: true);
            ScrollViewer scroll = grid._gridView.GetVisualDescendants().OfType<ScrollViewer>().Single();
            scroll.Offset = new Vector(scroll.Offset.X, (rowHeight * firstWiderRowIndex) + (rowHeight / 3));
            AssertViewportWidth(requireOverscan: false);
            scroll.Offset = default;
            window.Height += rowHeight;
            AssertViewportWidth(requireOverscan: false);
        }
        finally
        {
            window.Close();
            grid.CancelBackgroundTasks();
        }

        return;

        void AssertViewportWidth(bool requireOverscan)
        {
            Dispatcher.UIThread.RunJobs();
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The actual virtualized grid did not render.");
            ScrollContentPresenter viewport = grid._gridView.GetVisualDescendants()
                .OfType<ScrollContentPresenter>().Single();
            Control[] cells = [.. grid.GetVisualDescendants().OfType<Control>()
                .Where(control => control.Classes.Contains("revision-graph-cell"))];
            Control[] visible = [.. cells.Where(cell => cell.TranslatePoint(default, viewport) is Point origin
                && origin.Y < viewport.Bounds.Height && origin.Y + cell.Bounds.Height > 0)];
            visible.Should().NotBeEmpty();
            int visibleLaneCount = visible.Max(Lanes);
            int expectedWidth = GetSourceWidth(visibleLaneCount);
            if (requireOverscan)
            {
                cells.Max(Lanes).Should().BeGreaterThan(visibleLaneCount,
                    "the fixture must expose a wider realized-but-offscreen branch than the actual viewport");
            }

            visible.Any(cell => cell.TranslatePoint(default, viewport) is Point origin
                && (origin.Y < 0 || origin.Y + cell.Bounds.Height > viewport.Bounds.Height)).Should().BeTrue(
                "partially visible endpoint rows must participate in the viewport width");
            foreach (Control cell in cells)
            {
                Grid row = cell.GetVisualParent<Grid>()!;
                row.ColumnDefinitions[Grid.GetColumn(cell)].Width.Value.Should().Be(expectedWidth,
                    "all realized rows, including overscan, must receive the viewport's allocation");
            }
        }

        int Lanes(Control cell)
            => cell.DataContext is GitRevision revision && graph.TryGetRowIndex(revision.ObjectId, out int rowIndex)
                ? graph.GetSegmentsForRow(rowIndex)!.GetLaneCount()
                : 0;
    }

    [AvaloniaTest]
    public async Task ReloadRevisions_should_paint_real_commits_after_the_reader_finishes(
        [Values] bool showArtificial,
        [Values] bool diagonals)
    {
        bool previousArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        bool previousDiagonals = AppSettings.RenderGraphWithDiagonals.Value;
        AppSettings.RevisionGraphShowArtificialCommits = showArtificial;
        AppSettings.RenderGraphWithDiagonals.Value = diagonals;
        using GitModuleTestHelper repository = new(nameof(GraphHistoryRenderingTests));
        using GraphTraceListener trace = new();
        Trace.Listeners.Add(trace);
        RevisionGridControl? grid = null;
        Window? window = null;
        try
        {
            ImportHistory(repository.Module);
            IGitUICommandsSource source = CreateCommandsSource();
            source.UICommands.Module.Returns(repository.Module);
            grid = new RevisionGridControl
            {
                UICommandsSource = source,
                ShowUncommittedChangesIfPossible = true,
            };
            bool loaded = false;
            grid.RevisionsLoaded += (_, _) => loaded = true;
            window = new Window { Width = 1000, Height = 400, Content = grid };
            window.Show();
            grid.ReloadRevisions(repository.Module);
            Stopwatch timeout = Stopwatch.StartNew();
            while (!loaded && timeout.Elapsed < TimeSpan.FromSeconds(15))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            loaded.Should().BeTrue("the actual revision reader must complete before verifying graph paint");
            Dispatcher.UIThread.RunJobs();
            RevisionGraph graph = (RevisionGraph)(typeof(RevisionGridControl)
                .GetField("_revisionGraph", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(grid)!);
            graph.GetCachedCount().Should().Be(graph.Count);
            grid._gridView.Items.Count.Should().Be(600 + (showArtificial ? 2 : 0));
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The actual history grid did not render a frame.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            using SKBitmap bitmap = SKBitmap.Decode(stream.ToArray());
            Control[] cells = [.. grid.GetVisualDescendants()
                .OfType<Control>()
                .Where(control => control.Classes.Contains("revision-graph-cell"))];
            cells.Should().NotBeEmpty();
            ScrollContentPresenter viewport = grid._gridView.GetVisualDescendants()
                .OfType<ScrollContentPresenter>().Single();
            int visibleLaneCount = cells
                .Where(cell => cell.TranslatePoint(default, viewport) is Point origin
                    && origin.Y < viewport.Bounds.Height && origin.Y + cell.Bounds.Height > 0)
                .Select(cell => cell.DataContext)
                .OfType<GitRevision>()
                .Select(revision => graph.TryGetRowIndex(revision.ObjectId, out int rowIndex)
                    ? graph.GetSegmentsForRow(rowIndex)!.GetLaneCount()
                    : 0)
                .Max();
            visibleLaneCount.Should().BeGreaterThan(1, "the actual grid must allocate more than its initial single-lane slot");
            foreach (Control cell in cells)
            {
                Grid row = cell.GetVisualParent<Grid>()!;
                row.ColumnDefinitions[Grid.GetColumn(cell)].Width.Value.Should()
                    .Be(GetSourceWidth(visibleLaneCount));
            }

            int realNodes = 0;
            foreach (Control cell in cells)
            {
                if (cell.DataContext is not GitRevision { IsArtificial: false } revision
                    || !graph.TryGetRowIndex(revision.ObjectId, out int rowIndex))
                {
                    continue;
                }

                IRevisionGraphRow row = graph.GetSegmentsForRow(rowIndex)!;
                if (!row.Revision.IsRelative)
                {
                    continue;
                }

                Point point = new((row.GetCurrentRevisionLane() * GraphRenderer.LaneWidth) + (GraphRenderer.LaneWidth / 2),
                    cell.Bounds.Height / 2);
                if (cell.TranslatePoint(point, viewport) is not Point viewportCenter
                    || viewportCenter.Y < 0 || viewportCenter.Y >= viewport.Bounds.Height)
                {
                    continue;
                }

                Point? position = cell.TranslatePoint(point, window);
                if (position is not Point center || center.Y < 0 || center.Y >= bitmap.Height)
                {
                    continue;
                }

                // Verify a node in the actual provider-owned cell, not a separate renderer sheet.
                SKColor pixel = bitmap.GetPixel((int)center.X, (int)center.Y);
                bool laneColored = RevisionGraphLaneColor.PresetGraphBrushes
                    .OfType<ISolidColorBrush>()
                    .Any(brush => pixel == new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A));
                laneColored.Should().BeTrue($"real commit {revision.ObjectId} must paint a graph-lane color, not just its tinted row background; trace: {trace.Output}");
                realNodes++;
            }

            realNodes.Should().BeGreaterThan(5);
            trace.Output.Should().NotContain("Exception", "the graph painter must not silently swallow a failure");
        }
        finally
        {
            grid?.CancelBackgroundTasks();

            // Retire the painted rows before draining detail-loading callbacks, while
            // their repository and joinable context are still available.
            grid?.GetNativeTestAccessor().SetRevisions([]);
            window?.Close();
            Dispatcher.UIThread.RunJobs();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            Dispatcher.UIThread.RunJobs();
            Trace.Listeners.Remove(trace);
            AppSettings.RevisionGraphShowArtificialCommits = previousArtificial;
            AppSettings.RenderGraphWithDiagonals.Value = previousDiagonals;
            repository.Dispose();
            TestDirectory.Delete(repository.TemporaryPath);
        }
    }

    [AvaloniaTest]
    public void DrawItem_should_keep_every_commit_visible_through_long_interleaved_merge_history(
        [Values] bool diagonals,
        [Values] bool mergeCommonParents,
        [Values(RevisionGraphDrawStyle.Normal, RevisionGraphDrawStyle.DrawNonRelativesGray, RevisionGraphDrawStyle.HighlightSelected)] RevisionGraphDrawStyle drawStyle)
    {
        bool previousDiagonals = AppSettings.RenderGraphWithDiagonals.Value;
        bool previousMergeCommonParents = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        AppSettings.RenderGraphWithDiagonals.Value = diagonals;
        AppSettings.MergeGraphLanesHavingCommonParent.Value = mergeCommonParents;
        Window? window = null;
        try
        {
            RevisionGraph graph = BuildGraph();
            graph.Count.Should().Be(RevisionCount);
            graph.GetCachedCount().Should().Be(graph.Count);
            int laneCount = Enumerable.Range(0, graph.Count)
                .Max(index => graph.GetSegmentsForRow(index)!.GetLaneCount());
            laneCount.Should().BeGreaterThan(2, "interleaved tips, nested and octopus merges must exercise more than a toy single lane");
            GraphHistoryControl sheet = new(graph, drawStyle);
            window = new Window
            {
                Width = GraphRenderer.LaneWidth * laneCount,
                Height = RowHeight * graph.Count,
                Background = Brushes.Transparent,
                Content = sheet,
            };
            window.Show();
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The real graph renderer did not produce a headless frame.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            using SKBitmap bitmap = SKBitmap.Decode(stream.ToArray());

            sheet.RenderedRows.Should().Be(graph.Count);
            for (int rowIndex = 0; rowIndex < graph.Count; rowIndex++)
            {
                IRevisionGraphRow row = graph.GetSegmentsForRow(rowIndex)!;
                int x = (row.GetCurrentRevisionLane() * GraphRenderer.LaneWidth) + (GraphRenderer.LaneWidth / 2);
                int y = (rowIndex * RowHeight) + (RowHeight / 2);
                bitmap.GetPixel(x, y).Alpha.Should().BeGreaterThan(0,
                    $"commit {row.Revision.Objectid} on row {rowIndex} must be painted at its shared graph-model lane");
            }
        }
        finally
        {
            window?.Close();
            AppSettings.RenderGraphWithDiagonals.Value = previousDiagonals;
            AppSettings.MergeGraphLanesHavingCommonParent.Value = previousMergeCommonParents;
        }
    }

    private static int GetSourceWidth(int visibleLaneCount)
        => 6 + Math.Max(GraphRenderer.LaneWidth, GraphRenderer.LaneWidth * Math.Min(visibleLaneCount, GraphRenderer.MaxLanes));

    private static RevisionGraph BuildGraph()
    {
        RevisionGraph graph = new() { HeadId = Id(1) };
        graph.Add(Revision(1, 5, 9));
        graph.Add(Revision(2, 6, 15));
        graph.Add(Revision(3, 7, 19));
        graph.Add(Revision(4, 8, 20));
        for (int index = 5; index <= RevisionCount; index++)
        {
            List<int> parents = [];
            if (index < RevisionCount)
            {
                parents.Add(index + 1);
            }

            if (index % 11 == 0 && index + 5 <= RevisionCount)
            {
                parents.Add(index + 5);
            }

            if (index % 17 == 0 && index + 11 <= RevisionCount)
            {
                parents.Add(index + 11);
            }

            graph.Add(Revision(index, [.. parents]));
        }

        graph.HighlightBranch(graph.HeadId);
        graph.LoadingCompleted();
        graph.CacheTo(graph.Count - 1, graph.Count - 1);
        return graph;
    }

    private static void ImportHistory(GitModule module)
    {
        StringBuilder input = new();
        for (int index = 600; index >= 1; index--)
        {
            string message = $"Real reader revision {index}";
            input.Append($"commit refs/heads/main\nmark :{index}\ncommitter Graph test <graph@example.invalid> {1_800_000_000 + 600 - index} +0000\ndata {message.Length}\n{message}\n");
            int[] parents = index switch
            {
                1 => [5, 9],
                2 => [6, 15],
                3 => [7, 19],
                4 => [8, 20],
                < 600 => [index + 1],
                _ => [],
            };
            if (parents.Length > 0)
            {
                input.Append($"from :{parents[0]}\n");
                foreach (int parent in parents.Skip(1))
                {
                    input.Append($"merge :{parent}\n");
                }
            }

            if (index % 11 == 0 && index + 5 <= 600)
            {
                input.Append($"merge :{index + 5}\n");
            }

            if (index % 17 == 0 && index + 11 <= 600)
            {
                input.Append($"merge :{index + 11}\n");
            }

            input.Append('\n');
        }

        input.Append("reset refs/heads/tip\nfrom :2\n\nreset refs/heads/other\nfrom :3\n\nreset refs/heads/fourth\nfrom :4\n\ndone\n");
        module.GitExecutable.Execute("fast-import --quiet", writeInput: writer => writer.Write(input.ToString())).ExitCode.Should().Be(0);
        module.GitExecutable.RunCommand("symbolic-ref HEAD refs/heads/main").Should().BeTrue();
        module.GitExecutable.RunCommand("reset --hard --quiet").Should().BeTrue();
    }

    private static IGitUICommandsSource CreateCommandsSource()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        source.UICommands.Returns(commands);
        return source;
    }

    private static ObjectId Id(int index)
        => ObjectId.Parse(index.ToString("x8") + index.ToString("x32"));

    private static GitRevision Revision(int index, params int[] parents)
        => new(Id(index))
        {
            ParentIds = [.. parents.Select(Id)],
            Subject = $"Interleaved revision {index}",
            Author = "Graph test",
        };

    private sealed class GraphHistoryControl(RevisionGraph graph, RevisionGraphDrawStyle drawStyle) : Control
    {
        public int RenderedRows { get; private set; }

        public override void Render(DrawingContext context)
        {
            RenderedRows = 0;
            for (int rowIndex = 0; rowIndex < graph.Count; rowIndex++)
            {
                using (context.PushTransform(Matrix.CreateTranslation(0, rowIndex * RowHeight)))
                using (context.PushClip(new Rect(0, 0, Bounds.Width, RowHeight)))
                {
                    GraphRenderer.DrawItem(graph.Config, context, rowIndex, RowHeight,
                        graph.GetSegmentsForRow, drawStyle, graph.HeadId);
                }

                RenderedRows++;
            }
        }
    }

    private sealed class GraphTraceListener : TraceListener
    {
        private readonly ConcurrentQueue<string> _output = new();

        public string Output => string.Join(Environment.NewLine, _output);

        public override void Write(string? message) => _output.Enqueue(message ?? string.Empty);

        public override void WriteLine(string? message) => Write(message);
    }
}

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CommitInfoScrollViewportTests
{
    private const int SourceOwnerWidth = 1520;
    private const int SourceOwnerHeight = 482;
    private const string EvidenceEnvironmentVariable = "GITEXT_COMMITINFO_VIEWPORT_EVIDENCE";
    private const string RetainedMessage = """
        fix: align main-window text and tooltip behavior

        Match source text-cell bounds and hash capacity, Working Directory renderer
        colors, and automatic tooltip ownership while preserving explicit hints,
        caller bindings and popup-row settings.

        Correct raw capture roles, source field ownership, live panel bounds and
        actual pre-popup focus provenance. Keep measurement corrections separate
        from product repairs and retain the remaining 279 image findings.

        Verify both Windows solutions and the WSL build, 1394 Windows product cases,
        1391 Linux passes with three qualified native-font skips, 147 original
        native probes, and construction, capture-driver and bookkeeping checks on
        both platforms. Validate 54 native100 capture pairs and Windows, Wayland
        and X11 runtime smokes. Retain local evidence and clean generated outputs.
        """;
    private const string RetainedRefs = "Related links: <a href='https://example.invalid/commit'>View on GitHub</a>\n\n"
        + "Contained in branches:\n<a href='gitext://gotobranch/Avalonia_Port'>Avalonia_Port</a>\n\n"
        + "Contained in no tag\n\nDerives from tag: <a href='gitext://gototag/v3.00.00-rc2'>v3.00.00-rc2</a> + 3644 commits";
    private static readonly DateTime FixtureNowUtc = new(2026, 10, 23, 14, 9, 56, DateTimeKind.Utc);
    private static readonly JsonSerializerOptions EvidenceJsonOptions = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void CommitInfo_should_release_transient_scrollbars_when_the_same_final_contents_fit_the_unchanged_owner(bool scrollBeforeShrink)
    {
        WithConfiguredSettings(() =>
        {
            string finalMessage = SelectNaturalBoundaryMessage();
            using ViewportFixture fresh = new("fresh-final", SourceOwnerWidth, finalMessage, RetainedRefs);
            LayoutState freshState = fresh.Settle("fresh-final");
            using ViewportFixture transition = new("transition", SourceOwnerWidth, "subject", "loading refs");
            transition.Settle("short");
            string overflowMessage = CreateOverflowMessage(finalMessage);
            for (int cycle = 0; cycle < 3; cycle++)
            {
                transition.PostContents(overflowMessage, RetainedRefs);
                LayoutState overflow = transition.Settle($"overflow-{cycle}");
                overflow.Owner.Size.Should().Be(new Size(SourceOwnerWidth, SourceOwnerHeight));
                overflow.Extent.Height.Should().BeGreaterThan(overflow.Viewport.Height);
                overflow.VerticalBar.IsVisible.Should().BeTrue();
                overflow.HorizontalBar.IsVisible.Should().BeFalse(
                    "the real header fits this owner and the body and refs wrap within its source column");
                if (scrollBeforeShrink)
                {
                    transition.Scroll.Offset = new Vector(0, overflow.Extent.Height - overflow.Viewport.Height);
                    transition.Settle($"scrolled-overflow-{cycle}").Offset.Y.Should().BeGreaterThan(0);
                }

                transition.PostContents(finalMessage, RetainedRefs);
                LayoutState restored = transition.Settle($"restored-final-{cycle}");
                AssertFitsWithoutScrollbars(freshState);
                AssertFitsWithoutScrollbars(restored);
                AssertSameFinalLayout(freshState, restored);
                transition.Accessor.CommitMessage.GetPlainText().Should().Be(finalMessage.Replace("\r\n", "\n"));
                transition.Accessor.RevisionInfo.TextLayout.TextLines.Should().HaveCount(8);
                transition.Accessor.Avatar.IsVisible.Should().BeTrue();
                double.IsNaN(transition.Accessor.Header.Width).Should().BeTrue();
                double.IsNaN(transition.Accessor.Header.Height).Should().BeTrue();
            }
        });
    }

    [AvaloniaTest]
    public void CommitInfo_should_keep_genuine_header_horizontal_and_both_axis_overflow_reachable_and_release_them_after_widening()
    {
        WithConfiguredSettings(() =>
        {
            const int narrowOwnerWidth = 240;
            using ViewportFixture fixture = new("narrow-owner", narrowOwnerWidth, "subject", "refs");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                fixture.Window.Width = narrowOwnerWidth;
                fixture.PostContents("subject", "refs");
                LayoutState horizontal = fixture.Settle($"horizontal-{cycle}");
                horizontal.HeaderDesired.Width.Should().BeGreaterThan(horizontal.Owner.Width);
                horizontal.HorizontalBar.IsVisible.Should().BeTrue();
                horizontal.HorizontalBar.Maximum.Should().BeGreaterThan(0);
                horizontal.Extent.Width.Should().BeGreaterThan(horizontal.Viewport.Width);
                horizontal.VerticalBar.IsVisible.Should().BeFalse();

                fixture.PostContents(CreateOverflowMessage(RetainedMessage), RetainedRefs);
                LayoutState both = fixture.Settle($"both-axes-{cycle}");
                both.HorizontalBar.IsVisible.Should().BeTrue();
                both.VerticalBar.IsVisible.Should().BeTrue();
                both.Extent.Width.Should().BeGreaterThan(both.Viewport.Width);
                both.Extent.Height.Should().BeGreaterThan(both.Viewport.Height);
                fixture.Scroll.Offset = new Vector(both.Extent.Width - both.Viewport.Width, both.Extent.Height - both.Viewport.Height);
                LayoutState scrolled = fixture.Settle($"both-axes-scrolled-{cycle}");
                scrolled.Offset.X.Should().BeGreaterThan(0);
                scrolled.Offset.Y.Should().BeGreaterThan(0);

                fixture.Window.Width = SourceOwnerWidth;
                fixture.PostContents("subject", "refs");
                LayoutState widened = fixture.Settle($"wide-short-{cycle}");
                AssertFitsWithoutScrollbars(widened);
                widened.Owner.Size.Should().Be(new Size(SourceOwnerWidth, SourceOwnerHeight));
            }
        });
    }

    [AvaloniaTest]
    public void CommitInfo_should_reach_horizontal_overflow_when_only_the_required_vertical_gutter_makes_the_header_too_wide()
    {
        WithConfiguredSettings(() =>
        {
            using ViewportFixture probe = new("header-gutter-measurement", SourceOwnerWidth, "subject", "refs");
            LayoutState measured = probe.Settle("natural-header");
            int ownerWidth = (int)Math.Ceiling(measured.HeaderDesired.Width + (measured.VerticalBar.CrossAxisSize / 2));
            using ViewportFixture fixture = new("header-gutter-boundary", ownerWidth, "subject", "refs");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                fixture.PostContents("subject", "refs");
                LayoutState shortState = fixture.Settle($"header-fits-full-client-{cycle}");
                shortState.HeaderDesired.Width.Should().BeLessThanOrEqualTo(shortState.Owner.Width);
                shortState.HeaderDesired.Width.Should().BeGreaterThan(shortState.Owner.Width - shortState.VerticalBar.CrossAxisSize);
                AssertFitsWithoutScrollbars(shortState);

                fixture.PostContents(CreateOverflowMessage(RetainedMessage), RetainedRefs);
                LayoutState tallState = fixture.Settle($"header-exceeds-vertical-client-{cycle}");
                tallState.HorizontalBar.IsVisible.Should().BeTrue();
                tallState.VerticalBar.IsVisible.Should().BeTrue();
                tallState.Extent.Width.Should().BeGreaterThan(tallState.Viewport.Width);
                tallState.Extent.Height.Should().BeGreaterThan(tallState.Viewport.Height);
                fixture.Scroll.Offset = new Vector(tallState.Extent.Width - tallState.Viewport.Width,
                    tallState.Extent.Height - tallState.Viewport.Height);
                LayoutState scrolled = fixture.Settle($"header-gutter-scrolled-{cycle}");
                scrolled.Offset.X.Should().BeGreaterThan(0);
                scrolled.Offset.Y.Should().BeGreaterThan(0);
            }

            fixture.PostContents("subject", "refs");
            AssertFitsWithoutScrollbars(fixture.Settle("header-gutter-final-shrink"));
        });
    }

    private static void AssertFitsWithoutScrollbars(LayoutState state)
    {
        state.Table.Height.Should().BeLessThanOrEqualTo(state.Owner.Height);
        state.Table.Width.Should().Be(state.Owner.Width);
        state.HorizontalBar.IsVisible.Should().BeFalse("no actual contents exceed the unchanged full source client");
        state.VerticalBar.IsVisible.Should().BeFalse("a previous reduced viewport must not keep the other scrollbar alive");
        state.Viewport.Should().Be(state.Owner.Size);
        state.Extent.Width.Should().BeLessThanOrEqualTo(state.Viewport.Width);
        state.Extent.Height.Should().BeLessThanOrEqualTo(state.Viewport.Height);
        state.Offset.Should().Be(default(Vector));
        state.LayoutValid.Should().BeTrue();
    }

    private static void AssertSameFinalLayout(LayoutState expected, LayoutState actual)
    {
        actual.Owner.Should().Be(expected.Owner);
        actual.Table.Should().Be(expected.Table);
        actual.Header.Should().Be(expected.Header);
        actual.HeaderDesired.Should().Be(expected.HeaderDesired);
        actual.Body.Should().Be(expected.Body);
        actual.Refs.Should().Be(expected.Refs);
        actual.BodyContentsHeight.Should().Be(expected.BodyContentsHeight);
        actual.RefsContentsHeight.Should().Be(expected.RefsContentsHeight);
        actual.Viewport.Should().Be(expected.Viewport);
        actual.Extent.Should().Be(expected.Extent);
        actual.PresenterViewport.Should().Be(expected.PresenterViewport);
        actual.PresenterExtent.Should().Be(expected.PresenterExtent);
    }

    private static string CreateOverflowMessage(string message)
        => message + "\n" + string.Join("\n", Enumerable.Repeat("additional real overflow paragraph", 12));

    private static GitRevision CreateRevision()
    {
        long revisionTime = new DateTimeOffset(FixtureNowUtc.AddHours(-1)).ToUnixTimeSeconds();
        return new GitRevision(ObjectId.Parse("7de4d81c0230b6fafbaeddbdf411fe5ed4ac75be"))
        {
            Author = "nikola.begovic",
            Committer = "nikola.begovic",
            AuthorUnixTime = revisionTime,
            CommitUnixTime = revisionTime,
            ParentIds = [ObjectId.Parse("abcdef1234567890abcdef1234567890abcdef12")],
        };
    }

    private static string SelectNaturalBoundaryMessage()
    {
        using ViewportFixture probe = new("natural-content-selection", SourceOwnerWidth, RetainedMessage, RetainedRefs);
        LayoutState retained = probe.Settle("retained-message-natural");
        TestContext.Out.WriteLine($"Natural retained message: table={retained.Table}; owner={retained.Owner}; actual horizontal bar metric={retained.HorizontalBar.CrossAxisSize}. The retained native table473 is not assigned as a layout height.");
        if (IsWithinScrollbarGutter(retained))
        {
            return RetainedMessage;
        }

        // A fallback font may have different native line metrics. Select real text
        // paragraphs at the same fixed owner size, then create fresh controls from
        // that exact input; never pin a row height or resize the transition's host.
        string[] paragraphs = RetainedMessage.Replace("\r\n", "\n").Split('\n');
        for (int lines = 1; lines <= 64; lines++)
        {
            string message = string.Join("\n", Enumerable.Range(0, lines).Select(index => paragraphs[index % paragraphs.Length]));
            probe.PostContents(message, RetainedRefs);
            LayoutState measured = probe.Settle($"font-content-selection-{lines}", saveFrame: false);
            if (IsWithinScrollbarGutter(measured))
            {
                TestContext.Out.WriteLine($"Resolved font boundary uses {lines} real paragraphs: table={measured.Table}; owner={measured.Owner}.");
                return message;
            }
        }

        throw new AssertionException("The actual resolved font must provide naturally sized content inside the fixed owner's scrollbar-gutter boundary.");
    }

    private static bool IsWithinScrollbarGutter(LayoutState state)
    {
        state.HorizontalBar.CrossAxisSize.Should().BeGreaterThan(0);
        double spareHeight = state.Owner.Height - state.Table.Height;
        return spareHeight >= 0 && spareHeight < state.HorizontalBar.CrossAxisSize;
    }

    private static void WithConfiguredSettings(Action verify)
    {
        FieldInfo managerField = typeof(ThreadHelper).GetField("_taskManager", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(ThreadHelper).FullName, "_taskManager");
        object? originalManager = managerField.GetValue(null);
        SynchronizationContext? originalSynchronizationContext = SynchronizationContext.Current;
        WinFormsShims.Font originalUiFont = AppSettings.Font;
        WinFormsShims.Font originalCommitFont = AppSettings.CommitFont;
        bool originalShowAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        JoinableTaskContext? context = null;
        try
        {
            AvaloniaSynchronizationContext.InstallIfNeeded();
            context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
            ThreadHelper.JoinableTaskContext = context;
            AppSettings.Font = new WinFormsShims.Font("Segoe UI", 9);
            AppSettings.CommitFont = new WinFormsShims.Font("Segoe UI", 9);
            AppSettings.ShowAuthorAvatarInCommitInfo = true;
            AvaloniaFontSettings.ApplyAppSettings();
            verify();
        }
        finally
        {
            AppSettings.Font = originalUiFont;
            AppSettings.CommitFont = originalCommitFont;
            AppSettings.ShowAuthorAvatarInCommitInfo = originalShowAvatar;
            AvaloniaFontSettings.ApplyAppSettings();
            managerField.SetValue(null, originalManager);
            context?.Dispose();
            SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
        }
    }

    private sealed record BarState(
        string Orientation,
        string Visibility,
        bool IsVisible,
        bool IsEffectivelyVisible,
        Rect Bounds,
        double CrossAxisSize,
        double Maximum,
        double ViewportSize,
        double Value,
        Size DesiredSize,
        bool IsMeasureValid,
        bool IsArrangeValid);

    private sealed record TemplateGridState(
        Rect Bounds,
        Size DesiredSize,
        double FirstColumnWidth,
        double SecondColumnWidth,
        double FirstRowHeight,
        double SecondRowHeight,
        bool IsMeasureValid,
        bool IsArrangeValid);

    private sealed record LayoutState(
        Rect Owner,
        Rect Table,
        Rect Header,
        Size HeaderDesired,
        Rect Body,
        Rect Refs,
        int BodyContentsHeight,
        int RefsContentsHeight,
        int BodyCachedHeight,
        int RefsCachedHeight,
        Size Viewport,
        Size Extent,
        Vector Offset,
        Size PresenterViewport,
        Size PresenterExtent,
        Vector PresenterOffset,
        BarState HorizontalBar,
        BarState VerticalBar,
        TemplateGridState TemplateGrid,
        bool LayoutValid);

    private sealed record LayoutSample(string Stage, int Pass, string Phase, LayoutState State);

    private sealed record ContentInput(string Stage, string Body, string Refs);

    private sealed record ScrollChange(string Source, string Property, Size Viewport, Size Extent, bool HorizontalVisible, bool VerticalVisible);

    private sealed class ViewportFixture : IDisposable
    {
        private readonly CaptureFixtureClock _clock;
        private readonly string? _evidenceDirectory;
        private readonly List<LayoutSample> _samples = [];
        private readonly List<ContentInput> _inputs = [];
        private readonly List<ScrollChange> _scrollChanges = [];
        private readonly CommitInfo _control;
        private readonly ScrollContentPresenter _presenter;
        private readonly Grid _templateGrid;
        private readonly ScrollBar _horizontalBar;
        private readonly ScrollBar _verticalBar;
        private int _scrollChangeCount;

        public ViewportFixture(string label, int width, string message, string refs)
        {
            string? evidenceRoot = Environment.GetEnvironmentVariable(EvidenceEnvironmentVariable);
            if (!string.IsNullOrEmpty(evidenceRoot))
            {
                if (!Path.IsPathFullyQualified(evidenceRoot))
                {
                    throw new InvalidOperationException("Viewport evidence requires an explicit absolute directory.");
                }

                string caseName = string.Concat(TestContext.CurrentContext.Test.ID.Select(character =>
                    Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
                _evidenceDirectory = Path.Combine(evidenceRoot, $"{caseName}-{label}-{Guid.NewGuid():N}");
                Directory.CreateDirectory(_evidenceDirectory);
            }

            _control = new CommitInfo();
            Accessor.TableLayout.IsVisible = true;
            _clock = CaptureFixtureClock.Apply([Accessor.Header], FixtureNowUtc);
            Accessor.Header.CommandClicked += (_, _) => { };
            // A missing email keeps the actual avatar's sizing and fallback path but
            // prevents a layout regression from starting a network or cache request.
            Accessor.Header.ShowCommitInfo(CreateRevision(), [ObjectId.IndexId]);
            Accessor.CommitMessage.SetXHTMLText(message);
            Accessor.RevisionInfo.SetXHTMLText(refs);
            Window = new Window { Width = width, Height = SourceOwnerHeight, Content = _control };
            Window.Show();
            Scroll = _control.Content as ScrollViewer
                ?? throw new AssertionException("The real CommitInfo must retain its outer ScrollViewer.");
            Scroll.ApplyTemplate();
            _presenter = Scroll.GetVisualDescendants().OfType<ScrollContentPresenter>()
                .Single(presenter => ReferenceEquals(presenter.TemplatedParent, Scroll));
            _templateGrid = Scroll.GetVisualDescendants().OfType<Grid>()
                .Single(grid => ReferenceEquals(grid.TemplatedParent, Scroll));
            _horizontalBar = GetBar(Orientation.Horizontal);
            _verticalBar = GetBar(Orientation.Vertical);
            Scroll.PropertyChanged += ObserveScrollChange;
            _horizontalBar.PropertyChanged += ObserveScrollChange;
            _verticalBar.PropertyChanged += ObserveScrollChange;
        }

        public Window Window { get; }

        public ScrollViewer Scroll { get; }

        public CommitInfo.TestAccessor Accessor => _control.GetTestAccessor();

        public void Dispose()
        {
            try
            {
                if (_evidenceDirectory is not null)
                {
                    object evidence = new
                    {
                        framework = "avalonia-headless",
                        testName = TestContext.CurrentContext.Test.FullName,
                        testId = TestContext.CurrentContext.Test.ID,
                        fixtureNowUtc = FixtureNowUtc,
                        fonts = new
                        {
                            ui = new { family = Accessor.RevisionInfo.FontFamily.Name, sizeDip = Accessor.RevisionInfo.FontSize, Accessor.RevisionInfo.LineHeight },
                            commit = new { family = Accessor.CommitMessage.FontFamily.Name, sizeDip = Accessor.CommitMessage.FontSize, Accessor.CommitMessage.LineHeight },
                        },
                        inputs = _inputs,
                        samples = _samples,
                        scrollChangeCount = _scrollChangeCount,
                        firstScrollChanges = _scrollChanges,
                    };
                    using FileStream output = new(Path.Combine(_evidenceDirectory, "viewport.json"), FileMode.CreateNew);
                    JsonSerializer.Serialize(output, evidence, EvidenceJsonOptions);
                }
            }
            finally
            {
                Scroll.PropertyChanged -= ObserveScrollChange;
                _horizontalBar.PropertyChanged -= ObserveScrollChange;
                _verticalBar.PropertyChanged -= ObserveScrollChange;
                Window.Close();
                _clock.Dispose();
                Dispatcher.UIThread.RunJobs();
            }
        }

        public void PostContents(string message, string refs)
            => Dispatcher.UIThread.Post(() =>
            {
                Accessor.CommitMessage.SetXHTMLText(message);
                Accessor.RevisionInfo.SetXHTMLText(refs);
            });

        public LayoutState Settle(string stage, bool saveFrame = true)
        {
            LayoutState? previous = null;
            int stablePasses = 0;
            for (int pass = 0; pass < 12; pass++)
            {
                int changesBeforePass = _scrollChangeCount;
                Dispatcher.UIThread.RunJobs();
                Window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                LayoutState beforeRender = ReadState();
                _samples.Add(new LayoutSample(stage, pass, "beforeRender", beforeRender));
                // The headless capture API itself drains jobs and render ticks. A
                // snapshot taken before it is not evidence of the painted layout.
                using WriteableBitmap frame = Window.CaptureRenderedFrame()
                    ?? throw new AssertionException("The headless viewport must produce an actual rendered frame.");
                LayoutState state = ReadState();
                _samples.Add(new LayoutSample(stage, pass, "afterRender", state));
                if (state.LayoutValid && state == beforeRender && state == previous
                    && changesBeforePass == _scrollChangeCount
                    && Accessor.Avatar.Bounds.Height == AppSettings.AuthorImageSizeInCommitInfo)
                {
                    stablePasses++;
                    if (stablePasses == 2)
                    {
                        _inputs.Add(new ContentInput(stage, Accessor.CommitMessage.GetPlainText(), Accessor.RevisionInfo.GetPlainText()));
                        if (saveFrame)
                        {
                            SaveFrame(stage, frame);
                        }

                        TestContext.Out.WriteLine($"{stage}: owner={state.Owner} table={state.Table} viewport={state.Viewport} extent={state.Extent} offset={state.Offset} H={state.HorizontalBar.IsVisible}/{state.HorizontalBar.Maximum} V={state.VerticalBar.IsVisible}/{state.VerticalBar.Maximum}");
                        return state;
                    }
                }
                else
                {
                    stablePasses = 0;
                }

                previous = state;
            }

            using (WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The unsettled viewport must retain its actual rendered frame."))
            {
                _samples.Add(new LayoutSample(stage, 12, "unsettledAfterRender", ReadState()));
                SaveFrame(stage + "-unsettled", frame);
            }

            throw new AssertionException($"The actual fixed-size CommitInfo did not settle: {JsonSerializer.Serialize(previous, EvidenceJsonOptions)}");
        }

        private ScrollBar GetBar(Orientation orientation)
            => Scroll.GetVisualDescendants().OfType<ScrollBar>().Single(bar =>
                ReferenceEquals(bar.TemplatedParent, Scroll) && bar.Orientation == orientation);

        private LayoutState ReadState()
        {
            Layoutable[] layout = [Window, _control, Accessor.TableLayout, Accessor.Header,
                Accessor.CommitMessage, Accessor.RevisionInfo, Scroll, _presenter, _templateGrid];
            TemplateGridState template = new(_templateGrid.Bounds, _templateGrid.DesiredSize,
                _templateGrid.ColumnDefinitions[0].ActualWidth, _templateGrid.ColumnDefinitions[1].ActualWidth,
                _templateGrid.RowDefinitions[0].ActualHeight, _templateGrid.RowDefinitions[1].ActualHeight,
                _templateGrid.IsMeasureValid, _templateGrid.IsArrangeValid);
            return new LayoutState(_control.Bounds, Accessor.TableLayout.Bounds,
                Accessor.Header.Bounds, Accessor.Header.DesiredSize, Accessor.CommitMessage.Bounds, Accessor.RevisionInfo.Bounds,
                Accessor.CommitMessage.ContentsHeight, Accessor.RevisionInfo.ContentsHeight,
                Accessor.CommitMessageHeight, Accessor.RevisionInfoHeight,
                Scroll.Viewport, Scroll.Extent, Scroll.Offset, _presenter.Viewport, _presenter.Extent, _presenter.Offset,
                ReadBar(_horizontalBar), ReadBar(_verticalBar), template,
                layout.All(element => element.IsMeasureValid && element.IsArrangeValid)
                    && new[] { _horizontalBar, _verticalBar }.Where(bar => bar.IsVisible)
                        .All(bar => bar.IsMeasureValid && bar.IsArrangeValid));
        }

        private static BarState ReadBar(ScrollBar bar)
            => new(bar.Orientation.ToString(), bar.Visibility.ToString(), bar.IsVisible, bar.IsEffectivelyVisible,
                bar.Bounds, bar.Orientation == Orientation.Horizontal ? bar.Height : bar.Width, bar.Maximum, bar.ViewportSize, bar.Value,
                bar.DesiredSize, bar.IsMeasureValid, bar.IsArrangeValid);

        private void ObserveScrollChange(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Visual.IsVisibleProperty && e.Property != ScrollViewer.ViewportProperty && e.Property != ScrollViewer.ExtentProperty)
            {
                return;
            }

            _scrollChangeCount++;
            if (_scrollChanges.Count < 256)
            {
                string source = sender is ScrollBar bar ? bar.Orientation.ToString() : nameof(ScrollViewer);
                _scrollChanges.Add(new ScrollChange(source, e.Property.Name, Scroll.Viewport, Scroll.Extent,
                    _horizontalBar.IsVisible, _verticalBar.IsVisible));
            }
        }

        private void SaveFrame(string stage, WriteableBitmap frame)
        {
            if (_evidenceDirectory is null)
            {
                return;
            }

            using FileStream output = new(Path.Combine(_evidenceDirectory, stage + ".png"), FileMode.CreateNew);
            frame.Save(output, PngBitmapEncoderOptions.Default);
        }
    }
}

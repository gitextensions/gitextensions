using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridViewportTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(600, 2)]
    [TestCase(600, 40)]
    [TestCase(900, 2)]
    [TestCase(900, 40)]
    [TestCase(1520, 2)]
    [TestCase(1520, 40)]
    public void Revision_rows_should_fill_the_native_viewport_without_extending_beneath_its_scrollbar(int width, int revisionCount)
    {
        RevisionGridControl control = new();
        control.GetTestAccessor().SetRevisions(CreateRevisions(revisionCount));
        Window window = new() { Width = width, Height = 160, Content = control };
        window.Show();
        try
        {
            Settle(window);

            AssertViewport(control, shouldScroll: revisionCount > 2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(600)]
    [TestCase(900)]
    [TestCase(1520)]
    public void Revision_fill_column_should_release_and_restore_the_scrollbar_reservation_after_viewport_resize(int width)
    {
        RevisionGridControl control = new();
        control.GetTestAccessor().SetRevisions(CreateRevisions(40));
        Window window = new() { Width = width, Height = 160, Content = control };
        window.Show();
        try
        {
            Settle(window);
            double narrowMessageWidth = AssertViewport(control, shouldScroll: true);
            ScrollBar vertical = FindVerticalScrollBar(control);
            double scrollbarWidth = vertical.Bounds.Width;

            window.Height = 3000;
            Settle(window);
            double fullMessageWidth = AssertViewport(control, shouldScroll: false);
            fullMessageWidth.Should().Be(narrowMessageWidth + scrollbarWidth);

            window.Height = 160;
            Settle(window);
            AssertViewport(control, shouldScroll: true).Should().Be(narrowMessageWidth);
            Rect settledBounds = FindViewport(control).Bounds;
            Settle(window);
            FindViewport(control).Bounds.Should().Be(settledBounds);
            AssertViewport(control, shouldScroll: true).Should().Be(narrowMessageWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Revision_fill_column_should_follow_the_actual_scrollbar_when_history_grows_and_shrinks()
    {
        RevisionGridControl control = new();
        control.GetTestAccessor().SetRevisions(CreateRevisions(2));
        Window window = new() { Width = 900, Height = 160, Content = control };
        window.Show();
        try
        {
            Settle(window);
            double fullMessageWidth = AssertViewport(control, shouldScroll: false);

            control.GetTestAccessor().SetRevisions(CreateRevisions(40));
            Settle(window);
            double reservedMessageWidth = AssertViewport(control, shouldScroll: true);
            fullMessageWidth.Should().Be(reservedMessageWidth + FindVerticalScrollBar(control).Bounds.Width);

            control.GetTestAccessor().SetRevisions(CreateRevisions(2));
            Settle(window);
            AssertViewport(control, shouldScroll: false).Should().Be(fullMessageWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(2)]
    [TestCase(40)]
    public void Revision_columns_should_remain_reachable_at_their_source_minimum_during_horizontal_scroll_and_regrowth(int revisionCount)
    {
        RevisionGridControl control = new();
        control.GetTestAccessor().SetRevisions(CreateRevisions(revisionCount));
        Window window = new() { Width = 200, Height = 160, Content = control };
        window.Show();
        try
        {
            Settle(window);
            AssertHorizontalOverflow(control, window, shouldScrollVertically: revisionCount > 2);

            window.Width = 900;
            Settle(window);
            ScrollViewer scrollViewer = FindScrollViewer(control);
            scrollViewer.Offset.X.Should().Be(0);
            FindHorizontalScrollBar(control).IsVisible.Should().BeFalse();
            AssertViewport(control, shouldScroll: revisionCount > 2);

            window.Width = 200;
            Settle(window);
            AssertHorizontalOverflow(control, window, shouldScrollVertically: revisionCount > 2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(2)]
    [TestCase(40)]
    public void Revision_horizontal_extent_should_follow_column_widths_not_an_unbounded_commit_caption(int revisionCount)
    {
        RevisionGridControl control = new();
        GitRevision[] revisions = CreateRevisions(revisionCount).ToArray();
        control.GetTestAccessor().SetRevisions(revisions);
        Window window = new() { Width = 200, Height = 160, Content = control };
        window.Show();
        try
        {
            Settle(window);
            double originalExtent = FindScrollViewer(control).Extent.Width;
            foreach (GitRevision revision in revisions)
            {
                revision.Subject = new string('W', 1000);
            }

            control.GetTestAccessor().SetRevisions(revisions);
            Settle(window);
            AssertHorizontalOverflow(control, window, shouldScrollVertically: revisionCount > 2);
            FindScrollViewer(control).Extent.Width.Should().Be(originalExtent);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(2)]
    [TestCase(40)]
    public void Revision_horizontal_extent_should_remeasure_after_fixed_column_width_changes(int revisionCount)
    {
        RevisionGridControl control = new();
        control.GetTestAccessor().SetRevisions(CreateRevisions(revisionCount));
        Window window = new() { Width = 900, Height = 160, Content = control };
        window.Show();
        try
        {
            Settle(window);
            AssertViewport(control, shouldScroll: revisionCount > 2);
            var author = control.ColumnProviders.Single(provider => provider.Name == "Author Name");
            GridLength originalWidth = author.Column.Width;
            author.Column.Width = new GridLength(1000);
            control.ApplyColumnSettings();
            Settle(window);
            AssertHorizontalOverflow(control, window, shouldScrollVertically: revisionCount > 2);

            author.Column.Width = originalWidth;
            control.ApplyColumnSettings();
            Settle(window);
            FindScrollViewer(control).Offset.X.Should().Be(0);
            FindHorizontalScrollBar(control).IsVisible.Should().BeFalse();
            AssertViewport(control, shouldScroll: revisionCount > 2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(9, false)]
    [TestCase(11, false)]
    [TestCase(14, false)]
    [TestCase(9, true)]
    [TestCase(11, true)]
    [TestCase(14, true)]
    public void Nestled_ref_labels_should_use_actual_fill_bounds_after_initial_measure_shrink_and_regrowth(int points, bool includeTag)
    {
        bool showRemotes = AppSettings.ShowRemoteBranches;
        bool showTags = AppSettings.ShowTags;
        Window? window = null;
        try
        {
            AppSettings.ShowRemoteBranches = true;
            AppSettings.ShowTags = true;
            GitRevision revision = CreateRevisions(1).Single();
            IGitModule module = Substitute.For<IGitModule>();
            IGitRef local = CreateRef(module, revision.ObjectId, "main", remote: false);
            IGitRef remote = CreateRef(module, revision.ObjectId, "origin/main", remote: true);
            local.IsTrackingRemote(remote).Returns(true);
            revision.Refs = [local, remote];
            if (includeTag)
            {
                IGitRef tag = CreateRef(module, revision.ObjectId, "v1", remote: false);
                tag.IsHead.Returns(false);
                tag.IsTag.Returns(true);
                tag.CompleteName.Returns("refs/tags/v1");
                revision.Refs = [.. revision.Refs, tag];
            }

            RevisionGridControl control = new() { FontSize = points * 96d / 72 };
            control.GetTestAccessor().SetRevisions([revision]);
            window = new Window { Width = 1000, Height = 180, Content = control };
            window.Show();
            Settle(window);
            AssertNaturalCapsules(control, window, includeTag);

            window.Width = 200;
            Settle(window);
            RevisionGridRefRenderer.RefLabelControl first = control.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>().First();
            first.CapsuleBounds.Width.Should().BeLessThan(first.GetCapsuleWidth(double.PositiveInfinity));

            window.Width = 1000;
            Settle(window);
            AssertNaturalCapsules(control, window, includeTag);
        }
        finally
        {
            window?.Close();
            AppSettings.ShowRemoteBranches = showRemotes;
            AppSettings.ShowTags = showTags;
        }
    }

    private static void AssertNaturalCapsules(RevisionGridControl control, Window window, bool includeTag)
    {
        using var frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The real revision viewport must render its retained ref labels.");
        RevisionGridRefRenderer.RefLabelControl[] labels = control.GetVisualDescendants()
            .OfType<RevisionGridRefRenderer.RefLabelControl>().ToArray();
        labels.Should().HaveCount(includeTag ? 3 : 2);
        foreach (RevisionGridRefRenderer.RefLabelControl label in labels)
        {
            label.CapsuleBounds.Width.Should().Be(label.GetCapsuleWidth(double.PositiveInfinity),
                "a constrained first measure must not retain minimum-width capsules after the fill slot grows");
        }
    }

    private static void AssertHorizontalOverflow(RevisionGridControl control, Window window, bool shouldScrollVertically)
    {
        ListBox revisions = control.GetTestAccessor().Revisions;
        ScrollViewer.GetHorizontalScrollBarVisibility(revisions).Should().Be(ScrollBarVisibility.Auto);
        ScrollViewer scrollViewer = FindScrollViewer(control);
        ScrollContentPresenter viewport = FindViewport(control);
        ScrollBar horizontal = FindHorizontalScrollBar(control);
        horizontal.IsVisible.Should().BeTrue();
        horizontal.Bounds.Height.Should().Be(17);
        FindVerticalScrollBar(control).IsVisible.Should().Be(shouldScrollVertically);
        Grid.GetRowSpan(viewport).Should().Be(1);
        viewport.Bounds.Bottom.Should().Be(horizontal.Bounds.Top);
        double sourceMinimumWidth = control.ColumnProviders.Where(provider => provider.Column.IsVisible)
            .Sum(provider => provider.Column.Width.IsStar
                ? provider.Column.MinimumWidth
                : Math.Max(provider.Column.MinimumWidth, provider.Column.Width.Value));
        scrollViewer.Extent.Width.Should().Be(sourceMinimumWidth);
        int messageIndex = control.ColumnProviders.Single(provider => provider.Name == "Message").Index;
        Grid[] rows = revisions.GetVisualDescendants().OfType<Grid>()
            .Where(row => row.Classes.Contains("revision-row")).ToArray();
        rows.Should().NotBeEmpty();
        foreach (Grid row in rows)
        {
            row.Bounds.Width.Should().Be(sourceMinimumWidth);
            row.ColumnDefinitions[messageIndex].ActualWidth.Should().Be(
                control.ColumnProviders[messageIndex].Column.MinimumWidth);
        }

        scrollViewer.Offset = new Vector(scrollViewer.Extent.Width - scrollViewer.Viewport.Width, 0);
        Settle(window);
        scrollViewer.Offset.X.Should().BeGreaterThan(0);
        foreach (Grid row in revisions.GetVisualDescendants().OfType<Grid>()
                     .Where(row => row.Classes.Contains("revision-row")))
        {
            Point rightEdge = row.TranslatePoint(new Point(row.Bounds.Width, 0), viewport)
                ?? throw new InvalidOperationException("The scrolled revision row must remain attached to its viewport.");
            rightEdge.X.Should().Be(viewport.Bounds.Width,
                "the last source column must be reachable without being hidden beneath either scrollbar");
        }
    }

    private static double AssertViewport(RevisionGridControl control, bool shouldScroll)
    {
        ListBox revisions = control.GetTestAccessor().Revisions;
        ScrollViewer scrollViewer = revisions.GetVisualDescendants().OfType<ScrollViewer>().Single();
        ScrollContentPresenter viewport = FindViewport(control);
        ScrollBar vertical = FindVerticalScrollBar(control);
        ScrollViewer.GetAllowAutoHide(revisions).Should().BeFalse();
        scrollViewer.AllowAutoHide.Should().BeFalse();
        vertical.AllowAutoHide.Should().BeFalse();
        vertical.IsVisible.Should().Be(shouldScroll);
        Grid.GetColumnSpan(viewport).Should().Be(1,
            "the source DataGridView reserves its scrollbar column instead of overlaying cells");
        viewport.Bounds.Width.Should().Be(scrollViewer.Bounds.Width - (shouldScroll ? vertical.Bounds.Width : 0));
        if (shouldScroll)
        {
            vertical.Bounds.Width.Should().Be(17);
            viewport.Bounds.Right.Should().Be(vertical.Bounds.Left);
        }

        Grid[] rows = revisions.GetVisualDescendants().OfType<Grid>()
            .Where(row => row.Classes.Contains("revision-row")).ToArray();
        rows.Should().NotBeEmpty();
        int messageIndex = control.ColumnProviders.Single(provider => provider.Name == "Message").Index;
        foreach (Grid row in rows)
        {
            row.Bounds.Width.Should().Be(viewport.Bounds.Width);
            row.ColumnDefinitions.Sum(column => column.ActualWidth).Should().Be(viewport.Bounds.Width);
            double fixedWidth = row.ColumnDefinitions.Where((_, index) => index != messageIndex)
                .Sum(column => column.ActualWidth);
            row.ColumnDefinitions[messageIndex].ActualWidth.Should().Be(viewport.Bounds.Width - fixedWidth);
            row.ColumnDefinitions[messageIndex].Width.IsStar.Should().BeTrue();
        }

        return rows[0].ColumnDefinitions[messageIndex].ActualWidth;
    }

    private static IEnumerable<GitRevision> CreateRevisions(int count)
        => Enumerable.Range(1, count).Select(index => new GitRevision(ObjectId.Parse(index.ToString("x40")))
        {
            Subject = "Viewport revision " + index,
            Author = "Viewport Author",
            AuthorEmail = "viewport@example.invalid",
            AuthorUnixTime = 1_700_000_000,
            Committer = "Viewport Author",
            CommitterEmail = "viewport@example.invalid",
            CommitUnixTime = 1_700_000_000,
            Notes = string.Empty,
        });

    private static IGitRef CreateRef(IGitModule module, ObjectId objectId, string name, bool remote)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Module.Returns(module);
        gitRef.ObjectId.Returns(objectId);
        gitRef.Guid.Returns(objectId.ToString());
        gitRef.Name.Returns(name);
        gitRef.LocalName.Returns(remote ? name[(name.IndexOf('/') + 1)..] : name);
        gitRef.CompleteName.Returns((remote ? "refs/remotes/" : "refs/heads/") + name);
        gitRef.IsHead.Returns(!remote);
        gitRef.IsRemote.Returns(remote);
        gitRef.Remote.Returns(remote ? "origin" : string.Empty);
        return gitRef;
    }

    private static ScrollContentPresenter FindViewport(RevisionGridControl control)
        => control.GetTestAccessor().Revisions.GetVisualDescendants().OfType<ScrollContentPresenter>().Single();

    private static ScrollViewer FindScrollViewer(RevisionGridControl control)
        => control.GetTestAccessor().Revisions.GetVisualDescendants().OfType<ScrollViewer>().Single();

    private static ScrollBar FindHorizontalScrollBar(RevisionGridControl control)
        => control.GetTestAccessor().Revisions.GetVisualDescendants().OfType<ScrollBar>()
            .Single(scrollbar => scrollbar.Name == "PART_HorizontalScrollBar");

    private static ScrollBar FindVerticalScrollBar(RevisionGridControl control)
        => control.GetTestAccessor().Revisions.GetVisualDescendants().OfType<ScrollBar>()
            .Single(scrollbar => scrollbar.Name == "PART_VerticalScrollBar");

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}

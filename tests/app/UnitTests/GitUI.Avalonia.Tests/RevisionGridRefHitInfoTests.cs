using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Compat;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using Font = GitExtensions.Shims.WinForms.Font;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridRefHitInfoTests
{
    private const int SourceRightMargin = 5;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [Test]
    [TestCaseSource(nameof(ShapeAllocations))]
    public void Contains_should_preserve_every_integer_pixel_in_odd_and_even_source_maps(
        string shapeName, int height, int pointWidth, int[] slants)
    {
        RefLabelShape shape = Enum.Parse<RefLabelShape>(shapeName);
        Rect bounds = new(12, 14, 47, height);
        RefLabelHitInfo hit = new(bounds, shape, pointWidth, GitRef: null, StashReflogSelector: null);
        for (int y = 13; y <= 14 + height; y++)
        {
            for (int x = 11; x <= 59; x++)
            {
                bool expected = false;
                if (y >= bounds.Top && y < bounds.Bottom)
                {
                    int slant = slants[y - (int)bounds.Top];
                    int first = shape switch
                    {
                        RefLabelShape.PointLeft => (int)bounds.Left + slant,
                        RefLabelShape.NotchLeft => (int)bounds.Left + pointWidth - slant,
                        _ => (int)bounds.Left,
                    };
                    int last = shape switch
                    {
                        RefLabelShape.PointRight => Math.Min((int)bounds.Right - 1, (int)bounds.Right - slant),
                        RefLabelShape.NotchRight => Math.Min((int)bounds.Right - 1, (int)bounds.Right - pointWidth + slant),
                        _ => (int)bounds.Right - 1,
                    };
                    expected = x >= first && x <= last;
                }

                hit.Contains(new Point(x, y)).Should().Be(expected,
                    $"the source {shape} integer ownership map at ({x}, {y}) must be preserved");
            }
        }
    }

    [Test]
    [TestCase(0, 17)]
    [TestCase(-1, 18)]
    [TestCase(8, 0)]
    [TestCase(8, 1)]
    public void Contains_should_preserve_source_degenerate_slant_and_empty_rect_rules(int pointWidth, int height)
    {
        Rect bounds = new(12, 14, 47, height);
        foreach (RefLabelShape shape in Enum.GetValues<RefLabelShape>())
        {
            RefLabelHitInfo hit = new(bounds, shape, pointWidth, GitRef: null, StashReflogSelector: null);
            for (int y = 13; y <= 14 + height; y++)
            {
                for (int x = 11; x <= 59; x++)
                {
                    hit.Contains(new Point(x, y)).Should().Be(
                        x >= bounds.Left && x < bounds.Right && y >= bounds.Top && y < bounds.Bottom);
                }
            }
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(Shapes))]
    public void Retained_label_should_use_its_actual_integer_capsule_for_hit_testing(string shapeName)
    {
        using SettingsScope settings = new();
        RefLabelShape shape = Enum.Parse<RefLabelShape>(shapeName);
        IGitRef gitRef = CreateRef(Substitute.For<IGitModule>(), ObjectId.Random(), "main", isRemote: false);
        RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabel(gitRef, "main", shape, fill: true);
        label.FontFamily = new FontFamily("Segoe UI");
        label.FontSize = 12;
        label.HorizontalAlignment = HorizontalAlignment.Left;
        Window window = new() { Width = 240, Height = 80, Content = label };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Rect bounds = label.CapsuleBounds;
            bounds.Width.Should().BeGreaterThan(0);
            bounds.Height.Should().BeGreaterThan(0);
            bounds.X.Should().Be(Math.Truncate(bounds.X));
            bounds.Y.Should().Be(Math.Truncate(bounds.Y));
            bounds.Width.Should().Be(Math.Truncate(bounds.Width));
            bounds.Height.Should().Be(Math.Truncate(bounds.Height));
            RefLabelHitInfo hit = new(bounds, shape, label.HitPointWidth, gitRef, StashReflogSelector: null);
            for (int y = -1; y <= label.Bounds.Height; y++)
            {
                for (int x = -1; x <= label.Bounds.Width; x++)
                {
                    Point point = new(x, y);
                    label.Contains(point).Should().Be(hit.Contains(point));
                }
            }

            label.Contains(new Point(bounds.Right, bounds.Top)).Should().BeFalse();
            label.Contains(new Point(bounds.Left, bounds.Bottom)).Should().BeFalse();
            if (shape == RefLabelShape.Rect)
            {
                // The source hit rectangle owns corners outside the rounded paint path.
                label.Contains(bounds.TopLeft).Should().BeTrue();
                label.Contains(new Point(bounds.Right - 1, bounds.Top)).Should().BeTrue();
                label.Contains(new Point(bounds.Left, bounds.Bottom - 1)).Should().BeTrue();
                label.Contains(new Point(bounds.Right - 1, bounds.Bottom - 1)).Should().BeTrue();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Standalone_multiline_point_label_should_hit_with_its_source_font_space_width(bool selected)
    {
        using SettingsScope settings = new();
        IGitRef gitRef = CreateRef(Substitute.For<IGitModule>(), ObjectId.Random(), "release", isRemote: false);
        gitRef.IsHead.Returns(false);
        gitRef.IsTag.Returns(true);
        gitRef.IsSelected.Returns(selected);
        RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabel(
            gitRef, "release\nsecond-line", RefLabelShape.PointLeft, fill: true);
        label.FontFamily = new FontFamily("Segoe UI");
        label.FontSize = 12;
        label.HorizontalAlignment = HorizontalAlignment.Left;
        label.VerticalAlignment = VerticalAlignment.Top;
        Window window = new() { Width = 360, Height = 100, Content = label };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            label.FontWeight.Should().Be(selected ? FontWeight.Bold : FontWeight.Normal);
            int sourcePointWidth = RevisionGridRefRenderer.GetPointWidth(label);
            label.HitPointWidth.Should().Be(sourcePointWidth,
                "DrawSeparateRef uses the current NormalFont/BoldFont space height, not the painted caption height");
            label.PointWidth.Should().BeGreaterThan(sourcePointWidth);
            Rect bounds = label.CapsuleBounds;
            RefLabelHitInfo sourceHit = new(bounds, label.Shape, sourcePointWidth, gitRef, StashReflogSelector: null);
            RefLabelHitInfo captionHeightHit = new(bounds, label.Shape, label.PointWidth, gitRef, StashReflogSelector: null);
            bool foundDifferentOwnership = false;
            for (int y = 0; y < label.Bounds.Height; y++)
            {
                for (int x = 0; x < label.Bounds.Width; x++)
                {
                    Point point = new(x, y);
                    bool expected = sourceHit.Contains(point);
                    label.Contains(point).Should().Be(expected);
                    foundDifferentOwnership |= expected != captionHeightHit.Contains(point);
                }
            }

            foundDifferentOwnership.Should().BeTrue(
                "the multiline caption must distinguish the source font-space hit boundary from its larger painted point");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("None", true)]
    [TestCase("Head", false)]
    [TestCase("HeadMergeSource", false)]
    [TestCase("WorkingDirectory", false)]
    [TestCase("WorkingDirectory", true)]
    public void DrawRefEx_should_preserve_its_explicit_effective_icon_independently_of_ref_selection(
        string iconName, bool selected)
    {
        using SettingsScope settings = new();
        IGitRef gitRef = CreateRef(Substitute.For<IGitModule>(), ObjectId.Random(), "main", isRemote: false);
        gitRef.IsSelected.Returns(selected);
        RefLabelIcon icon = Enum.Parse<RefLabelIcon>(iconName);
        RefLabelIcon expectedIcon = icon is RefLabelIcon.Head or RefLabelIcon.HeadMergeSource
            ? icon
            : RefLabelIcon.None;
        (RevisionGridRefRenderer.RefLabelControl label, Action? highlight) = RevisionGridRefRenderer.DrawRefEx(
            isRowSelected: false, gitRef, "main", icon);
        (RevisionGridRefRenderer.RefLabelControl withoutIcon, _) = RevisionGridRefRenderer.DrawRefEx(
            isRowSelected: false, gitRef, "main", RefLabelIcon.None);
        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        withoutIcon.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        label.Icon.Should().Be(expectedIcon);
        highlight.Should().BeNull();
        label.DesiredSize.Width.Should().Be(withoutIcon.DesiredSize.Width
            + (expectedIcon == RefLabelIcon.None ? 0 : (int)RevisionGridControl.GetRowHeight(label) / 2));
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Narrow_retained_pair_should_join_at_the_clipped_first_capsule_and_limit_the_remote_to_remaining_width(
        bool narrowerThanPoint)
    {
        using SettingsScope settings = new();
        IGitModule module = Substitute.For<IGitModule>();
        ObjectId objectId = ObjectId.Random();
        IGitRef local = CreateRef(module, objectId, "long-local-branch-name-for-source-clipping", isRemote: false);
        IGitRef remote = CreateRef(module, objectId, "origin/main", isRemote: true);
        local.IsTrackingRemote(remote).Returns(true);
        Control pair = RevisionGridRefRenderer.CreateLabels([local, remote]).Single();
        pair.VerticalAlignment = VerticalAlignment.Center;
        Window window = new() { Width = 1000, Height = 100, Content = pair };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            RevisionGridRefRenderer.RefLabelControl[] labels =
                [.. pair.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()];
            RevisionGridRefRenderer.RefLabelControl first = labels[0];
            RevisionGridRefRenderer.RefLabelControl second = labels[1];
            Rect firstNatural = first.CapsuleBounds;
            Rect secondNatural = second.CapsuleBounds;
            int sourcePointWidth = first.HitPointWidth;
            window.Width = narrowerThanPoint
                ? Math.Max(1, sourcePointWidth / 2)
                : Math.Max(sourcePointWidth + 1, (int)firstNatural.Width / 2);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            int availableWidth = (int)pair.Bounds.Width;
            int firstWidth = Math.Min(availableWidth, (int)firstNatural.Width);
            int secondX = Math.Max(0, firstWidth - sourcePointWidth + 1);
            int secondWidth = Math.Min(availableWidth - secondX, (int)secondNatural.Width);
            first.CapsuleBounds.Should().Be(new Rect(firstNatural.X, firstNatural.Y, firstWidth, firstNatural.Height));
            second.Bounds.X.Should().Be(secondX);
            second.CapsuleBounds.Should().Be(new Rect(secondNatural.X, secondNatural.Y, secondWidth, secondNatural.Height));
            first.CapsuleBounds.Right.Should().BeLessThanOrEqualTo(availableWidth);
            (second.Bounds.X + second.CapsuleBounds.Right).Should().BeLessThanOrEqualTo(availableWidth);
            second.HitPointWidth.Should().Be(sourcePointWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Actual_message_cell_should_offer_the_nestled_pair_only_its_finite_remaining_column_width(bool minimumColumn)
    {
        using GridFixture fixture = new(nestled: true, localName: "long-local-branch-name-for-source-clipping",
            additionalTag: true);
        RevisionGridRefRenderer.RefLabelControl first = fixture.Labels[0];
        RevisionGridRefRenderer.RefLabelControl second = fixture.Labels[1];
        RevisionGridRefRenderer.RefLabelControl third = fixture.Labels[2];
        Rect firstNatural = first.CapsuleBounds;
        Rect secondNatural = second.CapsuleBounds;
        Rect thirdNatural = third.CapsuleBounds;
        fixture.Provider.Column.Width = new GridLength(minimumColumn
            ? fixture.Provider.Column.MinimumWidth
            : Math.Max(fixture.Provider.Column.MinimumWidth, (int)firstNatural.Width / 2));
        fixture.Grid.ApplyColumnSettings();
        fixture.Settle("narrow-column");
        first = fixture.Labels[0];
        second = fixture.Labels[1];
        third = fixture.Labels[2];
        Control pair = (Control)first.Parent!;
        Control contentPanel = (Control)pair.Parent!;
        int availableWidth = (int)contentPanel.Bounds.Width;
        int firstWidth = Math.Min(availableWidth, (int)firstNatural.Width);
        int secondX = Math.Max(0, firstWidth - first.HitPointWidth + 1);
        int secondWidth = Math.Min(availableWidth - secondX, (int)secondNatural.Width);
        int thirdX = secondX + secondWidth + SourceRightMargin;

        availableWidth.Should().BeLessThan((int)firstNatural.Width);
        first.CapsuleBounds.Should().Be(new Rect(firstNatural.X, firstNatural.Y, firstWidth, firstNatural.Height));
        second.Bounds.X.Should().Be(secondX);
        second.CapsuleBounds.Should().Be(new Rect(secondNatural.X, secondNatural.Y, secondWidth, secondNatural.Height));
        (second.Bounds.X + second.CapsuleBounds.Right).Should().BeLessThanOrEqualTo(availableWidth);
        third.TranslatePoint(default, contentPanel)!.Value.X.Should().Be(thirdX,
            "the source advances from the clipped remote capsule and its right margin, not capped pair DesiredSize");
        third.CapsuleBounds.Width.Should().Be(Math.Min(Math.Max(0, availableWidth - thirdX), thirdNatural.Width));
        fixture.Provider.HitTest(0, second.TranslatePoint(
            new Point(second.CapsuleBounds.Right, second.CapsuleBounds.Center.Y), fixture.Grid)!.Value)
            .Should().BeNull("the source clipped rectangle's right edge is exclusive");
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Retained_nestled_pair_should_preserve_first_label_geometry_and_ordered_grid_hits(bool remoteFirst)
    {
        using GridFixture fixture = new(nestled: true, remoteFirst);
        RevisionGridRefRenderer.RefLabelControl first = fixture.Labels[0];
        RevisionGridRefRenderer.RefLabelControl second = fixture.Labels[1];
        first.Shape.Should().Be(remoteFirst ? RefLabelShape.NotchRight : RefLabelShape.PointRight);
        second.Shape.Should().Be(remoteFirst ? RefLabelShape.PointLeft : RefLabelShape.NotchLeft);
        first.HitPointWidth.Should().Be(first.PointWidth);
        second.HitPointWidth.Should().Be(first.PointWidth,
            "the source registers both nestled hit regions with the first branch font's point width");

        Point firstOrigin = first.TranslatePoint(default, fixture.Grid)!.Value;
        Point secondOrigin = second.TranslatePoint(default, fixture.Grid)!.Value;
        secondOrigin.X.Should().Be(firstOrigin.X + first.CapsuleBounds.Right - first.PointWidth + 1);
        Rect firstBounds = first.CapsuleBounds.Translate(new Vector(firstOrigin.X, firstOrigin.Y));
        Rect secondBounds = second.CapsuleBounds.Translate(new Vector(secondOrigin.X, secondOrigin.Y));
        RefLabelHitInfo firstHit = new(firstBounds, first.Shape, first.HitPointWidth, first.GitRef, null);
        RefLabelHitInfo secondHit = new(secondBounds, second.Shape, second.HitPointWidth, second.GitRef, null);
        for (int y = (int)Math.Min(firstBounds.Top, secondBounds.Top) - 1; y <= Math.Max(firstBounds.Bottom, secondBounds.Bottom); y++)
        {
            for (int x = (int)firstBounds.Left - 1; x <= secondBounds.Right; x++)
            {
                Point point = new(x, y);
                RevisionGridRefRenderer.RefLabelControl? expected = firstHit.Contains(point)
                    ? first
                    : secondHit.Contains(point) ? second : null;
                fixture.Provider.HitTest(0, point).Should().BeSameAs(expected);
            }
        }

        fixture.Provider.HitTest(-1, firstBounds.Center).Should().BeNull();
        fixture.Provider.HitTest(1, firstBounds.Center).Should().BeNull();
        fixture.Provider.Clear();
        fixture.Provider.HitTest(0, firstBounds.Center).Should().BeNull();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Hover_and_context_should_route_overlapping_visual_bounds_to_the_first_source_label(bool remoteFirst)
    {
        using GridFixture fixture = new(nestled: true, remoteFirst);
        RevisionGridRefRenderer.RefLabelControl first = fixture.Labels[0];
        RevisionGridRefRenderer.RefLabelControl second = fixture.Labels[1];
        Point point = FindFirstOwnedOverlap(first, second);
        Point windowPoint = first.TranslatePoint(point, fixture.Window)!.Value;
        Point gridPoint = first.TranslatePoint(point, fixture.Grid)!.Value;
        fixture.Provider.HitTest(0, gridPoint).Should().BeSameAs(first);
        second.CapsuleBounds.Contains(first.TranslatePoint(point, second)!.Value).Should().BeTrue(
            "the pointer must be in both labels' bounding rectangles, not a non-overlapping label centre");
        fixture.Settle("overlap-before-pointer", windowPoint);
        Visual? rawHit = fixture.Window.InputHitTest(windowPoint) as Visual;
        rawHit.Should().NotBeNull();
        rawHit!.GetSelfAndVisualAncestors().OfType<Control>().Should().Contain(fixture.Cell,
            "the native cell owns pointer routing even when a shape's overlapping rectangle contains unpainted pixels");
        ToolTip.SetTip(second, "incidental child tooltip");

        fixture.MovePointer(windowPoint);
        first = fixture.Labels[0];
        second = fixture.Labels[1];

        first.IsHighlighted.Should().BeTrue();
        second.IsHighlighted.Should().BeFalse();
        (ToolTip.GetTip(fixture.Cell)?.ToString()).Should().StartWith("[" + first.GitRef!.Name + "]");
        (ToolTip.GetTip(fixture.Cell)?.ToString()).Should().NotContain("incidental child tooltip");
        fixture.OpenContextMenu(windowPoint);
        first = fixture.Labels[0];
        second = fixture.Labels[1];

        fixture.Menu.IsOpen.Should().BeTrue();
        fixture.OtherActions.IsVisible.Should().BeTrue();
        first.IsHighlighted.Should().BeTrue();
        second.IsHighlighted.Should().BeFalse();
        MenuItem checkout = fixture.Grid.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")!;
        checkout.Items.OfType<MenuItem>().Should().ContainSingle()
            .Which.Header.Should().Be(first.GitRef!.Name);
        checkout.Items.OfType<MenuItem>().Single().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        if (remoteFirst)
        {
            fixture.Commands.Received(1).StartCheckoutRemoteBranch(null, first.GitRef!.Name);
        }
        else
        {
            fixture.Commands.Received(1).StartCheckoutBranch(null, first.GitRef!.Name);
        }
    }

    [AvaloniaTest]
    public void Hover_should_cross_the_nestled_diagonal_and_restore_the_row_tooltip_outside_capsules()
    {
        using GridFixture fixture = new(nestled: true);
        RevisionGridRefRenderer.RefLabelControl first = fixture.Labels[0];
        RevisionGridRefRenderer.RefLabelControl second = fixture.Labels[1];
        fixture.MovePointer(first.TranslatePoint(FindFirstOwnedOverlap(first, second), fixture.Window)!.Value);
        first = fixture.Labels[0];
        second = fixture.Labels[1];
        first.IsHighlighted.Should().BeTrue();

        Point secondPoint = new(second.CapsuleBounds.Right - 2, second.CapsuleBounds.Top + ((int)second.CapsuleBounds.Height / 2));
        fixture.MovePointer(second.TranslatePoint(secondPoint, fixture.Window)!.Value);
        first = fixture.Labels[0];
        second = fixture.Labels[1];
        first.IsHighlighted.Should().BeFalse();
        second.IsHighlighted.Should().BeTrue();
        (ToolTip.GetTip(fixture.Cell)?.ToString()).Should().StartWith("[origin/main]");

        Point outside = new(second.CapsuleBounds.Right + 1, secondPoint.Y);
        second.Contains(outside).Should().BeFalse();
        fixture.MovePointer(second.TranslatePoint(outside, fixture.Window)!.Value);
        first = fixture.Labels[0];
        second = fixture.Labels[1];
        first.IsHighlighted.Should().BeFalse();
        second.IsHighlighted.Should().BeFalse();
        fixture.Cell.Cursor.Should().BeNull();
        (ToolTip.GetTip(fixture.Cell)?.ToString()).Should().Contain(fixture.Revision.Subject);
    }

    [AvaloniaTest]
    public void Rect_painted_corner_should_keep_ref_hover_and_context_ownership_but_not_the_outgoing_margin()
    {
        using GridFixture fixture = new(nestled: false);
        RevisionGridRefRenderer.RefLabelControl label = fixture.Labels.Single();
        label.Shape.Should().Be(RefLabelShape.Rect);
        Point point = label.TranslatePoint(label.CapsuleBounds.TopLeft, fixture.Window)!.Value;
        fixture.MovePointer(point);
        label = fixture.Labels.Single();
        label.IsHighlighted.Should().BeTrue();
        (ToolTip.GetTip(fixture.Cell)?.ToString()).Should().StartWith("[main]");
        fixture.OpenContextMenu(point);
        fixture.OtherActions.IsVisible.Should().BeTrue();
        fixture.Menu.Close();
        fixture.Settle("rect-context-closed");
        label = fixture.Labels.Single();

        Point margin = new(label.CapsuleBounds.Right + 1, label.CapsuleBounds.Center.Y);
        label.Contains(margin).Should().BeFalse();
        fixture.MovePointer(label.TranslatePoint(margin, fixture.Window)!.Value);
        label = fixture.Labels.Single();
        label.IsHighlighted.Should().BeFalse();
        fixture.OpenContextMenu(label.TranslatePoint(margin, fixture.Window)!.Value);
        fixture.OtherActions.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Stash_hit_should_keep_its_null_ref_highlight_and_focused_context_without_inventing_ref_tooltips()
    {
        using GridFixture fixture = new(nestled: false, stash: true);
        RevisionGridRefRenderer.RefLabelControl label = fixture.Labels.Single();
        label.GitRef.Should().BeNull();
        label.Icon.Should().Be(RefLabelIcon.Stash);
        Point point = label.CapsuleBounds.TopLeft;
        fixture.Provider.HitTest(0, label.TranslatePoint(point, fixture.Grid)!.Value).Should().BeSameAs(label);
        Point windowPoint = label.TranslatePoint(point, fixture.Window)!.Value;
        fixture.MovePointer(windowPoint);
        label = fixture.Labels.Single();
        label.IsHighlighted.Should().BeTrue();
        fixture.Cell.Cursor.Should().NotBeNull();
        fixture.OpenContextMenu(windowPoint);
        label = fixture.Labels.Single();
        fixture.Menu.IsOpen.Should().BeTrue();
        fixture.OtherActions.IsVisible.Should().BeTrue();
        label.IsHighlighted.Should().BeTrue();
    }

    private static IEnumerable<TestCaseData> ShapeAllocations()
    {
        // These row maps preserve native integer division, including truncated slants.
        (int Height, int Width, int[] Slants)[] maps =
        [
            (17, 8, [8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8]),
            (18, 9, [9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8]),
            (17, 3, [3, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 2, 2, 3]),
            (18, 3, [3, 2, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 2, 2, 2]),
        ];
        foreach (RefLabelShape shape in Enum.GetValues<RefLabelShape>())
        {
            foreach ((int height, int width, int[] slants) in maps)
            {
                yield return new TestCaseData(shape.ToString(), height, width, slants);
            }
        }
    }

    private static IEnumerable<TestCaseData> Shapes()
        => Enum.GetValues<RefLabelShape>().Select(shape => new TestCaseData(shape.ToString()));

    private static Point FindFirstOwnedOverlap(
        RevisionGridRefRenderer.RefLabelControl first,
        RevisionGridRefRenderer.RefLabelControl second)
    {
        for (int y = (int)first.CapsuleBounds.Top; y < first.CapsuleBounds.Bottom; y++)
        {
            for (int x = (int)first.CapsuleBounds.Right - 1; x >= first.CapsuleBounds.Left; x--)
            {
                Point point = new(x, y);
                Point secondPoint = first.TranslatePoint(point, second)!.Value;
                if (first.Contains(point) && second.CapsuleBounds.Contains(secondPoint))
                {
                    return point;
                }
            }
        }

        throw new InvalidOperationException("The nestled pair has no first-owned pixel inside the second visual rectangle.");
    }

    private static IGitRef CreateRef(IGitModule module, ObjectId objectId, string name, bool isRemote)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Module.Returns(module);
        gitRef.ObjectId.Returns(objectId);
        gitRef.Guid.Returns(objectId.ToString());
        gitRef.Name.Returns(name);
        gitRef.LocalName.Returns(isRemote ? name[(name.IndexOf('/') + 1)..] : name);
        gitRef.CompleteName.Returns((isRemote ? "refs/remotes/" : "refs/heads/") + name);
        gitRef.IsHead.Returns(!isRemote);
        gitRef.IsRemote.Returns(isRemote);
        gitRef.Remote.Returns(isRemote ? "origin" : string.Empty);
        gitRef.TrackingRemote.Returns("origin");
        return gitRef;
    }

    private sealed class SettingsScope : IDisposable
    {
        private readonly Font _font = AppSettings.Font;
        private readonly bool _showRemotes = AppSettings.ShowRemoteBranches;
        private readonly bool _showTags = AppSettings.ShowTags;
        private readonly bool _tooltips = AppSettings.ShowRevisionGridTooltips.Value;
        private readonly bool _advanced = AppSettings.AlwaysShowAdvOpt;

        public SettingsScope()
        {
            AppSettings.Font = new Font("Segoe UI", 9);
            AppSettings.ShowRemoteBranches = true;
            AppSettings.ShowTags = true;
            AppSettings.ShowRevisionGridTooltips.Value = true;
            AppSettings.AlwaysShowAdvOpt = false;
            AvaloniaFontSettings.ApplyAppSettings();
        }

        public void Dispose()
        {
            AppSettings.Font = _font;
            AppSettings.ShowRemoteBranches = _showRemotes;
            AppSettings.ShowTags = _showTags;
            AppSettings.ShowRevisionGridTooltips.Value = _tooltips;
            AppSettings.AlwaysShowAdvOpt = _advanced;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }

    private sealed class GridFixture : IDisposable
    {
        private readonly SettingsScope _settings = new();
        private int _evidenceSequence;

        public GridFixture(bool nestled, bool remoteFirst = false, bool stash = false, string localName = "main",
            bool additionalTag = false)
        {
            IGitModule module = Substitute.For<IGitModule>();
            module.GitVersion.Returns(Substitute.For<IGitVersion>());
            module.GetSelectedBranch().Returns("current");
            Commands = Substitute.For<IGitUICommands>();
            Commands.Module.Returns(module);
            Commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            Commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(Commands);
            Grid = new RevisionGridControl { UICommandsSource = source };
            Provider = Grid.ColumnProviders.OfType<MessageColumnProvider>().Single();
            Revision = new GitRevision(ObjectId.Parse("abcdef1234567890abcdef1234567890abcdef12"))
            {
                Subject = "Commit message outside ref capsules",
                Author = "Author",
            };
            Revision.IsArtificial.Should().BeFalse();
            if (stash)
            {
                Revision.ReflogSelector = "refs/stash@{3}";
            }
            else
            {
                IGitRef local = CreateRef(module, Revision.ObjectId, localName, isRemote: false);
                IGitRef remote = CreateRef(module, Revision.ObjectId, "origin/" + localName, isRemote: true);
                if (remoteFirst)
                {
                    Revision.Refs = [remote];
                    IAheadBehindDataProvider aheadBehind = Substitute.For<IAheadBehindDataProvider>();
                    aheadBehind.GetData(Arg.Any<string>()).Returns(new Dictionary<string, AheadBehindData>
                    {
                        ["main"] = new("main", "refs/remotes/origin/main", AheadCount: "1", BehindCount: "2"),
                    });
                    Grid.SetAheadBehindDataProvider(aheadBehind);
                }
                else
                {
                    local.IsTrackingRemote(remote).Returns(nestled);
                    Revision.Refs = nestled ? [local, remote] : [local];
                }
            }

            if (additionalTag)
            {
                const string tagName = "release-label-after-the-clipped-pair";
                IGitRef tag = CreateRef(module, Revision.ObjectId, tagName, isRemote: false);
                tag.IsHead.Returns(false);
                tag.IsTag.Returns(true);
                tag.CompleteName.Returns("refs/tags/" + tagName);
                Revision.Refs = [.. Revision.Refs, tag];
            }

            Grid.GetTestAccessor().SetRevisions([Revision]);
            Window = new Window { Width = 1000, Height = 180, Content = Grid };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Grid.GetTestAccessor().Revisions.SelectedItem = Revision;
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Menu = Grid.FindControl<ContextMenu>("mainContextMenu")!;
            OtherActions = Grid.FindControl<MenuItem>("tsmiOtherActions")!;
            Settle("initial");
        }

        public IGitUICommands Commands { get; }

        public RevisionGridControl Grid { get; }

        public MessageColumnProvider Provider { get; }

        public GitRevision Revision { get; }

        public Window Window { get; }

        public Control Cell => Grid.GetVisualDescendants().OfType<Control>()
            .Single(control => control.Classes.Contains("revision-message-cell"));

        public RevisionGridRefRenderer.RefLabelControl[] Labels
            => [.. Cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()];

        public ContextMenu Menu { get; }

        public MenuItem OtherActions { get; }

        public void MovePointer(Point point)
        {
            Window.MouseMove(point);
            Settle("pointer", point);
        }

        public void OpenContextMenu(Point point)
        {
            Window.MouseDown(point, MouseButton.Right);
            Window.MouseUp(point, MouseButton.Right);
            Settle("context-menu", point);
        }

        public void Settle(string stage, Point? pointer = null)
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();

            // Layout and render-scene hit testing advance separately. Rendering also
            // flushes queued row repaints, which replace the retained ref controls.
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The actual grid fixture did not render a headless frame.");
            string? evidenceDirectory = Environment.GetEnvironmentVariable("GITEXTENSIONS_REF_HIT_EVIDENCE");
            if (string.IsNullOrEmpty(evidenceDirectory))
            {
                return;
            }

            Directory.CreateDirectory(evidenceDirectory);
            string stem = TestContext.CurrentContext.Test.ID + "-" + (++_evidenceSequence) + "-" + stage;
            string imagePath = Path.Combine(evidenceDirectory, stem + ".png");
            frame.Save(imagePath, PngBitmapEncoderOptions.Default);
            object geometry = new
            {
                test = TestContext.CurrentContext.Test.Name,
                stage,
                pointer,
                rawHit = pointer is { } point ? Window.InputHitTest(point)?.GetType().FullName : null,
                rawHitLabel = pointer is { } labelPoint && Window.InputHitTest(labelPoint) is Visual hit
                    ? hit.GetSelfAndVisualAncestors().OfType<RevisionGridRefRenderer.RefLabelControl>()
                        .FirstOrDefault()?.Label
                    : null,
                cellBounds = Cell.Bounds.ToString(),
                parentBounds = Labels.FirstOrDefault()?.GetVisualParent()?.Bounds.ToString(),
                labels = Labels.Select(label => new
                {
                    label.Label,
                    shape = label.Shape.ToString(),
                    bounds = label.Bounds.ToString(),
                    capsule = label.CapsuleBounds.ToString(),
                    label.HitPointWidth,
                    label.IsHighlighted,
                    windowOrigin = label.TranslatePoint(default, Window)?.ToString(),
                }),
            };
            File.WriteAllText(Path.Combine(evidenceDirectory, stem + ".json"),
                JsonSerializer.Serialize(geometry, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddTestAttachment(imagePath);
        }

        public void Dispose()
        {
            Menu.Close();
            Window.Close();
            Grid.CancelBackgroundTasks();
            _settings.Dispose();
        }
    }
}

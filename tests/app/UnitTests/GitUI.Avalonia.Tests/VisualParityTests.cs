using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.LeftPanel;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class VisualParityTests
{
    [AvaloniaTest]
    public void Selection_palette_should_use_Git_Extensions_blue_in_both_theme_variants()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");

        application.TryGetResource(
            "GitExtensionsSelectionBackgroundBrush",
            ThemeVariant.Light,
            out object? lightSelection).Should().BeTrue();
        application.TryGetResource(
            "GitExtensionsSelectionBackgroundBrush",
            ThemeVariant.Dark,
            out object? darkSelection).Should().BeTrue();

        lightSelection.Should().BeOfType<SolidColorBrush>()
            .Which.Color.Should().Be(Color.Parse("#0078D4"));
        darkSelection.Should().BeOfType<SolidColorBrush>()
            .Which.Color.Should().Be(Color.Parse("#0067C0"));
    }

    [AvaloniaTest]
    public void List_and_tree_selection_should_use_shared_dense_metrics_in_both_theme_variants()
    {
        AssertListAndTreeStyles(ThemeVariant.Light, Color.Parse("#0078D4"));
        AssertListAndTreeStyles(ThemeVariant.Dark, Color.Parse("#0067C0"));
    }

    [AvaloniaTest]
    public void FormBrowse_should_keep_all_main_sections_usable_at_its_minimum_size()
    {
        FormBrowse form = new()
        {
            Width = 900,
            Height = 560,
        };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            form.MinWidth.Should().Be(900);
            form.MinHeight.Should().Be(560);
            form.repoObjectsTree.Bounds.Width.Should().BeGreaterThan(0);
            form.RevisionGrid.Bounds.Width.Should().BeGreaterThan(0);
            form.RevisionGrid.Bounds.Height.Should().BeGreaterThan(0);
            form.fileStatusList.Bounds.Height.Should().BeGreaterThan(0);
            form.fileViewer.Bounds.Width.Should().BeGreaterThan(0);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Ref_labels_should_use_contrast_safe_theme_colors_and_a_neutral_capsule()
    {
        AssertRefLabelRendering(
            ThemeVariant.Light,
            CreateRef("main", isHead: true),
            Color.Parse("#008000"),
            Color.Parse("#FFFFFF"));
        AssertRefLabelRendering(
            ThemeVariant.Dark,
            CreateRef("main", isHead: true),
            Color.Parse("#7FE28A"),
            Color.Parse("#202020"));
        AssertRefLabelRendering(
            ThemeVariant.Light,
            CreateRef("origin/main", isRemote: true),
            Color.Parse("#8B0009"),
            Color.Parse("#FFFFFF"));
        AssertRefLabelRendering(
            ThemeVariant.Dark,
            CreateRef("origin/main", isRemote: true),
            Color.Parse("#FD9797"),
            Color.Parse("#202020"));
        AssertRefLabelRendering(
            ThemeVariant.Light,
            CreateRef("v1.0", isTag: true),
            Color.Parse("#00008B"),
            Color.Parse("#FFFFFF"));
        AssertRefLabelRendering(
            ThemeVariant.Dark,
            CreateRef("v1.0", isTag: true),
            Color.Parse("#40BAF7"),
            Color.Parse("#202020"));
    }

    [AvaloniaTest]
    public void Ref_labels_should_render_the_checked_out_branch_and_nest_its_tracking_remote()
    {
        IGitRef local = CreateRef(
            "main",
            isHead: true,
            isSelected: true,
            localName: "main",
            mergeWith: "main",
            trackingRemote: "origin");
        IGitRef remote = CreateRef(
            "origin/main",
            isRemote: true,
            isSelectedHeadMergeSource: true,
            localName: "main",
            remote: "origin");

        RevisionGridRefRenderer.NestledRefLabelPanel pair =
            RevisionGridRefRenderer.CreateLabels([remote, local])
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<RevisionGridRefRenderer.NestledRefLabelPanel>()
                .Subject;
        RevisionGridRefRenderer.RefLabelControl[] labels =
        [
            .. pair.Children.OfType<RevisionGridRefRenderer.RefLabelControl>(),
        ];
        RevisionGridRefRenderer.RefLabelControl standaloneTrackingRemote =
            RevisionGridRefRenderer.CreateLabels(
                [CreateRef("origin/next", isRemote: true, isSelectedHeadMergeSource: true)])
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<RevisionGridRefRenderer.RefLabelControl>()
                .Subject;

        Window window = new()
        {
            Width = 320,
            Height = 80,
            RequestedThemeVariant = ThemeVariant.Light,
            Content = pair,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            labels.Should().HaveCount(2);
            labels[0].Label.Should().Be("main");
            labels[0].Shape.Should().Be(RefLabelShape.PointRight);
            labels[0].Icon.Should().Be(RefLabelIcon.Head);
            labels[0].FontWeight.Should().Be(FontWeight.Bold);
            labels[1].Label.Should().Be("origin");
            labels[1].Shape.Should().Be(RefLabelShape.NotchLeft);
            labels[1].Icon.Should().Be(RefLabelIcon.None);
            standaloneTrackingRemote.Icon.Should().Be(RefLabelIcon.HeadMergeSource);
            labels[1].Bounds.X.Should().BeLessThan(labels[0].Bounds.Right);
            pair.DesiredSize.Width.Should().BeLessThan(labels.Sum(label => label.DesiredSize.Width));
            window.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void RepoObjectsTree_should_render_parent_child_connectors()
    {
        RepoObjectsTree control = new();
        control.SetRefs(
        [
            CreateRef("first", isHead: true),
            CreateRef("second", isHead: true),
        ]);

        Window window = new()
        {
            Width = 260,
            Height = 220,
            RequestedThemeVariant = ThemeVariant.Light,
            Content = control,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TreeView tree = control.FindControl<TreeView>("treeMain")
                ?? throw new InvalidOperationException("The repository tree was not created.");
            TreeViewItem branches = tree.Items[0] as TreeViewItem
                ?? throw new InvalidOperationException("The branches group was not created.");
            TreeViewItem[] children = branches.Items
                .Cast<TreeViewItem>()
                .ToArray();
            RepoObjectsTree.TreeConnectorControl[] connectors = children
                .Select(child => child.Header)
                .OfType<Grid>()
                .SelectMany(header => header.Children)
                .OfType<RepoObjectsTree.TreeConnectorControl>()
                .ToArray();

            children.Should().HaveCount(2);
            connectors.Should().HaveCount(2);
            connectors[0].IsLastSibling.Should().BeFalse();
            connectors[1].IsLastSibling.Should().BeTrue();
            children.Should().OnlyContain(child => child.Bounds.Width > 0 && child.Bounds.Height > 0);
            connectors.Should().OnlyContain(connector => connector.Bounds.Width > 0 && connector.Bounds.Height > 0);
            connectors
                .SelectMany(connector => connector.Children)
                .OfType<Border>()
                .Should()
                .OnlyContain(line => GetColor(line.Background) == Color.Parse("#8A8A8A"));
            window.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void RepoObjectsTree_scrollbars_should_not_cover_the_last_expanded_item()
    {
        RepoObjectsTree control = new();
        control.SetRefs(
        [
            .. Enumerable.Range(0, 36)
                .Select(index => CreateRef(
                    $"feature/a-very-long-branch-name-{index:D2}",
                    isHead: true)),
            CreateRef(
                "origin/a-very-long-remote-branch-name",
                isRemote: true),
            CreateRef(
                "a-very-long-release-tag-name",
                isTag: true),
        ]);

        Window window = new()
        {
            Width = 190,
            Height = 180,
            Content = control,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            TreeView tree = control.FindControl<TreeView>("treeMain")
                ?? throw new InvalidOperationException("The repository tree was not created.");
            TreeViewItem[] groups = tree.Items.Cast<TreeViewItem>().ToArray();
            groups.Should().HaveCount(3);
            foreach (TreeViewItem group in groups)
            {
                group.IsExpanded = true;
            }

            Dispatcher.UIThread.RunJobs();

            ScrollViewer scrollViewer = tree.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .Single();
            ScrollBar[] scrollBars = scrollViewer.GetVisualDescendants()
                .OfType<ScrollBar>()
                .Where(scrollBar => scrollBar.IsVisible)
                .ToArray();
            scrollViewer.ScrollBarMaximum.X.Should().BeGreaterThan(0);
            scrollViewer.ScrollBarMaximum.Y.Should().BeGreaterThan(0);
            scrollBars.Should().Contain(scrollBar => scrollBar.Orientation == Orientation.Horizontal);
            scrollBars.Should().Contain(scrollBar => scrollBar.Orientation == Orientation.Vertical);
            ScrollViewer.GetAllowAutoHide(tree).Should().BeFalse();

            TreeViewItem lastItem = groups[^1].Items.Cast<TreeViewItem>().Last();
            lastItem.BringIntoView();
            Dispatcher.UIThread.RunJobs();

            Point lastItemBottom = lastItem.TranslatePoint(
                    new Point(0, lastItem.Bounds.Height),
                    scrollViewer)
                ?? throw new InvalidOperationException("The final tree item was not in the scroll viewer.");
            lastItemBottom.Y.Should().BeLessThanOrEqualTo(scrollViewer.Viewport.Height + 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Revision_grid_should_use_the_visible_lane_width_and_WinForms_left_margins()
    {
        RevisionGridControl control = new();
        ListBox revisions = control.FindControl<ListBox>("lstRevisions")
            ?? throw new InvalidOperationException("The revision list was not created.");
        revisions.ItemsSource = new[] { new GitRevision(ObjectId.Random()) };

        Window window = new()
        {
            Width = 700,
            Height = 180,
            Content = control,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Control graphCell = control.GetVisualDescendants()
                .OfType<Control>()
                .Single(item => item.Classes.Contains("revision-graph-cell"));
            Grid revisionRow = control.GetVisualDescendants()
                .OfType<Grid>()
                .Single(item => item.Classes.Contains("revision-row"));
            StackPanel messageCell = control.GetVisualDescendants()
                .OfType<StackPanel>()
                .Single(item => item.Classes.Contains("revision-message-cell"));

            graphCell.Margin.Left.Should().Be(6);
            revisionRow.ColumnDefinitions[0].Width.Value.Should().Be(22);
            messageCell.Margin.Left.Should().Be(6);
            RevisionGridControl.CalculateGraphColumnWidth(visibleLaneCount: 3).Should().Be(54);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertListAndTreeStyles(ThemeVariant themeVariant, Color selectionColor)
    {
        ListBox list = new()
        {
            ItemsSource = new[] { "revision" },
            SelectedIndex = 0,
        };
        TreeViewItem treeItem = new()
        {
            Header = "main",
            IsSelected = true,
        };
        Grid content = new()
        {
            RowDefinitions = new RowDefinitions("*,*"),
            Children =
            {
                list,
                treeItem,
            },
        };
        Grid.SetRow(treeItem, 1);

        Window window = new()
        {
            Width = 400,
            Height = 240,
            RequestedThemeVariant = themeVariant,
            Content = content,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            ListBoxItem listItem = list.ContainerFromIndex(0) as ListBoxItem
                ?? throw new InvalidOperationException("The list item was not realized.");
            ContentPresenter listPresenter = listItem.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_ContentPresenter");
            Border treeLayoutRoot = treeItem.GetVisualDescendants()
                .OfType<Border>()
                .Single(border => border.Name == "PART_LayoutRoot");

            listItem.MinHeight.Should().Be(24);
            listItem.Padding.Should().Be(new Thickness(6, 2));
            GetColor(listPresenter.Background).Should().Be(selectionColor);
            treeItem.MinHeight.Should().Be(24);
            GetColor(treeLayoutRoot.Background).Should().Be(selectionColor);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRefLabelRendering(
        ThemeVariant themeVariant,
        IGitRef gitRef,
        Color expectedForeground,
        Color expectedBackground)
    {
        RevisionGridRefRenderer.RefLabelControl label =
            RevisionGridRefRenderer.CreateLabels([gitRef])
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<RevisionGridRefRenderer.RefLabelControl>()
                .Subject;
        Window window = new()
        {
            Width = 320,
            Height = 120,
            RequestedThemeVariant = themeVariant,
            Content = label,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            GetColor(label.RefBrush).Should().Be(expectedForeground);
            GetColor(label.CapsuleBackgroundBrush).Should().Be(expectedBackground);
            label.Bounds.Height.Should().Be(24);
            label.FontSize.Should().BeGreaterThan(11);
            label.Shape.Should().Be(gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect);
            window.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    private static IGitRef CreateRef(
        string name,
        bool isHead = false,
        bool isRemote = false,
        bool isTag = false,
        bool isSelected = false,
        bool isSelectedHeadMergeSource = false,
        string? localName = null,
        string mergeWith = "",
        string remote = "",
        string trackingRemote = "")
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.IsHead.Returns(isHead);
        gitRef.IsRemote.Returns(isRemote);
        gitRef.IsTag.Returns(isTag);
        gitRef.IsSelected.Returns(isSelected);
        gitRef.IsSelectedHeadMergeSource.Returns(isSelectedHeadMergeSource);
        gitRef.LocalName.Returns(localName ?? name);
        gitRef.MergeWith.Returns(mergeWith);
        gitRef.Remote.Returns(remote);
        gitRef.TrackingRemote.Returns(trackingRemote);
        return gitRef;
    }

    private static Color GetColor(IBrush? brush)
        => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;
}

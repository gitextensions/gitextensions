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
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Theming;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class VisualParityTests
{
    [AvaloniaTest]
    public void Application_fonts_should_follow_point_based_Git_Extensions_settings()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");
        WinFormsShims.Font originalUiFont = AppSettings.Font;
        WinFormsShims.Font originalCommitFont = AppSettings.CommitFont;
        WinFormsShims.Font originalFixedWidthFont = AppSettings.FixedWidthFont;
        WinFormsShims.Font originalMonospaceFont = AppSettings.MonospaceFont;

        try
        {
            AppSettings.Font = new WinFormsShims.Font(
                "Parity UI",
                10,
                WinFormsShims.FontStyle.Bold | WinFormsShims.FontStyle.Italic);
            AppSettings.CommitFont = new WinFormsShims.Font("Parity Commit", 9);
            AppSettings.FixedWidthFont = new WinFormsShims.Font("Parity Diff", 11);
            AppSettings.MonospaceFont = new WinFormsShims.Font("Parity Monospace", 8);

            AvaloniaFontSettings.ApplyAppSettings();

            GetResource<FontFamily>(application, "GitExtensionsUiFontFamily").Name.Should().Be("Parity UI");
            GetResource<double>(application, "GitExtensionsUiFontSize").Should().BeApproximately(40d / 3d, 0.001);
            GetResource<FontStyle>(application, "GitExtensionsUiFontStyle").Should().Be(FontStyle.Italic);
            GetResource<FontWeight>(application, "GitExtensionsUiFontWeight").Should().Be(FontWeight.Bold);
            GetResource<FontFamily>(application, "GitExtensionsCommitFontFamily").Name.Should().Be("Parity Commit");
            GetResource<FontFamily>(application, "GitExtensionsFixedWidthFontFamily").Name.Should().Be("Parity Diff");
            GetResource<double>(application, "GitExtensionsFixedWidthFontSize").Should().BeApproximately(44d / 3d, 0.001);
            GetResource<FontFamily>(application, "GitExtensionsMonospaceFontFamily").Name.Should().Be("Parity Monospace");
        }
        finally
        {
            AppSettings.Font = originalUiFont;
            AppSettings.CommitFont = originalCommitFont;
            AppSettings.FixedWidthFont = originalFixedWidthFont;
            AppSettings.MonospaceFont = originalMonospaceFont;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }

    [AvaloniaTest]
    public void Dialog_styles_should_use_WinForms_metrics_and_square_group_box_chrome()
    {
        Grid mainPanel = new()
        {
            Classes = { "gitextensions-dialog-main" },
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        HeaderedContentControl groupBox = new()
        {
            Classes = { "gitextensions-group-box" },
            Header = "Repository type",
            Content = new TextBlock { Text = "Personal repository" },
        };
        Button action = new()
        {
            Classes = { "gitextensions-dialog-action" },
            Content = "Clone",
        };
        mainPanel.Children.Add(groupBox);
        mainPanel.Children.Add(action);
        Grid.SetRow(action, 1);

        Window window = new()
        {
            Width = 400,
            Height = 220,
            RequestedThemeVariant = ThemeVariant.Light,
            Content = mainPanel,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Border frame = groupBox.GetVisualDescendants()
                .OfType<Border>()
                .Single(border => border.Name == "PART_GroupBoxFrame");
            ContentPresenter header = groupBox.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_HeaderPresenter");

            mainPanel.Margin.Should().Be(new Thickness(12));
            action.MinWidth.Should().Be(75);
            action.MinHeight.Should().Be(25);
            groupBox.Padding.Should().Be(new Thickness(8));
            frame.BorderThickness.Should().Be(new Thickness(1, 0, 1, 1));
            frame.CornerRadius.Should().Be(new CornerRadius(0));
            header.Content.Should().Be("Repository type");
            GetColor(frame.BorderBrush).Should().Be(Color.Parse("#D2D2D2"));
        }
        finally
        {
            window.Close();
        }
    }

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
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
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
                Point repoTreePosition = form.repoObjectsTree.TranslatePoint(new Point(), form)
                    ?? throw new InvalidOperationException("The repository tree position was not available.");
                repoTreePosition.X.Should().BeApproximately(7, 0.1);
                form.repoObjectsTree.Bounds.Width.Should().BeApproximately(258, 0.1);
                form.repoObjectsTree.Bounds.Width.Should().BeGreaterThan(0);
                form.RevisionGrid.Bounds.Width.Should().BeGreaterThan(0);
                form.RevisionGrid.Bounds.Height.Should().BeGreaterThan(0);
                form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.CommitInfoTabPage);
                form.CommitInfoTabControl.Bounds.Height.Should().BeGreaterThan(0);
                form.CommitInfoTabPage.Bounds.Height.Should().Be(23);
                form.DiffTabPage.Bounds.Height.Should().Be(23);
                form.TreeTabPage.Bounds.Height.Should().Be(23);
                form.commitInfoBelowHost.Bounds.Height.Should().BeGreaterThan(0);
                Point commitHostPosition = form.commitInfoBelowHost.TranslatePoint(new Point(), form.CommitInfoTabControl)
                    ?? throw new InvalidOperationException("The commit-info host position was not available.");
                commitHostPosition.X.Should().BeLessThanOrEqualTo(1);
                form.RevisionInfo.Bounds.Height.Should().BeGreaterThan(0);

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                form.fileStatusList.Bounds.Height.Should().BeGreaterThan(0);
                form.fileViewer.Bounds.Width.Should().BeGreaterThan(0);

                form.CommitInfoTabControl.SelectedItem = form.TreeTabPage;
                Dispatcher.UIThread.RunJobs();
                form.fileTree.Bounds.Height.Should().BeGreaterThan(0);

                GridSplitter[] splitters =
                [
                    .. form.GetVisualDescendants()
                        .OfType<GridSplitter>()
                        .Where(splitter => splitter.IsVisible),
                ];
                splitters.Where(splitter => splitter.ResizeDirection == GridResizeDirection.Columns)
                    .Should().HaveCount(2)
                    .And.OnlyContain(splitter => splitter.Bounds.Width == 6);
                splitters.Where(splitter => splitter.ResizeDirection == GridResizeDirection.Rows)
                    .Should().ContainSingle()
                    .Which.Bounds.Height.Should().Be(6);
                splitters.Should().OnlyContain(splitter => GetColor(splitter.Background) == Colors.Transparent);

                Menu menu = form.FindControl<Menu>("mainMenuStrip")
                    ?? throw new InvalidOperationException("The main menu was not created.");
                menu.Bounds.Height.Should().Be(24);
                menu.Items.Cast<MenuItem>().Should().OnlyContain(item => item.Bounds.Height == 20);
                Rect[] closedMenuItemBounds = menu.Items.Cast<MenuItem>().Select(item => item.Bounds).ToArray();
                form.commandsToolStripMenuItem.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
                menu.Items.Cast<MenuItem>().Select(item => item.Bounds).Should().Equal(closedMenuItemBounds);
                form.commitToolStripMenuItem.Bounds.Height.Should().Be(22);
                ItemsPresenter menuItemsPresenter = form.commitToolStripMenuItem
                    .GetVisualAncestors()
                    .OfType<ItemsPresenter>()
                    .Single(presenter => presenter.Name == "PART_ItemsPresenter");
                menuItemsPresenter.Margin.Should().Be(new Thickness(0));

                Border openMenuBorder = form.commandsToolStripMenuItem
                    .GetVisualDescendants()
                    .OfType<Border>()
                    .Single(border => border.Name == "PART_LayoutRoot");
                openMenuBorder.BorderThickness.Should().Be(new Thickness(1, 1, 1, 0));
                openMenuBorder.CornerRadius.Should().Be(new CornerRadius(0));
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_toolbars_should_stay_compact_and_wrap_only_at_narrow_widths()
    {
        foreach (ThemeVariant theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            FormBrowse form = new()
            {
                Width = 1400,
                Height = 700,
                RequestedThemeVariant = theme,
            };
            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                WrapPanel toolPanel = form.FindControl<WrapPanel>("toolPanel")!;
                StackPanel mainToolbar = form.FindControl<StackPanel>("ToolStripMain")!;
                mainToolbar.Bounds.Height.Should().Be(25);
                toolPanel.Bounds.Height.Should().Be(25);
                mainToolbar.GetVisualDescendants().OfType<Button>()
                    .Should().OnlyContain(button => button.Bounds.Height <= 23);

                ComboBox[] editableInputs = form.GetVisualDescendants()
                    .OfType<ComboBox>()
                    .Where(combo => combo.Classes.Contains("gitextensions-toolbar-input"))
                    .ToArray();
                editableInputs.Should().HaveCount(3);
                foreach (ComboBox input in editableInputs)
                {
                    TextBox editor = input.GetVisualDescendants()
                        .OfType<TextBox>()
                        .Single(textBox => textBox.Name == "PART_EditableTextBox");
                    ScrollViewer textViewport = editor.GetVisualDescendants()
                        .OfType<ScrollViewer>()
                        .Single();
                    TextPresenter textPresenter = editor.GetVisualDescendants()
                        .OfType<TextPresenter>()
                        .Single();

                    input.Bounds.Height.Should().Be(23);
                    input.Bounds.Y.Should().Be(1);
                    textViewport.Bounds.Height.Should().BeGreaterThanOrEqualTo(textPresenter.Bounds.Height);
                    Point presenterPosition = textPresenter.TranslatePoint(default, input)!.Value;
                    double topInset = presenterPosition.Y;
                    double bottomInset = input.Bounds.Height - presenterPosition.Y - textPresenter.Bounds.Height;
                    Math.Abs(topInset - bottomInset).Should().BeLessThanOrEqualTo(1);

                    Grid templateGrid = editor.GetVisualAncestors().OfType<Grid>().First();
                    templateGrid.ColumnDefinitions[1].Width.Should().Be(new GridLength(16));
                    PathIcon dropDownGlyph = input.GetVisualDescendants()
                        .OfType<PathIcon>()
                        .Single(icon => icon.Name == "DropDownGlyph");
                    dropDownGlyph.Bounds.Size.Should().Be(new Size(7, 5));
                }

                SplitButton[] splitButtons = form.GetVisualDescendants()
                    .OfType<SplitButton>()
                    .Where(button => button.Classes.Contains("gitextensions-toolbar-button"))
                    .ToArray();
                splitButtons.Should().HaveCount(6);
                foreach (SplitButton splitButton in splitButtons)
                {
                    Button primaryButton = splitButton.GetVisualDescendants()
                        .OfType<Button>()
                        .Single(button => button.Name == "PART_PrimaryButton");
                    Button secondaryButton = splitButton.GetVisualDescendants()
                        .OfType<Button>()
                        .Single(button => button.Name == "PART_SecondaryButton");
                    PathIcon arrow = secondaryButton.GetVisualDescendants()
                        .OfType<PathIcon>()
                        .Single();

                    splitButton.Bounds.Height.Should().Be(23);
                    primaryButton.Bounds.Height.Should().Be(23);
                    secondaryButton.Bounds.Width.Should().Be(13);
                    secondaryButton.Bounds.Height.Should().Be(23);
                    arrow.Bounds.Size.Should().Be(new Size(7, 5));
                }

                IconSplitButton branchSelect = form.FindControl<IconSplitButton>("branchSelect")!;
                branchSelect.GetVisualDescendants().OfType<Image>().Should().ContainSingle();
                branchSelect.GetVisualDescendants().OfType<TextBlock>()
                    .Should().ContainSingle(text => text.Text == "Branch");
                foreach (IconSplitButton iconOnlyButton in new[]
                {
                    form.FindControl<IconSplitButton>("toolStripButtonPull")!,
                    form.FindControl<FilterToolBar>("ToolStripFilters")!
                        .FindControl<IconSplitButton>("tsbtnAdvancedFilter")!,
                })
                {
                    iconOnlyButton.GetVisualDescendants().OfType<Image>().Should().ContainSingle();
                    iconOnlyButton.GetVisualDescendants().OfType<TextBlock>().Should().BeEmpty();
                }

                form.Width = 900;
                Dispatcher.UIThread.RunJobs();
                toolPanel.Bounds.Height.Should().BeGreaterThan(25);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void Desktop_menus_should_not_reserve_touch_padding_or_empty_rows()
    {
        Button target = new() { Content = "Open" };
        Window window = new()
        {
            Width = 320,
            Height = 160,
            Content = target,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            MenuItem nestedContextItem = new() { Header = "Nested command" };
            MenuItem contextItem = new() { Header = "Context command", Items = { nestedContextItem } };
            ContextMenu contextMenu = new() { Items = { contextItem } };
            contextMenu.Open(target);
            Dispatcher.UIThread.RunJobs();
            AssertSingleItemPopupFits(contextItem);
            contextMenu.GetVisualDescendants()
                .OfType<ItemsPresenter>()
                .Single(presenter => presenter.Name == "PART_ItemsPresenter")
                .Margin.Should().Be(new Thickness(0));
            contextItem.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            AssertSingleItemPopupFits(nestedContextItem);
            contextMenu.Close();

            MenuItem flyoutItem = new() { Header = "Flyout command" };
            MenuFlyout flyout = new() { Items = { flyoutItem } };
            flyout.ShowAt(target);
            Dispatcher.UIThread.RunJobs();
            AssertSingleItemPopupFits(flyoutItem);
            TopLevel flyoutRoot = TopLevel.GetTopLevel(flyoutItem)!;
            MenuFlyoutPresenter flyoutPresenter = flyoutRoot.GetVisualDescendants()
                .Prepend(flyoutRoot)
                .OfType<MenuFlyoutPresenter>()
                .Single();
            flyoutPresenter.MinHeight.Should().Be(0);
            flyout.Hide();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Top_menu_should_keep_its_size_and_join_its_popup()
    {
        MenuItem child = new() { Header = "Child command" };
        MenuItem parent = new() { Header = "Commands", Items = { child } };
        Menu menu = new() { Items = { parent } };
        Window window = new()
        {
            Width = 320,
            Height = 160,
            Content = menu,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Rect closedBounds = parent.Bounds;
            Border layoutRoot = parent.GetVisualDescendants()
                .OfType<Border>()
                .Single(border => border.Name == "PART_LayoutRoot");

            parent.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            parent.Bounds.Should().Be(closedBounds);
            AssertSingleItemPopupFits(child);
            Point layoutPosition = layoutRoot.TranslatePoint(default, parent)!.Value;
            layoutPosition.Y.Should().Be(0);
            layoutRoot.Bounds.Height.Should().Be(parent.Bounds.Height);
        }
        finally
        {
            window.Close();
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
            double localMiddle = labels[0].Bounds.Height / 2;
            double localPointX = labels[0].Bounds.Width - 6;
            labels[0].Contains(new Point(localPointX, localMiddle)).Should().BeTrue();
            labels[0].Contains(new Point(localPointX, 1)).Should().BeFalse();
            double remoteMiddle = labels[1].Bounds.Height / 2;
            labels[1].Contains(new Point(1, remoteMiddle)).Should().BeFalse();
            labels[1].Contains(new Point(labels[1].PointWidth + 1, remoteMiddle)).Should().BeTrue();
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
            TreeConnectorControl[] connectors = tree
                .GetVisualDescendants()
                .OfType<TreeConnectorControl>()
                .ToArray();

            children.Should().HaveCount(2);
            connectors.Should().HaveCount(5);
            TreeConnectorControl[] childConnectors = connectors
                .Where(connector => children.Contains(connector.Item))
                .ToArray();
            childConnectors.Should().HaveCount(2);
            childConnectors[0].IsLastSibling.Should().BeFalse();
            childConnectors[1].IsLastSibling.Should().BeTrue();
            children.Should().OnlyContain(child => child.Bounds.Width > 0 && child.Bounds.Height > 0);
            connectors.Should().OnlyContain(connector =>
                connector.Bounds.Width > 0
                && connector.Bounds.Height >= 17
                && connector.Bounds.Height <= 18);

            ContentPresenter parentHeader = branches.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_HeaderPresenter"
                    && presenter.FindAncestorOfType<TreeViewItem>() == branches);
            ContentPresenter childHeader = children[0].GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_HeaderPresenter"
                    && presenter.FindAncestorOfType<TreeViewItem>() == children[0]);
            Image parentIcon = parentHeader.GetVisualDescendants().OfType<Image>().Single();
            TextBlock parentText = parentHeader.GetVisualDescendants().OfType<TextBlock>().Single();
            Image childIcon = childHeader.GetVisualDescendants().OfType<Image>().Single();
            TextBlock childText = childHeader.GetVisualDescendants().OfType<TextBlock>().Single();
            double parentIconX = parentIcon.TranslatePoint(default, branches)!.Value.X;
            double parentTextX = parentText.TranslatePoint(default, branches)!.Value.X;
            double childIconX = childIcon.TranslatePoint(default, children[0])!.Value.X;
            double childTextX = childText.TranslatePoint(default, children[0])!.Value.X;

            ToggleButton parentChevron = branches.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(toggle => toggle.Name == "PART_ExpandCollapseChevron"
                    && toggle.FindAncestorOfType<TreeViewItem>() == branches);
            Point parentChevronPosition = parentChevron.TranslatePoint(default, branches)!.Value;
            parentChevronPosition.X.Should().Be(4);
            parentChevron.Bounds.Size.Should().Be(new Size(12, 12));
            parentIconX.Should().BeApproximately(20, 0.1);
            (parentIconX - parentChevronPosition.X - parentChevron.Bounds.Width).Should().Be(4);
            childIconX.Should().Be(parentTextX);
            childTextX.Should().Be(parentTextX + 18);
            window.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Test]
    public void RepoObjectsTree_connectors_should_leave_the_chevron_rectangle_clear()
    {
        (Point Start, Point End)[] lines =
        [.. TreeConnectorControl.GetCurrentItemLines(x: 10, top: 0, bottom: 18, middle: 9, hasChevron: true)];

        lines.Should().Equal(
            (new Point(10, 0), new Point(10, 3)),
            (new Point(10, 15), new Point(10, 18)),
            (new Point(16, 9), new Point(20, 9)));
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

    [AvaloniaTest]
    public void Application_theme_should_follow_built_in_Git_Extensions_settings()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");
        ThemeId originalTheme = AppSettings.ThemeId;
        ThemeVariant? originalVariant = application.RequestedThemeVariant;
        try
        {
            AppSettings.ThemeId = ThemeId.DefaultLight;
            AvaloniaThemeSettings.ApplyAppSettings();
            application.RequestedThemeVariant.Should().Be(ThemeVariant.Light);
            ThemeModule.Settings.Theme.Id.Should().Be(ThemeId.DefaultLight);
            WinFormsShims.Application.SystemColorMode.Should().Be(WinFormsShims.SystemColorMode.Classic);
            GetResourceBrushColor(application, "GitExtensionsPanelBackgroundBrush", ThemeVariant.Light).Should().Be(Color.Parse("#FFFFFF"));
            GetResourceBrushColor(application, "GitExtensionsAppColorBranchBrush", ThemeVariant.Light).Should().Be(Color.Parse("#008000"));
            GetResourceBrushColor(application, "GitExtensionsKnownColorWindowTextBrush", ThemeVariant.Light).Should().Be(Colors.Black);
            GetResourceBrushColor(application, "GitExtensionsSelectionBackgroundBrush", ThemeVariant.Light).Should().Be(Color.Parse("#0078D4"));
            AssertPublishedThemeColors(application, ThemeVariant.Light);
            RevisionGraphLaneColor.NonRelativeColor.ToArgb().Should().Be(
                System.Drawing.ColorTranslator.FromHtml("#D3D3D3").ToArgb());

            AppSettings.ThemeId = ThemeId.DefaultDark;
            AvaloniaThemeSettings.ApplyAppSettings();
            application.RequestedThemeVariant.Should().Be(ThemeVariant.Dark);
            ThemeModule.Settings.Theme.Id.Should().Be(ThemeId.DefaultDark);
            WinFormsShims.Application.SystemColorMode.Should().Be(WinFormsShims.SystemColorMode.Dark);
            ThemeModule.Settings.Theme.GetColor(AppColor.PanelBackground).IsEmpty.Should().BeFalse();
            AppColor.PanelBackground.GetThemeColor().Should().Be(
                ThemeModule.Settings.Theme.GetColor(AppColor.PanelBackground));
            GetResourceBrushColor(application, "GitExtensionsPanelBackgroundBrush", ThemeVariant.Dark).Should().Be(Color.Parse("#323232"));
            GetResourceBrushColor(application, "GitExtensionsBranchRefBrush", ThemeVariant.Dark).Should().Be(Color.Parse("#7FE28A"));
            GetResourceBrushColor(application, "GitExtensionsDiffRemovedBrush", ThemeVariant.Dark).Should().Be(Color.Parse("#6E1919"));
            GetResourceBrushColor(application, "GitExtensionsKnownColorWindowTextBrush", ThemeVariant.Dark).Should().Be(Color.Parse("#F0F0F0"));
            GetResourceBrushColor(application, "GitExtensionsSelectionBackgroundBrush", ThemeVariant.Dark).Should().Be(Color.Parse("#0067C0"));
            AssertPublishedThemeColors(application, ThemeVariant.Dark);
            RevisionGraphLaneColor.NonRelativeColor.ToArgb().Should().Be(
                System.Drawing.ColorTranslator.FromHtml("#707070").ToArgb());

            AppSettings.ThemeId = ThemeId.WindowsAppColorModeId;
            AvaloniaThemeSettings.ApplyAppSettings();
            application.RequestedThemeVariant.Should().Be(ThemeVariant.Default);
            ThemeModule.Settings.Theme.Id.Should().Be(ThemeId.ColorModeThemeId);
            WinFormsShims.Application.SystemColorMode.Should().Be(
                application.ActualThemeVariant == ThemeVariant.Dark
                    ? WinFormsShims.SystemColorMode.Dark
                    : WinFormsShims.SystemColorMode.Classic);

            ThemeVariant simulatedPlatformVariant = application.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
            application.RequestedThemeVariant = simulatedPlatformVariant;
            Dispatcher.UIThread.RunJobs();
            ThemeModule.Settings.Theme.SystemColorMode.Should().Be(
                simulatedPlatformVariant == ThemeVariant.Dark
                    ? WinFormsShims.SystemColorMode.Dark
                    : WinFormsShims.SystemColorMode.Classic);
            GetResourceBrushColor(application, "GitExtensionsPanelBackgroundBrush", simulatedPlatformVariant).Should().Be(
                simulatedPlatformVariant == ThemeVariant.Dark ? Color.Parse("#323232") : Colors.White);
        }
        finally
        {
            AppSettings.ThemeId = originalTheme;
            AvaloniaThemeSettings.ApplyAppSettings();
            application.RequestedThemeVariant = originalVariant;
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

    private static void AssertSingleItemPopupFits(MenuItem item)
    {
        Visual[] ancestors = [.. item.GetVisualAncestors()];
        Control? popupSurface = ancestors.OfType<ContextMenu>().FirstOrDefault();
        popupSurface ??= ancestors.OfType<MenuFlyoutPresenter>().FirstOrDefault();
        popupSurface ??= ancestors.OfType<Border>().LastOrDefault();
        if (popupSurface is null)
        {
            throw new InvalidOperationException("The popup chrome was not created.");
        }

        Point itemPosition = item.TranslatePoint(default, popupSurface)
            ?? throw new InvalidOperationException("The menu item position was not available.");
        double topInset = itemPosition.Y;
        double bottomInset = popupSurface.Bounds.Height - itemPosition.Y - item.Bounds.Height;

        item.Bounds.Height.Should().Be(22);
        topInset.Should().BeLessThanOrEqualTo(1);
        bottomInset.Should().BeLessThanOrEqualTo(1);
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

    private static T GetResource<T>(Application application, string key)
    {
        application.TryGetResource(key, theme: null, out object? resource).Should().BeTrue();
        return resource.Should().BeOfType<T>().Subject;
    }

    private static Color GetResourceBrushColor(Application application, string key, ThemeVariant themeVariant)
    {
        application.TryGetResource(key, themeVariant, out object? resource).Should().BeTrue();
        return resource.Should().BeOfType<SolidColorBrush>().Which.Color;
    }

    private static void AssertPublishedThemeColors(Application application, ThemeVariant themeVariant)
    {
        foreach (AppColor name in Enum.GetValues<AppColor>())
        {
            System.Drawing.Color expected = ThemeModule.Settings.Theme.GetColor(name);
            if (expected.IsEmpty)
            {
                expected = ThemeModule.Settings.InvariantTheme.GetColor(name);
            }

            if (expected.IsEmpty)
            {
                application.TryGetResource(
                    AvaloniaThemeResources.AppColorPrefix + name + "Brush",
                    themeVariant,
                    out _).Should().BeFalse();
                continue;
            }

            GetResourceBrushColor(application, AvaloniaThemeResources.AppColorPrefix + name + "Brush", themeVariant)
                .Should().Be(Color.FromArgb(expected.A, expected.R, expected.G, expected.B));
        }

        foreach (System.Drawing.KnownColor name in Enum.GetValues<System.Drawing.KnownColor>())
        {
            if (!System.Drawing.Color.FromKnownColor(name).IsSystemColor)
            {
                continue;
            }

            application.TryGetResource(
                AvaloniaThemeResources.KnownColorPrefix + name + "Brush",
                themeVariant,
                out object? resource).Should().BeTrue();
            resource.Should().BeOfType<SolidColorBrush>();
        }
    }
}

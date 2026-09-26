using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Git.Gpg;
using GitCommands.Submodules;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using GitUI.Shells;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using SkiaSharp;
using SourceControls = GitUI.Compat.WinFormsControls;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormBrowseTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _revisionGraphShowArtificialCommits;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        AppSettings.RevisionGraphShowArtificialCommits = false;

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.RevisionGraphShowArtificialCommits = _revisionGraphShowArtificialCommits;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_tree_should_report_source_toolbar_colors_and_logical_visibility()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        try
        {
            SourceControls.ToolStripContainer toolPanel = form.FindControl<SourceControls.ToolStripContainer>("toolPanel")!;
            toolPanel.Children.Should().HaveCount(5);
            SourceControls.ToolStripPanel topPanel = form.FindControl<SourceControls.ToolStripPanel>("_topPanel")!;
            topPanel.Parent.Should().BeSameAs(toolPanel);
            topPanel.Children.Select(child => child.Name).Should().Equal(
                "toolStripMainHost", "toolStripFiltersHost", "ToolStripScripts");

            Button overflow = form.FindControl<Button>("toolStripMainOverflow")!;
            overflow.IsVisible.Should().BeTrue();
            overflow.Bounds.Width.Should().Be(11);

            foreach ((ThemeVariant theme, string background, string foreground, string sourceForeground, string windowText) in
                     new[]
                     {
                         (ThemeVariant.Light, "#FFF0F0F0", "#FF000000", "#FF000000", "#FF000000"),
                         (ThemeVariant.Dark, "#FF202020", "#FFFFFFFF", "#FFF0F0F0", "#FFF0F0F0"),
                     })
            {
                form.RequestedThemeVariant = theme;
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root;
                CaptureNode[] nodes = [.. Flatten(root)];
                nodes.Single(node => node.FieldName == "txtCommitGpgInfo")
                    .Colors.Background.Should().Be(
                        theme == ThemeVariant.Light ? "#FFF0F0F0" : "#FF5A5A5A",
                        "the source read-only GPG textbox uses the themed read-only input background");
                CaptureNode container = nodes.Single(node => node.FieldName == "toolPanel");
                container.Children.Should().HaveCount(5);
                container.Children.Should().OnlyContain(node => node.FieldName == null);
                container.Children.Select(node => node.Type).Should().Equal(
                    "System.Windows.Forms.ToolStripContentPanel",
                    "System.Windows.Forms.ToolStripPanel",
                    "System.Windows.Forms.ToolStripPanel",
                    "System.Windows.Forms.ToolStripPanel",
                    "System.Windows.Forms.ToolStripPanel");
                container.Children[3].Children.Select(node => node.FieldName).Should().Equal(
                    "ToolStripScripts", "ToolStripFilters", "ToolStripMain");
                foreach (string name in new[]
                         {
                             "toolStripFileExplorer", "userShell",
                         })
                {
                    CaptureNode item = nodes.Single(node => node.FieldName == name);
                    item.Colors.Background.Should().Be(background, $"{name} inherits the source ToolStrip background");
                    item.Colors.DisabledBackground.Should().Be(background);
                    item.Colors.Foreground.Should().Be(foreground);
                }

                foreach (string name in new[] { "toolStripButtonCommit", "toolStripButtonPush" })
                {
                    CaptureNode item = nodes.Single(node => node.FieldName == name);
                    item.Colors.Background.Should().Be("#00FFFFFF");
                    item.Colors.DisabledBackground.Should().Be("#00FFFFFF");
                    item.Colors.Foreground.Should().Be(sourceForeground);
                }

                CaptureNode stash = nodes.Single(node => node.FieldName == "toolStripSplitStash");
                stash.Colors.Background.Should().Be(stash.Visible == true ? "#00FFFFFF" : background);
                stash.Colors.DisabledBackground.Should().Be(stash.Visible == true ? "#00FFFFFF" : background);
                stash.Colors.Foreground.Should().Be(stash.Visible == true ? sourceForeground : foreground);
                CaptureNode stashSeparator = nodes.Single(node => node.FieldName == "toolStripSeparator2");
                stashSeparator.Colors.Background.Should().Be(stashSeparator.Visible == true ? "#00FFFFFF" : background);
                stashSeparator.Colors.DisabledBackground.Should().Be(stashSeparator.Visible == true ? "#00FFFFFF" : background);
                nodes.Single(node => node.FieldName == "toolStripButtonPull").Visible.Should().BeTrue();
                nodes.Single(node => node.FieldName == "toolStripButtonCommit").Visible.Should().BeTrue();
                nodes.Single(node => node.FieldName == "toolStripFileExplorer").Visible.Should().BeFalse();
                nodes.Single(node => node.FieldName == "_gridView").Colors.Foreground.Should().Be(windowText);
                nodes.Where(node => node.FieldName == "sepRefresh")
                    .Should().HaveCount(2).And.OnlyContain(node => node.Colors.Background == background);

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                CaptureNode[] selectedDiffNodes = [.. Flatten(new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root)];
                selectedDiffNodes.Where(node => node.FieldName == "sepRefresh")
                    .Select(node => node.Colors.Background)
                    .Should().Contain(theme == ThemeVariant.Light ? "#FFFFFFFF" : "#FF323232");
                form.CommitInfoTabControl.SelectedItem = form.TreeTabPage;
                Dispatcher.UIThread.RunJobs();
                CaptureNode[] selectedTreeNodes = [.. Flatten(new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root)];
                foreach (string name in new[] { "btnCollapseGroups", "btnRefresh" })
                {
                    CaptureNode item = selectedTreeNodes.Where(node => node.FieldName == name).Last();
                    item.Colors.Background.Should().Be(theme == ThemeVariant.Light ? "#FFFFFFFF" : "#FF323232");
                    item.Colors.Foreground.Should().Be(windowText);
                }

                selectedTreeNodes.Where(node => node.FieldName == "sepRefresh").Last()
                    .Colors.Background.Should().Be(background);
                form.CommitInfoTabControl.SelectedItem = form.CommitInfoTabPage;
                Dispatcher.UIThread.RunJobs();

                form.Width = 760;
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                CaptureNode[] narrowNodes = [.. Flatten(new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root)];
                foreach (string name in new[] { "toolStripSplitStash", "toolStripSeparator2" })
                {
                    CaptureNode item = narrowNodes.Single(node => node.FieldName == name);
                    item.Visible.Should().BeFalse();
                    item.Colors.Background.Should().Be(background);
                    item.Colors.DisabledBackground.Should().Be(background);
                    if (name == "toolStripSplitStash")
                    {
                        item.Colors.Foreground.Should().Be(foreground);
                    }
                }

                form.Width = 923;
                Dispatcher.UIThread.RunJobs();
            }
        }
        finally
        {
            form.Close();
        }

        static IEnumerable<CaptureNode> Flatten(CaptureNode node)
        {
            yield return node;
            foreach (CaptureNode child in node.Children)
            {
                foreach (CaptureNode descendant in Flatten(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public async Task Browse_custom_panel_color_should_not_replace_the_grid_window_color()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module))
        {
            RequestedThemeVariant = ThemeVariant.Dark,
        };
        Color panelColor = Color.Parse("#2B2D3A");
        form.Resources["GitExtensionsPanelBackgroundBrush"] = new SolidColorBrush(panelColor);
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null
                && form.RevisionGrid.FindControl<ListBox>("_gridView")?.Background is ISolidColorBrush
                && form.RevisionInfo.Background is ISolidColorBrush);
            ListBox grid = form.RevisionGrid.FindControl<ListBox>("_gridView")
                ?? throw new AssertionException("The revision grid was not materialized.");
            grid.Background.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(panelColor,
                "the original DataGridView BackColor follows AppColor.PanelBackground");
            ScrollViewer viewport = grid.GetVisualDescendants().OfType<ScrollViewer>()
                .Single(control => control.Name == "PART_ScrollViewer");
            viewport.Background.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(Color.Parse("#323232"),
                "the original DataGridView BackgroundColor paints its empty viewport with SystemColors.Window");
            form.RevisionInfo.Background.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(panelColor,
                "FormBrowse explicitly overrides the original CommitInfo control's background");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_capture_should_open_the_main_navigate_and_view_menus()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        try
        {
            foreach (string name in new[] { "navigateToolStripMenuItem", "viewToolStripMenuItem" })
            {
                MenuItem mainMenuItem = GetMainMenuItem(form, name);
                mainMenuItem.Should().BeOfType<SourceControls.ToolStripMenuItem>();
                MenuItem gridContextItem = name == "navigateToolStripMenuItem"
                    ? form.RevisionGrid.NavigateMenuItem
                    : form.RevisionGrid.ViewMenuItem;
                using (AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(form, new CaptureStatePlan
                       {
                           Id = name,
                           Kind = CaptureStateKind.MenuOpen,
                           TargetField = name,
                           WidthDip = name == "viewToolStripMenuItem" ? 923 : null,
                           HeightDip = name == "viewToolStripMenuItem" ? 866 : null,
                       }))
                {
                    mainMenuItem.IsSubMenuOpen.Should().BeTrue();
                    gridContextItem.IsSubMenuOpen.Should().BeFalse();
                    if (name == "viewToolStripMenuItem")
                    {
                        MenuItem toolbars = mainMenuItem.Items.OfType<MenuItem>()
                            .Single(item => item.Name == "toolbarsMenuItem");
                        MenuItem standard = toolbars.Items.OfType<MenuItem>().First();
                        MenuItem worktrees = standard.Items.OfType<MenuItem>()
                            .Single(item => item.Header?.ToString() == "Worktrees");
                        worktrees.IsChecked.Should().Be(
                            AppSettings.GetBool("formbrowse_toolbar_visibility_toolStripWorktrees", true));
                    }

                    CaptureNode primary = new AvaloniaControlTreeReader(form, renderScale: 1)
                        .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root;
                    CaptureNode mainMenu = Flatten(primary).Single(node => node.FieldName == "mainMenuStrip");
                    CaptureNode openedMenu = mainMenu.Children.Single(node => node.Name == name);
                    openedMenu.Expanded.Should().BeFalse("the source primary tree tracks the popup on a separate surface");
                    openedMenu.Children
                        .Should().OnlyContain(node => node.Visible == false,
                            "submenu rows are captured separately on their popup surface");
                    Control popupRoot = driver.PopupSurfaceRoots.Should().ContainSingle().Subject;
                    CaptureNode popup = new AvaloniaControlTreeReader(form, renderScale: 1)
                        .ReadSurface(
                            popupRoot,
                            "popup:0",
                            new PixelRect(0, 0, (int)popupRoot.Bounds.Width, (int)popupRoot.Bounds.Height))
                        .Root;
                    popup.Children.Where(node => node.Type == typeof(Separator).FullName)
                        .Should().NotBeEmpty()
                        .And.OnlyContain(node => node.BoundsDip.Width == popup.BoundsDip.Width - 4);
                    if (name == "viewToolStripMenuItem")
                    {
                        popup.BoundsDip.Height.Should().Be(866);
                        popup.Children.Should().HaveCount(45,
                            "the two logical Toolbars rows lie below the rendered native viewport");
                        popup.Children.Last().Name.Should().Be("SaveAsDefault");
                    }
                }
            }

            const string staticMenuName = "repositoryToolStripMenuItem";
            using (AvaloniaControlStateDriver.Apply(form, new CaptureStatePlan
                   {
                       Id = staticMenuName,
                       Kind = CaptureStateKind.MenuOpen,
                       TargetField = staticMenuName,
                   }))
            {
                CaptureNode primary = new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root;
                CaptureNode mainMenu = Flatten(primary).Single(node => node.FieldName == "mainMenuStrip");
                mainMenu.Children.Single(node => node.Name == staticMenuName).Children
                    .Should().Contain(node => node.Visible == true,
                        "the source primary tree retains visible rows for static Browse menus");
            }
        }
        finally
        {
            form.Close();
        }

        static IEnumerable<CaptureNode> Flatten(CaptureNode node)
        {
            yield return node;
            foreach (CaptureNode child in node.Children)
            {
                foreach (CaptureNode descendant in Flatten(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_filter_toolbar_should_use_remaining_width_without_clipping_commands()
    {
        using FormBrowse form = new() { Width = 1200, Height = 573 };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            FilterToolBar filters = form.FindControl<FilterToolBar>("ToolStripFilters")!;
            Control advancedFilter = filters.FindControl<Control>("tsbtnAdvancedFilter")!;
            Control showReflog = filters.FindControl<Control>("tsbShowReflog")!;
            Control showBranches = filters.FindControl<Control>("tssbtnShowBranches")!;
            form.toolStripFiltersHost.Bounds.Width.Should().BeGreaterThan(200);
            advancedFilter.Opacity.Should().Be(1);
            showBranches.Opacity.Should().Be(1);
            showReflog.Margin.Should().Be(default(Thickness));
            showBranches.Margin.Should().Be(default(Thickness));
            form.toolStripFiltersOverflow.IsVisible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }

        using FormBrowse narrow = new() { Width = 600, Height = 573 };
        narrow.Show();
        try
        {
            narrow.SizeToContent = SizeToContent.Manual;
            narrow.Width = 600;
            Dispatcher.UIThread.RunJobs();
            narrow.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            narrow.toolStripFiltersHost.Bounds.Width.Should().Be(50);
            narrow.FindControl<FilterToolBar>("ToolStripFilters")!
                .FindControl<Control>("tsbtnAdvancedFilter")!.Opacity.Should().Be(0,
                    "even the first item cannot fit beside the overflow button");
        }
        finally
        {
            narrow.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_toolbar_overflow_should_reach_commands_between_the_first_and_last_page()
    {
        using FormBrowse form = new() { Width = 360, Height = 573 };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            ScrollViewer viewport = form.toolStripMainViewport;
            Button overflow = form.toolStripMainOverflow;
            double maximumOffset = form.ToolStripMain.Bounds.Width - viewport.Viewport.Width;
            maximumOffset.Should().BeGreaterThan(viewport.Viewport.Width);
            overflow.IsVisible.Should().BeTrue();

            overflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            viewport.Offset.X.Should().BeGreaterThan(0).And.BeLessThan(maximumOffset);
            overflow.Content.Should().Be("»");

            while (viewport.Offset.X < maximumOffset)
            {
                overflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();
            }

            overflow.Content.Should().Be("«");
            Point settingsPosition = form.EditSettings.TranslatePoint(default, viewport)!.Value;
            settingsPosition.X.Should().BeGreaterThanOrEqualTo(0);
            settingsPosition.X.Should().BeLessThan(viewport.Viewport.Width);

            overflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            viewport.Offset.X.Should().Be(0);
            overflow.Content.Should().Be("»");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_toolbar_should_keep_branch_and_commit_visible_with_a_long_repository_caption()
    {
        using FormBrowse form = new() { Width = 760, Height = 573 };
        form.Show();
        try
        {
            WorkingDirectoryToolStripSplitButton selector = form.FindControl<WorkingDirectoryToolStripSplitButton>(
                "_NO_TRANSLATE_WorkingDir")!;
            selector.Content = new string('x', 200);
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            ScrollViewer viewport = form.toolStripMainViewport;
            Point branchPosition = form.branchSelect.TranslatePoint(default, viewport)!.Value;
            Point commitPosition = form.toolStripButtonCommit.TranslatePoint(default, viewport)!.Value;
            branchPosition.X.Should().BeGreaterThanOrEqualTo(0);
            commitPosition.X.Should().BeGreaterThan(branchPosition.X);
            (commitPosition.X + form.toolStripButtonCommit.Bounds.Width)
                .Should().BeLessThanOrEqualTo(viewport.Viewport.Width);
            selector.Bounds.Width.Should().BeGreaterThanOrEqualTo(83);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_toolbar_should_keep_the_stash_icon_visible_at_the_source_width()
    {
        using FormBrowse form = new() { Width = 923, Height = 573 };
        form.Show();
        try
        {
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            IconSplitButton stash = form.FindControl<IconSplitButton>("toolStripSplitStash")!;
            Point stashPosition = stash.TranslatePoint(default, form.toolStripMainViewport)!.Value;
            stash.Icon.Should().NotBeNull();
            stash.Content.Should().Be(string.Empty);
            stash.Classes.Should().Contain("gitextensions-icon-only");
            (stashPosition.X + stash.Bounds.Width)
                .Should().BeLessThanOrEqualTo(form.toolStripMainViewport.Viewport.Width);
            foreach (Control ancestor in stash.GetVisualAncestors().OfType<Control>().Where(control => control.ClipToBounds))
            {
                Point position = stash.TranslatePoint(default, ancestor)!.Value;
                (position.X + stash.Bounds.Width).Should().BeLessThanOrEqualTo(
                    ancestor.Bounds.Width,
                    $"{ancestor.Name ?? ancestor.GetType().Name} must not clip the Stash button");
            }

            CaptureSurface surface = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize(923, 573));
            FindStashOrNull(surface.Root)!.Visible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }

        static CaptureNode? FindStashOrNull(CaptureNode node)
            => node.FieldName == "toolStripSplitStash"
                ? node
                : node.Children.Select(FindStashOrNull).FirstOrDefault(child => child is not null);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_diff_tab_should_use_the_source_file_list_and_splitter_widths()
    {
        using FormBrowse form = new();
        form.Show();
        try
        {
            form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Grid split = form.revisionDiff.FindControl<Grid>("DiffSplitContainer")!;
            split.Bounds.Width.Should().Be(643);
            split.Bounds.Height.Should().Be(260);
            split.ColumnDefinitions[0].ActualWidth.Should().Be(300);
            split.ColumnDefinitions[1].ActualWidth.Should().Be(6);
            split.ColumnDefinitions[2].ActualWidth.Should().BeGreaterThan(300);
            form.revisionDiff.FindControl<FileStatusList>("DiffFiles")!.Bounds.Width.Should().Be(300);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_diff_tab_should_keep_the_original_refresh_and_find_toolbar_commands()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        try
        {
            form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            FileStatusList files = form.revisionDiff.FindControl<FileStatusList>("DiffFiles")!;
            files.CanUseFindInCommitFilesGitGrep.Should().BeTrue();
            files.FindControl<Button>("btnRefresh")!.IsVisible.Should().BeTrue();
            files.FindControl<Separator>("sepRefresh")!.IsVisible.Should().BeTrue();
            files.FindControl<IconSplitButton>("btnFindInFilesGitGrep")!.IsVisible.Should().BeTrue();
            files.FindControl<Separator>("sepOptions")!.IsVisible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_shell_split_button_should_expose_only_launchable_platform_choices()
    {
        using FormBrowse form = new();
        IconSplitButton userShell = form.FindControl<IconSplitButton>("userShell")!;
        MenuFlyout menu = (MenuFlyout)userShell.Flyout!;
        string[] actual = menu.Items.OfType<MenuItem>()
            .Select(item => item.Header?.ToString() ?? string.Empty)
            .ToArray();

        if (OperatingSystem.IsWindows())
        {
            string[] expected = new ShellProvider().GetShells()
                .Where(shell => shell.HasExecutable)
                .Select(shell => shell.Name)
                .ToArray();
            actual.Should().Equal(expected);
            userShell.Tag.Should().BeAssignableTo<IShellDescriptor>();
        }
        else
        {
            actual.Should().Equal("System terminal");
        }
    }

    [AvaloniaTest]
    public void FormBrowse_should_show_dashboard_only_when_the_repository_is_invalid()
    {
        GitModule invalidModule = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            _workingDirectory);
        using (FormBrowse dashboardForm = new(new GitUICommands(_serviceContainer, invalidModule)))
        {
            dashboardForm.Show();
            Dispatcher.UIThread.RunJobs();

            Dashboard dashboard = dashboardForm.GetVisualDescendants().OfType<Dashboard>()
                .Should().ContainSingle(dashboard => dashboard.Name == "dashboard" && dashboard.IsVisible)
                .Which;
            dashboardForm.FindControl<Grid>("mainContentGrid")!.IsVisible.Should().BeFalse();
            dashboardForm.FindControl<SourceControls.ToolStripContainer>("toolPanel")!.IsVisible.Should().BeTrue();
            MenuItem dashboardMenu = dashboardForm.FindControl<MenuItem>("dashboardToolStripMenuItem")!;
            dashboardMenu.IsVisible.Should().BeTrue();
            dashboardMenu.Items.OfType<MenuItem>()
                .Should().ContainSingle(item => item.Name == "mnuConfigure");
            dashboard.GetVisualDescendants().OfType<Button>()
                .Should().NotContain(button => button.Name == "mnuConfigure");
            dashboard.GetTestAccessor().Repositories.GetTestAccessor().Search.IsFocused.Should().BeTrue();
            dashboardForm.FindControl<MenuItem>("repositoryToolStripMenuItem")!.IsVisible.Should().BeFalse();
        }

        invalidModule.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        using FormBrowse repositoryForm = new(new GitUICommands(_serviceContainer, invalidModule));
        repositoryForm.Show();
        Dispatcher.UIThread.RunJobs();

        repositoryForm.GetVisualDescendants().OfType<Dashboard>().Should().BeEmpty();
        repositoryForm.FindControl<Grid>("mainContentGrid")!.IsVisible.Should().BeTrue();
        repositoryForm.FindControl<SourceControls.ToolStripContainer>("toolPanel")!.IsVisible.Should().BeTrue();
        repositoryForm.FindControl<MenuItem>("dashboardToolStripMenuItem")!.IsVisible.Should().BeFalse();
        repositoryForm.FindControl<MenuItem>("repositoryToolStripMenuItem")!.IsVisible.Should().BeTrue();
        repositoryForm.FindControl<MenuItem>("editgitignoreToolStripMenuItem1").Should().NotBeNull();
        repositoryForm.FindControl<MenuItem>("editGitAttributesToolStripMenuItem").Should().NotBeNull();
        repositoryForm.FindControl<MenuItem>("cleanupToolStripMenuItem").Should().NotBeNull();
        repositoryForm.FindControl<MenuItem>("checkoutToolStripMenuItem").Should().NotBeNull();
        repositoryForm.FindControl<MenuItem>("bisectToolStripMenuItem").Should().NotBeNull();
        repositoryForm.FindControl<MenuItem>("formatPatchToolStripMenuItem").Should().NotBeNull();
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_show_the_empty_repository_surface_when_no_revision_exists()
    {
        GitModule module = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.Show();

        TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
        await WaitUntilAsync(() => loadingStatus.Text == "0 revisions");
        form.RevisionGrid.GetTestAccessor().CurrentPage.Should().BeOfType<EmptyRepoControl>();
    }

    [AvaloniaTest]
    [TestCase(1)]
    [TestCase(1.25)]
    [TestCase(1.5)]
    [TestCase(2)]
    public void FormBrowse_should_measure_tab_headers_with_source_padding(double scale)
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.Show();
            form.SetRenderScaling(scale);
            // The headless scaling helper preserves its physical window size. Give every
            // case the same client space after scaling instead of testing a clipped pane.
            form.Width = 1400;
            form.Height = 850;
            Dispatcher.UIThread.RunJobs();
            if (Environment.GetEnvironmentVariable("GITEXT_TAB_HEADER_EVIDENCE") is { Length: > 0 } evidenceDirectory)
            {
                Directory.CreateDirectory(evidenceDirectory);
                using WriteableBitmap? frame = form.CaptureRenderedFrame();
                frame?.Save(Path.Combine(evidenceDirectory, $"tab-headers-{scale}.png"), PngBitmapEncoderOptions.Default);
            }

            TabControl tabs = form.FindControl<TabControl>("CommitInfoTabControl")!;
            TabItem[] pages = tabs.Items.OfType<TabItem>().Where(page => page.IsVisible).ToArray();
            pages.Should().NotBeEmpty();
            foreach (TabItem page in pages)
            {
                page.Padding.Should().Be(new Avalonia.Thickness(8, 6));
                double.IsNaN(page.Height).Should().BeTrue();
                page.Bounds.Height.Should().BeGreaterThanOrEqualTo(28, $"tab {page.Name}, window {form.Bounds}, tabs {tabs.Bounds}");
                Border layoutRoot = page.GetVisualDescendants().OfType<Border>()
                    .Single(border => border.Name == "PART_LayoutRoot");
                TextBlock caption = page.GetVisualDescendants().OfType<TextBlock>()
                    .Single(text => text.Text == page.Header?.ToString());
                Avalonia.Point origin = Avalonia.VisualExtensions.TranslatePoint(caption, default, layoutRoot)!.Value;
                origin.X.Should().BeGreaterThanOrEqualTo(layoutRoot.Padding.Left + 16);
                (origin.X + caption.Bounds.Width).Should().BeLessThanOrEqualTo(
                    layoutRoot.Bounds.Width - layoutRoot.Padding.Right + 1);
            }

            double previousHeight = pages[0].Bounds.Height;
            pages[0].FontSize = 20;
            Dispatcher.UIThread.RunJobs();
            pages[0].Bounds.Height.Should().BeGreaterThan(previousHeight);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FormBrowse_should_paint_source_tab_overflow_and_tree_scrollbar_surfaces()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module))
        {
            Width = 923,
            Height = 573,
        };
        form.Show();
        TreeView tree = form.repoObjectsTree.FindControl<TreeView>("treeMain")!;
        tree.ItemsSource = new object[]
        {
            new TreeViewItem { Header = new string('W', 80) },
        };

        foreach ((ThemeVariant theme, string tab, string overflow, string track, string thumb) in
                 new[]
                 {
                     (ThemeVariant.Light, "#FFF3F3F3", "#FFFFFFFF", "#FFF0F0F0", "#FF858585"),
                     (ThemeVariant.Dark, "#FF202020", "#FF232323", "#FF171717", "#FF959595"),
                 })
        {
            form.RequestedThemeVariant = theme;
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Border unselectedTabSurface = form.DiffTabPage.GetVisualDescendants()
                .OfType<Border>()
                .Single(border => border.Name == "PART_LayoutRoot");
            GetColor(unselectedTabSurface.Background).Should().Be(Color.Parse(tab));
            GetColor(form.FindControl<Button>("toolStripMainOverflow")!.Background).Should().Be(Color.Parse(overflow));
            Avalonia.Controls.Primitives.ScrollBar horizontal = tree.GetVisualDescendants()
                .OfType<Avalonia.Controls.Primitives.ScrollBar>()
                .Single(scrollBar => scrollBar.Orientation == Avalonia.Layout.Orientation.Horizontal);
            Avalonia.Controls.Primitives.Thumb position = horizontal.GetVisualDescendants()
                .OfType<Avalonia.Controls.Primitives.Thumb>()
                .Single();
            Border indicator = position.GetVisualDescendants()
                .OfType<Border>()
                .Single(border => border.Name == "PART_NativeIndicator");
            horizontal.Bounds.Height.Should().Be(17);
            GetColor(horizontal.Background).Should().Be(Color.Parse(track));
            position.Bounds.Height.Should().Be(17);
            position.IsHitTestVisible.Should().BeTrue();
            indicator.Bounds.Height.Should().Be(2);
            GetColor(indicator.Background).Should().Be(Color.Parse(thumb));

            Point thumbHitPoint = position.TranslatePoint(new Point(position.Bounds.Width / 2, 1), form)!.Value;
            Avalonia.Visual? hit = form.InputHitTest(thumbHitPoint) as Avalonia.Visual;
            hit?.GetSelfAndVisualAncestors().Should().Contain(position,
                "the full native scrollbar row, not only its two-pixel indicator, must accept mouse input");

            foreach (string name in new[]
                     {
                         "toolStripSeparator0",
                         "toolStripSeparator17",
                         "toolStripSeparator1",
                         "toolStripSeparator2",
                     })
            {
                Border separator = form.FindControl<Border>(name)!;
                Border line = separator.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(border => border.Classes.Contains("gitextensions-toolbar-separator-line"));
                separator.Bounds.Width.Should().Be(6);
                GetColor(separator.Background).Should().Be(Colors.Transparent);
                line.Bounds.Width.Should().Be(1);
                line.Bounds.Height.Should().Be(17);
            }

            Button toggleLeftPanel = form.FindControl<Button>("toggleLeftPanel")!;
            toggleLeftPanel.Classes.Should().Contain("checked");
            GetColor(toggleLeftPanel.Background).Should().Be(
                Color.Parse(theme == ThemeVariant.Dark ? "#28445B" : "#CCE8FF"));
        }

        static Color GetColor(IBrush? brush)
            => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_focus_the_revision_list_after_loading_and_when_commanded()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.Show();
        TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
        await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");
        ListBox revisions = form.RevisionGrid.GetTestAccessor().Revisions;
        object? focused = TopLevel.GetTopLevel(revisions)?.FocusManager?.GetFocusedElement();
        revisions.IsKeyboardFocusWithin.Should().BeTrue($"the focused element was {focused}");

        ComboBox revisionFilter = form.ToolStripFilters.GetTestAccessor().RevisionFilter;
        form.ToolStripFilters.SetFocus();
        revisionFilter.IsKeyboardFocusWithin.Should().BeTrue();

        form.ExecuteCommand(FormBrowse.Command.FocusRevisionGrid).Should().BeTrue();
        revisions.IsKeyboardFocusWithin.Should().BeTrue();
        revisionFilter.IsKeyboardFocusWithin.Should().BeFalse();
    }

    [AvaloniaTest]
    public void QuickFetch_should_stop_when_the_before_fetch_script_cancels()
    {
        TestScriptEventRecorder scriptEvents = TestScriptEventRecorder.Install(_serviceContainer);
        scriptEvents.CancelledEvents.Add(ScriptEvent.BeforeFetch);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.ExecuteCommand(FormBrowse.Command.QuickFetch).Should().BeTrue();

        scriptEvents.Events.Should().Equal(ScriptEvent.BeforeFetch);
    }

    [AvaloniaTest]
    public void AddNotes_should_ignore_a_missing_revision_selection()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.ExecuteCommand(FormBrowse.Command.AddNotes).Should().BeTrue();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_branch_selector_should_open_and_checkout_the_selected_branch()
    {
        bool originalAlwaysShowCheckout = AppSettings.AlwaysShowCheckoutBranchDlg;
        bool originalCheckForUncommittedChanges = AppSettings.CheckForUncommittedChangesInCheckoutBranch;
        bool originalCloseProcessDialog = AppSettings.CloseProcessDialog;
        GitModule module = CreateRepositoryWithInitialCommit();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
        try
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = false;
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = true;
            AppSettings.CloseProcessDialog = true;
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            form.Show();
            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

            IconSplitButton branchSelector = form.FindControl<IconSplitButton>("branchSelect")!;
            Button[] templateButtons = branchSelector.GetVisualDescendants()
                .OfType<Button>()
                .Where(button => button.Name is "PART_PrimaryButton" or "PART_SecondaryButton")
                .ToArray();
            Button primaryButton = templateButtons.Single(button => button.Name == "PART_PrimaryButton");
            Button secondaryButton = templateButtons.Single(button => button.Name == "PART_SecondaryButton");
            MenuFlyout flyout = (MenuFlyout)branchSelector.Flyout!;

            Click(form, primaryButton, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            flyout.IsOpen.Should().BeTrue();
            MenuItem[] branchItems = flyout.Items.OfType<MenuItem>().Skip(1).ToArray();
            branchItems.Select(item => item.Header as string).Should().Contain("feature");
            branchItems.Should().OnlyContain(item => item.Icon is Image);
            MenuItem checkoutItem = flyout.Items.OfType<MenuItem>().First();
            checkoutItem.Width.Should().BeGreaterThan(200,
                "the branch menu must reserve the complete checkout shortcut column");
            if (OperatingSystem.IsWindows())
            {
                checkoutItem.Width.Should().Be(268,
                    "the Windows capture measured the native branch menu at 96 DPI");
            }

            flyout.Placement.Should().Be(PlacementMode.BottomEdgeAlignedLeft,
                "WinForms aligns the branch menu with the selector's leading edge");

            flyout.Hide();
            Dispatcher.UIThread.RunJobs();
            Click(form, secondaryButton, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            flyout.IsOpen.Should().BeTrue("the arrow retains Avalonia's native split-button behavior");
            MenuItem feature = flyout.Items
                .OfType<MenuItem>()
                .Single(item => item.Header as string == "feature");
            TopLevel popup = TopLevel.GetTopLevel(feature)
                ?? throw new InvalidOperationException("The branch flyout did not render.");
            Click(popup, feature, MouseButton.Left);

            await WaitUntilAsync(() => module.GetSelectedBranch() == "feature");
            await WaitUntilAsync(() => branchSelector.Content as string == "feature");
        }
        finally
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = originalAlwaysShowCheckout;
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = originalCheckForUncommittedChanges;
            AppSettings.CloseProcessDialog = originalCloseProcessDialog;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_should_show_artificial_revisions_and_live_toolbar_status()
    {
        bool originalShowArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        bool originalShowArtificialStatus = AppSettings.ShowGitStatusForArtificialCommits;
        bool originalShowToolbarStatus = AppSettings.ShowGitStatusInBrowseToolbar;
        bool originalShowAheadBehind = AppSettings.ShowAheadBehindData;
        string remoteDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.Remote-{Guid.NewGuid():N}");
        Directory.CreateDirectory(remoteDirectory);

        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            AppSettings.ShowGitStatusForArtificialCommits = true;
            AppSettings.ShowGitStatusInBrowseToolbar = true;
            AppSettings.ShowAheadBehindData = true;

            GitModule module = CreateRepositoryWithInitialCommit();
            GitModule remote = new(
                _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
                remoteDirectory);
            remote.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
            module.GitExecutable.RunCommand(
                new GitArgumentBuilder("remote") { "add", "origin", remoteDirectory });
            module.GitExecutable.RunCommand(
                new GitArgumentBuilder("push") { "--quiet", "--set-upstream", "origin", "HEAD" });

            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second commit");
            module.GitExecutable.RunCommand(
                new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "unstaged");
            File.WriteAllText(Path.Combine(_workingDirectory, "staged.txt"), "staged");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "staged.txt" });

            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            form.Show();

            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            IconButton commitButton = form.FindControl<IconButton>("toolStripButtonCommit")!;
            ToolStripPushButton pushButton = form.FindControl<ToolStripPushButton>("toolStripButtonPush")!;

            await WaitUntilAsync(() =>
                loadingStatus.Text == "4 revisions"
                && commitButton.Content?.ToString() == "Commit (2)"
                && pushButton.GetTestAccessor().GetButtonText() == "1↑");
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            commitButton.Bounds.Width.Should().Be(Math.Ceiling(commitButton.Bounds.Width));
            commitButton.Bounds.Width.Should().BeGreaterThanOrEqualTo(88);
            if (OperatingSystem.IsWindows())
            {
                commitButton.Bounds.Width.Should().Be(88);
            }

            form.RevisionGrid.ShowUncommittedChangesIfPossible.Should().BeTrue();
            form.RevisionGrid.GetChangeCount(ObjectId.WorkTreeId)!.Changed.Should().ContainSingle();
            form.RevisionGrid.GetChangeCount(ObjectId.IndexId)!.New.Should().ContainSingle();
            commitButton.Icon.Should().BeSameAs(Images.RepoStateMixed);
            form.RevisionGrid.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .Select(label => label.Label)
                .Should()
                .Contain([ResourceManager.TranslatedStrings.Workspace, ResourceManager.TranslatedStrings.Index]);
            RevisionGridControl.TestAccessor revisionGrid = form.RevisionGrid.GetTestAccessor();
            revisionGrid.HasGraphParent(ObjectId.WorkTreeId, ObjectId.IndexId).Should().BeTrue();
            revisionGrid.HasGraphParent(ObjectId.IndexId, module.GetCurrentCheckout()).Should().BeTrue();
        }
        finally
        {
            AppSettings.RevisionGraphShowArtificialCommits = originalShowArtificial;
            AppSettings.ShowGitStatusForArtificialCommits = originalShowArtificialStatus;
            AppSettings.ShowGitStatusInBrowseToolbar = originalShowToolbarStatus;
            AppSettings.ShowAheadBehindData = originalShowAheadBehind;
            TestDirectory.Delete(remoteDirectory);
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_reload_after_repository_changed_notification()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        GitUICommands commands = new(_serviceContainer, module);
        FormBrowse form = new(commands);
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");

            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

            RevisionGridRefRenderer.RefLabelControl currentBranch =
                revisionGrid.GetVisualDescendants()
                    .OfType<RevisionGridRefRenderer.RefLabelControl>()
                    .Single();
            currentBranch.Icon.Should().Be(RefLabelIcon.Head);
            currentBranch.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);
            TextBlock currentCommitSubject = revisionGrid.GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-subject"));
            currentCommitSubject.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);

            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "updated");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId remoteCommit = module.GetCurrentCheckout();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("reset") { "--quiet", "--hard", "HEAD~" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { "refs/remotes/origin/main", remoteCommit });

            bool reloadStarted = false;
            loadingStatus.PropertyChanged += (_, e) =>
                reloadStarted |= e.Property == TextBlock.TextProperty && loadingStatus.Text == "Loading";
            commands.RepoChangedNotifier.Notify();

            await WaitUntilAsync(() => reloadStarted && loadingStatus.Text == "2 revisions");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_should_load_stashes_into_the_left_panel_and_select_their_revision()
    {
        bool originalShowStashes = AppSettings.ShowStashes;
        bool originalShowStashTree = AppSettings.RepoObjectsTreeShowStashes;
        FormBrowse? form = null;
        try
        {
            AppSettings.ShowStashes = true;
            AppSettings.RepoObjectsTreeShowStashes = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "stashed");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "older left panel".Quote() });
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "stashed again");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "latest left panel".Quote() });
            IReadOnlyCollection<GitRevision> stashes = new RevisionReader(module).GetStashes(CancellationToken.None);
            GitRevision olderStash = stashes.Last();
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RepoObjectsTree repoObjectsTree = form.FindControl<RepoObjectsTree>("repoObjectsTree")!;
            RepoObjectsTree.TestAccessor accessor = repoObjectsTree.GetTestAccessor();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;

            await WaitUntilAsync(() => accessor.Tree.Items.Count == 6);
            TreeViewItem stashRoot = accessor.Tree.Items.Cast<TreeViewItem>().Last();
            await WaitUntilAsync(() => stashRoot.Items.Count == 2);
            TreeViewItem stashItem = stashRoot.Items.Cast<TreeViewItem>().Last();

            accessor.Tree.SelectedItem = stashItem;

            await WaitUntilAsync(() => revisionGrid.SelectedRevision?.ObjectId == olderStash.ObjectId);
            repoObjectsTree.SelectedRevisionObjectId.Should().Be(olderStash.ObjectId);
            revisionGrid.SelectedRevision!.ReflogSelector.Should().Be("refs/stash@{1}");
            revisionGrid.GetTestAccessor().Revisions.Items
                .Cast<GitRevision>()
                .Select(revision => revision.ObjectId)
                .Should().OnlyHaveUniqueItems();
        }
        finally
        {
            form?.Close();
            AppSettings.ShowStashes = originalShowStashes;
            AppSettings.RepoObjectsTreeShowStashes = originalShowStashTree;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    [Category("P8.6h.3b.2b.2b.2b.4")]
    public async Task Revision_grid_should_publish_the_final_graph_order_with_stash_and_artificial_rows()
    {
        bool originalShowStashes = AppSettings.ShowStashes;
        bool originalShowArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        FormBrowse? form = null;
        try
        {
            AppSettings.ShowStashes = true;
            AppSettings.RevisionGraphShowArtificialCommits = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId head = module.GetCurrentCheckout();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "stashed");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "row order".Quote() });
            ObjectId stash = module.RevParse("refs/stash");
            ObjectId stashIndex = module.RevParse("refs/stash^2");
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "working");

            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RevisionGridControl revisionGrid = form.RevisionGrid;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "6 revisions");
            ObjectId initial = module.RevParse("HEAD~1");
            ObjectId[] revisionOrder = [.. revisionGrid.GetTestAccessor().Revisions.Items
                .Cast<GitRevision>()
                .Select(revision => revision.ObjectId)];

            revisionOrder.Should().BeEquivalentTo([stash, stashIndex, ObjectId.WorkTreeId, ObjectId.IndexId, head, initial]);
            revisionOrder.Should().OnlyHaveUniqueItems();
            Array.IndexOf(revisionOrder, stash).Should().BeLessThan(Array.IndexOf(revisionOrder, stashIndex));
            int artificialIndex = Array.IndexOf(revisionOrder, ObjectId.WorkTreeId);
            revisionOrder.Skip(artificialIndex).Take(2).Should().Equal(ObjectId.WorkTreeId, ObjectId.IndexId);
            artificialIndex.Should().BeLessThan(Array.IndexOf(revisionOrder, head));

            revisionGrid.SetSelectedRevision(head).Should().BeTrue();
            ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem deleteBranch = revisionGrid.FindControl<MenuItem>("deleteBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Delete-branch menu item was not created.");
            contextMenu.Open(revisionGrid.GetTestAccessor().Revisions);
            Dispatcher.UIThread.RunJobs();
            deleteBranch.IsVisible.Should().BeTrue();
            deleteBranch.IsEnabled.Should().BeFalse();
            deleteBranch.Bounds.Height.Should().BeGreaterThan(0);
            contextMenu.Close();
        }
        finally
        {
            form?.Close();
            AppSettings.ShowStashes = originalShowStashes;
            AppSettings.RevisionGraphShowArtificialCommits = originalShowArtificial;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_should_expose_loaded_worktrees_in_the_left_panel_and_toolbar()
    {
        bool originalShowWorktrees = AppSettings.RepoObjectsTreeShowWorktrees;
        string linkedPath = $"{_workingDirectory}-linked";
        FormBrowse? form = null;
        try
        {
            AppSettings.RepoObjectsTreeShowWorktrees = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("worktree") { "add", "--quiet", linkedPath.Quote(), "feature" });
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RepoObjectsTree repoObjectsTree = form.FindControl<RepoObjectsTree>("repoObjectsTree")!;
            RepoObjectsTree.TestAccessor accessor = repoObjectsTree.GetTestAccessor();
            IconSplitButton worktreeButton = form.FindControl<IconSplitButton>("toolStripWorktrees")!;

            await WaitUntilAsync(() => worktreeButton.IsVisible);

            TreeViewItem root = accessor.Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Worktrees", StringComparison.Ordinal));
            HeaderText(root).Should().Be("Worktrees");
            root.Items.Cast<TreeViewItem>().Should().HaveCount(2);

            MenuFlyout flyout = (MenuFlyout)worktreeButton.Flyout!;
            flyout.ShowAt(worktreeButton);
            Dispatcher.UIThread.RunJobs();
            MenuItem[] worktreeItems = flyout.Items.OfType<MenuItem>().Take(2).ToArray();
            worktreeItems.Should().HaveCount(2);
            worktreeItems[0].IsChecked.Should().BeTrue();
            worktreeItems[0].IsEnabled.Should().BeFalse();
            worktreeItems[1].Header!.ToString().Should().Contain("feature");
            worktreeItems[1].IsEnabled.Should().BeTrue();
            flyout.Items.OfType<MenuItem>().Skip(2).Select(item => item.Header!.ToString()).Should().Equal(
                GitUI.TranslatedStrings.CreateWorktree,
                GitUI.TranslatedStrings.PruneWorktrees,
                GitUI.TranslatedStrings.ManageWorktrees);
            flyout.Hide();
        }
        finally
        {
            form?.Close();
            AppSettings.RepoObjectsTreeShowWorktrees = originalShowWorktrees;
            TestDirectory.Delete(linkedPath);
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FormBrowse_should_publish_submodule_provider_updates_in_the_source_toolbar_menu()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        string childPath = Path.Combine(_workingDirectory, "child");
        Directory.CreateDirectory(childPath);
        ISubmoduleStatusProvider provider = Substitute.For<ISubmoduleStatusProvider>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(Arg.Any<Type>()).Returns(call =>
            call.Arg<Type>() == typeof(ISubmoduleStatusProvider)
                ? provider
                : _serviceContainer.GetService(call.Arg<Type>()));

        using FormBrowse form = new(commands);
        form.Show();
        Dispatcher.UIThread.RunJobs();
        IconSplitButton levelUp = form.FindControl<IconSplitButton>("toolStripButtonLevelUp")!;
        MenuFlyout flyout = (MenuFlyout)levelUp.Flyout!;

        provider.StatusUpdating += Raise.Event<EventHandler>(provider, EventArgs.Empty);
        Dispatcher.UIThread.RunJobs();
        flyout.Items.OfType<MenuItem>().Should().ContainSingle()
            .Which.Should().Match<MenuItem>(item => item.Header!.ToString() == "Loading..." && item.IsEnabled);

        SubmoduleInfo top = new("top", _workingDirectory, bold: true);
        SubmoduleInfo child = new("child", childPath, bold: false);
        SubmoduleInfoResult result = new()
        {
            TopProject = top,
        };
        result.OurSubmodules.Add(child);
        result.AllSubmodules.Add(child);
        provider.StatusUpdated += Raise.Event<EventHandler<SubmoduleStatusEventArgs>>(
            provider,
            new SubmoduleStatusEventArgs(result, structureUpdated: true, CancellationToken.None));
        Dispatcher.UIThread.RunJobs();

        MenuItem[] menuItems = flyout.Items.OfType<MenuItem>().ToArray();
        menuItems.Select(item => item.Header!.ToString()).Should().Equal("child", "_Update all submodules");
        menuItems[0].Tag.Should().Be(childPath);
        menuItems[0].Icon.Should().BeOfType<Image>();
        flyout.Items.OfType<Separator>().Should().ContainSingle();
        ToolTip.GetTip(levelUp).Should().Be(string.Empty);

        form.ExecuteCommand(FormBrowse.Command.GoToSubmodule).Should().BeTrue();
        flyout.IsOpen.Should().BeTrue();
        flyout.Hide();
    }

    [AvaloniaTest]
    public void FormBrowse_worktree_surfaces_should_reuse_the_existing_translation_keys()
    {
        FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        MenuItem maintenance = form.FindControl<MenuItem>("gitMaintenanceToolStripMenuItem")!;
        maintenance.Items.OfType<MenuItem>().Should().ContainSingle(item => item.Name == "recoverLostObjectsToolStripMenuItem");

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "manageWorktreeToolStripMenuItem", "Text", "Manage &worktrees...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "toolStripMenuItemReflog", "Text", "Show reflo&g...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "toolStripWorktrees", "ToolTipText", "Worktrees");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "toolStripButtonLevelUp", "ToolTipText", "Submodules");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "archiveToolStripMenuItem", "Text", "Archi&ve revision...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitMaintenanceToolStripMenuItem", "Text", "&Git maintenance");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "compressGitDatabaseToolStripMenuItem", "Text", "&Compress git database");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "recoverLostObjectsToolStripMenuItem", "Text", "&Recover lost objects...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "deleteIndexLockToolStripMenuItem", "Text", "&Delete index.lock");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "editLocalGitConfigToolStripMenuItem", "Text", "&Edit .git/config");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "repoSettingsToolStripMenuItem", "Text", "Rep&ository settings...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "editgitignoreToolStripMenuItem1", "Text", "Edit .git&ignore");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "editgitinfoexcludeToolStripMenuItem", "Text", "Edit .git/info/&exclude");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "editGitAttributesToolStripMenuItem", "Text", "Edit .git&attributes");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "editmailmapToolStripMenuItem", "Text", "Edit .&mailmap");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "menuitemSparse", "Text", "Sparse Wor&king Copy");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "closeToolStripMenuItem", "Text", "&Close (go to Dashboard)");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "refreshDashboardToolStripMenuItem", "Text", "&Refresh");
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void FormBrowse_and_RevisionGrid_should_preserve_source_control_identities_on_native_Avalonia_controls()
    {
        using FormBrowse form = new();

        form.FindControl<SourceControls.MenuStripEx>("mainMenuStrip").Should().NotBeNull();
        form.FindControl<SourceControls.ToolStripMenuItem>("repositoryToolStripMenuItem").Should().NotBeNull();
        form.FindControl<SourceControls.ToolStripContainer>("toolPanel").Should().NotBeNull();
        form.FindControl<SourceControls.SplitContainer>("RightSplitContainer").Should().NotBeNull();
        form.FindControl<SourceControls.SplitContainer>("RevisionsSplitContainer").Should().NotBeNull();
        form.FindControl<SourceControls.Panel>("RevisionGridContainer").Should().NotBeNull();
        form.FindControl<SourceControls.TabPage>("CommitInfoTabPage").Should().NotBeNull();
        form.FindControl<SourceControls.TabPage>("DiffTabPage").Should().NotBeNull();
        form.FindControl<SourceControls.TabPage>("TreeTabPage").Should().NotBeNull();
        form.FindControl<SourceControls.TabPage>("GpgInfoTabPage").Should().NotBeNull();
        form.RevisionGrid.FindControl<SourceControls.ContextMenuStrip>("mainContextMenu").Should().NotBeNull();
        form.RevisionGrid.FindControl<SourceControls.ToolStripMenuItem>("commitToolStripMenuItem").Should().NotBeNull();
        form.RevisionGrid.FindControl<SourceControls.ToolStripSeparator>("sepCommit").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormBrowse_repository_menu_should_match_the_current_supported_WinForms_inventory()
    {
        using FormBrowse form = new();

        string[] actualItems = form.repositoryToolStripMenuItem.Items
            .Select(item => item switch
            {
                Separator => "|",
                MenuItem menuItem => menuItem.Name ?? throw new InvalidOperationException("A repository menu item has no name."),
                _ => throw new InvalidOperationException($"Unexpected repository menu entry: {item?.GetType().Name}"),
            })
            .ToArray();
        actualItems.Should().Equal(
            "refreshToolStripMenuItem",
            "fileExplorerToolStripMenuItem",
            "|",
            "manageRemoteRepositoriesToolStripMenuItem1",
            "|",
            "manageSubmodulesToolStripMenuItem",
            "updateAllSubmodulesToolStripMenuItem",
            "synchronizeAllSubmodulesToolStripMenuItem",
            "|",
            "manageWorktreeToolStripMenuItem",
            "|",
            "editgitignoreToolStripMenuItem1",
            "editgitinfoexcludeToolStripMenuItem",
            "editGitAttributesToolStripMenuItem",
            "editmailmapToolStripMenuItem",
            "menuitemSparse",
            "|",
            "gitMaintenanceToolStripMenuItem",
            "repoSettingsToolStripMenuItem",
            "|",
            "closeToolStripMenuItem");

        KeyGesture fileExplorerShortcut = new(Key.O, KeyModifiers.Control | KeyModifiers.Shift);
        form.fileExplorerToolStripMenuItem.HotKey.Should().BeEquivalentTo(fileExplorerShortcut);
        form.fileExplorerToolStripMenuItem.InputGesture.Should().BeEquivalentTo(fileExplorerShortcut);

        MenuItem maintenance = form.gitMaintenanceToolStripMenuItem;
        maintenance.Items.OfType<MenuItem>().Select(item => item.Name).Should().Equal(
            "compressGitDatabaseToolStripMenuItem",
            "recoverLostObjectsToolStripMenuItem",
            "deleteIndexLockToolStripMenuItem",
            "editLocalGitConfigToolStripMenuItem");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FormBrowse_refresh_shortcut_should_follow_the_visible_repository_or_dashboard_menu()
    {
        GitModule repository = CreateRepositoryWithInitialCommit();
        using FormBrowse repositoryForm = new(new GitUICommands(_serviceContainer, repository));

        repositoryForm.refreshToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.F5));
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(repositoryForm.refreshToolStripMenuItem).Should().Be("F5");
        repositoryForm.refreshDashboardToolStripMenuItem.InputGesture.Should().BeNull();
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(repositoryForm.refreshDashboardToolStripMenuItem).Should().BeNull();

        string nonRepositoryPath = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Dashboard-{Guid.NewGuid():N}");
        Directory.CreateDirectory(nonRepositoryPath);
        try
        {
            GitModule dashboard = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), nonRepositoryPath);
            using FormBrowse dashboardForm = new(new GitUICommands(_serviceContainer, dashboard));

            dashboardForm.refreshToolStripMenuItem.InputGesture.Should().BeNull();
            WinFormsToolStripMenuSizer.GetShortcutDisplayString(dashboardForm.refreshToolStripMenuItem).Should().BeNull();
            dashboardForm.refreshDashboardToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.F5));
            WinFormsToolStripMenuSizer.GetShortcutDisplayString(dashboardForm.refreshDashboardToolStripMenuItem).Should().Be("F5");
        }
        finally
        {
            TestDirectory.Delete(nonRepositoryPath);
        }
    }

    [AvaloniaTest]
    public void FormBrowse_repository_menu_should_route_ported_dialog_commands_and_preserve_bare_repository_state()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        using FormBrowse form = new(commands);

        form.manageRemoteRepositoriesToolStripMenuItem1.IsEnabled.Should().BeTrue();
        form.manageSubmodulesToolStripMenuItem.IsEnabled.Should().BeFalse();
        form.updateAllSubmodulesToolStripMenuItem.IsEnabled.Should().BeFalse();
        form.synchronizeAllSubmodulesToolStripMenuItem.IsEnabled.Should().BeFalse();
        form.gitMaintenanceToolStripMenuItem.IsEnabled.Should().BeTrue();
        form.repoSettingsToolStripMenuItem.IsEnabled.Should().BeTrue();

        form.manageRemoteRepositoriesToolStripMenuItem1.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.manageSubmodulesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.updateAllSubmodulesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.synchronizeAllSubmodulesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.recoverLostObjectsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.repoSettingsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.editgitignoreToolStripMenuItem1.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.editgitinfoexcludeToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.editGitAttributesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.editmailmapToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.menuitemSparse.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartRemotesDialog(form);
        commands.Received(1).StartSubmodulesDialog(form);
        commands.Received(1).StartUpdateSubmodulesDialog(form);
        commands.Received(1).StartSyncSubmodulesDialog(form);
        commands.Received(1).StartVerifyDatabaseDialog(form);
        commands.Received(1).StartRepoSettingsDialog(form);
        commands.Received(1).StartEditGitIgnoreDialog(form, false);
        commands.Received(1).StartEditGitIgnoreDialog(form, true);
        commands.Received(1).StartEditGitAttributesDialog(form);
        commands.Received(1).StartMailMapDialog(form);
        commands.Received(1).StartSparseWorkingCopyDialog(form);
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menus_should_match_the_current_supported_WinForms_inventory()
    {
        using FormBrowse form = new();

        GetItemNames(form.fileToolStripMenuItem).Should().Equal(
            "initNewRepositoryToolStripMenuItem",
            "openToolStripMenuItem",
            "tsmiFavouriteRepositories",
            "tsmiRecentRepositories",
            "|",
            "cloneToolStripMenuItem",
            "|",
            "exitToolStripMenuItem");
        GetItemNames(form.toolsToolStripMenuItem).Should().Equal(
            "gitBashToolStripMenuItem",
            "gitGUIToolStripMenuItem",
            "kGitToolStripMenuItem",
            "|",
            "PuTTYToolStripMenuItem",
            "|",
            "gitcommandLogToolStripMenuItem",
            "|",
            "settingsToolStripMenuItem");
        GetItemNames(form.helpToolStripMenuItem).Should().Equal(
            "userManualToolStripMenuItem",
            "changelogToolStripMenuItem",
            "|",
            "translateToolStripMenuItem",
            "|",
            "donateToolStripMenuItem",
            "tsmiTelemetryEnabled",
            "reportAnIssueToolStripMenuItem",
            "checkForUpdatesToolStripMenuItem",
            "aboutToolStripMenuItem");

        form.toolsToolStripMenuItem.GetTestAccessor().PuTTYMenuItem.IsVisible.Should().Be(OperatingSystem.IsWindows());

        return;

        static string[] GetItemNames(MenuItem parent)
            => parent.Items
                .Select(item => item switch
                {
                    Separator => "|",
                    MenuItem menuItem => menuItem.Name
                        ?? throw new InvalidOperationException($"A {parent.Name} child has no name."),
                    _ => throw new InvalidOperationException(
                        $"Unexpected {parent.Name} entry: {item?.GetType().Name}"),
                })
                .ToArray();
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menus_should_preserve_translation_identities()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "fileToolStripMenuItem", "Text", "&Start");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "initNewRepositoryToolStripMenuItem", "Text", "&Create new repository...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "openToolStripMenuItem", "Text", "&Open...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "cloneToolStripMenuItem", "Text", "C&lone repository...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitBashToolStripMenuItem", "Text", "Git &bash");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitGUIToolStripMenuItem", "Text", "Git &GUI");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "kGitToolStripMenuItem", "Text", "Git&K");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "PuTTYToolStripMenuItem", "Text", "&PuTTY");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "startAuthenticationAgentToolStripMenuItem", "Text", "Start authentication agent");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "generateOrImportKeyToolStripMenuItem", "Text", "Generate or import key");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitcommandLogToolStripMenuItem", "Text", "Git &command log");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "settingsToolStripMenuItem", "Text", "&Settings...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "userManualToolStripMenuItem", "Text", "User &manual");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "translateToolStripMenuItem", "Text", "&Translate");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "tsmiTelemetryEnabled", "Text", "&Yes, I allow telemetry");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "reportAnIssueToolStripMenuItem", "Text", "&Report an issue");
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menu_roots_should_not_receive_the_host_title_translation()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(nameof(FormBrowse), "$this", "Text", Arg.Any<Func<string>>())
            .Returns("Git Extensions");
        translation.TranslateItem(nameof(FormBrowse), "fileToolStripMenuItem", "Text", Arg.Any<Func<string>>())
            .Returns("&Start translated");
        translation.TranslateItem(nameof(FormBrowse), "toolsToolStripMenuItem", "Text", Arg.Any<Func<string>>())
            .Returns("&Tools translated");
        translation.TranslateItem(nameof(FormBrowse), "helpToolStripMenuItem", "Text", Arg.Any<Func<string>>())
            .Returns("&Help translated");

        form.TranslateItems(translation);

        form.fileToolStripMenuItem.Header.Should().Be("_Start translated");
        form.toolsToolStripMenuItem.Header.Should().Be("_Tools translated");
        form.helpToolStripMenuItem.Header.Should().Be("_Help translated");
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menus_should_route_commands_and_refresh_state()
    {
        bool isBare = false;
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(false);
        module.IsBareRepository().Returns(_ => isBare);
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        using FormBrowse form = new(commands);
        StartToolStripMenuItem.TestAccessor start = form.fileToolStripMenuItem.GetTestAccessor();
        ToolsToolStripMenuItem.TestAccessor tools = form.toolsToolStripMenuItem.GetTestAccessor();
        HelpToolStripMenuItem.TestAccessor help = form.helpToolStripMenuItem.GetTestAccessor();

        start.InitNewRepositoryMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        start.CloneMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        tools.GitGuiMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        tools.GitKMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        tools.SettingsMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        help.TelemetryMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartInitializeDialog(
            null,
            null,
            Arg.Any<EventHandler<GitModuleEventArgs>>());
        commands.Received(1).StartCloneDialog(
            null,
            string.Empty,
            false,
            Arg.Any<EventHandler<GitModuleEventArgs>>());
        module.Received(1).RunGui();
        module.Received(1).RunGitK();
        commands.Received(1).StartSettingsDialog(null, null);
        commands.Received(1).StartGeneralSettingsDialog(null);

        form.toolsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
        tools.GitGuiMenuItem.IsEnabled.Should().BeTrue();
        isBare = true;
        form.toolsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
        tools.GitGuiMenuItem.IsEnabled.Should().BeFalse();

        bool? originalTelemetry = AppSettings.TelemetryEnabled;
        try
        {
            AppSettings.TelemetryEnabled = true;
            form.helpToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            help.TelemetryMenuItem.IsChecked.Should().BeTrue();
        }
        finally
        {
            AppSettings.TelemetryEnabled = originalTelemetry;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_should_execute_the_git_command_log_shortcut_while_the_tools_menu_is_closed()
    {
        FormGitCommandLog.TestAccessor.OpenInstance?.Close();
        Dispatcher.UIThread.RunJobs();
        using FormBrowse form = new();
        KeyEventArgs keyEvent = new()
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.F12,
        };

        form.RaiseEvent(keyEvent);
        Dispatcher.UIThread.RunJobs();

        keyEvent.Handled.Should().BeTrue();
        FormGitCommandLog commandLog = FormGitCommandLog.TestAccessor.OpenInstance
            ?? throw new AssertionException("F12 did not open the Git command log.");
        commandLog.IsVisible.Should().BeTrue();
        form.toolsToolStripMenuItem.IsSubMenuOpen.Should().BeFalse();

        commandLog.Close();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FormBrowse_navigate_and_view_menus_should_match_the_supported_revision_grid_inventory()
    {
        using FormBrowse form = new();
        MenuItem navigate = GetMainMenuItem(form, "navigateToolStripMenuItem");
        MenuItem view = GetMainMenuItem(form, "viewToolStripMenuItem");

        form.mainMenuStrip.Items.OfType<MenuItem>().Select(item => item.Name).Should().Equal(
            "fileToolStripMenuItem",
            "dashboardToolStripMenuItem",
            "repositoryToolStripMenuItem",
            "navigateToolStripMenuItem",
            "viewToolStripMenuItem",
            "commandsToolStripMenuItem",
            "_repositoryHostsToolStripMenuItem",
            "pluginsToolStripMenuItem",
            "toolsToolStripMenuItem",
            "helpToolStripMenuItem");
        GetTaggedItemNames(navigate).Should().Equal(
            "ToggleBetweenArtificialAndHeadCommits",
            "GotoCurrentRevision",
            "GotoCommit",
            "|",
            "GotoChildCommit",
            "GotoParentCommit",
            "GotoFirstParentCommit",
            "GotoLastParentCommit",
            "GotoMergeBaseCommit",
            "|",
            "NavigateBackward",
            "NavigateForward",
            "|",
            "QuickSearch",
            "PrevQuickSearch",
            "NextQuickSearch");
        GetTaggedItemNames(view).Should().Equal(
            "BranchesToolStripMenuItem",
            "ShowAllBranches",
            "ShowCurrentBranchOnly",
            "ShowFilteredBranches",
            "ShowReflogReferences",
            "|",
            "filterToolStripMenuItem",
            "|",
            "drawNonrelativesGrayToolStripMenuItem",
            "HighlightSelectedBranch",
            "|",
            "CommitsToolStripMenuItem",
            "ShowArtificialCommits",
            "ShowStashes",
            "showGitNotesToolStripMenuItem",
            "ShowSessionCheckpoints",
            "|",
            "Grid_labelsToolStripMenuItem",
            "ShowRemoteBranches",
            "showTagsToolStripMenuItem",
            "ShowSuperprojectTags",
            "ShowSuperprojectRemoteBranches",
            "ShowSuperprojectBranches",
            "|",
            "Grid_infoToolStripMenuItem",
            "showBuildStatusIconToolStripMenuItem",
            "showBuildStatusTextToolStripMenuItem",
            "showCommitMessageBodyToolStripMenuItem",
            "showAuthorDateToolStripMenuItem",
            "showRelativeDateToolStripMenuItem",
            "|",
            "ColumnsToolStripMenuItem",
            "showRevisionGraphColumnToolStripMenuItem",
            "showGitNotesColumnToolStripMenuItem",
            "showAuthorAvatarColumnToolStripMenuItem",
            "showAuthorNameColumnToolStripMenuItem",
            "showDateColumnToolStripMenuItem",
            "showIdColumnToolStripMenuItem",
            "|",
            "SortingToolStripMenuItem",
            "AuthorDateSort",
            "TopoOrder",
            "|",
            "Settings_persistenceToolStripMenuItem",
            "SaveAsDefault",
            "|",
            "toolbarsMenuItem");

        foreach (string captionTag in new[]
        {
            "BranchesToolStripMenuItem",
            "CommitsToolStripMenuItem",
            "Grid_labelsToolStripMenuItem",
            "Grid_infoToolStripMenuItem",
            "ColumnsToolStripMenuItem",
            "SortingToolStripMenuItem",
            "Settings_persistenceToolStripMenuItem",
        })
        {
            MenuItem caption = GetTaggedMenuItem(view, captionTag);
            caption.IsEnabled.Should().BeFalse();
            caption.IsHitTestVisible.Should().BeFalse();
            caption.Classes.Should().Contain("gitextensions-menu-caption");
        }

        MenuItem toolbars = GetTaggedMenuItem(view, "toolbarsMenuItem");
        toolbars.Items.OfType<MenuItem>().Select(item => item.Header).Should().Equal("Standard", "Filters", "Scripts");
        MenuItem[] toolbarItems = [.. toolbars.Items.OfType<MenuItem>()];
        toolbarItems[0].Items.Should().HaveCount(25);
        toolbarItems[1].Items.Should().HaveCount(7);
        toolbarItems[2].Items.Should().BeEmpty();
        MenuItem scripts = toolbars.Items.OfType<MenuItem>().Last();
        StackPanel scriptsToolbar = form.FindControl<StackPanel>("ToolStripScripts")!;
        scriptsToolbar.IsVisible.Should().BeTrue();
        scripts.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        scriptsToolbar.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void FormBrowse_navigate_and_view_menus_should_preserve_translation_ownership()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(FormBrowse),
                "navigateToolStripMenuItem",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("&Navigate translated");
        translation.TranslateItem(
                "RevisionGrid",
                "ShowRemoteBranches",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("Translated remote &branches");

        form.RevisionGrid.AddTranslationItems(translation);
        form.AddTranslationItems(translation);
        form.RevisionGrid.TranslateItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "navigateToolStripMenuItem", "Text", "&Navigate");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "viewToolStripMenuItem", "Text", "&View");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "toolbarsMenuItem", "Text", "Toolbars");
        translation.Received(1).AddTranslationItem(
            "RevisionGrid", "BranchesToolStripMenuItem", "Text", "Branches");
        translation.Received(1).AddTranslationItem(
            "RevisionGrid", "ShowRemoteBranches", "Text", "Show remote &branches");
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormBrowse), "ShowRemoteBranches", "Text", Arg.Any<string>());

        MenuItem navigate = GetMainMenuItem(form, "navigateToolStripMenuItem");
        MenuItem view = GetMainMenuItem(form, "viewToolStripMenuItem");
        navigate.Header.Should().Be("_Navigate translated");
        GetTaggedMenuItem(view, "ShowRemoteBranches").Header.Should().Be("Translated remote _branches");
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void FormBrowse_view_menu_should_share_revision_grid_command_state_and_routing()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(true);
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        try
        {
            AppSettings.ShowRemoteBranches = false;
            using FormBrowse form = new(commands);
            MenuItem view = GetMainMenuItem(form, "viewToolStripMenuItem");
            MenuItem mainRemote = GetTaggedMenuItem(view, "ShowRemoteBranches");
            MenuItem contextRemote = GetTaggedMenuItem(form.RevisionGrid.ViewMenuItem, "ShowRemoteBranches");

            view.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            mainRemote.IsChecked.Should().BeFalse();
            contextRemote.IsChecked.Should().BeFalse();
            mainRemote.InputGesture.Should().Be(new KeyGesture(Key.R, KeyModifiers.Control | KeyModifiers.Shift));

            mainRemote.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.ShowRemoteBranches.Should().BeTrue();
            mainRemote.IsChecked.Should().BeTrue();
            contextRemote.IsChecked.Should().BeTrue();

            AppSettings.ShowRemoteBranches = false;
            view.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            mainRemote.IsChecked.Should().BeFalse();
            contextRemote.IsChecked.Should().BeFalse();
        }
        finally
        {
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_navigate_menu_should_route_through_the_revision_grid_selection()
    {
        bool originalShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "dirty");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(notifier);
            commands.GetService(Arg.Any<Type>())
                .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
            using FormBrowse form = new(commands);
            form.Show();
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.IsArtificial == true);

            MenuItem navigate = GetMainMenuItem(form, "navigateToolStripMenuItem");
            MenuItem goToCurrent = GetTaggedMenuItem(navigate, "GotoCurrentRevision");
            MenuItem toggleArtificial = GetTaggedMenuItem(navigate, "ToggleBetweenArtificialAndHeadCommits");
            navigate.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            goToCurrent.IsEnabled.Should().BeTrue();

            goToCurrent.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            form.RevisionGrid.SelectedRevision!.ObjectId.Should().Be(module.GetCurrentCheckout());

            navigate.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            toggleArtificial.IsEnabled.Should().BeTrue();
            toggleArtificial.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            form.RevisionGrid.SelectedRevision!.IsArtificial.Should().BeTrue();
        }
        finally
        {
            AppSettings.RevisionGraphShowArtificialCommits = originalShowArtificialCommits;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_commands_menu_should_match_the_current_supported_WinForms_inventory()
    {
        using FormBrowse form = new();

        string[] actualItems = form.commandsToolStripMenuItem.Items
            .Select(item => item switch
            {
                Separator => "|",
                MenuItem menuItem => menuItem.Name ?? throw new InvalidOperationException("A Commands menu item has no name."),
                _ => throw new InvalidOperationException($"Unexpected Commands menu entry: {item?.GetType().Name}"),
            })
            .ToArray();
        actualItems.Should().Equal(
            "commitToolStripMenuItem",
            "undoLastCommitToolStripMenuItem",
            "pullToolStripMenuItem",
            "pushToolStripMenuItem",
            "|",
            "stashToolStripMenuItem",
            "resetToolStripMenuItem",
            "cleanupToolStripMenuItem",
            "|",
            "branchToolStripMenuItem",
            "deleteBranchToolStripMenuItem",
            "checkoutBranchToolStripMenuItem",
            "mergeBranchToolStripMenuItem",
            "rebaseToolStripMenuItem",
            "runMergetoolToolStripMenuItem",
            "|",
            "tagToolStripMenuItem",
            "deleteTagToolStripMenuItem",
            "|",
            "cherryPickToolStripMenuItem",
            "archiveToolStripMenuItem",
            "checkoutToolStripMenuItem",
            "bisectToolStripMenuItem",
            "toolStripMenuItemReflog",
            "|",
            "formatPatchToolStripMenuItem",
            "applyPatchToolStripMenuItem",
            "patchToolStripMenuItem");

        MenuFlyout pullFlyout = (MenuFlyout)form.toolStripButtonPull.Flyout!;
        pullFlyout.Items.Should().Contain(form.fetchAllToolStripMenuItem);
    }

    [AvaloniaTest]
    public void FormBrowse_pull_toolbar_shortcuts_should_preserve_the_original_action_order()
    {
        using FormBrowse form = new();

        string[] shortcutNames = form.ToolStripMain.Children
            .OfType<IconButton>()
            .Where(button => button.Name?.StartsWith(FormBrowse.FetchPullToolbarShortcutsPrefix, StringComparison.Ordinal) is true)
            .Select(button => button.Name!)
            .ToArray();

        shortcutNames.Should().Equal(
            "pull_shortcut_fetchToolStripMenuItem",
            "pull_shortcut_fetchAllToolStripMenuItem",
            "pull_shortcut_fetchPruneAllToolStripMenuItem",
            "pull_shortcut_mergeToolStripMenuItem",
            "pull_shortcut_rebaseToolStripMenuItem1",
            "pull_shortcut_pullToolStripMenuItem1");
        MenuItem[] defaultPullItems = form.setDefaultPullButtonActionToolStripMenuItem.Items
            .OfType<MenuItem>()
            .ToArray();
        defaultPullItems.Select(item => item.Name).Should().Equal(
            "pullToolStripMenuItem1SetDefault",
            "mergeToolStripMenuItemSetDefault",
            "rebaseToolStripMenuItem1SetDefault",
            "fetchToolStripMenuItemSetDefault",
            "fetchAllToolStripMenuItemSetDefault",
            "fetchPruneAllToolStripMenuItemSetDefault");
        defaultPullItems.Select(item => item.Tag).Should().Equal(
            GitPullAction.None,
            GitPullAction.Merge,
            GitPullAction.Rebase,
            GitPullAction.Fetch,
            GitPullAction.FetchAll,
            GitPullAction.FetchPruneAll);
    }

    [AvaloniaTest]
    public void FormBrowse_commands_menu_should_preserve_translation_identities()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "undoLastCommitToolStripMenuItem", "Text", "&Undo last commit...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "pushToolStripMenuItem", "Text", "&Push...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "resetToolStripMenuItem", "Text", "&Reset changes...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "runMergetoolToolStripMenuItem", "Text", "&Solve merge conflicts...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "cherryPickToolStripMenuItem", "Text", "Cherr&y pick...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "cleanupToolStripMenuItem", "Text", "Clean &working directory...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "checkoutToolStripMenuItem", "Text", "Check&out revision...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "bisectToolStripMenuItem", "Text", "B&isect...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "formatPatchToolStripMenuItem", "Text", "&Format patch...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "applyPatchToolStripMenuItem", "Text", "&Apply patch...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "fetchAllToolStripMenuItem", "Text", "Fetch &all");
    }

    [AvaloniaTest]
    public async Task FormBrowse_commands_menu_should_route_functional_commands_and_update_selection_state()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        using FormBrowse form = new(commands);

        form.Show();
        TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
        await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && form.RevisionGrid.SelectedRevision is not null);
        form.commandsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

        form.commitToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.Space, KeyModifiers.Control));
        form.pullToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.Down, KeyModifiers.Control));
        form.pushToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.Up, KeyModifiers.Control));
        form.branchToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.B, KeyModifiers.Control));
        form.checkoutBranchToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.OemPeriod, KeyModifiers.Control));
        form.mergeBranchToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.M, KeyModifiers.Control));
        form.rebaseToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.E, KeyModifiers.Control | KeyModifiers.Shift));
        form.tagToolStripMenuItem.InputGesture.Should().Be(new KeyGesture(Key.T, KeyModifiers.Control));

        new[]
        {
            form.commitToolStripMenuItem,
            form.undoLastCommitToolStripMenuItem,
            form.pushToolStripMenuItem,
            form.resetToolStripMenuItem,
            form.cleanupToolStripMenuItem,
            form.branchToolStripMenuItem,
            form.deleteBranchToolStripMenuItem,
            form.checkoutBranchToolStripMenuItem,
            form.mergeBranchToolStripMenuItem,
            form.rebaseToolStripMenuItem,
            form.runMergetoolToolStripMenuItem,
            form.tagToolStripMenuItem,
            form.deleteTagToolStripMenuItem,
            form.cherryPickToolStripMenuItem,
            form.archiveToolStripMenuItem,
            form.checkoutToolStripMenuItem,
            form.bisectToolStripMenuItem,
            form.toolStripMenuItemReflog,
            form.formatPatchToolStripMenuItem,
            form.applyPatchToolStripMenuItem,
        }.Should().OnlyContain(item => item.IsEnabled);

        form.pushToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.resetToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.runMergetoolToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.cherryPickToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.cleanupToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.checkoutToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.formatPatchToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.applyPatchToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartPushDialog(form, pushOnShow: false);
        commands.Received(1).StartResetChangesDialog(
            form,
            Arg.Any<IReadOnlyCollection<GitItemStatus>>(),
            onlyWorkTree: false);
        commands.Received(1).StartResolveConflictsDialog(form, offerCommit: true);
        commands.Received(1).StartCherryPickDialog(
            form,
            Arg.Is<IEnumerable<GitRevision>>(revisions => revisions.Single().ObjectId == module.GetCurrentCheckout()));
        commands.Received(1).StartCleanupRepositoryDialog(form);
        commands.Received(1).StartCheckoutRevisionDialog(form);
        commands.Received(1).StartFormatPatchDialog(form);
        commands.Received(1).StartApplyPatchDialog(form, patchFile: null);
    }

    [AvaloniaTest]
    public void FormBrowse_commands_menu_should_follow_invalid_and_bare_repository_state()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        using (FormBrowse invalidForm = new(commands))
        {
            invalidForm.repositoryToolStripMenuItem.IsVisible.Should().BeFalse();
            invalidForm.commandsToolStripMenuItem.IsVisible.Should().BeFalse();
        }

        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
        using FormBrowse bareForm = new(commands);
        bareForm.repositoryToolStripMenuItem.IsVisible.Should().BeTrue();
        bareForm.commandsToolStripMenuItem.IsVisible.Should().BeTrue();
        bareForm.commandsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

        new[]
        {
            bareForm.commitToolStripMenuItem,
            bareForm.undoLastCommitToolStripMenuItem,
            bareForm.stashToolStripMenuItem,
            bareForm.resetToolStripMenuItem,
            bareForm.cleanupToolStripMenuItem,
            bareForm.branchToolStripMenuItem,
            bareForm.deleteBranchToolStripMenuItem,
            bareForm.checkoutBranchToolStripMenuItem,
            bareForm.mergeBranchToolStripMenuItem,
            bareForm.rebaseToolStripMenuItem,
            bareForm.runMergetoolToolStripMenuItem,
            bareForm.cherryPickToolStripMenuItem,
            bareForm.checkoutToolStripMenuItem,
            bareForm.bisectToolStripMenuItem,
            bareForm.toolStripMenuItemReflog,
            bareForm.applyPatchToolStripMenuItem,
        }.Should().OnlyContain(item => !item.IsEnabled);

        bareForm.pullToolStripMenuItem.IsEnabled.Should().BeTrue();
        bareForm.pushToolStripMenuItem.IsEnabled.Should().BeTrue();
        bareForm.patchToolStripMenuItem.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_undo_last_commit_should_preserve_changes_in_the_index()
    {
        bool originalDontConfirm = AppSettings.DontConfirmUndoLastCommit.Value;
        FormBrowse? form = null;
        try
        {
            AppSettings.DontConfirmUndoLastCommit.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            ObjectId initialCommit = module.GetCurrentCheckout();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");
            bool reloadStarted = false;
            loadingStatus.PropertyChanged += (_, e) =>
                reloadStarted |= e.Property == TextBlock.TextProperty && loadingStatus.Text == "Loading";

            form.undoLastCommitToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            await WaitUntilAsync(() => reloadStarted && loadingStatus.Text == "1 revisions");
            module.GetCurrentCheckout().Should().Be(initialCommit);
            module.GitExecutable.GetOutput(new GitArgumentBuilder("diff") { "--cached", "--name-only" })
                .Should().Contain("tracked.txt");
        }
        finally
        {
            form?.Close();
            AppSettings.DontConfirmUndoLastCommit.Value = originalDontConfirm;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_repository_host_menu_should_preserve_translation_identities()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_repositoryHostsToolStripMenuItem",
            "Text",
            "(Repository hosts)");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_forkCloneRepositoryToolStripMenuItem",
            "Text",
            "&Fork/Clone repository...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_viewPullRequestsToolStripMenuItem",
            "Text",
            "View &pull requests...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_createPullRequestsToolStripMenuItem",
            "Text",
            "&Create pull requests...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_addUpstreamRemoteToolStripMenuItem",
            "Text",
            "&Add upstream remote");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_noReposHostPluginLoaded",
            "Text",
            "No repository host plugin loaded.");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_noReposHostFound",
            "Text",
            "Could not find any relevant repository hosts for the currently open repository.");
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FormBrowse_repository_host_menu_should_route_all_functional_contributions()
    {
        IRepositoryHostPlugin[] originalHosts = [.. PluginRegistry.GitHosters];
        GitModule module = CreateRepositoryWithInitialCommit();
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.Name.Returns("TestHost");
        host.GitModuleIsRelevantToMe().Returns(true);
        PluginRegistry.GitHosters.Clear();
        PluginRegistry.GitHosters.Add(host);
        FormBrowse? form = null;

        try
        {
            form = new FormBrowse(commands);
            form.Show();
            form.UpdateRepositoryHostsMenuForTest();
            MenuItem hostMenu = form.FindControl<MenuItem>("_repositoryHostsToolStripMenuItem")!;
            MenuItem forkClone = form.FindControl<MenuItem>("_forkCloneRepositoryToolStripMenuItem")!;
            MenuItem viewPullRequests = form.FindControl<MenuItem>("_viewPullRequestsToolStripMenuItem")!;
            MenuItem createPullRequest = form.FindControl<MenuItem>("_createPullRequestsToolStripMenuItem")!;
            MenuItem addUpstream = form.FindControl<MenuItem>("_addUpstreamRemoteToolStripMenuItem")!;

            hostMenu.IsVisible.Should().BeTrue();
            hostMenu.Header.Should().Be("TestHost");
            forkClone.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            viewPullRequests.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createPullRequest.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            addUpstream.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).StartCloneForkFromHoster(
                form,
                host,
                Arg.Any<EventHandler<GitModuleEventArgs>>());
            commands.Received(1).StartPullRequestsDialog(form, host);
            commands.Received(1).StartCreatePullRequest(form, host);
            commands.Received(1).AddUpstreamRemote(form, host);
        }
        finally
        {
            form?.Close();
            PluginRegistry.GitHosters.Clear();
            PluginRegistry.GitHosters.AddRange(originalHosts);
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_quick_revision_filter_should_reload_the_revision_grid()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

        FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");

            FilterToolBar filters = form.FindControl<FilterToolBar>("ToolStripFilters")!;
            ComboBox revisionFilter = filters.FindControl<ComboBox>("tstxtRevisionFilter")!;
            revisionFilter.Text = "initial";
            revisionFilter.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyUpEvent,
                Key = Key.Enter,
            });

            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");
            revisionGrid.SelectedRevision!.Subject.Should().Be("initial");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_apply_command_line_browse_arguments_before_loading()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        ObjectId selectedId = module.GetCurrentCheckout();
        File.WriteAllText(Path.Combine(_workingDirectory, "other.txt"), "other");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "other.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial other".Quote() });
        BrowseArguments args = new()
        {
            RevFilter = "initial",
            PathFilter = "tracked.txt",
            SelectedId = selectedId,
            IsFileHistoryMode = true,
        };

        FormBrowse form = new(new GitUICommands(_serviceContainer, module), args);
        try
        {
            form.Show();
            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && form.RevisionGrid.SelectedRevision is not null);

            form.RevisionGrid.SelectedId.Should().Be(selectedId);
            form.FindControl<FilterToolBar>("ToolStripFilters")!
                .FindControl<ComboBox>("tstxtRevisionFilter")!.Text.Should().Be("initial");
            form.FindControl<Control>("leftPanel")!.IsVisible.Should().BeFalse();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_commit_and_diff_tabs_should_follow_the_selected_revision()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "\nsecond");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "2 revisions"
                    && form.RevisionInfo.Revision?.Subject == "second");

                form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.CommitInfoTabPage);
                form.fileStatusList.GitItemStatuses.Should().BeEmpty();
                form.fileStatusList.FindControl<MenuItem>("tsmiToolbar")!.Items.Count.Should().Be(18,
                    "WinForms builds the hidden Diff toolbar menu when FileStatusList binds");
                form.revisionDiff.FileViewer.TextEditor.Text.Should().BeEmpty(
                    "the source does not load the hidden Diff tab while Commit is selected");

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() => form.fileStatusList.GitItemStatuses.Count == 1
                    && form.fileViewer.TextEditor.Text.Contains("+second", StringComparison.Ordinal));
                form.fileStatusList.Bounds.Height.Should().BeGreaterThan(0);
                form.fileStatusList.SelectedItem!.Item.Name.Should().Be("tracked.txt");
                form.fileViewer.TextEditor.Text.Should().Contain("+second");
                form.revisionDiff.FileViewer.TextEditor.TextArea.TextView.ScrollOffset.Y.Should().Be(0,
                    "a newly opened Diff tab should show the patch header before its changed lines");
                form.revisionDiff.FileViewer.TextEditor.Options.AllowScrollBelowDocument.Should().BeFalse(
                    "the source editor hides its vertical scrollbar when the short patch fits");
                Border toolbar = form.revisionDiff.FileViewer.FindControl<Border>("fileviewerToolbar")!;
                ComboBox encoding = form.revisionDiff.FileViewer.FindControl<ComboBox>("encodingToolStripComboBox")!;
                toolbar.IsVisible.Should().BeFalse();
                using (AvaloniaControlStateDriver.Apply(form, new CaptureStatePlan
                       {
                           Id = "diff-text.focused",
                           Kind = CaptureStateKind.Focus,
                           TargetField = "DiffText",
                       }))
                {
                    form.revisionDiff.FileViewer.TextEditor.TextArea.IsFocused.Should().BeTrue();
                    toolbar.IsVisible.Should().BeFalse("keyboard focus does not reveal a pointer-hover toolbar");
                    encoding.SelectedItem.Should().BeNull("the hidden source encoding selector remains unselected");
                }
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
    [Category("P8.6i.126")]
    public void Browse_revision_pane_should_paint_the_native_split_container_border()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module))
        {
            Width = 923,
            Height = 573,
            RequestedThemeVariant = ThemeVariant.Light,
        };
        form.Show();
        Dispatcher.UIThread.RunJobs();

        Grid revisions = form.FindControl<Grid>("RevisionsSplitContainer")!;
        Point origin = revisions.TranslatePoint(default, form)!.Value;
        using WriteableBitmap frame = form.CaptureRenderedFrame()
            ?? throw new AssertionException("Browse revision pane did not render.");
        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        using SKBitmap bitmap = SKBitmap.Decode(stream);
        SKColor border = new(224, 224, 224);
        int left = (int)origin.X;
        int top = (int)origin.Y;
        int right = left + (int)revisions.Bounds.Width - 1;
        int bottom = top + (int)revisions.Bounds.Height - 1;
        int middleX = (left + right) / 2;
        int middleY = (top + bottom) / 2;
        bitmap.GetPixel(middleX, top).Should().Be(border, "the native splitter paints its top border");
        bitmap.GetPixel(middleX, bottom).Should().Be(border, "the native splitter paints its bottom border");
        bitmap.GetPixel(left, middleY).Should().Be(border, "the native splitter paints its left border");
        bitmap.GetPixel(right, middleY).Should().Be(border, "the native splitter paints its right border");

        Border pageHost = form.FindControl<Border>("commitInfoBelowHost")!;
        Point pageOrigin = pageHost.TranslatePoint(default, form)!.Value;
        Border pageFrame = form.CommitInfoTabControl.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Classes.Contains("gitextensions-workspace-page-frame"));
        GetColor(pageFrame.BorderBrush).Should().Be(Color.Parse("#FFE5E5E5"));
        pageFrame.BorderThickness.Should().Be(new Thickness(1, 0, 0, 0));
        pageHost.Child!.Bounds.Width.Should().Be(643,
            "the native TabPage retains a one-pixel side inset without an extra painted border");
        bitmap.GetPixel((int)pageOrigin.X + 10, (int)pageOrigin.Y).Should().Be(new SKColor(255, 255, 255),
            "WinForms places CommitInfo directly inside the TabPage display rectangle");

        CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, new PixelSize(bitmap.Width, bitmap.Height)).Root;
        CaptureNode splitNode = Flatten(root).Single(node => node.FieldName == "RevisionsSplitContainer");
        splitNode.Colors.Background.Should().Be("#00FFFFFF",
            "the native frame does not change the split container's transparent BackColor");
        splitNode.Children.Should().NotContain(node => node.ControlKind == "control" && node.FieldName == null,
            "the frame is renderer-only, not an extra source control");
        Flatten(root).Should().NotContain(node => node.Name == "PART_NativeIndicator"
            || node.Name == "gitextensions-workspace-page-frame"
            || node.Name == "gitextensions-toolbar-separator-line",
            "native paint helpers are not independent controls in the source tree");

        static IEnumerable<CaptureNode> Flatten(CaptureNode node)
        {
            yield return node;
            foreach (CaptureNode child in node.Children)
            {
                foreach (CaptureNode descendant in Flatten(child))
                {
                    yield return descendant;
                }
            }
        }

        static Color GetColor(IBrush? brush)
            => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [NonParallelizable]
    public async Task Browse_layout_toolbar_buttons_should_reflect_the_visible_panels()
    {
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module))
            {
                Width = 923,
                Height = 573,
            };
            form.Show();
            Button leftToggle = form.FindControl<Button>("toggleLeftPanel")!;
            Button splitToggle = form.FindControl<Button>("toggleSplitViewLayout")!;
            Control leftPanel = form.FindControl<Control>("leftPanel")!;

            leftPanel.IsVisible.Should().BeTrue();
            leftToggle.IsVisible.Should().BeTrue();
            leftToggle.Bounds.Width.Should().BeGreaterThan(0);
            leftToggle.Classes.Should().Contain("checked");
            splitToggle.Classes.Should().Contain("checked");

            Grid rightSplit = form.FindControl<Grid>("RightSplitContainer")!;
            rightSplit.RowDefinitions[0].ActualHeight.Should().Be(211);
            form.Height = 866;
            Dispatcher.UIThread.RunJobs();
            rightSplit.RowDefinitions[0].ActualHeight.Should().Be(333,
                "the unfixed WinForms pane scales its Designer splitter distance with the window");

            Click(form, leftToggle, MouseButton.Left);
            leftPanel.IsVisible.Should().BeFalse();
            leftToggle.Classes.Should().NotContain("checked");

            module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
            Click(form, leftToggle, MouseButton.Left);
            leftPanel.IsVisible.Should().BeTrue();
            leftToggle.Classes.Should().Contain("checked");
            RepoObjectsTree tree = form.FindControl<RepoObjectsTree>("repoObjectsTree")!;
            await WaitUntilAsync(() => tree.GetTestAccessor().Tree.Items
                .Cast<TreeViewItem>()
                .Where(item => HeaderText(item).StartsWith("Branches", StringComparison.Ordinal))
                .SelectMany(item => item.Items.Cast<TreeViewItem>())
                .Any(item => HeaderText(item).Contains("feature", StringComparison.Ordinal)));

            Click(form, splitToggle, MouseButton.Left);
            AppSettings.ShowSplitViewLayout.Should().BeFalse();
            splitToggle.Classes.Should().NotContain("checked");
            form.Height = 946;
            Dispatcher.UIThread.RunJobs();
            rightSplit.RowDefinitions[0].ActualHeight.Should().Be(rightSplit.Bounds.Height,
                "hiding the lower pane must let the revision graph fill the available height");

            Click(form, splitToggle, MouseButton.Left);
            AppSettings.ShowSplitViewLayout.Should().BeTrue();
            splitToggle.Classes.Should().Contain("checked");
            rightSplit.RowDefinitions[0].ActualHeight.Should().Be(
                Math.Floor(209 * rightSplit.Bounds.Height / 502),
                "reopening the split view restores the source's proportional sizing after a resize");
        }
        finally
        {
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public async Task Browse_drop_should_select_an_existing_repository_path_and_reject_siblings()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        string trackedFile = Path.Combine(_workingDirectory, "tracked.txt");
        string siblingDirectory = _workingDirectory + "-sibling";
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        Directory.CreateDirectory(siblingDirectory);
        try
        {
            File.WriteAllText(Path.Combine(siblingDirectory, "tracked.txt"), "not in the repository");
            FormBrowse.GetRelativePathExistingInRepo(trackedFile, module.WorkingDir)
                .Should().Be(RelativePath.From("tracked.txt"));
            FormBrowse.GetRelativePathExistingInRepo(Path.Combine(siblingDirectory, "tracked.txt"), module.WorkingDir)
                .Should().BeNull();
            FormBrowse.GetRelativePathExistingInRepo(module.WorkingDir, module.WorkingDir)
                .Should().BeNull();

            form.Show();
            Control leftPanel = form.FindControl<Control>("leftPanel")!;
            DragDrop.GetAllowDrop(leftPanel).Should().BeTrue();
            using DataTransfer data = new();
            data.Add(DataTransferItem.CreateText(trackedFile));
            DragEventArgs over = new(DragDrop.DragOverEvent, data, leftPanel, new Avalonia.Point(10, 10), KeyModifiers.None);
            leftPanel.RaiseEvent(over);
            over.DragEffects.Should().Be(DragDropEffects.Move);
            DragEventArgs drop = new(DragDrop.DropEvent, data, leftPanel, new Avalonia.Point(10, 10), KeyModifiers.None);
            leftPanel.RaiseEvent(drop);
            form.FindControl<TabControl>("CommitInfoTabControl")!.SelectedItem
                .Should().BeSameAs(form.FindControl<TabItem>("TreeTabPage"));
            await form.JoinLoadOperationsForTestAsync();
            await WaitUntilAsync(() => form.fileTree.FileStatusList.SelectedRelativePath == RelativePath.From("tracked.txt"));

            TabControl tabs = form.FindControl<TabControl>("CommitInfoTabControl")!;
            tabs.SelectedItem = form.FindControl<TabItem>("CommitInfoTabPage");
            IStorageFile storageFile = Substitute.For<IStorageFile>();
            storageFile.Path.Returns(new Uri(trackedFile));
            using DataTransfer files = new();
            files.Add(DataTransferItem.CreateFile(storageFile));
            form.RaiseEvent(new DragEventArgs(DragDrop.DropEvent, files, form, new Avalonia.Point(10, 10), KeyModifiers.None));
            tabs.SelectedItem.Should().BeSameAs(form.FindControl<TabItem>("TreeTabPage"));

            IStorageFile patchFile = Substitute.For<IStorageFile>();
            patchFile.Path.Returns(new Uri(Path.Combine(_workingDirectory, "sample.patch")));
            using DataTransfer patch = new();
            patch.Add(DataTransferItem.CreateFile(patchFile));
            ListBox revisions = form.RevisionGrid.FindControl<ListBox>("_gridView")!;
            DragEventArgs patchOver = new(DragDrop.DragOverEvent, patch, revisions, new Avalonia.Point(10, 10), KeyModifiers.None);
            revisions.RaiseEvent(patchOver);
            patchOver.DragEffects.Should().Be(DragDropEffects.Copy,
                "the revision grid retains its original patch-drop behavior");
        }
        finally
        {
            form.Close();
            TestDirectory.Delete(siblingDirectory);
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_toolbar_should_hide_a_partially_clipped_stash_command()
    {
        using FormBrowse form = new() { Width = 923, Height = 573 };
        form.Show();
        try
        {
            WorkingDirectoryToolStripSplitButton selector = form.FindControl<WorkingDirectoryToolStripSplitButton>(
                "_NO_TRANSLATE_WorkingDir")!;
            string caption = "~\\AppData\\Local\\Temp\\GitExtensions.MainScreenParity.MenuFocus";
            selector.Content = caption;
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            ScrollViewer viewport = form.toolStripMainViewport;
            IconSplitButton stash = form.FindControl<IconSplitButton>("toolStripSplitStash")!;
            Point position = stash.TranslatePoint(default, viewport)!.Value;
            while (position.X >= viewport.Viewport.Width && caption.Length > 1)
            {
                // Platform font metrics can cap the repository selector at a slightly
                // different width; find the first genuinely clipped Stash position.
                caption = caption[..^1];
                selector.Content = caption;
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                position = stash.TranslatePoint(default, viewport)!.Value;
            }

            position.X.Should().BeLessThan(viewport.Viewport.Width);
            (position.X + stash.Bounds.Width).Should().BeGreaterThan(viewport.Viewport.Width);
            stash.Opacity.Should().Be(0, "ToolStrip sends the entire partly clipped Stash command to overflow");
            stash.IsHitTestVisible.Should().BeFalse();

            form.toolStripMainOverflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            stash.Opacity.Should().Be(1, "the complete command becomes available on the next overflow page");
            stash.IsHitTestVisible.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FormBrowse_start_menu_should_fit_its_repository_command_caption()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        try
        {
            form.fileToolStripMenuItem.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            MenuItem[] commands = [.. form.fileToolStripMenuItem.Items.OfType<MenuItem>()
                .Where(item => item.Classes.Contains("gitextensions-menu-no-gesture"))];
            commands.Should().HaveCount(4);
            foreach (MenuItem command in commands)
            {
                Avalonia.Controls.Presenters.ContentPresenter presenter = command.GetVisualDescendants()
                    .OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                    .Single(control => control.Name == "PART_HeaderPresenter");
                Avalonia.Controls.Primitives.AccessText header = command.GetVisualDescendants()
                    .OfType<Avalonia.Controls.Primitives.AccessText>().Single();
                header.Measure(Size.Infinity);
                presenter.Bounds.Width.Should().BeGreaterThanOrEqualTo(header.DesiredSize.Width,
                    $"the {command.Name} caption must fit the source-width Start menu without clipping");
            }
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_file_tree_should_load_lazily_and_follow_the_path_filter()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            Directory.CreateDirectory(Path.Combine(_workingDirectory, "src"));
            Directory.CreateDirectory(Path.Combine(_workingDirectory, "docs"));
            File.WriteAllText(Path.Combine(_workingDirectory, "src", "followed.txt"), "followed file");
            File.WriteAllText(Path.Combine(_workingDirectory, "docs", "other.txt"), "other file");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "src/followed.txt", "docs/other.txt" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "add tree".Quote() });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");

                form.fileTree.DisplayedRevision.Should().BeNull("the hidden tab must not enumerate the repository tree");
                form.fileTree.FileStatusList.GitItemStatuses.Should().BeEmpty();

                form.RevisionGrid.SetAndApplyPathFilter("\"src/followed.txt\"");
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "1 revisions"
                    && form.RevisionGrid.SelectedRevision?.Subject == "add tree");

                form.CommitInfoTabControl.SelectedItem = form.TreeTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() =>
                    form.fileTree.DisplayedRevision?.Subject == "add tree"
                    && form.fileTree.FileStatusList.GitItemStatuses.Count == 3
                    && form.fileTree.FileStatusList.SelectedItem?.Item.Name == "src/followed.txt"
                    && form.fileTree.FileViewer.TextEditor.Text.Contains("followed file", StringComparison.Ordinal));

                TreeView tree = form.fileTree.FileStatusList.FindControl<TreeView>("tvFiles")!;
                ListBox list = form.fileTree.FileStatusList.FindControl<ListBox>("lstFiles")!;
                tree.IsVisible.Should().BeTrue();
                list.IsVisible.Should().BeFalse();
                form.fileTree.FileStatusList.SelectedRelativePath.Should().Be(RelativePath.From("src/followed.txt"));
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
    public async Task FormBrowse_diff_should_toggle_blame_in_the_existing_viewer()
    {
        bool originalUseDiffViewerForBlame = AppSettings.UseDiffViewerForBlame.Value;
        try
        {
            AppSettings.UseDiffViewerForBlame.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "\nsecond");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "2 revisions");

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() =>
                    form.fileStatusList.SelectedItem?.Item.Name == "tracked.txt"
                    && form.fileViewer.TextEditor.Text.Contains("+second", StringComparison.Ordinal));
                MenuItem blameMenu = form.fileStatusList.FindControl<MenuItem>("tsmiBlame")!;
                BlameControl blame = form.revisionDiff.FindControl<BlameControl>("BlameControl")!;

                blameMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                await WaitUntilAsync(() =>
                    blame.IsVisible
                    && blame.BlameFile.TextEditor.Text.Contains("second", StringComparison.Ordinal));
                blameMenu.IsChecked.Should().BeTrue();
                form.fileViewer.IsVisible.Should().BeFalse();

                blameMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                await WaitUntilAsync(() =>
                    form.fileViewer.IsVisible
                    && form.fileViewer.TextEditor.Text.Contains("+second", StringComparison.Ordinal));
                blame.IsVisible.Should().BeFalse();
                blameMenu.IsChecked.Should().BeFalse();
            }
            finally
            {
                Stopwatch closeStopwatch = Stopwatch.StartNew();
                form.Close();
                closeStopwatch.Stop();
                closeStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5),
                    "switching away from Blame must not leave an owner task waiting on the unpumped Avalonia dispatcher");
            }
        }
        finally
        {
            AppSettings.UseDiffViewerForBlame.Value = originalUseDiffViewerForBlame;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_diff_should_open_file_tree_in_blame_mode_when_configured()
    {
        bool originalUseDiffViewerForBlame = AppSettings.UseDiffViewerForBlame.Value;
        try
        {
            AppSettings.UseDiffViewerForBlame.Value = false;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "\nsecond");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "2 revisions");

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() =>
                    form.fileStatusList.SelectedItem?.Item.Name == "tracked.txt");
                form.fileStatusList.FindControl<MenuItem>("tsmiBlame")!
                    .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

                BlameControl blame = form.fileTree.FindControl<BlameControl>("BlameControl")!;
                await WaitUntilAsync(() =>
                    ReferenceEquals(form.CommitInfoTabControl.SelectedItem, form.TreeTabPage)
                    && form.fileTree.FileStatusList.SelectedItem?.Item.Name == "tracked.txt"
                    && blame.IsVisible
                    && blame.BlameFile.TextEditor.Text.Contains("second", StringComparison.Ordinal));
                form.fileTree.FileStatusList.FindControl<MenuItem>("tsmiBlame")!.IsChecked.Should().BeTrue();
                form.fileTree.FileViewer.IsVisible.Should().BeFalse();
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.UseDiffViewerForBlame.Value = originalUseDiffViewerForBlame;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public async Task FormBrowse_gpg_tab_should_load_lazily_and_ignore_stale_results()
    {
        bool originalShowGpgInformation = AppSettings.ShowGpgInformation.Value;
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.ShowGpgInformation.Value = true;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.WriteAllText(Path.Combine(_workingDirectory, "second.txt"), "second commit");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "second.txt" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "second commit".Quote() });

            ConcurrentDictionary<ObjectId, TaskCompletionSource<GpgInfo?>> completions = [];
            IGpgInfoProvider provider = Substitute.For<IGpgInfoProvider>();
            provider.LoadGpgInfoAsync(Arg.Any<GitRevision?>()).Returns(callInfo =>
            {
                GitRevision revision = callInfo.Arg<GitRevision>();
                TaskCompletionSource<GpgInfo?> completion = new();
                completions[revision.ObjectId] = completion;
                return completion.Task;
            });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module), provider);
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");

                GitRevision headRevision = form.RevisionGrid.SelectedRevision!;
                ObjectId parentId = headRevision.FirstParentId;
                await WaitUntilAsync(() => form.GpgInfoTabPage.IsVisible);
                form.GpgInfoTabPage.IsVisible.Should().BeTrue();
                form.revisionGpgInfo1.Margin.Should().Be(new Thickness(1, 0, 1, 1));
                _ = provider.DidNotReceive().LoadGpgInfoAsync(Arg.Any<GitRevision?>());

                form.CommitInfoTabControl.SelectedItem = form.GpgInfoTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() => completions.ContainsKey(headRevision.ObjectId));
                form.revisionGpgInfo1.IsKeyboardFocusWithin.Should().BeTrue();

                form.RevisionGrid.SetSelectedRevision(parentId).Should().BeTrue();
                await WaitUntilAsync(() => completions.ContainsKey(parentId));
                completions[parentId].SetResult(new GpgInfo(
                    CommitStatus.MissingPublicKey,
                    "current revision signature",
                    TagStatus.TagNotSigned,
                    TagVerificationMessage: null));

                TextBox commitInfo = form.revisionGpgInfo1.FindControl<TextBox>("txtCommitGpgInfo")!;
                TextBox tagInfo = form.revisionGpgInfo1.FindControl<TextBox>("txtTagGpgInfo")!;
                Image commitPicture = form.revisionGpgInfo1.FindControl<Image>("commitSignPicture")!;
                Image tagPicture = form.revisionGpgInfo1.FindControl<Image>("tagSignPicture")!;
                await WaitUntilAsync(() => commitInfo.Text == "current revision signature");
                tagInfo.Text.Should().Be("Tag is not signed");
                tagInfo.IsVisible.Should().BeTrue();
                commitPicture.Source.Should().BeSameAs(GitUI.Properties.Images.CommitSignatureWarning);
                tagPicture.IsVisible.Should().BeFalse();

                completions[headRevision.ObjectId].SetResult(new GpgInfo(
                    CommitStatus.GoodSignature,
                    "stale revision signature",
                    TagStatus.OneGood,
                    "stale tag signature"));
                Dispatcher.UIThread.RunJobs();
                commitInfo.Text.Should().Be("current revision signature");

                form.RefreshGpgInfo(new GitRevision(ObjectId.WorkTreeId));
                form.GpgInfoTabPage.IsVisible.Should().BeFalse();
                form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.TreeTabPage);

                AppSettings.ShowGpgInformation.Value = false;
                form.RefreshGpgInfo(headRevision);
                form.GpgInfoTabPage.IsVisible.Should().BeFalse();
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowGpgInformation.Value = originalShowGpgInformation;
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_cancel_an_unfinished_gpg_load_when_closed()
    {
        bool originalShowGpgInformation = AppSettings.ShowGpgInformation.Value;
        try
        {
            AppSettings.ShowGpgInformation.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            TaskCompletionSource<GpgInfo?> unfinishedLoad = new(TaskCreationOptions.RunContinuationsAsynchronously);
            IGpgInfoProvider provider = Substitute.For<IGpgInfoProvider>();
            provider.LoadGpgInfoAsync(Arg.Any<GitRevision?>()).Returns(unfinishedLoad.Task);

            FormBrowse form = new(new GitUICommands(_serviceContainer, module), provider);
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

                form.CommitInfoTabControl.SelectedItem = form.GpgInfoTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() => provider.ReceivedCalls().Any());
                form.RefreshGpgInfo(new GitRevision(ObjectId.WorkTreeId));
                Dispatcher.UIThread.RunJobs();

                Stopwatch stopwatch = Stopwatch.StartNew();
                form.Close();
                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5),
                    "closing the browser must cancel its pending GPG wait instead of blocking for the task-manager timeout");
            }
            finally
            {
                if (form.IsVisible)
                {
                    form.Close();
                }
            }
        }
        finally
        {
            AppSettings.ShowGpgInformation.Value = originalShowGpgInformation;
        }
    }

    [AvaloniaTest]
    public async Task Revision_grid_notes_provider_should_load_and_render_git_notes()
    {
        bool originalShowNotesColumn = AppSettings.ShowGitNotesColumn.Value;
        bool originalShowGitNotes = AppSettings.ShowGitNotes;
        bool originalShowToolTips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowGitNotesColumn.Value = true;
            AppSettings.ShowGitNotes = false;
            AppSettings.ShowRevisionGridTooltips.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("notes")
            {
                "add",
                "-m",
                "First note\nSecond note".Quote(),
            }).Should().BeTrue();

            GitUICommands commands = new(_serviceContainer, module);
            FormBrowse form = new(commands);
            try
            {
                form.Show();
                RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                    ?? throw new InvalidOperationException("Revision grid was not created.");
                TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                    ?? throw new InvalidOperationException("Revision loading status was not created.");

                await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

                TextBlock notesCell = revisionGrid.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Single(textBlock => textBlock.Classes.Contains("revision-notes-cell"));
                notesCell.Text.Should().Be("First note");
                ToolTip.GetTip(notesCell).Should().Be("First note\nSecond note");
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowGitNotesColumn.Value = originalShowNotesColumn;
            AppSettings.ShowGitNotes = originalShowGitNotes;
            AppSettings.ShowRevisionGridTooltips.Value = originalShowToolTips;
        }
    }

    [AvaloniaTest]
    public async Task Revision_grid_should_highlight_the_selected_author_and_expose_lane_tooltips()
    {
        bool originalShowAuthor = AppSettings.ShowAuthorNameColumn;
        bool originalShowToolTips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowAuthorNameColumn = true;
            AppSettings.ShowRevisionGridTooltips.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            ObjectId initialCommit = module.GetCurrentCheckout();
            module.SetSetting("user.name", "Second Author");
            module.SetSetting("user.email", "second@example.com");
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId secondCommit = module.GetCurrentCheckout();

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                    ?? throw new InvalidOperationException("Revision grid was not created.");
                TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                    ?? throw new InvalidOperationException("Revision loading status was not created.");
                await WaitUntilAsync(() => loadingStatus.Text == "2 revisions" && revisionGrid.SelectedRevision is not null);

                TextBlock[] authorCells =
                [
                    .. revisionGrid.GetVisualDescendants()
                        .OfType<TextBlock>()
                        .Where(textBlock => textBlock.Classes.Contains("revision-author-cell")),
                ];
                authorCells.Single(cell => cell.Text == "Second Author").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Bold);
                authorCells.Single(cell => cell.Text == "Avalonia Test").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Normal);

                RevisionGraphColumnProvider graphProvider =
                    (RevisionGraphColumnProvider)revisionGrid.ColumnProviders[0];
                graphProvider.GetLaneToolTip(revisionGrid.SelectedRevision!, x: 1)
                    .Should().Contain(revisionGrid.SelectedRevision!.Guid);

                revisionGrid.SetSelectedRevision(initialCommit).Should().BeTrue();
                Dispatcher.UIThread.RunJobs();
                authorCells.Single(cell => cell.Text == "Avalonia Test").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Bold);
                authorCells.Single(cell => cell.Text == "Second Author").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Normal);

                IGitRef relatedRef = Substitute.For<IGitRef>();
                relatedRef.Guid.Returns(initialCommit.ToString());
                relatedRef.ObjectId.Returns(initialCommit);
                revisionGrid.SetSelectedRevision(secondCommit).Should().BeTrue();
                revisionGrid.TryGoToRelatedRef(relatedRef).Should().BeTrue();
                revisionGrid.SelectedRevision!.ObjectId.Should().Be(initialCommit);
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowAuthorNameColumn = originalShowAuthor;
            AppSettings.ShowRevisionGridTooltips.Value = originalShowToolTips;
        }
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public async Task RevisionGrid_rebase_menu_should_match_current_branch_state()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
            ?? throw new InvalidOperationException("Revision grid was not created.");
        TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
            ?? throw new InvalidOperationException("Revision loading status was not created.");
        await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && revisionGrid.SelectedRevision is not null);

        ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("mainContextMenu")
            ?? throw new InvalidOperationException("Revision context menu was not created.");
        ListBox revisions = revisionGrid.FindControl<ListBox>("_gridView")
            ?? throw new InvalidOperationException("Revision list was not created.");
        MenuItem rebaseOn = revisionGrid.FindControl<MenuItem>("rebaseOnToolStripMenuItem")
            ?? throw new InvalidOperationException("Rebase-on menu item was not created.");
        MenuItem rebase = revisionGrid.FindControl<MenuItem>("rebaseToolStripMenuItem")
            ?? throw new InvalidOperationException("Rebase menu item was not created.");
        MenuItem rebaseInteractively = revisionGrid.FindControl<MenuItem>("rebaseInteractivelyToolStripMenuItem")
            ?? throw new InvalidOperationException("Interactive-rebase menu item was not created.");
        MenuItem rebaseWithAdvancedOptions = revisionGrid.FindControl<MenuItem>("rebaseWithAdvOptionsToolStripMenuItem")
            ?? throw new InvalidOperationException("Advanced-rebase menu item was not created.");

        contextMenu.Open(revisions);
        Dispatcher.UIThread.RunJobs();
        rebaseOn.IsVisible.Should().BeTrue("WinForms keeps the Rebase on parent available for a regular revision");
        rebaseOn.IsEnabled.Should().BeTrue();

        rebaseOn.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        rebase.IsEnabled.Should().BeFalse("the selected HEAD has no other branch to rebase onto");
        rebaseInteractively.IsEnabled.Should().BeFalse();
        rebaseWithAdvancedOptions.IsEnabled.Should().BeFalse();
        rebaseOn.IsSubMenuOpen = false;
        contextMenu.Close();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.4")]
    public async Task RevisionGrid_context_menu_should_route_the_selected_revision()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { "refs/remotes/origin/main", "HEAD" });
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        FormBrowse form = new(commands);
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && revisionGrid.SelectedRevision is not null);

            ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem checkoutBranch = revisionGrid.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Checkout-branch menu item was not created.");
            MenuItem pushBranch = revisionGrid.FindControl<MenuItem>("tsmiPushBranch")
                ?? throw new InvalidOperationException("Push-branch menu item was not created.");
            MenuItem mergeBranch = revisionGrid.FindControl<MenuItem>("mergeBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Merge-branch menu item was not created.");
            MenuItem resetCurrentBranch = revisionGrid.FindControl<MenuItem>("resetCurrentBranchToHereToolStripMenuItem")
                ?? throw new InvalidOperationException("Reset-current-branch menu item was not created.");
            MenuItem createBranch = revisionGrid.FindControl<MenuItem>("createNewBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-branch menu item was not created.");
            MenuItem renameBranch = revisionGrid.FindControl<MenuItem>("renameBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Rename-branch menu item was not created.");
            MenuItem deleteBranch = revisionGrid.FindControl<MenuItem>("deleteBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Delete-branch menu item was not created.");
            MenuItem createTag = revisionGrid.FindControl<MenuItem>("createTagToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-tag menu item was not created.");
            MenuItem archiveRevision = revisionGrid.FindControl<MenuItem>("archiveRevisionToolStripMenuItem")
                ?? throw new InvalidOperationException("Archive-revision menu item was not created.");
            MenuItem cherryPick = revisionGrid.FindControl<MenuItem>("cherryPickCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Cherry-pick menu item was not created.");
            MenuItem revertCommit = revisionGrid.FindControl<MenuItem>("revertCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Revert-commit menu item was not created.");
            MenuItem archive = form.FindControl<MenuItem>("archiveToolStripMenuItem")
                ?? throw new InvalidOperationException("Archive menu item was not created.");
            CopyContextMenuItem copy = revisionGrid.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")
                ?? throw new InvalidOperationException("Copy menu item was not created.");
            MenuItem rebase = revisionGrid.FindControl<MenuItem>("rebaseToolStripMenuItem")
                ?? throw new InvalidOperationException("Rebase menu item was not created.");
            MenuItem editCommit = revisionGrid.FindControl<MenuItem>("editCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Edit-commit menu item was not created.");
            MenuItem rewordCommit = revisionGrid.FindControl<MenuItem>("rewordCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Reword-commit menu item was not created.");
            MenuItem view = revisionGrid.FindControl<MenuItem>("viewToolStripMenuItem")
                ?? throw new InvalidOperationException("View menu item was not created.");
            ListBox revisions = revisionGrid.FindControl<ListBox>("_gridView")
                ?? throw new InvalidOperationException("Revision list was not created.");

            (string Name, ThemeVariant Variant)[] themes =
            [
                ("Light", ThemeVariant.Light),
                ("Dark", ThemeVariant.Dark),
            ];
            foreach ((string themeName, ThemeVariant themeVariant) in themes)
            {
                form.RequestedThemeVariant = themeVariant;
                contextMenu.Open(revisions);
                Dispatcher.UIThread.RunJobs();

                TopLevel contextMenuRoot = TopLevel.GetTopLevel(contextMenu)
                    ?? throw new InvalidOperationException("Revision context menu did not open in a top level.");
                WriteableBitmap? contextMenuFrame = contextMenuRoot.CaptureRenderedFrame();
                contextMenuFrame.Should().NotBeNull($"the opened context menu should render in {themeName}");
                copy.Bounds.Height.Should().BeGreaterThan(0);
                deleteBranch.IsVisible.Should().BeTrue();
                deleteBranch.IsEnabled.Should().BeTrue();
                deleteBranch.Bounds.Height.Should().BeGreaterThan(0);
                view.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
                WriteableBitmap? viewMenuFrame = contextMenuRoot.CaptureRenderedFrame();
                viewMenuFrame.Should().NotBeNull($"the opened revision View menu should render in {themeName}");
                if (Environment.GetEnvironmentVariable("GITEXT_CAPTURE_REVISION_CONTEXT_MENU") == "1")
                {
                    string captureDirectory = Path.Combine(Path.GetTempPath(), "gitextensions-avalonia-revision-context");
                    Directory.CreateDirectory(captureDirectory);
                    using FileStream stream = File.Create(Path.Combine(captureDirectory, $"{themeName}.png"));
                    contextMenuFrame!.Save(stream, PngBitmapEncoderOptions.Default);
                    using FileStream viewStream = File.Create(Path.Combine(captureDirectory, $"{themeName}.View.png"));
                    viewMenuFrame!.Save(viewStream, PngBitmapEncoderOptions.Default);
                }

                view.IsSubMenuOpen = false;
                contextMenu.Close();
            }

            ObjectId selectedObjectId = revisionGrid.SelectedRevision!.ObjectId;
            checkoutBranch.IsEnabled.Should().BeTrue();
            pushBranch.IsEnabled.Should().BeTrue();
            mergeBranch.IsEnabled.Should().BeTrue();
            resetCurrentBranch.IsVisible.Should().BeTrue();
            createBranch.IsEnabled.Should().BeTrue();
            renameBranch.IsEnabled.Should().BeTrue();
            deleteBranch.IsEnabled.Should().BeTrue();
            createTag.IsEnabled.Should().BeTrue();
            archiveRevision.IsVisible.Should().BeTrue();
            cherryPick.IsVisible.Should().BeTrue();
            revertCommit.IsVisible.Should().BeTrue();
            archive.IsEnabled.Should().BeTrue();
            editCommit.IsEnabled.Should().BeTrue();
            rewordCommit.IsEnabled.Should().BeTrue();
            copy.Items.Should().NotBeEmpty();

            MenuItem checkoutFeature = checkoutBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem pushFeature = pushBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem mergeFeature = mergeBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem renameFeature = renameBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem deleteFeature = deleteBranch.Items.OfType<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem deleteRemoteMain = deleteBranch.Items.OfType<MenuItem>()
                .Single(item => item.Header?.ToString() == "origin/main");

            checkoutFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            pushFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            mergeFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            renameFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            deleteFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            deleteRemoteMain.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createTag.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            archiveRevision.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            cherryPick.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            revertCommit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            archive.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).StartCheckoutBranch(form, "feature");
            bool pushCompleted;
            commands.Received(1).StartPushDialog(form, false, false, out pushCompleted, "feature");
            commands.Received(1).StartMergeBranchDialog(form, "feature");
            commands.Received(1).StartRenameDialog(form, "feature");
            commands.Received(1).StartDeleteBranchDialog(form, "feature");
            commands.Received(1).StartDeleteRemoteBranchDialog(form, "origin/main");
            commands.Received(1).StartCreateBranchDialog(form, selectedObjectId);
            commands.Received(1).StartCreateTagDialog(
                form,
                Arg.Is<GitRevision>(revision => revision.ObjectId == selectedObjectId));
            commands.Received(2).StartArchiveDialog(
                form,
                Arg.Is<GitRevision>(revision => revision.ObjectId == selectedObjectId),
                null,
                null);
            commands.Received(1).StartCherryPickDialog(
                form,
                Arg.Is<IEnumerable<GitRevision>>(revisions => revisions.Single().ObjectId == selectedObjectId));
            commands.Received(1).StartRevertCommitDialog(
                form,
                Arg.Is<GitRevision>(revision => revision.ObjectId == selectedObjectId));

            bool originalDontConfirmRebase = AppSettings.DontConfirmRebase.Value;
            try
            {
                AppSettings.DontConfirmRebase.Value = true;
                rebase.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                commands.Received(1).StartRebase(form, "feature");
            }
            finally
            {
                AppSettings.DontConfirmRebase.Value = originalDontConfirmRebase;
            }
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(RawInputModifiers.None, false, true)]
    [TestCase(RawInputModifiers.Shift, false, false)]
    [TestCase(RawInputModifiers.None, true, false)]
    [TestCase(RawInputModifiers.Control, true, true)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Shift, false, true)]
    public async Task RevisionGrid_ref_context_menu_should_filter_actions_and_restore_the_full_menu(
        RawInputModifiers modifiers,
        bool alwaysShowAdvanced,
        bool focused)
    {
        bool previousAlwaysShowAdvanced = AppSettings.AlwaysShowAdvOpt;
        AppSettings.AlwaysShowAdvOpt = alwaysShowAdvanced;
        GitModule module = CreateRepositoryWithInitialCommit();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "other" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { "release" }).Should().BeTrue();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        FormBrowse form = new(commands) { Width = 1400, Height = 850 };
        try
        {
            form.Show();
            RevisionGridControl grid = form.RevisionGrid;
            await WaitUntilAsync(() => grid.SelectedRevision is not null
                && grid.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()
                    .Any(label => label.GitRef?.Name == "feature" && label.Bounds.Width > 0));
            using WriteableBitmap? initialFrame = form.CaptureRenderedFrame();
            RevisionGridRefRenderer.RefLabelControl label = grid.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .First(label => label.GitRef?.Name == "feature");
            ContextMenu menu = grid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("Missing revision menu.");
            MenuItem otherActions = grid.FindControl<MenuItem>("tsmiOtherActions")
                ?? throw new InvalidOperationException("Missing other actions.");
            MenuItem rename = grid.FindControl<MenuItem>("renameBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Missing rename menu.");
            MenuItem createTag = grid.FindControl<MenuItem>("createTagToolStripMenuItem")
                ?? throw new InvalidOperationException("Missing create tag menu.");
            MenuItem deleteTag = grid.FindControl<MenuItem>("deleteTagToolStripMenuItem")
                ?? throw new InvalidOperationException("Missing delete tag menu.");
            CopyContextMenuItem copy = grid.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")
                ?? throw new InvalidOperationException("Missing copy menu.");
            Avalonia.Point point = Avalonia.VisualExtensions.TranslatePoint(label, new Avalonia.Point(label.Bounds.Width / 2, label.Bounds.Height / 2), form)
                ?? throw new InvalidOperationException("The ref label is not attached.");
            if (Environment.GetEnvironmentVariable("GITEXT_REF_MENU_EVIDENCE") is { Length: > 0 } initialEvidenceDirectory)
            {
                Directory.CreateDirectory(initialEvidenceDirectory);
                initialFrame?.Save(Path.Combine(initialEvidenceDirectory, $"ref-initial-{modifiers}-{alwaysShowAdvanced}.png"), PngBitmapEncoderOptions.Default);
            }

            Avalonia.Visual? hit = form.InputHitTest(point) as Avalonia.Visual;
            ReferenceEquals(hit?.GetSelfAndVisualAncestors()
                .OfType<RevisionGridRefRenderer.RefLabelControl>().FirstOrDefault(), label).Should().BeTrue(
                    $"the pointer at {point} must hit the ref label at {label.Bounds}, not {hit?.GetType().Name}");
            form.MouseMove(point);
            form.MouseDown(point, MouseButton.Right, modifiers);
            form.MouseUp(point, MouseButton.Right, modifiers);
            Dispatcher.UIThread.RunJobs();

            if (Environment.GetEnvironmentVariable("GITEXT_REF_MENU_EVIDENCE") is { Length: > 0 } evidenceDirectory)
            {
                Directory.CreateDirectory(evidenceDirectory);
                using WriteableBitmap? frame = form.CaptureRenderedFrame();
                frame?.Save(Path.Combine(evidenceDirectory, $"ref-menu-{modifiers}-{alwaysShowAdvanced}.png"), PngBitmapEncoderOptions.Default);
                if (TopLevel.GetTopLevel(menu) is { } popup)
                {
                    using WriteableBitmap? popupFrame = popup.CaptureRenderedFrame();
                    popupFrame?.Save(Path.Combine(evidenceDirectory, $"ref-popup-{modifiers}-{alwaysShowAdvanced}.png"), PngBitmapEncoderOptions.Default);
                }
            }

            menu.IsOpen.Should().BeTrue();
            otherActions.IsVisible.Should().Be(focused);
            grid.RefreshRealizedRows();
            Dispatcher.UIThread.RunJobs();
            menu.IsOpen.Should().BeTrue("refreshing recycled ref labels must not detach the popup anchor");
            otherActions.Items.Contains(createTag).Should().Be(focused);
            menu.Items.Contains(createTag).Should().Be(!focused);
            rename.Items.OfType<MenuItem>().Select(item => item.Header).Should().Equal("feature");
            deleteTag.IsVisible.Should().BeFalse("the clicked branch is not a tag");
            copy.RefreshItems();
            copy.Items.OfType<MenuItem>().Select(item => item.Header?.ToString())
                .Should().NotContain(header => header != null && (header.Contains("other") || header.Contains("release")));

            // Child clicks bubble in Avalonia; a single-ref shortcut must not execute twice.
            rename.Items.OfType<MenuItem>().Single().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            commands.Received(1).StartRenameDialog(form, "feature");
            commands.ClearReceivedCalls();
            rename.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            commands.Received(1).StartRenameDialog(form, "feature");

            menu.Close();
            grid.GetTestAccessor().Revisions.Focus().Should().BeTrue();
            form.KeyPress(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, keySymbol: null);
            form.KeyRelease(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, keySymbol: null);
            Dispatcher.UIThread.RunJobs();
            menu.IsOpen.Should().BeTrue();
            otherActions.IsVisible.Should().BeFalse();
            otherActions.Items.Should().BeEmpty();
            menu.Items.Contains(createTag).Should().BeTrue();
            deleteTag.IsVisible.Should().BeTrue();
            rename.Items.OfType<MenuItem>().Select(item => item.Header).Should().Contain("other");
            copy.RefreshItems();
            copy.Items.OfType<MenuItem>().Select(item => item.Header?.ToString())
                .Should().Contain(header => header != null && header.Contains("other"));
            menu.Close();
        }
        finally
        {
            form.RevisionGrid.FindControl<ContextMenu>("mainContextMenu")?.Close();
            form.Close();
            AppSettings.AlwaysShowAdvOpt = previousAlwaysShowAdvanced;
        }
    }

    [AvaloniaTest]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task RevisionGrid_should_include_stashed_untracked_commits_up_to_the_configured_limit(int limit)
    {
        bool previousShowStashes = AppSettings.ShowStashes;
        bool previousShowReflog = AppSettings.ShowReflogReferences;
        const string limitSetting = "maxStashesWithUntrackedFiles";
        string? previousLimit = AppSettings.GetString(limitSetting, null);
        FormBrowse? form = null;
        try
        {
            AppSettings.ShowStashes = true;
            AppSettings.ShowReflogReferences.Value = false;
            AppSettings.SetInt(limitSetting, limit);
            GitModule module = CreateRepositoryWithInitialCommit();
            for (int index = 0; index < 2; index++)
            {
                File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), $"change {index}");
                module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
                File.WriteAllText(Path.Combine(_workingDirectory, $"untracked-{index}.txt"), "untracked");
                module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "--include-untracked" }).Should().BeTrue();
            }

            GitRevision[] stashes = [.. new RevisionReader(module).GetStashes(CancellationToken.None)];
            stashes.Should().HaveCount(2);
            ObjectId olderUntracked = stashes[1].ParentIds![2];
            ObjectId olderIndex = stashes[1].ParentIds![1];
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RevisionGridControl grid = form.RevisionGrid;
            await WaitUntilAsync(() => grid.GetTestAccessor().Revisions.Items.OfType<GitRevision>()
                .Any(revision => revision.ObjectId == stashes[1].ObjectId));
            GitRevision[] revisions = [.. grid.GetTestAccessor().Revisions.Items.OfType<GitRevision>()];
            revisions.Select(revision => revision.ObjectId).Should().OnlyHaveUniqueItems();
            GitRevision olderStash = revisions.Single(revision => revision.ObjectId == stashes[1].ObjectId);
            olderStash.ParentIds.Should().HaveCount(limit > 1 ? 2 : 1);
            revisions.Any(revision => revision.ObjectId == olderUntracked).Should().Be(limit > 1);
            revisions.Should().NotContain(revision => revision.ObjectId == olderIndex);
        }
        finally
        {
            form?.Close();
            AppSettings.ShowStashes = previousShowStashes;
            AppSettings.ShowReflogReferences.Value = previousShowReflog;
            AppSettings.SettingsContainer.SetString(limitSetting, previousLimit);
        }
    }

    [AvaloniaTest]
    [TestCase(false, false, false, "WorkTree,Index,Head")]
    [TestCase(true, true, true, "WorkTree,Index,Head")]
    [TestCase(true, true, false, "WorkTree,Head,WorkTree")]
    [TestCase(true, false, true, "Index,Head,Index")]
    [TestCase(true, false, false, "Head,Head,Head")]
    public async Task RevisionGrid_should_cycle_worktree_index_and_head_using_status_visibility(
        bool showStatus,
        bool worktreeChanged,
        bool indexChanged,
        string expectedSequence)
    {
        bool previousShowArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        bool previousShowStatus = AppSettings.ShowGitStatusForArtificialCommits;
        FormBrowse? form = null;
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            AppSettings.ShowGitStatusForArtificialCommits = showStatus;
            GitModule module = CreateRepositoryWithInitialCommit();
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RevisionGridControl grid = form.RevisionGrid;
            await WaitUntilAsync(() => grid.GetTestAccessor().Revisions.Items.Count >= 3);
            ObjectId head = module.GetCurrentCheckout();
            grid.SetSelectedRevision(head);
            List<GitItemStatus> status = [];
            if (worktreeChanged)
            {
                status.Add(new GitItemStatus("tracked.txt") { Staged = StagedStatus.WorkTree, IsChanged = true });
            }

            if (indexChanged)
            {
                status.Add(new GitItemStatus("staged.txt") { Staged = StagedStatus.Index, IsNew = true });
            }

            int toggled = 0;
            grid.ToggledBetweenArtificialAndHeadCommits += (_, _) => toggled++;
            foreach (string expected in expectedSequence.Split(','))
            {
                grid.UpdateArtificialCommitCount(status);
                grid.ToggleBetweenArtificialAndHeadCommits();
                ObjectId expectedId = expected switch
                {
                    "WorkTree" => ObjectId.WorkTreeId,
                    "Index" => ObjectId.IndexId,
                    _ => head,
                };
                grid.SelectedRevision?.ObjectId.Should().Be(expectedId);
            }

            toggled.Should().Be(3);
        }
        finally
        {
            form?.Close();
            AppSettings.RevisionGraphShowArtificialCommits = previousShowArtificial;
            AppSettings.ShowGitStatusForArtificialCommits = previousShowStatus;
        }
    }

    private GitModule CreateRepositoryWithInitialCommit()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        return module;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the repository reload should complete before the timeout");
    }

    private static void Click(TopLevel topLevel, Control control, MouseButton button)
    {
        using WriteableBitmap? frame = topLevel.CaptureRenderedFrame();
        Avalonia.Point clickPoint = Avalonia.VisualExtensions.TranslatePoint(
            control,
            new Avalonia.Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            topLevel) ?? throw new InvalidOperationException("The control position was not available.");
        topLevel.MouseDown(clickPoint, button, RawInputModifiers.None);
        topLevel.MouseUp(clickPoint, button, RawInputModifiers.None);
    }

    private static string HeaderText(TreeViewItem item)
        => ((TextBlock)((StackPanel)item.Header!).Children[1]).Text!;

    private static MenuItem GetMainMenuItem(FormBrowse form, string name)
        => form.mainMenuStrip.Items
            .OfType<MenuItem>()
            .Single(item => item.Name == name);

    private static MenuItem GetTaggedMenuItem(MenuItem parent, string tag)
        => parent.Items
            .OfType<MenuItem>()
            .Single(item => item.Tag as string == tag || item.Name == tag);

    private static string[] GetTaggedItemNames(MenuItem parent)
        => parent.Items
            .Select(item => item switch
            {
                Separator => "|",
                MenuItem menuItem => menuItem.Tag as string ?? menuItem.Name
                    ?? throw new InvalidOperationException("A shared menu command has no tag."),
                _ => throw new InvalidOperationException($"Unexpected shared menu entry: {item?.GetType().Name}"),
            })
            .ToArray();
}

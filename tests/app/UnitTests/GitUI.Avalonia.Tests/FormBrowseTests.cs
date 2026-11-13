using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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
using GitUI.Theming;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
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
    private AppSettingsTestScope _settingsScope = null!;
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _revisionGraphShowArtificialCommits;

    [AvaloniaTest]
    [TestCase(CommitInfoPosition.LeftwardFromList)]
    [TestCase(CommitInfoPosition.RightwardFromList)]
    public async Task Browse_splitter_settings_should_migrate_legacy_native_distances_and_keep_the_restored_ratio(CommitInfoPosition position)
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalSplitView = AppSettings.ShowSplitViewLayout;
        AppSettingsPath settings = new("FormBrowse.Avalonia");
        string[] names = ["MainSplitContainer", "RevisionsSplitContainer", "RightSplitContainer"];
        string[] suffixes = ["Size", "Distance", "Dpi", "FontSize", "Panel1Collapsed", "Panel2Collapsed"];
        Dictionary<string, string?> previousInput = names.SelectMany(name => suffixes.Select(suffix => name + "_" + suffix)).ToDictionary(key => key, settings.GetValue);
        try
        {
            AppSettings.CommitInfoPosition = position;
            AppSettings.ShowSplitViewLayout = true;
            foreach (string key in previousInput.Keys)
            {
                settings.SetValue(key, null);
            }

            foreach (string name in names)
            {
                settings.SetValue(name + "_Size", "6");
                settings.SetValue(name + "_Panel1Collapsed", "false");
                settings.SetValue(name + "_Panel2Collapsed", "false");
            }

            settings.SetValue("MainSplitContainer_Distance", "275");
            settings.SetValue("RevisionsSplitContainer_Distance", "240");
            settings.SetValue("RightSplitContainer_Distance", "210");
            using FormBrowse form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithInitialCommit()));
            Dictionary<string, string?> previous = form.GetTestAccessor().SplitterManager.GetTestAccessor().Splitters
                .SelectMany(splitter => suffixes.Select(suffix => splitter.DistanceSettingsKey[..^"Distance".Length] + suffix))
                .ToDictionary(key => key, settings.GetValue);
            foreach ((string key, string? value) in previousInput)
            {
                previous[key] = value;
            }

            try
            {
                form.Show();
                await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(30));
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                Grid main = form.FindControl<Grid>("MainSplitContainer")!;
                Grid revisions = form.FindControl<Grid>("RevisionsSplitContainer")!;
                Grid right = form.FindControl<Grid>("RightSplitContainer")!;
                int infoColumn = position == CommitInfoPosition.LeftwardFromList ? 0 : 4;
                main.ColumnDefinitions[0].ActualWidth.Should().Be(275);
                revisions.ColumnDefinitions[infoColumn].ActualWidth.Should().Be(240);
                right.RowDefinitions[0].ActualHeight.Should().Be(210);
                form.GetTestAccessor().SplitterManager.SaveSplitters();
                settings.GetValue("RevisionsSplitContainer_Distance").Should().Be(Convert.ToInt32(position == CommitInfoPosition.LeftwardFromList
                    ? 240 : revisions.Bounds.Width - 240).ToString(System.Globalization.CultureInfo.InvariantCulture));
                settings.GetValue("MainSplitContainer_Dpi").Should().Be("96");
                double previousSize = right.Bounds.Height;
                form.Width += 150;
                form.Height += 100;
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                right.Bounds.Height.Should().BeGreaterThan(previousSize);
                main.ColumnDefinitions[0].ActualWidth.Should().Be(275);
                revisions.ColumnDefinitions[infoColumn].ActualWidth.Should().Be(240);
                right.RowDefinitions[0].ActualHeight.Should().Be(Math.Floor(210 * right.Bounds.Height / previousSize));
            }
            finally
            {
                form.Close();
                foreach ((string key, string? value) in previous)
                {
                    settings.SetValue(key, value);
                }
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalSplitView;
            foreach ((string key, string? value) in previousInput)
            {
                settings.SetValue(key, value);
            }
        }
    }

    [AvaloniaTest]
    public async Task Browse_splitter_settings_should_use_the_real_container_bounds()
    {
        bool originalTab = AppSettings.ShowOutputHistoryAsTab;
        try
        {
            AppSettings.ShowOutputHistoryAsTab.Value = false;
            GitModule module = CreateRepositoryWithInitialCommit();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            GitCommands.AppSettingsPath settings = new("FormBrowse.Avalonia");
            string[] suffixes = ["Size", "Distance", "Dpi", "FontSize", "Panel1Collapsed", "Panel2Collapsed"];
            Dictionary<string, string?> previous = form.GetTestAccessor().SplitterManager.GetTestAccessor().Splitters
                .SelectMany(splitter => suffixes.Select(suffix => splitter.DistanceSettingsKey[..^"Distance".Length] + suffix))
                .ToDictionary(key => key, settings.GetValue);
            form.Show();
            try
            {
                await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(30));
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                form.GetTestAccessor().SplitterManager.SaveSplitters();
                foreach ((string name, string container, bool horizontal) in new[]
                {
                    ("MainSplitContainer", "MainSplitContainer", false),
                    ("RevisionsSplitContainer", "RevisionsSplitContainer", false),
                    ("RightSplitContainer", "RightSplitContainer", true),
                    ("LeftSplitContainer", "mainContentGrid", true),
                })
                {
                    Grid grid = form.FindControl<Grid>(container)!;
                    int actualSize = Convert.ToInt32(horizontal ? grid.Bounds.Height : grid.Bounds.Width);
                    actualSize.Should().BeGreaterThan(6);
                    settings.GetValue(name + "_Size").Should().Be(actualSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            finally
            {
                form.Close();
                foreach ((string key, string? value) in previous)
                {
                    settings.SetValue(key, value);
                }
            }
        }
        finally
        {
            AppSettings.ShowOutputHistoryAsTab.Value = originalTab;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_notification_bars_should_preserve_the_height_needed_by_enlarged_buttons()
    {
        WinFormsShims.Font originalFont = AppSettings.Font;
        try
        {
            AppSettings.Font = new WinFormsShims.Font("Arial", 18);
            using FormBrowse form = new();
            InteractiveGitActionControl standalone = new();
            foreach (string name in new[] { "notificationBarGitActionInProgress", "notificationBarBisectInProgress" })
            {
                InteractiveGitActionControl bar = form.FindControl<InteractiveGitActionControl>(name)!;
                bar.MinHeight.Should().Be(standalone.MinHeight);
                bar.MinHeight.Should().BeGreaterThan(33);
            }
        }
        finally
        {
            AppSettings.Font = originalFont;
        }
    }

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _settingsScope = new AppSettingsTestScope();
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
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = _revisionGraphShowArtificialCommits;
            _serviceContainer.Dispose();
            TestDirectory.Delete(_workingDirectory);
        }
        finally
        {
            _settingsScope.Dispose();
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Dashboard_should_publish_real_MEF_hosters_before_loading_other_plugins(bool isDashboard)
    {
        System.Reflection.FieldInfo providerField = typeof(ManagedExtensibility).GetField("_exportProvider", BindingFlags.NonPublic | BindingFlags.Static)!;
        System.Reflection.FieldInfo loadedField = typeof(PluginRegistry).GetField("_isLoaded", BindingFlags.NonPublic | BindingFlags.Static)!;
        System.Reflection.PropertyInfo registeredProperty = typeof(PluginRegistry).GetProperty(nameof(PluginRegistry.PluginsRegistered))!;
        object? previousProvider = providerField.GetValue(null);
        object? previousLoaded = loadedField.GetValue(null);
        bool previousRegistered = PluginRegistry.PluginsRegistered;
        IGitPlugin[] previousPlugins = [.. PluginRegistry.Plugins];
        IRepositoryHostPlugin[] previousHosts = [.. PluginRegistry.GitHosters];
        Microsoft.VisualStudio.Composition.AttributedPartDiscoveryV1 discovery = new(Microsoft.VisualStudio.Composition.Resolver.DefaultInstance);
        Microsoft.VisualStudio.Composition.DiscoveredParts parts = await discovery.CreatePartsAsync([typeof(StartupPhaseExports)]);
        Microsoft.VisualStudio.Composition.ComposableCatalog catalog = Microsoft.VisualStudio.Composition.ComposableCatalog.Create(Microsoft.VisualStudio.Composition.Resolver.DefaultInstance).AddParts(parts);
        Microsoft.VisualStudio.Composition.CompositionConfiguration configuration = Microsoft.VisualStudio.Composition.CompositionConfiguration.Create(catalog);
        configuration.ThrowOnErrors();
        using Microsoft.VisualStudio.Composition.ExportProvider provider = Microsoft.VisualStudio.Composition.RuntimeComposition.CreateRuntimeComposition(configuration).CreateExportProviderFactory().CreateExportProvider();
        FormBrowse? form = null;
        bool hostLinkBeforeOtherPlugin = false;
        bool hostMenuBeforeOtherPlugin = false;
        try
        {
            PluginRegistry.Plugins.Clear();
            PluginRegistry.GitHosters.Clear();
            loadedField.SetValue(null, false);
            registeredProperty.SetValue(null, false);
            StartupPhaseExports.Hoster = Substitute.For<IRepositoryHostPlugin>();
            StartupPhaseExports.Hoster.Name.Returns("Startup host");
            StartupPhaseExports.Hoster.Description.Returns("Startup host plugin");
            StartupPhaseExports.Hoster.Id.Returns(Guid.Parse("0561fb5a-aeb0-4c51-a650-246f89175ff8"));
            StartupPhaseExports.Other = Substitute.For<IGitPlugin>();
            StartupPhaseExports.Other.Name.Returns("Startup other");
            StartupPhaseExports.Other.Description.Returns("Startup other plugin");
            StartupPhaseExports.Other.Id.Returns(Guid.Parse("0561fb5a-aeb0-4c51-a650-246f89175ff9"));
            ManagedExtensibility.SetTestExportProvider(provider);
            form = new FormBrowse(new GitUICommands(_serviceContainer, isDashboard ? new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory) : CreateRepositoryWithInitialCommit()));
            StartupPhaseExports.BeforeOther = () =>
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    ReadStartupState();
                }
                else
                {
                    Dispatcher.UIThread.Invoke(ReadStartupState);
                }
            };

            void ReadStartupState()
            {
                hostMenuBeforeOtherPlugin = form.FindControl<MenuItem>("_repositoryHostsToolStripMenuItem") is { IsVisible: true, Header: "Startup host" };
                hostLinkBeforeOtherPlugin = form.GetLogicalDescendants().OfType<DashboardLinkLabel>()
                    .Any(link => link.Text.Contains("Startup host", StringComparison.Ordinal));
            }

            form.Show();
            await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(30));
            hostMenuBeforeOtherPlugin.Should().Be(isDashboard);
            hostLinkBeforeOtherPlugin.Should().Be(isDashboard);
            PluginRegistry.GitHosters.Should().ContainSingle().Which.Should().BeSameAs(StartupPhaseExports.Hoster);
            StartupPhaseExports.Hoster.Received(1).Register(form.UICommands);
            StartupPhaseExports.Other.Received(1).Register(form.UICommands);
        }
        finally
        {
            try
            {
                if (form is not null)
                {
                    try
                    {
                        await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(30));
                    }
                    finally
                    {
                        form.Close();
                        ((IDisposable)form).Dispose();
                    }
                }
            }
            finally
            {
                StartupPhaseExports.BeforeOther = null;
                StartupPhaseExports.Hoster = null!;
                StartupPhaseExports.Other = null!;
                providerField.SetValue(null, previousProvider);
                loadedField.SetValue(null, previousLoaded);
                registeredProperty.SetValue(null, previousRegistered);
                PluginRegistry.Plugins.Clear();
                PluginRegistry.Plugins.AddRange(previousPlugins);
                PluginRegistry.GitHosters.Clear();
                PluginRegistry.GitHosters.AddRange(previousHosts);
            }
        }
    }

    [System.ComponentModel.Composition.PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public sealed class StartupPhaseExports
    {
        public static IRepositoryHostPlugin Hoster { get; set; } = null!;
        public static IGitPlugin Other { get; set; } = null!;
        public static Action? BeforeOther { get; set; }

        [System.ComponentModel.Composition.Export(typeof(IRepositoryHostPlugin))]
        [System.ComponentModel.Composition.Export(typeof(IGitPlugin))]
        public IRepositoryHostPlugin Host => Hoster;

        [System.ComponentModel.Composition.Export(typeof(IGitPlugin))]
        public IGitPlugin General
        {
            get
            {
                BeforeOther?.Invoke();
                return Other;
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_should_register_all_source_splitter_boundaries()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        SplitterManager manager = form.GetTestAccessor().SplitterManager;

        manager.GetTestAccessor().Splitters
            .Select(splitter => splitter.DistanceSettingsKey)
            .Should().Contain(
                "MainSplitContainer_Distance",
                "RevisionsSplitContainer_Distance",
                "RightSplitContainer_Distance",
                "LeftSplitContainer_Distance");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_commit_info_side_width_should_include_the_source_scrollbar_allowance()
    {
        using FormBrowse form = new();

        GridLength commitInfoWidth = form.GetTestAccessor().CommitInfoWidth;

        commitInfoWidth.Value.Should().Be(507,
            "the source uses 490 content pixels plus the 17-pixel 96-DPI system scrollbar width");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public async Task Browse_revision_fill_column_should_use_the_full_viewport_without_vertical_overflow()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module))
        {
            Width = 923,
            Height = 573,
        };
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null);
            CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root;
            CaptureNode grid = Descendants(root).Single(node => node.FieldName == "_gridView");

            grid.Columns.Where(column => column.Visible).Sum(column => column.WidthDip)
                .Should().Be(grid.ClientSizeDip.Width,
                    "the source fill column uses the scrollbar-free viewport when one row does not overflow");
        }
        finally
        {
            form.Close();
        }

        static IEnumerable<CaptureNode> Descendants(CaptureNode node)
        {
            yield return node;
            foreach (CaptureNode child in node.Children)
            {
                foreach (CaptureNode descendant in Descendants(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_tree_should_report_source_toolbar_colors_and_logical_visibility()
    {
        foreach ((string ownerType, string designer, string field, string sourceType) in new[]
        {
            ("GitUI.CommandsDialogs.FormBrowse", "src/app/GitUI/CommandsDialogs/FormBrowse.Designer.cs", "toolStripSplitStash", "ToolStripSplitButton"),
            ("GitUI.FileStatusList", "src/app/GitUI/UserControls/FileStatusList.Designer.cs", "sepRefresh", "ToolStripSeparator"),
        })
        {
            WinFormsInputMetadata.SourceByType[ownerType].Single(control => control.FieldName == field).SourceType.Should().Be(sourceType);
            ReadOriginalDesigner(designer).Should().NotMatchRegex($@"(?m)^\s*(?:this\.)?{field}\.BackColor\s*=",
                "the shared paint-default mapping requires evidence that the original item has no authored BackColor");
        }

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
            overflow.Bounds.Width.Should().Be(16, "the actual source ToolStripOverflowButton reserves its sixteen-pixel maximum width");

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
                    item.Colors.Background.Should().Be("#00FFFFFF", $"{name} retains the source transparent toolbar background");
                    item.Colors.DisabledBackground.Should().Be("#00FFFFFF");
                    item.Colors.Foreground.Should().Be(sourceForeground);
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
                IconSplitButton actualStash = form.FindControl<IconSplitButton>("toolStripSplitStash")!;
                IBrush? originalStashPaint = actualStash.Background;
                try
                {
                    foreach (Color authored in new[] { Colors.CornflowerBlue, Color.Parse("#00FFFFFF") })
                    {
                        actualStash.Background = new SolidColorBrush(authored);
                        CaptureNode authoredStash = Flatten(new AvaloniaControlTreeReader(form, renderScale: 1)
                            .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root)
                            .Single(node => node.FieldName == "toolStripSplitStash");
                        authoredStash.Colors.Background.Should().Be($"#{authored.A:X2}{authored.R:X2}{authored.G:X2}{authored.B:X2}",
                            "an authored stash BackColor is distinct from its shared renderer-only default brush");
                        authoredStash.Colors.DisabledBackground.Should().Be(authoredStash.Colors.Background);
                    }
                }
                finally
                {
                    actualStash.Background = originalStashPaint;
                }

                CaptureNode restoredStash = Flatten(new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize((int)form.Bounds.Width, (int)form.Bounds.Height)).Root)
                    .Single(node => node.FieldName == "toolStripSplitStash");
                restoredStash.Colors.Background.Should().Be(restoredStash.Visible == true ? "#00FFFFFF" : background);
                CaptureNode stashSeparator = nodes.Single(node => node.FieldName == "toolStripSeparator2");
                stashSeparator.Colors.Background.Should().Be(stashSeparator.Visible == true ? "#00FFFFFF" : background);
                stashSeparator.Colors.DisabledBackground.Should().Be(stashSeparator.Visible == true ? "#00FFFFFF" : background);
                foreach (Control item in form.ToolStripMain.Items)
                {
                    if (item.Name is not null && nodes.SingleOrDefault(node => node.FieldName == item.Name) is CaptureNode node)
                    {
                        node.Visible.Should().Be(item.IsVisible
                            && form.ToolStripMain.GetItemPlacement(item) == NativeToolStripItemPlacement.Main,
                            $"{item.Name} remains a logical source item, but a closed overflow does not paint it in the main surface");
                    }
                }

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

        static string ReadOriginalDesigner(string relativePath)
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
            {
                directory = directory.Parent;
            }

            string sourceRoot = directory?.FullName
                ?? throw new InvalidOperationException("The original Designer source root is unavailable.");
            return File.ReadAllText(Path.Combine(sourceRoot, relativePath));
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
    public void Browse_working_directory_capture_should_emit_the_hosted_filter_before_its_separator()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 923, Height = 573 };
        form.Show();
        try
        {
            CaptureNode closedPrimary = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize(923, 573)).Root;
            CaptureNode closedWorkingDirectory = Flatten(closedPrimary)
                .Single(node => node.FieldName == "_NO_TRANSLATE_WorkingDir");
            closedWorkingDirectory.Children.Should().BeEmpty(
                "the source does not add its hosted filter until the working-directory drop-down opens");

            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(form, new CaptureStatePlan
            {
                Id = "working-directory.open",
                Kind = CaptureStateKind.MenuOpen,
                TargetField = "_NO_TRANSLATE_WorkingDir",
            });

            CaptureNode primary = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize(923, 573)).Root;
            CaptureNode workingDirectory = Flatten(primary)
                .Single(node => node.FieldName == "_NO_TRANSLATE_WorkingDir");
            Control popupRoot = driver.PopupSurfaceRoots.Should().ContainSingle().Subject;
            WorkingDirectoryToolStripSplitButton selector = form.FindControl<WorkingDirectoryToolStripSplitButton>("_NO_TRANSLATE_WorkingDir")!;
            TextBox filter = selector.GetTestAccessor().Filter;
            Point filterOrigin = filter.TranslatePoint(default, popupRoot)!.Value;
            Rect actualFilterBounds = new(filterOrigin, filter.Bounds.Size);
            AssertHostedFilterOrder(workingDirectory.Children, actualFilterBounds);

            CaptureNode popup = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadSurface(
                    popupRoot,
                    "popup:0",
                    new PixelRect(0, 0, (int)popupRoot.Bounds.Width, (int)popupRoot.Bounds.Height))
                .Root;
            AssertHostedFilterOrder(popup.Children, actualFilterBounds);
        }
        finally
        {
            form.Close();
        }

        static void AssertHostedFilterOrder(IReadOnlyList<CaptureNode> nodes, Rect actualBounds)
        {
            nodes.Should().HaveCountGreaterThan(1);
            nodes[0].Type.Should().Be("System.Windows.Forms.ToolStripTextBox");
            nodes[0].ControlKind.Should().Be("menuItem");
            nodes[0].Text.Should().BeEmpty();
            nodes[0].BoundsDip.X.Should().Be(decimal.Round((decimal)actualBounds.X, 4));
            nodes[0].BoundsDip.Y.Should().Be(decimal.Round((decimal)actualBounds.Y, 4));
            nodes[0].BoundsDip.Width.Should().Be(decimal.Round((decimal)actualBounds.Width, 4));
            nodes[0].BoundsDip.Height.Should().Be(decimal.Round((decimal)actualBounds.Height, 4));
            nodes[1].Type.Should().Be(typeof(NativeToolStripDropDownSeparator).FullName);
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
            showReflog.Margin.Should().Be(new Thickness(0, 1, 0, 2));
            showBranches.Margin.Should().Be(new Thickness(0, 1, 0, 2));
            NativeToolStrip items = filters.Strip;
            Control[] originalItems = items.Items.ToArray();
            form.toolStripFiltersOverflow.IsVisible.Should().Be(items.HasOverflow);

            form.Width = 923;
            Dispatcher.UIThread.RunJobs();
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            form.toolStripFiltersOverflow.IsVisible.Should().BeTrue();
            items.Items.Should().Equal(originalItems);
            items.Items.Should().OnlyContain(item => item.Opacity == 1 && item.IsHitTestVisible);
            items.OverflowItems.Should().NotBeEmpty();
            foreach (Control item in items.Items.Where(item => items.GetItemPlacement(item) == NativeToolStripItemPlacement.Main))
            {
                item.Bounds.Right.Should().BeLessThanOrEqualTo(items.Bounds.Width);
            }
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
            FilterToolBar filters = narrow.FindControl<FilterToolBar>("ToolStripFilters")!;
            Control advanced = filters.FindControl<Control>("tsbtnAdvancedFilter")!;
            advanced.Opacity.Should().Be(1);
            filters.Strip.GetItemPlacement(advanced).Should().Be(NativeToolStripItemPlacement.Overflow,
                "even the first item cannot fit beside the overflow button");
        }
        finally
        {
            narrow.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_toolbar_overflow_should_present_the_same_source_commands_in_a_wrapped_popup()
    {
        using FormBrowse form = new() { Width = 360, Height = 573 };
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            NativeToolStrip toolbar = form.ToolStripMain;
            Button overflow = form.toolStripMainOverflow;
            Control[] sourceItems = toolbar.Items.ToArray();
            toolbar.PreferredSize.Width.Should().BeGreaterThan(toolbar.Bounds.Width);
            overflow.IsVisible.Should().BeTrue();

            overflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            toolbar.IsOverflowOpen.Should().BeTrue();
            toolbar.Items.Should().Equal(sourceItems);
            toolbar.OverflowItems.Should().Contain(form.EditSettings);
            foreach (Control command in toolbar.OverflowItems)
            {
                command.Parent.Should().BeSameAs(toolbar, "visual popup reparenting must retain the command owner");
                if (NativeToolStrip.GetItemIsSeparator(command))
                {
                    command.GetVisualParent().Should().BeNull("the native overflow omits separators from DisplayedItems");
                }
                else
                {
                    command.GetVisualParent().Should().BeSameAs(toolbar.OverflowContent);
                }

                command.Opacity.Should().Be(1);
                command.IsHitTestVisible.Should().BeTrue();
            }

            overflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            toolbar.IsOverflowOpen.Should().BeFalse();
            toolbar.Items.Should().Equal(sourceItems);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Browse_toolbar_should_move_whole_long_repository_and_later_commands_to_source_overflow()
    {
        using FormBrowse form = new() { Width = 760, Height = 573 };
        form.Show();
        try
        {
            WorkingDirectoryToolStripSplitButton selector = form.FindControl<WorkingDirectoryToolStripSplitButton>(
                "_NO_TRANSLATE_WorkingDir")!;
            selector.Width = double.NaN;
            selector.Content = new string('x', 200);
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            NativeToolStrip toolbar = form.ToolStripMain;
            toolbar.GetItemPlacement(selector).Should().Be(NativeToolStripItemPlacement.Overflow);
            toolbar.GetItemPlacement(form.branchSelect).Should().Be(NativeToolStripItemPlacement.Overflow);
            toolbar.GetItemPlacement(form.toolStripButtonCommit).Should().Be(NativeToolStripItemPlacement.Overflow);
            selector.MaxWidth.Should().Be(double.PositiveInfinity, "native AsNeeded moves whole items instead of squeezing the repository selector");
            toolbar.ShowOverflow();
            Dispatcher.UIThread.RunJobs();
            selector.Bounds.Width.Should().BeGreaterThan(toolbar.Bounds.Width);
            const int sourceBorder = 2;
            const int sourceImageWidth = 16;
            const int sourceDropDownWidth = 11;
            const int sourceSplitterWidth = 1;
            double sourceWidth = Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(selector, selector.Content as string ?? string.Empty).Width)
                + sourceImageWidth + (sourceBorder * 2) + sourceDropDownWidth + sourceSplitterWidth;
            selector.Bounds.Width.Should().Be(sourceWidth, "the source split-button preferred allocation is retained even when the popup host is narrower");
            toolbar.IsOverflowOpen.Should().BeTrue();
            toolbar.OverflowContent.Bounds.Width.Should().BeGreaterThan(0);
            TestContext.Out.WriteLine($"Source item width={sourceWidth}; actual popup viewport width={toolbar.OverflowContent.Bounds.Width}; "
                + $"actual host chain={string.Join(", ", toolbar.OverflowContent.GetVisualAncestors().Select(parent => parent.GetType().Name))}; "
                + "full accessibility of an item wider than the available popup viewport is unverified.");
            form.branchSelect.GetVisualParent().Should().BeSameAs(toolbar.OverflowContent);
            form.toolStripButtonCommit.GetVisualParent().Should().BeSameAs(toolbar.OverflowContent);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(0, 71)]
    [TestCase(2, 88)]
    [TestCase(10, 94)]
    public void Browse_commit_status_width_should_measure_current_source_text_while_the_overflow_is_closed(int count, int nativeWidth)
    {
        using FormBrowse form = new() { Width = 360, Height = 573 };
        form.Show();
        try
        {
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            IconButton command = form.toolStripButtonCommit;
            form.ToolStripMain.GetItemPlacement(command).Should().Be(NativeToolStripItemPlacement.Overflow);
            command.GetVisualParent().Should().BeNull();
            MethodInfo update = typeof(FormBrowse).GetMethod("UpdateCommitButtonAndGetBrush", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The original commit-status sizing boundary must remain available.");
            GitItemStatus[] statuses = Enumerable.Range(0, count)
                .Select(index => new GitItemStatus($"file-{index}.txt") { Staged = StagedStatus.WorkTree, IsChanged = true }).ToArray();
            update.Invoke(form, [statuses, count > 0]);
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            string caption = count > 0 ? $"Commit ({count})" : "Commit";
            command.Content.Should().Be(caption);
            double preferred = Math.Max(23, Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(command, caption).Width) + 20);
            command.Width.Should().Be(preferred);
            if (OperatingSystem.IsWindows())
            {
                command.Width.Should().Be(nativeWidth, "actual original default-font ToolStripButton preferred sizes compose the current caption and image");
            }

            update.Invoke(form, [null, true]);
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            command.Content.Should().Be("Commit");
            command.Width.Should().Be(preferred, "the source retains the counted width while loading, even before the closed overflow has arranged the item");

            form.ToolStripMain.ShowOverflow();
            Dispatcher.UIThread.RunJobs();
            command.Bounds.Width.Should().Be(preferred);
            command.GetVisualParent().Should().BeSameAs(form.ToolStripMain.OverflowContent);
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
            IconButton settings = form.FindControl<IconButton>("EditSettings")!;
            Point stashPosition = stash.TranslatePoint(default, form.ToolStripMain)!.Value;
            Point settingsPosition = settings.TranslatePoint(default, form.ToolStripMain)!.Value;
            stash.Icon.Should().NotBeNull();
            stash.Content.Should().Be(string.Empty);
            stash.Classes.Should().Contain("gitextensions-icon-only");
            (stashPosition.X + stash.Bounds.Width)
                .Should().BeLessThanOrEqualTo(form.ToolStripMain.Bounds.Width);
            settings.Opacity.Should().Be(1);
            settings.IsHitTestVisible.Should().BeTrue();
            settingsPosition.X.Should().BeGreaterThanOrEqualTo(0);
            (settingsPosition.X + settings.Bounds.Width)
                .Should().BeLessThanOrEqualTo(form.ToolStripMain.Bounds.Width);
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

            // Closed overflow gives the source combo hosts their current two-pixel margins,
            // so the AutoSize row is 27, not its initial 25. The 573-DIP client leaves 507
            // after its 27-DIP menu, 27-DIP toolbar and 12-DIP content padding. The split
            // takes floor(209 * 507 / 502) = 211; splitter6 and tab insets30 leave260.
            form.toolStripFiltersHost.Bounds.Height.Should().Be(27);
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

            invalidModule.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
            dashboardForm.SetWorkingDir(_workingDirectory);
            Dispatcher.UIThread.RunJobs();

            dashboard.IsVisible.Should().BeFalse();
            dashboardForm.FindControl<Grid>("mainContentGrid")!.IsVisible.Should().BeTrue();
            dashboardForm.FindControl<MenuItem>("repositoryToolStripMenuItem")!.IsVisible.Should().BeTrue();
            dashboardForm.FindControl<Menu>("mainMenuStrip")!.Items
                .OfType<MenuItem>()
                .Select(item => item.Name)
                .Should().Contain(["navigateToolStripMenuItem", "viewToolStripMenuItem"]);
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
    public void FormBrowse_should_paint_source_tab_overflow_and_toolbar_surfaces_with_the_shared_scrollbar()
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

        foreach ((ThemeVariant theme, string tab, string overflow, string scrollThumb) in
                 new[]
                 {
                     (ThemeVariant.Light, "#FFF3F3F3", "#FFFFFFFF", "#33000000"),
                     (ThemeVariant.Dark, "#FF202020", "#FF232323", "#33FFFFFF"),
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
            horizontal.Bounds.Height.Should().Be(17);
            GetColor(position.Background).Should().Be(Color.Parse(scrollThumb));
            position.GetVisualDescendants().Should().NotContain(
                visual => visual.Name == "PART_NativeIndicator",
                "the repository tree uses the same unmodified scrollbar template as other views");

            foreach (string name in new[]
                     {
                         "toolStripSeparator0",
                         "toolStripSeparator17",
                         "toolStripSeparator1",
                         "toolStripSeparator2",
                     })
            {
                Border separator = form.FindControl<Border>(name)!;
                NativeToolStripSeparatorChrome line = (NativeToolStripSeparatorChrome)separator.Child!;
                separator.Bounds.Width.Should().Be(6);
                GetColor(separator.Background).Should().Be(Colors.Transparent);
                separator.Bounds.Height.Should().Be(25);
                line.Bounds.Size.Should().Be(separator.Bounds.Size);
                line.IsHitTestVisible.Should().BeFalse();
                line.Focusable.Should().BeFalse();
                line.UseSystemVisualStyle.Should().Be(theme == ThemeVariant.Light);
            }

            Button toggleLeftPanel = form.FindControl<Button>("toggleLeftPanel")!;
            toggleLeftPanel.Classes.Should().Contain("checked");
            Avalonia.Controls.Primitives.ToggleButton showSubmodules =
                form.repoObjectsTree.FindControl<Avalonia.Controls.Primitives.ToggleButton>("tsbShowSubmodules")!;
            showSubmodules.IsChecked = true;
            form.MouseMove(new Avalonia.Point(form.ClientSize.Width - 1, form.ClientSize.Height - 1));
            Dispatcher.UIThread.RunJobs();

            using (WriteableBitmap frame = form.CaptureRenderedFrame()
                       ?? throw new InvalidOperationException("The Browse toolbar frame is unavailable."))
            {
                using MemoryStream stream = new();
                frame.Save(stream, PngBitmapEncoderOptions.Default);
                stream.Position = 0;
                using SKBitmap bitmap = SKBitmap.Decode(stream);
                string? evidence = Environment.GetEnvironmentVariable("GITEXT_TOOLBAR_STATE_EVIDENCE");
                string? evidencePath = null;
                if (!string.IsNullOrWhiteSpace(evidence))
                {
                    string directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID);
                    Directory.CreateDirectory(directory);
                    evidencePath = Path.Combine(directory, theme.ToString().ToLowerInvariant());
                    File.Exists(evidencePath + ".png").Should().BeFalse("a retained toolbar frame must not overwrite another run");
                    frame.Save(evidencePath + ".png", PngBitmapEncoderOptions.Default);
                }

                Border separator = form.FindControl<Border>("toolStripSeparator0")!;
                Avalonia.Point origin = separator.TranslatePoint(default, form)!.Value;
                Color backdrop = GetColor(form.Background);
                for (int y = 0; y < 25; y++)
                {
                    for (int x = 0; x < 6; x++)
                    {
                        Color expected = theme == ThemeVariant.Light
                            ? x == 2 && y >= 2 && y <= 22 ? Color.Parse("#8C8C8C") : backdrop
                            : x == 3 && y >= 5 && y <= 19 ? Color.Parse("#404040")
                                : x == 4 && y >= 6 && y <= 20 ? Color.Parse("#101010") : backdrop;
                        bitmap.GetPixel((int)origin.X + x, (int)origin.Y + y).Should().Be(
                            new SKColor(expected.R, expected.G, expected.B, expected.A),
                            $"the actual named toolbar owner paints source chrome at {x},{y} in {theme}");
                    }
                }

                List<(string Name, Avalonia.Point Origin, int Row, SKColor[] Actual, SKColor[] Expected)> rows = [];
                foreach (Button owner in new Button[] { toggleLeftPanel, showSubmodules })
                {
                    // The original renderer paints state separately from the content presenter.
                    GetColor(owner.Background).Should().Be(Colors.Transparent);
                    GetColor(GetPresenter(owner).Background).Should().Be(Colors.Transparent);
                    NativeToolStripButtonChrome chrome = owner.GetVisualDescendants().OfType<NativeToolStripButtonChrome>().Single();
                    chrome.IsChecked.Should().BeTrue();
                    chrome.IsSelected.Should().BeFalse();
                    chrome.IsPressed.Should().BeFalse();
                    chrome.UseSystemVisualStyle.Should().Be(theme == ThemeVariant.Light);
                    GetColor(chrome.CheckedBrush).Should().Be(Color.Parse(theme == ThemeVariant.Light ? "#CCE8FF" : "#00000000"));
                    chrome.Bounds.Size.Should().Be(owner.Bounds.Size);
                    Image icon = owner.GetVisualDescendants().OfType<Image>().Single();
                    icon.TranslatePoint(default, chrome)!.Value.Y.Should().BeGreaterThanOrEqualTo(2,
                        "the sampled source chrome rows must not include the actual icon");
                    Avalonia.Point chromeOrigin = chrome.TranslatePoint(default, form)!.Value;
                    int width = (int)chrome.Bounds.Width;
                    Color border = Color.Parse(theme == ThemeVariant.Light ? "#0078D4" : "#2864B4");
                    Color fill = theme == ThemeVariant.Light ? Color.Parse("#CCE8FF") : GetAmbientBackdrop(owner);
                    for (int y = 0; y <= 1; y++)
                    {
                        SKColor[] actual = new SKColor[width];
                        SKColor[] expected = new SKColor[width];
                        for (int x = 0; x < width; x++)
                        {
                            Color color = y == 0 || x == 0 || x == width - 1 ? border
                                : theme != ThemeVariant.Light ? fill
                                : x == 1 || x == width - 2 ? Color.Parse("#AAD9FF")
                                : x == 2 || x == width - 3 ? Color.Parse("#C8E6FF") : fill;
                            actual[x] = bitmap.GetPixel((int)chromeOrigin.X + x, (int)chromeOrigin.Y + y);
                            expected[x] = new SKColor(color.R, color.G, color.B, color.A);
                        }

                        rows.Add((owner.Name!, chromeOrigin, y, actual, expected));
                    }
                }

                if (evidencePath is not null)
                {
                    File.WriteAllText(evidencePath + ".json", JsonSerializer.Serialize(new
                    {
                        test = TestContext.CurrentContext.Test.FullName,
                        theme = theme.ToString(),
                        captureMethod = "actualAvaloniaHeadlessFormBrowseToolbarFrame",
                        PixelSize = new { bitmap.Width, bitmap.Height },
                        rows = rows.Select(row => new
                        {
                            row.Name,
                            row.Origin,
                            width = row.Actual.Length,
                            row.Row,
                            actual = row.Actual.Select(color => color.ToString()).ToArray(),
                            expected = row.Expected.Select(color => color.ToString()).ToArray(),
                        }),
                        scope = "Source-backed checked chrome regression; not a new paired native or full-parity claim.",
                    }, new JsonSerializerOptions { WriteIndented = true }));
                }

                foreach ((string name, Avalonia.Point _, int row, SKColor[] actual, SKColor[] expected) in rows)
                {
                    actual.Should().Equal(expected,
                        $"{name} must paint original checked chrome at row {row} in {theme}, without a platform accent");
                }
            }
        }

        static Avalonia.Controls.Presenters.ContentPresenter GetPresenter(Control control)
            => control.GetVisualDescendants()
                .OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_ContentPresenter");

        static Color GetColor(IBrush? brush)
            => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;

        static Color GetAmbientBackdrop(Control owner)
        {
            foreach (Control ancestor in owner.GetVisualAncestors().OfType<Control>())
            {
                IBrush? background = ancestor switch
                {
                    Border border => border.Background,
                    Panel panel => panel.Background,
                    Avalonia.Controls.Primitives.TemplatedControl templated => templated.Background,
                    _ => null,
                };
                if (background is null || background.Opacity == 0)
                {
                    continue;
                }

                Color color = GetColor(background);
                if (color.A == 0)
                {
                    continue;
                }

                background.Opacity.Should().Be(1, "this source backdrop is an opaque ancestor, not another state overlay");
                color.A.Should().Be(byte.MaxValue);
                return color;
            }

            throw new AssertionException("The actual toolbar must have an opaque background owner.");
        }
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

        // A long temporary repository path can legitimately push both editors into
        // source overflow. Exercise the visible focus route using owner preferred sizes.
        form.Width = form.ToolStripMain.PreferredSize.Width + form.ToolStripFilters.Strip.PreferredSize.Width + 100;
        form.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        ComboBox revisionFilter = form.ToolStripFilters.GetTestAccessor().tstxtRevisionFilter;
        form.ToolStripFilters.Strip.GetItemPlacement(revisionFilter).Should().Be(NativeToolStripItemPlacement.Main,
            "the actual source focus route does not open closed overflow controls");
        form.ToolStripFilters.SetFocus();
        revisionFilter.IsKeyboardFocusWithin.Should().BeTrue();

        form.ExecuteCommand(FormBrowse.Command.FocusRevisionGrid).Should().BeTrue();
        revisions.IsKeyboardFocusWithin.Should().BeTrue();
        revisionFilter.IsKeyboardFocusWithin.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_control_A_should_select_only_the_first_diff_group(bool focusFiles)
    {
        bool previousParents = AppSettings.ShowDiffForAllParents;
        DiffListSortType previousSort = DiffListSortService.Instance.DiffListSorting;
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.ShowDiffForAllParents = true;
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithMerge();
            ObjectId head = module.GetCurrentCheckout();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            try
            {
                RevisionGridControl grid = form.RevisionGrid;
                await WaitUntilAsync(() => grid.SelectedRevision?.ObjectId == head);
                Click(form, form.DiffTabPage, MouseButton.Left);
                FileStatusList files = form.revisionDiff.FileStatusList;
                TreeView tree = files.FindControl<TreeView>("tvDiffFiles")!;
                await WaitUntilAsync(() => files.GitItemStatuses.Count == 4 && tree.ItemCount == 2);
                form.UpdateLayout();
                Control target;
                if (focusFiles)
                {
                    TreeViewItem group = (TreeViewItem)tree.ContainerFromIndex(0)!;
                    TreeViewItem file = group.GetVisualDescendants().OfType<TreeViewItem>()
                        .First(item => item.DataContext is FileStatusList.DiffTreeNode { Item: not null });
                    target = file.GetVisualDescendants().OfType<Control>().First(control => control.Name == "PART_HeaderPresenter");
                }
                else
                {
                    target = (ListBoxItem)grid.GetTestAccessor().Revisions.ContainerFromIndex(0)!;
                }

                Click(form, target, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                files.IsKeyboardFocusWithin.Should().Be(focusFiles);
                grid.GetTestAccessor().Revisions.IsKeyboardFocusWithin.Should().Be(!focusFiles);
                grid.GetSelectedRevisions().Should().ContainSingle().Which.ObjectId.Should().Be(head);

                form.KeyPress(Key.A, RawInputModifiers.Control, PhysicalKey.A, keySymbol: "a");
                form.KeyRelease(Key.A, RawInputModifiers.Control, PhysicalKey.A, keySymbol: "a");
                Dispatcher.UIThread.RunJobs();

                grid.GetSelectedRevisions().Should().ContainSingle().Which.ObjectId.Should().Be(head);
                files.SelectedItems.Select(item => item.Item.Name).Should().BeEquivalentTo(["left-a.txt", "left-b.txt"]);
                files.IsKeyboardFocusWithin.Should().BeTrue();

                TextBox filter = files.GetTestAccessor().FilterComboBox.GetVisualDescendants().OfType<TextBox>().Single();
                Click(form, filter, MouseButton.Left);
                form.KeyTextInput("left");
                filter.Text.Should().Be("left");
                form.KeyPress(Key.A, RawInputModifiers.Control, PhysicalKey.A, keySymbol: "a");
                form.KeyRelease(Key.A, RawInputModifiers.Control, PhysicalKey.A, keySymbol: "a");
                filter.SelectedText.Should().Be("left", "Ctrl+A in a text field must remain a text selection");
                filter.IsKeyboardFocusWithin.Should().BeTrue();
                grid.GetSelectedRevisions().Should().ContainSingle().Which.ObjectId.Should().Be(head);
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowDiffForAllParents = previousParents;
            DiffListSortService.Instance.DiffListSorting = previousSort;
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_plain_R_should_open_the_real_reset_dialog_from_the_diff_file(bool treeMode)
    {
        DiffListSortType previousSort = DiffListSortService.Instance.DiffListSorting;
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            DiffListSortService.Instance.DiffListSorting = treeMode ? DiffListSortType.FilePath : DiffListSortType.FilePathFlat;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second revision\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" }).Should().BeTrue();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            try
            {
                await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout());
                Click(form, form.DiffTabPage, MouseButton.Left);
                FileStatusList files = form.revisionDiff.FileStatusList;
                await WaitUntilAsync(() => files.SelectedItem?.Item.Name == "tracked.txt"
                    && form.fileViewer.TextEditor.Text.Contains("+second revision", StringComparison.Ordinal));
                form.UpdateLayout();
                FileStatusList.TestAccessor accessor = files.GetTestAccessor();
                Control target = (TreeViewItem)accessor.DiffTree.ContainerFromIndex(0)!;
                target.IsEffectivelyVisible.Should().BeTrue();
                Click(form, target, MouseButton.Left);
                files.IsKeyboardFocusWithin.Should().BeTrue();

                int openedCount = 0;
                Exception? observationFailure = null;
                using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<FormResetChanges>((dialog, _) =>
                {
                    openedCount++;
                    Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            Click(dialog, dialog.FindControl<Button>("btnCancel")!, MouseButton.Left);
                        }
                        catch (Exception exception)
                        {
                            observationFailure = exception;
                            dialog.Close();
                        }
                    });
                });

                form.KeyPress(Key.R, RawInputModifiers.None, PhysicalKey.R, keySymbol: "r");
                form.KeyRelease(Key.R, RawInputModifiers.None, PhysicalKey.R, keySymbol: "r");
                Dispatcher.UIThread.RunJobs();

                observationFailure.Should().BeNull();
                openedCount.Should().Be(1, "the default BrowseDiff R shortcut must invoke the production reset confirmation");
                File.ReadAllText(Path.Combine(_workingDirectory, "tracked.txt")).Should().Be("second revision\n",
                    "the actual Cancel button must leave the working file unchanged");
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = previousSort;
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_default_artificial_commit_shortcut_should_use_the_live_staged_only_status()
    {
        bool previousArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        bool previousStatus = AppSettings.ShowGitStatusForArtificialCommits;
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            AppSettings.ShowGitStatusForArtificialCommits = true;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "staged-only\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
            ObjectId head = module.GetCurrentCheckout();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            try
            {
                RevisionGridControl grid = form.RevisionGrid;
                ListBox revisions = grid.GetTestAccessor().Revisions;
                await WaitUntilAsync(() => revisions.ItemCount == 3
                    && grid.GetChangeCount(ObjectId.WorkTreeId) is { DataValid: true, HasChanges: false }
                    && grid.GetChangeCount(ObjectId.IndexId) is { DataValid: true, HasChanges: true });
                form.UpdateLayout();
                ListBoxItem headRow = (ListBoxItem)revisions.ContainerFromIndex(2)!;
                headRow.DataContext.Should().BeOfType<GitRevision>().Which.ObjectId.Should().Be(head);
                Point point = headRow.TranslatePoint(new Point(100, headRow.Bounds.Height / 2), form)!.Value;
                form.MouseMove(point, RawInputModifiers.None);
                form.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
                form.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
                await WaitUntilAsync(() => grid.SelectedRevision?.ObjectId == head);

                int toggled = 0;
                grid.ToggledBetweenArtificialAndHeadCommits += (_, _) => toggled++;
                ObjectId[] expected = [ObjectId.IndexId, head, ObjectId.IndexId, head];
                foreach (ObjectId revision in expected)
                {
                    form.KeyPress(Key.OemBackslash, RawInputModifiers.Control, PhysicalKey.IntlBackslash, keySymbol: "<");
                    form.KeyRelease(Key.OemBackslash, RawInputModifiers.Control, PhysicalKey.IntlBackslash, keySymbol: "<");
                    await WaitUntilAsync(() => grid.SelectedRevision?.ObjectId == revision);
                    form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.DiffTabPage);
                    await WaitUntilAsync(() => form.revisionDiff.FileStatusList.IsKeyboardFocusWithin);
                    form.revisionDiff.FileStatusList.IsKeyboardFocusWithin.Should().BeTrue("the source toggle moves focus to the file list, where the next shortcut must still work");
                    grid.GetChangeCount(ObjectId.WorkTreeId)!.HasChanges.Should().BeFalse();
                    grid.GetChangeCount(ObjectId.IndexId)!.HasChanges.Should().BeTrue();
                }

                toggled.Should().Be(expected.Length);
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.RevisionGraphShowArtificialCommits = previousArtificial;
            AppSettings.ShowGitStatusForArtificialCommits = previousStatus;
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_diff_groups_should_select_their_files_and_support_arrow_navigation()
    {
        bool previousParents = AppSettings.ShowDiffForAllParents;
        DiffListSortType previousSort = DiffListSortService.Instance.DiffListSorting;
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.ShowDiffForAllParents = true;
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithMerge();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            try
            {
                await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout());
                Click(form, form.DiffTabPage, MouseButton.Left);
                FileStatusList files = form.revisionDiff.FileStatusList;
                TreeView tree = files.FindControl<TreeView>("tvDiffFiles")!;
                await WaitUntilAsync(() => files.GitItemStatuses.Count == 4 && tree.ItemCount == 2);
                for (int groupIndex = 0; groupIndex < 2; groupIndex++)
                {
                    form.UpdateLayout();
                    TreeViewItem group = (TreeViewItem)tree.ContainerFromIndex(groupIndex)!;
                    Control header = group.GetVisualDescendants().OfType<Control>()
                        .First(control => control.Name == "PART_HeaderPresenter"
                            && ReferenceEquals(control.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault(), group));
                    Click(form, header, MouseButton.Left);
                    Dispatcher.UIThread.RunJobs();
                    tree.SelectedItem.Should().BeSameAs(tree.Items[groupIndex]);
                    group.IsKeyboardFocusWithin.Should().BeTrue();
                    Control? focused = form.FocusManager!.GetFocusedElement() as Control;
                    TreeViewItem? focusedItem = focused as TreeViewItem
                        ?? focused?.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
                    focusedItem.Should().BeSameAs(group, "the focused header must belong to this group rather than a child file");
                    string prefix = groupIndex == 0 ? "left" : "right";
                    files.SelectedItems.Select(item => item.Item.Name).Should().BeEquivalentTo([$"{prefix}-a.txt", $"{prefix}-b.txt"]);
                    files.SelectedItem.Should().BeNull("a source group has descendants but is not itself a file");
                    files.HasSelection.Should().BeTrue();
                    files.SelectedFolder.Should().Be(RelativePath.From(string.Empty));
                    await WaitUntilAsync(() => form.fileViewer.TextEditor.Text.Contains("(2) /", StringComparison.Ordinal)
                        && form.fileViewer.TextEditor.Text.Contains($"{prefix}-a.txt", StringComparison.Ordinal)
                        && form.fileViewer.TextEditor.Text.Contains($"{prefix}-b.txt", StringComparison.Ordinal));

                    form.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
                    form.KeyRelease(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
                    await WaitUntilAsync(() => files.SelectedItem is not null);
                    files.SelectedItems.Should().ContainSingle().Which.Item.Name.Should().Be($"{prefix}-a.txt");
                    files.SelectedFolder.Should().BeNull("a file leaf is not a folder summary");

                    form.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, keySymbol: null);
                    form.KeyRelease(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, keySymbol: null);
                    Dispatcher.UIThread.RunJobs();
                    tree.SelectedItem.Should().BeSameAs(tree.Items[groupIndex]);
                    files.SelectedItems.Select(item => item.Item.Name).Should().BeEquivalentTo([$"{prefix}-a.txt", $"{prefix}-b.txt"]);
                }

                Click(form, form.TreeTabPage, MouseButton.Left);
                Click(form, form.DiffTabPage, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                tree.SelectedItem.Should().BeSameAs(tree.Items[1], "returning to Diff must retain its selected group");
                files.HasSelection.Should().BeTrue();

                TreeViewItem firstGroup = (TreeViewItem)tree.ContainerFromIndex(0)!;
                Control firstHeader = firstGroup.GetVisualDescendants().OfType<Control>()
                    .First(control => control.Name == "PART_HeaderPresenter"
                        && ReferenceEquals(control.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault(), firstGroup));
                Point point = firstHeader.TranslatePoint(new Point(firstHeader.Bounds.Width / 2, firstHeader.Bounds.Height / 2), form)!.Value;
                form.MouseMove(point, RawInputModifiers.Control);
                form.MouseDown(point, MouseButton.Left, RawInputModifiers.Control);
                form.MouseUp(point, MouseButton.Left, RawInputModifiers.Control);
                (tree.SelectedItems?.Count ?? 0).Should().Be(2);
                files.HasSelection.Should().BeTrue("logical group selections remain selections even without directly selected file leaves");
                files.SelectedFolder.Should().BeNull("the original summarizes a folder only when one node is selected");
                files.SelectedItems.Should().BeEmpty("multiple group tags are not direct file tags in the original");
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowDiffForAllParents = previousParents;
            DiffListSortService.Instance.DiffListSorting = previousSort;
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_revision_arrow_navigation_should_keep_focus_while_the_diff_loads()
    {
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            for (int index = 1; index <= 2; index++)
            {
                File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), $"revision-{index}\n");
                module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", $"revision-{index}" }).Should().BeTrue();
            }

            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            try
            {
                RevisionGridControl grid = form.RevisionGrid;
                ListBox revisions = grid.GetTestAccessor().Revisions;
                await WaitUntilAsync(() => revisions.ItemCount == 3);
                Click(form, form.TreeTabPage, MouseButton.Left);
                Click(form, form.DiffTabPage, MouseButton.Left);
                await WaitUntilAsync(() => form.fileViewer.TextEditor.Text.Contains("+revision-2", StringComparison.Ordinal));
                form.revisionDiff.FileStatusList.IsKeyboardFocusWithin.Should().BeTrue("a real Diff tab change should focus its file list");
                form.UpdateLayout();
                ListBoxItem row = (ListBoxItem)revisions.ContainerFromIndex(1)!;
                Point point = row.TranslatePoint(new Point(100, row.Bounds.Height / 2), form)!.Value;
                form.MouseMove(point, RawInputModifiers.None);
                form.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
                form.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
                await AssertDiffAndFocusAsync(1);

                foreach ((Key key, PhysicalKey physicalKey, int index) in new[]
                {
                    (Key.Up, PhysicalKey.ArrowUp, 0),
                    (Key.Down, PhysicalKey.ArrowDown, 1),
                    (Key.Down, PhysicalKey.ArrowDown, 2),
                    (Key.Up, PhysicalKey.ArrowUp, 1),
                })
                {
                    form.KeyPress(key, RawInputModifiers.None, physicalKey, keySymbol: null);
                    form.KeyRelease(key, RawInputModifiers.None, physicalKey, keySymbol: null);
                    await AssertDiffAndFocusAsync(index);
                }

                return;

                async Task AssertDiffAndFocusAsync(int index)
                {
                    GitRevision expected = (GitRevision)revisions.Items[index]!;
                    string text = index == 2 ? "content" : $"revision-{2 - index}";
                    await WaitUntilAsync(() => grid.SelectedRevision?.ObjectId == expected.ObjectId
                        && form.revisionDiff.FileStatusList.SelectedItem?.SecondRevision.ObjectId == expected.ObjectId
                        && (index == 2
                            ? form.fileViewer.TextEditor.Text == text
                            : form.fileViewer.TextEditor.Text.Contains("+" + text, StringComparison.Ordinal)));
                    revisions.IsKeyboardFocusWithin.Should().BeTrue("loading a diff must not move focus away from revision navigation");
                    form.revisionDiff.FileStatusList.IsKeyboardFocusWithin.Should().BeFalse();
                }
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
        }
    }

    [AvaloniaTest]
    [TestCase(2)]
    [TestCase(3)]
    public async Task FormBrowse_control_click_revisions_should_display_the_actual_endpoint_diff(int selectedCount)
    {
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            ObjectId oldest = module.GetCurrentCheckout();
            for (int index = 1; index <= 2; index++)
            {
                File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), $"revision-{index}\n");
                module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", $"revision-{index}" }).Should().BeTrue();
            }

            ObjectId newest = module.GetCurrentCheckout();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            try
            {
                RevisionGridControl grid = form.RevisionGrid;
                await WaitUntilAsync(() => grid.GetTestAccessor().Revisions.ItemCount == 3);
                Click(form, form.DiffTabPage, MouseButton.Left);
                await WaitUntilAsync(() => form.revisionDiff.FileStatusList.SelectedItem?.SecondRevision.ObjectId == newest
                    && form.fileViewer.TextEditor.Text.Contains("+revision-2", StringComparison.Ordinal));

                ClickRow(2, RawInputModifiers.None);
                if (selectedCount == 3)
                {
                    ClickRow(1, RawInputModifiers.Control);
                }

                ClickRow(0, RawInputModifiers.Control);
                IReadOnlyList<GitRevision> selected = grid.GetSelectedRevisions();
                selected.Should().HaveCount(selectedCount);
                selected[0].ObjectId.Should().Be(newest);
                selected[^1].ObjectId.Should().Be(oldest);
                await WaitUntilAsync(() => form.revisionDiff.FileStatusList.SelectedItem?.FirstRevision?.ObjectId == oldest
                    && form.revisionDiff.FileStatusList.SelectedItem?.SecondRevision.ObjectId == newest
                    && form.fileViewer.TextEditor.Text.Contains("-content", StringComparison.Ordinal)
                    && form.fileViewer.TextEditor.Text.Contains("+revision-2", StringComparison.Ordinal));

                IReadOnlyList<FileStatusWithDescription> groups = (IReadOnlyList<FileStatusWithDescription>)typeof(FileStatusList)
                    .GetProperty("GitItemStatusesWithDescription", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(form.revisionDiff.FileStatusList)!;
                ObjectId[] expectedBases = selectedCount == 3 ? [oldest, module.RevParse("HEAD~1")] : [oldest];
                groups.Select(group => group.FirstRev!.ObjectId).Should().Equal(expectedBases);
                groups.Select(group => group.SecondRev.ObjectId).Should().OnlyContain(revision => revision == newest);
                form.revisionDiff.FileStatusList.GitItemStatuses.Should().HaveCount(selectedCount - 1)
                    .And.OnlyContain(item => item.Name == "tracked.txt");
                form.revisionDiff.FileStatusList.FirstGroupItems.Should().ContainSingle().Which.FirstRevision!.ObjectId.Should().Be(oldest);
                form.fileViewer.TextEditor.Text.Should().NotContain("-revision-1");

                void ClickRow(int index, RawInputModifiers modifiers)
                {
                    form.UpdateLayout();
                    ListBoxItem row = (ListBoxItem)grid.GetTestAccessor().Revisions.ContainerFromIndex(index)!;
                    Point point = row.TranslatePoint(new Point(100, row.Bounds.Height / 2), form)!.Value;
                    form.MouseMove(point, modifiers);
                    form.MouseDown(point, MouseButton.Left, modifiers);
                    form.MouseUp(point, MouseButton.Left, modifiers);
                    Dispatcher.UIThread.RunJobs();
                }
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_F5_should_reload_the_actual_graph_and_preserve_the_pointer_selection(bool multiple)
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        for (int index = 0; index < 2; index++)
        {
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), $"\nrevision {index}");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", $"revision-{index}" }).Should().BeTrue();
        }

        GitUICommands commands = new(_serviceContainer, module);
        using FormBrowse form = new(commands) { Width = 1200, Height = 700 };
        RevisionGridControl grid = form.RevisionGrid;
        RevisionGraph graph = (RevisionGraph)typeof(RevisionGridControl).GetField("_revisionGraph", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(grid)!;
        int loaded = 0;
        int notified = 0;
        grid.RevisionsLoaded += (_, _) => loaded++;
        commands.PostRepositoryChanged += (_, _) => notified++;
        form.Show();
        try
        {
            await WaitUntilAsync(() => loaded > 0 && grid.GetTestAccessor().Revisions.ItemCount == 3 && graph.GetCachedCount() == 3);
            ClickRow(2, RawInputModifiers.None);
            if (multiple)
            {
                ClickRow(0, RawInputModifiers.Control);
            }

            ObjectId[] selected = [.. grid.GetSelectedRevisions().Select(revision => revision.ObjectId)];
            selected.Should().HaveCount(multiple ? 2 : 1);
            SaveGraphFrame("initial");
            for (int refresh = 0; refresh < 2; refresh++)
            {
                int previousLoaded = loaded;
                int previousNotified = notified;

                form.KeyPress(Key.F5, RawInputModifiers.None, PhysicalKey.F5, keySymbol: null);
                form.KeyRelease(Key.F5, RawInputModifiers.None, PhysicalKey.F5, keySymbol: null);
                Dispatcher.UIThread.RunJobs();

                notified.Should().Be(previousNotified + 1, "the original F5 shortcut broadcasts a repository refresh");
                await WaitUntilAsync(() => loaded > previousLoaded && graph.GetCachedCount() == 3);
                grid.GetSelectedRevisions().Select(revision => revision.ObjectId).Should().Equal(selected);
                SaveGraphFrame($"refresh-{refresh}");
            }
        }
        finally
        {
            form.Close();
        }

        void ClickRow(int index, RawInputModifiers modifiers)
        {
            form.UpdateLayout();
            ListBoxItem row = (ListBoxItem)grid.GetTestAccessor().Revisions.ContainerFromIndex(index)!;
            Point point = row.TranslatePoint(new Point(100, row.Bounds.Height / 2), form)!.Value;
            form.MouseMove(point, modifiers);
            form.MouseDown(point, MouseButton.Left, modifiers);
            form.MouseUp(point, MouseButton.Left, modifiers);
            Dispatcher.UIThread.RunJobs();
        }

        void SaveGraphFrame(string state)
        {
            form.UpdateLayout();
            Control[] cells = [.. grid.GetVisualDescendants().OfType<Control>().Where(control => control.Classes.Contains("revision-graph-cell") && control.IsEffectivelyVisible)];
            cells.Should().HaveCount(3);
            cells.Should().OnlyContain(cell => cell.Bounds.Width > 0 && cell.Bounds.Height > 0);
            graph.Count.Should().Be(3);
            graph.GetCachedCount().Should().Be(3);
            using WriteableBitmap? frame = form.CaptureRenderedFrame();
            frame.Should().NotBeNull();
            string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults", "Browse-refresh", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, $"Browse-F5-{multiple}-{state}.png");
            frame!.Save(path, PngBitmapEncoderOptions.Default);
            TestContext.AddTestAttachment(path, $"Browse graph after {state}");
        }
    }

    [AvaloniaTest]
    public void FormBrowse_F5_should_refresh_the_dashboard_without_notifying_a_repository()
    {
        GitUICommands commands = new(_serviceContainer, new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory));
        using FormBrowse form = new(commands);
        int clicked = 0;
        int notified = 0;
        form.refreshDashboardToolStripMenuItem.Click += (_, e) =>
        {
            if (ReferenceEquals(e.Source, form.refreshDashboardToolStripMenuItem))
            {
                clicked++;
            }
        };
        commands.PostRepositoryChanged += (_, _) => notified++;
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            form.KeyPress(Key.F5, RawInputModifiers.None, PhysicalKey.F5, keySymbol: null);
            form.KeyRelease(Key.F5, RawInputModifiers.None, PhysicalKey.F5, keySymbol: null);
            Dispatcher.UIThread.RunJobs();

            clicked.Should().Be(1);
            notified.Should().Be(0);
        }
        finally
        {
            form.Close();
        }
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

            CaptureNode captureRoot = new AvaloniaControlTreeReader(form, 1)
                .ReadPrimary(form, new PixelSize(900, 600)).Root;
            Stack<CaptureNode> captureNodes = new([captureRoot]);
            CaptureNode? capturedPush = null;
            while (captureNodes.TryPop(out CaptureNode? node))
            {
                if (node.FieldName == "toolStripButtonPush")
                {
                    capturedPush = node;
                }

                foreach (CaptureNode child in node.Children)
                {
                    captureNodes.Push(child);
                }
            }

            capturedPush.Should().NotBeNull();
            capturedPush!.Text.Should().Be("1↑");

            double sourceCommitWidth = Math.Max(23,
                Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(commitButton, "Commit (2)").Width) + 20);
            commitButton.Width.Should().Be(sourceCommitWidth);
            commitButton.Width.Should().Be(Math.Ceiling(commitButton.Width));
            if (OperatingSystem.IsWindows())
            {
                commitButton.Width.Should().Be(88);
            }

            if (form.ToolStripMain.GetItemPlacement(commitButton) == NativeToolStripItemPlacement.Overflow)
            {
                form.ToolStripMain.ShowOverflow();
                Dispatcher.UIThread.RunJobs();
                commitButton.GetVisualParent().Should().BeSameAs(form.ToolStripMain.OverflowContent);
            }

            commitButton.Bounds.Width.Should().Be(sourceCommitWidth);
            form.ToolStripMain.CloseOverflow();
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

            await WaitUntilAsync(() => accessor.TreeView.Items.Count == 6);
            TreeViewItem stashRoot = accessor.TreeView.Items.Cast<TreeViewItem>().Last();
            await WaitUntilAsync(() => stashRoot.Items.Count == 2);
            TreeViewItem stashItem = stashRoot.Items.Cast<TreeViewItem>().Last();

            accessor.TreeView.SelectedItem = stashItem;

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

            await WaitUntilAsync(() => accessor.TreeView.Items.OfType<TreeViewItem>()
                .Any(item => HeaderText(item).StartsWith("Worktrees", StringComparison.Ordinal) && item.Items.Count == 2));
            worktreeButton.IsVisible.Should().BeTrue();

            TreeViewItem root = accessor.TreeView.Items.Cast<TreeViewItem>()
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
    [Category("P8.6i.126")]
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

        SubmoduleInfo top = new("top", module.WorkingDir, bold: true);
        SubmoduleInfo child = new("child", childPath.EnsureTrailingPathSeparator(), bold: false);
        SubmoduleInfoResult result = new()
        {
            Module = module,
            TopProject = top,
        };
        result.OurSubmodules.Add(child);
        result.AllSubmodules.Add(child);
        provider.StatusUpdated += Raise.Event<EventHandler<SubmoduleStatusEventArgs>>(
            provider,
            new SubmoduleStatusEventArgs(result, structureUpdated: true, CancellationToken.None));
        Dispatcher.UIThread.RunJobs();

        SubmoduleTree submoduleTree = form.repoObjectsTree.GetTestAccessor().TreeView.Items
            .Cast<TreeViewItem>()
            .Select(item => item.Tag)
            .OfType<SubmoduleTree>()
            .Single();
        SubmoduleNode childNode = submoduleTree.DescendantsAndSelf().OfType<SubmoduleNode>()
            .Should().ContainSingle(node => !node.IsCurrent)
            .Which;
        childNode.Info.Should().BeSameAs(child);
        childNode.Parent.Should().BeOfType<SubmoduleNode>(
            "a direct submodule must not be nested beneath a duplicate same-named folder");

        MenuItem[] menuItems = flyout.Items.OfType<MenuItem>().ToArray();
        menuItems.Select(item => item.Header!.ToString()).Should().Equal("child", "_Update all submodules");
        menuItems[0].Tag.Should().Be(child.Path);
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
            "ShowSessionRefs",
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
        NativeToolStrip scriptsToolbar = form.FindControl<NativeToolStrip>("ToolStripScripts")!;
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
        GitModuleTestStub.ConfigureEmptyRepositoryReads(module);
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
        bool originalShowGitStatus = AppSettings.ShowGitStatusForArtificialCommits;
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            // This fixture tests navigation, not the asynchronously populated change-count
            // filter. A saved status setting must not skip its artificial destination.
            AppSettings.ShowGitStatusForArtificialCommits = false;
            GitModule module = CreateRepositoryWithInitialCommit();
            ObjectId head = module.GetCurrentCheckout();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "dirty");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(notifier);
            commands.GetService(Arg.Any<Type>())
                .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
            using FormBrowse form = new(commands);
            form.Show();
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == head
                && form.RevisionGrid.GetTestAccessor().Revisions.Items.OfType<GitRevision>()
                    .Any(revision => revision.ObjectId == ObjectId.WorkTreeId));

            // The source starts on HEAD; explicitly establish the artificial-row navigation precondition.
            form.RevisionGrid.SetSelectedRevision(ObjectId.WorkTreeId).Should().BeTrue();
            form.RevisionGrid.SelectedRevision!.IsArtificial.Should().BeTrue();

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
            AppSettings.ShowGitStatusForArtificialCommits = originalShowGitStatus;
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
    [Category("P8.6i.126")]
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
            invalidForm.FindControl<IconSplitButton>("toolStripButtonLevelUp")!.IsEnabled.Should().BeTrue(
                "WinForms enables navigation whenever a non-empty working directory exists");
            invalidForm.checkoutBranchToolStripMenuItem.IsEnabled.Should().BeFalse();
            invalidForm.cleanupToolStripMenuItem.IsEnabled.Should().BeFalse();
            invalidForm.applyPatchToolStripMenuItem.IsEnabled.Should().BeFalse();
            invalidForm.FindControl<Button>("toolStripFileExplorer")!.IsEnabled.Should().BeFalse();
            invalidForm.FindControl<MenuItem>("_viewPullRequestsToolStripMenuItem")!.IsEnabled.Should().BeFalse();
            invalidForm.FindControl<MenuItem>("_createPullRequestsToolStripMenuItem")!.IsEnabled.Should().BeFalse();
            invalidForm.FindControl<MenuItem>("_addUpstreamRemoteToolStripMenuItem")!.IsEnabled.Should().BeFalse();
        }

        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
        using FormBrowse bareForm = new(commands);
        bareForm.repositoryToolStripMenuItem.IsVisible.Should().BeTrue();
        bareForm.commandsToolStripMenuItem.IsVisible.Should().BeTrue();
        bareForm.FindControl<IconSplitButton>("toolStripButtonLevelUp")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("_viewPullRequestsToolStripMenuItem")!.IsEnabled.Should().BeTrue();
        bareForm.FindControl<MenuItem>("_createPullRequestsToolStripMenuItem")!.IsEnabled.Should().BeTrue();
        bareForm.FindControl<MenuItem>("_addUpstreamRemoteToolStripMenuItem")!.IsEnabled.Should().BeTrue();
        bareForm.FindControl<MenuItem>("pullToolStripMenuItem1")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("mergeToolStripMenuItem")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("rebaseToolStripMenuItem1")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("stashChangesToolStripMenuItem")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("cleanupToolStripMenuItem")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("applyPatchToolStripMenuItem")!.IsEnabled.Should().BeFalse();
        bareForm.FindControl<MenuItem>("stashPopToolStripMenuItem")!.IsEnabled.Should().BeTrue(
            "WinForms disables the owning stash split button but leaves this child state unchanged");
        bareForm.FindControl<MenuItem>("manageStashesToolStripMenuItem")!.IsEnabled.Should().BeTrue(
            "WinForms disables the owning stash split button but leaves this child state unchanged");
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

            form.RevisionGrid.SelectedRevision?.ObjectId.Should().Be(selectedId);
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
    public async Task FormBrowse_diff_should_render_the_original_default_git_background_colors()
    {
        const string Before = "public string value = \"before-value\";\n";
        const string After = "public string value = \"after-value\";\n";
        CommitInfoPosition previousPosition = AppSettings.CommitInfoPosition;
        bool previousSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.UseGitColoring.Value.Should().BeTrue("the original appearance settings enable Git coloring by default");
            AppSettings.ReverseGitColoring.Value.Should().BeTrue("the original defaults request themed background colors");
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            module.SetSetting("color.diff.old", string.Empty);
            module.SetSetting("color.diff.new", string.Empty);
            File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), Before);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "baseline-line" }).Should().BeTrue();
            File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), After);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "replace-content" }).Should().BeTrue();
            ObjectId head = module.GetCurrentCheckout();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 800 };
            form.Show();
            try
            {
                await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == head);
                Click(form, form.DiffTabPage, MouseButton.Left);
                GitUI.Editor.FileViewer viewer = form.revisionDiff.FileViewer;
                await WaitUntilAsync(() => form.fileStatusList.SelectedItem?.Item.Name == "tracked.txt"
                    && viewer.TextEditor.Text.Contains("+" + After.TrimEnd('\n'), StringComparison.Ordinal));
                GitUI.Editor.FileViewerInternal native = viewer.FindControl<GitUI.Editor.FileViewerInternal>("internalFileViewer")
                    ?? throw new InvalidOperationException("The production internal diff viewer was not created.");
                GitUI.Editor.Diff.DiffHighlightService service = native.DiffHighlightService
                    ?? throw new InvalidOperationException("The production diff did not create its highlight service.");
                service.Should().BeOfType<GitUI.Editor.Diff.PatchHighlightService>();
                service.UseGitColoring.Should().BeTrue();
                service.UseBackgroundColoring.Should().BeTrue();
                System.Drawing.Color removed = GitExtUtils.GitUI.Theming.AppColor.AnsiTerminalRedBackNormal.GetThemeColor();
                System.Drawing.Color added = GitExtUtils.GitUI.Theming.AppColor.AnsiTerminalGreenBackNormal.GetThemeColor();
                int removedOffset = viewer.TextEditor.Text.IndexOf("-" + Before.TrimEnd('\n'), StringComparison.Ordinal) + 1;
                int addedOffset = viewer.TextEditor.Text.IndexOf("+" + After.TrimEnd('\n'), StringComparison.Ordinal) + 1;
                removedOffset.Should().BeGreaterThan(0);
                addedOffset.Should().BeGreaterThan(0);
                service.TextMarkers.Should().Contain(marker => marker.Offset <= removedOffset && marker.EndOffset > removedOffset
                    && marker.BackColor.HasValue && marker.BackColor.Value.ToArgb() == removed.ToArgb());
                service.TextMarkers.Should().Contain(marker => marker.Offset <= addedOffset && marker.EndOffset > addedOffset
                    && marker.BackColor.HasValue && marker.BackColor.Value.ToArgb() == added.ToArgb());
                service.InlineMarkers.Should().Contain(marker => marker.IsRemoved && marker.Length > 0);
                service.InlineMarkers.Should().Contain(marker => !marker.IsRemoved && marker.Length > 0);
                viewer.TextEditor.Text.Should().NotContain("\u001b");
                form.UpdateLayout();
                using WriteableBitmap frame = form.CaptureRenderedFrame()
                    ?? throw new AssertionException("The real Browse diff must render headlessly.");
                if (Environment.GetEnvironmentVariable("GITEXT_DIFF_EVIDENCE") is { Length: > 0 } evidence)
                {
                    Directory.CreateDirectory(evidence);
                    frame.Save(Path.Combine(evidence, "browse-git-colors.png"), PngBitmapEncoderOptions.Default);
                }

                using MemoryStream encoded = new();
                frame.Save(encoded, PngBitmapEncoderOptions.Default);
                encoded.Position = 0;
                using SKBitmap pixels = SKBitmap.Decode(encoded)
                    ?? throw new AssertionException("The actual diff frame must decode.");
                AvaloniaEdit.Rendering.TextView textView = viewer.TextEditor.TextArea.TextView;
                Point origin = textView.TranslatePoint(default, form)
                    ?? throw new InvalidOperationException("The diff viewport must belong to Browse.");
                Rect viewport = new(origin, textView.Bounds.Size);
                viewport.Width.Should().BeGreaterThan(0);
                viewport.Height.Should().BeGreaterThan(0);
                foreach ((int offset, System.Drawing.Color sourceColor) in new[] { (removedOffset, removed), (addedOffset, added) })
                {
                    int lineNumber = viewer.TextEditor.Document.GetLineByOffset(offset).LineNumber;
                    AvaloniaEdit.Rendering.VisualLine line = textView.VisualLines.Single(line =>
                        line.FirstDocumentLine.LineNumber <= lineNumber && line.LastDocumentLine.LineNumber >= lineNumber);
                    Point blank = new(viewport.Right - 2, origin.Y + line.VisualTop - textView.ScrollOffset.Y + (line.Height / 2));
                    blank.Y.Should().BeGreaterThanOrEqualTo(viewport.Top);
                    blank.Y.Should().BeLessThan(viewport.Bottom);
                    SKColor expected = new(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A);
                    SKColor actual = pixels.GetPixel((int)Math.Floor(blank.X * form.RenderScaling), (int)Math.Floor(blank.Y * form.RenderScaling));
                    actual.Should().Be(expected, "the original fills the whole changed line, including blank space beyond the text");
                }
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = previousPosition;
            AppSettings.ShowSplitViewLayout = previousSplitView;
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
            await WaitUntilAsync(() => tree.GetTestAccessor().TreeView.Items
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
    public void Browse_toolbar_should_overflow_the_whole_stash_command_without_disabling_or_hiding_it()
    {
        using FormBrowse form = new() { Width = 923, Height = 573 };
        form.Show();
        try
        {
            WorkingDirectoryToolStripSplitButton selector = form.FindControl<WorkingDirectoryToolStripSplitButton>(
                "_NO_TRANSLATE_WorkingDir")!;
            selector.Content = new string('x', 200);
            form.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            NativeToolStrip toolbar = form.ToolStripMain;
            IconSplitButton stash = form.FindControl<IconSplitButton>("toolStripSplitStash")!;
            toolbar.GetItemPlacement(stash).Should().Be(NativeToolStripItemPlacement.Overflow);
            stash.GetVisualParent().Should().BeNull("the closed popup does not paint a clipped main-row fragment");
            stash.Opacity.Should().Be(1);
            stash.IsHitTestVisible.Should().BeTrue();

            form.toolStripMainOverflow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            toolbar.IsOverflowOpen.Should().BeTrue();
            stash.GetVisualParent().Should().BeSameAs(toolbar.OverflowContent);
            stash.Bounds.Width.Should().Be(32);
            stash.Opacity.Should().Be(1, "the complete original command is available inside the actual source-shaped popup");
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
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_file_tree_should_clear_empty_selection_and_refill_without_retaining_the_previous_file(bool clearWhileHidden)
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            form.Show();
            try
            {
                await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null);
                GitRevision revision = form.RevisionGrid.GetSelectedRevisions().Single();
                form.CommitInfoTabControl.SelectedItem = form.TreeTabPage;
                await WaitUntilAsync(() =>
                    form.fileTree.DisplayedRevision?.ObjectId == revision.ObjectId
                    && form.fileTree.FileStatusList.SelectedItem?.Item.Name == "tracked.txt"
                    && form.fileTree.FileViewer.TextEditor.Text == "content");

                if (clearWhileHidden)
                {
                    form.CommitInfoTabControl.SelectedItem = form.CommitInfoTabPage;
                    Dispatcher.UIThread.RunJobs();
                }

                ListBox revisions = form.RevisionGrid.GetTestAccessor().Revisions;
                (revisions.SelectedItems
                    ?? throw new AssertionException("The actual revision grid must retain its selection collection.")).Clear();
                form.RevisionGrid.GetSelectedRevisions().Should().BeEmpty();
                if (clearWhileHidden)
                {
                    (form.fileTree.DisplayedRevision?.ObjectId).Should().Be(revision.ObjectId,
                        "the original defers hidden File tree updates until that tab is selected");
                    form.CommitInfoTabControl.SelectedItem = form.TreeTabPage;
                }

                await WaitUntilAsync(() =>
                    form.fileTree.DisplayedRevision is null
                    && form.fileTree.FileStatusList.GitItemStatuses.Count == 0
                    && form.fileTree.FileViewer.TextEditor.Text.Length == 0);
                form.fileTree.FileStatusList.SelectedItem.Should().BeNull();

                form.RevisionGrid.SetSelectedRevision(revision.ObjectId).Should().BeTrue();
                await WaitUntilAsync(() =>
                    form.fileTree.DisplayedRevision?.ObjectId == revision.ObjectId
                    && form.fileTree.FileStatusList.SelectedItem?.Item.Name == "tracked.txt"
                    && form.fileTree.FileViewer.TextEditor.Text == "content");
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
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_script_file_options_should_follow_the_effective_lower_pane_visibility(bool fileTree)
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            form.Show();
            try
            {
                await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null);
                RevisionDiffControl pane = fileTree ? form.fileTree : form.revisionDiff;
                TabItem page = fileTree ? form.TreeTabPage : form.DiffTabPage;
                form.CommitInfoTabControl.SelectedItem = page;
                await WaitUntilAsync(() =>
                    pane.FileStatusList.SelectedItem?.Item.Name == "tracked.txt"
                    && pane.FileViewer.TextEditor.Text.Length > 0);
                pane.IsEffectivelyVisible.Should().BeTrue();
                TopLevel.GetTopLevel(pane).Should().BeSameAs(form);
                RevisionDiffControl inactivePane = fileTree ? form.revisionDiff : form.fileTree;
                TopLevel.GetTopLevel(inactivePane).Should().BeNull(
                    "Avalonia detaches inactive tab contents instead of setting their local visibility false");
                form.GetScriptOptionsProvider().GetValues("SelectedRelativePaths")
                    .Should().Equal("tracked.txt".EscapeForCommandLine());

                Button splitToggle = form.FindControl<Button>("toggleSplitViewLayout")
                    ?? throw new AssertionException("The source split-view command must remain available.");
                Click(form, splitToggle, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                AppSettings.ShowSplitViewLayout.Should().BeFalse();
                form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(page,
                    "hiding the panel does not change the retained selected tab");
                pane.IsEffectivelyVisible.Should().BeFalse();
                (pane.FileStatusList.SelectedItem?.Item.Name).Should().Be("tracked.txt",
                    "source hidden panes retain their data, but do not contribute script options");
                form.GetScriptOptionsProvider().Should().BeSameAs(ScriptOptionsProviderBase.Default);

                Click(form, splitToggle, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                pane.IsEffectivelyVisible.Should().BeTrue();
                TopLevel.GetTopLevel(pane).Should().BeSameAs(form);
                form.GetScriptOptionsProvider().GetValues("SelectedRelativePaths")
                    .Should().Equal("tracked.txt".EscapeForCommandLine());
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
            ConcurrentDictionary<ObjectId, CancellationToken> cancellationTokens = [];
            provider.LoadGpgInfoAsync(Arg.Any<GitRevision?>(), Arg.Any<CancellationToken>()).Returns(callInfo =>
            {
                GitRevision revision = callInfo.Arg<GitRevision>();
                TaskCompletionSource<GpgInfo?> completion = new();
                completions[revision.ObjectId] = completion;
                cancellationTokens[revision.ObjectId] = callInfo.Arg<CancellationToken>();
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
                _ = provider.DidNotReceive().LoadGpgInfoAsync(Arg.Any<GitRevision?>(), Arg.Any<CancellationToken>());

                form.CommitInfoTabControl.SelectedItem = form.GpgInfoTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() => completions.ContainsKey(headRevision.ObjectId));
                form.revisionGpgInfo1.IsKeyboardFocusWithin.Should().BeTrue();
                form.revisionGpgInfo1.FindControl<TextBox>("txtCommitGpgInfo")!.Text.Should().Be(GitUI.TranslatedStrings.LoadingData);
                cancellationTokens[headRevision.ObjectId].CanBeCanceled.Should().BeTrue();

                form.RevisionGrid.SetSelectedRevision(parentId).Should().BeTrue();
                await WaitUntilAsync(() => completions.ContainsKey(parentId));
                cancellationTokens[headRevision.ObjectId].IsCancellationRequested.Should().BeTrue();
                form.revisionGpgInfo1.FindControl<TextBox>("txtCommitGpgInfo")!.Text.Should().Be(GitUI.TranslatedStrings.LoadingData);
                form.revisionGpgInfo1.FindControl<Image>("commitSignPicture")!.IsVisible.Should().BeFalse();
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
            CancellationToken providerCancellationToken = default;
            provider.LoadGpgInfoAsync(Arg.Any<GitRevision?>(), Arg.Any<CancellationToken>()).Returns(callInfo =>
            {
                providerCancellationToken = callInfo.Arg<CancellationToken>();
#pragma warning disable VSTHRD003 // The fake provider deliberately owns an unfinished task to prove browser cancellation does not wait for it.
                return unfinishedLoad.Task;
#pragma warning restore VSTHRD003
            });

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

                providerCancellationToken.IsCancellationRequested.Should().BeTrue();

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
                graphProvider.TryGetToolTip(revisionGrid.SelectedRevision!, x: 1, out string? graphToolTip)
                    .Should().BeTrue();
                graphToolTip.Should().Contain(revisionGrid.SelectedRevision!.Guid);

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
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_revision_menu_underlines_should_survive_real_pointer_reopening(bool english)
    {
        using TranslationTestScope translationScope = new(english ? "English" : "");
        GitModule module = CreateRepositoryWithInitialCommit();
        ObjectId head = module.GetCurrentCheckout();
        string branch = module.GetSelectedBranch();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
        ContextMenu? menu = null;
        try
        {
            form.Show();
            RevisionGridControl grid = form.RevisionGrid;
            ListBox revisions = grid.GetTestAccessor().Revisions;
            await WaitUntilAsync(() => grid.SelectedRevision is { } revision
                && revision.ObjectId == head && revision.Refs.Any(gitRef => gitRef.Name == branch));
            menu = grid.FindControl<ContextMenu>("mainContextMenu")!;
            CopyContextMenuItem copy = grid.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")!;
            Size doubleTapSize = form.GetPlatformSettings()?.GetDoubleTapSize(PointerType.Mouse)
                ?? throw new InvalidOperationException("The platform must define pointer gesture bounds.");
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                form.UpdateLayout();
                ListBoxItem row = (ListBoxItem)revisions.ContainerFromIndex(0)!;
                Point point = row.TranslatePoint(new Point(
                    (row.Bounds.Width / 2) + ((attempt - 1) * (doubleTapSize.Width + 1)), row.Bounds.Height / 2), form)!.Value;
                form.MouseMove(point, RawInputModifiers.None);
                form.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
                form.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
                await WaitUntilAsync(() => menu.IsOpen);
                TopLevel contextPopup = TopLevel.GetTopLevel(menu)!;
                AssertUnderlines(menu, contextPopup, $"{attempt}-context");
                Point copyPoint = copy.TranslatePoint(new Point(copy.Bounds.Width / 2, copy.Bounds.Height / 2), contextPopup)!.Value;
                contextPopup.MouseMove(copyPoint, RawInputModifiers.None);
                await WaitUntilAsync(() => copy.IsSubMenuOpen && copy.Items.OfType<MenuItem>().Any(item => item.Header?.ToString()?.StartsWith("_1:", StringComparison.Ordinal) == true));
                MenuItem first = copy.Items.OfType<MenuItem>().Single(item => item.Header?.ToString()?.StartsWith("_1:", StringComparison.Ordinal) == true);
                AssertUnderlines(copy, TopLevel.GetTopLevel(first)!, $"{attempt}-copy");
                for (int escape = 0; escape < 2 && menu.IsOpen; escape++)
                {
                    contextPopup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
                    contextPopup.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
                }

                await WaitUntilAsync(() => !menu.IsOpen);
            }
        }
        finally
        {
            menu?.Close();
            form.Close();
        }

        return;

        void AssertUnderlines(ItemsControl owner, TopLevel popup, string part)
        {
            using WriteableBitmap? frame = popup.CaptureRenderedFrame();
            frame.Should().NotBeNull();
            if (Environment.GetEnvironmentVariable("GITEXT_REF_MENU_EVIDENCE") is { Length: > 0 } evidence)
            {
                Directory.CreateDirectory(evidence);
                frame!.Save(Path.Combine(evidence, $"underlines-{english}-{part}.png"), PngBitmapEncoderOptions.Default);
            }

            AccessText[] captions = owner.Items.OfType<MenuItem>()
                .SelectMany(item => item.GetVisualDescendants().OfType<AccessText>()).Where(text => text.AccessKey != default).ToArray();
            captions.Should().NotBeEmpty();
            captions.Should().OnlyContain(text => text.ShowAccessKey, "the original keeps menu mnemonic underlines visible on every opening");
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_revision_copy_mnemonics_should_remain_visible_and_execute_after_reopening(bool english)
    {
        using TranslationTestScope translationScope = new(english ? "English" : "");
        FieldInfo clipboardField = typeof(WinFormsShims.ShimHost).GetField("_clipboard", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? previousClipboard = clipboardField.GetValue(null);
        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        GitModule module = CreateRepositoryWithInitialCommit();
        ObjectId head = module.GetCurrentCheckout();
        string branch = module.GetSelectedBranch();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
        ContextMenu? menu = null;
        try
        {
            WinFormsShims.ShimHost.Clipboard = clipboard;
            form.Show();
            RevisionGridControl grid = form.RevisionGrid;
            ListBox revisions = grid.GetTestAccessor().Revisions;
            await WaitUntilAsync(() => grid.SelectedRevision is { } revision
                && revision.ObjectId == head && revision.Refs.Any(gitRef => gitRef.Name == branch));
            menu = grid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("The production revision menu was not created.");
            CopyContextMenuItem copy = grid.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")
                ?? throw new InvalidOperationException("The production Copy menu was not created.");
            Size doubleTapSize = form.GetPlatformSettings()?.GetDoubleTapSize(PointerType.Mouse)
                ?? throw new InvalidOperationException("The platform must define pointer gesture bounds.");
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                form.UpdateLayout();
                ListBoxItem row = (ListBoxItem)revisions.ContainerFromIndex(0)!;
                Point point = row.TranslatePoint(new Point(
                    (row.Bounds.Width / 2) + ((attempt - 1) * (doubleTapSize.Width + 1)), row.Bounds.Height / 2), form)
                    ?? throw new InvalidOperationException("The revision row must belong to Browse.");
                form.MouseMove(point, RawInputModifiers.None);
                form.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
                form.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
                await WaitUntilAsync(() => menu.IsOpen);
                TopLevel contextPopup = TopLevel.GetTopLevel(menu)
                    ?? throw new InvalidOperationException("The context menu must have a real popup root.");
                using WriteableBitmap? contextFrame = contextPopup.CaptureRenderedFrame();
                contextFrame.Should().NotBeNull();
                if (Environment.GetEnvironmentVariable("GITEXT_REF_MENU_EVIDENCE") is { Length: > 0 } contextEvidence)
                {
                    Directory.CreateDirectory(contextEvidence);
                    contextFrame!.Save(Path.Combine(contextEvidence, $"mnemonic-{english}-{attempt}-context.png"), PngBitmapEncoderOptions.Default);
                }

                AccessText copyCaption = copy.GetVisualDescendants().OfType<AccessText>().Single();
                copyCaption.ShowAccessKey.Should().BeTrue("the original keeps context-menu mnemonics visible on every opening");
                contextPopup.KeyPress(Key.C, RawInputModifiers.None, PhysicalKey.C, keySymbol: "c");
                contextPopup.KeyTextInput("c");
                contextPopup.KeyRelease(Key.C, RawInputModifiers.None, PhysicalKey.C, keySymbol: "c");
                await WaitUntilAsync(() => copy.IsSubMenuOpen);
                copy.IsSubMenuOpen.Should().BeTrue("the unique Copy mnemonic must open its submenu without Enter");
                MenuItem first = copy.Items.OfType<MenuItem>().Single(item => item.Header?.ToString()?.StartsWith("_1:", StringComparison.Ordinal) == true);
                TopLevel childPopup = TopLevel.GetTopLevel(first)
                    ?? throw new InvalidOperationException("The Copy action must belong to its actual popup.");
                using WriteableBitmap? childFrame = childPopup.CaptureRenderedFrame();
                childFrame.Should().NotBeNull();
                if (Environment.GetEnvironmentVariable("GITEXT_REF_MENU_EVIDENCE") is { Length: > 0 } childEvidence)
                {
                    Directory.CreateDirectory(childEvidence);
                    childFrame!.Save(Path.Combine(childEvidence, $"mnemonic-{english}-{attempt}-copy.png"), PngBitmapEncoderOptions.Default);
                }

                first.GetVisualDescendants().OfType<AccessText>().Single().ShowAccessKey.Should().BeTrue();
                childPopup.KeyPress(Key.D1, RawInputModifiers.None, PhysicalKey.Digit1, keySymbol: "1");
                childPopup.KeyTextInput("1");
                childPopup.KeyRelease(Key.D1, RawInputModifiers.None, PhysicalKey.Digit1, keySymbol: "1");
                await WaitUntilAsync(() => !menu.IsOpen);
                clipboard.Received(attempt).SetText(branch);
                clipboard.ReceivedCalls().Count(call => call.GetMethodInfo().Name == nameof(WinFormsShims.IClipboard.SetText))
                    .Should().Be(attempt, "one numeric mnemonic must copy once and dismiss the menu");
                grid.GetSelectedRevisions().Should().ContainSingle().Which.ObjectId.Should().Be(head);
            }
        }
        finally
        {
            menu?.Close();
            form.Close();
            clipboardField.SetValue(null, previousClipboard);
        }
    }

    [AvaloniaTest]
    [TestCase(Key.Apps, RawInputModifiers.None)]
    [TestCase(Key.F10, RawInputModifiers.Shift)]
    public async Task FormBrowse_context_menu_keys_should_open_the_real_revision_menu(Key key, RawInputModifiers modifiers)
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        ObjectId head = module.GetCurrentCheckout();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
        ContextMenu? menu = null;
        form.Show();
        try
        {
            RevisionGridControl grid = form.RevisionGrid;
            ListBox revisions = grid.GetTestAccessor().Revisions;
            await WaitUntilAsync(() => revisions.ItemCount == 1 && grid.SelectedRevision?.ObjectId == head);
            menu = grid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("The production revision menu was not created.");
            int doubleClickCount = 0;
            grid.DoubleClickRevision += (_, _) => doubleClickCount++;
            int openingCount = 0;
            menu.Opening += (_, _) => openingCount++;
            PhysicalKey physicalKey = key == Key.Apps ? PhysicalKey.ContextMenu : PhysicalKey.F10;
            Size doubleTapSize = form.GetPlatformSettings()?.GetDoubleTapSize(PointerType.Mouse)
                ?? throw new InvalidOperationException("The platform must define pointer gesture bounds.");
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                form.UpdateLayout();
                ListBoxItem row = (ListBoxItem)revisions.ContainerFromIndex(0)!;
                Point point = row.TranslatePoint(new Point(
                    (row.Bounds.Width / 2) + ((attempt - 1) * (doubleTapSize.Width + 1)), row.Bounds.Height / 2), form)
                    ?? throw new InvalidOperationException("The revision row must belong to Browse.");
                form.MouseMove(point, RawInputModifiers.None);
                form.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
                form.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
                doubleClickCount.Should().Be(0, "each row input must be a single click, not the Commit Diff command");
                revisions.IsKeyboardFocusWithin.Should().BeTrue($"a single row click must focus the grid (attempt {attempt}, double clicks {doubleClickCount})");

                form.KeyPress(key, modifiers, physicalKey, keySymbol: null);
                form.KeyRelease(key, modifiers, physicalKey, keySymbol: null);
                await WaitUntilAsync(() => menu.IsOpen);
                Dispatcher.UIThread.RunJobs();

                menu.IsOpen.Should().BeTrue("the matching key release must not dismiss the context menu");
                openingCount.Should().Be(attempt, "one real key gesture must raise Opening exactly once");
                grid.GetSelectedRevisions().Should().ContainSingle().Which.ObjectId.Should().Be(head);
                menu.Items.OfType<CopyContextMenuItem>().Should().ContainSingle().Which.IsEffectivelyEnabled.Should().BeTrue();
                TopLevel popup = TopLevel.GetTopLevel(menu)
                    ?? throw new InvalidOperationException("The real context menu must have a popup root.");
                using WriteableBitmap? frame = popup.CaptureRenderedFrame();
                frame.Should().NotBeNull("the menu must actually render, not just set IsOpen");
                popup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
                form.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
                await WaitUntilAsync(() => !menu.IsOpen);
                Dispatcher.UIThread.RunJobs();
            }
        }
        finally
        {
            menu?.Close();
            form.Close();
        }
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
            MenuItem navigate = revisionGrid.FindControl<MenuItem>("navigateToolStripMenuItem")
                ?? throw new InvalidOperationException("Navigate menu item was not created.");
            MenuItem openCommitsWithDiffTool = revisionGrid.FindControl<MenuItem>("openCommitsWithDiffToolMenuItem")
                ?? throw new InvalidOperationException("Open-commits-with-difftool item was not created.");
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
                navigate.Items.OfType<MenuItem>().Should().OnlyContain(
                    item => item.IsEnabled,
                    "WinForms leaves revision navigation entries enabled and lets unavailable commands no-op");
                openCommitsWithDiffTool.IsEnabled.Should().BeFalse(
                    "WinForms requires both comparison revisions before enabling the difftool command");
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

            checkoutBranch.Items.Should().HaveCount(3);
            checkoutBranch.Items[1].Should().BeOfType<GitUI.Compat.WinFormsControls.ToolStripSeparator>();
            MenuItem checkoutRemoteMain = checkoutBranch.Items.OfType<MenuItem>()
                .Single(item => item.Header?.ToString() == "origin/main");
            MenuItem checkoutFeature = checkoutBranch.Items.OfType<MenuItem>()
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
            checkoutRemoteMain.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
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
            commands.Received(1).StartCheckoutRemoteBranch(form, "origin/main");
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
            form.MouseMove(new Avalonia.Point(form.Bounds.Width - 1, form.Bounds.Height - 1));
            Dispatcher.UIThread.RunJobs();
            form.MouseMove(point);
            Dispatcher.UIThread.RunJobs();
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
            GetFeatureLabel().IsHighlighted.Should().BeTrue(
                "the original keeps a hovered ref highlighted while its context menu is open");
            otherActions.IsVisible.Should().Be(focused);
            grid.RefreshRealizedRows();
            Dispatcher.UIThread.RunJobs();
            menu.IsOpen.Should().BeTrue("refreshing recycled ref labels must not detach the popup anchor");
            RevisionGridRefRenderer.RefLabelControl refreshedLabel = grid.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .First(item => item.GitRef?.Name == "feature");
            refreshedLabel.IsHighlighted.Should().BeTrue(
                "recycling the row must preserve the logical ref highlight until the context menu closes");
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
            refreshedLabel.IsHighlighted.Should().BeFalse();
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

            RevisionGridRefRenderer.RefLabelControl GetFeatureLabel()
                => grid.GetVisualDescendants()
                    .OfType<RevisionGridRefRenderer.RefLabelControl>()
                    .First(item => item.GitRef?.Name == "feature");
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

    [AvaloniaTest]
    [NonParallelizable]
    [Category("P8.6i.126")]
    public async Task RevisionGrid_control_click_on_selected_artificial_row_should_add_previous_revision()
    {
        bool previousShowArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        FormBrowse? form = null;
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" })
                .Should().BeTrue();
            ObjectId previousRevision = module.RevParse("HEAD~1");
            form = new FormBrowse(new GitUICommands(_serviceContainer, module)) { Width = 1200, Height = 700 };
            form.Show();
            RevisionGridControl grid = form.RevisionGrid;
            await WaitUntilAsync(() => grid.GetTestAccessor().Revisions.Items.Count >= 4);
            grid.SetSelectedRevision(ObjectId.WorkTreeId).Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            ListBoxItem row = grid.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .Single(item => item.DataContext is GitRevision revision && revision.ObjectId == ObjectId.WorkTreeId);
            Control messageCell = row.GetVisualDescendants()
                .OfType<Control>()
                .Single(control => control.Classes.Contains("revision-message-cell"));
            Avalonia.Point point = messageCell.TranslatePoint(
                    new Avalonia.Point(messageCell.Bounds.Width / 2, messageCell.Bounds.Height / 2),
                    form)
                ?? throw new InvalidOperationException("The artificial revision cell is not attached.");

            form.MouseMove(point, RawInputModifiers.Control);
            form.MouseDown(point, MouseButton.Left, RawInputModifiers.Control);
            form.MouseUp(point, MouseButton.Left, RawInputModifiers.Control);
            await WaitUntilAsync(() => grid.GetTestAccessor().Revisions.SelectedItems?.Count == 2);

            grid.GetTestAccessor().Revisions.SelectedItems!
                .Cast<GitRevision>()
                .Select(revision => revision.ObjectId)
                .Should().BeEquivalentTo([ObjectId.WorkTreeId, previousRevision]);
        }
        finally
        {
            form?.Close();
            AppSettings.RevisionGraphShowArtificialCommits = previousShowArtificial;
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task FormBrowse_Copy_paths_should_resolve_commands_through_the_owning_pane(bool fileTree, bool fullPath)
    {
        FieldInfo clipboardField = typeof(WinFormsShims.ShimHost).GetField("_clipboard", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? previousClipboard = clipboardField.GetValue(null);
        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1400, Height = 850 };
        FileStatusList files = fileTree ? form.fileTree.FileStatusList : form.revisionDiff.FileStatusList;
        ContextMenu menu = files.GetTestAccessor().ContextMenu;
        try
        {
            form.Show();
            form.CommitInfoTabControl.SelectedItem = fileTree ? form.TreeTabPage : form.DiffTabPage;
            await form.JoinLoadOperationsForTestAsync().WaitAsync(TimeSpan.FromSeconds(30));
            await WaitUntilAsync(() => files.SelectedItem?.Item.Name == "tracked.txt");
            menu.Open(files);
            Dispatcher.UIThread.RunJobs();
            CopyPathsToolStripMenuItem copy = (CopyPathsToolStripMenuItem)files.GetTestAccessor().CopyPathsMenuItem;
            copy.IsEnabled.Should().BeTrue();
            copy.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            MenuItem action = fullPath ? copy.GetTestAccessor().FullNativeMenuItem : copy.GetTestAccessor().RelativeNativeMenuItem;
            TopLevel popup = TopLevel.GetTopLevel(action) ?? throw new AssertionException("The copy action must be in its open popup.");
            Click(popup, action, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            clipboard.Received(1).SetText(fullPath ? Path.Combine(module.WorkingDir, "tracked.txt") : "tracked.txt");
            clipboard.ReceivedCalls().Should().HaveCount(1);
        }
        finally
        {
            menu.Close();
            form.Close();
            clipboardField.SetValue(null, previousClipboard);
        }
    }

    private GitModule CreateRepositoryWithMerge()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "issue-merge-left" }).Should().BeTrue();
        CommitFiles("left");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "issue-merge-right", "HEAD~1" }).Should().BeTrue();
        CommitFiles("right");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("merge") { "--quiet", "--no-ff", "issue-merge-left", "-m", "merge" }).Should().BeTrue();
        return module;

        void CommitFiles(string prefix)
        {
            File.WriteAllText(Path.Combine(_workingDirectory, $"{prefix}-a.txt"), $"{prefix} a\n");
            File.WriteAllText(Path.Combine(_workingDirectory, $"{prefix}-b.txt"), $"{prefix} b\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", $"{prefix}-a.txt", $"{prefix}-b.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", prefix }).Should().BeTrue();
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

using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.RepoHosting;
using GitUI.Compat;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ColumnHeader = GitUI.Compat.WinFormsControls.ColumnHeader;
using FlowLayoutPanel = GitUI.Compat.WinFormsControls.FlowLayoutPanel;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepositoryHostForkCloneTests
{
    private ServiceContainer _serviceContainer = null!;
    private StubMessageBoxHost _messageBoxHost = null!;
    private WinFormsShims.IMessageBoxHost? _originalMessageBoxHost;
    private StubFolderPicker _folderPicker = null!;
    private WinFormsShims.IFolderPicker? _originalFolderPicker;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _originalMessageBoxHost = TryGetMessageBoxHost();
        _messageBoxHost = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxHost;
        _originalFolderPicker = TryGetFolderPicker();
        _folderPicker = new StubFolderPicker();
        WinFormsShims.ShimHost.FolderPicker = _folderPicker;
    }

    [TearDown]
    public void TearDown()
    {
        WinFormsShims.ShimHost.MessageBoxHost = _originalMessageBoxHost ?? new StubMessageBoxHost();
        WinFormsShims.ShimHost.FolderPicker = _originalFolderPicker ?? new StubFolderPicker();
        _serviceContainer.Dispose();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_preserve_layout_and_translation_identities()
    {
        using ForkAndCloneForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(744);
        form.Height.Should().Be(552);
        TabControl tabControl = form.FindControl<TabControl>("tabControl")!;
        tabControl.ItemCount.Should().Be(2);
        tabControl.Classes.Should().Contain("gitextensions-native-tabs");
        form.FindControl<ListBox>("myReposLV").Should().NotBeNull();
        form.FindControl<ListBox>("searchResultsLV").Should().NotBeNull();
        form.FindControl<ColumnHeader>("columnHeaderMyReposName")!.Text.Should().Be("Name");
        form.FindControl<ColumnHeader>("columnHeaderSearchOwner")!.Text.Should().Be("Owner");
        form.FindControl<NumericUpDown>("depthUpDown")!.Maximum.Should().Be(999);
        form.FindControl<Grid>("tableLayoutPanel2")!.RowDefinitions[1].Height.Value.Should().Be(183);
        form.FindControl<TextBox>("destinationTB")!.Width.Should().Be(294);
        FolderBrowserButton browse = form.FindControl<FolderBrowserButton>("browseForCloneToDirbtn")!;
        browse.Height.Should().Be(23);
        browse.Text.Should().BeEmpty();
        browse.PathShowingControl.Should().BeSameAs(form.FindControl<TextBox>("createDirTB"));
        IconButton browseButton = browse.FindControl<IconButton>("buttonBrowse")!;
        browseButton.Content.Should().Be("_Browse...");
        browseButton.Icon.Should().NotBeNull();
        browseButton.Classes.Should().Contain("gitextensions-native-dialog-action");
        browseButton.MinWidth.Should().Be(100);
        browseButton.MinHeight.Should().Be(25);
        browseButton.Height.Should().Be(25);
        browseButton.VerticalAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Top);
        form.FindControl<TextBox>("createDirTB")!.Width.Should().Be(183);
        form.FindControl<ComboBox>("addUpstreamRemoteAsCB")!.Width.Should().Be(200);
        HeaderedContentControl cloneSetup = form.FindControl<HeaderedContentControl>("cloneSetupGB")!;
        cloneSetup.Classes.Should().Contain("repository-host-clone-group");
        Grid cloneLayout = cloneSetup.Content.Should().BeOfType<Grid>().Subject;
        cloneLayout.Children.Should().HaveCount(12);
        AssertGridBounds(form.FindControl<TextBlock>("label1")!, 7, 16, 104, 15);
        AssertGridBounds(form.FindControl<TextBox>("destinationTB")!, 10, 32, 294, 23);
        AssertGridBounds(browse, 310, 32, 102, 23);
        AssertGridBounds(form.FindControl<TextBlock>("ProtocolLabel")!, 418, 36, 55, 15);
        AssertGridBounds(form.FindControl<ComboBox>("ProtocolDropdownList")!, 470, 32, 121, 23);
        AssertGridBounds(form.FindControl<TextBlock>("createDirectoryLbl")!, 7, 55, 94, 15);
        AssertGridBounds(form.FindControl<TextBox>("createDirTB")!, 10, 71, 183, 23);
        AssertGridBounds(form.FindControl<TextBlock>("label3")!, 211, 55, 140, 15);
        AssertGridBounds(form.FindControl<ComboBox>("addUpstreamRemoteAsCB")!, 212, 71, 200, 23);
        TextBlock cloneInfo = form.FindControl<TextBlock>("cloneInfoText")!;
        cloneInfo.Margin.Should().Be(new Avalonia.Thickness(10, 97, 7, 0));
        cloneInfo.Height.Should().Be(35);
        cloneInfo.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Stretch);
        AssertGridBounds(form.FindControl<TextBlock>("depthLabel")!, 7, 135, 72, 15);
        AssertGridBounds(form.FindControl<NumericUpDown>("depthUpDown")!, 10, 151, 100, 23);
        FlowLayoutPanel footerPanel = form.FindControl<FlowLayoutPanel>("flowLayoutPanel1")!;
        footerPanel.Margin.Should().Be(new Avalonia.Thickness(3));
        Grid footer = footerPanel.Child.Should().BeOfType<Grid>().Subject;
        footer.ColumnDefinitions.Select(column => column.Width.Value).Should().Equal(1, 120, 6, 120, 3);
        Grid.GetColumn(form.FindControl<Button>("cloneBtn")!).Should().Be(1);
        Grid.GetColumn(form.FindControl<Button>("_NO_TRANSLATE_closeBtn")!).Should().Be(3);
        Grid myRepositoriesHeader = (Grid)form.FindControl<ContentControl>("columnHeaderMyReposName")!.Parent!;
        myRepositoriesHeader.ColumnDefinitions.Select(column => column.Width.Value)
            .Should().Equal(180, 45, 50, 45);
        Grid searchHeader = (Grid)form.FindControl<ContentControl>("columnHeaderSearchName")!.Parent!;
        searchHeader.ColumnDefinitions.Select(column => column.Width.Value)
            .Should().Equal(180, 110, 41, 40);
        Grid myRepositoriesLayout = form.FindControl<Grid>("tableLayoutPanel5")!;
        myRepositoriesLayout.Margin.Should().Be(new Avalonia.Thickness(7, 4, 7, 7));
        ((Grid)myRepositoriesLayout.Children[0]).Margin.Should().Be(new Avalonia.Thickness(3));
        Label helpText = form.FindControl<Label>("helpTextLbl")!;
        helpText.Margin.Should().Be(new Avalonia.Thickness(3, 0));
        helpText.Padding.Should().Be(new Avalonia.Thickness(0));
        helpText.HorizontalContentAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Stretch);
        helpText.VerticalContentAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Stretch);

        Grid searchLayout = form.FindControl<Grid>("tableLayoutPanel1")!;
        searchLayout.Margin.Should().Be(new Avalonia.Thickness(7, 4, 7, 7));
        searchLayout.RowDefinitions[0].Height.IsAuto.Should().BeTrue();
        searchLayout.RowDefinitions[1].Height.IsStar.Should().BeTrue();
        searchLayout.RowDefinitions[2].Height.IsAuto.Should().BeTrue();
        FlowLayoutPanel searchPanel = form.FindControl<FlowLayoutPanel>("flowLayoutPanel2")!;
        searchPanel.Height.Should().Be(35);
        searchPanel.Margin.Should().Be(new Avalonia.Thickness(3));
        StackPanel searchActions = searchPanel.Child.Should().BeOfType<StackPanel>().Subject;
        searchActions.Spacing.Should().Be(0);
        foreach (string fieldName in new[] { "searchTB", "searchBtn", "getFromUserBtn" })
        {
            Control control = form.FindControl<Control>(fieldName)!;
            control.Height.Should().Be(23);
            control.Margin.Should().Be(new Avalonia.Thickness(3));
            control.VerticalAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Top);
        }

        TextBlock orLabel = form.FindControl<TextBlock>("orLbl")!;
        orLabel.Classes.Should().Contain("gitextensions-auto-label");
        orLabel.Margin.Should().Be(new Avalonia.Thickness(3, 0));
        Grid repositorySearchResults = form.FindControl<Grid>("tableLayoutPanel3")!;
        repositorySearchResults.Margin.Should().Be(new Avalonia.Thickness(3));
        ((Grid)repositorySearchResults.Children[0]).Margin.Should().Be(new Avalonia.Thickness(3));
        Grid repositoryDescription = form.FindControl<Grid>("tableLayoutPanel4")!;
        repositoryDescription.Margin.Should().Be(new Avalonia.Thickness(3));
        TextBlock description = form.FindControl<TextBlock>("descriptionLbl")!;
        description.Classes.Should().Contain("gitextensions-auto-label");
        description.Margin.Should().Be(new Avalonia.Thickness(3, 0));
        form.FindControl<TextBox>("searchResultItemDescription")!.Margin.Should().Be(new Avalonia.Thickness(3));
        Button openRepository = form.FindControl<Button>("openGitupPageBtn")!;
        openRepository.Width.Should().Be(116);
        openRepository.Height.Should().Be(23);
        openRepository.Margin.Should().Be(new Avalonia.Thickness(3));
        Button fork = form.FindControl<Button>("forkBtn")!;
        fork.Width.Should().Be(150);
        fork.Height.Should().Be(23);
        fork.Margin.Should().Be(new Avalonia.Thickness(3));
        foreach (string buttonName in new[]
                 {
                     "searchBtn",
                     "getFromUserBtn",
                     "openGitupPageBtn",
                     "forkBtn",
                     "cloneBtn",
                     "_NO_TRANSLATE_closeBtn"
                 })
        {
            form.FindControl<Button>(buttonName)!.Classes.Should().Contain("gitextensions-native-dialog-action");
        }

        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "$this", "Text", "Remote repository fork and clone");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "myReposPage", "Text", "My repositories");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "searchReposPage", "Text", "Search for repositories");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposName", "Text", "Name");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposIsFork", "Text", "Is fork");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposForks", "Text", "# Forks");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderMyReposIsPrivate", "Text", "Private");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchName", "Text", "Name");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchOwner", "Text", "Owner");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchIsFork", "Text", "Is fork");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "columnHeaderSearchForks", "Text", "# Forks");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "helpTextLbl", "Text",
            "If you want to fork a repository owned by somebody else, go to the Search for repositories tab.");
        translation.Received(1).AddTranslationItem(
            nameof(ForkAndCloneForm), "_strWillCloneInfo", "Text",
            "Will clone {0} into {1}.\r\nYou can not push unless you are a collaborator. {2}");
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_project_both_native_list_substitutes_with_source_columns()
    {
        using ForkAndCloneForm form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureSurface surface = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, new PixelSize(744, 552));
        CaptureNode[] nodes = Flatten(surface.Root).ToArray();
        CaptureNode owned = nodes.Single(node => node.FieldName == "myReposLV");
        CaptureNode search = nodes.Single(node => node.FieldName == "searchResultsLV");
        CaptureNode depth = nodes.Single(node => node.FieldName == "depthUpDown");
        CaptureNode browse = nodes.Single(node => node.FieldName == "browseForCloneToDirbtn");
        CaptureNode tabs = nodes.Single(node => node.FieldName == "tabControl");
        CaptureNode layout = nodes.Single(node => node.FieldName == "tableLayoutPanel2");

        owned.BorderStyle.Should().Be("Fixed3D");
        owned.Columns.Select(column => column.FieldName).Should().Equal(
            "columnHeaderMyReposName",
            "columnHeaderMyReposIsFork",
            "columnHeaderMyReposForks",
            "columnHeaderMyReposIsPrivate");
        search.Columns.Select(column => column.FieldName).Should().Equal(
            "columnHeaderSearchName",
            "columnHeaderSearchOwner",
            "columnHeaderSearchIsFork",
            "columnHeaderSearchForks");
        owned.Columns.Select(column => column.WidthDip).Should().Equal(180, 45, 50, 45);
        search.Columns.Select(column => column.WidthDip).Should().Equal(180, 110, 41, 40);
        search.Columns.Should().OnlyContain(column => column.Visible);
        depth.TabStop.Should().BeTrue();
        depth.BorderWidthDip.Should().BeNull();
        browse.TabStop.Should().BeTrue();
        browse.BorderStyle.Should().Be("None");
        tabs.Focused.Should().BeTrue();
        owned.Focused.Should().BeFalse();
        layout.BorderStyle.Should().Be("None");
        layout.Font.Should().NotBeNull();
        nodes.Where(node => node.FieldName?.StartsWith("columnHeader", StringComparison.Ordinal) == true)
            .Should().BeEmpty();
        nodes.Where(node => node != surface.Root
                            && node.FieldName is null
                            && node.Name is null)
            .Should().BeEmpty("unnamed Avalonia layout panels are not WinForms product controls");
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_project_native_tab_pages_and_hidden_descendants()
    {
        using ForkAndCloneForm form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureNode[] initialNodes = ReadNodes();
        CaptureNode tabs = initialNodes.Single(node => node.FieldName == "tabControl");
        CaptureNode ownedPage = initialNodes.Single(node => node.FieldName == "myReposPage");
        CaptureNode searchPage = initialNodes.Single(node => node.FieldName == "searchReposPage");
        CaptureNode ownedLayout = initialNodes.Single(node => node.FieldName == "tableLayoutPanel5");
        CaptureNode helpText = initialNodes.Single(node => node.FieldName == "helpTextLbl");

        tabs.BoundsDip.Should().Be(new CaptureRectangleF { X = 3, Y = 3, Width = 738, Height = 323 });
        tabs.Dock.Should().Be("Fill");
        tabs.TabStop.Should().BeTrue();
        ownedPage.BoundsDip.Should().Be(new CaptureRectangleF { X = 4, Y = 30, Width = 730, Height = 289 });
        ownedPage.Padding.Dip.Should().Be(new CaptureThicknessF { Left = 3, Top = 3, Right = 3, Bottom = 3 });
        ownedPage.Visible.Should().BeTrue();
        ownedLayout.BoundsDip.Should().Be(new CaptureRectangleF { X = 3, Y = 3, Width = 724, Height = 283 });
        helpText.BoundsDip.Should().Be(new CaptureRectangleF { X = 509, Y = 0, Width = 212, Height = 283 });
        Grid ownedGrid = form.FindControl<Grid>("tableLayoutPanel5")!;
        ownedGrid.ColumnDefinitions[0].Width.IsAbsolute.Should().BeTrue();
        ownedGrid.ColumnDefinitions[1].Width.IsAbsolute.Should().BeTrue();
        ownedGrid.ColumnDefinitions.Select(column => column.Width.Value).Should().Equal(506, 218);
        searchPage.Visible.Should().BeFalse();
        Flatten(searchPage).Should().OnlyContain(node => node.Visible != true);

        TabControl tabControl = form.FindControl<TabControl>("tabControl")!;
        tabControl.SelectedItem = form.FindControl<TabItem>("searchReposPage");
        Dispatcher.UIThread.RunJobs();

        CaptureNode[] searchNodes = ReadNodes();
        CaptureNode nowHiddenOwnedPage = searchNodes.Single(node => node.FieldName == "myReposPage");
        CaptureNode selectedSearchPage = searchNodes.Single(node => node.FieldName == "searchReposPage");
        nowHiddenOwnedPage.Visible.Should().BeFalse();
        Flatten(nowHiddenOwnedPage).Should().OnlyContain(node => node.Visible != true);
        selectedSearchPage.Visible.Should().BeTrue();
        selectedSearchPage.BoundsDip.Should().Be(new CaptureRectangleF { X = 4, Y = 30, Width = 730, Height = 289 });

        form.Close();

        CaptureNode[] ReadNodes()
            => Flatten(
                    new AvaloniaControlTreeReader(form, renderScale: 1)
                        .ReadPrimary(form, new PixelSize(744, 552)).Root)
                .ToArray();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_anchor_clone_information_to_both_horizontal_edges()
    {
        using ForkAndCloneForm form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();
        TextBlock cloneInfo = form.FindControl<TextBlock>("cloneInfoText")!;

        cloneInfo.Bounds.Width.Should().Be(719);
        form.Width = 844;
        Dispatcher.UIThread.RunJobs();

        cloneInfo.Bounds.Width.Should().Be(819);
        form.Close();
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_load_sort_and_select_owned_repositories()
    {
        IHostedRepository beta = CreateRepository("beta", owner: "me", isFork: false);
        IHostedRepository alpha = CreateRepository("alpha", owner: "me", isFork: true);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([beta, alpha]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.Combine(Path.GetTempPath(), "fork-clone");

        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.MyRepositoryNames.Should().Equal("alpha", "beta");
        accessor.CloneEnabled.Should().BeFalse();
        accessor.CreateDirectory.Should().BeEmpty();

        accessor.SelectMyRepository(0);
        accessor.CloneEnabled.Should().BeTrue();
        accessor.CreateDirectory.Should().Be("alpha");
        accessor.TargetDirectory.Should().Be(Path.Combine(accessor.Destination, "alpha"));
        accessor.CloneInfo.Should().Contain("https://example.test/alpha.git");
        accessor.CloneInfo.Should().Contain("push access");
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
    {
        yield return root;
        foreach (CaptureNode child in root.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static void AssertGridBounds(Control control, double left, double top, double width, double height)
    {
        control.Margin.Should().Be(new Avalonia.Thickness(left, top, 0, 0));
        control.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Left);
        control.VerticalAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Top);
        if (width > 0)
        {
            control.Width.Should().Be(width);
        }

        control.Height.Should().Be(height);
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_autosize_content_columns_instead_of_reusing_capture_widths()
    {
        IHostedRepository repository = CreateRepository(
            "repository-name-that-is-wider-than-the-Designer-column",
            owner: "owner-name-that-is-wider-than-the-Designer-column",
            isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([repository]);
        host.SearchForRepository("wide").Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Grid myHeader = (Grid)form.FindControl<ContentControl>("columnHeaderMyReposName")!.Parent!;
        myHeader.ColumnDefinitions[0].Width.Value.Should().BeGreaterThan(180);

        accessor.StartSearch("wide", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Grid searchHeader = (Grid)form.FindControl<ContentControl>("columnHeaderSearchName")!.Parent!;
        searchHeader.ColumnDefinitions[0].Width.Value.Should().BeGreaterThan(180);
        searchHeader.ColumnDefinitions[1].Width.Value.Should().BeGreaterThan(110);
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_search_sort_and_show_repository_details()
    {
        IHostedRepository zulu = CreateRepository("zulu", owner: "other", isFork: false);
        IHostedRepository alpha = CreateRepository("alpha", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("query").Returns([zulu, alpha]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.Combine(Path.GetTempPath(), "fork-clone");

        await accessor.SearchAsync("query", byUser: false).WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SearchResultNames.Should().Equal("alpha", "zulu");

        accessor.SelectSearchResult(0);
        accessor.CloneEnabled.Should().BeTrue();
        accessor.Description.Should().Be("alpha description");
        accessor.CloneInfo.Should().Contain("can not push");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_preserve_source_query_whitespace_rules()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("  repository  ").Returns([]);
        host.GetRepositoriesOfUser("user").Returns([]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("  repository  ", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.StartSearch("  user  ", byUser: true);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        host.Received(1).SearchForRepository("  repository  ");
        host.Received(1).GetRepositoriesOfUser("user");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_publish_only_the_latest_search_result()
    {
        TaskCompletionSource firstSearchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstSearch = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IHostedRepository stale = CreateRepository("stale", owner: "other", isFork: false);
        IHostedRepository current = CreateRepository("current", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("first").Returns(_ =>
        {
            firstSearchStarted.TrySetResult();
            releaseFirstSearch.Task.GetAwaiter().GetResult();
            return [stale];
        });
        host.SearchForRepository("second").Returns([current]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("first", byUser: false);
        await firstSearchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.StartSearch("second", byUser: false);
        releaseFirstSearch.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SearchResultNames.Should().Equal("current");
        accessor.SearchEnabled.Should().BeTrue();
        accessor.GetFromUserEnabled.Should().BeTrue();
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_keep_get_from_user_available_during_repository_search()
    {
        TaskCompletionSource searchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseSearch = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("pending").Returns(_ =>
        {
            searchStarted.TrySetResult();
            releaseSearch.Task.GetAwaiter().GetResult();
            return Array.Empty<IHostedRepository>();
        });
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("pending", byUser: false);
        await searchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SearchEnabled.Should().BeFalse();
        accessor.GetFromUserEnabled.Should().BeTrue();

        releaseSearch.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SearchEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_retain_searching_row_and_description_on_source_failure_states()
    {
        IHostedRepository repository = CreateRepository("project", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("project").Returns([repository]);
        host.SearchForRepository("failure").Returns(_ => throw new InvalidOperationException("search failed"));
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();

        await accessor.SearchAsync("project", byUser: false).WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectSearchResult(0);
        accessor.Description.Should().Be("project description");

        accessor.StartSearch("failure", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SearchResultNames.Should().Equal(" : SEARCHING : ");
        accessor.Description.Should().Be("project description");
        accessor.ForkEnabled.Should().BeFalse();
        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Search failed!" + Environment.NewLine + "search failed");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_report_owned_repository_load_failure_in_the_help_text()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns(_ => throw new InvalidOperationException("owned repositories failed"));
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.MyRepositoryNames.Should().BeEmpty();
        accessor.HelpText.Should().Contain("Failed to get repositories");
        accessor.HelpText.Should().Contain("owned repositories failed");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_preserve_the_selected_protocol_across_repository_changes()
    {
        IHostedRepository first = CreateRepository("first", owner: "me", isFork: false);
        IHostedRepository second = CreateRepository("second", owner: "me", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([first, second]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();
        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SelectMyRepository(0);
        accessor.SelectedProtocol = GitProtocol.Ssh;
        accessor.SelectMyRepository(1);

        accessor.SelectedProtocol.Should().Be(GitProtocol.Ssh);
        second.Received().CloneProtocol = GitProtocol.Ssh;
        accessor.CloneInfo.Should().Contain("second.git");
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_preserve_target_directory_and_depth_rules()
    {
        using ForkAndCloneForm form = CreateForm(Substitute.For<IRepositoryHostPlugin>());
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.Combine(Path.GetTempPath(), "destination");
        accessor.CreateDirectory = "project";

        accessor.TargetDirectory.Should().Be(Path.Combine(accessor.Destination, "project"));
        accessor.Depth.Should().BeNull();

        accessor.SetDepth(42);
        accessor.Depth.Should().Be(42);

        accessor.CreateDirectory = " project ";
        accessor.TargetDirectory.Should().Be(Path.Combine(accessor.Destination, " project "));
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_report_invalid_native_path_characters_during_validation()
    {
        IHostedRepository repository = CreateRepository("project", owner: "me", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetMyRepos().Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();
        await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectMyRepository(0);
        accessor.CloneEnabled.Should().BeTrue();

        accessor.CreateDirectory = "project" + Path.GetInvalidPathChars()[0];
        (bool destinationInvalid, bool createDirectoryInvalid) = accessor.ValidatePaths();

        accessor.CloneEnabled.Should().BeTrue();
        destinationInvalid.Should().BeFalse();
        createDirectoryInvalid.Should().BeTrue();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_report_an_empty_clone_destination()
    {
        using ForkAndCloneForm form = CreateForm(Substitute.For<IRepositoryHostPlugin>());
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = string.Empty;
        accessor.CreateDirectory = "project";

        accessor.GetTargetDirectoryWithValidation().Should().BeNull();

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Clone folder can not be empty");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_route_user_not_found_and_restore_search_actions()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetRepositoriesOfUser("missing-user")
            .Returns(_ => throw new InvalidOperationException("HTTP 404"));
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.StartSearch("missing-user", byUser: true);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("User not found!");
        accessor.SearchEnabled.Should().BeTrue();
        accessor.GetFromUserEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_fork_the_selected_repository_and_reload_owned_repositories()
    {
        IHostedRepository repository = CreateRepository("project", owner: "other", isFork: false);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("project").Returns([repository]);
        host.GetMyRepos().Returns([repository]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();

        accessor.StartSearch("project", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectSearchResult(0);
        accessor.ForkSelectedRepository();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        repository.Received(1).Fork();
        accessor.IsMyRepositoriesTabSelected.Should().BeTrue();
        accessor.MyRepositoryNames.Should().Equal("project");
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_report_a_fork_failure_and_reload_owned_repositories()
    {
        IHostedRepository repository = CreateRepository("project", owner: "other", isFork: false);
        repository.When(candidate => candidate.Fork())
            .Do(_ => throw new InvalidOperationException("provider failed"));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.SearchForRepository("project").Returns([repository]);
        host.GetMyRepos().Returns([]);
        using ForkAndCloneForm form = CreateForm(host);
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Destination = Path.GetTempPath();

        accessor.StartSearch("project", byUser: false);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectSearchResult(0);
        accessor.ForkSelectedRepository();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to fork:" + Environment.NewLine + "provider failed");
        accessor.IsMyRepositoriesTabSelected.Should().BeTrue();
        host.Received(1).GetMyRepos();
    }

    [AvaloniaTest]
    public void ForkAndCloneForm_should_route_folder_picker_accept_and_cancel()
    {
        using ForkAndCloneForm form = CreateForm(Substitute.For<IRepositoryHostPlugin>());
        ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
        string initialDirectory = Path.Combine(Path.GetTempPath(), "initial-clone-root");
        string selectedDirectory = Path.Combine(Path.GetTempPath(), "selected-clone-root");
        accessor.Destination = initialDirectory;
        _folderPicker.Result = selectedDirectory;

        accessor.BrowseForCloneDirectory();

        accessor.Destination.Should().Be(selectedDirectory);
        _folderPicker.RequestedPaths.Should().Equal(initialDirectory);

        _folderPicker.Result = null;
        form.FindControl<FolderBrowserButton>("browseForCloneToDirbtn")!
            .FindControl<Button>("buttonBrowse")!
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        accessor.Destination.Should().Be(selectedDirectory);
        _folderPicker.RequestedPaths.Should().Equal(initialDirectory, selectedDirectory);

        accessor.Destination = string.Empty;
        accessor.BrowseForCloneDirectory();

        string expectedRoot = Path.GetPathRoot(Environment.CurrentDirectory) ?? Environment.CurrentDirectory;
        _folderPicker.RequestedPaths.Should().Equal(initialDirectory, selectedDirectory, expectedRoot);
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_clone_and_add_the_selected_upstream_remote()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RepositoryHostClone-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string destinationRoot = Path.Combine(root, "clones");
        string targetDirectory = Path.Combine(destinationRoot, "project");
        Directory.CreateDirectory(destinationRoot);
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "main");
            IGitUICommands commands = CreateCommands(sourceModule);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(call => sourceModule.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));
            IGitModule? selectedModule = null;
            IHostedRepository repository = CreateRepository("project", owner: "me", isFork: true);
            repository.CloneUrl.Returns(sourceDirectory);
            repository.ParentOwner.Returns("parent");
            repository.ParentUrl.Returns("https://example.test/parent/project.git");
            IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
            host.GetMyRepos().Returns([repository]);
            using ForkAndCloneForm form = new(
                commands,
                host,
                (_, args) => selectedModule = args.GitModule);
            ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.Destination = destinationRoot;
            await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.SelectMyRepository(0);
            accessor.UpstreamRemoteName = "upstream";

            accessor.CloneSelectedRepository();

            selectedModule.Should().NotBeNull();
            Path.TrimEndingDirectorySeparator(selectedModule!.WorkingDir)
                .Should().Be(Path.TrimEndingDirectorySeparator(targetDirectory));
            selectedModule.GetCurrentCheckout().Should().Be(sourceModule.GetCurrentCheckout());
            IReadOnlyList<Remote> remotes = await selectedModule.GetRemotesAsync();
            Remote origin = remotes.Should().ContainSingle(remote => remote.Name == "origin").Which;
            Path.GetFullPath(origin.FetchUrl).Should().Be(Path.GetFullPath(sourceDirectory));
            remotes.Should().Contain(remote => remote.Name == "upstream" && remote.FetchUrl == repository.ParentUrl);
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public async Task ForkAndCloneForm_should_stop_when_the_clone_process_fails()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.RepositoryHostCloneFailure-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string destinationRoot = Path.Combine(root, "clones");
        Directory.CreateDirectory(destinationRoot);
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "main");
            IGitUICommands commands = CreateCommands(sourceModule);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(false);
            bool moduleChanged = false;
            IHostedRepository repository = CreateRepository("project", owner: "me", isFork: false);
            repository.CloneUrl.Returns(sourceDirectory);
            IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
            host.GetMyRepos().Returns([repository]);
            using ForkAndCloneForm form = new(
                commands,
                host,
                (_, _) => moduleChanged = true);
            ForkAndCloneForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.Destination = destinationRoot;
            await accessor.LoadMyRepositoriesAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.SelectMyRepository(0);

            accessor.CloneSelectedRepository();

            moduleChanged.Should().BeFalse();
            Directory.Exists(Path.Combine(destinationRoot, "project")).Should().BeFalse();
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public void StartCloneForkFromHoster_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCloneForkFromHoster(owner: null, host, gitModuleChanged: null);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    private ForkAndCloneForm CreateForm(IRepositoryHostPlugin host)
    {
        host.Name.Returns("Test host");
        IGitUICommands commands = CreateCommands(Substitute.For<IGitModule>());
        return new ForkAndCloneForm(commands, host, gitModuleChanged: null);
    }

    private IGitUICommands CreateCommands(IGitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        return commands;
    }

    private GitModule CreateCommittedRepository(string workingDirectory, string branch)
    {
        Directory.CreateDirectory(workingDirectory);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", branch }).Should().BeTrue();
        return module;
    }

    private static IHostedRepository CreateRepository(string name, string owner, bool isFork)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.Name.Returns(name);
        repository.Owner.Returns(owner);
        repository.Description.Returns($"{name} description");
        repository.IsAFork.Returns(isFork);
        repository.IsPrivate.Returns(false);
        repository.Forks.Returns(3);
        repository.Homepage.Returns($"https://example.test/{name}");
        repository.CloneUrl.Returns($"https://example.test/{name}.git");
        repository.SupportedCloneProtocols.Returns([GitProtocol.Https, GitProtocol.Ssh]);
        repository.CloneProtocol.Returns(GitProtocol.Https);
        return repository;
    }

    private static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
    {
        try
        {
            return WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static WinFormsShims.IFolderPicker? TryGetFolderPicker()
    {
        try
        {
            return WinFormsShims.ShimHost.FolderPicker;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private sealed class StubFolderPicker : WinFormsShims.IFolderPicker
    {
        public string? Result { get; set; }

        public List<string?> RequestedPaths { get; } = [];

        public string? PickFolder(WinFormsShims.IWin32Window? owner, string? selectedPath)
        {
            RequestedPaths.Add(selectedPath);
            return Result;
        }
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return WinFormsShims.DialogResult.OK;
        }
    }
}

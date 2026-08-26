using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using FlowLayoutPanel = GitUI.Compat.WinFormsControls.FlowLayoutPanel;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepositoryHostPullRequestTests
{
    private const string BaseSha = "1111111111111111111111111111111111111111";
    private const string HeadSha = "2222222222222222222222222222222222222222";
    private const string Diff = """
        diff --git a/src/file.txt b/src/file.txt
        index 1111111..2222222 100644
        --- a/src/file.txt
        +++ b/src/file.txt
        @@ -1 +1 @@
        -old
        +new
        """;

    private ServiceContainer _serviceContainer = null!;
    private string _originalApplicationExecutablePath = null!;
    private StubMessageBoxHost _messageBoxHost = null!;
    private WinFormsShims.IMessageBoxHost? _originalMessageBoxHost;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        _originalApplicationExecutablePath = settingsAccessor.ApplicationExecutablePath;
        settingsAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");

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
    }

    [TearDown]
    public void TearDown()
    {
        WinFormsShims.ShimHost.MessageBoxHost = _originalMessageBoxHost ?? new StubMessageBoxHost();
        AppSettings.GetTestAccessor().ApplicationExecutablePath = _originalApplicationExecutablePath;
        _serviceContainer.Dispose();
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_preserve_layout_and_translation_identities()
    {
        using ViewPullRequestsForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(754);
        form.Height.Should().Be(511);
        form.FindControl<TabControl>("tabControl1")!.ItemCount.Should().Be(2);
        form.FindControl<FileStatusList>("_fileStatusList").Should().NotBeNull();
        form.FindControl<GitUI.Editor.FileViewer>("_diffViewer").Should().NotBeNull();
        form.FindControl<GitUI.SpellChecker.EditNetSpell>("_postCommentText").Should().NotBeNull();

        FlowLayoutPanel repositorySelectorPanel = form.FindControl<FlowLayoutPanel>("flowLayoutPanel2")!;
        repositorySelectorPanel.Margin.Should().Be(new Avalonia.Thickness(2));
        StackPanel repositorySelector = repositorySelectorPanel.Child.Should().BeOfType<StackPanel>().Subject;
        repositorySelector.Spacing.Should().Be(0);
        TextBlock chooseRepository = form.FindControl<TextBlock>("_chooseRepo")!;
        chooseRepository.Width.Should().Be(106);
        chooseRepository.Height.Should().Be(15);
        chooseRepository.Margin.Should().Be(new Avalonia.Thickness(3, 0));
        ComboBox hostedRepository = form.FindControl<ComboBox>("_selectHostedRepoCB")!;
        hostedRepository.Width.Should().Be(258);
        hostedRepository.Height.Should().Be(23);
        hostedRepository.Margin.Should().Be(new Avalonia.Thickness(3));
        form.FindControl<Grid>("tableLayoutPanel2")!.RowDefinitions[0].Height.Value.Should().Be(33);
        Grid pullRequestLayout = form.FindControl<Grid>("tableLayoutPanel3")!;
        pullRequestLayout.Margin.Should().Be(new Avalonia.Thickness(2));
        pullRequestLayout.ColumnDefinitions[1].Width.IsAuto.Should().BeTrue();
        ((Grid)pullRequestLayout.Children[0]).Margin.Should().Be(new Avalonia.Thickness(3));
        Grid pullRequestHeader = (Grid)form.FindControl<ContentControl>("columnHeaderId")!.Parent!;
        pullRequestHeader.Margin.Should().Be(new Avalonia.Thickness(0, 0, 4, 0));
        pullRequestHeader.ColumnDefinitions[4].Width.IsStar.Should().BeTrue();
        pullRequestHeader.ColumnDefinitions
            .Where((_, index) => index != 4)
            .Should().OnlyContain(column => column.Width.IsAbsolute && column.Width.Value > 0);
        FlowLayoutPanel pullRequestActionsPanel = form.FindControl<FlowLayoutPanel>("flowLayoutPanel3")!;
        pullRequestActionsPanel.Width.Should().Be(160);
        pullRequestActionsPanel.Margin.Should().Be(new Avalonia.Thickness(2));
        StackPanel pullRequestActions = pullRequestActionsPanel.Child.Should().BeOfType<StackPanel>().Subject;
        foreach (string buttonName in new[] { "_fetchBtn", "_addAndFetchBtn", "_closePullRequestBtn" })
        {
            Button button = form.FindControl<Button>(buttonName)!;
            button.Width.Should().Be(155);
            button.Height.Should().Be(29);
            button.Margin.Should().Be(new Avalonia.Thickness(3));
            button.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Left);
        }

        TabControl tabControl = form.FindControl<TabControl>("tabControl1")!;
        tabControl.Classes.Should().Contain("gitextensions-native-tabs");
        tabControl.Margin.Should().Be(new Avalonia.Thickness(0));
        tabControl.Padding.Should().Be(new Avalonia.Thickness(0));
        TabItem diffTab = form.FindControl<TabItem>("tabPage1")!;
        diffTab.Height.Should().Be(28);
        diffTab.MinHeight.Should().Be(28);
        diffTab.Padding.Should().Be(new Avalonia.Thickness(8, 2));
        TabItem commentsTab = form.FindControl<TabItem>("tabPage2")!;
        commentsTab.Height.Should().Be(28);
        commentsTab.MinHeight.Should().Be(28);
        commentsTab.Padding.Should().Be(new Avalonia.Thickness(8, 2));
        Grid diffLayout = form.FindControl<Grid>("splitContainer3")!;
        diffLayout.Margin.Should().Be(new Avalonia.Thickness(6, 2, 6, 6));
        diffLayout.RowDefinitions[0].Height.IsStar.Should().BeTrue();
        diffLayout.RowDefinitions[0].Height.Value.Should().Be(112);
        diffLayout.RowDefinitions[1].Height.Value.Should().Be(6);
        diffLayout.RowDefinitions[2].Height.IsStar.Should().BeTrue();
        diffLayout.RowDefinitions[2].Height.Value.Should().Be(203);
        diffLayout.Children.Should().Contain(form.FindControl<FileStatusList>("_fileStatusList")!);
        diffLayout.Children.Should().Contain(form.FindControl<GitUI.Editor.FileViewer>("_diffViewer")!);

        Grid commentsLayout = form.FindControl<Grid>("tableLayoutPanel1")!;
        commentsLayout.Margin.Should().Be(new Avalonia.Thickness(6, 2, 6, 6));
        commentsLayout.RowDefinitions[0].Height.IsStar.Should().BeTrue();
        commentsLayout.RowDefinitions[1].Height.Value.Should().Be(80);
        commentsLayout.RowDefinitions[2].Height.Value.Should().Be(33);
        ListBox discussion = form.FindControl<ListBox>("_discussionWB")!;
        discussion.Margin.Should().Be(new Avalonia.Thickness(3));
        discussion.Classes.Should().Contain("repository-host-discussion");
        discussion.Focusable.Should().BeTrue();
        discussion.BorderThickness.Should().Be(new Avalonia.Thickness(0));
        discussion.Padding.Should().Be(new Avalonia.Thickness(0));
        form.FindControl<GitUI.SpellChecker.EditNetSpell>("_postCommentText")!.Margin.Should().Be(new Avalonia.Thickness(2));
        FlowLayoutPanel commentActionsPanel = form.FindControl<FlowLayoutPanel>("flowLayoutPanel1")!;
        commentActionsPanel.Margin.Should().Be(new Avalonia.Thickness(2));
        DockPanel commentActions = commentActionsPanel.Child.Should().BeOfType<DockPanel>().Subject;
        commentActions.LastChildFill.Should().BeFalse();
        Button refreshComments = form.FindControl<Button>("_refreshCommentsBtn")!;
        refreshComments.Width.Should().Be(100);
        refreshComments.Height.Should().Be(23);
        refreshComments.Margin.Should().Be(new Avalonia.Thickness(3));
        DockPanel.GetDock(refreshComments).Should().Be(Dock.Right);
        Button postComment = form.FindControl<Button>("_postComment")!;
        postComment.Width.Should().Be(131);
        postComment.Height.Should().Be(23);
        postComment.Margin.Should().Be(new Avalonia.Thickness(3));
        DockPanel.GetDock(postComment).Should().Be(Dock.Right);
        foreach (string buttonName in new[]
                 {
                     "_fetchBtn",
                     "_addAndFetchBtn",
                     "_closePullRequestBtn",
                     "_refreshCommentsBtn",
                     "_postComment"
                 })
        {
            form.FindControl<Button>(buttonName)!.Classes.Should().Contain("gitextensions-native-dialog-action");
        }

        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "$this", "Text", "View Pull Requests");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "tabPage1", "Text", "Diffs");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "tabPage2", "Text", "Comments");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderHeading", "Text", "Heading");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderBy", "Text", "By");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderCreated", "Text", "Created");
        translation.Received(1).AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderBranch", "Text", "Will be fetched to branch");
        translation.DidNotReceive().AddTranslationItem(
            nameof(ViewPullRequestsForm), "columnHeaderId", Arg.Any<string>(), Arg.Any<string>());
    }

    [AvaloniaTest]
    public async Task AsyncLoader_should_raise_the_source_loading_error_event_on_the_ui_thread()
    {
        GitUI.CommandsDialogs.RepoHosting.AsyncLoader loader = new();
        InvalidOperationException expected = new("provider failed");
        Exception? observed = null;
        bool raisedOnMainThread = false;
        loader.LoadingError += (_, args) =>
        {
            observed = args.Exception;
            raisedOnMainThread = loader.JoinableTaskFactory.Context.IsOnMainThread;
        };

        loader.FileAndForget(() => Task.FromException(expected));
        await loader.JoinPendingOperationsAsync(CancellationToken.None);

        observed.Should().BeSameAs(expected);
        raisedOnMainThread.Should().BeTrue();
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_project_the_native_list_substitute_as_one_column_list()
    {
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        form.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureSurface surface = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, new PixelSize(754, 511));
        CaptureNode[] nodes = Flatten(surface.Root).ToArray();
        CaptureNode list = nodes.Single(node => node.FieldName == "_pullRequestsList");
        CaptureNode fetch = nodes.Single(node => node.FieldName == "_fetchBtn");
        CaptureNode hostedRepository = nodes.Single(node => node.FieldName == "_selectHostedRepoCB");
        CaptureNode commentsPage = nodes.Single(node => node.FieldName == "tabPage2");
        CaptureNode commentsLayout = nodes.Single(node => node.FieldName == "tableLayoutPanel1");

        list.BoundsDip.Should().Be(new CaptureRectangleF { X = 3, Y = 3, Width = 580, Height = 103 });
        list.ClientSizeDip.Should().Be(new CaptureSizeF { Width = 576, Height = 99 });
        list.BorderStyle.Should().Be("Fixed3D");
        list.Anchor.Should().Equal("Top", "Bottom", "Left", "Right");
        list.Dock.Should().Be("None");
        list.AutoSize.Should().BeFalse();
        list.TabStop.Should().BeTrue();
        fetch.Colors.Background.Should().Be("#00FFFFFF");
        hostedRepository.Focused.Should().BeTrue();
        fetch.Focused.Should().BeFalse();
        commentsPage.BorderStyle.Should().Be("None");
        commentsLayout.BorderStyle.Should().Be("None");
        commentsLayout.Font.Should().NotBeNull();
        list.Columns.Select(column => column.FieldName).Should().Equal(
            "columnHeaderId",
            "columnHeaderHeading",
            "columnHeaderBy",
            "columnHeaderCreated",
            "columnHeaderBranch");
        list.Columns.Select(column => column.HeaderText).Should().Equal(
            "#",
            "Heading",
            "By",
            "Created",
            "Will be fetched to branch");
        nodes.Where(node => node.FieldName?.StartsWith("columnHeader", StringComparison.Ordinal) == true)
            .Should().BeEmpty();
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_preserve_the_embedded_file_status_list_layout_and_semantic_tree()
    {
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        form.Show();
        Dispatcher.UIThread.RunJobs();

        FileStatusList files = form.FindControl<FileStatusList>("_fileStatusList")!;
        GitItemStatus worktree = new("tracked.txt")
        {
            IsChanged = true,
            IsTracked = true,
            Staged = StagedStatus.WorkTree,
        };
        files.SetDiffs([worktree]);
        Dispatcher.UIThread.RunJobs();

        StackPanel toolbarControl = files.FindControl<StackPanel>("Toolbar")!;
        TextBlock splitterControl = files.FindControl<TextBlock>("lblSplitter")!;
        ComboBox filterControl = files.FindControl<ComboBox>("cboFilterComboBox")!;
        ListBox activeListControl = files.FindControl<ListBox>("lstFiles")!;
        BoundsInFiles(toolbarControl).Should().Be(new Rect(0, 0, 742, 25));
        BoundsInFiles(splitterControl).Should().Be(new Rect(0, 25, 742, 2));
        BoundsInFiles(filterControl).Should().Be(new Rect(0, 27, 742, 23));
        BoundsInFiles(activeListControl).Should().Be(new Rect(0, 50, 742, 62));

        Control asTreeButton = files.FindControl<Control>("btnAsTree")!;
        asTreeButton.Bounds.Should().Be(new Rect(0, 1, 32, 22));
        files.FindControl<Separator>("sepAsTree")!.IsVisible.Should().BeFalse();
        files.FindControl<Separator>("sepGroupBy")!.Bounds.Should().Be(new Rect(32, 0, 6, 25));
        files.FindControl<Control>("btnByPath")!.Bounds.Should().Be(new Rect(38, 1, 23, 22));

        CaptureNode[] nodes = Flatten(
                new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize(754, 511)).Root)
            .ToArray();
        CaptureNode toolbar = nodes.Single(node => node.FieldName == "Toolbar");
        CaptureNode list = nodes.Single(node => node.FieldName == "FileStatusListView");
        CaptureNode filter = nodes.Single(node => node.FieldName == "cboFilterComboBox");
        CaptureNode gitGrep = nodes.Single(node => node.FieldName == "cboFindInCommitFilesGitGrep");
        CaptureNode asTree = nodes.Single(node => node.FieldName == "btnAsTree");
        CaptureNode closedDropDownItem = nodes.Single(node => node.FieldName == "tsmiGroupByFilePathTree");

        toolbar.ControlKind.Should().Be("toolStrip");
        toolbar.BoundsDip.Should().Be(new CaptureRectangleF { X = 0, Y = 0, Width = 742, Height = 25 });
        list.ControlKind.Should().Be("tree");
        list.BoundsDip.Should().Be(new CaptureRectangleF { X = 0, Y = 50, Width = 742, Height = 62 });
        list.Visible.Should().BeTrue();
        list.Colors.Background.Should().Be("#FFFFFFFF");
        list.Colors.SelectionBackground.Should().Be("#FF0078D7");
        list.Colors.InactiveSelectionBackground.Should().Be("#FFBFCDDB");
        list.Colors.Additional["hotTrack"].Should().Be("#FF0066CC");
        filter.Font!.Style.Should().Equal("Italic");
        filter.Colors.Foreground.Should().Be("#FF6D6D6D");
        filter.Colors.Border.Should().BeNull();
        filter.FlatStyle.Should().BeNull();
        gitGrep.Visible.Should().BeFalse();
        gitGrep.Font!.Style.Should().Equal("Bold");
        gitGrep.Colors.Border.Should().BeNull();
        gitGrep.FlatStyle.Should().BeNull();
        asTree.ControlKind.Should().Be("menuItem");
        asTree.Colors.Background.Should().Be("#00FFFFFF");
        asTree.Margin!.Dip.Should().Be(new CaptureThicknessF { Left = 0, Top = 1, Right = 0, Bottom = 2 });
        closedDropDownItem.ControlKind.Should().Be("menuItem");
        closedDropDownItem.Visible.Should().BeFalse();
        nodes.Where(node => node.FieldName is "lstFiles" or "tvDiffFiles" or "tvFiles")
            .Should().BeEmpty();

        Rect BoundsInFiles(Control control)
            => new(control.TranslatePoint(default, files)!.Value, control.Bounds.Size);
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_preserve_the_embedded_file_viewer_toolbar_and_semantic_tree()
    {
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        form.Show();
        Dispatcher.UIThread.RunJobs();

        GitUI.Editor.FileViewer viewer = form.FindControl<GitUI.Editor.FileViewer>("_diffViewer")!;
        CaptureNode[] hiddenNodes = Flatten(
                new AvaloniaControlTreeReader(form, renderScale: 1)
                    .ReadPrimary(form, new PixelSize(754, 511)).Root)
            .ToArray();
        CaptureNode toolbarNode = hiddenNodes.Single(node => node.FieldName == "fileviewerToolbar");
        CaptureNode nextNode = hiddenNodes.Single(node => node.FieldName == "nextChangeButton");
        CaptureNode internalNode = hiddenNodes.Single(node => node.FieldName == "internalFileViewer");
        CaptureNode editorNode = hiddenNodes.Single(node => node.FieldName == "TextEditor");

        toolbarNode.ControlKind.Should().Be("toolStrip");
        toolbarNode.Anchor.Should().Equal("Top", "Right");
        toolbarNode.Dock.Should().Be("None");
        toolbarNode.AutoSize.Should().BeTrue();
        toolbarNode.TabIndex.Should().Be(0);
        toolbarNode.TabStop.Should().BeFalse();
        nextNode.ControlKind.Should().Be("menuItem");
        nextNode.ToolTip.Should().Be("Next change");
        nextNode.Visible.Should().BeFalse();
        toolbarNode.Children.Should().OnlyContain(node => node.Visible == false);
        internalNode.BorderStyle.Should().Be("None");
        internalNode.TabStop.Should().BeTrue();
        editorNode.Children.Should().BeEmpty();
        editorNode.BorderStyle.Should().Be("None");
        editorNode.TabStop.Should().BeTrue();
        editorNode.ReadOnly.Should().BeNull();
        hiddenNodes.Single(node => node.FieldName == "splitContainer2").ControlKind.Should().Be("split");
        hiddenNodes.Single(node => node.FieldName == "splitContainer3").ControlKind.Should().Be("split");
        hiddenNodes.Should().NotContain(
            node => node.Name == "FindInCommitFilesGitGrepPanel" || node.Name == "ImagePreview");
        hiddenNodes.Should().NotContain(node => node.Type == "GitUI.SpellChecker.SpellCheckAdorner");

        GitUI.Editor.FileViewer.TestAccessor accessor = viewer.GetTestAccessor();
        accessor.FileViewerToolbar.IsVisible = true;
        Dispatcher.UIThread.RunJobs();

        BoundsInViewer(accessor.FileViewerToolbar).Should().Be(new Rect(292, 0, 410, 25));
        BoundsInToolbar(viewer.FindControl<Control>("nextChangeButton")!).Should().Be(new Rect(0, 1, 23, 22));
        BoundsInToolbar(viewer.FindControl<Control>("previousChangeButton")!).Should().Be(new Rect(23, 1, 23, 22));
        BoundsInToolbar(viewer.FindControl<Separator>("toolStripSeparator3")!).Should().Be(new Rect(46, 0, 6, 25));
        BoundsInToolbar(viewer.FindControl<ComboBox>("encodingToolStripComboBox")!).Should().Be(new Rect(243, 0, 140, 25));
        BoundsInToolbar(viewer.FindControl<Control>("settingsButton")!).Should().Be(new Rect(384, 1, 23, 22));

        Rect BoundsInViewer(Control control)
            => new(control.TranslatePoint(default, viewer)!.Value, control.Bounds.Size);

        Rect BoundsInToolbar(Control control)
            => new(control.TranslatePoint(default, accessor.FileViewerToolbar)!.Value, control.Bounds.Size);
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_project_native_tab_pages_and_hidden_descendants()
    {
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        form.Show();
        Dispatcher.UIThread.RunJobs();

        CaptureNode[] initialNodes = ReadNodes();
        CaptureNode tabs = initialNodes.Single(node => node.FieldName == "tabControl1");
        CaptureNode diffPage = initialNodes.Single(node => node.FieldName == "tabPage1");
        CaptureNode commentsPage = initialNodes.Single(node => node.FieldName == "tabPage2");
        CaptureNode diffLayout = initialNodes.Single(node => node.FieldName == "splitContainer3");

        tabs.BoundsDip.Should().Be(new CaptureRectangleF { X = 0, Y = 0, Width = 754, Height = 359 });
        tabs.Dock.Should().Be("Fill");
        tabs.TabStop.Should().BeTrue();
        diffPage.BoundsDip.Should().Be(new CaptureRectangleF { X = 4, Y = 30, Width = 746, Height = 325 });
        diffPage.Visible.Should().BeTrue();
        diffLayout.BoundsDip.Should().Be(new CaptureRectangleF { X = 2, Y = 2, Width = 742, Height = 321 });
        commentsPage.Visible.Should().BeFalse();
        Flatten(commentsPage).Should().OnlyContain(node => node.Visible != true);

        TabControl tabControl = form.FindControl<TabControl>("tabControl1")!;
        tabControl.SelectedItem = form.FindControl<TabItem>("tabPage2");
        Dispatcher.UIThread.RunJobs();

        CaptureNode[] commentNodes = ReadNodes();
        CaptureNode nowHiddenDiffPage = commentNodes.Single(node => node.FieldName == "tabPage1");
        CaptureNode selectedCommentsPage = commentNodes.Single(node => node.FieldName == "tabPage2");
        CaptureNode discussion = commentNodes.Single(node => node.FieldName == "_discussionWB");
        commentNodes.Single(node => node.FieldName == "_diffViewer").Colors.Foreground.Should().Be("#FF000000");
        commentNodes.Single(node => node.FieldName == "_fileStatusList").Colors.Foreground.Should().Be("#FF000000");
        commentNodes.Single(node => node.FieldName == "lblSplitter").Colors.Foreground.Should().Be("#FF000000");
        commentNodes.Single(node => node.FieldName == "internalFileViewer").Colors.Foreground.Should().Be("#FF000000");
        nowHiddenDiffPage.Visible.Should().BeFalse();
        Flatten(nowHiddenDiffPage).Should().OnlyContain(node => node.Visible != true);
        selectedCommentsPage.Visible.Should().BeTrue();
        selectedCommentsPage.BoundsDip.Should().Be(new CaptureRectangleF { X = 4, Y = 30, Width = 746, Height = 325 });
        discussion.ControlKind.Should().Be("control");
        discussion.BorderStyle.Should().BeNull();
        discussion.BorderWidthDip.Should().BeNull();
        discussion.Children.Should().BeEmpty();

        form.Close();

        CaptureNode[] ReadNodes()
            => Flatten(
                    new AvaloniaControlTreeReader(form, renderScale: 1)
                        .ReadPrimary(form, new PixelSize(754, 511)).Root)
                .ToArray();
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_resize_both_source_split_containers_proportionally()
    {
        using ViewPullRequestsForm form = new();
        form.Show();
        Dispatcher.UIThread.RunJobs();
        Grid top = form.FindControl<Grid>("tableLayoutPanel2")!;
        Grid diff = form.FindControl<Grid>("splitContainer3")!;
        FileStatusList files = form.FindControl<FileStatusList>("_fileStatusList")!;

        top.Bounds.Height.Should().Be(146);
        files.Bounds.Height.Should().Be(112);
        form.Width = 854;
        form.Height = 591;
        Dispatcher.UIThread.RunJobs();

        top.Bounds.Height.Should().Be(168);
        diff.Bounds.Height.Should().Be(379);
        files.Bounds.Height.Should().Be(132);
        form.Close();
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(NativeListThemeCases))]
    public void ViewPullRequestsForm_should_use_native_list_chrome(
        ThemeVariant themeVariant,
        string header,
        string selection,
        string inactiveSelection)
    {
        using ViewPullRequestsForm form = new() { RequestedThemeVariant = themeVariant };
        form.Show();
        Dispatcher.UIThread.RunJobs();

        ListBox list = form.FindControl<ListBox>("_pullRequestsList")!;
        list.BorderThickness.Should().Be(new Thickness(1, 0, 1, 1));
        ResolveColor("GitExtensionsNativeListHeaderBackgroundBrush").Should().Be(Color.Parse(header));
        ResolveColor("GitExtensionsNativeListSelectionBackgroundBrush").Should().Be(Color.Parse(selection));
        ResolveColor("GitExtensionsNativeListInactiveSelectionBackgroundBrush").Should().Be(Color.Parse(inactiveSelection));

        Color ResolveColor(string key)
        {
            form.TryFindResource(key, form.ActualThemeVariant, out object? resource).Should().BeTrue();
            return resource.Should().BeOfType<SolidColorBrush>().Subject.Color;
        }
    }

    private static IEnumerable<TestCaseData> NativeListThemeCases()
    {
        yield return new TestCaseData(ThemeVariant.Light, "#FFFFFF", "#CCE8FF", "#D9D9D9")
            .SetName("ViewPullRequestsForm_should_use_native_list_chrome_light");
        yield return new TestCaseData(ThemeVariant.Dark, "#191919", "#28445B", "#2B2B2B")
            .SetName("ViewPullRequestsForm_should_use_native_list_chrome_dark");
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_load_current_hosted_remote_and_pull_requests()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IHostedRepository repository = CreateRepository(pullRequest);
        IHostedRemote remote = Substitute.For<IHostedRemote>();
        remote.Name.Returns("origin");
        remote.DisplayData.Returns("owner/repository (origin)");
        remote.GetHostedRepository().Returns(repository);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([remote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);

        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.ItemCount.Should().Be(1);
        accessor.HostedRepositories.SelectedIndex.Should().Be(0);
        accessor.PullRequests.ItemCount.Should().Be(1);
        accessor.DiffItems.Should().ContainSingle(item => item.Name == "src/file.txt");
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_mask_the_form_only_while_initial_remotes_load()
    {
        TaskCompletionSource loadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseLoad = new(initialState: false);
        IHostedRemote remote = CreateRemote("origin", CreateRepository());
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns(
            _ =>
            {
                loadStarted.TrySetResult();
                releaseLoad.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                return [remote];
            });
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        form.Show();
        Dispatcher.UIThread.RunJobs();
        await loadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        form.GetVisualDescendants().OfType<LoadingControl>().Should().ContainSingle();

        releaseLoad.Set();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Dispatcher.UIThread.RunJobs();

        form.GetVisualDescendants().OfType<LoadingControl>().Should().BeEmpty();
        form.Close();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_initial_provider_failure_and_remove_the_mask()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule()
            .Returns(_ => throw new InvalidOperationException("host discovery failed"));
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);

        form.Show();
        Dispatcher.UIThread.RunJobs();
        await form.GetTestAccessor().JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Dispatcher.UIThread.RunJobs();

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Contain("host discovery failed");
        form.GetVisualDescendants().OfType<LoadingControl>().Should().BeEmpty();
        form.Close();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_size_non_title_columns_to_their_content()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.Owner.Returns("a-contributor-name-that-is-wider-than-the-header");
        pullRequest.FetchBranch.Returns("pr/42");
        IHostedRemote remote = CreateRemote("origin", CreateRepository(pullRequest));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([remote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        Grid header = (Grid)form.FindControl<ContentControl>("columnHeaderId")!.Parent!;
        double initialOwnerWidth = header.ColumnDefinitions[2].Width.Value;
        header.ColumnDefinitions[4].Width.IsStar.Should().BeTrue();
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        header.ColumnDefinitions[1].Width.IsStar.Should().BeTrue();
        header.ColumnDefinitions[2].Width.Value.Should().BeGreaterThan(initialOwnerWidth);
        header.ColumnDefinitions[4].Width.IsAbsolute.Should().BeTrue();
        header.ColumnDefinitions[4].Width.Value.Should().BeGreaterThan(1);
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_skip_broken_and_empty_remotes_during_first_load()
    {
        IHostedRemote brokenRemote = Substitute.For<IHostedRemote>();
        brokenRemote.Name.Returns("origin");
        brokenRemote.DisplayData.Returns("broken/repository (origin)");
        brokenRemote.GetHostedRepository().Returns(_ => throw new InvalidOperationException("remote failed"));
        IHostedRepository emptyRepository = CreateRepository();
        IHostedRemote emptyRemote = CreateRemote("empty", emptyRepository);
        IPullRequestInformation pullRequest = CreatePullRequest();
        IHostedRemote populatedRemote = CreateRemote("populated", CreateRepository(pullRequest));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([brokenRemote, emptyRemote, populatedRemote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.SelectedIndex.Should().Be(2);
        accessor.PullRequestTitles.Should().Equal("Portable pull request viewer");
        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Contain("remote failed");
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_retain_the_source_first_load_state_at_the_final_empty_remote()
    {
        IHostedRemote emptyRemote = CreateRemote("origin", CreateRepository());
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([emptyRemote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.SelectedIndex.Should().Be(0);
        accessor.HostedRepositorySelectionEnabled.Should().BeTrue();
        accessor.PullRequestDisplayTitles.Should().Equal(" : LOADING : ");
        accessor.IsFirstLoad.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_discard_a_superseded_remote_load()
    {
        TaskCompletionSource firstLoadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstLoad = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPullRequestInformation stalePullRequest = CreatePullRequest();
        stalePullRequest.Title.Returns("Stale pull request");
        IHostedRepository firstRepository = Substitute.For<IHostedRepository>();
        firstRepository.GetPullRequests().Returns(_ =>
        {
            firstLoadStarted.TrySetResult();
            releaseFirstLoad.Task.GetAwaiter().GetResult();
            return [stalePullRequest];
        });
        IPullRequestInformation currentPullRequest = CreatePullRequest();
        currentPullRequest.Title.Returns("Current pull request");
        IHostedRemote firstRemote = CreateRemote("origin", firstRepository);
        IHostedRemote secondRemote = CreateRemote("upstream", CreateRepository(currentPullRequest));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([firstRemote, secondRemote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await firstLoadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.SelectHostedRepository(1);
        releaseFirstLoad.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositories.SelectedIndex.Should().Be(1);
        accessor.PullRequestTitles.Should().Equal("Current pull request");
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_retain_source_loading_state_when_pull_request_loading_fails()
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetPullRequests().Returns(_ => throw new InvalidOperationException("pull request load failed"));
        IHostedRemote remote = CreateRemote("origin", repository);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([remote]);
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentRemote().Returns("origin");
        module.GetRemotesAsync().Returns([]);
        using ViewPullRequestsForm form = CreateForm(host, module);
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.HostedRepositorySelectionEnabled.Should().BeFalse();
        accessor.PullRequestDisplayTitles.Should().Equal(" : LOADING : ");
        accessor.HostedRepositories.SelectedIndex.Should().Be(0);
        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to fetch pull data!" + Environment.NewLine + "pull request load failed");
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_load_diff_and_native_discussion_rows()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = pullRequest.GetDiscussion();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.DiffItems.Should().ContainSingle(item => item.Name == "src/file.txt");
        accessor.Discussion.ItemCount.Should().Be(1);
        discussion.DidNotReceive().ForceReload();
        pullRequest.HeadRepo.Received().CloneProtocol = GitProtocol.Https;
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_scroll_the_native_discussion_to_the_document_end()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = pullRequest.GetDiscussion();
        IDiscussionEntry secondEntry = Substitute.For<IDiscussionEntry>();
        secondEntry.Author.Returns("Reviewer");
        secondEntry.Created.Returns(new DateTime(2026, 7, 27, 13, 0, 0, DateTimeKind.Utc));
        secondEntry.Body.Returns("Ready for review.");
        discussion.Entries.Returns([.. discussion.Entries, secondEntry]);
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        form.Show();
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        form.FindControl<TabControl>("tabControl1")!.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        ScrollViewer scrollViewer = accessor.Discussion
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .Single();
        ItemsPresenter itemsPresenter = accessor.Discussion
            .GetVisualDescendants()
            .OfType<ItemsPresenter>()
            .Single();
        ListBoxItem lastContainer = accessor.Discussion.ContainerFromIndex(1) as ListBoxItem
            ?? throw new AssertionException("The second discussion row was not materialized.");
        itemsPresenter.MinHeight.Should().BeGreaterThanOrEqualTo(
            scrollViewer.Viewport.Height + lastContainer.Margin.Bottom);
        scrollViewer.Offset.Y.Should().BeGreaterThan(0);
        scrollViewer.Offset.Y.Should().Be(scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_cancel_discussion_loading_when_the_hosted_repository_clears()
    {
        TaskCompletionSource loadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseLoad = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPullRequestInformation pullRequest = CreatePullRequest();
        IPullRequestDiscussion discussion = Substitute.For<IPullRequestDiscussion>();
        pullRequest.GetDiscussion().Returns(_ =>
        {
            loadStarted.TrySetResult();
            releaseLoad.Task.GetAwaiter().GetResult();
            return discussion;
        });
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await loadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.ClearHostedRepositorySelection();
        releaseLoad.TrySetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.Discussion.ItemCount.Should().Be(0);
        accessor.FetchEnabled.Should().BeTrue();
        accessor.AddAndFetchEnabled.Should().BeTrue();
        accessor.CloseEnabled.Should().BeTrue();
        accessor.RefreshEnabled.Should().BeTrue();
        accessor.PostEnabled.Should().BeTrue();
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void ViewPullRequestsForm_should_preserve_source_action_defaults_without_a_selection()
    {
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.FetchEnabled.Should().BeTrue();
        accessor.AddAndFetchEnabled.Should().BeTrue();
        accessor.CloseEnabled.Should().BeTrue();
        accessor.RefreshEnabled.Should().BeTrue();
        accessor.PostEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_cancel_diff_loading_when_pull_request_selection_clears()
    {
        TaskCompletionSource<string> diffSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.GetDiffDataAsync().Returns(diffSource.Task);
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        accessor.ClearPullRequestSelection();
        diffSource.TrySetResult(Diff);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.DiffItems.Should().BeEmpty();
        accessor.Discussion.ItemCount.Should().Be(0);
        accessor.FetchEnabled.Should().BeTrue();
        _messageBoxHost.Messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_preserve_loaded_file_rows_when_selection_clears()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.DiffItems.Should().ContainSingle();

        accessor.ClearPullRequestSelection();

        accessor.DiffItems.Should().ContainSingle(item => item.Name == "src/file.txt");
        accessor.Discussion.ItemCount.Should().Be(0);
        accessor.DiffText.Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_close_the_selected_pull_request_and_reload()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.ClosePullRequest();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        pullRequest.Received(1).Close();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_a_close_failure_and_restore_the_action()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.When(candidate => candidate.Close())
            .Do(_ => throw new InvalidOperationException("close failed"));
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.ClosePullRequest();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to close pull request!" + Environment.NewLine + "close failed");
        accessor.CloseEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_a_discussion_load_failure()
    {
        IPullRequestInformation pullRequest = CreatePullRequest();
        pullRequest.GetDiscussion().Returns(_ => throw new InvalidOperationException("discussion failed"));
        using ViewPullRequestsForm form = CreateForm(
            Substitute.For<IRepositoryHostPlugin>(),
            Substitute.For<IGitModule>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();

        accessor.SelectPullRequest(pullRequest);
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Could not load discussion!" + Environment.NewLine + "discussion failed");
        accessor.Discussion.ItemCount.Should().Be(0);
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_fetch_the_selected_pull_request_into_its_local_branch()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullRequestFetch-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string targetDirectory = Path.Combine(root, "target");
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "feature");
            GitModule targetModule = CreateCommittedRepository(targetDirectory, "main");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = CreateCommands(targetModule, notifier);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(call => targetModule.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));
            IPullRequestInformation pullRequest = CreatePullRequest(sourceDirectory);
            using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
            ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.SelectPullRequest(pullRequest);
            await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.FetchEnabled.Should().BeTrue();

            accessor.FetchPullRequest();

            targetModule.GetRefs(RefsFilter.Heads)
                .Select(gitRef => gitRef.LocalName)
                .Should().Contain("pr/42");
            notifier.Received(1).Notify();
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_add_remote_fetch_and_checkout_the_selected_pull_request()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullRequestCheckout-{Guid.NewGuid():N}");
        string sourceDirectory = Path.Combine(root, "source");
        string targetDirectory = Path.Combine(root, "target");
        try
        {
            GitModule sourceModule = CreateCommittedRepository(sourceDirectory, "feature");
            GitModule targetModule = CreateCommittedRepository(targetDirectory, "main");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = CreateCommands(targetModule, notifier);
            commands.StartGitCommandProcessDialog(
                    Arg.Any<WinFormsShims.IWin32Window>(),
                    Arg.Any<ArgumentString>())
                .Returns(call => targetModule.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));
            IPullRequestInformation pullRequest = CreatePullRequest(sourceDirectory);
            using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
            ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.SelectPullRequest(pullRequest);
            await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
            accessor.AddAndFetchEnabled.Should().BeTrue();

            accessor.AddRemoteFetchAndCheckout();

            IReadOnlyList<Remote> remotes = await targetModule.GetRemotesAsync();
            Remote contributor = remotes.Should().ContainSingle(remote => remote.Name == "contributor").Which;
            Path.GetFullPath(contributor.FetchUrl).Should().Be(Path.GetFullPath(sourceDirectory));
            targetModule.GetCurrentCheckout().Should().Be(sourceModule.GetCurrentCheckout());
            notifier.Received(1).Lock();
            notifier.Received(3).Notify();
            notifier.Received(1).UnLock(false);
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_not_notify_when_fetch_is_cancelled_or_fails()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.FetchCmd(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), fetchTags: false)
            .Returns((ArgumentString)"fetch-command");
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = CreateCommands(module, notifier);
        commands.StartGitCommandProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Any<ArgumentString>())
            .Returns(false);
        using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(CreatePullRequest());
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.FetchPullRequest();

        notifier.DidNotReceive().Notify();
    }

    [AvaloniaTest]
    public async Task ViewPullRequestsForm_should_report_add_remote_failure_and_unlock_notifications()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.AddRemote("contributor", "https://example.test/contributor/repository.git")
            .Returns("add failed");
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = CreateCommands(module, notifier);
        using ViewPullRequestsForm form = new(commands, Substitute.For<IRepositoryHostPlugin>());
        ViewPullRequestsForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.SelectPullRequest(CreatePullRequest());
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.AddRemoteFetchAndCheckout();

        _messageBoxHost.Messages.Should().ContainSingle().Which.Should().Be("add failed");
        commands.DidNotReceive().StartGitCommandProcessDialog(
            Arg.Any<WinFormsShims.IWin32Window>(),
            Arg.Any<ArgumentString>());
        notifier.Received(1).Lock();
        notifier.Received(1).UnLock(false);
        notifier.DidNotReceive().Notify();
    }

    [Test]
    public void ViewPullRequestsForm_should_split_provider_diff_into_file_rows()
    {
        IReadOnlyList<GitItemStatus> items = ViewPullRequestsForm.TestAccessor.ParseDiffForTesting(
            Diff,
            BaseSha,
            HeadSha);

        items.Should().ContainSingle();
        items[0].Name.Should().Be("src/file.txt");
        items[0].IsChanged.Should().BeTrue();
        items[0].IsTracked.Should().BeTrue();
    }

    [Test]
    public void ViewPullRequestsForm_should_reject_an_invalid_head_revision()
    {
        Action action = () => ViewPullRequestsForm.TestAccessor.ParseDiffForTesting(
            Diff,
            BaseSha,
            "not-an-object-id");

        action.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void ViewPullRequestsForm_should_reject_an_unrecognised_file_patch()
    {
        const string malformedDiff = "diff --git this is not a file header with enough content";

        Action action = () => ViewPullRequestsForm.TestAccessor.ParseDiffForTesting(
            malformedDiff,
            BaseSha,
            HeadSha);

        action.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void DiscussionHtmlCreator_should_project_comment_and_commit_entries_to_native_rows()
    {
        IDiscussionEntry comment = Substitute.For<IDiscussionEntry>();
        comment.Author.Returns((string?)null);
        comment.Created.Returns(new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc));
        comment.Body.Returns("First line\nSecond line");
        ICommitDiscussionEntry commit = Substitute.For<ICommitDiscussionEntry>();
        commit.Author.Returns("Contributor");
        commit.Created.Returns(new DateTime(2026, 7, 27, 13, 0, 0, DateTimeKind.Utc));
        commit.Body.Returns((string?)null);
        commit.Sha.Returns((string?)null);

        IReadOnlyList<DiscussionHtmlCreator.DiscussionEntryPresentation> rows =
            DiscussionHtmlCreator.CreateFor([comment, commit]);

        rows.Should().Equal(
            new DiscussionHtmlCreator.DiscussionEntryPresentation(
                "[UNKNOWN]",
                comment.Created.ToString(),
                "First line\nSecond line",
                null),
            new DiscussionHtmlCreator.DiscussionEntryPresentation(
                "Contributor",
                commit.Created.ToString(),
                "[UNKNOWN]",
                "[UNKNOWN]"));
        DiscussionHtmlCreator.CreateFor().Should().BeEmpty();
    }

    [AvaloniaTest]
    public void StartPullRequestsDialog_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartPullRequestsDialog(owner: null, host);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    private static ViewPullRequestsForm CreateForm(IRepositoryHostPlugin host, IGitModule module)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return new ViewPullRequestsForm(commands, host);
    }

    private IGitUICommands CreateCommands(IGitModule module, ILockableNotifier notifier)
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
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
        File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), branch);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-M", branch }).Should().BeTrue();
        return module;
    }

    private static IHostedRepository CreateRepository(IPullRequestInformation pullRequest)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetPullRequests().Returns([pullRequest]);
        return repository;
    }

    private static IHostedRepository CreateRepository(params IPullRequestInformation[] pullRequests)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetPullRequests().Returns(pullRequests);
        return repository;
    }

    private static IHostedRemote CreateRemote(string name, IHostedRepository repository)
    {
        IHostedRemote remote = Substitute.For<IHostedRemote>();
        remote.Name.Returns(name);
        remote.DisplayData.Returns($"owner/repository ({name})");
        remote.GetHostedRepository().Returns(repository);
        return remote;
    }

    private static IPullRequestInformation CreatePullRequest(string? cloneUrl = null)
    {
        IHostedRepository headRepository = Substitute.For<IHostedRepository>();
        headRepository.CloneUrl.Returns(cloneUrl ?? "https://example.test/contributor/repository.git");

        IDiscussionEntry entry = Substitute.For<IDiscussionEntry>();
        entry.Author.Returns("Contributor");
        entry.Created.Returns(new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc));
        entry.Body.Returns("Discussion body");
        IPullRequestDiscussion discussion = Substitute.For<IPullRequestDiscussion>();
        discussion.Entries.Returns([entry]);

        IPullRequestInformation pullRequest = Substitute.For<IPullRequestInformation>();
        pullRequest.Id.Returns("42");
        pullRequest.Title.Returns("Portable pull request viewer");
        pullRequest.Owner.Returns("contributor");
        pullRequest.Created.Returns(new DateTime(2026, 7, 27, 11, 0, 0, DateTimeKind.Utc));
        pullRequest.FetchBranch.Returns("pr/42");
        pullRequest.BaseSha.Returns(BaseSha);
        pullRequest.HeadSha.Returns(HeadSha);
        pullRequest.HeadRef.Returns("feature");
        pullRequest.HeadRepo.Returns(headRepository);
        pullRequest.GetDiffDataAsync().Returns(Task.FromResult(Diff));
        pullRequest.GetDiscussion().Returns(discussion);
        return pullRequest;
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

using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.RepoHosting;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RepositoryHostCreatePullRequestTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private string _originalApplicationExecutablePath = null!;
    private StubMessageBoxHost _messageBoxHost = null!;
    private IRepositoryHostPlugin[] _originalGitHosters = null!;

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
        _serviceContainer.AddService<IAppTitleGenerator>(
            new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _messageBoxHost = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxHost;
        _originalGitHosters = [.. PluginRegistry.GitHosters];
        _workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.CreatePullRequestTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_workingDirectory, ".github"));
    }

    [TearDown]
    public void TearDown()
    {
        PluginRegistry.GitHosters.Clear();
        PluginRegistry.GitHosters.AddRange(_originalGitHosters);
        AppSettings.GetTestAccessor().ApplicationExecutablePath = _originalApplicationExecutablePath;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void CreatePullRequestForm_should_preserve_layout_and_translation_identities()
    {
        using CreatePullRequestForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(546);
        form.Height.Should().Be(323);
        form.FindControl<ComboBox>("_pullReqTargetsCB").Should().NotBeNull();
        form.FindControl<ComboBox>("_yourBranchesCB").Should().NotBeNull();
        form.FindControl<ComboBox>("_remoteBranchesCB").Should().NotBeNull();
        form.FindControl<GitUI.SpellChecker.EditNetSpell>("_bodyTB").Should().NotBeNull();
        foreach (string labelName in new[] { "label1", "label2", "label3", "label4", "label5" })
        {
            TextBlock label = form.FindControl<TextBlock>(labelName)!;
            label.Classes.Should().Contain("gitextensions-auto-label");
            label.Height.Should().Be(15);
            label.Padding.Should().Be(new Avalonia.Thickness(3, 0));
        }

        TextBox title = form.FindControl<TextBox>("_titleTB")!;
        title.Height.Should().Be(23);
        title.Margin.Should().Be(new Avalonia.Thickness(0, 1, 0, 0));
        form.FindControl<TextBlock>("label1")!.Margin.Should().Be(new Avalonia.Thickness(0, 4, 0, 0));
        form.FindControl<TextBlock>("label2")!.Margin.Should().Be(new Avalonia.Thickness(0, 3, 0, 0));
        double titleLabelWidth = form.FindControl<TextBlock>("label1")!.Width;
        double bodyLabelWidth = form.FindControl<TextBlock>("label2")!.Width;
        double yourBranchLabelWidth = form.FindControl<TextBlock>("label4")!.Width;
        double targetBranchLabelWidth = form.FindControl<TextBlock>("label5")!.Width;
        if (OperatingSystem.IsWindows())
        {
            (titleLabelWidth, bodyLabelWidth, yourBranchLabelWidth, targetBranchLabelWidth)
                .Should().Be((33, 37, 74, 83));
        }
        else
        {
            titleLabelWidth.Should().BePositive().And.BeLessThan(bodyLabelWidth);
            bodyLabelWidth.Should().BeLessThan(yourBranchLabelWidth);
            yourBranchLabelWidth.Should().BeLessThan(targetBranchLabelWidth);
        }

        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "$this", "Text", "Create Pull Request");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "_createBtn", "Text", "Create");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "groupBox1", "Text", "Pull request data");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label1", "Text", "Title:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label2", "Text", "Body:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label3", "Text", "Target repository:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label4", "Text", "Your branch:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "label5", "Text", "Target branch:");
        translation.Received(1).AddTranslationItem(
            nameof(CreatePullRequestForm), "_strFailedToLoadTemplate", "Text", "Failed to load PR template from file.");
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(NativeChromeThemeCases))]
    public void CreatePullRequestForm_should_use_native_input_group_and_button_chrome(
        ThemeVariant themeVariant,
        string inputBackground,
        string buttonBackground,
        string buttonBorder,
        string flatStyle)
    {
        using CreatePullRequestForm form = CreateForm(CreateFixture(), chooseRemote: null, chooseBranch: null);
        form.RequestedThemeVariant = themeVariant;
        form.Show();
        Dispatcher.UIThread.RunJobs();

        TextBox title = form.FindControl<TextBox>("_titleTB")!;
        HeaderedContentControl group = form.FindControl<HeaderedContentControl>("groupBox1")!;
        Button create = form.FindControl<Button>("_createBtn")!;
        Border groupFrame = group.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Name == "PART_GroupBoxFrame");
        Border chrome = create.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Name == "PART_NativeButtonChrome");
        CaptureNode[] nodes = [.. Flatten(
            new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new PixelSize(546, 323)).Root)];
        CaptureNode node = nodes.Single(candidate => candidate.FieldName == "_createBtn");
        CaptureNode targetRepository = nodes.Single(candidate => candidate.FieldName == "_pullReqTargetsCB");
        CaptureNode sourceBranch = nodes.Single(candidate => candidate.FieldName == "_yourBranchesCB");
        CaptureNode body = nodes.Single(candidate => candidate.FieldName == "_bodyTB");
        title.Background.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(Color.Parse(inputBackground));
        group.Classes.Should().Contain("gitextensions-native-group-border");
        groupFrame.BorderBrush.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(Color.Parse("#DCDCDC"));
        create.Classes.Should().Contain("gitextensions-native-dialog-action");
        chrome.Background.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(Color.Parse(buttonBackground));
        chrome.BorderBrush.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(Color.Parse(buttonBorder));
        chrome.CornerRadius.Should().Be(new CornerRadius(4));
        node.FlatStyle.Should().Be(flatStyle);
        node.Padding.Dip.Should().Be(new CaptureThicknessF { Left = 0, Top = 0, Right = 0, Bottom = 0 });
        node.Margin.Dip.Should().Be(new CaptureThicknessF { Left = 3, Top = 3, Right = 3, Bottom = 3 });
        node.BorderWidthDip.Should().BeNull();
        node.Dock.Should().Be("None");
        node.AutoSize.Should().BeFalse();
        node.Alignment.Should().Be("MiddleCenter");
        node.Colors.Background.Should().Be(themeVariant == ThemeVariant.Dark ? "#FF202020" : "#FFF0F0F0");
        node.Colors.Border.Should().BeNull();
        targetRepository.Selected.Should().BeFalse();
        targetRepository.BorderWidthDip.Should().BeNull();
        body.TabStop.Should().BeTrue();
        nodes.Where(candidate => candidate != nodes[0]
                                 && candidate.FieldName is null
                                 && candidate.Name is null)
            .Should().BeEmpty("unnamed Avalonia layout panels are not WinForms product controls");
        targetRepository.Text.Should().Be("project/repository");
        sourceBranch.Focused.Should().BeTrue();

        form.Close();
    }

    private static IEnumerable<TestCaseData> NativeChromeThemeCases()
    {
        yield return new TestCaseData(ThemeVariant.Light, "#FFFFFF", "#FDFDFD", "#D0D0D0", "Standard")
            .SetName("CreatePullRequestForm_should_use_native_input_group_and_button_chrome_light");
        yield return new TestCaseData(ThemeVariant.Dark, "#2E2E2E", "#333333", "#9B9B9B", "Flat")
            .SetName("CreatePullRequestForm_should_use_native_input_group_and_button_chrome_dark");
    }

    [AvaloniaTest]
    public void CreatePullRequestForm_should_focus_the_hosted_body_editor()
    {
        using CreatePullRequestForm form = CreateForm(CreateFixture(), chooseRemote: null, chooseBranch: null);
        GitUI.SpellChecker.EditNetSpell body = form.FindControl<GitUI.SpellChecker.EditNetSpell>("_bodyTB")!;
        form.Show();
        form.SetRenderScaling(1.25);
        Dispatcher.UIThread.RunJobs();

        body.Focus().Should().BeTrue();
        Dispatcher.UIThread.RunJobs();
        body.GetTestAccessor().TextBox.IsFocused.Should().BeTrue();

        form.Close();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_load_selected_remotes_branches_title_and_template()
    {
        PullRequestFixture fixture = CreateFixture();
        await File.WriteAllTextAsync(
            Path.Combine(_workingDirectory, ".github", "PULL_REQUEST_TEMPLATE.md"),
            "Template body");
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.TargetRepositories.SelectedItem.Should().BeSameAs(fixture.TargetRemote);
        accessor.SourceBranches.SelectedItem.Should().Be("feature");
        accessor.TargetBranches.SelectedItem.Should().Be("develop");
        accessor.Title.Should().Be("Suggested title");
        accessor.Body.Should().Be("Template body");
        accessor.CreateEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_mask_the_form_only_while_initial_remotes_load()
    {
        TaskCompletionSource loadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseLoad = new(initialState: false);
        PullRequestFixture fixture = CreateFixture();
        fixture.Host.GetHostedRemotesForModule().Returns(
            _ =>
            {
                loadStarted.TrySetResult();
                releaseLoad.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                return [fixture.SourceRemote, fixture.TargetRemote];
            });
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

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
    public async Task CreatePullRequestForm_should_enumerate_lazy_remotes_off_the_UI_thread()
    {
        TaskCompletionSource enumerationStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseEnumeration = new(initialState: false);
        PullRequestFixture fixture = CreateFixture();
        fixture.Host.GetHostedRemotesForModule().Returns(
            new BlockingReadOnlyList<IHostedRemote>(
                [fixture.SourceRemote, fixture.TargetRemote],
                enumerationStarted,
                releaseEnumeration));
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        form.Show();
        Dispatcher.UIThread.RunJobs();
        await enumerationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Dispatcher.UIThread.CheckAccess().Should().BeTrue();
        form.GetVisualDescendants().OfType<LoadingControl>().Should().ContainSingle();

        releaseEnumeration.Set();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Dispatcher.UIThread.RunJobs();

        accessor.TargetRepositories.ItemCount.Should().Be(1);
        form.GetVisualDescendants().OfType<LoadingControl>().Should().BeEmpty();
        form.Close();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_remove_the_initial_mask_when_remote_loading_fails()
    {
        PullRequestFixture fixture = CreateFixture();
        fixture.Host.GetHostedRemotesForModule().Returns(_ => throw new InvalidOperationException("remote load failed"));
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        form.Mask();

        Func<Task> act = () => accessor.InitializeAsync();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("remote load failed");
        Dispatcher.UIThread.RunJobs();

        ((Panel)form.Content!).Children.OfType<LoadingControl>().Should().BeEmpty();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_fall_back_to_the_first_target_when_the_requested_remote_is_missing()
    {
        PullRequestFixture fixture = CreateFixture();
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "missing", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.TargetRepositories.SelectedItem.Should().BeSameAs(fixture.TargetRemote);
        accessor.TargetBranches.SelectedItem.Should().Be("develop");
        accessor.CreateEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_require_an_owned_source_remote()
    {
        PullRequestFixture fixture = CreateFixture();
        fixture.Host.GetHostedRemotesForModule().Returns([fixture.TargetRemote]);
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.SourceBranches.Items.Should().BeEmpty();
        accessor.TargetBranches.SelectedItem.Should().Be("develop");
        accessor.CreateEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_report_a_branch_load_failure_and_keep_create_disabled()
    {
        PullRequestFixture fixture = CreateFixture();
        fixture.TargetRepository.GetBranches().Returns(_ => throw new InvalidOperationException("branch load failed"));
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.TargetBranches.Items.Should().BeEmpty();
        accessor.CreateEnabled.Should().BeFalse();
        _messageBoxHost.Messages.Should().ContainSingle(message => message.Contains("branch load failed", StringComparison.Ordinal));
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_discard_a_superseded_target_branch_load()
    {
        TaskCompletionSource firstLoadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseFirstLoad = new(initialState: false);
        PullRequestFixture fixture = CreateFixture();
        fixture.TargetRepository.GetBranches().Returns(
            _ =>
            {
                firstLoadStarted.TrySetResult();
                releaseFirstLoad.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                return CreateBranches("stale");
            });
        IHostedRepository secondRepository = CreateRepository(
            defaultBranch: "current",
            branches: ["current"]);
        IHostedRemote secondRemote = CreateRemote(
            name: "second",
            displayData: "project/second",
            isOwnedByMe: false,
            secondRepository);
        fixture.Host.GetHostedRemotesForModule().Returns(
            [fixture.SourceRemote, fixture.TargetRemote, secondRemote]);
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await firstLoadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.TargetRepositories.SelectedItem = secondRemote;
        releaseFirstLoad.Set();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.TargetRepositories.SelectedItem.Should().BeSameAs(secondRemote);
        accessor.TargetBranches.Items.Cast<string>().Should().Equal("current");
        accessor.TargetBranches.SelectedItem.Should().Be("current");
        accessor.CreateEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_cancel_target_loading_when_the_remote_selection_is_cleared()
    {
        TaskCompletionSource loadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseLoad = new(initialState: false);
        PullRequestFixture fixture = CreateFixture();
        fixture.TargetRepository.GetBranches().Returns(
            _ =>
            {
                loadStarted.TrySetResult();
                releaseLoad.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                return CreateBranches("stale");
            });
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();

        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await loadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.TargetRepositories.SelectedIndex = -1;
        releaseLoad.Set();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        accessor.TargetRepositories.SelectedItem.Should().BeNull();
        accessor.TargetBranches.Items.Should().BeEmpty();
        accessor.CreateEnabled.Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_create_with_selected_provider_values()
    {
        PullRequestFixture fixture = CreateFixture();
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "Portable PR";
        accessor.Body = "Created from Avalonia";

        accessor.Create();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        fixture.TargetRepository.Received(1).CreatePullRequest(
            "feature",
            "develop",
            "Portable PR",
            "Created from Avalonia");
        _messageBoxHost.Messages.Should().Contain("Done");
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_run_provider_off_the_ui_thread()
    {
        int uiThreadId = Environment.CurrentManagedThreadId;
        TaskCompletionSource<int> providerThread = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource providerRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        PullRequestFixture fixture = CreateFixture();
        fixture.TargetRepository
            .When(repository => repository.CreatePullRequest("feature", "develop", "Portable PR", "Body"))
            .Do(
                _ =>
                {
                    providerThread.TrySetResult(Environment.CurrentManagedThreadId);
                    providerRelease.Task.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                });
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "Portable PR";
        accessor.Body = "Body";

        accessor.Create();

        int providerThreadId = await providerThread.Task.WaitAsync(TimeSpan.FromSeconds(5));
        accessor.CreateEnabled.Should().BeFalse();
        providerRelease.SetResult();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        providerThreadId.Should().NotBe(uiThreadId);
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_require_a_non_empty_title()
    {
        PullRequestFixture fixture = CreateFixture();
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "  ";

        accessor.Create();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("You must specify a title.");
        fixture.TargetRepository.DidNotReceiveWithAnyArgs()
            .CreatePullRequest(default!, default!, default!, default!);
    }

    [AvaloniaTest]
    public async Task CreatePullRequestForm_should_report_a_provider_failure_and_restore_create()
    {
        PullRequestFixture fixture = CreateFixture();
        fixture.TargetRepository
            .When(repository => repository.CreatePullRequest("feature", "develop", "Portable PR", "Body"))
            .Do(_ => throw new InvalidOperationException("provider failed"));
        using CreatePullRequestForm form = CreateForm(fixture, chooseRemote: "upstream", chooseBranch: "feature");
        CreatePullRequestForm.TestAccessor accessor = form.GetTestAccessor();
        await accessor.InitializeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));
        accessor.Title = "Portable PR";
        accessor.Body = "Body";

        accessor.Create();
        await accessor.JoinOperationsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        _messageBoxHost.Messages.Should().ContainSingle()
            .Which.Should().Be("Failed to create pull request." + Environment.NewLine + "provider failed");
        accessor.CreateEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void StartCreatePullRequest_should_open_provider_configuration_when_required()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(false);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCreatePullRequest(owner: null, host);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    [AvaloniaTest]
    public void StartCreatePullRequest_should_route_the_single_relevant_host()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GitModuleIsRelevantToMe().Returns(true);
        host.ConfigurationOk.Returns(false);
        PluginRegistry.GitHosters.Add(host);
        GitUICommands commands = new(_serviceContainer, Substitute.For<IGitModule>());

        commands.StartCreatePullRequest(owner: null);

        host.Received(1).Execute(Arg.Any<GitUIEventArgs>());
    }

    [AvaloniaTest]
    public async Task AddUpstreamRemote_should_run_the_provider_under_the_browse_owner()
    {
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.ConfigurationOk.Returns(true);
        host.AddUpstreamRemoteAsync().Returns(Task.FromResult<string?>(null));
        GitModule module = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            _workingDirectory);
        GitUICommands commands = new(_serviceContainer, module);
        FormBrowse owner = new(commands);
        try
        {
            commands.AddUpstreamRemote(owner, host);
            await owner.JoinLoadOperationsForTestAsync()
                .WaitAsync(TimeSpan.FromSeconds(5));

            await host.Received(1).AddUpstreamRemoteAsync();
        }
        finally
        {
            owner.Close();
        }
    }

    private CreatePullRequestForm CreateForm(
        PullRequestFixture fixture,
        string? chooseRemote,
        string? chooseBranch)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(true);
        module.GetSelectedBranch().Returns("main");
        module.GetPreviousCommitMessages(
                count: 1,
                revision: "origin/feature",
                authorPattern: string.Empty)
            .Returns(["Suggested title\nDetails"]);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return new CreatePullRequestForm(
            commands,
            fixture.Host,
            chooseRemote,
            chooseBranch);
    }

    private static PullRequestFixture CreateFixture()
    {
        IHostedRepository sourceRepository = CreateRepository(
            defaultBranch: "main",
            branches: ["main", "feature"]);
        IHostedRemote sourceRemote = CreateRemote(
            name: "origin",
            displayData: "owner/repository",
            isOwnedByMe: true,
            sourceRepository);
        IHostedRepository targetRepository = CreateRepository(
            defaultBranch: "develop",
            branches: ["main", "develop"]);
        IHostedRemote targetRemote = CreateRemote(
            name: "upstream",
            displayData: "project/repository",
            isOwnedByMe: false,
            targetRepository);
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.GetHostedRemotesForModule().Returns([sourceRemote, targetRemote]);
        return new PullRequestFixture(
            host,
            sourceRemote,
            targetRemote,
            sourceRepository,
            targetRepository);
    }

    private static IHostedRepository CreateRepository(
        string defaultBranch,
        IReadOnlyList<string> branches)
    {
        IHostedRepository repository = Substitute.For<IHostedRepository>();
        repository.GetDefaultBranch().Returns(defaultBranch);
        IHostedBranch[] hostedBranches = CreateBranches(branches.ToArray());
        repository.GetBranches().Returns(hostedBranches);
        return repository;
    }

    private static IHostedBranch[] CreateBranches(params string[] names)
        => names.Select(
                name =>
                {
                    IHostedBranch branch = Substitute.For<IHostedBranch>();
                    branch.Name.Returns(name);
                    return branch;
                })
            .ToArray();

    private static IHostedRemote CreateRemote(
        string name,
        string displayData,
        bool isOwnedByMe,
        IHostedRepository repository)
    {
        IHostedRemote remote = Substitute.For<IHostedRemote>();
        remote.Name.Returns(name);
        remote.DisplayData.Returns(displayData);
        remote.IsOwnedByMe.Returns(isOwnedByMe);
        remote.GetHostedRepository().Returns(repository);
        return remote;
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

    private sealed record PullRequestFixture(
        IRepositoryHostPlugin Host,
        IHostedRemote SourceRemote,
        IHostedRemote TargetRemote,
        IHostedRepository SourceRepository,
        IHostedRepository TargetRepository);

    private sealed class BlockingReadOnlyList<T>(
        IReadOnlyList<T> items,
        TaskCompletionSource enumerationStarted,
        ManualResetEventSlim releaseEnumeration) : IReadOnlyList<T>
    {
        public int Count => items.Count;

        public T this[int index] => items[index];

        public IEnumerator<T> GetEnumerator()
        {
            enumerationStarted.TrySetResult();
            releaseEnumeration.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
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

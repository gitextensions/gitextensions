using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CloneAndInitTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private StubMessageBoxHost _messageBoxes = null!;
    private bool _closeProcessDialog;

    [SetUp]
    public void SetUp()
    {
        // The clone runs in the process dialog; it must close itself on success.
        _closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;

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

        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CloneAndInitTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.CloseProcessDialog = _closeProcessDialog;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormInit_should_construct()
    {
        FormInit form = new();

        form.GetTestAccessor().DirectoryCombo.Should().NotBeNull();
        form.FindControl<RadioButton>("Personal")!.IsChecked.Should().BeTrue();
        form.FindControl<RadioButton>("Central")!.IsChecked.Should().NotBeTrue();
        form.FindControl<FolderBrowserButton>("Browse").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormInit_should_use_existing_translation_keys_once()
    {
        FormInit form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormInit), "$this", "Text", "Create new repository");
        translation.Received(1).AddTranslationItem(nameof(FormInit), "label1", "Text", "Directory");
        translation.Received(1).AddTranslationItem(nameof(FormInit), "groupBox1", "Text", "Repository type");
        translation.Received(1).AddTranslationItem(nameof(FormInit), "Personal", "Text", "Personal repository");
        translation.Received(1).AddTranslationItem(nameof(FormInit), "Central", "Text", "Central repository, no working directory  (--bare --shared=all)");
        translation.Received(1).AddTranslationItem(nameof(FormInit), "Init", "Text", "Create");
        translation.Received(1).AddTranslationItem(nameof(FormInit), "_initMsgBoxCaption", "Text", "Create new repository");

        AssertDistinctKeys(translation);
    }

    [AvaloniaTest]
    public void FolderBrowserButton_should_use_its_own_translation_key()
    {
        FolderBrowserButton button = new();
        ITranslation translation = Substitute.For<ITranslation>();

        button.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FolderBrowserButton), "buttonBrowse", "Text", "&Browse...");
    }

    [AvaloniaTest]
    public void FormInit_should_reject_a_relative_directory()
    {
        FormInit form = new(CreateCommands().Commands, dir: "", gitModuleChanged: null);
        form.Show();
        try
        {
            form.GetTestAccessor().DirectoryCombo.Text = "relative/path";
            form.GetTestAccessor().IsRootedDirectoryPath("relative/path").Should().BeFalse();

            form.FindControl<Button>("Init")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            _messageBoxes.Messages.Should().ContainSingle(message => message == "Please choose a directory.");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormInit_should_initialize_a_repository()
    {
        string newRepoPath = Path.Combine(_workingDirectory, "new-repo");
        (IGitUICommands commands, _) = CreateCommands();

        bool moduleChanged = false;
        FormInit form = new(commands, newRepoPath, (_, _) => moduleChanged = true);
        form.Show();
        try
        {
            form.FindControl<Button>("Init")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible);

            GitModule initializedModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), newRepoPath);
            initializedModule.IsValidGitWorkingDir().Should().BeTrue();
            moduleChanged.Should().BeTrue();
            _messageBoxes.Messages.Should().ContainSingle(message => message.Contains("Initialized empty Git repository"));
        }
        finally
        {
            form.Close();
            await RepositoryHistoryManager.Locals.RemoveRecentAsync(newRepoPath);
        }
    }

    [AvaloniaTest]
    public void FormClone_should_construct_with_the_default_branch_items()
    {
        FormClone form = new(CreateCommands().Commands, url: null, openedFromProtocolHandler: false, gitModuleChanged: null);

        ComboBox branches = form.FindControl<ComboBox>("_NO_TRANSLATE_Branches")!;
        branches.ItemCount.Should().Be(2, "the default remote-HEAD and no-checkout items are listed");
        form.FindControl<RadioButton>("PersonalRepository")!.IsChecked.Should().BeTrue();
        form.FindControl<CheckBox>("cbDownloadFullHistory")!.IsChecked.Should().BeTrue();
        form.FindControl<CheckBox>("cbIntializeAllSubmodules")!.IsChecked.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormClone_should_not_render_mnemonic_markers_in_plain_text_labels()
    {
        FormClone form = new();

        form.FindControl<TextBlock>("repositoryLabel")!.Text.Should().Be("Repository to clone:");
        form.FindControl<TextBlock>("destinationLabel")!.Text.Should().Be("Destination:");
        form.FindControl<TextBlock>("subdirectoryLabel")!.Text.Should().Be("Subdirectory to create:");
        form.FindControl<TextBlock>("brachLabel")!.Text.Should().Be("Branch:");
    }

    [AvaloniaTest]
    public void FormClone_should_use_existing_translation_keys_once()
    {
        FormClone form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormClone), "$this", "Text", "Clone");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "repositoryLabel", "Text", "Repository to &clone:");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "destinationLabel", "Text", "&Destination:");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "subdirectoryLabel", "Text", "&Subdirectory to create:");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "brachLabel", "Text", "Br&anch:");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "FromBrowse", "Text", "&Browse");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "ToBrowse", "Text", "B&rowse");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "PersonalRepository", "Text", "&Personal repository");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "CentralRepository", "Text", "P&ublic repository, no working directory  (--bare)");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "cbIntializeAllSubmodules", "Text", "Initialize all submodules");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "cbDownloadFullHistory", "Text", "Download full &history");
        translation.Received(1).AddTranslationItem(nameof(FormClone), "Ok", "Text", "Clone");

        // The WinForms Designer stored this tooltip under the ToolTip component's name.
        translation.Received(1).AddTranslationItem(nameof(FormClone), "cbDownloadFullHistory", "ttHints", Arg.Is<string>(tip => tip.StartsWith("The default Git behavior")));

        AssertDistinctKeys(translation);
    }

    [AvaloniaTest]
    public void FormClone_should_extract_urls_like_WinForms()
    {
        FormClone form = new();
        FormClone.TestAccessor accessor = form.GetTestAccessor();

        accessor.TryExtractUrl("https://github.com/gitextensions/gitextensions.git", out string url).Should().BeTrue();
        url.Should().Be("https://github.com/gitextensions/gitextensions.git");

        accessor.TryExtractUrl("clone https://example.com/repo.git now", out url).Should().BeTrue();
        url.Should().Be("https://example.com/repo.git");

        accessor.TryExtractUrl("no url here", out _).Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task FormClone_should_show_busy_state_and_report_native_branch_loading_errors()
    {
        const string error = "ssh: Could not resolve hostname example.invalid";
        IGitModule module = Substitute.For<IGitModule>();
        module.GetRemoteServerRefs(
                Arg.Any<string>(),
                tags: false,
                branches: true,
                out Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callInfo[3] = error;
                return Array.Empty<IGitRef>();
            });
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormClone form = new(commands, url: null, openedFromProtocolHandler: false, gitModuleChanged: null);
        FormClone.TestAccessor accessor = form.GetTestAccessor();
        accessor.Source.Text = "ssh://example.invalid/repository.git";

        accessor.LoadBranches();

        form.Cursor.Should().NotBeNull("remote branch discovery should expose the original busy state");
        await WaitUntilAsync(() => _messageBoxes.Messages.Contains(error));
        form.Cursor.Should().BeNull();
        accessor.Branches.ItemCount.Should().Be(2, "an error must leave the default clone choices intact");
    }

    [AvaloniaTest]
    public async Task FormClone_should_clone_a_local_repository()
    {
        string sourcePath = CreateSourceRepository();
        string destination = Path.Combine(_workingDirectory, "cloned");
        Directory.CreateDirectory(destination);

        (IGitUICommands commands, _) = CreateCommands();

        FormClone form = new(commands, url: null, openedFromProtocolHandler: false, gitModuleChanged: null);
        form.Show();
        string dirTo = Path.Combine(destination, "source");
        try
        {
            ComboBox from = form.FindControl<ComboBox>("_NO_TRANSLATE_From")!;
            ComboBox to = form.FindControl<ComboBox>("_NO_TRANSLATE_To")!;
            TextBox newDirectory = form.FindControl<TextBox>("_NO_TRANSLATE_NewDirectory")!;
            TextBlock info = form.FindControl<TextBlock>("Info")!;

            from.Text = sourcePath;
            to.Text = destination;

            FormClone.TestAccessor accessor = form.GetTestAccessor();
            accessor.LoadBranches();
            await WaitUntilAsync(() => accessor.Branches.Items.Cast<object?>().Any(item => Equals(item, "main")));
            form.Cursor.Should().BeNull();

            // Typing the source fills the subdirectory with the repository name.
            newDirectory.Text.Should().Be("source");
            info.Text.Should().Contain(dirTo).And.Contain("(New directory)");

            form.FindControl<Button>("Ok")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible);

            GitModule clonedModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), dirTo);
            clonedModule.IsValidGitWorkingDir().Should().BeTrue();
            File.Exists(Path.Combine(dirTo, "readme.txt")).Should().BeTrue("the work tree should be checked out");
        }
        finally
        {
            form.Close();
            await RepositoryHistoryManager.Locals.RemoveRecentAsync(dirTo);
            await RepositoryHistoryManager.Remotes.RemoveRecentAsync(sourcePath);
        }
    }

    private string CreateSourceRepository()
    {
        string sourcePath = Path.Combine(_workingDirectory, "source");
        Directory.CreateDirectory(sourcePath);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), sourcePath);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(sourcePath, "readme.txt"), "hello\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return sourcePath;
    }

    private (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        return (commands, module);
    }

    private static void AssertDistinctKeys(ITranslation translation)
    {
        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the condition should be met within the timeout");
    }

    /// <summary>Answers every message box with its default-affirmative result.</summary>
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
            return buttons switch
            {
                WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel => WinFormsShims.DialogResult.Yes,
                _ => WinFormsShims.DialogResult.OK,
            };
        }
    }
}

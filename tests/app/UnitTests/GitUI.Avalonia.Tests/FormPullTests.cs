using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FormPullTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _bareRemoteDirectory = null!;
    private string _publisherDirectory = null!;
    private string _submoduleBareDirectory = null!;
    private string _submoduleSeedDirectory = null!;
    private string _workingDirectory = null!;
    private bool _autoStash;
    private bool? _autoPopStashAfterPull;
    private bool _closeProcessDialog;
    private bool _dontConfirmFetchAndPruneAll;
    private bool _dontConfirmResolveConflicts;
    private bool? _dontConfirmUpdateSubmodulesOnCheckout;
    private GitPullAction _formPullAction;
    private StubMessageBoxHost _messageBoxes = null!;
    private bool? _updateSubmodulesOnCheckout;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _autoStash = AppSettings.AutoStash;
        _autoPopStashAfterPull = AppSettings.AutoPopStashAfterPull;
        _closeProcessDialog = AppSettings.CloseProcessDialog;
        _dontConfirmFetchAndPruneAll = AppSettings.DontConfirmFetchAndPruneAll;
        _dontConfirmResolveConflicts = AppSettings.DontConfirmResolveConflicts;
        _dontConfirmUpdateSubmodulesOnCheckout = AppSettings.DontConfirmUpdateSubmodulesOnCheckout;
        _formPullAction = AppSettings.FormPullAction;
        _updateSubmodulesOnCheckout = AppSettings.UpdateSubmodulesOnCheckout;
        _messageBoxes = new StubMessageBoxHost { Result = WinFormsShims.DialogResult.No };
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;

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

        string testId = Guid.NewGuid().ToString("N");
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}");
        _publisherDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}-publisher");
        _bareRemoteDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}.git");
        _submoduleBareDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}-submodule.git");
        _submoduleSeedDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}-submodule-seed");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.AutoStash = _autoStash;
        AppSettings.AutoPopStashAfterPull = _autoPopStashAfterPull;
        AppSettings.CloseProcessDialog = _closeProcessDialog;
        AppSettings.DontConfirmFetchAndPruneAll = _dontConfirmFetchAndPruneAll;
        AppSettings.DontConfirmResolveConflicts = _dontConfirmResolveConflicts;
        AppSettings.DontConfirmUpdateSubmodulesOnCheckout = _dontConfirmUpdateSubmodulesOnCheckout;
        AppSettings.FormPullAction = _formPullAction;
        AppSettings.UpdateSubmodulesOnCheckout = _updateSubmodulesOnCheckout;
        _serviceContainer.Dispose();
        DeleteDirectory(_workingDirectory);
        DeleteDirectory(_publisherDirectory);
        DeleteDirectory(_bareRemoteDirectory);
        DeleteDirectory(_submoduleSeedDirectory);
        DeleteDirectory(_submoduleBareDirectory);

        static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                TestDirectory.Delete(directory);
            }
        }
    }

    [AvaloniaTest]
    public void FormPull_should_construct_and_use_existing_translation_keys()
    {
        FormPull form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormPull), "$this", "Text", "Pull");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "GroupPullFrom", "Text", "Pull from");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "GroupBranch", "Text", "Branch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "GroupMergeOptions", "Text", "Merge options");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "GroupTagOptions", "Text", "Tag options");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "AddRemote", "Text", "Mana&ge remotes");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "PullFromRemote", "Text", "&Remote");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "PullFromUrl", "Text", "&URL");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Merge", "Text", "&Merge remote branch into current branch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Rebase", "Text", "R&ebase current branch on top of remote branch, creates linear history (use with caution)");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Fetch", "Text", "Do not merge, only &fetch remote changes");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "ReachableTags", "Text", "Follow &tagopt, if not specified, fetch tags reachable from remote HEAD");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "NoTags", "Text", "Fetch &no tag");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "AllTags", "Text", "Fetch &all tags");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Unshallow", "Text", "Do&wnload full history");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Prune", "Text", "&Prune remote branches");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "PruneTags", "Text", "Prune remote branches an&d tags");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Mergetool", "Text", "&Solve conflicts");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Stash", "Text", "Stash &changes");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "AutoStash", "Text", "Auto stas&h");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "lblLocalBranch", "Text", "&Local branch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "lblRemoteBranch", "Text", "Rem&ote branch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "PullFromRemote", "Tooltip", "Remote repository to pull from");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "PullFromUrl", "Tooltip", "Url to pull from");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "lblLocalBranch", "Tooltip", "Local branch to create or reset to the remote branch selected.");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "lblRemoteBranch", "Tooltip", "Remote branch to pull. Leave empty to pull all branches.");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "_buttonFetch", "Text", "&Fetch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "_buttonPull", "Text", "&Pull");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "_formTitleFetch", "Text", "Fetch ({0})");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "_formTitlePull", "Text", "Pull ({0})");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "_selectRemoteRepository", "Text", "Please select a remote repository");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public void FormPull_should_show_remote_branches_and_fetch_mode()
    {
        GitModule module = CreateRepositoryWithNewRemoteCommit(out _);
        FormPull form = new(new GitUICommands(_serviceContainer, module), defaultRemoteBranch: null, defaultRemote: null, GitPullAction.Fetch);

        form.Show();
        try
        {
            ComboBox remotes = form.FindControl<ComboBox>("_NO_TRANSLATE_Remotes")
                ?? throw new InvalidOperationException("Remote selector was not created.");
            TextBox localBranch = form.FindControl<TextBox>("localBranch")
                ?? throw new InvalidOperationException("Local branch field was not created.");
            ComboBox remoteBranch = form.FindControl<ComboBox>("Branches")
                ?? throw new InvalidOperationException("Remote branch field was not created.");
            Button pull = form.FindControl<Button>("Pull")
                ?? throw new InvalidOperationException("Pull button was not created.");
            TextBlock localBranchLabel = form.FindControl<TextBlock>("lblLocalBranch")
                ?? throw new InvalidOperationException("Local branch label was not created.");
            TextBlock remoteBranchLabel = form.FindControl<TextBlock>("lblRemoteBranch")
                ?? throw new InvalidOperationException("Remote branch label was not created.");

            remotes.SelectedItem.Should().Be("origin");
            localBranch.Text.Should().BeEmpty("fetch mode may target a separately named local branch");
            localBranch.IsEnabled.Should().BeTrue();
            remoteBranch.Text.Should().Be(module.GetSelectedBranch());
            localBranchLabel.Text.Should().Be("Local branch");
            remoteBranchLabel.Text.Should().Be("Remote branch");
            pull.Content.Should().Be("_Fetch");
            pull.IsEnabled.Should().BeTrue();
            form.Title.Should().StartWith("Fetch (");
            form.FindControl<HeaderedContentControl>("GroupTagOptions").Should().NotBeNull();
            form.FindControl<RadioButton>("PullFromUrl").Should().NotBeNull();
            form.FindControl<CheckBox>("Prune").Should().NotBeNull();
            form.CaptureRenderedFrame().Should().NotBeNull("the complete pull dialog should render headlessly");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void StartPullDialogAndPullImmediately_should_pull_from_a_local_bare_remote()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;
        try
        {
            GitModule module = CreateRepositoryWithNewRemoteCommit(out ObjectId remoteCommit);
            string branch = module.GetSelectedBranch();
            GitUICommands commands = new(_serviceContainer, module);
            FormPull owner = new(commands, branch, "origin", GitPullAction.Merge);
            owner.Show();

            bool result;
            bool pullCompleted;
            try
            {
                result = commands.StartPullDialogAndPullImmediately(
                    out pullCompleted,
                    owner,
                    remoteBranch: branch,
                    remote: "origin",
                    GitPullAction.Merge);
            }
            finally
            {
                owner.Close();
            }

            result.Should().BeTrue();
            pullCompleted.Should().BeTrue();
            module.GetCurrentCheckout().Should().Be(remoteCommit);
            File.ReadAllText(Path.Combine(_workingDirectory, "tracked.txt")).Should().Contain("remote line");
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
        }
    }

    [AvaloniaTest]
    public void StartPullDialogAndPullImmediately_should_fetch_without_changing_head()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;
        try
        {
            GitModule module = CreateRepositoryWithNewRemoteCommit(out ObjectId remoteCommit);
            ObjectId localCommit = module.GetCurrentCheckout();
            string branch = module.GetSelectedBranch();
            GitUICommands commands = new(_serviceContainer, module);
            FormPull owner = new(commands, defaultRemoteBranch: null, "origin", GitPullAction.Fetch);
            owner.Show();

            bool result;
            bool fetchCompleted;
            try
            {
                result = commands.StartPullDialogAndPullImmediately(
                    out fetchCompleted,
                    owner,
                    remoteBranch: null,
                    remote: "origin",
                    GitPullAction.Fetch);
            }
            finally
            {
                owner.Close();
            }

            result.Should().BeTrue();
            fetchCompleted.Should().BeTrue();
            module.GetCurrentCheckout().Should().Be(localCommit);
            module.RevParse($"refs/remotes/origin/{branch}").Should().Be(remoteCommit);
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
        }
    }

    [AvaloniaTest]
    public void FormPull_should_configure_fetch_and_prune_all()
    {
        GitModule module = CreateRepositoryWithNewRemoteCommit(out _);
        FormPull form = new(new GitUICommands(_serviceContainer, module), defaultRemoteBranch: null, defaultRemote: null, GitPullAction.FetchPruneAll);
        try
        {
            FormPull.TestAccessor accessor = form.GetTestAccessor();

            accessor.Fetch.IsChecked.Should().BeTrue();
            accessor.Remotes.Text.Should().Be("[ All ]");
            accessor.Prune.IsChecked.Should().BeTrue();
            accessor.PruneTags.IsChecked.Should().BeFalse();
            accessor.Merge.IsEnabled.Should().BeFalse();
            accessor.Rebase.IsEnabled.Should().BeFalse();
            accessor.LocalBranch.IsEnabled.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormPull_should_build_rebase_and_complete_fetch_arguments()
    {
        GitModule module = CreateRepositoryWithNewRemoteCommit(out _);
        FormPull form = new(new GitUICommands(_serviceContainer, module), module.GetSelectedBranch(), "origin", GitPullAction.Merge);
        try
        {
            FormPull.TestAccessor accessor = form.GetTestAccessor();
            RadioButton noTags = form.FindControl<RadioButton>("NoTags")!;
            RadioButton allTags = form.FindControl<RadioButton>("AllTags")!;
            CheckBox unshallow = form.FindControl<CheckBox>("Unshallow")!;

            accessor.Rebase.IsChecked = true;
            noTags.IsChecked = true;
            unshallow.IsChecked = true;
            using (FormProcess rebase = accessor.CreateFormProcess("origin", module.GetSelectedBranch(), module.GetSelectedBranch()))
            {
                rebase.ProcessArguments.Should().Contain("pull");
                rebase.ProcessArguments.Should().Contain("--rebase");
                rebase.ProcessArguments.Should().Contain("--no-tags");
                rebase.ProcessArguments.Should().Contain("--unshallow");
            }

            accessor.Fetch.IsChecked = true;
            allTags.IsChecked = true;
            accessor.PruneTags.IsChecked = true;
            using FormProcess fetch = accessor.CreateFormProcess("origin", localBranch: null, remoteBranch: null);
            fetch.ProcessArguments.Should().Contain("fetch");
            fetch.ProcessArguments.Should().Contain("--tags");
            fetch.ProcessArguments.Should().Contain("--unshallow");
            fetch.ProcessArguments.Should().Contain("--prune");
            fetch.ProcessArguments.Should().Contain("--prune-tags");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormPull_should_pull_from_a_portable_local_path_without_a_configured_remote()
    {
        AppSettings.CloseProcessDialog = true;
        GitModule module = CreateRepositoryWithNewRemoteCommit(out ObjectId remoteCommit);
        string branch = module.GetSelectedBranch();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "remove", "origin" });
        FormPull form = new(new GitUICommands(_serviceContainer, module), branch, defaultRemote: null, GitPullAction.Merge);
        try
        {
            form.FindControl<RadioButton>("PullFromUrl")!.IsChecked = true;
            form.FindControl<ComboBox>("comboBoxPullSource")!.Text = _bareRemoteDirectory;
            form.FindControl<ComboBox>("Branches")!.Text = branch;

            form.PullChanges(form).Should().Be(WinFormsShims.DialogResult.OK);
            form.ErrorOccurred.Should().BeFalse();
            module.GetCurrentCheckout().Should().Be(remoteCommit);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormPull_should_restore_auto_stashed_changes_after_pull()
    {
        AppSettings.CloseProcessDialog = true;
        AppSettings.AutoPopStashAfterPull = true;
        GitModule module = CreateRepositoryWithNewRemoteCommit(out ObjectId remoteCommit);
        string localFile = Path.Combine(_workingDirectory, "local.txt");
        File.WriteAllText(localFile, "local edit\n");
        FormPull form = new(new GitUICommands(_serviceContainer, module), module.GetSelectedBranch(), "origin", GitPullAction.Merge);
        try
        {
            form.GetTestAccessor().AutoStash.IsChecked = true;

            form.PullChanges(form).Should().Be(WinFormsShims.DialogResult.OK);
            form.ErrorOccurred.Should().BeFalse();
            module.GetCurrentCheckout().Should().Be(remoteCommit);
            File.ReadAllLines(localFile).Should().Equal("local edit");
            module.GitExecutable.GetOutput(new GitArgumentBuilder("stash") { "list" }).Should().BeEmpty();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormPull_should_offer_conflict_recovery_after_a_failed_merge()
    {
        AppSettings.DontConfirmResolveConflicts = false;
        GitModule module = CreateRepositoryWithNewRemoteCommit(out _);
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "local line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "local-conflict" });
        FormPull form = new(new GitUICommands(_serviceContainer, module), module.GetSelectedBranch(), "origin", GitPullAction.Merge);
        try
        {
            FormPull.TestAccessor accessor = form.GetTestAccessor();
            using FormProcess process = accessor.CreateFormProcess("origin", module.GetSelectedBranch(), module.GetSelectedBranch());
            process.Show();
            DateTime timeout = DateTime.UtcNow.AddSeconds(30);
            while (!process.Ok.IsEnabled && DateTime.UtcNow < timeout)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(25);
            }

            process.Ok.IsEnabled.Should().BeTrue("the conflicting pull process should finish");
            process.ErrorOccurred().Should().BeTrue();
            process.Close();
            module.InTheMiddleOfConflictedMerge().Should().BeTrue();
            accessor.CheckMergeConflictsOnError(form).Should().BeTrue();
            _messageBoxes.Messages.Should().NotBeEmpty("the failed pull should offer the shared conflict-resolution flow");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormPull_should_initialize_a_newly_pulled_submodule()
    {
        AppSettings.CloseProcessDialog = true;
        AppSettings.UpdateSubmodulesOnCheckout = true;
        GitModule module = CreateRepositoryWithNewRemoteCommit(out _);
        AddSubmoduleToPublisher(module);
        module.SetSetting("protocol.file.allow", "always");
        string? originalAllowedProtocols = Environment.GetEnvironmentVariable("GIT_ALLOW_PROTOCOL");
        Environment.SetEnvironmentVariable("GIT_ALLOW_PROTOCOL", "file");
        FormPull form = new(new GitUICommands(_serviceContainer, module), module.GetSelectedBranch(), "origin", GitPullAction.Merge);
        try
        {
            form.PullChanges(form).Should().Be(WinFormsShims.DialogResult.OK);
            form.ErrorOccurred.Should().BeFalse();
            module.GetSubmodule("deps/sample").IsValidGitWorkingDir().Should().BeTrue();
        }
        finally
        {
            form.Close();
            Environment.SetEnvironmentVariable("GIT_ALLOW_PROTOCOL", originalAllowedProtocols);
        }
    }

    private GitModule CreateRepositoryWithNewRemoteCommit(out ObjectId remoteCommit)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "initial line\n");
        File.WriteAllText(Path.Combine(_workingDirectory, "local.txt"), "initial local\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt", "local.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        string branch = module.GetSelectedBranch();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare", _bareRemoteDirectory });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", _bareRemoteDirectory });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "-u", "origin", branch });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("clone") { "--quiet", _bareRemoteDirectory, _publisherDirectory });

        GitModule publisher = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _publisherDirectory);
        publisher.SetSetting("user.name", "Avalonia Publisher");
        publisher.SetSetting("user.email", "publisher@example.com");
        File.WriteAllText(Path.Combine(_publisherDirectory, "tracked.txt"), "remote line\n");
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "remote-commit" });
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "origin", branch });
        remoteCommit = publisher.GetCurrentCheckout();
        return module;
    }

    private void AddSubmoduleToPublisher(GitModule module)
    {
        Directory.CreateDirectory(_submoduleSeedDirectory);
        GitModule seed = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _submoduleSeedDirectory);
        seed.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        seed.SetSetting("user.name", "Avalonia Submodule");
        seed.SetSetting("user.email", "submodule@example.com");
        File.WriteAllText(Path.Combine(_submoduleSeedDirectory, "README.md"), "submodule\n");
        seed.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "README.md" });
        seed.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "submodule-initial" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("clone") { "--quiet", "--bare", _submoduleSeedDirectory, _submoduleBareDirectory });

        GitModule publisher = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _publisherDirectory);
        GitArgumentBuilder addSubmodule = new("submodule")
        {
            new GitConfigItem("protocol.file.allow", "always"),
            "add",
            _submoduleBareDirectory.Quote(),
            "deps/sample",
        };
        publisher.GitExecutable.RunCommand(addSubmodule);
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "add-submodule" });
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "origin", module.GetSelectedBranch() });
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Result { get; set; }

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return buttons is WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel
                ? Result
                : WinFormsShims.DialogResult.OK;
        }
    }
}

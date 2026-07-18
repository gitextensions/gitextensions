using System.ComponentModel.Design;
using Avalonia.Controls;
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
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormPullTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _bareRemoteDirectory = null!;
    private string _publisherDirectory = null!;
    private string _workingDirectory = null!;

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

        string testId = Guid.NewGuid().ToString("N");
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}");
        _publisherDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}-publisher");
        _bareRemoteDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PullTests-{testId}.git");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        DeleteDirectory(_workingDirectory);
        DeleteDirectory(_publisherDirectory);
        DeleteDirectory(_bareRemoteDirectory);

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
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Merge", "Text", "&Merge remote branch into current branch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Rebase", "Text", "R&ebase current branch on top of remote branch, creates linear history (use with caution)");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "Fetch", "Text", "Do not merge, only &fetch remote changes");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "lblLocalBranch", "Text", "&Local branch");
        translation.Received(1).AddTranslationItem(nameof(FormPull), "lblRemoteBranch", "Text", "Rem&ote branch");
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
            TextBox remoteBranch = form.FindControl<TextBox>("Branches")
                ?? throw new InvalidOperationException("Remote branch field was not created.");
            Button pull = form.FindControl<Button>("Pull")
                ?? throw new InvalidOperationException("Pull button was not created.");
            TextBlock localBranchLabel = form.FindControl<TextBlock>("lblLocalBranch")
                ?? throw new InvalidOperationException("Local branch label was not created.");
            TextBlock remoteBranchLabel = form.FindControl<TextBlock>("lblRemoteBranch")
                ?? throw new InvalidOperationException("Remote branch label was not created.");

            remotes.SelectedItem.Should().Be("origin");
            localBranch.Text.Should().Be(module.GetSelectedBranch());
            remoteBranch.Text.Should().Be(module.GetSelectedBranch());
            localBranchLabel.Text.Should().Be("Local branch");
            remoteBranchLabel.Text.Should().Be("Remote branch");
            pull.Content.Should().Be("_Fetch");
            pull.IsEnabled.Should().BeTrue();
            form.Title.Should().StartWith("Fetch (");
            form.CaptureRenderedFrame().Should().NotBeNull("the reduced pull dialog should render headlessly");
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

    private GitModule CreateRepositoryWithNewRemoteCommit(out ObjectId remoteCommit)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        string branch = module.GetSelectedBranch();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare", _bareRemoteDirectory });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", _bareRemoteDirectory });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "-u", "origin", branch });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("clone") { "--quiet", _bareRemoteDirectory, _publisherDirectory });

        GitModule publisher = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _publisherDirectory);
        publisher.SetSetting("user.name", "Avalonia Publisher");
        publisher.SetSetting("user.email", "publisher@example.com");
        File.AppendAllText(Path.Combine(_publisherDirectory, "tracked.txt"), "remote line\n");
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "remote-commit" });
        publisher.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "origin", branch });
        remoteCommit = publisher.GetCurrentCheckout();
        return module;
    }
}

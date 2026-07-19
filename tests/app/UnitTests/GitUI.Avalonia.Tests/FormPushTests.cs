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
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormPushTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _bareRemoteDirectory = null!;
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
        WinFormsShims.ShimHost.MessageBoxHost = new StubMessageBoxHost { Result = WinFormsShims.DialogResult.Yes };

        string testId = Guid.NewGuid().ToString("N");
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PushTests-{testId}");
        _bareRemoteDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PushTests-{testId}.git");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
        if (Directory.Exists(_bareRemoteDirectory))
        {
            TestDirectory.Delete(_bareRemoteDirectory);
        }
    }

    [AvaloniaTest]
    public void FormPush_should_construct_and_use_existing_translation_keys()
    {
        FormPush form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormPush), "$this", "Text", "Push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "groupBox2", "Text", "Push to");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "labelFrom", "Text", "&Branch to push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "labelTo", "Text", "&to");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "ckForceWithLease", "Text", "&Force with lease");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "AddRemote", "Text", "&Manage remotes");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "BranchTab", "Text", "Push branches");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "TagTab", "Text", "Push tags");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "MultipleBranchTab", "Text", "Push multiple branches");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "ForcePushBranches", "Text", "F&orce push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "ForcePushTags", "Text", "&Force push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "ReplaceTrackingReference", "Text", "R&eplace tracking reference");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "RecursiveSubmodules", "Item2", "On-demand");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "LocalColumn", "HeaderText", "Local Branch");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "DeleteColumn", "HeaderText", "Delete Remote Branch");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "PushToRemote", "toolTip1", "Remote repository to push to");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "BranchTab", "ToolTipText", "Push branches and commits to remote repository.");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "folderBrowserButton1", "Text", "Bro&wse...");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_forceWithLeaseTooltips", "Text", Arg.Any<string>());
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_noCurrentBranch", "Text", "No branch is selected, cannot push.");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_pushCaption", "Text", "Push");
        translation.Received(1).AddTranslationItem(nameof(FormPush), "_pushToCaption", "Text", "Push to {0}");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
        form.Close();
    }

    [AvaloniaTest]
    public void FormPush_should_show_the_current_branch_selected_remote_and_force_with_lease()
    {
        GitModule module = CreateRepositoryAndRemote();
        FormPush form = new(new GitUICommands(_serviceContainer, module));

        form.Show();
        try
        {
            ComboBox remotes = form.FindControl<ComboBox>("_NO_TRANSLATE_Remotes")
                ?? throw new InvalidOperationException("Remote selector was not created.");
            ComboBox branch = form.FindControl<ComboBox>("_NO_TRANSLATE_Branch")
                ?? throw new InvalidOperationException("Branch field was not created.");
            ComboBox remoteBranch = form.FindControl<ComboBox>("RemoteBranch")
                ?? throw new InvalidOperationException("Remote branch field was not created.");
            CheckBox forceWithLease = form.FindControl<CheckBox>("ckForceWithLease")
                ?? throw new InvalidOperationException("Force-with-lease checkbox was not created.");
            Button push = form.FindControl<Button>("Push")
                ?? throw new InvalidOperationException("Push button was not created.");
            TextBlock labelFrom = form.FindControl<TextBlock>("labelFrom")
                ?? throw new InvalidOperationException("Branch label was not created.");
            TextBlock labelTo = form.FindControl<TextBlock>("labelTo")
                ?? throw new InvalidOperationException("Remote-branch label was not created.");

            remotes.SelectedItem.Should().Be("origin");
            labelFrom.Text.Should().Be("Branch to push");
            labelTo.Text.Should().Be("to");
            (branch.SelectedItem as string ?? branch.Text).Should().Be(module.GetSelectedBranch());
            remoteBranch.Text.Should().Be(branch.Text);
            push.IsEnabled.Should().BeTrue();
            forceWithLease.IsChecked.Should().BeFalse();

            form.CheckForceWithLease();

            forceWithLease.IsChecked.Should().BeTrue();
            form.CaptureRenderedFrame().Should().NotBeNull("the reduced push dialog should render headlessly");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void StartPushDialog_should_push_the_current_branch_to_a_local_bare_remote()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        bool dontConfirmNewBranch = AppSettings.DontConfirmPushNewBranch;
        bool dontConfirmTracking = AppSettings.DontConfirmAddTrackingRef;
        AppSettings.CloseProcessDialog = true;
        AppSettings.DontConfirmPushNewBranch = true;
        AppSettings.DontConfirmAddTrackingRef = true;
        try
        {
            GitModule module = CreateRepositoryAndRemote();
            ObjectId localCommit = module.GetCurrentCheckout();
            string branch = module.GetSelectedBranch();
            GitUICommands commands = new(_serviceContainer, module);
            FormPush owner = new(commands);
            owner.Show();

            bool result;
            bool pushCompleted;
            try
            {
                result = commands.StartPushDialog(owner, pushOnShow: true, forceWithLease: false, out pushCompleted);
            }
            finally
            {
                owner.Close();
            }

            GitModule remoteModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _bareRemoteDirectory);
            result.Should().BeTrue();
            pushCompleted.Should().BeTrue();
            remoteModule.RevParse($"refs/heads/{branch}").Should().Be(localCommit);
            module.GetSetting($"branch.{branch}.remote").Should().Be("origin");
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
            AppSettings.DontConfirmPushNewBranch = dontConfirmNewBranch;
            AppSettings.DontConfirmAddTrackingRef = dontConfirmTracking;
        }
    }

    [AvaloniaTest]
    public void FormPush_should_push_to_a_portable_local_path_without_a_configured_remote()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;
        GitModule module = CreateRepositoryAndRemote();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "remove", "origin" });
        string branch = module.GetSelectedBranch();
        ObjectId localCommit = module.GetCurrentCheckout();
        FormPush form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.Show();
            form.FindControl<RadioButton>("PushToUrl")!.IsChecked = true;
            form.FindControl<ComboBox>("PushDestination")!.Text = _bareRemoteDirectory;
            Button push = form.FindControl<Button>("Push")!;
            push.IsEnabled.Should().BeTrue("an absolute local destination is valid on every desktop platform");
            push.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            GitModule remoteModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _bareRemoteDirectory);
            remoteModule.RevParse($"refs/heads/{branch}").Should().Be(localCommit);
            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void FormPush_should_push_a_selected_tag_to_the_remote()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        AppSettings.CloseProcessDialog = true;
        GitModule module = CreateRepositoryAndRemote();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { "v1.0" });
        FormPush form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.Show();
            form.FindControl<TabControl>("TabControlTagBranch")!.SelectedItem = form.FindControl<TabItem>("TagTab")!;
            form.FindControl<ComboBox>("TagComboBox")!.Text = "v1.0";
            Button push = form.FindControl<Button>("Push")!;
            push.IsEnabled.Should().BeTrue();
            push.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            GitModule remoteModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _bareRemoteDirectory);
            remoteModule.RevParse("refs/tags/v1.0").Should().Be(module.RevParse("refs/tags/v1.0"));
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void StartPushDialog_should_pull_merge_and_retry_a_rejected_push()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        GitPullAction? autoPull = AppSettings.AutoPullOnPushRejectedAction;
        GitPullAction defaultPull = AppSettings.DefaultPullAction;
        bool dontConfirmNewBranch = AppSettings.DontConfirmPushNewBranch;
        bool dontConfirmTracking = AppSettings.DontConfirmAddTrackingRef;
        string updaterDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.PushUpdater-{Guid.NewGuid():N}");
        AppSettings.CloseProcessDialog = true;
        AppSettings.AutoPullOnPushRejectedAction = GitPullAction.Merge;
        AppSettings.DefaultPullAction = GitPullAction.Merge;
        AppSettings.DontConfirmPushNewBranch = true;
        AppSettings.DontConfirmAddTrackingRef = true;
        try
        {
            GitModule module = CreateRepositoryAndRemote();
            string branch = module.GetSelectedBranch();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "-u", "origin", $"{branch}:{branch}" });

            module.GitExecutable.RunCommand(new GitArgumentBuilder("clone") { "--quiet", _bareRemoteDirectory, updaterDirectory });
            GitModule updater = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), updaterDirectory);
            updater.SetSetting("user.name", "Remote Test");
            updater.SetSetting("user.email", "remote@example.com");
            File.WriteAllText(Path.Combine(updaterDirectory, "remote.txt"), "remote change\n");
            updater.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "remote.txt" });
            updater.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "remote change".Quote() });
            updater.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "--quiet", "origin", branch });

            File.WriteAllText(Path.Combine(_workingDirectory, "local.txt"), "local change\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "local.txt" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "local change".Quote() });

            GitUICommands commands = new(_serviceContainer, module);
            FormPush owner = new(commands);
            owner.Show();
            bool result;
            bool pushCompleted;
            try
            {
                result = commands.StartPushDialog(owner, pushOnShow: true, forceWithLease: false, out pushCompleted);
            }
            finally
            {
                owner.Close();
            }

            GitModule remoteModule = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _bareRemoteDirectory);
            result.Should().BeTrue();
            pushCompleted.Should().BeTrue();
            remoteModule.RevParse($"refs/heads/{branch}").Should().Be(module.GetCurrentCheckout());
            module.GetRevision(module.GetCurrentCheckout(), shortFormat: false, loadRefs: false).ParentIds
                .Should().HaveCount(2, "the rejected push should pull with merge before retrying");
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
            AppSettings.AutoPullOnPushRejectedAction = autoPull;
            AppSettings.DefaultPullAction = defaultPull;
            AppSettings.DontConfirmPushNewBranch = dontConfirmNewBranch;
            AppSettings.DontConfirmAddTrackingRef = dontConfirmTracking;
            if (Directory.Exists(updaterDirectory))
            {
                TestDirectory.Delete(updaterDirectory);
            }
        }
    }

    private GitModule CreateRepositoryAndRemote()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare", _bareRemoteDirectory });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", _bareRemoteDirectory });
        return module;
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

    [AvaloniaTest]
    public void FormPush_should_build_branch_all_and_tag_push_arguments()
    {
        GitModule module = CreateRepositoryAndRemote();
        FormPush form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            ComboBox branches = form.FindControl<ComboBox>("_NO_TRANSLATE_Branch")!;
            ComboBox remoteBranch = form.FindControl<ComboBox>("RemoteBranch")!;
            ComboBox recursiveSubmodules = form.FindControl<ComboBox>("RecursiveSubmodules")!;
            CheckBox forceWithLease = form.FindControl<CheckBox>("ckForceWithLease")!;
            TabControl tabs = form.FindControl<TabControl>("TabControlTagBranch")!;
            TabItem branchTab = form.FindControl<TabItem>("BranchTab")!;
            TabItem tagTab = form.FindControl<TabItem>("TagTab")!;
            ComboBox tags = form.FindControl<ComboBox>("TagComboBox")!;
            CheckBox forceTags = form.FindControl<CheckBox>("ForcePushTags")!;
            FormPush.TestAccessor accessor = form.GetTestAccessor();

            tabs.SelectedItem = branchTab;
            branches.SelectedItem = module.GetSelectedBranch();
            branches.Text = module.GetSelectedBranch();
            remoteBranch.Text = "published-main";
            recursiveSubmodules.SelectedIndex = 2;
            forceWithLease.IsChecked = true;
            string branchArguments = accessor.CreatePushArguments("origin", track: true).ToString();
            branchArguments.Should().Contain("--force-with-lease");
            branchArguments.Should().Contain("--recurse-submodules=on-demand");
            branchArguments.Should().Contain("-u");
            branchArguments.Should().Contain("refs/heads/published-main");

            branches.SelectedItem = "[ All ]";
            branches.Text = "[ All ]";
            accessor.CreatePushArguments("origin").ToString().Should().Contain("--all");

            tabs.SelectedItem = tagTab;
            tags.Text = "v1.0";
            forceTags.IsChecked = true;
            string tagArguments = accessor.CreatePushArguments("origin").ToString();
            tagArguments.Should().Contain("push -f --progress");
            tagArguments.Should().Contain("tag v1.0");

            tags.Text = "[ All ]";
            accessor.CreatePushArguments("origin").ToString().Should().Contain("--tags");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormPush_should_build_mutually_exclusive_multiple_branch_actions()
    {
        GitModule module = CreateRepositoryAndRemote();
        string currentBranch = module.GetSelectedBranch();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "remote-only" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("push") { "origin", $"{currentBranch}:{currentBranch}", "feature:feature", "remote-only:remote-only" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "-D", "remote-only" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("fetch") { "origin" });

        FormPush form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            TabControl tabs = form.FindControl<TabControl>("TabControlTagBranch")!;
            tabs.SelectedItem = form.FindControl<TabItem>("MultipleBranchTab")!;
            FormPush.TestAccessor accessor = form.GetTestAccessor();
            accessor.UpdateMultiBranchView();

            FormPush.BranchPushRow current = accessor.BranchRows.Single(row => row.LocalBranch == currentBranch);
            FormPush.BranchPushRow feature = accessor.BranchRows.Single(row => row.LocalBranch == "feature");
            FormPush.BranchPushRow remoteOnly = accessor.BranchRows.Single(row => row.RemoteBranch == "remote-only" && !row.CanPush);
            current.SetPush(true);
            feature.SetForce(true);
            remoteOnly.SetDelete(true);

            string arguments = accessor.CreatePushArguments("origin").ToString();
            arguments.Should().Contain($"refs/heads/{currentBranch}:refs/heads/{currentBranch}");
            arguments.Should().Contain("+refs/heads/feature:refs/heads/feature");
            arguments.Should().Contain(":refs/heads/remote-only");

            feature.SetDelete(true);
            feature.Push.Should().BeFalse();
            feature.Force.Should().BeFalse();
            feature.Delete.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }
}

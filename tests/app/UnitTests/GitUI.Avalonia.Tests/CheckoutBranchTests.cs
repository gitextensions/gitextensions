using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.ScriptsEngine;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CheckoutBranchTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormCheckoutBranch_should_populate_local_branches_and_collapse_remote_options()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");

        FormCheckoutBranch form = new(commands, branch: string.Empty, remote: false);

        ComboBox branches = form.FindControl<ComboBox>("Branches")
            ?? throw new InvalidOperationException("Branch selector was not created.");
        Control remoteBranch = form.FindControl<Control>("Remotebranch")
            ?? throw new InvalidOperationException("Remote branch selector was not created.");
        Control remoteOptions = form.FindControl<Control>("tlpnlRemoteOptions")
            ?? throw new InvalidOperationException("Remote options were not created.");

        branches.ItemsSource!.Cast<string>().Should().BeEquivalentTo(
            new[] { "feature", "main" },
            options => options.WithStrictOrdering());
        remoteBranch.IsVisible.Should().BeTrue("checking out a remote branch is offered");
        remoteOptions.IsVisible.Should().BeFalse("the remote options only apply to a remote branch");
    }

    [AvaloniaTest]
    public void Switching_to_remote_branches_should_populate_them_and_show_the_remote_options()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");

        FormCheckoutBranch form = new(commands, branch: string.Empty, remote: false);
        ComboBox branches = form.FindControl<ComboBox>("Branches")!;
        RadioButton remoteBranch = form.FindControl<RadioButton>("Remotebranch")!;
        Control remoteOptions = form.FindControl<Control>("tlpnlRemoteOptions")!;
        RadioButton resetBranch = form.FindControl<RadioButton>("rbResetBranch")!;
        TextBox customBranchName = form.FindControl<TextBox>("txtCustomBranchName")!;

        remoteBranch.IsChecked = true;

        remoteOptions.IsVisible.Should().BeTrue();
        branches.ItemsSource!.Cast<string>().Should().BeEquivalentTo(
            new[] { "origin/feature", "origin/main" },
            options => options.WithStrictOrdering());

        branches.SelectedItem = "origin/feature";

        customBranchName.Text.Should().Be("origin_feature", "the suggested name combines remote and branch");
        resetBranch.Content.Should().Be(
            "Cr_eate local branch with same name:",
            "no local tracking branch exists, so reset becomes create");
    }

    [AvaloniaTest]
    public void OkClick_should_run_checkout_for_the_selected_local_branch()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature");
        TestScriptEventRecorder scriptEvents = (TestScriptEventRecorder)commands.GetRequiredService<IScriptsRunner>();
        module.IsDirtyDir().Returns(true);
        IGitCommand? checkoutCommand = null;
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<IGitCommand>(command => checkoutCommand = command))
            .Returns(true);

        FormCheckoutBranch form = new(commands, branch: string.Empty, remote: false);
        form.Show();
        try
        {
            ComboBox branches = form.FindControl<ComboBox>("Branches")
                ?? throw new InvalidOperationException("Branch selector was not created.");
            Button ok = form.FindControl<Button>("Ok")
                ?? throw new InvalidOperationException("Checkout button was not created.");
            RadioButton merge = form.FindControl<RadioButton>("rbMerge")
                ?? throw new InvalidOperationException("Merge-local-changes option was not created.");

            branches.SelectedItem = "feature";
            merge.IsChecked = true;
            ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            checkoutCommand.Should().NotBeNull();
            checkoutCommand!.ChangesRepoState.Should().BeTrue();
            checkoutCommand.Arguments.ToString().Should().Be("checkout --merge \"feature\"");
            scriptEvents.Events.Should().Equal(ScriptEvent.BeforeCheckout, ScriptEvent.AfterCheckout);
            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
            module.Received().GetRefs(RefsFilter.Heads);
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void OkClick_should_checkout_a_remote_branch_tracking_a_new_custom_branch()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");
        IGitCommand? checkoutCommand = null;
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<IGitCommand>(command => checkoutCommand = command))
            .Returns(true);

        FormCheckoutBranch form = new(commands, branch: "origin/feature", remote: true);
        form.Show();
        try
        {
            RadioButton createWithCustomName = form.FindControl<RadioButton>("rbCreateBranchWithCustomName")!;
            TextBox customBranchName = form.FindControl<TextBox>("txtCustomBranchName")!;
            Button ok = form.FindControl<Button>("Ok")!;

            createWithCustomName.IsChecked = true;
            customBranchName.Text.Should().Be("origin_feature");
            ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            checkoutCommand.Should().NotBeNull();
            checkoutCommand!.Arguments.ToString().Should().Be("checkout -b \"origin_feature\" --track \"origin/feature\"");
            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void OkClick_should_checkout_a_remote_branch_in_detached_head()
    {
        (IGitUICommands commands, _) = CreateCommands("main", "feature");
        IGitCommand? checkoutCommand = null;
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<IGitCommand>(command => checkoutCommand = command))
            .Returns(true);

        FormCheckoutBranch form = new(commands, branch: "origin/feature", remote: true);
        form.Show();
        try
        {
            RadioButton dontCreate = form.FindControl<RadioButton>("rbDontCreate")!;
            Button ok = form.FindControl<Button>("Ok")!;

            dontCreate.IsChecked = true;
            ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            checkoutCommand.Should().NotBeNull();
            checkoutCommand!.Arguments.ToString().Should().Be("checkout \"origin/feature\"");
            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void OkClick_should_stash_before_checkout_and_pop_afterwards()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature");
        module.IsDirtyDir().Returns(true);
        commands.StartCommandLineProcessDialog(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<IGitCommand>()).Returns(true);

        bool? original = AppSettings.AutoPopStashAfterCheckoutBranch;
        try
        {
            // Answer the "apply stashed items again?" question without showing it.
            AppSettings.AutoPopStashAfterCheckoutBranch = true;

            FormCheckoutBranch form = new(commands, branch: string.Empty, remote: false);
            form.Show();
            try
            {
                ComboBox branches = form.FindControl<ComboBox>("Branches")!;
                RadioButton stash = form.FindControl<RadioButton>("rbStash")!;
                Button ok = form.FindControl<Button>("Ok")!;

                stash.IsVisible.Should().BeTrue("stashing local changes is offered again");

                branches.SelectedItem = "feature";
                stash.IsChecked = true;
                ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                commands.Received(1).StashSave(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>());
                commands.Received(1).StashPop(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<string>());
                form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
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
            AppSettings.AutoPopStashAfterCheckoutBranch = original;
        }
    }

    [AvaloniaTest]
    public void OkClick_should_not_stash_a_clean_working_directory()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature");
        module.IsDirtyDir().Returns(false);
        commands.StartCommandLineProcessDialog(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<IGitCommand>()).Returns(true);

        FormCheckoutBranch form = new(commands, branch: string.Empty, remote: false);
        form.Show();
        try
        {
            ComboBox branches = form.FindControl<ComboBox>("Branches")!;
            RadioButton stash = form.FindControl<RadioButton>("rbStash")!;
            Button ok = form.FindControl<Button>("Ok")!;

            branches.SelectedItem = "feature";
            stash.IsChecked = true;
            ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            commands.DidNotReceive().StashSave(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>());
            commands.DidNotReceive().StashPop(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<string>());
        }
        finally
        {
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void OkClick_should_checkout_a_remote_branch_in_a_real_repository()
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CheckoutRemote-{Guid.NewGuid():N}");
        string upstreamDirectory = Path.Combine(root, "upstream");
        string workingDirectory = Path.Combine(root, "clone");
        Directory.CreateDirectory(upstreamDirectory);
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        try
        {
            AppSettings.CloseProcessDialog = true;

            using ServiceContainer services = CreateServiceContainer();
            IGitExecutorProvider executorProvider = services.GetRequiredService<IGitExecutorProvider>();

            GitModule upstream = new(executorProvider, upstreamDirectory);
            upstream.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
            upstream.SetSetting("user.name", "Avalonia Test");
            upstream.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(upstreamDirectory, "tracked.txt"), "content");
            upstream.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
            upstream.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            upstream.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" }).Should().BeTrue();

            GitModule rootModule = new(executorProvider, root);
            rootModule.GitExecutable.RunCommand(new GitArgumentBuilder("clone") { "--quiet", "upstream", "clone" }).Should().BeTrue();

            GitModule module = new(executorProvider, workingDirectory);
            GitUICommands commands = new(services, module);
            FormCheckoutBranch form = new(commands, "origin/feature", remote: true);
            form.Show();
            try
            {
                RadioButton createWithCustomName = form.FindControl<RadioButton>("rbCreateBranchWithCustomName")!;
                Button ok = form.FindControl<Button>("Ok")!;

                createWithCustomName.IsChecked = true;
                ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();

                form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
                module.GetSelectedBranch().Should().Be("origin_feature", "the tracking branch was created and checked out");
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
            AppSettings.CloseProcessDialog = closeProcessDialog;
            TestDirectory.Delete(root);
        }
    }

    [Test]
    public void StartCheckoutBranch_should_honor_pre_checkout_cancellation()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(true);
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);
        commands.PreCheckoutBranch += (_, e) => e.Cancel = true;

        bool result = commands.StartCheckoutBranch(owner: null);

        result.Should().BeFalse();
        module.Received(1).IsValidGitWorkingDir();
    }

    [Test]
    public void DoActionOnRepo_should_notify_after_a_successful_action()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(true);
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);
        int notifications = 0;
        commands.PostRepositoryChanged += (_, _) => notifications++;

        bool result = commands.DoActionOnRepo(() => true);

        result.Should().BeTrue();
        notifications.Should().Be(1);
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands(params string[] branchNames)
    {
        IReadOnlyList<IGitRef> branches = branchNames.Select(CreateBranch).ToList();
        IReadOnlyList<IGitRef> remoteBranches = branchNames.Select(name => CreateBranch($"origin/{name}")).ToList();
        IGitModule module = Substitute.For<IGitModule>();
        module.IsDirtyDir().Returns(false);
        module.GetCurrentCheckout().Returns(default(ObjectId));
        module.GetRefs(RefsFilter.Heads).Returns(branches);
        module.GetRefs(RefsFilter.Remotes).Returns(remoteBranches);
        module.GetRemoteNames().Returns(["origin"]);
        module.GetLocalTrackingBranchName(Arg.Any<string>(), Arg.Any<string>()).Returns((string?)null);
        module.CheckBranchFormat(Arg.Any<string>()).Returns(true);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IGitBranchNameNormaliser)).Returns(Substitute.For<IGitBranchNameNormaliser>());
        commands.GetService(typeof(IScriptsRunner)).Returns(new TestScriptEventRecorder());
        return (commands, module);

        static IGitRef CreateBranch(string name)
        {
            IGitRef branch = Substitute.For<IGitRef>();
            branch.Name.Returns(name);
            return branch;
        }
    }

    private static ServiceContainer CreateServiceContainer()
    {
        ServiceContainer services = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        services.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        GitCommands.ServiceContainerRegistry.RegisterServices(services);
        GitUI.ServiceContainerRegistry.RegisterServices(services);
        return services;
    }
}

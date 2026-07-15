using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
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
    public void FormCheckoutBranch_should_populate_local_branches_and_hide_remote_options()
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
        remoteBranch.IsVisible.Should().BeFalse();
        remoteOptions.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void OkClick_should_run_checkout_for_the_selected_local_branch()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands("main", "feature");
        module.IsDirtyDir().Returns(true);
        IGitCommand? checkoutCommand = null;
        commands.StartCommandLineProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<IGitCommand>(command => checkoutCommand = command))
            .Returns(true);

        FormCheckoutBranch form = new(commands, branch: string.Empty, remote: false);
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
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        module.Received().GetRefs(RefsFilter.Heads);
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
        IGitModule module = Substitute.For<IGitModule>();
        module.IsDirtyDir().Returns(false);
        module.GetCurrentCheckout().Returns(default(ObjectId));
        module.GetRefs(RefsFilter.Heads).Returns(branches);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);

        static IGitRef CreateBranch(string name)
        {
            IGitRef branch = Substitute.For<IGitRef>();
            branch.Name.Returns(name);
            return branch;
        }
    }
}

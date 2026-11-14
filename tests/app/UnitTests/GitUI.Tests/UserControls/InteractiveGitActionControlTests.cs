using CommonTestUtils;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls;
using NSubstitute;

namespace GitUITests.UserControls;

[Apartment(ApartmentState.STA)]
public class InteractiveGitActionControlTests
{
    private InteractiveGitActionControl _control = null!;
    private InteractiveGitActionControl.TestAccessor _accessor;

    [SetUp]
    public void SetUp()
    {
        _control = new InteractiveGitActionControl();
        _accessor = _control.GetTestAccessor();

        IGitUICommands uiCommands = Substitute.For<IGitUICommands>();
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Returns(uiCommands);
        _control.UICommandsSource = uiCommandsSource;
    }

    [TearDown]
    public void TearDown()
    {
        _control.Dispose();
    }

    [Test]
    public async Task RefreshGitAction_should_check_for_conflicts_in_the_background()
    {
        bool? checkedOnMainThread = null;
        IGitModule module = CreateModule();
        module.InTheMiddleOfConflictedMerge(throwOnErrorExit: false).Returns(_ =>
        {
            checkedOnMainThread = ThreadHelper.JoinableTaskContext.IsOnMainThread;
            return true;
        });
        module.InTheMiddleOfMerge().Returns(true);
        SetModule(module);

        _control.RefreshGitAction(checkForConflicts: true);
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        checkedOnMainThread.Should().BeFalse();
        _accessor.Action.Should().Be(InteractiveGitActionControl.GitAction.Merge);
        _accessor.HasConflicts.Should().BeTrue();
        _accessor.Visible.Should().BeTrue();
    }

    [Test]
    public async Task RefreshGitAction_should_not_check_for_conflicts_if_not_allowed()
    {
        IGitModule module = CreateModule();
        module.InTheMiddleOfRebase().Returns(true);
        SetModule(module);

        _control.RefreshGitAction(checkForConflicts: false);
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        module.DidNotReceiveWithAnyArgs().InTheMiddleOfConflictedMerge();
        _accessor.Action.Should().Be(InteractiveGitActionControl.GitAction.Rebase);
        _accessor.HasConflicts.Should().BeFalse();
    }

    [Test]
    public async Task RefreshGitAction_should_ignore_invalid_working_directory()
    {
        IGitModule module = CreateModule();
        module.IsValidGitWorkingDir().Returns(false);
        SetModule(module);

        _control.RefreshGitAction(checkForConflicts: true);
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        module.DidNotReceiveWithAnyArgs().InTheMiddleOfConflictedMerge();
        _accessor.Action.Should().Be(InteractiveGitActionControl.GitAction.Unknown);
    }

    [Test]
    public async Task RefreshGitAction_should_apply_the_latest_request_only()
    {
        TaskCompletionSource firstCheckStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseFirstCheck = new();
        int conflictChecks = 0;
        IGitModule module = CreateModule();
        module.InTheMiddleOfConflictedMerge(throwOnErrorExit: false).Returns(_ =>
        {
            if (Interlocked.Increment(ref conflictChecks) != 1)
            {
                return false;
            }

            firstCheckStarted.SetResult();
            releaseFirstCheck.Wait(AsyncTestHelper.UnexpectedTimeout);
            return true;
        });
        module.InTheMiddleOfMerge().Returns(true);
        SetModule(module);

        _control.RefreshGitAction(checkForConflicts: true);
        await firstCheckStarted.Task;
        _control.RefreshGitAction(checkForConflicts: true);

        using CancellationTokenSource timeout = new(AsyncTestHelper.UnexpectedTimeout);
        while (_accessor.Action != InteractiveGitActionControl.GitAction.Merge)
        {
            await Task.Delay(millisecondsDelay: 10, timeout.Token);
        }

        releaseFirstCheck.Set();
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        _accessor.HasConflicts.Should().BeFalse();
    }

    [Test]
    public async Task RefreshGitAction_should_cancel_the_pending_check_if_the_working_directory_is_not_valid_anymore()
    {
        TaskCompletionSource checkStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseCheck = new();
        IGitModule module = CreateModule();
        module.InTheMiddleOfConflictedMerge(throwOnErrorExit: false).Returns(_ =>
        {
            checkStarted.SetResult();
            releaseCheck.Wait(AsyncTestHelper.UnexpectedTimeout);
            return true;
        });
        module.InTheMiddleOfMerge().Returns(true);
        SetModule(module);

        _control.RefreshGitAction(checkForConflicts: true);
        await checkStarted.Task;
        module.IsValidGitWorkingDir().Returns(false);
        _control.RefreshGitAction(checkForConflicts: true);

        releaseCheck.Set();
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        _accessor.Visible.Should().BeFalse();
        _accessor.Controls.Count.Should().Be(0);
    }

    [Test]
    public async Task RefreshGitAction_should_hide_the_banner_of_the_previous_repo_until_the_new_repo_is_checked()
    {
        IGitModule previousModule = CreateModule();
        previousModule.InTheMiddleOfMerge().Returns(true);
        SetModule(previousModule);
        _control.RefreshGitAction(checkForConflicts: false);
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);
        _accessor.Controls.Contains(_accessor.AbortButton).Should().BeTrue();

        TaskCompletionSource checkStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseCheck = new();
        IGitModule module = CreateModule();
        module.InTheMiddleOfRebase().Returns(_ =>
        {
            checkStarted.SetResult();
            releaseCheck.Wait(AsyncTestHelper.UnexpectedTimeout);
            return true;
        });
        SetModule(module);

        _control.RefreshGitAction(checkForConflicts: false);
        await checkStarted.Task;

        // The buttons act on the repo which is open now
        _accessor.Visible.Should().BeFalse();
        _accessor.Controls.Count.Should().Be(0);

        releaseCheck.Set();
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        _accessor.Action.Should().Be(InteractiveGitActionControl.GitAction.Rebase);
        _accessor.Visible.Should().BeTrue();
    }

    [Test]
    public async Task RefreshGitAction_should_keep_the_banner_of_the_same_repo_until_it_is_checked_again()
    {
        IGitModule module = CreateModule();
        module.InTheMiddleOfMerge().Returns(true);
        SetModule(module);
        _control.RefreshGitAction(checkForConflicts: false);
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        TaskCompletionSource checkStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseCheck = new();
        module.InTheMiddleOfRebase().Returns(_ =>
        {
            checkStarted.SetResult();
            releaseCheck.Wait(AsyncTestHelper.UnexpectedTimeout);
            return false;
        });

        _control.RefreshGitAction(checkForConflicts: false);
        await checkStarted.Task;

        _accessor.Visible.Should().BeTrue();
        _accessor.Controls.Contains(_accessor.AbortButton).Should().BeTrue();

        releaseCheck.Set();
        await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

        _accessor.Action.Should().Be(InteractiveGitActionControl.GitAction.Merge);
    }

    [TestCase(InteractiveGitActionControl.GitAction.Rebase, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Merge, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Bisect, false)]
    public void The_buttons_must_be_tall_enough_for_their_caption(InteractiveGitActionControl.GitAction action, bool conflicts)
    {
        _accessor.SetGitAction(action, conflicts);
        _control.PerformLayout();

        foreach (Button button in _accessor.Controls.OfType<Button>())
        {
            button.Height.Should().BeGreaterThanOrEqualTo(button.GetPreferredSize(Size.Empty).Height,
                $"the caption of '{button.Text}' would be cut off otherwise");
        }
    }

    [TestCase(InteractiveGitActionControl.GitAction.Rebase, false)]
    [TestCase(InteractiveGitActionControl.GitAction.Rebase, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Merge, false)]
    [TestCase(InteractiveGitActionControl.GitAction.Merge, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Patch, false)]
    [TestCase(InteractiveGitActionControl.GitAction.Patch, true)]
    [TestCase(InteractiveGitActionControl.GitAction.Bisect, false)]
    [TestCase(InteractiveGitActionControl.GitAction.None, false)]
    [TestCase(InteractiveGitActionControl.GitAction.None, true)]
    public void SetState(InteractiveGitActionControl.GitAction action, bool conflicts)
    {
        _accessor.SetGitAction(action, conflicts);

        _accessor.Action.Should().Be(action);
        _accessor.HasConflicts.Should().Be(conflicts);

        switch (action)
        {
            case InteractiveGitActionControl.GitAction.None:
                _accessor.Visible.Should().Be(conflicts);
                _accessor.Controls.Contains(_accessor.ResolveButton).Should().Be(conflicts);
                _accessor.Controls.Count.Should().Be(conflicts ? 1 : 0);
                break;
            case InteractiveGitActionControl.GitAction.Bisect:
                _accessor.Visible.Should().BeTrue();
                _accessor.Controls.Contains(_accessor.MoreButton).Should().BeTrue();
                break;
            case InteractiveGitActionControl.GitAction.Rebase:
            case InteractiveGitActionControl.GitAction.Patch:
                _accessor.Visible.Should().BeTrue();
                _accessor.Controls.Contains(_accessor.ResolveButton).Should().Be(conflicts);
                _accessor.Controls.Contains(_accessor.ContinueButton).Should().Be(!conflicts);
                _accessor.Controls.Contains(_accessor.AbortButton).Should().BeTrue();
                _accessor.Controls.Contains(_accessor.MoreButton).Should().BeTrue();
                break;
            case InteractiveGitActionControl.GitAction.Merge:
                _accessor.Visible.Should().BeTrue();
                _accessor.Controls.Contains(_accessor.ResolveButton).Should().Be(conflicts);
                _accessor.Controls.Contains(_accessor.ContinueButton).Should().Be(!conflicts);
                _accessor.Controls.Contains(_accessor.AbortButton).Should().BeTrue();
                break;
        }
    }

    private static IGitModule CreateModule()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(true);
        return module;
    }

    private void SetModule(IGitModule module)
        => _control.UICommandsSource.UICommands.Module.Returns(module);
}

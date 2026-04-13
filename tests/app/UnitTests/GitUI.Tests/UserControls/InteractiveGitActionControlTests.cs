using GitUI.UserControls;

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
    }

    [TearDown]
    public void TearDown()
    {
        _control.Dispose();
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
}

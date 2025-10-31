using GitUI.UserControls;

namespace GitUITests.UserControls;

[Apartment(ApartmentState.STA)]
public class InteractiveGitActionControlTests
{
    private InteractiveGitActionControl.TestAccessor _accessor;

    [SetUp]
    public void SetUp()
    {
        _accessor = new InteractiveGitActionControl().GetTestAccessor();
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

        ClassicAssert.AreEqual(action, _accessor.Action);
        ClassicAssert.AreEqual(conflicts, _accessor.HasConflicts);

        switch (action)
        {
            case InteractiveGitActionControl.GitAction.None:
                ClassicAssert.AreEqual(conflicts, _accessor.Visible);
                ClassicAssert.AreEqual(conflicts, _accessor.Controls.Contains(_accessor.ResolveButton));
                ClassicAssert.AreEqual(!conflicts, _accessor.Controls.Count == 0);
                break;
            case InteractiveGitActionControl.GitAction.Bisect:
                ClassicAssert.That(_accessor.Visible);
                ClassicAssert.That(_accessor.Controls.Contains(_accessor.MoreButton));
                break;
            case InteractiveGitActionControl.GitAction.Rebase:
            case InteractiveGitActionControl.GitAction.Patch:
                ClassicAssert.That(_accessor.Visible);
                ClassicAssert.AreEqual(conflicts, _accessor.Controls.Contains(_accessor.ResolveButton));
                ClassicAssert.AreEqual(!conflicts, _accessor.Controls.Contains(_accessor.ContinueButton));
                ClassicAssert.That(_accessor.Controls.Contains(_accessor.AbortButton));
                ClassicAssert.That(_accessor.Controls.Contains(_accessor.MoreButton));
                break;
            case InteractiveGitActionControl.GitAction.Merge:
                ClassicAssert.That(_accessor.Visible);
                ClassicAssert.AreEqual(conflicts, _accessor.Controls.Contains(_accessor.ResolveButton));
                ClassicAssert.AreEqual(!conflicts, _accessor.Controls.Contains(_accessor.ContinueButton));
                ClassicAssert.That(_accessor.Controls.Contains(_accessor.AbortButton));
                break;
        }
    }
}

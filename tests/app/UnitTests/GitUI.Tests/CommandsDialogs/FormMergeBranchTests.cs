using GitUI.CommandsDialogs;

namespace GitUITests.CommandsDialogs;
public sealed class FormMergeBranchTests
{
    [Test]
    public void ShouldOpenCommitDialogAfterSquash_should_be_true_when_squash_merge_succeeded_and_commit_wanted()
    {
        FormMergeBranch.ShouldOpenCommitDialogAfterSquash(mergeSucceeded: true, squash: true, doNotCommit: false)
            .Should().BeTrue();
    }

    [Test]
    public void ShouldOpenCommitDialogAfterSquash_should_be_false_when_do_not_commit_is_checked()
    {
        FormMergeBranch.ShouldOpenCommitDialogAfterSquash(mergeSucceeded: true, squash: true, doNotCommit: true)
            .Should().BeFalse();
    }

    [Test]
    public void ShouldOpenCommitDialogAfterSquash_should_be_false_for_regular_merges()
    {
        // A regular merge commits by itself when "Do not commit" is unchecked,
        // so no commit dialog must be offered.
        FormMergeBranch.ShouldOpenCommitDialogAfterSquash(mergeSucceeded: true, squash: false, doNotCommit: false)
            .Should().BeFalse();
    }

    [Test]
    public void ShouldOpenCommitDialogAfterSquash_should_be_false_when_the_merge_failed()
    {
        // A failed (conflicted) squash merge is handled by the conflict
        // resolution dialog, which offers the commit itself.
        FormMergeBranch.ShouldOpenCommitDialogAfterSquash(mergeSucceeded: false, squash: true, doNotCommit: false)
            .Should().BeFalse();
    }
}

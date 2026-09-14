using GitExtensions.Extensibility.Git;
using GitUI;
using NSubstitute;

namespace GitUITests;

public sealed class GitUICommandsWorktreeTests
{
    private const string MainWorktreePath = @"C:\repos\main";
    private const string LinkedWorktreePath = @"C:\repos\linked";

    [TestCase(MainWorktreePath, LinkedWorktreePath, false)]
    [TestCase(LinkedWorktreePath, LinkedWorktreePath, false)]
    [TestCase(LinkedWorktreePath, MainWorktreePath, true)]
    [TestCase(@"C:\repos\unregistered", MainWorktreePath, false)]
    public void WorktreeDelete_should_reject_main_current_deleted_and_unregistered_worktrees(
        string targetPath, string currentPath, bool linkedIsDeleted)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(currentPath);
        module.GetWorktrees().Returns(
        [
            new GitWorktree(MainWorktreePath, GitWorktreeHeadType.Branch, null, "main", IsDeleted: false),
            new GitWorktree(LinkedWorktreePath, GitWorktreeHeadType.Branch, null, "linked", linkedIsDeleted),
        ]);
        GitUICommands commands = new(GitUICommands.EmptyServiceProvider, module);

        commands.WorktreeDelete(owner: null, targetPath).Should().BeFalse();
        module.DidNotReceive().IsValidGitWorkingDir();
    }

    [Test]
    public void WorktreeDelete_should_reject_when_no_worktrees_are_reported()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(LinkedWorktreePath);
        module.GetWorktrees().Returns(Array.Empty<GitWorktree>());
        GitUICommands commands = new(GitUICommands.EmptyServiceProvider, module);

        commands.WorktreeDelete(owner: null, MainWorktreePath).Should().BeFalse();
        module.DidNotReceive().IsValidGitWorkingDir();
    }
}

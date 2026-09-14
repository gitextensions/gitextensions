using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests_Git;

partial class CommandsTests
{
    [TestCase(@"C:\repos\linked")]
    [TestCase(@"C:\my repos\linked worktree")]
    [TestCase("/home/user/linked worktree")]
    [TestCase("--force")]
    public void RemoveWorktree_should_quote_path_and_preserve_git_safety_checks(string path)
    {
        IGitCommand command = Commands.RemoveWorktree(path);

        command.Arguments.Should().Be($"worktree remove --force -- \"{path}\"");
        command.AccessesRemote.Should().BeFalse();
        command.ChangesRepoState.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void RemoveWorktree_should_reject_empty_path(string? path)
    {
        ((Action)(() => Commands.RemoveWorktree(path!))).Should().Throw<ArgumentException>();
    }
}

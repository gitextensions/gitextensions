using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests_Git;

partial class CommandsTests
{
    [TestCase(@"C:\repos\feature", null, "worktree remove --force \"C:/repos/feature\"")]
    [TestCase(@"C:\my repos\work tree", null, "worktree remove --force \"C:/my repos/work tree\"")]
    [TestCase(@"\\wsl$\Ubuntu\home\me\wt", "Ubuntu", "worktree remove --force \"/home/me/wt\"")]
    public void TryRemoveWorktree_should_let_git_remove_the_worktree(string worktreePath, string? wslDistro, string expectedArguments)
    {
        // git deletes the directory itself, which is what makes symlinks in a \\wsl$ worktree work
        // with WSL git, and it also removes the administrative files, so no prune is run.
        MockExecutable executable = new();
        IGitModule module = CreateModule(executable, wslDistro);

        using (executable.StageOutput(expectedArguments, ""))
        {
            bool removed = module.TryRemoveWorktree(worktreePath, out string? errorMessage);

            removed.Should().BeTrue();
            errorMessage.Should().BeNull();
        }

        executable.Verify();
    }

    [Test]
    public void TryRemoveWorktree_should_delete_the_directory_and_prune_if_git_refuses()
    {
        MockExecutable executable = new();
        IGitModule module = CreateModule(executable);
        string worktreePath = CreateTemporaryWorktreeDirectory();

        try
        {
            using (executable.StageOutput($"worktree remove --force \"{worktreePath.ToPosixPath()}\"", "", exitCode: 128, error: "fatal: cannot remove a locked working tree"))
            using (executable.StageOutput("worktree prune", ""))
            {
                bool removed = module.TryRemoveWorktree(worktreePath, out string? errorMessage);

                removed.Should().BeTrue();
                errorMessage.Should().BeNull();
                Directory.Exists(worktreePath).Should().BeFalse();
            }

            executable.Verify();
        }
        finally
        {
            DeleteTemporaryWorktreeDirectory(worktreePath);
        }
    }

    [Test]
    public void TryRemoveWorktree_should_report_the_error_if_prune_fails()
    {
        MockExecutable executable = new();
        IGitModule module = CreateModule(executable);
        string worktreePath = CreateTemporaryWorktreeDirectory();

        try
        {
            using (executable.StageOutput($"worktree remove --force \"{worktreePath.ToPosixPath()}\"", "", exitCode: 128, error: "fatal: cannot remove a locked working tree"))
            using (executable.StageOutput("worktree prune", "", exitCode: 128, error: "fatal: unable to prune\n"))
            {
                bool removed = module.TryRemoveWorktree(worktreePath, out string? errorMessage);

                removed.Should().BeFalse();
                errorMessage.Should().Be("fatal: unable to prune");
                Directory.Exists(worktreePath).Should().BeFalse();
            }

            executable.Verify();
        }
        finally
        {
            DeleteTemporaryWorktreeDirectory(worktreePath);
        }
    }

    [Test]
    public void TryRemoveWorktree_should_report_both_errors_if_the_directory_cannot_be_deleted_either()
    {
        MockExecutable executable = new();
        IGitModule module = CreateModule(executable);
        string worktreePath = CreateTemporaryWorktreeDirectory();

        try
        {
            using (new FileStream(Path.Combine(worktreePath, "locked.txt"), FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (executable.StageOutput($"worktree remove --force \"{worktreePath.ToPosixPath()}\"", "", exitCode: 128, error: "fatal: failed to delete"))
            {
                bool removed = module.TryRemoveWorktree(worktreePath, out string? errorMessage);

                removed.Should().BeFalse();
                errorMessage.Should().Contain("fatal: failed to delete");
                errorMessage.Should().Contain("locked.txt");
            }

            executable.Verify();
        }
        finally
        {
            DeleteTemporaryWorktreeDirectory(worktreePath);
        }
    }

    private static IGitModule CreateModule(MockExecutable executable, string? wslDistro = null)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GitExecutable.Returns(executable);
        module.GetPathForGitExecution(Arg.Any<string?>()).Returns(callInfo => PathUtil.GetPathForGitExecution(callInfo.Arg<string?>(), wslDistro));
        return module;
    }

    private static string CreateTemporaryWorktreeDirectory()
    {
        string worktreePath = Path.Combine(Path.GetTempPath(), $"ge-worktree-{Guid.NewGuid():N}");
        Directory.CreateDirectory(worktreePath);
        File.WriteAllText(Path.Combine(worktreePath, ".git"), "gitdir: elsewhere");
        return worktreePath;
    }

    private static void DeleteTemporaryWorktreeDirectory(string worktreePath)
    {
        if (Directory.Exists(worktreePath))
        {
            Directory.Delete(worktreePath, recursive: true);
        }
    }
}

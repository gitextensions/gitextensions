using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Git;
public sealed class GitModuleWorktreeTests
{
    private GitModule _gitModule = null!;
    private MockExecutable _executable = null!;

    [SetUp]
    public void SetUp()
    {
        // Clear the static GitVersion cache so the mock executable is always queried for --version.
        GitVersion.ResetVersion();
        _executable = new MockExecutable();
        _executable.StageOutput("--version", $"git version {GitVersion.LastRecommendedVersion}");
        _gitModule = GetGitModuleWithExecutable(_executable);
    }

    [TearDown]
    public void TearDown()
    {
        _executable.Verify();

        // Do not leave a mocked version cached for other fixtures.
        GitVersion.ResetVersion();
    }

    [Test]
    public void GetWorktrees_should_parse_single_worktree_with_branch()
    {
        string output = string.Join('\0',
            "worktree C:/repos/main",
            "HEAD abc1234abc1234abc1234abc1234abc1234abc12",
            "branch refs/heads/master",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Should().HaveCount(1);
            worktrees[0].Path.Should().Be("C:\\repos\\main");
            worktrees[0].HeadType.Should().Be(GitWorktreeHeadType.Branch);
            worktrees[0].Sha1.Should().Be("abc1234abc1234abc1234abc1234abc1234abc12");
            worktrees[0].Branch.Should().Be("master");
            worktrees[0].IsDeleted.Should().BeTrue();
        }
    }

    [Test]
    public void GetWorktrees_should_parse_detached_head()
    {
        string output = string.Join('\0',
            "worktree C:/repos/detached",
            "HEAD def5678def5678def5678def5678def5678def56",
            "detached",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Should().HaveCount(1);
            worktrees[0].HeadType.Should().Be(GitWorktreeHeadType.Detached);
            worktrees[0].Sha1.Should().Be("def5678def5678def5678def5678def5678def56");
            worktrees[0].Branch.Should().BeNull();
        }
    }

    [Test]
    public void GetWorktrees_should_parse_bare_repo()
    {
        string output = string.Join('\0',
            "worktree C:/repos/bare",
            "bare",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Should().HaveCount(1);
            worktrees[0].HeadType.Should().Be(GitWorktreeHeadType.Bare);
            worktrees[0].Sha1.Should().BeNull();
            worktrees[0].Branch.Should().BeNull();
        }
    }

    [Test]
    public void GetWorktrees_should_parse_multiple_worktrees()
    {
        string output = string.Join('\0',
            "worktree C:/repos/main",
            "HEAD aaaa1234aaaa1234aaaa1234aaaa1234aaaa1234",
            "branch refs/heads/master",
            "", // end of first record
            "worktree C:/repos/feature",
            "HEAD bbbb5678bbbb5678bbbb5678bbbb5678bbbb5678",
            "branch refs/heads/feature/my-feature",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Should().HaveCount(2);

            worktrees[0].Path.Should().Be("C:\\repos\\main");
            worktrees[0].Branch.Should().Be("master");

            worktrees[1].Path.Should().Be("C:\\repos\\feature");
            worktrees[1].Branch.Should().Be("feature/my-feature");
        }
    }

    [Test]
    public void GetWorktrees_should_mark_only_the_first_worktree_as_the_main_one()
    {
        // git always reports the main worktree first, and refuses to remove it. Deleting its
        // directory would take the repository with it, so it has to be told apart from the rest.
        string output = string.Join('\0',
            "worktree C:/repos/main",
            "HEAD aaaa1234aaaa1234aaaa1234aaaa1234aaaa1234",
            "branch refs/heads/master",
            "",
            "worktree C:/repos/feature",
            "HEAD bbbb5678bbbb5678bbbb5678bbbb5678bbbb5678",
            "branch refs/heads/feature/my-feature",
            "",
            "worktree C:/repos/hotfix",
            "HEAD cccc9012cccc9012cccc9012cccc9012cccc9012",
            "branch refs/heads/hotfix",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Select(worktree => worktree.IsMain).Should().Equal(true, false, false);
        }
    }

    [Test]
    public void GetWorktrees_should_handle_path_with_spaces()
    {
        string output = string.Join('\0',
            "worktree C:/my repos/work tree",
            "HEAD abc1234abc1234abc1234abc1234abc1234abc12",
            "branch refs/heads/main",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Should().HaveCount(1);
            worktrees[0].Path.Should().Be("C:\\my repos\\work tree");
            worktrees[0].Branch.Should().Be("main");
        }
    }

    [Test]
    public void GetWorktrees_should_strip_refs_heads_prefix_from_branch()
    {
        string output = string.Join('\0',
            "worktree C:/repos/main",
            "HEAD abc1234abc1234abc1234abc1234abc1234abc12",
            "branch refs/heads/feature/nested/branch",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees[0].Branch.Should().Be("feature/nested/branch");
        }
    }

    [Test]
    public void GetWorktrees_should_parse_locked_worktree_with_reason()
    {
        string output = string.Join('\0',
            "worktree C:/repos/main",
            "HEAD aaaa1234aaaa1234aaaa1234aaaa1234aaaa1234",
            "branch refs/heads/master",
            "",
            "worktree C:/repos/usb",
            "HEAD bbbb5678bbbb5678bbbb5678bbbb5678bbbb5678",
            "branch refs/heads/usb",
            "locked on a removable drive",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees[0].IsLocked.Should().BeFalse();
            worktrees[0].LockReason.Should().BeNull();

            worktrees[1].IsLocked.Should().BeTrue();
            worktrees[1].LockReason.Should().Be("on a removable drive");
            worktrees[1].IsPrunable.Should().BeFalse();
        }
    }

    [Test]
    public void GetWorktrees_should_parse_locked_worktree_without_reason()
    {
        string output = string.Join('\0',
            "worktree C:/repos/locked",
            "HEAD abc1234abc1234abc1234abc1234abc1234abc12",
            "detached",
            "locked",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees[0].IsLocked.Should().BeTrue();
            worktrees[0].LockReason.Should().BeNull();
            worktrees[0].HeadType.Should().Be(GitWorktreeHeadType.Detached);
        }
    }

    [Test]
    public void GetWorktrees_should_parse_prunable_worktree()
    {
        string output = string.Join('\0',
            "worktree C:/repos/main",
            "HEAD aaaa1234aaaa1234aaaa1234aaaa1234aaaa1234",
            "branch refs/heads/master",
            "",
            "worktree C:/repos/gone",
            "HEAD bbbb5678bbbb5678bbbb5678bbbb5678bbbb5678",
            "branch refs/heads/gone",
            "prunable gitdir file points to non-existent location",
            "",
            "worktree C:/repos/next",
            "HEAD cccc9012cccc9012cccc9012cccc9012cccc9012",
            "branch refs/heads/next",
            "", "");

        using (_executable.StageOutput("worktree list --porcelain -z", output))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Select(worktree => worktree.IsPrunable).Should().Equal(false, true, false);
            worktrees[1].PrunableReason.Should().Be("gitdir file points to non-existent location");
            worktrees[1].IsLocked.Should().BeFalse();

            // The state of one record must not leak into the next one.
            worktrees[2].PrunableReason.Should().BeNull();
        }
    }

    [TestCase('\0')] // git worktree list --porcelain -z (git 2.36+)
    [TestCase('\n')] // git worktree list --porcelain
    public void ParseWorktrees_should_parse_locked_and_prunable_with_either_separator(char separator)
    {
        // Parsing runs no git command, so the --version staged in SetUp is not needed.
        _executable = new MockExecutable();
        _gitModule = GetGitModuleWithExecutable(_executable);

        string output = string.Join(separator,
            "worktree C:/repos/main",
            "HEAD aaaa1234aaaa1234aaaa1234aaaa1234aaaa1234",
            "branch refs/heads/master",
            "",
            "worktree C:/repos/usb",
            "HEAD bbbb5678bbbb5678bbbb5678bbbb5678bbbb5678",
            "branch refs/heads/usb",
            "locked on a removable drive",
            "",
            "worktree C:/repos/gone",
            "HEAD cccc9012cccc9012cccc9012cccc9012cccc9012",
            "detached",
            "prunable gitdir file points to non-existent location",
            "", "");

        List<GitWorktree> worktrees = _gitModule.GetTestAccessor().ParseWorktrees(output, separator);

        worktrees.Select(worktree => worktree.Path).Should().Equal("C:\\repos\\main", "C:\\repos\\usb", "C:\\repos\\gone");
        worktrees.Select(worktree => worktree.IsLocked).Should().Equal(false, true, false);
        worktrees[1].LockReason.Should().Be("on a removable drive");
        worktrees.Select(worktree => worktree.IsPrunable).Should().Equal(false, false, true);
        worktrees[2].PrunableReason.Should().Be("gitdir file points to non-existent location");
        worktrees[2].HeadType.Should().Be(GitWorktreeHeadType.Detached);
    }

    [Test]
    public void GetWorktrees_should_return_empty_list_for_empty_output()
    {
        using (_executable.StageOutput("worktree list --porcelain -z", ""))
        {
            IReadOnlyList<GitWorktree> worktrees = _gitModule.GetWorktrees();

            worktrees.Should().BeEmpty();
        }
    }

    private static GitModule GetGitModuleWithExecutable(IExecutable executable)
    {
        GitModule module = new(new GitExecutorProvider(new GitDirectoryResolver()), "");

        GitExecutor.TestAccessor executorAccessor = module.GetTestAccessor().Executor;
        executorAccessor.GitExecutable = executable;
        executorAccessor.GitWindowsExecutable = executable;
        executorAccessor.GitCommandRunner = new GitCommandRunner(executable, () => GitModule.SystemEncoding);

        return module;
    }
}

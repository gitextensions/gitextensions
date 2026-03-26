using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Git;

[TestFixture]
public sealed class GitModuleWorktreeTests
{
    private GitModule _gitModule;
    private MockExecutable _executable;

    [SetUp]
    public void SetUp()
    {
        _executable = new MockExecutable();
        _gitModule = GetGitModuleWithExecutable(_executable);
    }

    [TearDown]
    public void TearDown()
    {
        _executable.Verify();
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

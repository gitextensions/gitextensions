using CommonTestUtils;
using FluentAssertions;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests.Git;

[TestFixture]
partial class CommandsTests
{
    private string _tempDir = null!;
    private string _gitDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _gitDir = Path.Combine(_tempDir, ".git");
        Directory.CreateDirectory(_gitDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Test]
    public void GetSelectedBranch_should_return_branch_name_from_HEAD_file()
    {
        IGitExecutor gitExecutor = Substitute.For<IGitExecutor>();
        gitExecutor.IsReftableRepo.Returns(false);
        gitExecutor.WorkingDir.Returns(_tempDir);
        gitExecutor.GetGitDirectory().Returns(_gitDir);

        File.WriteAllText(Path.Combine(_gitDir, "HEAD"), "ref: refs/heads/main");

        string branch = GitCommands.Git.Commands.GetSelectedBranch(gitExecutor);
        branch.Should().Be("main");
    }

    [Test]
    public void GetSelectedBranch_should_return_detached_branch_when_HEAD_is_detached()
    {
        IGitExecutor gitExecutor = Substitute.For<IGitExecutor>();
        gitExecutor.IsReftableRepo.Returns(false);
        gitExecutor.WorkingDir.Returns(_tempDir);
        gitExecutor.GetGitDirectory().Returns(_gitDir);

        File.WriteAllText(Path.Combine(_gitDir, "HEAD"), "9601551c564b48208bccd50b705264e9bd68140d");

        string branch = GitCommands.Git.Commands.GetSelectedBranch(gitExecutor);
        branch.Should().Be("(no branch)");
    }

    [Test]
    public void GetSelectedBranch_should_return_empty_if_HEAD_file_missing()
    {
        MockExecutable executable = new();
        IGitExecutor gitExecutor = Substitute.For<IGitExecutor>();
        gitExecutor.IsReftableRepo.Returns(false);
        gitExecutor.WorkingDir.Returns(_tempDir);
        gitExecutor.GetGitDirectory().Returns(_gitDir);
        gitExecutor.GitExecutable.Returns(executable);

        using (executable.StageOutput("symbolic-ref --quiet HEAD", string.Empty, exitCode: 1))
        {
            string branch = GitCommands.Git.Commands.GetSelectedBranch(gitExecutor, emptyIfDetached: true);
            branch.Should().BeEmpty();
        }
    }

    [Test]
    public void GetSelectedBranch_should_fallback_to_git_command_when_reftable_repo()
    {
        MockExecutable executable = new();
        IGitExecutor gitExecutor = Substitute.For<IGitExecutor>();
        gitExecutor.IsReftableRepo.Returns(true);
        gitExecutor.WorkingDir.Returns(_tempDir);
        gitExecutor.GitExecutable.Returns(executable);

        using (executable.StageOutput("symbolic-ref --quiet HEAD", "refs/heads/feature\n"))
        {
            string branch = GitCommands.Git.Commands.GetSelectedBranch(gitExecutor);
            branch.Should().Be("feature");
        }
    }

    [Test]
    public void GetSelectedBranch_should_return_detached_branch_when_command_fails_and_emptyIfDetached_false()
    {
        MockExecutable executable = new();
        IGitExecutor gitExecutor = Substitute.For<IGitExecutor>();
        gitExecutor.IsReftableRepo.Returns(true);
        gitExecutor.WorkingDir.Returns(_tempDir);
        gitExecutor.GitExecutable.Returns(executable);

        using (executable.StageOutput("symbolic-ref --quiet HEAD", string.Empty, exitCode: 1))
        {
            string branch = GitCommands.Git.Commands.GetSelectedBranch(gitExecutor, emptyIfDetached: false);
            branch.Should().Be("(no branch)");
        }
    }

    [Test]
    public void GetSelectedBranch_should_return_empty_when_command_fails_and_emptyIfDetached_true()
    {
        MockExecutable executable = new();
        IGitExecutor gitExecutor = Substitute.For<IGitExecutor>();
        gitExecutor.IsReftableRepo.Returns(true);
        gitExecutor.WorkingDir.Returns(_tempDir);
        gitExecutor.GitExecutable.Returns(executable);

        using (executable.StageOutput("symbolic-ref --quiet HEAD", string.Empty, exitCode: 1))
        {
            string branch = GitCommands.Git.Commands.GetSelectedBranch(gitExecutor, emptyIfDetached: true);
            branch.Should().BeEmpty();
        }
    }
}

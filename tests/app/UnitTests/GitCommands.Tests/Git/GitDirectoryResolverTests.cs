using System.IO.Abstractions;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using NSubstitute;

namespace GitCommandsTests.Git;
public class GitDirectoryResolverTests
{
    private readonly string _workingDir = @"c:\dev\repo";
    private string _gitWorkingDir = null!;
    private string _gitFile = null!;
    private FileBase _file = null!;
    private DirectoryBase _directory = null!;
    private IFileSystem _fileSystem = null!;
    private GitDirectoryResolver _resolver = null!;

    [SetUp]
    public void Setup()
    {
        _gitFile = Path.Combine(_workingDir, ".git");
        _gitWorkingDir = _gitFile.EnsureTrailingPathSeparator();

        _file = Substitute.For<FileBase>();
        _directory = Substitute.For<DirectoryBase>();
        _fileSystem = Substitute.For<IFileSystem>();
        _fileSystem.Directory.Returns(_directory);
        _fileSystem.File.Returns(_file);

        _directory.Exists(_workingDir).Returns(true);

        _resolver = new GitDirectoryResolver(_fileSystem);
    }

    [Test]
    public void Resolve_should_throw_if_path_is_null()
    {
        ((Action)(() => _resolver.Resolve(null!))).Should().Throw<ArgumentNullException>();
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void Resolve_should_return_empty_string_if_working_dir_is_empty(string workingDir)
    {
        _resolver.Resolve(workingDir).Should().BeEmpty();
    }

    [Test]
    public void Resolve_should_return_original_path_if_git_file_or_folder_absent()
    {
        _directory.Exists(_gitWorkingDir).Returns(false);
        _file.Exists(Arg.Any<string>()).Returns(false);

        _resolver.Resolve(_workingDir).Should().Be(_workingDir);
    }

    [Test]
    public void Resolve_should_return_path_to_git_folder_if_present()
    {
        _directory.Exists(_gitWorkingDir).Returns(true);
        _file.Exists(Arg.Any<string>()).Returns(true);

        _resolver.Resolve(_workingDir).Should().Be(_gitWorkingDir);
    }

    [Platform(Include = "Win")]
    [Test]
    public void Resolve_should_return_path_from_git_file_if_present()
    {
        _file.Exists(_gitFile).Returns(true);
        _file.ReadLines(_gitFile).Returns(new[] { "", " ", @"gitdir: c:/dev/repo/.git/modules/Externals/Git.hub", "text" });

        _resolver.Resolve(_workingDir).Should().Be(@"c:\dev\repo\.git\modules\Externals\Git.hub\");

        _directory.DidNotReceive().Exists(_gitWorkingDir);
    }

    [Test]
    public void Resolve_should_return_resolved_full_path_from_git_file_if_present()
    {
        _file.Exists(_gitFile).Returns(true);
        _file.ReadLines(_gitFile).Returns(new[] { "", " ", @"gitdir: ../.git/modules/Externals/Git.hub", "text" });

        _resolver.Resolve(_workingDir).Should().Be(@"c:\dev\.git\modules\Externals\Git.hub\");

        _directory.DidNotReceive().Exists(_gitWorkingDir);
    }

    [Platform(Include = "Win")]
    [Test]
    public void Resolve_should_reconstruct_wsl_unc_path_for_posix_gitdir_in_linked_worktree()
    {
        // Regression test for #13272: committing from a linked worktree in a WSL repo.
        // The working tree is accessed via a \\wsl$ UNC path, but the .git file's "gitdir:"
        // line points at a POSIX absolute path (as written by the Linux git binary) that
        // lives under the main repo's .git, on a different top-level directory.
        // The resolver must convert that POSIX path back into the \\wsl$ UNC path so that
        // COMMITMESSAGE is written to the same location git reads it from.
        string workingDir = @"\\wsl$\Ubuntu\home\user\source\MyRepo.wt\ACTIVE";
        string gitFile = Path.Combine(workingDir, ".git");

        _file.Exists(gitFile).Returns(true);
        _file.ReadLines(gitFile).Returns(new[] { "gitdir: /home/user/source/MyRepo/.git/worktrees/ACTIVE" });

        _resolver.Resolve(workingDir)
            .Should().Be(@"\\wsl$\Ubuntu\home\user\source\MyRepo\.git\worktrees\ACTIVE\");

        _directory.DidNotReceive().Exists(gitFile.EnsureTrailingPathSeparator());
    }

    [Platform(Include = "Win")]
    [Test]
    public void Resolve_should_map_wsl_mnt_path_to_windows_drive()
    {
        // A /mnt/<drive> path (WSL view of a Windows drive) must map back to the drive letter.
        string workingDir = @"\\wsl$\Ubuntu\mnt\c\dev\repo";
        string gitFile = Path.Combine(workingDir, ".git");

        _file.Exists(gitFile).Returns(true);
        _file.ReadLines(gitFile).Returns(new[] { "gitdir: /mnt/c/dev/repo/.git/worktrees/ACTIVE" });

        _resolver.Resolve(workingDir)
            .Should().Be(@"C:\dev\repo\.git\worktrees\ACTIVE\");
    }

    [Platform(Include = "Win")]
    [Test]
    public void Resolve_should_return_unc_gitdir_path_unchanged()
    {
        // A fully qualified UNC gitdir path is already usable and must be returned as-is.
        _file.Exists(_gitFile).Returns(true);
        _file.ReadLines(_gitFile).Returns(new[] { @"gitdir: \\wsl$\Ubuntu\home\user\source\MyRepo\.git\worktrees\ACTIVE" });

        _resolver.Resolve(_workingDir)
            .Should().Be(@"\\wsl$\Ubuntu\home\user\source\MyRepo\.git\worktrees\ACTIVE\");
    }

    [Test]
    public void Resolve_non_bare_repository_real_filesystem()
    {
        _resolver = new GitDirectoryResolver();
        using GitModuleTestHelper helper = new();
        _resolver.Resolve(helper.Module.WorkingDir).Should().Be(helper.Module.WorkingDirGitDir);
    }

    [Test]
    public void Resolve_submodule_real_filesystem()
    {
        using GitModuleTestHelper helper = new();
        string submodulePath = Path.Combine(helper.Module.WorkingDir, "External", "Git.hub");
        helper.CreateFile(submodulePath, ".git", "\r \r\ngitdir: ../../.git/modules/Externals/Git.hub\r\ntext");
        _resolver = new GitDirectoryResolver();

        _resolver.Resolve(submodulePath).Should().Be($@"{helper.Module.WorkingDirGitDir}modules\Externals\Git.hub\");
        _resolver.Resolve(helper.Module.WorkingDir).Should().Be(helper.Module.WorkingDirGitDir);
    }
}

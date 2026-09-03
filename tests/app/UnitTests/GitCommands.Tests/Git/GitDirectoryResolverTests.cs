using System.IO.Abstractions;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using NSubstitute;

namespace GitCommandsTests.Git;
public class GitDirectoryResolverTests
{
    // The resolver combines and roots paths natively, so the working directory must be a
    // native one; the .git file always spells its gitdir with '/'.
    private readonly string _workingDir = OperatingSystem.IsWindows() ? @"c:\dev\repo" : "/dev/repo";
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

    [Test]
    public void Resolve_should_return_path_from_git_file_if_present()
    {
        string gitDir = OperatingSystem.IsWindows() ? "c:/dev/repo/.git/modules/Externals/Git.hub" : "/dev/repo/.git/modules/Externals/Git.hub";
        string expected = OperatingSystem.IsWindows() ? @"c:\dev\repo\.git\modules\Externals\Git.hub\" : "/dev/repo/.git/modules/Externals/Git.hub/";

        _file.Exists(_gitFile).Returns(true);
        _file.ReadLines(_gitFile).Returns(new[] { "", " ", $"gitdir: {gitDir}", "text" });

        _resolver.Resolve(_workingDir).Should().Be(expected);

        _directory.DidNotReceive().Exists(_gitWorkingDir);
    }

    [Test]
    public void Resolve_should_return_resolved_full_path_from_git_file_if_present()
    {
        string expected = OperatingSystem.IsWindows() ? @"c:\dev\.git\modules\Externals\Git.hub\" : "/dev/.git/modules/Externals/Git.hub/";

        _file.Exists(_gitFile).Returns(true);
        _file.ReadLines(_gitFile).Returns(new[] { "", " ", @"gitdir: ../.git/modules/Externals/Git.hub", "text" });

        _resolver.Resolve(_workingDir).Should().Be(expected);

        _directory.DidNotReceive().Exists(_gitWorkingDir);
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

        _resolver.Resolve(submodulePath).Should().Be(Path.Combine(helper.Module.WorkingDirGitDir, "modules", "Externals", "Git.hub").EnsureTrailingPathSeparator());
        _resolver.Resolve(helper.Module.WorkingDir).Should().Be(helper.Module.WorkingDirGitDir);
    }
}

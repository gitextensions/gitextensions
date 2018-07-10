using System;
using System.IO;
using System.IO.Abstractions;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitDirectoryResolverTests
    {
        private readonly string _workingDir = @"c:\dev\repo";
        private string _gitWorkingDir;
        private string _gitFile;
        private FileBase _file;
        private DirectoryBase _directory;
        private IFileSystem _fileSystem;
        private GitDirectoryResolver _resolver;

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
            ((Action)(() => _resolver.Resolve(null))).Should().Throw<ArgumentNullException>();
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
            _file.Exists(Arg.Any<string>()).Returns(false);

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

        [Test]
        public void Resolve_non_bare_repository_real_filesystem()
        {
            _resolver = new GitDirectoryResolver();
            using (var helper = new GitModuleTestHelper())
            {
                _resolver.Resolve(helper.Module.WorkingDir).Should().Be(helper.Module.WorkingDirGitDir);
            }
        }

        [Test]
        public void Resolve_submodule_real_filesystem()
        {
            using (var helper = new GitModuleTestHelper())
            {
                var submodulePath = Path.Combine(helper.Module.WorkingDir, "External", "Git.hub");
                helper.CreateFile(submodulePath, ".git", "\r \r\ngitdir: ../../.git/modules/Externals/Git.hub\r\ntext");
                _resolver = new GitDirectoryResolver();

                _resolver.Resolve(submodulePath).Should().Be($@"{helper.Module.WorkingDirGitDir}modules\Externals\Git.hub\");
                _resolver.Resolve(helper.Module.WorkingDir).Should().Be(helper.Module.WorkingDirGitDir);
            }
        }
    }
}

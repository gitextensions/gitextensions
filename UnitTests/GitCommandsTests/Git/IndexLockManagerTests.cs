using System;
using System.IO;
using System.IO.Abstractions;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class IndexLockManagerTests
    {
        private const string IndexLock = "index.lock";
        private readonly string _workingDir = @"c:\dev\repo";
        private string _gitWorkingDir;
        private string _indexLockFile;
        private string _gitFile;
        private FileBase _file;
        private DirectoryBase _directory;
        private IFileSystem _fileSystem;
        private IGitDirectoryResolver _gitDirectoryResolver;
        private IGitModule _module;
        private IIndexLockManager _manager;

        [SetUp]
        public void Setup()
        {
            _gitFile = Path.Combine(_workingDir, ".git");
            _gitWorkingDir = _gitFile.EnsureTrailingPathSeparator();
            _indexLockFile = Path.Combine(_gitWorkingDir, IndexLock);

            _file = Substitute.For<FileBase>();
            _directory = Substitute.For<DirectoryBase>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.Directory.Returns(_directory);
            _fileSystem.File.Returns(_file);

            _module = Substitute.For<IGitModule>();
            _module.WorkingDir.Returns(_workingDir);

            _gitDirectoryResolver = Substitute.For<IGitDirectoryResolver>();
            _gitDirectoryResolver.Resolve(_workingDir).Returns(_gitWorkingDir);

            _manager = new IndexLockManager(_module, _gitDirectoryResolver, _fileSystem);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void IsIndexLocked(bool fileExists)
        {
            _file.Exists(Arg.Any<string>()).Returns(fileExists);

            _manager.IsIndexLocked().Should().Be(fileExists);
        }

        [Test]
        public void UnlockIndex_should_only_delete_working_folder_lock_if_requested()
        {
            _manager.UnlockIndex(false);

            _module.DidNotReceive().GetSubmodulesLocalPaths();
        }

        [Test]
        public void UnlockIndex_should_not_delete_lock_if_absent()
        {
            _file.Exists(_indexLockFile).Returns(false);

            _manager.UnlockIndex(false);

            _module.DidNotReceive().GetSubmodulesLocalPaths();
            _file.DidNotReceive().Delete(Arg.Any<string>());
        }

        [Test]
        public void UnlockIndex_should_only_delete_lock_if_exists()
        {
            _file.Exists(_indexLockFile).Returns(true);

            _manager.UnlockIndex(false);

            _module.DidNotReceive().GetSubmodulesLocalPaths();
            _file.Received(1).Delete(Arg.Any<string>());
            _file.Received(1).Delete(_indexLockFile);
        }

        [Test]
        public void UnlockIndex_should_throw_if_delete_fails()
        {
            _file.Exists(_indexLockFile).Returns(true);
            _file.When(x => x.Delete(_indexLockFile)).Throw(new DivideByZeroException("boom"));

            var ex = Assert.Throws<FileDeleteException>(() => _manager.UnlockIndex(false));

            ex.FileName.Should().Be(_indexLockFile);
            _module.DidNotReceive().GetSubmodulesLocalPaths();
            _file.Received(1).Delete(Arg.Any<string>());
            _file.Received(1).Delete(_indexLockFile);
        }

        [Platform(Include = "Win")]
        [Test]
        public void UnlockIndex_should_delete_submodule_locks_if_requested()
        {
            _file.Exists(_indexLockFile).Returns(true);

            var submoduleGitHubWorkingDir = $@"{_workingDir}\Externals\Git.hub\";
            var submoduleNbugWorkingDir = $@"{_workingDir}\Externals\NBug\";
            var submoduleGitHubWorkingDirGitDir = $@"{_gitWorkingDir}\modules\Externals\Git.hub\";
            var submoduleNBugWorkingDirGitDir = $@"{_gitWorkingDir}\modules\Externals\NBug\";
            var submoduleGitHubIndexLock = $@"{_gitWorkingDir}\modules\Externals\Git.hub\{IndexLock}";
            var submoduleNBugIndexLock = $@"{_gitWorkingDir}\modules\Externals\NBug\{IndexLock}";

            _module.GetSubmodulesLocalPaths().Returns(new[] { "Externals/Git.hub", "Externals/NBug" });
            _module.GetSubmoduleFullPath(Arg.Any<string>())
                .Returns(submoduleGitHubWorkingDir, submoduleNbugWorkingDir);
            _gitDirectoryResolver.Resolve(submoduleGitHubWorkingDir).Returns(submoduleGitHubWorkingDirGitDir);
            _gitDirectoryResolver.Resolve(submoduleNbugWorkingDir).Returns(submoduleNBugWorkingDirGitDir);
            _file.Exists(submoduleGitHubIndexLock).Returns(true);
            _file.Exists(submoduleNBugIndexLock).Returns(false);

            _manager.UnlockIndex(true);

            _file.Received().Delete(submoduleGitHubIndexLock);
            _file.DidNotReceive().Delete(submoduleNBugIndexLock);
        }

        [Test]
        public void Resolve_submodule_real_filesystem()
        {
            using (var helper = new GitModuleTestHelper())
            {
                helper.CreateFile(helper.Module.WorkingDir, ".gitmodules", @"[submodule ""Externals/NBug""]
    path = Externals/NBug
    url = https://github.com/gitextensions/NBug.git
[submodule ""Externals/Git.hub""]
    path = Externals/Git.hub
    url = https://github.com/gitextensions/Git.hub.gitk");

                var submoduleGitHub = Path.Combine(helper.Module.WorkingDir, "Externals", "Git.hub");
                var submoduleNBug = Path.Combine(helper.Module.WorkingDir, "Externals", "NBug");
                var submoduleGitHubWorkingDirGitDir = Path.Combine(helper.Module.WorkingDirGitDir, "modules", "Externals", "Git.hub");
                var submoduleNbugWorkingDirGitDir = Path.Combine(helper.Module.WorkingDirGitDir, "modules", "Externals", "NBug");
                helper.CreateFile(submoduleGitHub, ".git", "gitdir: ../../.git/modules/Externals/Git.hub");
                helper.CreateFile(submoduleNBug, ".git", "gitdir: ../../.git/modules/Externals/NBug");
                helper.CreateFile(helper.Module.WorkingDirGitDir, IndexLock, "");
                helper.CreateFile(submoduleGitHubWorkingDirGitDir, IndexLock, "");

                _manager = new IndexLockManager(helper.Module);

                var indexLock = Path.Combine(helper.Module.WorkingDirGitDir, IndexLock);
                File.Exists(indexLock).Should().BeTrue();
                var gitHubIndexLock = Path.Combine(submoduleGitHubWorkingDirGitDir, IndexLock);
                File.Exists(gitHubIndexLock).Should().BeTrue();
                var nbugIndexLock = Path.Combine(submoduleNbugWorkingDirGitDir, IndexLock);
                File.Exists(nbugIndexLock).Should().BeFalse();

                _manager.UnlockIndex(true);

                File.Exists(indexLock).Should().BeFalse();
                File.Exists(gitHubIndexLock).Should().BeFalse();
                File.Exists(nbugIndexLock).Should().BeFalse();
            }
        }
    }
}

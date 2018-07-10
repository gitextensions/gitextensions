using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using GitCommands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class SshPathLocatorTest
    {
        private IFileSystem _fileSystem;
        private IEnvironmentAbstraction _environment;

        [SetUp]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _environment = Substitute.For<IEnvironmentAbstraction>();
        }

        [Test]
        public void Find_should_return_GIT_SSH_environment_variable_if_set()
        {
            const string path = @"C:\somedir\ssh.exe";
            _environment.GetEnvironmentVariable("GIT_SSH", EnvironmentVariableTarget.Process).Returns(path);
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(@"c:\someotherdir").Should().Be(path);
        }

        [Test]
        public void Find_on_null_should_return_empty_string_if_no_GIT_SSH_is_set()
        {
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(null).Should().Be(string.Empty);
        }

        [Test]
        public void Find_on_gitBinDir_having_no_parent_should_return_empty_string_if_no_GIT_SSH_is_set()
        {
            const string path = @"C:\";
            var directoryBase = Substitute.For<DirectoryBase>();
            directoryBase.GetParent(path).Returns((DirectoryInfoBase)null);
            _fileSystem.Directory.Returns(directoryBase);
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(path).Should().Be(string.Empty);
        }

        [Test]
        public void File_system_access_throwing_should_return_empty_string()
        {
            const string path = @"C:\";
            var directoryBase = Substitute.For<DirectoryBase>();
            directoryBase.GetParent(path).Throws<Exception>();
            _fileSystem.Directory.Returns(directoryBase);
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(path).Should().Be(string.Empty);
        }

        [Test]
        public void Find_on_gitBinDir_parent_throwing_should_return_empty_string()
        {
            const string path = @"C:\";
            var directoryBase = Substitute.For<DirectoryBase>();
            directoryBase.GetParent(path).Throws<Exception>();
            _fileSystem.Directory.Returns(directoryBase);
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(path).Should().Be(string.Empty);
        }

        [Test]
        public void Find_on_gitBinDir_having_ssh_exe_in_parent_directory_children_should_return_first_ssh_exe_found()
        {
            var path = SetUpFileSystemWithSshExePathsAs("first", "second");
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(path).Should().Be("first");
        }

        [Test]
        public void Find_on_gitBinDir_having_no_ssh_exe_in_parent_directory_children_should_return_empty_string()
        {
            var path = SetUpFileSystemWithSshExePathsAs();
            var sshPathLocator = new SshPathLocator(_fileSystem, _environment);
            sshPathLocator.Find(path).Should().Be(string.Empty);
        }

        private string SetUpFileSystemWithSshExePathsAs(params string[] sshExePaths)
        {
            const string path = @"C:\somedir";
            const string parentPath = @"C:\";
            var gitDir = Substitute.For<DirectoryInfoBase>();
            gitDir.FullName.Returns(parentPath);
            var directoryBase = Substitute.For<DirectoryBase>();
            directoryBase.GetParent(path).Returns(gitDir);
            directoryBase.EnumerateFiles(parentPath, "ssh.exe", SearchOption.AllDirectories).Returns(sshExePaths);
            _fileSystem.Directory.Returns(directoryBase);
            return path;
        }
    }
}
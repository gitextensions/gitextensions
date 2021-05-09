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

        [SetUp]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("nul")]
        [TestCase(@"y:\unknown\dir")]
        [TestCase("$?")]
        public void GetSshFromGitDir_should_return_null_if_invalid_gitgir(string gitdir)
        {
            SshPathLocator sshPathLocator = new(_fileSystem);
            sshPathLocator.GetSshFromGitDir(gitdir).Should().BeNull();
        }

        [Test]
        public void GetSshFromGitDir_should_return_null_if_system_access_throwing()
        {
            const string path = @"C:\";
            var directoryBase = Substitute.For<DirectoryBase>();
            directoryBase.GetParent(path).Throws<Exception>();
            _fileSystem.Directory.Returns(directoryBase);
            SshPathLocator sshPathLocator = new(_fileSystem);
            sshPathLocator.GetSshFromGitDir(path).Should().BeNull();
        }

        [Test]
        public void GetSshFromGitDir_on_gitBinDir_having_no_ssh_exe_in_parent_directory_children_should_return_null()
        {
            string path = SetUpFileSystemWithSshExePathsAs();
            SshPathLocator sshPathLocator = new(_fileSystem);
            sshPathLocator.GetSshFromGitDir(path).Should().BeNull();
        }

        [Theory]
        public void GetSshFromGitDir_on_gitBinDir_should_work_with_or_without_trailing_separator(bool withTrailingSeparator)
        {
            const string sshExe = @"C:\someotherdir\ssh.exe";
            string path = SetUpFileSystemWithSshExePathsAs(sshExe);
            SshPathLocator sshPathLocator = new(_fileSystem);
            sshPathLocator.GetSshFromGitDir(@"C:\someotherdir" + (withTrailingSeparator ? @"\" : "")).Should().BeNull();
        }

        private string SetUpFileSystemWithSshExePathsAs(params string[] sshExePaths)
        {
            const string path = @"C:\somedir";
            const string parentPath = @"C:\";
            var gitBinDir = Substitute.For<DirectoryInfoBase>();
            gitBinDir.FullName.Returns(path);
            var gitDir = Substitute.For<DirectoryInfoBase>();
            gitDir.FullName.Returns(parentPath);
            var directoryBase = Substitute.For<DirectoryBase>();
            directoryBase.GetParent(path).Returns(gitDir);

            // a single trailing separator is considered a sub-directory by Directory.GetParent
            directoryBase.GetParent(path + @"\").Returns(gitBinDir);

            directoryBase.EnumerateFiles(parentPath, "ssh.exe", SearchOption.AllDirectories).Returns(sshExePaths);
            _fileSystem.Directory.Returns(directoryBase);
            return path;
        }
    }
}

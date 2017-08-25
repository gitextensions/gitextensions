using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class CommitTemplateManagerTests
    {
        private IGitModule _module;
        private FileBase _file;
        private IFileSystem _fileSystem;
        private CommitTemplateManager _manager;


        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _file = Substitute.For<FileBase>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);
            _manager = new CommitTemplateManager(_module, _fileSystem);
        }


        [TestCase(null)]
        [TestCase("")]
        public void LoadGitCommitTemplate_commit_template_not_defined(string template)
        {
            _module.GetEffectiveSetting("commit.template").Returns(template);

            _manager.LoadGitCommitTemplate().Should().BeNull();
        }

        [TestCase("~/template.txt")]
        [TestCase("./template.txt")]
        [TestCase("template.txt")]
        public void LoadGitCommitTemplate_should_throw_if_template_not_found(string template)
        {
            const string workingDir = @"c:\dev\repo";
            _module.WorkingDir.Returns(workingDir);
            _module.GetEffectiveSetting("commit.template").Returns(template);

            ((Action)(() => _manager.LoadGitCommitTemplate())).ShouldThrow<FileNotFoundException>();
        }

        [Test]
        public void LoadGitCommitTemplate_should_resolve_full_path_to_template()
        {
            const string workingDir = @"c:\dev\repo";
            _module.WorkingDir.Returns(workingDir);
            _module.GetEffectiveSetting("commit.template").Returns("./template.txt");
            _file.Exists(Arg.Any<string>()).Returns(true);
            _file.ReadAllText(Arg.Any<string>()).Returns("");

            _manager.LoadGitCommitTemplate();

            _file.Received(1).Exists(Path.Combine(workingDir, "template.txt"));
        }

        [Test]
        public void LoadGitCommitTemplate_should_load_file_content()
        {
            const string workingDir = @"c:\dev\repo";
            _module.WorkingDir.Returns(workingDir);
            _module.GetEffectiveSetting("commit.template").Returns("./template.txt");
            _file.Exists(Arg.Any<string>()).Returns(true);
            _file.ReadAllText(Arg.Any<string>()).Returns("line1");

            var content = _manager.LoadGitCommitTemplate();

            content.Should().NotBeEmpty();
        }

        [Test]
        public void LoadGitCommitTemplate_should_replace_CR_in_file_content()
        {
            const string workingDir = @"c:\dev\repo";
            _module.WorkingDir.Returns(workingDir);
            _module.GetEffectiveSetting("commit.template").Returns("./template.txt");
            _file.Exists(Arg.Any<string>()).Returns(true);
            _file.ReadAllText(Arg.Any<string>()).Returns("line1\r\nline2\rline3\nline4");

            var content = _manager.LoadGitCommitTemplate();

            content.Should().Be("line1\nline2line3\nline4");
        }
    }
}
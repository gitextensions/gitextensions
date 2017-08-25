using System;
using System.IO;
using FluentAssertions;
using GitCommands;
using NUnit.Framework;


namespace GitCommandsTests
{
    [TestFixture]
    public class CommitTemplateManagerTests_2
    {
        private GitModule _module;
        private CommitTemplateManager _manager;


        [SetUp]
        public void Setup()
        {
            _module = TestUtl.CreateEmptyRepo();
            _manager = new CommitTemplateManager(_module);
        }


        [TestCase(null)]
        [TestCase("")]
        public void XLoadGitCommitTemplate_commit_template_not_defined(string template)
        {
            _module.LocalConfigFile.SetValue("commit.template", template);

            _manager.LoadGitCommitTemplate().Should().BeNull();
        }

        [TestCase("~/template.txt")]
        [TestCase("./template.txt")]
        [TestCase("template.txt")]
        public void XLoadGitCommitTemplate_should_throw_if_template_not_found(string template)
        {
            _module.LocalConfigFile.SetValue("commit.template", template);

            ((Action)(() => _manager.LoadGitCommitTemplate())).ShouldThrow<FileNotFoundException>();
        }

        [Test]
        public void XLoadGitCommitTemplate_should_resolve_full_path_to_template()
        {
            _module.LocalConfigFile.SetValue("commit.template", "./template.txt");
            TestUtl.Touch(_module.WorkingDir, "template.txt", "");

            _manager.LoadGitCommitTemplate();
        }

        [Test]
        public void XLoadGitCommitTemplate_should_load_file_content()
        {
            _module.LocalConfigFile.SetValue("commit.template", "./template.txt");
            TestUtl.Touch(_module.WorkingDir, "template.txt", "line1");

            var content = _manager.LoadGitCommitTemplate();

            content.Should().NotBeEmpty();
        }

        [Test]
        public void XLoadGitCommitTemplate_should_replace_CR_in_file_content()
        {
            _module.LocalConfigFile.SetValue("commit.template", "./template.txt");
            TestUtl.Touch(_module.WorkingDir, "template.txt", "line1\r\nline2\rline3\nline4");

            var content = _manager.LoadGitCommitTemplate();

            content.Should().Be("line1\nline2line3\nline4");
        }
    }
}

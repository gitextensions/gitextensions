using System;
using System.Collections;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CommonTestUtils;
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
        private readonly string _workingDir = @"c:\dev\repo";
        private IGitModule _module;
        private FileBase _file;
        private IFileSystem _fileSystem;
        private IFullPathResolver _fullPathResolver;
        private CommitTemplateManager _manager;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();

            _fullPathResolver = Substitute.For<IFullPathResolver>();

            _file = Substitute.For<FileBase>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);
            _manager = new CommitTemplateManager(_module, _fullPathResolver, _fileSystem);
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
            _module.WorkingDir.Returns(_workingDir);
            _module.GetEffectiveSetting("commit.template").Returns(template);

            ((Action)(() => _manager.LoadGitCommitTemplate())).Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void LoadGitCommitTemplate_should_load_file_content()
        {
            const string relativePath = "./template.txt";
            string fullPath = Path.GetFullPath(Path.Combine(_workingDir, relativePath));

            _module.WorkingDir.Returns(_workingDir);
            _fullPathResolver.Resolve(relativePath).Returns(fullPath);
            _module.GetEffectiveSetting("commit.template").Returns(relativePath);
            _file.Exists(fullPath).Returns(true);
            _file.ReadAllText(fullPath).Returns("line1");

            var content = _manager.LoadGitCommitTemplate();

            content.Should().NotBeEmpty();
        }

        [Test]
        public void Register_should_not_add_duplicates()
        {
            const string templateName = "template1";
            var count = _manager.RegisteredTemplates.Count();
            _manager.Register(templateName, () => "text1");
            _manager.Register(templateName, () => "text2");
            _manager.RegisteredTemplates.Count().Should().Be(count + 1);
        }

        [Test]
        public void RegisteredTemplates_should_be_threadsafe()
        {
            _manager.Register("template1", () => "text1");
            _manager.Register("template2", () => "text2");
            var i = _manager.RegisteredTemplates.Count() + 1;
            foreach (var managerRegisteredTemplate in _manager.RegisteredTemplates)
            {
                _manager.Unregister(managerRegisteredTemplate.Name);
                _manager.Register($"template{i}", () => "text");
                i++;
            }
        }

        [Test]
        public void RegisteredTemplates_should_be_immutable()
        {
            _manager.Register("template1", () => "text1");
            var expectedCount = _manager.RegisteredTemplates.Count();
            expectedCount.Should().BeGreaterThan(0);
            ((IList)_manager.RegisteredTemplates).Clear();
            _manager.RegisteredTemplates.Count().Should().Be(expectedCount);
        }

        public class IntegrationTests
        {
            [Test]
            public void LoadGitCommitTemplate_real_filesystem()
            {
                using (var helper = new GitModuleTestHelper())
                {
                    var manager = new CommitTemplateManager(helper.Module);

                    const string content = "line1\r\nline2\rline3\nline4";

                    helper.Module.SetSetting("commit.template", "template.txt");
                    helper.CreateRepoFile("template.txt", content);

                    var body = manager.LoadGitCommitTemplate();

                    body.Should().Be(content);
                }
            }
        }
    }
}
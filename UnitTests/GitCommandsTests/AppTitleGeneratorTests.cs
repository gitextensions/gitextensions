using System;
using System.Diagnostics;
using FluentAssertions;
using GitCommands;
using GitCommands.Repository;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class AppTitleGeneratorTests
    {
        private const string ShortName = "gitextension";
        private IAppInformationalVersionProvider _appInformationalVersionProvider;
        private IRepositoryShortNameProvider _repositoryShortNameProvider;
        private AppTitleGenerator _appTitleGenerator;


        [SetUp]
        public void Setup()
        {
            _appInformationalVersionProvider = Substitute.For<IAppInformationalVersionProvider>();
            _repositoryShortNameProvider = Substitute.For<IRepositoryShortNameProvider>();
            _repositoryShortNameProvider.Get(Arg.Any<string>()).Returns(ShortName);

            _appTitleGenerator = new AppTitleGenerator(_appInformationalVersionProvider, _repositoryShortNameProvider);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Generate_should_throw_if_working_folder_invalid(string path)
        {
            ((Action)(() => _appTitleGenerator.Generate(path, false, null))).ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Generate_should_return_default_title_if_not_valid_working_directory()
        {
            var title = _appTitleGenerator.Generate("a", false, null);
            title.Should().Be("Git Extensions");
        }

        [Test]
        public void Generate_should_include_no_branch_if_supplied()
        {
            var title = _appTitleGenerator.Generate("a", true, null);
            title.Should().StartWith($"{ShortName} (no branch) - Git Extensions");
        }

        [Test]
        public void Generate_should_include_suplied_branch_without_braces()
        {
            string branchName = "feature/my_(test)_branch";
            var title = _appTitleGenerator.Generate("a", true, "(" + branchName + ")");
            title.Should().StartWith($"{ShortName} ({branchName}) - Git Extensions");
        }

        [Test]
        public void Generate_should_include_version_number()
        {
            var version = "1.2.3.4";
            _appInformationalVersionProvider.Get().Returns(version);
            var title = _appTitleGenerator.Generate("a", true, null);

            title.Should().StartWith($"{ShortName} (no branch) - Git Extensions v{version}");
        }

        [Conditional("DEBUG")]
        [Test]
        public void Generate_should_include_debug_suffix()
        {
            var version = "1.2.3.4";
            _appInformationalVersionProvider.Get().Returns(version);
            var title = _appTitleGenerator.Generate("a", true, null);

            title.Should().Be($"{ShortName} (no branch) - Git Extensions v{version} -> DEBUG <-");
        }
    }
}
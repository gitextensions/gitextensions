using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class AppTitleGeneratorTests
    {
        private const string ShortName = "gitextension";
        private IRepositoryDescriptionProvider _repositoryDescriptionProvider;
        private AppTitleGenerator _appTitleGenerator;

        [SetUp]
        public void Setup()
        {
            _repositoryDescriptionProvider = Substitute.For<IRepositoryDescriptionProvider>();
            _repositoryDescriptionProvider.Get(Arg.Any<string>()).Returns(ShortName);

            _appTitleGenerator = new AppTitleGenerator(_repositoryDescriptionProvider);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Generate_should_return_default_title_if_invalid_working_directory(string path)
        {
            var title = _appTitleGenerator.Generate(path, false, null);
            title.Should().Be("Git Extensions");
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
        public void Generate_should_include_supplied_branch_without_braces()
        {
            string branchName = "feature/my_(test)_branch";
            var title = _appTitleGenerator.Generate("a", true, "(" + branchName + ")");
            title.Should().StartWith($"{ShortName} ({branchName}) - Git Extensions");
        }

#if DEBUG
        [Test]
        public void Generate_should_include_debug_suffix()
        {
            var title = _appTitleGenerator.Generate("a", true, null);

            title.Should().Be($"{ShortName} (no branch) - Git Extensions [DEBUG]");
        }
#endif
    }
}
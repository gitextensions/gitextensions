using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using NSubstitute;

namespace GitCommandsTests
{
    [TestFixture]
    public class AppTitleGeneratorTests
    {
        private const string ShortName = "gitextension";
        private string _defaultBranchName = "no branch";
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
            string title = _appTitleGenerator.Generate(path, false, null);
            title.Should().Be(AppSettings.ApplicationName);
        }

        [Test]
        public void Generate_should_return_default_title_if_not_valid_working_directory()
        {
            string title = _appTitleGenerator.Generate("a", false, null);
            title.Should().Be(AppSettings.ApplicationName);
        }

        [Test]
        public void Generate_should_include_no_branch_if_supplied()
        {
            string title = _appTitleGenerator.Generate("a", true, null, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($"{ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_include_supplied_branch()
        {
            string branchName = "feature/my_(test)_branch";
            string title = _appTitleGenerator.Generate("a", true, branchName: branchName, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($"{ShortName} ({branchName}) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_include_supplied_pathname()
        {
            string branchName = "feature/my_(test)_branch";
            string pathName = "folder/folder/file";
            string title = _appTitleGenerator.Generate("a", true, branchName: branchName, pathName: pathName);
            title.Should().StartWith($@"""file"" {ShortName} ({branchName}) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_not_quote_already_quoted_pathname()
        {
            string pathName = @"""folder/folder/file""";
            string title = _appTitleGenerator.Generate("a", true, pathName: pathName, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($@"""file"" {ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName}");
        }

        [TestCase(@"""folder/folder/""", @"folder/folder/")]
        [TestCase(@"folder/folder/", @"folder/folder/")]
        [TestCase(@"""folder/folder/f*ile extra""", @"f*ile extra")]
        public void Generate_should_quote_paths_where_filenames_cannot_be_resolved(string pathName, string expectedPath)
        {
            string title = _appTitleGenerator.Generate("a", true, pathName: pathName, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($@"""{expectedPath}"" {ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName}");
        }

#if DEBUG
        [Test]
        public void Generate_should_include_debug_suffix([Values(null, "invalid")] string buildSha)
        {
            string buildBranch = "build_branch";
            AppTitleGenerator.Initialise(buildSha, buildBranch);
            string title = _appTitleGenerator.Generate("a", true, null, defaultBranchName: _defaultBranchName);

            title.Should().Be($"{ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName} [DEBUG]");
        }

        [Test]
        public void Generate_should_include_build_suffix()
        {
            string buildSha = "1234567812345678123456781234567812345678";
            string buildBranch = "build_branch";
            AppTitleGenerator.Initialise(buildSha, buildBranch);
            string title = _appTitleGenerator.Generate("a", true, null, defaultBranchName: _defaultBranchName);

            title.Should().Be($"{ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName} {buildSha[..8]} ({buildBranch})");
        }
#endif
    }
}

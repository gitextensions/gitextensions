using System.Composition;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft.VisualStudio.Composition;
using ExportAttribute = System.ComponentModel.Composition.ExportAttribute;
using PartNotDiscoverableAttribute = System.ComponentModel.Composition.PartNotDiscoverableAttribute;

namespace GitCommandsTests
{
    [TestFixture]
    public class AppTitleGeneratorTests
    {
        private TestComposition _composition;
        private IAppTitleGenerator _appTitleGenerator;
        private string _defaultBranchName = "no branch";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _composition = TestComposition.Empty
                .AddParts(typeof(MockRepositoryDescriptionProvider))
                .AddParts(typeof(IAppTitleGenerator))
                .AddParts(typeof(AppTitleGenerator));
        }

        [SetUp]
        public void Setup()
        {
            ExportProvider mefExportProvider = _composition.ExportProviderFactory.CreateExportProvider();
            _appTitleGenerator = mefExportProvider.GetExportedValue<IAppTitleGenerator>();
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
            title.Should().StartWith($"{MockRepositoryDescriptionProvider.ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_include_supplied_branch()
        {
            string branchName = "feature/my_(test)_branch";
            string title = _appTitleGenerator.Generate("a", true, branchName: branchName, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($"{MockRepositoryDescriptionProvider.ShortName} ({branchName}) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_include_supplied_pathname()
        {
            string branchName = "feature/my_(test)_branch";
            string pathName = "folder/folder/file";
            string title = _appTitleGenerator.Generate("a", true, branchName: branchName, pathName: pathName);
            title.Should().StartWith($@"""file"" {MockRepositoryDescriptionProvider.ShortName} ({branchName}) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_not_quote_already_quoted_pathname()
        {
            string pathName = @"""folder/folder/file""";
            string title = _appTitleGenerator.Generate("a", true, pathName: pathName, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($@"""file"" {MockRepositoryDescriptionProvider.ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName}");
        }

        [TestCase(@"""folder/folder/""", @"folder/folder/")]
        [TestCase(@"folder/folder/", @"folder/folder/")]
        [TestCase(@"""folder/folder/f*ile extra""", @"f*ile extra")]
        public void Generate_should_quote_paths_where_filenames_cannot_be_resolved(string pathName, string expectedPath)
        {
            string title = _appTitleGenerator.Generate("a", true, pathName: pathName, defaultBranchName: _defaultBranchName);
            title.Should().StartWith($@"""{expectedPath}"" {MockRepositoryDescriptionProvider.ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName}");
        }

#if DEBUG
        [Test]
        public void Generate_should_include_debug_suffix([Values(null, "invalid")] string buildSha)
        {
            string buildBranch = "build_branch";
            AppTitleGenerator.Initialise(buildSha, buildBranch);
            string title = _appTitleGenerator.Generate("a", true, null, defaultBranchName: _defaultBranchName);

            title.Should().Be($"{MockRepositoryDescriptionProvider.ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName} [DEBUG]");
        }

        [Test]
        public void Generate_should_include_build_suffix()
        {
            string buildSha = "1234567812345678123456781234567812345678";
            string buildBranch = "build_branch";
            AppTitleGenerator.Initialise(buildSha, buildBranch);
            string title = _appTitleGenerator.Generate("a", true, null, defaultBranchName: _defaultBranchName);

            title.Should().Be($"{MockRepositoryDescriptionProvider.ShortName} ({_defaultBranchName}) - {AppSettings.ApplicationName} {buildSha.Substring(0, 8)} ({buildBranch})");
        }
#endif
    }

    [Shared, PartNotDiscoverable]
    [Export(typeof(IRepositoryDescriptionProvider))]
    internal class MockRepositoryDescriptionProvider : IRepositoryDescriptionProvider
    {
        internal const string ShortName = "gitextension";

        public string Get(string repositoryDir) => ShortName;
    }
}

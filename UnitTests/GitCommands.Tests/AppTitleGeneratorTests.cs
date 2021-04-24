using System.Composition;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft.VisualStudio.Composition;
using NUnit.Framework;
using ExportAttribute = System.ComponentModel.Composition.ExportAttribute;
using PartNotDiscoverableAttribute = System.ComponentModel.Composition.PartNotDiscoverableAttribute;

namespace GitCommandsTests
{
    [TestFixture]
    public class AppTitleGeneratorTests
    {
        private TestComposition _composition;
        private IAppTitleGenerator _appTitleGenerator;

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
            string title = _appTitleGenerator.Generate("a", true, null);
            title.Should().StartWith($"{MockRepositoryDescriptionProvider.ShortName} (no branch) - {AppSettings.ApplicationName}");
        }

        [Test]
        public void Generate_should_include_supplied_branch_without_braces()
        {
            string branchName = "feature/my_(test)_branch";
            string title = _appTitleGenerator.Generate("a", true, "(" + branchName + ")");
            title.Should().StartWith($"{MockRepositoryDescriptionProvider.ShortName} ({branchName}) - {AppSettings.ApplicationName}");
        }

#if DEBUG
        [Test]
        public void Generate_should_include_debug_suffix([Values(null, "invalid")] string buildSha)
        {
            string buildBranch = "build_branch";
            AppTitleGenerator.Initialise(buildSha, buildBranch);
            string title = _appTitleGenerator.Generate("a", true, null);

            title.Should().Be($"{MockRepositoryDescriptionProvider.ShortName} (no branch) - {AppSettings.ApplicationName} [DEBUG]");
        }

        [Test]
        public void Generate_should_include_build_suffix()
        {
            string buildSha = "1234567812345678123456781234567812345678";
            string buildBranch = "build_branch";
            AppTitleGenerator.Initialise(buildSha, buildBranch);
            string title = _appTitleGenerator.Generate("a", true, null);

            title.Should().Be($"{MockRepositoryDescriptionProvider.ShortName} (no branch) - {AppSettings.ApplicationName} {buildSha.Substring(0, 8)} ({buildBranch})");
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

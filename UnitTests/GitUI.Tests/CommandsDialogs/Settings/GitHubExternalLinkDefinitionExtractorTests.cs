using FluentAssertions;
using GitCommands.ExternalLinks;
using GitUI.CommandsDialogs.SettingsDialog.RevisionLinks;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.Settings
{
    [TestFixture]
    public class GitHubExternalLinkDefinitionExtractorTests
    {
        [TestCase("https://github.com/owner/repo.git")]
        [TestCase("http://github.com/owner/repo.git")]
        [TestCase("https://github.com/owner/repo")]
        [TestCase("ssh://git@github.com/owner/repo.git")]
        [TestCase("git@github.com/owner/repo.git")]
        public void Should_get_link_definitions(string url)
        {
            var externalLinkDefinitions = new GitHubExternalLinkDefinitionExtractor().GetDefinitions(url);
            externalLinkDefinitions.Should().HaveCount(3);
            foreach (ExternalLinkDefinition externalLinkDefinition in externalLinkDefinitions)
            {
                externalLinkDefinition.LinkFormats.Should().HaveCountGreaterOrEqualTo(1);
                foreach (ExternalLinkFormat externalLinkFormat in externalLinkDefinition.LinkFormats)
                {
                    externalLinkFormat.Format.Should().Contain("owner").And.Contain("repo");
                }
            }
        }

        [Test]
        public void Should_get_link_definitions_When_no_remote_url_provided()
        {
            var externalLinkDefinitions = new GitHubExternalLinkDefinitionExtractor().GetDefinitions(null);
            externalLinkDefinitions.Should().HaveCount(3);
            foreach (ExternalLinkDefinition externalLinkDefinition in externalLinkDefinitions)
            {
                externalLinkDefinition.LinkFormats.Should().HaveCountGreaterOrEqualTo(1);
                foreach (ExternalLinkFormat externalLinkFormat in externalLinkDefinition.LinkFormats)
                {
                    externalLinkFormat.Format.Should().Contain("ORGANIZATION_NAME").And.Contain("REPO_NAME");
                }
            }
        }
    }
}

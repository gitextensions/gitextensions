using FluentAssertions;
using GitCommands.ExternalLinks;
using GitUI.CommandsDialogs.SettingsDialog.RevisionLinks;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.Settings
{
    [TestFixture]
    public class AzureDevOpsExternalLinkDefinitionExtractorTests
    {
        [TestCase("https://owner@dev.azure.com/owner/project/_git/repo")]
        [TestCase("git@ssh.dev.azure.com:v3/owner/project/repo")]
        [TestCase("https://owner.visualstudio.com/project/_git/repo")]
        [TestCase("owner@vs-ssh.visualstudio.com:v3/owner/project/repo")]
        public void Should_get_link_definitions_When_successfuly_parsing_remote_url(string url)
        {
            var externalLinkDefinitions = new AzureDevopsExternalLinkDefinitionExtractor().GetDefinitions(url);
            externalLinkDefinitions.Should().HaveCount(2);
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
            var externalLinkDefinitions = new AzureDevopsExternalLinkDefinitionExtractor().GetDefinitions(null);
            externalLinkDefinitions.Should().HaveCount(2);
            foreach (ExternalLinkDefinition externalLinkDefinition in externalLinkDefinitions)
            {
                externalLinkDefinition.LinkFormats.Should().HaveCountGreaterOrEqualTo(1);
                foreach (ExternalLinkFormat externalLinkFormat in externalLinkDefinition.LinkFormats)
                {
                    externalLinkFormat.Format.Should().Contain("ACCOUNT_NAME").And.Contain("REPO_NAME");
                }
            }
        }
    }
}

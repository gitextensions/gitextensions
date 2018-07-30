using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Remotes;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.ExternalLinks
{
    [TestFixture]
    public class ExternalLinkRevisionParserTests
    {
        private IGitRemoteManager _remoteManager;
        private ExternalLinkRevisionParser _parser;
        private ExternalLinkDefinition _linkDef;
        private GitRevision _revision;

        [SetUp]
        public void Setup()
        {
            _linkDef = Parse(GetGitHubIssuesXmlDef()).First();

            _revision = new GitRevision(ObjectId.Random());

            _remoteManager = Substitute.For<IGitRemoteManager>();
            _remoteManager.LoadRemotes(false).Returns(GetDefaultRemotes());

            _parser = new ExternalLinkRevisionParser(_remoteManager);
        }

        [Test]
        public void ParseGitHubIssueForUpstreamLink()
        {
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            var expectedLinks = new[]
            {
                new ExternalLink("Issue 3657", "https://github.com/gitextensions/gitextensions/issues/3657")
            };

            var actualLinks = _parser.Parse(_revision, _linkDef);
            actualLinks.Should().Equal(expectedLinks);
        }

        [Test]
        public void ParseGitHubIssueForOriginLink()
        {
            _linkDef.UseRemotesPattern = "origin|upstream";
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            var expectedLinks = new[]
            {
                new ExternalLink("Issue 3657", "https://github.com/jbialobr/gitextensions/issues/3657")
            };

            var actualLinks = _parser.Parse(_revision, _linkDef);
            actualLinks.Should().Equal(expectedLinks);
        }

        [Test]
        public void ParseGitHubIssueForUpstreamAndOriginLink()
        {
            _linkDef.UseOnlyFirstRemote = false;
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            var expectedLinks = new[]
            {
                new ExternalLink("Issue 3657", "https://github.com/gitextensions/gitextensions/issues/3657"),
                new ExternalLink("Issue 3657", "https://github.com/jbialobr/gitextensions/issues/3657")
            };

            var actualLinks = _parser.Parse(_revision, _linkDef);
            actualLinks.Should().Equal(expectedLinks);
        }

        [Test]
        public void ParseGitHubIssueForAllRemotesLink()
        {
            _linkDef.UseRemotesPattern = string.Empty;
            _linkDef.UseOnlyFirstRemote = false;
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            var expectedLinks = new[]
            {
                new ExternalLink("Issue 3657", "https://github.com/jbialobr/gitextensions/issues/3657"),
                new ExternalLink("Issue 3657", "https://github.com/gitextensions/gitextensions/issues/3657"),
                new ExternalLink("Issue 3657", "https://github.com/russkie/gitextensions/issues/3657")
            };

            var actualLinks = _parser.Parse(_revision, _linkDef);
            actualLinks.Should().Equal(expectedLinks);
        }

        [Test]
        public void ParseLinkWithEmptyRemotePart()
        {
            _linkDef = Parse(GetEmptyRemotePartXmlDef()).First();
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            var expectedLinks = new[]
            {
                new ExternalLink("Issue 3657", "https://github.com/gitextensions/gitextensions/issues/3657")
            };

            var actualLinks = _parser.Parse(_revision, _linkDef);
            actualLinks.Should().Equal(expectedLinks);
        }

        private static BindingList<GitRemote> GetDefaultRemotes()
        {
            var remotes = new BindingList<GitRemote>();
            remotes.Add(new GitRemote
            {
                Name = "origin",
                Url = "https://github.com/jbialobr/gitextensions.git"
            });

            remotes.Add(new GitRemote
            {
                Name = "upstream",
                Url = "https://github.com/gitextensions/gitextensions.git"
            });

            remotes.Add(new GitRemote
            {
                Name = "RussKie",
                Url = "https://github.com/russkie/gitextensions.git"
            });

            return remotes;
        }

        [CanBeNull]
        private static IReadOnlyList<ExternalLinkDefinition> Parse(string xml)
        {
            var serializer = new XmlSerializer(typeof(List<ExternalLinkDefinition>));
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.Deserialize(xmlReader) as List<ExternalLinkDefinition>;
                }
            }
        }

        private static string GetGitHubIssuesXmlDef()
        {
            return @"<?xml version=""1.0"" ?>
<ArrayOfGitExtLinkDef>
<GitExtLinkDef>
    <SearchInParts>
        <RevisionPart>Message</RevisionPart>
        <RevisionPart>LocalBranches</RevisionPart>
        <RevisionPart>RemoteBranches</RevisionPart>
    </SearchInParts>
    <RemoteSearchInParts>
        <RemotePart>URL</RemotePart>
    </RemoteSearchInParts>
    <LinkFormats>
        <GitExtLinkFormat>
            <Caption>Issue {1}</Caption>
            <Format>https://github.com/{0}/issues/{1}</Format>
        </GitExtLinkFormat>
    </LinkFormats>
    <Name>GitHub - issues</Name>
    <SearchPattern>(\s*(,|and)?\s*#\d+)+</SearchPattern>
    <NestedSearchPattern>(\d+)+</NestedSearchPattern>
    <Enabled>true</Enabled>
    <RemoteSearchPattern>github.com[:/](.+)\.git</RemoteSearchPattern>
    <UseRemotesPattern>upstream|origin</UseRemotesPattern>
    <UseOnlyFirstRemote>true</UseOnlyFirstRemote>
</GitExtLinkDef>
</ArrayOfGitExtLinkDef>
       ";
        }

        private static string GetEmptyRemotePartXmlDef()
        {
            return @"<?xml version=""1.0"" ?>
<ArrayOfGitExtLinkDef>
<GitExtLinkDef>
    <SearchInParts>
        <RevisionPart>Message</RevisionPart>
        <RevisionPart>LocalBranches</RevisionPart>
        <RevisionPart>RemoteBranches</RevisionPart>
    </SearchInParts>
    <RemoteSearchInParts>
    </RemoteSearchInParts>
    <LinkFormats>
        <GitExtLinkFormat>
            <Caption>Issue {0}</Caption>
            <Format>https://github.com/gitextensions/gitextensions/issues/{0}</Format>
        </GitExtLinkFormat>
    </LinkFormats>
    <Name>GitHub - issues</Name>
    <SearchPattern>(\s*(,|and)?\s*#\d+)+</SearchPattern>
    <NestedSearchPattern>(\d+)+</NestedSearchPattern>
    <Enabled>true</Enabled>
    <RemoteSearchPattern></RemoteSearchPattern>
    <UseRemotesPattern></UseRemotesPattern>
    <UseOnlyFirstRemote>false</UseOnlyFirstRemote>
</GitExtLinkDef>
</ArrayOfGitExtLinkDef>
       ";
        }
    }
}

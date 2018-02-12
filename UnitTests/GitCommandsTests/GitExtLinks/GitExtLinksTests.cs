using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using GitCommands;
using GitCommands.GitExtLinks;
using GitCommands.Remote;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.GitExtLinks
{
    [TestFixture]
    public class GitExtLinksTests
    {
        private IGitRemoteManager _remoteManager;
        private GitExtLinkDef _linkDef;
        private GitRevision _revision;


        [SetUp]
        public void Setup()
        {
            _linkDef = GetGithubIssuesLinkDef();

            _revision = new GitRevision(null, "");

            _remoteManager = Substitute.For<IGitRemoteManager>();
            _remoteManager.LoadRemotes(false).Returns(GetDefaultRemotes());
        }


        [Test]
        public void ParseGithubIssueForUpstreamLink()
        {
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            IEnumerable<GitExtLink> expectedLinks = new[]
            {
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/gitextensions/gitextensions/issues/3657"
                }
            };

            var actualLinks = _linkDef.Parse(_revision, _remoteManager);
            actualLinks.Should().Equal(expectedLinks);
        }

        [Test]
        public void ParseGithubIssueForOriginLink()
        {
            _linkDef.UseRemotesPattern = "origin|upstream";
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            IEnumerable<GitExtLink> expectedLinks = new[]
            {
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/jbialobr/gitextensions/issues/3657"
                }
            };

            var actualLinks = _linkDef.Parse(_revision, _remoteManager);
            actualLinks.Should().Equal(expectedLinks);
        }


        [Test]
        public void ParseGithubIssueForUpstreamAndOriginLink()
        {
            _linkDef.UseOnlyFirstRemote = false;
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            IEnumerable<GitExtLink> expectedLinks = new[]
            {
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/gitextensions/gitextensions/issues/3657"
                },
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/jbialobr/gitextensions/issues/3657"
                }
            };

            var actualLinks = _linkDef.Parse(_revision, _remoteManager);
            actualLinks.Should().Equal(expectedLinks);
        }

        [Test]
        public void ParseGithubIssueForAllRemotesLink()
        {
            _linkDef.UseRemotesPattern = string.Empty;
            _linkDef.UseOnlyFirstRemote = false;
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            IEnumerable<GitExtLink> expectedLinks = new[]
            {
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/jbialobr/gitextensions/issues/3657"
                },
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/gitextensions/gitextensions/issues/3657"
                },
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/russkie/gitextensions/issues/3657"
                }
            };

            var actualLinks = _linkDef.Parse(_revision, _remoteManager);
            actualLinks.Should().Equal(expectedLinks);
        }


        [Test]
        public void ParseLinkWithEmptyRemotePart()
        {
            _linkDef = GitExtLinksParser.LoadFromXmlString(GetEmptyRemotePartXmlDef()).First();
            _revision.Body = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";
            IEnumerable<GitExtLink> expectedLinks = new[]
            {
                new GitExtLink()
                {
                    Caption = "Issue 3657",
                    URI = "https://github.com/gitextensions/gitextensions/issues/3657"
                }
            };

            var actualLinks = _linkDef.Parse(_revision, _remoteManager);
            actualLinks.Should().Equal(expectedLinks);
        }


        private static BindingList<GitRemote> GetDefaultRemotes()
        {
            var remotes = new BindingList<GitRemote>();
            remotes.Add(new GitRemote()
            {
                Name = "origin",
                Url = "https://github.com/jbialobr/gitextensions.git"
            });

            remotes.Add(new GitRemote()
            {
                Name = "upstream",
                Url = "https://github.com/gitextensions/gitextensions.git"
            });

            remotes.Add(new GitRemote()
            {
                Name = "RussKie",
                Url = "https://github.com/russkie/gitextensions.git"
            });

            return remotes;
        }

        private static GitExtLinkDef GetGithubIssuesLinkDef()
        {
            return GitExtLinksParser.LoadFromXmlString(GetGithubIssuesXmlDef()).First();
        }

        private static string GetGithubIssuesXmlDef()
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

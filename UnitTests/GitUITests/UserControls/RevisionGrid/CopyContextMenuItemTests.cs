using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using NUnit.Framework;
using ResourceManager;

namespace GitUITests.UserControls.RevisionGrid
{
    [TestFixture]
    public class CopyContextMenuItemTests
    {
        private string _originalTranslation;
        private CopyContextMenuItem _copyContextMenuItem;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _originalTranslation = AppSettings.CurrentTranslation;
            AppSettings.CurrentTranslation = "en";
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.CurrentTranslation = _originalTranslation;
        }

        [SetUp]
        public void Setup()
        {
            _copyContextMenuItem = new CopyContextMenuItem();
            _copyContextMenuItem.Owner = new ToolStrip();
        }

        [Test]
        public void Should_should_show_nothing_if_no_revision_supplied()
        {
            _copyContextMenuItem.SetRevisionFunc(() => null);

            _copyContextMenuItem.ShowDropDown();

            _copyContextMenuItem.DropDownItems.Count.Should().Be(0);
        }

        [TestCaseSource(nameof(GetArtificialCommits))]
        public void Should_should_show_minimum_info_for_artificial_commits(ObjectId objectId)
        {
            var revisions = new[] { new GitRevision(objectId) };
            _copyContextMenuItem.SetRevisionFunc(() => revisions);

            _copyContextMenuItem.ShowDropDown();

            _copyContextMenuItem.DropDownItems.Count.Should().Be(4);

            _copyContextMenuItem.DropDownItems[0].Text.Should().StartWith(AddHotKey(Strings.GetCommitHash(1), 'C'));
            _copyContextMenuItem.DropDownItems[1].Text.Should().StartWith(AddHotKey(Strings.GetMessage(1), 'M'));
            _copyContextMenuItem.DropDownItems[2].Text.Should().StartWith(AddHotKey(Strings.GetAuthor(1), 'A'));
            _copyContextMenuItem.DropDownItems[3].Text.Should().StartWith(AddHotKey(Strings.Date, 'D'));
        }

        [Test]
        public void Should_show_info_if_commit_has_defined_branches()
        {
            var revision = new GitRevision(ObjectId.Random());
            var refs = new List<IGitRef>
            {
                new GitRef(null, revision.ObjectId, "refs/heads/branch1"),
                new GitRef(null, revision.ObjectId, "refs/heads/branch2")
            };
            revision.Refs = refs;
            var revisions = new[] { revision };
            _copyContextMenuItem.SetRevisionFunc(() => revisions);

            _copyContextMenuItem.ShowDropDown();

            _copyContextMenuItem.DropDownItems.Count.Should().Be(8);
            _copyContextMenuItem.DropDownItems[0].Text.Should().Be(Strings.Branches);
            _copyContextMenuItem.DropDownItems[1].Text.Should().EndWith("branch1");
            _copyContextMenuItem.DropDownItems[2].Text.Should().EndWith("branch2");
            _copyContextMenuItem.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
            _copyContextMenuItem.DropDownItems[4].Text.Should().StartWith(AddHotKey(Strings.GetCommitHash(1), 'C'));
            _copyContextMenuItem.DropDownItems[5].Text.Should().StartWith(AddHotKey(Strings.GetMessage(1), 'M'));
            _copyContextMenuItem.DropDownItems[6].Text.Should().StartWith(AddHotKey(Strings.GetAuthor(1), 'A'));
            _copyContextMenuItem.DropDownItems[7].Text.Should().StartWith(AddHotKey(Strings.Date, 'D'));
        }

        [Test]
        public void Should_show_info_if_commit_has_defined_tags()
        {
            var revision = new GitRevision(ObjectId.Random());
            var refs = new List<IGitRef>
            {
                new GitRef(null, revision.ObjectId, "refs/tags/tag1"),
                new GitRef(null, revision.ObjectId, "refs/tags/tag2")
            };
            revision.Refs = refs;
            var revisions = new[] { revision };
            _copyContextMenuItem.SetRevisionFunc(() => revisions);

            _copyContextMenuItem.ShowDropDown();

            _copyContextMenuItem.DropDownItems.Count.Should().Be(8);
            _copyContextMenuItem.DropDownItems[0].Text.Should().Be(Strings.Tags);
            _copyContextMenuItem.DropDownItems[1].Text.Should().EndWith("tag1");
            _copyContextMenuItem.DropDownItems[2].Text.Should().EndWith("tag2");
            _copyContextMenuItem.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
            _copyContextMenuItem.DropDownItems[4].Text.Should().StartWith(AddHotKey(Strings.GetCommitHash(1), 'C'));
            _copyContextMenuItem.DropDownItems[5].Text.Should().StartWith(AddHotKey(Strings.GetMessage(1), 'M'));
            _copyContextMenuItem.DropDownItems[6].Text.Should().StartWith(AddHotKey(Strings.GetAuthor(1), 'A'));
            _copyContextMenuItem.DropDownItems[7].Text.Should().StartWith(AddHotKey(Strings.Date, 'D'));
        }

        [Test]
        public void Should_show_info_if_commit_has_defined_branches_and_tags()
        {
            var revision = new GitRevision(ObjectId.Random());
            var refs = new List<IGitRef>
            {
                new GitRef(null, revision.ObjectId, "refs/tags/tag1"),
                new GitRef(null, revision.ObjectId, "refs/heads/branch1"),
                new GitRef(null, revision.ObjectId, "refs/tags/tag2"),
                new GitRef(null, revision.ObjectId, "refs/heads/branch2"),
            };
            revision.Refs = refs;
            var revisions = new[] { revision };
            _copyContextMenuItem.SetRevisionFunc(() => revisions);

            _copyContextMenuItem.ShowDropDown();

            _copyContextMenuItem.DropDownItems.Count.Should().Be(12);
            _copyContextMenuItem.DropDownItems[0].Text.Should().Be(Strings.Branches);
            _copyContextMenuItem.DropDownItems[1].Text.Should().EndWith("branch1");
            _copyContextMenuItem.DropDownItems[2].Text.Should().EndWith("branch2");
            _copyContextMenuItem.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
            _copyContextMenuItem.DropDownItems[4].Text.Should().Be(Strings.Tags);
            _copyContextMenuItem.DropDownItems[5].Text.Should().EndWith("tag1");
            _copyContextMenuItem.DropDownItems[6].Text.Should().EndWith("tag2");
            _copyContextMenuItem.DropDownItems[7].Should().BeOfType<ToolStripSeparator>();
            _copyContextMenuItem.DropDownItems[8].Text.Should().StartWith(AddHotKey(Strings.GetCommitHash(1), 'C'));
            _copyContextMenuItem.DropDownItems[9].Text.Should().StartWith(AddHotKey(Strings.GetMessage(1), 'M'));
            _copyContextMenuItem.DropDownItems[10].Text.Should().StartWith(AddHotKey(Strings.GetAuthor(1), 'A'));
            _copyContextMenuItem.DropDownItems[11].Text.Should().StartWith(AddHotKey(Strings.Date, 'D'));
        }

        [Test]
        public void Should_should_show_info_for_multiple_commits()
        {
            var rev1 = new GitRevision(ObjectId.Random())
            {
                Author = "Author1",
                AuthorEmail = "author1@foo.bla",
                AuthorDate = new DateTime(2018, 10, 23, 11, 34, 21),
            };
            var rev2 = new GitRevision(ObjectId.Random())
            {
                Author = "Author2",
                AuthorEmail = "author2@foo.bla",
                Committer = "Committer2",
                CommitterEmail = "committer2@foo.bar",
                CommitDate = new DateTime(2018, 10, 23, 11, 34, 21),
            };
            var rev3 = new GitRevision(ObjectId.Random())
            {
                Author = "Author3",
                AuthorEmail = "author3@foo.bla",
            };
            var revisions = new[] { rev1, rev2, rev3 };
            _copyContextMenuItem.SetRevisionFunc(() => revisions);

            _copyContextMenuItem.ShowDropDown();

            _copyContextMenuItem.DropDownItems.Count.Should().Be(5);

            _copyContextMenuItem.DropDownItems[0].Text.Should().StartWith(AddHotKey(Strings.GetCommitHash(revisions.Length), 'C'));
            _copyContextMenuItem.DropDownItems[1].Text.Should().StartWith(AddHotKey(Strings.GetMessage(revisions.Length), 'M'));
            _copyContextMenuItem.DropDownItems[2].Text.Should().StartWith(AddHotKey(Strings.GetAuthor(revisions.Length), 'A'));
            _copyContextMenuItem.DropDownItems[3].Text.Should().StartWith(AddHotKey(Strings.GetAuthorDate(revisions.Length), 'T'));
            _copyContextMenuItem.DropDownItems[4].Text.Should().StartWith(AddHotKey(Strings.GetCommitDate(revisions.Length), 'D'));
        }

        private string AddHotKey(string label, char? hotkey)
        {
            if (!hotkey.HasValue)
            {
                return label;
            }

            int position = label.IndexOf(hotkey.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
            if (position >= 0)
            {
                label = label.Insert(position, "&");
            }

            return label;
        }

        private static IEnumerable<ObjectId> GetArtificialCommits
        {
            get
            {
                yield return ObjectId.WorkTreeId;
                yield return ObjectId.IndexId;
                yield return ObjectId.CombinedDiffId;
            }
        }
    }
}
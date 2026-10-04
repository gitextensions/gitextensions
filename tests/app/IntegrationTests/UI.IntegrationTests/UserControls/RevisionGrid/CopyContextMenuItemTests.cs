using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitExtensions.UITests.UserControls.RevisionGrid;
public class CopyContextMenuItemTests
{
    private string? _originalTranslation;
    private CopyContextMenuItem _copyContextMenuItem = null!;

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
        _copyContextMenuItem = new CopyContextMenuItem
        {
            Owner = new ToolStrip()
        };
    }

    [TearDown]
    public void TearDown()
    {
        _copyContextMenuItem.Owner!.Dispose();
        _copyContextMenuItem.Dispose();
    }

    [Test]
    public void Fixed_item_should_copy_the_selection_without_the_menu_ever_being_opened()
    {
        // A toolbar button clicks the item directly; the Copy menu may never have been shown.
        GitRevision revision = new(ObjectId.Random());
        _copyContextMenuItem.SetRevisionFunc(() => [revision]);
        List<string> copied = CaptureCopies();

        _copyContextMenuItem.CommitHashMenuItem.PerformClick();

        copied.Should().Equal(revision.Guid);
    }

    [Test]
    public void Fixed_item_should_copy_the_current_selection_rather_than_the_one_last_shown()
    {
        GitRevision first = new(ObjectId.Random());
        GitRevision second = new(ObjectId.Random());
        GitRevision[] selection = [first];
        _copyContextMenuItem.SetRevisionFunc(() => selection);
        List<string> copied = CaptureCopies();

        _copyContextMenuItem.ShowDropDown();
        _copyContextMenuItem.HideDropDown();
        selection = [second];
        _copyContextMenuItem.CommitHashMenuItem.PerformClick();

        copied.Should().Equal(second.Guid);
    }

    [Test]
    public void Fixed_items_should_each_copy_their_own_part_of_the_selection()
    {
        GitRevision revision = new(ObjectId.Random())
        {
            Author = "John Doe",
            AuthorEmail = "john@example.com",
            Subject = "Subject",
            Body = "Subject\n\nBody",
            AuthorUnixTime = 1_791_115_200
        };
        _copyContextMenuItem.SetRevisionFunc(() => [revision]);
        List<string> copied = CaptureCopies();

        _copyContextMenuItem.MessageMenuItem.PerformClick();
        _copyContextMenuItem.AuthorMenuItem.PerformClick();
        _copyContextMenuItem.DateMenuItem.PerformClick();

        copied.Should().Equal("Subject\n\nBody", "John Doe <john@example.com>", revision.AuthorDate.ToString());
    }

    [Test]
    public void Fixed_item_should_copy_one_line_per_selected_revision()
    {
        GitRevision first = new(ObjectId.Random());
        GitRevision second = new(ObjectId.Random());
        _copyContextMenuItem.SetRevisionFunc(() => [first, second]);
        List<string> copied = CaptureCopies();

        _copyContextMenuItem.CommitHashMenuItem.PerformClick();

        copied.Should().Equal($"{first.Guid}\n{second.Guid}");
    }

    [Test]
    public void Fixed_item_should_copy_nothing_without_a_selection()
    {
        _copyContextMenuItem.SetRevisionFunc(() => []);
        List<string> copied = CaptureCopies();

        _copyContextMenuItem.CommitHashMenuItem.PerformClick();

        copied.Should().BeEmpty();
    }

    private List<string> CaptureCopies()
    {
        List<string> copied = [];
        _copyContextMenuItem.GetTestAccessor().CopyText = text =>
        {
            copied.Add(text);
            return true;
        };
        return copied;
    }

    [Test]
    public void Should_contain_persistent_fixed_items_if_no_revision_supplied()
    {
        _copyContextMenuItem.SetRevisionFunc(() => null!);

        _copyContextMenuItem.ShowDropDown();

        // The four named sub-actions (commit hash, message, author, date) are seeded permanently so
        // toolbar introspection can discover them before any revision is selected. The drop-down is
        // hidden when there is no revision, so these items are never shown to the user.
        _copyContextMenuItem.DropDownItems.Count.Should().Be(4);
    }

    [TestCaseSource(nameof(GetArtificialCommits))]
    public void Should_should_show_minimum_info_for_artificial_commits(ObjectId objectId)
    {
        GitRevision[] revisions = [new GitRevision(objectId)];
        _copyContextMenuItem.SetRevisionFunc(() => revisions);

        _copyContextMenuItem.ShowDropDown();

        _copyContextMenuItem.DropDownItems.Count.Should().Be(4);

        _copyContextMenuItem.DropDownItems[0].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetCommitHash(1), 'C'));
        _copyContextMenuItem.DropDownItems[1].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetMessage(1), 'M'));
        _copyContextMenuItem.DropDownItems[2].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetAuthor(1), 'A'));
        _copyContextMenuItem.DropDownItems[3].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.Date, 'D'));
    }

    [Test]
    public void Should_show_info_if_commit_has_defined_branches()
    {
        GitRevision revision = new(ObjectId.Random());
        List<IGitRef> refs =
        [
            new GitRef(null!, revision.ObjectId, "refs/heads/branch1"),
            new GitRef(null!, revision.ObjectId, "refs/heads/branch2")
        ];
        revision.Refs = refs;
        GitRevision[] revisions = [revision];
        _copyContextMenuItem.SetRevisionFunc(() => revisions);

        _copyContextMenuItem.ShowDropDown();

        _copyContextMenuItem.DropDownItems.Count.Should().Be(8);
        _copyContextMenuItem.DropDownItems[0].Text.Should().Be(TranslatedStrings.Branches);
        _copyContextMenuItem.DropDownItems[1].Text.Should().EndWith("branch1");
        _copyContextMenuItem.DropDownItems[2].Text.Should().EndWith("branch2");
        _copyContextMenuItem.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
        _copyContextMenuItem.DropDownItems[4].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetCommitHash(1), 'C'));
        _copyContextMenuItem.DropDownItems[5].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetMessage(1), 'M'));
        _copyContextMenuItem.DropDownItems[6].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetAuthor(1), 'A'));
        _copyContextMenuItem.DropDownItems[7].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.Date, 'D'));
    }

    [Test]
    public void Should_show_info_if_commit_has_defined_tags()
    {
        GitRevision revision = new(ObjectId.Random());
        List<IGitRef> refs =
        [
            new GitRef(null!, revision.ObjectId, "refs/tags/tag1"),
            new GitRef(null!, revision.ObjectId, "refs/tags/tag2")
        ];
        revision.Refs = refs;
        GitRevision[] revisions = [revision];
        _copyContextMenuItem.SetRevisionFunc(() => revisions);

        _copyContextMenuItem.ShowDropDown();

        _copyContextMenuItem.DropDownItems.Count.Should().Be(8);
        _copyContextMenuItem.DropDownItems[0].Text.Should().Be(TranslatedStrings.Tags);
        _copyContextMenuItem.DropDownItems[1].Text.Should().EndWith("tag1");
        _copyContextMenuItem.DropDownItems[2].Text.Should().EndWith("tag2");
        _copyContextMenuItem.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
        _copyContextMenuItem.DropDownItems[4].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetCommitHash(1), 'C'));
        _copyContextMenuItem.DropDownItems[5].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetMessage(1), 'M'));
        _copyContextMenuItem.DropDownItems[6].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetAuthor(1), 'A'));
        _copyContextMenuItem.DropDownItems[7].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.Date, 'D'));
    }

    [Test]
    public void Should_show_info_if_commit_has_defined_branches_and_tags()
    {
        GitRevision revision = new(ObjectId.Random());
        List<IGitRef> refs =
        [
            new GitRef(null!, revision.ObjectId, "refs/tags/tag1"),
            new GitRef(null!, revision.ObjectId, "refs/heads/branch1"),
            new GitRef(null!, revision.ObjectId, "refs/tags/tag2"),
            new GitRef(null!, revision.ObjectId, "refs/heads/branch2"),
        ];
        revision.Refs = refs;
        GitRevision[] revisions = [revision];
        _copyContextMenuItem.SetRevisionFunc(() => revisions);

        _copyContextMenuItem.ShowDropDown();

        _copyContextMenuItem.DropDownItems.Count.Should().Be(12);
        _copyContextMenuItem.DropDownItems[0].Text.Should().Be(TranslatedStrings.Branches);
        _copyContextMenuItem.DropDownItems[1].Text.Should().EndWith("branch1");
        _copyContextMenuItem.DropDownItems[2].Text.Should().EndWith("branch2");
        _copyContextMenuItem.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
        _copyContextMenuItem.DropDownItems[4].Text.Should().Be(TranslatedStrings.Tags);
        _copyContextMenuItem.DropDownItems[5].Text.Should().EndWith("tag1");
        _copyContextMenuItem.DropDownItems[6].Text.Should().EndWith("tag2");
        _copyContextMenuItem.DropDownItems[7].Should().BeOfType<ToolStripSeparator>();
        _copyContextMenuItem.DropDownItems[8].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetCommitHash(1), 'C'));
        _copyContextMenuItem.DropDownItems[9].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetMessage(1), 'M'));
        _copyContextMenuItem.DropDownItems[10].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetAuthor(1), 'A'));
        _copyContextMenuItem.DropDownItems[11].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.Date, 'D'));
    }

    [Test]
    public void Should_should_show_info_for_multiple_commits()
    {
        GitRevision rev1 = new(ObjectId.Random())
        {
            Author = "Author1",
            AuthorEmail = "author1@foo.bla",
            AuthorUnixTime = DateTimeUtils.ToUnixTime(new DateTime(2018, 10, 23, 11, 34, 21)),
        };
        GitRevision rev2 = new(ObjectId.Random())
        {
            Author = "Author2",
            AuthorEmail = "author2@foo.bla",
            Committer = "Committer2",
            CommitterEmail = "committer2@foo.bar",
            CommitUnixTime = DateTimeUtils.ToUnixTime(new DateTime(2018, 10, 23, 11, 34, 21)),
        };
        GitRevision rev3 = new(ObjectId.Random())
        {
            Author = "Author3",
            AuthorEmail = "author3@foo.bla",
        };
        GitRevision[] revisions = [rev1, rev2, rev3];
        _copyContextMenuItem.SetRevisionFunc(() => revisions);

        _copyContextMenuItem.ShowDropDown();

        _copyContextMenuItem.DropDownItems.Count.Should().Be(5);

        _copyContextMenuItem.DropDownItems[0].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetCommitHash(revisions.Length), 'C'));
        _copyContextMenuItem.DropDownItems[1].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetMessage(revisions.Length), 'M'));
        _copyContextMenuItem.DropDownItems[2].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetAuthor(revisions.Length), 'A'));
        _copyContextMenuItem.DropDownItems[3].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetAuthorDate(revisions.Length), 'T'));
        _copyContextMenuItem.DropDownItems[4].Text.Should().StartWith(AddHotKey(ResourceManager.TranslatedStrings.GetCommitDate(revisions.Length), 'D'));
    }

    private static string AddHotKey(string label, char? hotkey)
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

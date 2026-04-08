using AwesomeAssertions;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.RefContextMenus;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.RefContextMenus;

[TestFixture]
public class LocalBranchContextMenuProviderTests
{
    private LocalBranchContextMenuProvider _provider = null!;
    private IGitUICommands _uiCommands = null!;
    private RefContextMenuContext _context = null!;
    private ObjectId _currentCheckout = null!;

    [SetUp]
    public void Setup()
    {
        _provider = new LocalBranchContextMenuProvider();
        _uiCommands = Substitute.For<IGitUICommands>();
        _currentCheckout = ObjectId.Random();
        _context = new RefContextMenuContext
        {
            UICommands = _uiCommands,
            ParentForm = null,
            CurrentBranchRef = "refs/heads/main",
            CurrentCheckout = _currentCheckout,
            IsBareRepository = false,
            GetRefUnambiguousName = r => r.Name,
            GetLatestSelectedRevision = () => null,
            PerformRefreshRevisions = () => { },
            DropStash = (_, _) => { },
        };
    }

    [Test]
    public void Handles_should_return_true_for_head_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsHead.Returns(true);

        _provider.Handles(gitRef, stashReflogSelector: null).Should().BeTrue();
    }

    [Test]
    public void Handles_should_return_false_for_remote_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsHead.Returns(false);
        gitRef.IsRemote.Returns(true);

        _provider.Handles(gitRef, stashReflogSelector: null).Should().BeFalse();
    }

    [Test]
    public void Handles_should_return_false_for_tag_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsHead.Returns(false);
        gitRef.IsTag.Returns(true);

        _provider.Handles(gitRef, stashReflogSelector: null).Should().BeFalse();
    }

    [Test]
    public void Handles_should_return_false_for_null_ref()
    {
        _provider.Handles(gitRef: null, stashReflogSelector: null).Should().BeFalse();
    }

    [Test]
    public void Populate_should_add_nothing_when_gitRef_is_null()
    {
        using ContextMenuStrip menu = new();
        _provider.Populate(menu, gitRef: null, stashReflogSelector: null, _context);

        menu.Items.Count.Should().Be(0);
    }

    [Test]
    public void Populate_should_include_checkout_for_non_current_branch()
    {
        IGitRef gitRef = CreateLocalBranchRef("feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().Contain(t => t.Contains("Checkout"));
    }

    [Test]
    public void Populate_should_not_include_checkout_for_current_branch()
    {
        IGitRef gitRef = CreateLocalBranchRef("main", ObjectId.Random());
        gitRef.CompleteName.Returns("refs/heads/main");
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().NotContain(t => t.Contains("Checkout"));
    }

    [Test]
    public void Populate_should_include_merge_and_rebase_for_non_current_head_branch()
    {
        IGitRef gitRef = CreateLocalBranchRef("feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        IEnumerable<string> texts = menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text!);
        texts.Should().Contain(t => t.Contains("Merge"));
        texts.Should().Contain(t => t.Contains("Rebase"));
    }

    [Test]
    public void Populate_should_not_include_merge_or_rebase_when_at_current_head()
    {
        IGitRef gitRef = CreateLocalBranchRef("feature", _currentCheckout);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().NotContain(t => t.Contains("Merge"))
            .And.NotContain(t => t.Contains("Rebase"));
    }

    [Test]
    public void Populate_should_include_delete_for_non_current_branch()
    {
        IGitRef gitRef = CreateLocalBranchRef("feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().Contain(t => t.Contains("Delete"));
    }

    [Test]
    public void Populate_should_not_include_delete_for_current_branch()
    {
        IGitRef gitRef = CreateLocalBranchRef("main", ObjectId.Random());
        gitRef.CompleteName.Returns("refs/heads/main");
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().NotContain(t => t.Contains("Delete"));
    }

    [Test]
    public void Populate_should_always_include_rename()
    {
        IGitRef gitRef = CreateLocalBranchRef("main", ObjectId.Random());
        gitRef.CompleteName.Returns("refs/heads/main");
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().Contain(t => t.Contains("name"));
    }

    [Test]
    public void Populate_should_include_push_for_non_bare_repository()
    {
        IGitRef gitRef = CreateLocalBranchRef("feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().Contain(t => t.Contains("Push"));
    }

    [Test]
    public void Populate_should_not_include_push_for_bare_repository()
    {
        RefContextMenuContext bareContext = new()
        {
            UICommands = _uiCommands,
            ParentForm = null,
            CurrentBranchRef = "refs/heads/main",
            CurrentCheckout = _currentCheckout,
            IsBareRepository = true,
            GetRefUnambiguousName = r => r.Name,
            GetLatestSelectedRevision = () => null,
            PerformRefreshRevisions = () => { },
            DropStash = (_, _) => { },
        };

        IGitRef gitRef = CreateLocalBranchRef("feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, bareContext);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().NotContain(t => t.Contains("Push"));
    }

    private static IGitRef CreateLocalBranchRef(string name, ObjectId objectId)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsHead.Returns(true);
        gitRef.IsRemote.Returns(false);
        gitRef.IsTag.Returns(false);
        gitRef.Name.Returns(name);
        gitRef.CompleteName.Returns($"refs/heads/{name}");
        gitRef.ObjectId.Returns(objectId);
        return gitRef;
    }
}

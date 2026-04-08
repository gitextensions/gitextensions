using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.RefContextMenus;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.RefContextMenus;
public class RemoteBranchContextMenuProviderTests
{
    private RemoteBranchContextMenuProvider _provider = null!;
    private IGitUICommands _uiCommands = null!;
    private RefContextMenuContext _context = null!;
    private ObjectId _currentCheckout = ObjectId.Random();

    [SetUp]
    public void Setup()
    {
        _provider = new RemoteBranchContextMenuProvider();
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

    [TearDown]
    public void TearDown()
    {
        ((IDisposable)_provider).Dispose();
    }

    [Test]
    public void Handles_should_return_true_for_remote_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsRemote.Returns(true);

        _provider.Handles(gitRef, stashReflogSelector: null).Should().BeTrue();
    }

    [Test]
    public void Handles_should_return_false_for_head_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsRemote.Returns(false);
        gitRef.IsHead.Returns(true);

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
    public void Populate_should_include_checkout_for_non_bare_repo()
    {
        IGitRef gitRef = CreateRemoteBranchRef("origin/feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text?.Replace("&", ""))
            .Should().Contain(t => t.Contains("Checkout"));
    }

    [Test]
    public void Populate_should_include_merge_and_rebase_when_not_at_head()
    {
        IGitRef gitRef = CreateRemoteBranchRef("origin/feature", ObjectId.Random());
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
        IGitRef gitRef = CreateRemoteBranchRef("origin/feature", _currentCheckout);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().NotContain(t => t.Contains("Merge"))
            .And.NotContain(t => t.Contains("Rebase"));
    }

    [Test]
    public void Populate_should_always_include_delete()
    {
        IGitRef gitRef = CreateRemoteBranchRef("origin/feature", _currentCheckout);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().Contain(t => t.Contains("Delete"));
    }

    [Test]
    public void Populate_should_not_include_checkout_for_bare_repo()
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

        IGitRef gitRef = CreateRemoteBranchRef("origin/feature", ObjectId.Random());
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, bareContext);

        menu.Items.Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator)
            .Select(i => i.Text)
            .Should().NotContain(t => t.Contains("Checkout"))
            .And.NotContain(t => t.Contains("Merge"))
            .And.NotContain(t => t.Contains("Rebase"));
    }

    private static IGitRef CreateRemoteBranchRef(string name, ObjectId objectId)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsHead.Returns(false);
        gitRef.IsRemote.Returns(true);
        gitRef.IsTag.Returns(false);
        gitRef.Name.Returns(name);
        gitRef.CompleteName.Returns($"refs/remotes/{name}");
        gitRef.ObjectId.Returns(objectId);
        return gitRef;
    }
}

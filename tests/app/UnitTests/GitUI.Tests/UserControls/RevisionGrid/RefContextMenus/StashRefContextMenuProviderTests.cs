using AwesomeAssertions;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.RefContextMenus;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.RefContextMenus;

[TestFixture]
public class StashRefContextMenuProviderTests
{
    private StashRefContextMenuProvider _provider = null!;
    private IGitUICommands _uiCommands = null!;
    private RefContextMenuContext _context = null!;

    [SetUp]
    public void Setup()
    {
        _provider = new StashRefContextMenuProvider();
        _uiCommands = Substitute.For<IGitUICommands>();
        _context = new RefContextMenuContext
        {
            UICommands = _uiCommands,
            ParentForm = null,
            CurrentBranchRef = "refs/heads/main",
            CurrentCheckout = ObjectId.Random(),
            IsBareRepository = false,
            GetRefUnambiguousName = r => r.Name,
            GetLatestSelectedRevision = () => null,
            PerformRefreshRevisions = () => { },
            DropStash = (_, _) => { },
        };
    }

    [Test]
    public void Handles_should_return_true_for_stash_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsStash.Returns(true);

        _provider.Handles(gitRef, stashReflogSelector: null).Should().BeTrue();
    }

    [Test]
    public void Handles_should_return_false_for_non_stash_ref()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsStash.Returns(false);
        gitRef.IsHead.Returns(true);

        _provider.Handles(gitRef, stashReflogSelector: null).Should().BeFalse();
    }

    [Test]
    public void Handles_should_return_false_for_null_ref()
    {
        _provider.Handles(gitRef: null, stashReflogSelector: null).Should().BeFalse();
    }

    [Test]
    public void Populate_should_add_apply_pop_and_drop_items()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsStash.Returns(true);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        IEnumerable<string> texts = menu.Items.Cast<ToolStripItem>().Select(i => i.Text!);
        texts.Should().Contain(t => t.Contains("Apply"));
        texts.Should().Contain(t => t.Contains("op stash") && !t.Contains("Drop") && !t.Contains("Dr"));
        texts.Should().Contain(t => t.StartsWith("Dr"));
    }

    [Test]
    public void Populate_should_have_exactly_three_items()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsStash.Returns(true);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef, stashReflogSelector: null, _context);

        menu.Items.Count.Should().Be(3);
    }
}

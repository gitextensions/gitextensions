using AwesomeAssertions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.RefContextMenus;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.RefContextMenus;

[TestFixture]
public class StashSelectorContextMenuProviderTests
{
    private StashSelectorContextMenuProvider _provider = null!;
    private IGitUICommands _uiCommands = null!;

    [SetUp]
    public void Setup()
    {
        _provider = new StashSelectorContextMenuProvider();
        _uiCommands = Substitute.For<IGitUICommands>();
    }

    [Test]
    public void Handles_should_return_true_when_gitRef_is_null_and_selector_is_not_null()
    {
        _provider.Handles(gitRef: null, stashReflogSelector: "stash@{0}").Should().BeTrue();
    }

    [Test]
    public void Handles_should_return_false_when_both_are_null()
    {
        _provider.Handles(gitRef: null, stashReflogSelector: null).Should().BeFalse();
    }

    [Test]
    public void Handles_should_return_false_when_gitRef_is_not_null()
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        _provider.Handles(gitRef, stashReflogSelector: "stash@{0}").Should().BeFalse();
    }

    [Test]
    public void Populate_should_add_nothing_when_revision_is_null()
    {
        RefContextMenuContext context = CreateContext(getLatestSelectedRevision: () => null);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef: null, stashReflogSelector: "stash@{0}", context);

        menu.Items.Count.Should().Be(0);
    }

    [Test]
    public void Populate_should_add_nothing_when_revision_is_not_stash()
    {
        GitRevision revision = new(ObjectId.Random());
        RefContextMenuContext context = CreateContext(getLatestSelectedRevision: () => revision);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef: null, stashReflogSelector: "stash@{0}", context);

        menu.Items.Count.Should().Be(0);
    }

    [Test]
    public void Populate_should_add_apply_pop_and_drop_for_stash_revision()
    {
        GitRevision revision = new(ObjectId.Random())
        {
            ReflogSelector = "stash@{0}",
        };
        RefContextMenuContext context = CreateContext(getLatestSelectedRevision: () => revision);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef: null, stashReflogSelector: "stash@{0}", context);

        IEnumerable<string> texts = menu.Items.Cast<ToolStripItem>().Select(i => i.Text!);
        texts.Should().Contain(t => t.Contains("Apply"));
        texts.Should().Contain(t => t.StartsWith("P") && t.Contains("op stash"));
        texts.Should().Contain(t => t.StartsWith("Dr"));
        menu.Items.Count.Should().Be(3);
    }

    [Test]
    public void Populate_should_add_only_apply_for_autostash_revision()
    {
        GitRevision revision = new(ObjectId.Random())
        {
            IsAutostash = true,
        };
        RefContextMenuContext context = CreateContext(getLatestSelectedRevision: () => revision);
        using ContextMenuStrip menu = new();

        _provider.Populate(menu, gitRef: null, stashReflogSelector: "stash@{0}", context);

        menu.Items.Count.Should().Be(1);
        menu.Items[0].Text.Should().Contain("Apply");
    }

    private RefContextMenuContext CreateContext(Func<GitRevision?> getLatestSelectedRevision)
    {
        return new RefContextMenuContext
        {
            UICommands = _uiCommands,
            ParentForm = null,
            CurrentBranchRef = "refs/heads/main",
            CurrentCheckout = ObjectId.Random(),
            IsBareRepository = false,
            GetRefUnambiguousName = r => r.Name,
            GetLatestSelectedRevision = getLatestSelectedRevision,
            PerformRefreshRevisions = () => { },
            DropStash = (_, _) => { },
        };
    }
}

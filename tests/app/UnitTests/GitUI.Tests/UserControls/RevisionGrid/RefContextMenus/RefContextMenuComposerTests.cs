using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid.RefContextMenus;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.RefContextMenus;
public class RefContextMenuComposerTests
{
    private IGitUICommands _uiCommands = null!;
    private RefContextMenuContext _context = null!;

    [SetUp]
    public void Setup()
    {
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
    public void Build_should_return_null_when_no_provider_handles_ref()
    {
        IRefContextMenuProvider provider = Substitute.For<IRefContextMenuProvider>();
        provider.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(false);

        RefContextMenuComposer composer = new([provider]);

        ContextMenuStrip? menu = composer.Build(gitRef: null, stashReflogSelector: null, _context);

        menu.Should().BeNull();
    }

    [Test]
    public void Build_should_return_null_when_provider_handles_but_produces_no_items()
    {
        IRefContextMenuProvider provider = Substitute.For<IRefContextMenuProvider>();
        provider.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(true);

        RefContextMenuComposer composer = new([provider]);

        ContextMenuStrip? menu = composer.Build(gitRef: null, stashReflogSelector: null, _context);

        menu.Should().BeNull();
    }

    [Test]
    public void Build_should_return_menu_with_copy_item_when_provider_adds_items()
    {
        IRefContextMenuProvider provider = Substitute.For<IRefContextMenuProvider>();
        provider.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(true);
        provider.When(p => p.Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>()))
            .Do(ci => ci.Arg<ContextMenuStrip>().Items.Add(new ToolStripMenuItem("Test")));

        RefContextMenuComposer composer = new([provider]);

        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns("feature/test");

        ContextMenuStrip? menu = composer.Build(gitRef, stashReflogSelector: null, _context);

        menu.Should().NotBeNull();
        // Provider item + separator + copy item
        menu!.Items.Count.Should().Be(3);
        menu.Items[0].Text.Should().Be("Test");
        menu.Items[1].Should().BeOfType<ToolStripSeparator>();
        menu.Items[2].Text.Should().Contain("y name to clipboard");

        menu.Dispose();
    }

    [Test]
    public void Build_should_use_first_matching_provider()
    {
        IRefContextMenuProvider first = Substitute.For<IRefContextMenuProvider>();
        first.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(true);
        first.When(p => p.Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>()))
            .Do(ci => ci.Arg<ContextMenuStrip>().Items.Add(new ToolStripMenuItem("First")));

        IRefContextMenuProvider second = Substitute.For<IRefContextMenuProvider>();
        second.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(true);
        second.When(p => p.Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>()))
            .Do(ci => ci.Arg<ContextMenuStrip>().Items.Add(new ToolStripMenuItem("Second")));

        RefContextMenuComposer composer = new([first, second]);

        ContextMenuStrip? menu = composer.Build(gitRef: null, stashReflogSelector: "stash@{0}", _context);

        menu.Should().NotBeNull();
        menu!.Items[0].Text.Should().Be("First");
        second.DidNotReceive().Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>());

        menu.Dispose();
    }

    [Test]
    public void Build_should_use_stashReflogSelector_for_copy_when_gitRef_is_null()
    {
        IRefContextMenuProvider provider = Substitute.For<IRefContextMenuProvider>();
        provider.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(true);
        provider.When(p => p.Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>()))
            .Do(ci => ci.Arg<ContextMenuStrip>().Items.Add(new ToolStripMenuItem("Item")));

        RefContextMenuComposer composer = new([provider]);

        ContextMenuStrip? menu = composer.Build(gitRef: null, stashReflogSelector: "stash@{0}", _context);

        menu.Should().NotBeNull();
        // Last item is the copy item
        menu!.Items[^1].Should().BeOfType<ToolStripMenuItem>();

        menu.Dispose();
    }

    [Test]
    public void Build_should_skip_provider_that_does_not_handle()
    {
        IRefContextMenuProvider nonHandler = Substitute.For<IRefContextMenuProvider>();
        nonHandler.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(false);

        IRefContextMenuProvider handler = Substitute.For<IRefContextMenuProvider>();
        handler.Handles(Arg.Any<IGitRef?>(), Arg.Any<string?>()).Returns(true);
        handler.When(p => p.Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>()))
            .Do(ci => ci.Arg<ContextMenuStrip>().Items.Add(new ToolStripMenuItem("Handled")));

        RefContextMenuComposer composer = new([nonHandler, handler]);

        ContextMenuStrip? menu = composer.Build(gitRef: null, stashReflogSelector: "stash@{0}", _context);

        menu.Should().NotBeNull();
        nonHandler.DidNotReceive().Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>());
        handler.Received(1).Populate(Arg.Any<ContextMenuStrip>(), Arg.Any<IGitRef?>(), Arg.Any<string?>(), Arg.Any<RefContextMenuContext>());

        menu!.Dispose();
    }
}

using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.Graph;
public class RevisionGraphColumnTests
{
    private const int _rowHeight = 42;

    [Test]
    public void RenderGraphToCache_should_start_cache_with_single_line()
    {
        const int rowCount = 2;
        Setup(rowCount, out RevisionGraphColumnProvider.TestAccessor testAccessor);

        const int rowIndex = 1;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
        testAccessor.GraphCache.Capacity.Should().Be(3 * rowCount);
    }

    [Test]
    public void RenderGraphToCache_should_draw_from_cache()
    {
        Setup(rowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor);

        const int rowIndex = 1;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);

        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
    }

    [Test]
    public void RenderGraphToCache_should_fill_forward_then_restart()
    {
        Setup(rowCount: 10, visibleRowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor);

        int rowIndex = 0;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(1);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(2);

        rowIndex = testAccessor.GraphCache.HeadRow + (2 * (testAccessor.GraphCache.Capacity - 1)) + 1;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
    }

    [Test]
    public void RenderGraphToCache_should_scroll_forward()
    {
        Setup(rowCount: 10, visibleRowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor);

        int rowIndex = 0;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(1);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(2);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(3);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(4);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(5);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(6);

        ++rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(1);
        testAccessor.GraphCache.HeadRow.Should().Be(1);
        testAccessor.GraphCache.Count.Should().Be(6);

        rowIndex = testAccessor.GraphCache.HeadRow + (2 * (testAccessor.GraphCache.Capacity - 1));
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(1 - 1);
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex - (testAccessor.GraphCache.Capacity - 1));
        testAccessor.GraphCache.Count.Should().Be(6);
    }

    [Test]
    public void RenderGraphToCache_should_scroll_backward_then_restart()
    {
        const int rowCount = 10;
        Setup(rowCount, visibleRowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor);

        int rowIndex = rowCount - 1;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);

        --rowIndex;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(testAccessor.GraphCache.Capacity - 1);
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(2);

        rowIndex = testAccessor.GraphCache.HeadRow - testAccessor.GraphCache.Capacity;
        testAccessor.RenderRowToCache(rowIndex, _rowHeight);
        testAccessor.GraphCache.Head.Should().Be(testAccessor.GraphCache.Capacity - 1); // unchanged, does not really matter
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
    }

    [Test]
    public void SetHoverHighlight_should_include_tip_and_ancestors_for_matching_branch_group()
    {
        const string tipId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string parentId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        const string rootId = "cccccccccccccccccccccccccccccccccccccccc";
        const string otherTipId = "dddddddddddddddddddddddddddddddddddddddd";

        RevisionGraph revisionGraph = new();
        revisionGraph.Add(CreateRevision(
            tipId,
            [parentId],
            CreateBranchRef(localName: "main", isHead: true),
            CreateBranchRef(localName: "main", isHead: false, isRemote: true)));
        revisionGraph.Add(CreateRevision(otherTipId, [rootId], CreateBranchRef(localName: "feature", isHead: true)));
        revisionGraph.Add(CreateRevision(parentId, [rootId]));
        revisionGraph.Add(CreateRevision(rootId, []));

        RevisionGraphColumnProvider.TestAccessor testAccessor = CreateProvider(revisionGraph);

        testAccessor.SetHoverHighlight(new HashSet<string> { "main" });

        testAccessor.HoverHighlightedIds.Should().NotBeNull();
        testAccessor.HoverHighlightedIds.Should().BeEquivalentTo(
        [
            ObjectId.Parse(tipId),
            ObjectId.Parse(parentId),
            ObjectId.Parse(rootId),
        ]);
    }

    [Test]
    public void SetHoverHighlight_should_not_mark_cache_dirty_when_hover_selection_is_unchanged()
    {
        const string tipId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string parentId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        RevisionGraph revisionGraph = new();
        revisionGraph.Add(CreateRevision(tipId, [parentId], CreateBranchRef(localName: "main", isHead: true)));
        revisionGraph.Add(CreateRevision(parentId, []));

        RevisionGraphColumnProvider.TestAccessor testAccessor = CreateProvider(revisionGraph);
        VisibleRowRange range = new(fromIndex: 0, visibleRowCount: 2);
        testAccessor.RenderGraphToCache(range, toRowIndex: 1, _rowHeight);

        testAccessor.SetHoverHighlight(new HashSet<string> { "main" });
        testAccessor.IsHoverHighlightDirty.Should().BeTrue();

        testAccessor.RenderGraphToCache(range, toRowIndex: 1, _rowHeight);
        testAccessor.IsHoverHighlightDirty.Should().BeFalse();

        testAccessor.SetHoverHighlight(new HashSet<string> { "main" });
        testAccessor.IsHoverHighlightDirty.Should().BeFalse();
    }

    private static void Setup(int rowCount, out RevisionGraphColumnProvider.TestAccessor testAccessor)
        => Setup(rowCount, rowCount, out testAccessor);

    private static void Setup(int rowCount, int visibleRowCount, out RevisionGraphColumnProvider.TestAccessor testAccessor)
    {
        RevisionGraph revisionGraph = new();
        for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
        {
            revisionGraph.Add(new GitRevision(ObjectId.Random()));
        }

        revisionGraph.CacheTo(currentRowIndex: visibleRowCount, lastToCacheRowIndex: visibleRowCount);

        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder = Substitute.For<IGitRevisionSummaryBuilder>();

        RevisionGraphColumnProvider revisionGraphColumnProvider = new(revisionGraph, gitRevisionSummaryBuilder);
        testAccessor = revisionGraphColumnProvider.GetTestAccessor();

        testAccessor.GraphCache.Capacity.Should().Be(0);

        VisibleRowRange range = new(fromIndex: 0, visibleRowCount);
        testAccessor.RenderGraphToCache(range, rowCount - 1, _rowHeight);
        int expectedCapacity = 3 * visibleRowCount;
        testAccessor.GraphCache.Capacity.Should().Be(expectedCapacity);
        testAccessor.GraphCache.Count.Should().Be(Math.Min(rowCount, expectedCapacity));

        testAccessor.GraphCache.Reset();
        testAccessor.GraphCache.Capacity.Should().Be(expectedCapacity);
        testAccessor.GraphCache.Count.Should().Be(0);
    }

    private static IGitRef CreateBranchRef(string localName, bool isHead, bool isRemote = false)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.IsHead.Returns(isHead);
        gitRef.IsRemote.Returns(isRemote);
        gitRef.LocalName.Returns(localName);
        return gitRef;
    }

    private static RevisionGraphColumnProvider.TestAccessor CreateProvider(RevisionGraph revisionGraph)
    {
        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder = Substitute.For<IGitRevisionSummaryBuilder>();
        RevisionGraphColumnProvider revisionGraphColumnProvider = new(revisionGraph, gitRevisionSummaryBuilder);
        return revisionGraphColumnProvider.GetTestAccessor();
    }

    private static GitRevision CreateRevision(string id, IReadOnlyList<string> parentIds, params IGitRef[] refs)
    {
        List<ObjectId> parsedParentIds = [];
        foreach (string parentId in parentIds)
        {
            parsedParentIds.Add(ObjectId.Parse(parentId));
        }

        return new GitRevision(ObjectId.Parse(id))
        {
            ParentIds = parsedParentIds,
            Refs = refs,
        };
    }
}

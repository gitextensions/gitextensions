using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.Graph;

public class HoverHighlightComputerTests
{
    private const int _rowHeight = 42;

    [Test]
    public void SetHoverHighlight_should_include_tip_and_ancestors_within_visible_range()
    {
        const string tipId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string parentId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        const string rootId = "cccccccccccccccccccccccccccccccccccccccc";
        const string otherTipId = "dddddddddddddddddddddddddddddddddddddddd";

        RevisionGraph revisionGraph = new();
        IGitRef main = CreateBranchRef(localName: "main", isHead: true);
        revisionGraph.Add(CreateRevision(tipId, [parentId], main));
        revisionGraph.Add(CreateRevision(otherTipId, [rootId], CreateBranchRef(localName: "feature", isHead: true)));
        revisionGraph.Add(CreateRevision(parentId, [rootId]));
        revisionGraph.Add(CreateRevision(rootId, []));

        const int rowCount = 4;
        revisionGraph.CacheTo(rowCount - 1, rowCount - 1);

        RevisionGraphColumnProvider.TestAccessor testAccessor = CreateProvider(revisionGraph);
        VisibleRowRange range = new(fromIndex: 0, count: rowCount);
        testAccessor.RenderGraphToCache(range, toRowIndex: rowCount - 1, _rowHeight);

        testAccessor.HoverHighlight.SetHoverHighlight(main, rowIndex: GetRowForId(revisionGraph, rowCount, tipId));

        testAccessor.HoverHighlight.HighlightedIds.Should().BeEquivalentTo(
        [
            ObjectId.Parse(tipId),
            ObjectId.Parse(parentId),
            ObjectId.Parse(rootId),
        ]);
    }

    [Test]
    public void SetHoverHighlight_should_include_one_ancestor_outside_visible_range()
    {
        const string tipId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string parentId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        const string rootId = "cccccccccccccccccccccccccccccccccccccccc";

        RevisionGraph revisionGraph = new();
        IGitRef main = CreateBranchRef(localName: "main", isHead: true);
        revisionGraph.Add(CreateRevision(tipId, [parentId], main));
        revisionGraph.Add(CreateRevision(parentId, [rootId]));
        revisionGraph.Add(CreateRevision(rootId, []));

        const int rowCount = 3;
        revisionGraph.CacheTo(rowCount - 1, rowCount - 1);

        RevisionGraphColumnProvider.TestAccessor testAccessor = CreateProvider(revisionGraph);

        // Visible range covers only tip and parent; root is scrolled out of view.
        VisibleRowRange range = new(fromIndex: 0, count: 2);
        testAccessor.RenderGraphToCache(range, toRowIndex: 1, _rowHeight);

        testAccessor.HoverHighlight.SetHoverHighlight(main, rowIndex: GetRowForId(revisionGraph, rowCount, tipId));

        testAccessor.HoverHighlight.HighlightedIds.Should().BeEquivalentTo(
        [
            ObjectId.Parse(tipId),
            ObjectId.Parse(parentId),
            ObjectId.Parse(rootId),
        ]);
    }

    [Test]
    public void SetHoverHighlight_should_clear_highlighted_ids_when_gitRef_is_null()
    {
        const string tipId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string parentId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        RevisionGraph revisionGraph = new();
        IGitRef main = CreateBranchRef(localName: "main", isHead: true);
        revisionGraph.Add(CreateRevision(tipId, [parentId], main));
        revisionGraph.Add(CreateRevision(parentId, []));

        const int rowCount = 2;
        revisionGraph.CacheTo(rowCount - 1, rowCount - 1);

        RevisionGraphColumnProvider.TestAccessor testAccessor = CreateProvider(revisionGraph);
        VisibleRowRange range = new(fromIndex: 0, count: rowCount);
        testAccessor.RenderGraphToCache(range, toRowIndex: rowCount - 1, _rowHeight);

        testAccessor.HoverHighlight.SetHoverHighlight(main, rowIndex: GetRowForId(revisionGraph, rowCount, tipId));
        testAccessor.HoverHighlight.HighlightedIds.Should().NotBeNull();

        testAccessor.RenderGraphToCache(range, toRowIndex: rowCount - 1, _rowHeight);
        testAccessor.HoverHighlight.IsDirty.Should().BeFalse();

        testAccessor.HoverHighlight.SetHoverHighlight(gitRef: null);
        testAccessor.HoverHighlight.HighlightedIds.Should().BeNull();
        testAccessor.HoverHighlight.IsDirty.Should().BeTrue();
    }

    [Test]
    public void SetHoverHighlight_should_not_mark_cache_dirty_when_hover_selection_is_unchanged()
    {
        const string tipId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string parentId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        RevisionGraph revisionGraph = new();
        IGitRef main = CreateBranchRef(localName: "main", isHead: true);
        revisionGraph.Add(CreateRevision(tipId, [parentId], main));
        revisionGraph.Add(CreateRevision(parentId, []));

        const int rowCount = 2;
        revisionGraph.CacheTo(rowCount - 1, rowCount - 1);

        RevisionGraphColumnProvider.TestAccessor testAccessor = CreateProvider(revisionGraph);
        VisibleRowRange range = new(fromIndex: 0, count: rowCount);
        testAccessor.RenderGraphToCache(range, toRowIndex: rowCount - 1, _rowHeight);

        int tipRow = GetRowForId(revisionGraph, rowCount, tipId);

        testAccessor.HoverHighlight.SetHoverHighlight(main, tipRow);
        testAccessor.HoverHighlight.IsDirty.Should().BeTrue();

        testAccessor.RenderGraphToCache(range, toRowIndex: rowCount - 1, _rowHeight);
        testAccessor.HoverHighlight.IsDirty.Should().BeFalse();

        testAccessor.HoverHighlight.SetHoverHighlight(main, tipRow);
        testAccessor.HoverHighlight.IsDirty.Should().BeFalse();
    }

    private static int GetRowForId(RevisionGraph revisionGraph, int rowCount, string id)
    {
        ObjectId objectId = ObjectId.Parse(id);
        for (int row = 0; row < rowCount; row++)
        {
            if (revisionGraph.GetNodeForRow(row)?.Objectid == objectId)
            {
                return row;
            }
        }

        throw new InvalidOperationException($"No row found for revision {id}.");
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

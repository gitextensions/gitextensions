using System.Drawing.Imaging;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid.Graph;

[TestFixture]
public class RevisionGraphColumnTests
{
    private const int _rowHeight = 42;
    private const int _cellWidth = 1000;

    private static readonly Rectangle _paintRectangle = new(0, 0, _cellWidth, _rowHeight);

    [Test]
    public void PaintGraphCell_should_start_cache_with_single_line()
    {
        const int rowCount = 2;
        Setup(rowCount, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics);

        const int rowIndex = 1;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
        testAccessor.GraphCache.Capacity.Should().Be((2 * rowCount) + 1);
    }

    [Test]
    public void PaintGraphCell_should_draw_from_cache()
    {
        Setup(rowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics);

        const int rowIndex = 1;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);

        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
    }

    [Test]
    public void PaintGraphCell_should_fill_forward_then_restart()
    {
        Setup(rowCount: 10, visibleRowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics);

        int rowIndex = 0;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(1);

        ++rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(2);

        rowIndex = testAccessor.GraphCache.HeadRow + (2 * (testAccessor.GraphCache.Capacity - 1)) + 1;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
    }

    [Test]
    public void PaintGraphCell_should_scroll_forward()
    {
        Setup(rowCount: 10, visibleRowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics);

        int rowIndex = 0;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(1);

        ++rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(2);

        ++rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(3);

        ++rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(4);

        ++rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0);
        testAccessor.GraphCache.HeadRow.Should().Be(0);
        testAccessor.GraphCache.Count.Should().Be(5);

        ++rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(1);
        testAccessor.GraphCache.HeadRow.Should().Be(1);
        testAccessor.GraphCache.Count.Should().Be(5);

        rowIndex = testAccessor.GraphCache.HeadRow + (2 * (testAccessor.GraphCache.Capacity - 1));
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(1 - 1);
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex - (testAccessor.GraphCache.Capacity - 1));
        testAccessor.GraphCache.Count.Should().Be(5);
    }

    [Test]
    public void PaintGraphCell_should_scroll_backward_then_restart()
    {
        const int rowCount = 10;
        Setup(rowCount, visibleRowCount: 2, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics);

        int rowIndex = rowCount - 1;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(0); // not really mandatory, but Head is reset in Clear()
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);

        --rowIndex;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(testAccessor.GraphCache.Capacity - 1);
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(2);

        rowIndex = testAccessor.GraphCache.HeadRow - testAccessor.GraphCache.Capacity;
        testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
            .Should().BeTrue();
        testAccessor.GraphCache.Head.Should().Be(testAccessor.GraphCache.Capacity - 1); // unchanged, does not really matter
        testAccessor.GraphCache.HeadRow.Should().Be(rowIndex);
        testAccessor.GraphCache.Count.Should().Be(1);
    }

    private static void Setup(int rowCount, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics)
        => Setup(rowCount, rowCount, out testAccessor, out graphics);

    private static void Setup(int rowCount, int visibleRowCount, out RevisionGraphColumnProvider.TestAccessor testAccessor, out Graphics graphics)
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

        Bitmap destinationBitmap = new(width: _cellWidth, _rowHeight * rowCount, PixelFormat.Format32bppPArgb);
        graphics = Graphics.FromImage(destinationBitmap);
        testAccessor.GraphCache.Capacity.Should().Be(0);

        for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
        {
            testAccessor.PaintGraphCell(rowIndex, _rowHeight, _paintRectangle, graphics)
                .Should().BeTrue();
            testAccessor.GraphCache.Count.Should().Be(0);
        }

        revisionGraphColumnProvider.OnVisibleRowsChanged(new VisibleRowRange(fromIndex: 0, visibleRowCount));
        testAccessor.GraphCache.Capacity.Should().Be((2 * visibleRowCount) + 1);
    }
}

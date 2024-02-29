using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

internal sealed class GraphCache
{
    internal Bitmap? GraphBitmap { get; private set; }
    internal Graphics? GraphBitmapGraphics { get; private set; }

    /// <summary>
    /// The 'slot' that is the head of the circular bitmap.
    /// </summary>
    internal int Head { get; set; }

    /// <summary>
    /// The node row that is in the head slot.
    /// </summary>
    internal int HeadRow { get; set; }

    /// <summary>
    /// Number of elements in the cache.
    /// </summary>
    internal int Count { get; set; }

    /// <summary>
    /// Number of elements allowed in the cache. Is based on control height.
    /// </summary>
    internal int Capacity { get; private set; }

    internal void AdjustCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        Capacity = capacity;
    }

    internal void Allocate(int width, int height)
    {
        if (GraphBitmap is not null && GraphBitmap.Width >= width && GraphBitmap.Height >= height)
        {
            return;
        }

        if (GraphBitmap is not null)
        {
            GraphBitmap.Dispose();
            GraphBitmap = null;
        }

        if (GraphBitmapGraphics is not null)
        {
            GraphBitmapGraphics.Dispose();
            GraphBitmapGraphics = null;
        }

        GraphBitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        GraphBitmapGraphics = Graphics.FromImage(GraphBitmap);
        GraphBitmapGraphics.SmoothingMode = SmoothingMode.AntiAlias;

        // With SmoothingMode != None it is better to use PixelOffsetMode.HighQuality
        // e.g. to avoid shrinking rectangles, ellipses and etc. by 1 px from right bottom
        GraphBitmapGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        Reset();
    }

    internal void CopyFrom(GraphCache source)
    {
        Capacity = source.Capacity;
        Bitmap sourceBitmap = source.GraphBitmap;
        Allocate(sourceBitmap.Width, sourceBitmap.Height);
        GraphBitmapGraphics.CompositingMode = CompositingMode.SourceCopy;
        GraphBitmapGraphics.DrawImage(sourceBitmap, 0, 0);
        Count = source.Count;
        Head = source.Head;
        HeadRow = source.HeadRow;
    }

    /// <summary>
    /// Maps a graph row to a cache row.
    /// </summary>
    /// <param name="rowIndex">The row index in the entire graph.</param>
    /// <returns>The row index in the cache.</returns>
    internal int GetCacheRow(int rowIndex) => (Head + rowIndex - HeadRow) % Capacity;

    internal void Reset()
    {
        Head = 0;
        Count = 0;
    }
}

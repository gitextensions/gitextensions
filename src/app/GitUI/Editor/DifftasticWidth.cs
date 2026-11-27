using System.Windows.Forms;

namespace GitUI.Editor;

/// <summary>
///  Computes the difftastic output width (in columns) from the available viewer width and the actual font metrics.
/// </summary>
/// <remarks>
///  The side-by-side diff is rendered at a fixed number of columns (<c>DFT_WIDTH</c>). Guessing that
///  number from a hard-coded character width (7 px) overestimates the column count for fonts whose
///  cell is wider than 7 px (e.g. Courier New 9 pt), which makes the rendered diff too wide. Measuring
///  the cell width from the actual font and clamping to a sane range fixes that.
/// </remarks>
internal static class DifftasticWidth
{
    private const int MinColumns = 88;
    private const int MaxColumns = 200;

    // A long monospace sample so the integer measurement error is negligible after division.
    private static readonly string MonospaceSample = new('0', 100);

    /// <summary>
    ///  Computes an even column count that fits <paramref name="availableWidthPx"/> given the character width.
    /// </summary>
    /// <param name="availableWidthPx">The available width in pixels.</param>
    /// <param name="charWidthPx">The width of a single character in pixels (from the actual font).</param>
    /// <returns>A column count clamped to [88, 200] and rounded down to an even number.</returns>
    public static int ComputeSideBySideWidth(int availableWidthPx, float charWidthPx)
    {
        float columns = charWidthPx > 0 ? availableWidthPx / charWidthPx : MinColumns;
        return Math.Clamp((int)columns, MinColumns, MaxColumns) / 2 * 2;
    }

    /// <summary>
    ///  Measures the monospace cell width (in pixels) of <paramref name="font"/>.
    /// </summary>
    /// <param name="font">The monospace font to measure.</param>
    /// <returns>The width of a single character in pixels, or 0 if the font is null.</returns>
    public static float MeasureCharWidth(Font? font)
    {
        if (font is null)
        {
            return 0f;
        }

        // A monospace font has a constant advance width; measuring a long run of identical characters
        // and dividing yields the cell width reliably (a single glyph's bounding box can be narrower
        // than its advance width).
        return TextRenderer.MeasureText(MonospaceSample, font).Width / (float)MonospaceSample.Length;
    }
}

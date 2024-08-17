using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class AnsiEscapeUtilities
{
    [GeneratedRegex(@"\u001b\[((?<escNo>\d+)\s*[:;]?\s*)*m", RegexOptions.ExplicitCapture)]
    private static partial Regex EscapeRegex();

    private const int _boldOffset = 8;

    /// <summary>
    /// Debug print colors similar to https://github.com/robertknight/konsole/raw/master/tests/color-spaces.pl
    /// </summary>
    public static void PrintColors(StringBuilder sb, List<TextMarker> textMarkers)
    {
        int currentColorId = 0;
        sb.Append('\n');

        // color id (standard) colors
        foreach (bool fore in new List<bool>() { true, false })
        {
            foreach (int offset in new List<int>() { 0, _boldOffset })
            {
                foreach (int dim in new List<int>() { 0, 2 })
                {
                    if (dim == 0)
                    {
                        sb.Append($"{offset:d1} ");
                    }
                    else
                    {
                        sb.Append($"{" ",2}");
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        sb.Append("@!");
                        int colorId = i + offset;
                        TryGetColorsFromEscapeSequence(new List<int>() { 0, dim, 38 + (fore ? 0 : 10), 5, colorId }, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: false);
                        if (TryGetTextMarker(new()
                        {
                            DocOffset = sb.Length - 2,
                            Length = 2,
                            BackColor = backColor,
                            ForeColor = foreColor
                        },
                                out TextMarker tm))
                        {
                            textMarkers.Add(tm);
                        }
                    }

                    sb.Append('\n');
                }
            }

            sb.Append('\n');
        }
    }

    /// <summary>
    /// Parse the text and convert ansi console escape sequences to highlight info applicable to the FileViewer.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="sb">StringBuilder to appened the text with with the escape sequences removed.</param>
    /// <param name="textMarkers">Detected and current started highlight info for the document.</param>
    public static void ParseEscape(string text, StringBuilder sb, List<TextMarker> textMarkers, bool themeColors = false, bool traceErrors = true)
    {
        int errorCount = 0;
        int prevLineOffset = 0;
        int currentColorId = 0; // current color, used when just bold etc is set
        HighlightInfo currentHighlight = new()
        {
            DocOffset = sb.Length,
            Length = -1,
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        for (Match match = EscapeRegex().Match(text); match.Success; match = match.NextMatch())
        {
            sb.Append(text[prevLineOffset..match.Index]);
            prevLineOffset = match.Index + match.Length;

            // An escape sequence can include several attributes (empty/unparsable is break).
            List<int> escapeCodes = match.Groups["escNo"].Captures.Select(i => int.TryParse(i.ToString(), out int attribute) ? attribute : 0).ToList();

            if (TryGetColorsFromEscapeSequence(escapeCodes, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors))
            {
                if (currentHighlight.Length >= 0)
                {
                    // End current match so new can be started, this is expected but normally not used by Git (?).
                    // Also assume that the new colors are "complete", not just overlay.
                    ++errorCount;
                    Trace.WriteLineIf(traceErrors && errorCount <= 3, $"New start marker without end for grep ({sb.Length}, {match.Index}){(errorCount == 1 ? " in:\n" + text : "")}");

                    currentHighlight.Length = sb.Length - currentHighlight.DocOffset;
                    if (TryGetTextMarker(currentHighlight, out TextMarker tm))
                    {
                        textMarkers.Add(tm);
                    }
                }

                // Start of new segment
                currentHighlight.DocOffset = sb.Length;
                currentHighlight.Length = 0;
                currentHighlight.BackColor = backColor;
                currentHighlight.ForeColor = foreColor;
            }
            else
            {
                EndCurrentHighlight();
            }
        }

        sb.Append(text.AsSpan(prevLineOffset));
        EndCurrentHighlight();

        return;

        void EndCurrentHighlight()
        {
            // Reset escape sequence, end of segment

            if (currentHighlight.Length < 0)
            {
                // Previous was a reset, just ignore.
                return;
            }

            if (currentHighlight.Length > 0)
            {
                // This could be escapeCodes setting without any effect,
                // Git also ends "configured with empty" (like diff context set in GE) without start with end segment.
                Debug.WriteLineIf(sb.Length < 256, $"Debug: Unexpected no ongoing marker at {sb.Length}.");
                return;
            }

            currentHighlight.Length = sb.Length - currentHighlight.DocOffset;
            if (TryGetTextMarker(currentHighlight, out TextMarker tm))
            {
                textMarkers.Add(tm);
            }

            currentHighlight.Length = -1;
        }
    }

    /// <summary>
    /// Try to get the back/forecolor from the array of ANSI escape sequences defined in
    /// https://en.wikipedia.org/wiki/ANSI_escape_code#Colors
    /// </summary>
    /// <param name="escapeCodes">Array of escape codes to interpret.</param>
    /// <param name="backColor">Background color if set.</param>
    /// <param name="foreColor">Foreground color if set.</param>
    /// <param name="currentColorId">The fore color ANSI id (not argb color) if it was set. Can be used in follow-up sequences.</param>
    /// <returns><see langword="true"/> if a color was set; otherwise <see langword="false"/>.</returns>
    private static bool TryGetColorsFromEscapeSequence(IList<int> escapeCodes, out Color? backColor, out Color? foreColor, ref int currentColorId, bool themeColors)
    {
        bool result = false;
        backColor = null;
        foreColor = null;

        // A subset of attributes supported, most are ignored, other handled as bold/dim
        bool reverse = false; // swap fore/back
        bool fore = true; // fore color is first in list

        (int id, bool bold, bool dim) initCurrent = (-1, false, false);
        (int id, bool bold, bool dim) currentFore = initCurrent;
        (int id, bool bold, bool dim) currentBack = initCurrent;
        for (int i = 0; i < escapeCodes.Count; ++i)
        {
            switch (escapeCodes[i])
            {
                case 0: // Reset or normal, all attributes become turned off
                    result = false;
                    reverse = false;
                    backColor = null;
                    foreColor = null;
                    currentColorId = 0; // default black
                    currentFore = initCurrent;
                    currentBack = initCurrent;
                    break;
                case 1: // bold
                case 4: // underline/ul
                case 5: // slow blink
                case 6: // fast blink
                case 9: // strike
                    if (fore)
                    {
                        currentFore.bold = true;
                        if (currentFore.id < 0 && foreColor is null)
                        {
                            // Something is set for foreground, use default color
                            currentFore.id = currentColorId;
                        }
                    }
                    else
                    {
                        currentBack.bold = true;
                    }

                    break;
                case 2: // dim
                case 3: // italic
                case 8: // conceal
                    if (fore)
                    {
                        currentFore.dim = true;
                        if (currentFore.id < 0)
                        {
                            // Something is set for foreground, use default color
                            currentFore.id = currentColorId;
                        }
                    }
                    else
                    {
                        currentBack.dim = true;
                    }

                    break;
                case 7: // reverse
                    reverse = true;
                    break;
                case 39: // Default foreground color
                    foreColor = null;
                    currentFore.id = currentColorId = 0;
                    break;
                case 49: // Default background color
                    backColor = null;
                    currentBack.id = 7; // White theme
                    break;
                case >= 30 and <= 37: // Set foreground color
                case >= 90 and <= 97: // Set bold foreground color
                    fore = true;
                    currentFore.bold = currentFore.bold || escapeCodes[i] >= 90;
                    currentColorId = escapeCodes[i] - (escapeCodes[i] >= 90 ? 90 : 30);
                    currentFore.id = currentColorId;

                    break;
                case >= 40 and <= 47: // Set background color
                case >= 100 and <= 107: // Set bold background color
                    fore = false;
                    currentBack.bold = currentBack.bold || escapeCodes[i] >= 100;
                    currentBack.id = escapeCodes[i] - (escapeCodes[i] >= 100 ? 100 : 40);
                    break;
                case 38: // Set foreground color with sequence
                case 48: // Set background color with sequence
                    if (i >= escapeCodes.Count - 2)
                    {
                        Trace.WriteLine($"Unexpected too few arguments for {i} {escapeCodes}");
                        DebugHelpers.Fail($"Unexpected too few arguments for {i} {escapeCodes}");
                        break;
                    }

                    Color color;
                    fore = escapeCodes[i] == 38;
                    ++i;

                    if (escapeCodes[i] == 5)
                    {
                        // ESC[38:5:⟨n⟩m Select foreground color
                        ++i;
                        bool bold = escapeCodes[i] is >= _boldOffset and < 2 * _boldOffset;
                        int id = escapeCodes[i] - (bold ? _boldOffset : 0);
                        if (fore)
                        {
                            currentFore.bold = bold;
                            currentFore.id = id;
                        }
                        else
                        {
                            currentBack.bold = bold;
                            currentBack.id = id;
                        }
                    }
                    else if (escapeCodes[i] == 2)
                    {
                        // ESC[38;2;⟨r⟩;⟨g⟩;⟨b⟩ m Select RGB foreground color
                        if (i >= escapeCodes.Count - 3)
                        {
                            Trace.WriteLine($"Unexpected too few arguments for {i} {escapeCodes}");
                            DebugHelpers.Fail($"Unexpected too few arguments for {i} {escapeCodes}");
                            break;
                        }

                        color = Color.FromArgb(escapeCodes[i + 1], escapeCodes[i + 2], escapeCodes[i + 3]);
                        i += 3;

                        // Unknown fixed identifier, reset id
                        // Reset also for background to avoid CS0165
                        currentColorId = 0;

                        // Set the color, override if set later
                        if (fore)
                        {
                            currentFore.id = -1;
                            foreColor = color;
                        }
                        else
                        {
                            currentBack.id = -1;
                            backColor = color;
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"Unexpected argument for {i} {escapeCodes}");
                        DebugHelpers.Fail($"Unexpected argument for {i} {escapeCodes}");
                        break;
                    }

                    break;
                default: // Ignore unhandled sequences
                    break;
            }
        }

        if (themeColors && !reverse
            && (currentBack.id < 0 || backColor is null)
            && foreColor is null
            && currentFore.id is 1 or 2 && !currentFore.dim)
        {
            // Assume this is a fit for the theme colors with reverse color (e.g. difftastic)
            reverse = true;
        }

        if (reverse)
        {
            (backColor, foreColor) = (foreColor, backColor);
            (currentBack, currentFore) = (currentFore, currentBack);
        }

        if (currentFore.id >= 0)
        {
            foreColor = Get8bitColor(currentFore.id + (currentFore.bold ? _boldOffset : 0), fore: true, currentFore.dim, out currentColorId);
        }

        if (currentBack.id >= 0)
        {
            backColor = Get8bitColor(currentBack.id + (currentBack.bold ? _boldOffset : 0), fore: false, currentBack.dim, out _);
        }

        if (backColor is not null && foreColor is null)
        {
            foreColor = ColorHelper.GetForeColorForBackColor((Color)backColor);
        }

        // Set result if there are changes
        // No action if there are only attribute changes like bold/dim/reverse
        // (not generated by Git but could be set by the user)
        if (backColor is not null || foreColor is not null)
        {
            result = true;
        }

        return result;
    }

    /// <summary>
    /// Decode 3, 4, and 8-bit colors from https://en.wikipedia.org/wiki/ANSI_escape_code#24-bit
    /// The named 3/4 bit colors for the invariant theme is from the "theme" at
    /// https://github.com/mintty/mintty/blob/master/themes/helmholtz
    /// </summary>
    /// <param name="colorCode">The color code to decode.</param>
    /// <param name="colorId">ANSI color id if known, otherwise default black.</param>
    /// <returns>The 24-bit RGB color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Unexpected value.</exception>
    private static Color Get8bitColor(int colorCode, bool fore, bool dim, out int colorId)
    {
        // Color code definitions
        const int blackId = 0;
        const int redId = 1;
        const int greenId = 2;
        const int yellowId = 3;
        const int blueId = 4;
        const int magentaId = 5;
        const int cyanId = 6;
        const int whiteId = 7;

        if (colorCode is >= blackId and <= whiteId
            or >= blackId + _boldOffset and <= whiteId + _boldOffset)
        {
            // Mask bold, last three bits
            colorId = colorCode & 7;
        }
        else
        {
            // Reset to default (black)
            colorId = 0;
        }

        Color color = colorCode switch
        {
            blackId => fore
                ? AppColor.AnsiTerminalBlackForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalBlackBackNormal.GetThemeColor(),
            blackId + _boldOffset => fore
                ? AppColor.AnsiTerminalBlackForeBold.GetThemeColor()
                : AppColor.AnsiTerminalBlackBackBold.GetThemeColor(),

            redId => fore
                ? AppColor.AnsiTerminalRedForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalRedBackNormal.GetThemeColor(),
            redId + _boldOffset => fore
                ? AppColor.AnsiTerminalRedForeBold.GetThemeColor()
                : AppColor.AnsiTerminalRedBackBold.GetThemeColor(),

            greenId => fore
                ? AppColor.AnsiTerminalGreenForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalGreenBackNormal.GetThemeColor(),
            greenId + _boldOffset => fore
                ? AppColor.AnsiTerminalGreenForeBold.GetThemeColor()
                : AppColor.AnsiTerminalGreenBackBold.GetThemeColor(),

            yellowId => fore
                ? AppColor.AnsiTerminalYellowForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalYellowBackNormal.GetThemeColor(),
            yellowId + _boldOffset => fore
                ? AppColor.AnsiTerminalYellowForeBold.GetThemeColor()
                : AppColor.AnsiTerminalYellowBackBold.GetThemeColor(),

            blueId => fore
                ? AppColor.AnsiTerminalBlueForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalBlueBackNormal.GetThemeColor(),
            blueId + _boldOffset => fore
                ? AppColor.AnsiTerminalBlueForeBold.GetThemeColor()
                : AppColor.AnsiTerminalBlueBackBold.GetThemeColor(),

            magentaId => fore
                ? AppColor.AnsiTerminalMagentaForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalMagentaBackNormal.GetThemeColor(),
            magentaId + _boldOffset => fore
                ? AppColor.AnsiTerminalMagentaForeBold.GetThemeColor()
                : AppColor.AnsiTerminalMagentaBackBold.GetThemeColor(),

            cyanId => fore
                ? AppColor.AnsiTerminalCyanForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalCyanBackNormal.GetThemeColor(),
            cyanId + _boldOffset => fore
                ? AppColor.AnsiTerminalCyanForeBold.GetThemeColor()
                : AppColor.AnsiTerminalCyanBackBold.GetThemeColor(),

            whiteId => fore
                ? AppColor.AnsiTerminalWhiteForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalWhiteBackNormal.GetThemeColor(),
            whiteId + _boldOffset => fore
                ? AppColor.AnsiTerminalWhiteForeBold.GetThemeColor()
                : AppColor.AnsiTerminalWhiteBackBold.GetThemeColor(),

            >= 16 and < 232 => Get216Colors(colorCode),
            >= 232 and <= 255 => Get24StepGray(colorCode),
            _ => throw new ArgumentOutOfRangeException(nameof(colorCode), colorCode, $"Unexpected value for ANSI color.")
        };

        if (dim)
        {
            color = DimColor(color);
        }

        return color;

        static Color Get216Colors(int level)
        {
            int i = level - 16; // 2 * _boldOffset
            int blue = Get8bitFrom6over3bit(i % 6);
            int green = Get8bitFrom6over3bit((i % 36) / 6);
            int red = Get8bitFrom6over3bit(i / 36);

            return Color.FromArgb(red, green, blue);

            // Convert 0-5 to 0-255
            static int Get8bitFrom6over3bit(int color)
                => color * 51;
        }

        static Color Get24StepGray(int level)
        {
            // Convert 0-23 to 0-253
            int i = (level - 232) * 11;
            return Color.FromArgb(i, i, i);
        }
    }

    private static Color DimColor(Color color)
    {
        // Blend the color with the background, halve each value first
        // Note: With themes, defaultBackground must be dynamic
        const uint defaultBackground = 0xff_ffff;
        int dimCode = (int)(((color.ToArgb() & 0xFEFEFEFE) >> 1) + ((defaultBackground & 0xFEFEFEFE) >> 1));
        return Color.FromArgb((dimCode >> 16) & 0xff, (dimCode >> 8) & 0xff, dimCode & 0xff);
    }

    /// <summary>
    /// Add pre-parsed highlight info (parsed ANSI escape sequences) as markers in the document.
    /// </summary>
    /// <param name="hl">Info to set.</param>
    public static bool TryGetTextMarker(HighlightInfo hl, out TextMarker? textMarker)
    {
        if (hl.DocOffset < 0 || hl.Length < 0)
        {
            Trace.WriteLine($"Unexpected no docOffset or backColor ({hl})");
            DebugHelpers.Fail($"Unexpected no docOffset or backColor ({hl})");
            textMarker = null;
            return false;
        }

        if (hl.BackColor is null)
        {
            // BackColor must always be set
            // TODO get default back color, guess if the user has not set the value
            hl.BackColor = Color.White; // document.LineSegmentCollection.FirstOrDefault().GetColorForPosition(0).BackgroundColor;
        }

        textMarker = hl.ForeColor is null
            ? new TextMarker(hl.DocOffset, hl.Length, TextMarkerType.SolidBlock, (Color)hl.BackColor)
            : new TextMarker(hl.DocOffset, hl.Length, TextMarkerType.SolidBlock, (Color)hl.BackColor, (Color)hl.ForeColor);
        return true;
    }

    internal readonly struct TestAccessor
    {
        public static bool TryGetColorsFromEscapeSequence(IList<int> escapeCodes, out Color? backColor, out Color? foreColor, ref int currentColorId)
            => AnsiEscapeUtilities.TryGetColorsFromEscapeSequence(escapeCodes, out backColor, out foreColor, ref currentColorId, themeColors: false);

        public static Color Get8bitColor(int colorCode, bool fore, bool dim, out int colorId)
            => AnsiEscapeUtilities.Get8bitColor(colorCode, fore, dim, out colorId);

        public static int GetBoldOffset() => _boldOffset;
    }
}

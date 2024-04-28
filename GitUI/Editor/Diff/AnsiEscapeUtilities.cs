using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class AnsiEscapeUtilities
{
    [GeneratedRegex(@"\u001b\[((?<escNo>\d+)\s*[:;]?\s*)*m", RegexOptions.ExplicitCapture)]
    private static partial Regex EscapeRegex();

    // Offset for faint/dim is escapeCodes workaround to handle dim in Get8bitColor()
    private const int _dimOffset = 0x0001_0000;
    private const int _boldOffset = 8;

    /// <summary>
    /// Override Git colors from GE theme color.
    /// </summary>
    /// <param name="key">The Git config key: https://git-scm.com/docs/git-config#Documentation/git-config.txt-colordiffltslotgt.</param>
    /// <param name="appBackColor">The back color to add, fore color to be derived from it.</param>
    /// <returns>The GitConfigItem.</returns>
    public static GitConfigItem SetUnsetGitColor(
                    string key,
                    AppColor appBackColor)
    {
        // Override the Git default coloring (mask the alpha for Git)
        Color backColor = appBackColor.GetThemeColor();
        int back = backColor.ToArgb() & 0xffffff;
        Color foreColor = ColorHelper.GetForeColorForBackColor(backColor);
        int fore = foreColor.ToArgb() & 0xffffff;

        return new GitConfigItem(key, $"#{fore:x6} #{back:x6}");
    }

#if DEBUG
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
            foreach (int offset in new List<int>() { 0, _boldOffset, _dimOffset })
            {
                sb.Append($"{offset:d5} ");
                for (int i = 0; i < 8; i++)
                {
                    sb.Append("@!");
                    int colorId = i + offset;
                    TryGetColorsFromEscapeSequence(new List<int>() { 38 + (fore ? 0 : 10), 5, colorId }, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: false);
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

            sb.Append('\n');
        }

        // GE theme colors - primarily used as background color with adjusted foreground
        foreach (List<Color?> cs in new List<List<Color?>>()
            {
            new() { null, AppColor.DiffRemoved.GetThemeColor(), AppColor.DiffAdded.GetThemeColor(), null, null, null, null, AppColor.DiffSection.GetThemeColor(), },
            new() { null, AppColor.DiffRemovedExtra.GetThemeColor(), AppColor.DiffAddedExtra.GetThemeColor(), }
            })
        {
            sb.Append($"{" ",6}");
            foreach (Color? color in cs)
            {
                sb.Append(color is null ? "  " : "@!");
                if (color is not null)
                {
                    if (TryGetTextMarker(new()
                    {
                        DocOffset = sb.Length - 2,
                        Length = 2,
                        BackColor = color,
                        ForeColor = ColorHelper.GetForeColorForBackColor((Color)color)
                    },
                        out TextMarker tm))
                    {
                        textMarkers.Add(tm);
                    }
                }
            }

            sb.Append('\n');
        }
    }
#endif

    /// <summary>
    /// Parse the text and convert ansi console escape sequences to highlight info applicable to the FileViewer.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="sb">StringBuilder to appened the text with with the escape sequences removed.</param>
    /// <param name="textMarkers">Detected and current started highlight info for the document.</param>
    public static void ParseEscape(string text, StringBuilder sb, List<TextMarker> textMarkers, bool themeColors = false)
    {
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
                    Trace.WriteLine($"New start marker without end for grep ({sb.Length},{match.Index}) {text}");

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
        bool bold = false; // Git bright, increased intensity
        bool dim = false; // faint, decreased intensity
        bool reverse = false; // swap isFore/back
        for (int i = 0; i < escapeCodes.Count; ++i)
        {
            switch (escapeCodes[i])
            {
                case 0: // Reset or normal, all attributes become turned off
                    result = false;
                    bold = false;
                    dim = false;
                    reverse = false;
                    backColor = null;
                    foreColor = null;
                    currentColorId = 0; // default black
                    break;
                case 1: // bold
                case 4: // underline/ul
                case 5: // slow blink
                case 6: // fast blink
                case 9: // strike
                    bold = true;
                    foreColor = Get8bitColor(currentColorId + GetBoldDimOffset(), out _);
                    break;
                case 2: // dim
                case 3: // italic
                    dim = true;
                    foreColor = Get8bitColor(currentColorId + GetBoldDimOffset(), out _);
                    break;
                case 7: // reverse
                    reverse = true;
                    break;
                case 39: // Default foreground color
                    foreColor = null;
                    currentColorId = 0;
                    break;
                case 49: // Default background color
                    backColor = null;
                    break;
                case >= 30 and <= 37: // Set foreground color
                case >= 90 and <= 97: // Set bright foreground color
                    bold = bold || escapeCodes[i] >= 90;
                    currentColorId = escapeCodes[i] - (escapeCodes[i] >= 90 ? 90 : 30);
                    if (themeColors && currentColorId is 1 or 2 && !dim && !reverse && backColor is null)
                    {
                        // Assume this is a fit for the theme colors with reverse color
                        if (currentColorId == 1)
                        {
                            backColor = (bold ? AppColor.DiffRemovedExtra : AppColor.DiffRemoved).GetThemeColor();
                        }
                        else
                        {
                            backColor = (bold ? AppColor.DiffAddedExtra : AppColor.DiffAdded).GetThemeColor();
                        }
                    }
                    else
                    {
                        foreColor = Get8bitColor(currentColorId + GetBoldDimOffset(), out _);
                    }

                    break;
                case >= 40 and <= 47: // Set background color
                case >= 100 and <= 107: // Set bright background color
                    bold = bold || escapeCodes[i] >= 100;
                    int backColorId = escapeCodes[i] - (escapeCodes[i] >= 100 ? 100 : 40);
                    backColor = Get8bitColor(backColorId + GetBoldDimOffset(), out _);
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
                    bool isFore = escapeCodes[i] == 38;
                    ++i;

                    if (escapeCodes[i] == 5)
                    {
                        // ESC[38:5:⟨n⟩m Select foreground color
                        ++i;
                        bold = escapeCodes[i] is >= _boldOffset and < 2 * _boldOffset;
                        color = Get8bitColor(escapeCodes[i], out currentColorId);
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
                    }
                    else
                    {
                        Trace.WriteLine($"Unexpected argument for {i} {escapeCodes}");
                        DebugHelpers.Fail($"Unexpected argument for {i} {escapeCodes}");
                        break;
                    }

                    if (isFore)
                    {
                        foreColor = color;
                    }
                    else
                    {
                        backColor = color;
                    }

                    break;
                default: // Ignore unhandled sequences
                    break;
            }
        }

        if (reverse)
        {
            (backColor, foreColor) = (foreColor, backColor);
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

        int GetBoldDimOffset()
        {
            if (dim)
            {
                // bold has no effect when dim
                return (int)_dimOffset;
            }

            return bold ? (int)_boldOffset : 0;
        }
    }

    /// <summary>
    /// Decode 3, 4, and 8-bit colors from https://en.wikipedia.org/wiki/ANSI_escape_code#24-bit
    /// Named 3/4 bit colors from the "theme" at https://github.com/mintty/mintty/blob/master/themes/helmholtz (could be integrated with GE colors)
    /// (Dimmed/faint colors are not in the theme, extracted printing to terminal.)
    /// Note: A difference from at least mintty handles sequences like "30;5;1" differently from "31",
    /// as well as background and foreground differs occasionally.
    /// </summary>
    /// <param name="colorCode">The color code to decode.</param>
    /// <param name="colorId">ANSI color id if known, otherwise default black.</param>
    /// <returns>The 24-bit RGB color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Unexpected value.</exception>
    private static Color Get8bitColor(int colorCode, out int colorId)
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
            or >= blackId + _boldOffset and <= whiteId + _boldOffset
            or >= blackId + _dimOffset and <= whiteId + _dimOffset)
        {
            // Mask bold or dim, last three bits
            colorId = colorCode & 7;
        }
        else
        {
            // Reset to default (black)
            colorId = 0;
        }

        return colorCode switch
        {
            blackId => Color.Black,
            blackId + _boldOffset => Color.FromArgb(96, 96, 96),
            blackId + _dimOffset => Color.FromArgb(127, 127, 127),

            redId => Color.FromArgb(212, 44, 58),
            redId + _boldOffset => Color.FromArgb(255, 118, 118),
            redId + _dimOffset => Color.FromArgb(208, 142, 147),

            greenId => Color.FromArgb(137, 190, 127),
            greenId + _boldOffset => Color.FromArgb(0, 242, 0),
            greenId + _dimOffset => Color.FromArgb(137, 190, 127),

            yellowId => Color.FromArgb(192, 160, 0),
            yellowId + _boldOffset => Color.FromArgb(242, 242, 0),
            yellowId + _dimOffset => Color.FromArgb(199, 187, 127),

            blueId => Color.FromArgb(0, 93, 255),
            blueId + _boldOffset => Color.FromArgb(125, 151, 255),
            blueId + _dimOffset => Color.FromArgb(127, 143, 233),

            magentaId => Color.FromArgb(177, 72, 198),
            magentaId + _boldOffset => Color.FromArgb(255, 112, 255),
            magentaId + _dimOffset => Color.FromArgb(194, 154, 202),

            cyanId => Color.FromArgb(0, 168, 154),
            cyanId + _boldOffset => Color.FromArgb(0, 240, 240),
            cyanId + _dimOffset => Color.FromArgb(127, 190, 184),

            whiteId => Color.FromArgb(191, 191, 191),
            whiteId + _boldOffset => Color.FromArgb(255, 255, 255),
            whiteId + _dimOffset => Color.FromArgb(222, 222, 222),

            >= 16 and < 232 => Get216Colors(colorCode),
            >= 232 and <= 255 => Get24StepGray(colorCode),
            _ => throw new ArgumentOutOfRangeException(nameof(colorCode), colorCode, $"Unexpected value for ANSI color.")
        };

        static Color Get216Colors(int level)
        {
            int i = level - 16;
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

        public static Color Get8bitColor(int colorCode, out int colorId)
            => AnsiEscapeUtilities.Get8bitColor(colorCode, out colorId);

        public static int GetBoldOffset() => _boldOffset;
        public static int GetDimOffset() => _dimOffset;
    }
}

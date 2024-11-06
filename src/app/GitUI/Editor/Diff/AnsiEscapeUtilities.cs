using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class AnsiEscapeUtilities
{
    [GeneratedRegex(@"\u001b\[((?<escNo>\d+)\s*[:;]?\s*)*m", RegexOptions.ExplicitCapture)]
    private static partial Regex EscapeRegex();
    private static readonly List<bool> _fores = [true, false];
    private static readonly List<bool> _bolds = [false, true];

    // Color code definitions
    private const int _blackId = 0;
    private const int _redId = 1;
    private const int _greenId = 2;
    private const int _yellowId = 3;
    private const int _blueId = 4;
    private const int _magentaId = 5;
    private const int _cyanId = 6;
    private const int _whiteId = 7;

    private const int _boldOffset = 8;

    /// <summary>
    /// Debug print colors similar to https://github.com/robertknight/konsole/raw/master/tests/color-spaces.pl
    /// </summary>
    public static void PrintColors(StringBuilder sb, List<TextMarker> textMarkers)
    {
        int currentColorId = 0;
        sb.Append('\n');

        // color id (standard) colors
        foreach (bool fore in _fores)
        {
            foreach (bool bold in _bolds)
            {
                foreach (int dim in new List<int>() { 0, 2 })
                {
                    for (int i = _blackId; i <= _whiteId; i++)
                    {
                        sb.Append("@!");
                        TryGetColorsFromEscapeSequence(new List<int>() { 0, dim, i + 30 + (fore ? 0 : 10) + (bold ? 60 : 0) }, out Color? backColor, out Color? foreColor, ref currentColorId, themeColors: false);
                        if (TryGetTextMarker(new()
                                {
                                    DocOffset = sb.Length - 2,
                                    Length = 2,
                                    BackColor = backColor,
                                    ForeColor = foreColor
                                },
                                prevMarker: null,
                                sb,
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
            BackColor = SystemColors.Window,
            ForeColor = SystemColors.WindowText,
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
                    Trace.WriteLineIf(traceErrors && errorCount <= 3, $"New start marker without explicit end ({sb.Length}, {match.Index}){(errorCount == 1 ? " in:\n" + text : "")}");
                    EndCurrentHighlight();
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

            if (currentHighlight.Length < 0 || sb.Length == currentHighlight.DocOffset)
            {
                // Previous was a reset, just ignore.
                currentHighlight.Length = -1;
                return;
            }

            if (currentHighlight.Length > 0)
            {
                // This could be escapeCodes setting without any effect,
                // Git also ends "configured with empty" (like diff context set in GE) without start with end segment.
                Debug.WriteLineIf(sb.Length < 256, $"Debug: Unexpected no ongoing marker at {sb.Length}.");
                return;
            }

            int len = sb.Length - currentHighlight.DocOffset;
            if (len == 1 && sb[^1] == '\r')
            {
                // Marker without any visible effect, likely diff.colorMovedWS
                currentHighlight.Length = -1;
                ++currentHighlight.DocOffset;
                return;
            }

            currentHighlight.Length = len;
            TextMarker? prevMarker = textMarkers.Count == 0 ? null : textMarkers[^1];
            if (TryGetTextMarker(currentHighlight, prevMarker, sb, out TextMarker tm))
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
    /// <returns><see langword="true"/> if a color was set; if just reset <see langword="false"/>.</returns>
    private static bool TryGetColorsFromEscapeSequence(IList<int> escapeCodes, out Color? backColor, out Color? foreColor, ref int currentColorId, bool themeColors)
    {
        bool result = false;
        backColor = null;
        foreColor = null;
        int currentFore = -1;
        int currentBack = -1;

        // A subset of attributes supported, most are ignored, other handled as bold/dim
        bool reverse = false; // swap fore/back
        bool bold = false;
        bool dim = false;
        bool isChange = false; // Handle only bold/dim changes to current colors

        if (escapeCodes.Count == 0)
        {
            // Empty sequence is the same as reset to normal
            escapeCodes = [0];
        }

        for (int i = 0; i < escapeCodes.Count; ++i)
        {
            switch (escapeCodes[i])
            {
                case 0: // Reset or normal, all attributes become turned off
                    result = false;
                    reverse = false;
                    backColor = null;
                    foreColor = null;
                    currentColorId = _blackId;
                    currentFore = -1;
                    currentBack = -1;
                    bold = false;
                    dim = false;
                    break;
                case 1: // bold
                case 4: // underline/ul
                case 5: // slow blink
                case 6: // fast blink
                case 9: // strike
                    bold = true;
                    isChange = true;
                    break;
                case 2: // dim
                case 3: // italic
                case 8: // conceal
                    dim = true;
                    isChange = true;
                    break;
                case 7: // reverse
                    reverse = true;
                    break;
                case 22: // normal color, intensity
                    bold = false;
                    dim = false;
                    break;
                case 39: // Default foreground color
                    foreColor = null;
                    currentFore = currentColorId = _blackId;
                    break;
                case 49: // Default background color
                    backColor = null;
                    currentBack = _whiteId;
                    break;
                case >= 30 and <= 37: // Set foreground color
                    currentFore = escapeCodes[i] - 30;
                    break;
                case >= 90 and <= 97: // Set bold foreground color
                    currentFore = escapeCodes[i] - 90 + _boldOffset;
                    break;
                case >= 40 and <= 47: // Set background color
                    currentBack = escapeCodes[i] - 40;
                    break;
                case >= 100 and <= 107: // Set bold background color
                    currentBack = escapeCodes[i] - 100 + _boldOffset;
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
                    bool fore = escapeCodes[i] == 38;
                    ++i;

                    if (escapeCodes[i] == 5)
                    {
                        // ESC[38:5:⟨n⟩m Select foreground color
                        ++i;
                        int id = escapeCodes[i];
                        currentColorId = _blackId;
                        if (fore)
                        {
                            currentFore = id;
                        }
                        else
                        {
                            currentBack = id;
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
                        currentColorId = _blackId;

                        // Set the color, override if set later
                        if (fore)
                        {
                            currentFore = -1;
                            foreColor = color;
                        }
                        else
                        {
                            currentBack = -1;
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

        if (themeColors && !reverse && !dim
            && (currentBack < 0 || backColor is null)
            && foreColor is null
            && currentFore is 1 or 2)
        {
            // Assume this is a fit for the theme colors with reverse color (e.g. difftastic)
            // Change bold -> normal, normal -> dim to match GE theme better
            // difftastic 'normal' is not only unchanged why GE unchanged dim-dim is not used
            backColor = Get8bitColor(currentFore, fore: false, bold: false, dim: !bold);
            currentFore = -1;
        }

        if (isChange && (foreColor is null && backColor is null && currentFore < 0 && currentBack < 0))
        {
            currentFore = currentColorId;
        }

        if (reverse)
        {
            (backColor, foreColor) = (foreColor, backColor);
            (currentBack, currentFore) = (currentFore, currentBack);
        }

        if (currentFore >= 0)
        {
            if (currentFore <= _whiteId + _boldOffset)
            {
                // Mask bold, last three bits
                currentColorId = currentFore & (_boldOffset - 1);
            }

            foreColor = Get8bitColor(currentFore, fore: true, bold, dim);
        }

        if (currentBack >= 0)
        {
            backColor = Get8bitColor(currentBack, fore: false, bold, dim);
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
    /// <returns>The 24-bit RGB color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Unexpected value.</exception>
    private static Color Get8bitColor(int colorCode, bool fore, bool bold, bool dim)
    {
        if (bold && colorCode is >= _blackId and <= _whiteId)
        {
            colorCode += _boldOffset;
        }

        Color color = colorCode switch
        {
            _blackId => fore
                ? AppColor.AnsiTerminalBlackForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalBlackBackNormal.GetThemeColor(),
            _blackId + _boldOffset => fore
                ? AppColor.AnsiTerminalBlackForeBold.GetThemeColor()
                : AppColor.AnsiTerminalBlackBackBold.GetThemeColor(),

            _redId => fore
                ? AppColor.AnsiTerminalRedForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalRedBackNormal.GetThemeColor(),
            _redId + _boldOffset => fore
                ? AppColor.AnsiTerminalRedForeBold.GetThemeColor()
                : AppColor.AnsiTerminalRedBackBold.GetThemeColor(),

            _greenId => fore
                ? AppColor.AnsiTerminalGreenForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalGreenBackNormal.GetThemeColor(),
            _greenId + _boldOffset => fore
                ? AppColor.AnsiTerminalGreenForeBold.GetThemeColor()
                : AppColor.AnsiTerminalGreenBackBold.GetThemeColor(),

            _yellowId => fore
                ? AppColor.AnsiTerminalYellowForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalYellowBackNormal.GetThemeColor(),
            _yellowId + _boldOffset => fore
                ? AppColor.AnsiTerminalYellowForeBold.GetThemeColor()
                : AppColor.AnsiTerminalYellowBackBold.GetThemeColor(),

            _blueId => fore
                ? AppColor.AnsiTerminalBlueForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalBlueBackNormal.GetThemeColor(),
            _blueId + _boldOffset => fore
                ? AppColor.AnsiTerminalBlueForeBold.GetThemeColor()
                : AppColor.AnsiTerminalBlueBackBold.GetThemeColor(),

            _magentaId => fore
                ? AppColor.AnsiTerminalMagentaForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalMagentaBackNormal.GetThemeColor(),
            _magentaId + _boldOffset => fore
                ? AppColor.AnsiTerminalMagentaForeBold.GetThemeColor()
                : AppColor.AnsiTerminalMagentaBackBold.GetThemeColor(),

            _cyanId => fore
                ? AppColor.AnsiTerminalCyanForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalCyanBackNormal.GetThemeColor(),
            _cyanId + _boldOffset => fore
                ? AppColor.AnsiTerminalCyanForeBold.GetThemeColor()
                : AppColor.AnsiTerminalCyanBackBold.GetThemeColor(),

            _whiteId => fore
                ? AppColor.AnsiTerminalWhiteForeNormal.GetThemeColor()
                : AppColor.AnsiTerminalWhiteBackNormal.GetThemeColor(),
            _whiteId + _boldOffset => fore
                ? AppColor.AnsiTerminalWhiteForeBold.GetThemeColor()
                : AppColor.AnsiTerminalWhiteBackBold.GetThemeColor(),

            >= 16 and < 232 => Get216Colors(colorCode),
            >= 232 and <= 255 => Get24StepGray(colorCode),
            _ => throw new ArgumentOutOfRangeException(nameof(colorCode), colorCode, $"Unexpected value for ANSI color.")
        };

        if (dim)
        {
            color = ColorHelper.DimColor(color);
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

    /// <summary>
    /// Add pre-parsed highlight info (parsed ANSI escape sequences) as markers in the document.
    /// </summary>
    /// <param name="hl">Info to set.</param>
    /// <param name="prevMarker">Previous marker to potentially merge with.</param>
    /// <param name="sb">Text in the document.</param>
    /// <param name="textMarker">The created text marker or null</param>
    /// <returns><see langword="true"/> if a marker was created, otherwise <see langword="false"/>.</returns>
    public static bool TryGetTextMarker(HighlightInfo hl, TextMarker? prevMarker, StringBuilder sb, out TextMarker? textMarker)
    {
        if (hl.DocOffset < 0 || hl.Length < 0)
        {
            Trace.WriteLine($"Unexpected no docOffset or backColor ({hl})");
            DebugHelpers.Fail($"Unexpected no docOffset or backColor ({hl})");
            textMarker = null;
            return false;
        }

        // BackColor must always be set
        hl.BackColor ??= SystemColors.Window;

        // Check if segment can be merged with the previous
        if (prevMarker is not null
            && prevMarker.Color == hl.BackColor
            && prevMarker.ForeColor == hl.ForeColor)
        {
            int gapLen = hl.DocOffset - prevMarker.EndOffset - 1;
            if (gapLen == 0)
            {
                // zero gap, Git often have consecutive sections (like '+' in separate)
            }
            else if (gapLen == 1
                && sb[hl.DocOffset - 1] is ('\n' or '\r'))
            {
                // Only \n, gap is a newline, not colored in the viewer
            }
            else if (gapLen == 2
                && sb[hl.DocOffset - 2] == '\r'
                && sb[hl.DocOffset - 1] == '\n')
            {
                // Only \r\n
            }
            else
            {
                gapLen = -1;
            }

            if (gapLen >= 0)
            {
                prevMarker.Length += gapLen + hl.Length;
                textMarker = null;
                return false;
            }
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

        public static Color Get8bitColor(int colorCode, bool fore, bool bold, bool dim)
            => AnsiEscapeUtilities.Get8bitColor(colorCode, fore, bold, dim);

        public static int GetBoldOffset() => _boldOffset;
    }
}

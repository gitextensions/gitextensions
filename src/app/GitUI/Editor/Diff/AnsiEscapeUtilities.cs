﻿using System.Diagnostics;
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
            foreach (int offset in new List<int>() { 0, _boldOffset })
            {
                foreach (int dim in new List<int>() { 0, 2 })
                {
                    sb.Append($"{offset:d1} ");
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

        // GE theme colors - primarily used as background color with adjusted foreground
        foreach (List<Color?> cs in new List<List<Color?>>()
            {
            new() { null, AppColor.DiffRemoved.GetThemeColor(), AppColor.DiffAdded.GetThemeColor(), null, null, null, null, AppColor.DiffSection.GetThemeColor(), },
            new() { null, AppColor.DiffRemovedExtra.GetThemeColor(), AppColor.DiffAddedExtra.GetThemeColor(), }
            })
        {
            sb.Append($"{" ",2}");
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
        bool bold = false; // Git bright, increased intensity
        bool dim = false; // faint, decreased intensity
        bool reverse = false; // swap fore/back
        bool fore = true; // Set fore color
        bool dimApplied = false; // dim must be applied only once
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
                    if (fore)
                    {
                        foreColor = Get8bitColor(currentColorId + GetBoldOffset(), dim, out _);
                    }
                    else
                    {
                        backColor = Get8bitColor(currentColorId + GetBoldOffset(), dim, out _);
                    }

                    dimApplied = false;
                    break;
                case 2: // dim
                case 3: // italic
                case 8: // conceal
                    dim = true;
                    if (!dimApplied)
                    {
                        if (fore)
                        {
                            foreColor = foreColor is null ? foreColor : DimColor((Color)foreColor);
                        }
                        else
                        {
                            backColor = backColor is null ? backColor : DimColor((Color)backColor);
                        }
                    }

                    dimApplied = true;
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
                case >= 90 and <= 97: // Set bold foreground color
                    fore = true;
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
                        foreColor = Get8bitColor(currentColorId + GetBoldOffset(), dim, out _);
                    }

                    dimApplied = false;
                    break;
                case >= 40 and <= 47: // Set background color
                case >= 100 and <= 107: // Set bold background color
                    fore = false;
                    bold = bold || escapeCodes[i] >= 100;
                    int backColorId = escapeCodes[i] - (escapeCodes[i] >= 100 ? 100 : 40);
                    backColor = Get8bitColor(backColorId + GetBoldOffset(), dim, out _);
                    dimApplied = false;
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
                        bold = escapeCodes[i] is >= _boldOffset and < 2 * _boldOffset;
                        color = Get8bitColor(escapeCodes[i], dim, out currentColorId);
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

                    if (fore)
                    {
                        foreColor = color;
                    }
                    else
                    {
                        backColor = color;
                    }

                    dimApplied = false;
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

        int GetBoldOffset() => bold ? (int)_boldOffset : 0;
    }

    /// <summary>
    /// Decode 3, 4, and 8-bit colors from https://en.wikipedia.org/wiki/ANSI_escape_code#24-bit
    /// The named 3/4 bit colors for the invariant theme is from the "theme" at
    /// https://github.com/mintty/mintty/blob/master/themes/helmholtz
    /// with the primary difference that mintty allows background and foreground colors to be separatly configured.
    /// </summary>
    /// <param name="colorCode">The color code to decode.</param>
    /// <param name="colorId">ANSI color id if known, otherwise default black.</param>
    /// <returns>The 24-bit RGB color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Unexpected value.</exception>
    private static Color Get8bitColor(int colorCode, bool dim, out int colorId)
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
            blackId => AppColor.AnsiTerminalBlackNormal.GetThemeColor(),
            blackId + _boldOffset => AppColor.AnsiTerminalBlackBold.GetThemeColor(),

            redId => AppColor.AnsiTerminalRedNormal.GetThemeColor(),
            redId + _boldOffset => AppColor.AnsiTerminalRedBold.GetThemeColor(),

            greenId => AppColor.AnsiTerminalGreenNormal.GetThemeColor(),
            greenId + _boldOffset => AppColor.AnsiTerminalGreenBold.GetThemeColor(),

            yellowId => AppColor.AnsiTerminalYellowNormal.GetThemeColor(),
            yellowId + _boldOffset => AppColor.AnsiTerminalYellowBold.GetThemeColor(),

            blueId => AppColor.AnsiTerminalBlueNormal.GetThemeColor(),
            blueId + _boldOffset => AppColor.AnsiTerminalBlueBold.GetThemeColor(),

            magentaId => AppColor.AnsiTerminalMagentaNormal.GetThemeColor(),
            magentaId + _boldOffset => AppColor.AnsiTerminalMagentaBold.GetThemeColor(),

            cyanId => AppColor.AnsiTerminalCyanNormal.GetThemeColor(),
            cyanId + _boldOffset => AppColor.AnsiTerminalCyanBold.GetThemeColor(),

            whiteId => AppColor.AnsiTerminalWhiteNormal.GetThemeColor(),
            whiteId + _boldOffset => AppColor.AnsiTerminalWhiteBold.GetThemeColor(),

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

        public static Color Get8bitColor(int colorCode, bool dim, out int colorId)
            => AnsiEscapeUtilities.Get8bitColor(colorCode, dim, out colorId);

        public static int GetBoldOffset() => _boldOffset;
    }
}

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUIPluginInterfaces;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class GrepHighlightService : TextHighlightService
{
    private readonly bool _useGitColoring;
    private readonly List<TextMarker> _textMarkers = [];
    private List<(int lineno, bool match)> _matchInfos = [];

    [GeneratedRegex(@"-e\s*(((?<q>""|')(?<quote>.*?)\k<q>)|(?<noquote>[^\s]+))", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepSearchStringRegex();
    [GeneratedRegex(@"^(?<line>\d+)(?<kind>:|.)(?<text>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepLineRegex();
    [GeneratedRegex(@"^(?<line>\d+)(:(?<column>\d+))?(?<kind>:|.)(?<text>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepLineColumnRegex();

    public GrepHighlightService(ref string text, bool useGitColoring, string grepString)
    {
        _useGitColoring = useGitColoring;
        SetText(ref text, useGitColoring, grepString);
    }

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        lineNumbersControl.DisplayLineNum(GetDiffLinesInfo(), showLeftColumn: false);
    }

    /// <summary>
    /// Get the next/previous line for the grep match.
    /// </summary>
    /// <param name="rowIndexInText">The row index (starting from 0) for the current position.</param>
    /// <param name="next"><c>true</c> if next position, <c>false</c> if previous.</param>
    /// <returns>The next/previous line if found, -1 otherwise.</returns>
    public int GetGrepLineNum(int rowIndexInText, bool next)
    {
        int increase = next ? 1 : -1;

        // If start index is on a match, move to next
        if (rowIndexInText >= 0 && rowIndexInText < _matchInfos.Count && _matchInfos[rowIndexInText].match)
        {
            rowIndexInText += increase;
        }

        while (rowIndexInText >= 0 && rowIndexInText < _matchInfos.Count && !_matchInfos[rowIndexInText].match)
        {
            rowIndexInText += increase;
        }

        return rowIndexInText >= 0 && rowIndexInText < _matchInfos.Count && _matchInfos[rowIndexInText].match ? rowIndexInText : -1;
    }

    public static GitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
    {
        if (!useGitColoring)
        {
            // Use default
            return null;
        }

        GitCommandConfiguration commandConfiguration = new();
        IReadOnlyList<GitConfigItem> items = GitCommandConfiguration.Default.Get("grep");
        foreach (GitConfigItem cfg in items)
        {
            commandConfiguration.Add(cfg, "grep");
        }

        // No coloring, values are parsed
        commandConfiguration.Add(new GitConfigItem("color.grep.lineNumber", ""), "grep");
        commandConfiguration.Add(new GitConfigItem("color.grep.separator", ""), "grep");

        // Override Git default coloring unless the user overrides
        // As empty and unset are both reported as "", user cannot ignore with empty string
        if (AppSettings.UseGEThemeGitColoring.Value)
        {
            commandConfiguration.Add(AnsiEscapeUtilities.SetUnsetGitColor(
               "color.grep.matchSelected",
               AppColor.DiffRemovedExtra),
               "grep");
        }

        return commandConfiguration;
    }

    public override void AddTextHighlighting(IDocument document)
    {
        foreach (TextMarker tm in _textMarkers)
        {
            document.MarkerStrategy.AddMarker(tm);
        }

        return;
    }

    private void SetText(ref string text, bool useGitColoring, string grepString)
    {
        // Guess the length of the match string (otherwise use default 1)
        int grepLength = 1;
        if (!useGitColoring
            && GrepSearchStringRegex().Match(grepString) is Match grepLengthMatch && grepLengthMatch.Success)
        {
            grepLength = grepLengthMatch.Groups["quote"].Success
                ? grepLengthMatch.Groups["quote"].Length
                : grepLengthMatch.Groups["noquote"].Length;
        }

        StringBuilder sb = new(text.Length);
        foreach (string line in text.LazySplit('\n'))
        {
            if (line == "--")
            {
                if (sb.Length > 0)
                {
                    _matchInfos.Add((DiffLineInfo.NotApplicableLineNum, false));
                    sb.Append('\n');
                }

                continue;
            }

            // Parse line no and if match (must not have colors)
            Match match = useGitColoring
                ? GrepLineRegex().Match(line)
                : GrepLineColumnRegex().Match(line);
            if (!match.Success || !int.TryParse(match.Groups["line"].ValueSpan, out int lineNo))
            {
                if (line.Length > 0)
                {
                    Trace.WriteLine($"Cannot parse lineNo for grep {line} ({sb.Length})");
                    DebugHelpers.Fail($"Cannot parse lineNo for grep {line} ({sb.Length})");
                }

                // git-grep emitts an enpty line last, should not be displayed.
                // Other occurrences should not occur, just print them to debug.
                sb.Append(line);
                continue;
            }

            bool isMatch = match.Groups["kind"].Success && match.Groups["kind"].Value == ":";
            _matchInfos.Add((lineNo, isMatch));
            string grepText = match.Groups["text"].Value;

            if (useGitColoring)
            {
                AnsiEscapeUtilities.ParseEscape(grepText, sb, _textMarkers);
            }
            else
            {
                if (isMatch && match.Groups["column"].Success
                    && int.TryParse(match.Groups["column"].ValueSpan, out int column) && column > 0)
                {
                    Color color = AppColor.DiffAddedExtra.GetThemeColor();
                    Color forecolor = ColorHelper.GetForeColorForBackColor(color);
                    if (AnsiEscapeUtilities.TryGetTextMarker(new()
                    {
                        DocOffset = sb.Length + column - 1,
                        Length = grepLength,
                        BackColor = color,
                        ForeColor = forecolor
                    },
                        out TextMarker tm))
                    {
                        _textMarkers.Add(tm);
                    }
                }

                sb.Append(grepText);
            }

            sb.Append('\n');
        }

#if DEBUG
        if (grepString == "-e \"Colors\"")
        {
            AnsiEscapeUtilities.PrintColors(sb, _textMarkers);
        }
#endif
        text = sb.ToString();
    }

    /// <summary>
    /// Get the type of line including the line number for the lines visible in the editor.
    /// This information is parsed already when the git-grep command was parsed,
    /// for git-diff this is parsed dynamically.
    /// </summary>
    /// <returns>The type of contents for all editor lines.</returns>
    private DiffLinesInfo GetDiffLinesInfo()
    {
        DiffLinesInfo result = new();
        for (int i = 0; i < _matchInfos.Count; ++i)
        {
            int lineno = _matchInfos[i].lineno;
            bool match = _matchInfos[i].match;

            DiffLineInfo diffLineInfo = new()
            {
                LineNumInDiff = i + 1,
                LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                RightLineNumber = lineno,
                LineType = lineno == DiffLineInfo.NotApplicableLineNum
                    ? DiffLineType.Header
                    : match
                        ? DiffLineType.Grep
                        : DiffLineType.Context
            };

            result.Add(diffLineInfo);
        }

        return result;
    }
}

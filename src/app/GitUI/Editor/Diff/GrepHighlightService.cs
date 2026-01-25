using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class GrepHighlightService : TextHighlightService
{
    private const string _grepResultKind_FunctionHeader = "=";
    private const string _grepResultKind_Match = ":";
    private const string _grepResultKind_Separator = "--";
    private const string _grepResultKind_Unknown = "";

    private readonly List<TextMarker> _textMarkers = [];
    private DiffLinesInfo _diffLinesInfo = new();

    [GeneratedRegex(@"^(?<line>\d+)(?<kind>:|.)(?<text>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepLineRegex { get; }

    public GrepHighlightService(ref string text, DiffViewerLineNumberControl lineNumbersControl)
    {
        SetText(ref text);
        lineNumbersControl.DisplayLineNum(_diffLinesInfo, showLeftColumn: false);
    }

    public override void AddTextHighlighting(IDocument document)
        => document.MarkerStrategy.AddMarkers(_textMarkers);

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is (DiffLineType.Minus or DiffLineType.Plus or DiffLineType.MinusPlus or DiffLineType.Grep);

    /// <summary>
    /// Get the next/previous line for the grep match.
    /// </summary>
    /// <param name="rowIndexInText">The row index (starting from 0) for the current position.</param>
    /// <param name="next"><c>true</c> if next position, <c>false</c> if previous.</param>
    /// <returns>The next/previous index if found, -1 otherwise.</returns>
    public int GetGrepLineNum(int rowIndexInText, bool next)
    {
        int increase = next ? 1 : -1;

        // If start index is on a match, move to next
        if (_diffLinesInfo.DiffLines.TryGetValue(rowIndexInText, out DiffLineInfo lineInfo) && lineInfo.LineType == DiffLineType.Grep)
        {
            rowIndexInText += increase;
        }

        while (_diffLinesInfo.DiffLines.TryGetValue(rowIndexInText, out lineInfo) && lineInfo.LineType != DiffLineType.Grep)
        {
            rowIndexInText += increase;
        }

        return lineInfo?.LineType is DiffLineType.Grep ? rowIndexInText - increase : -1;
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module)
    {
        GitCommandConfiguration commandConfiguration = new();
        IReadOnlyList<GitConfigItem> items = GitCommandConfiguration.Default.Get("grep");
        foreach (GitConfigItem cfg in items)
        {
            commandConfiguration.Add(cfg, "grep");
        }

        // No coloring, values are parsed
        commandConfiguration.Add(new GitConfigItem("color.grep.linenumber", ""), "grep");
        commandConfiguration.Add(new GitConfigItem("color.grep.separator", ""), "grep");

        SetIfUnsetInGit(key: "color.grep.function", value: "white dim reverse");
        if (AppSettings.ReverseGitColoring.Value)
        {
            SetIfUnsetInGit(key: "color.grep.matchselected", value: "red bold reverse");
        }

        return commandConfiguration;

        void SetIfUnsetInGit(string key, string value)
        {
            // Note: Only check Windows, not WSL settings
            if (string.IsNullOrEmpty(module.GetEffectiveSetting(key)))
            {
                commandConfiguration.Add(new GitConfigItem(key, value), "grep");
            }
        }
    }

    private void SetText(ref string text)
    {
        StringBuilder sb = new(text.Length);
        bool skipNextSeparator = false;
        bool pendingSeparator = false;
        bool firstError = true;
        foreach (string line in text.LazySplit('\n'))
        {
            if (line == _grepResultKind_Separator)
            {
                if (!skipNextSeparator && sb.Length > 0)
                {
                    pendingSeparator = true;
                }

                continue;
            }

            // Parse line no and if match (must not have colors)
            Match match = GrepLineRegex.Match(line);
            if (!match.Success || !int.TryParse(match.Groups["line"].ValueSpan, out int lineNo))
            {
                if (line.Length > 0)
                {
                    if (firstError)
                    {
                        firstError = false;
                        Trace.WriteLine($"Cannot parse lineNo for grep {line.ShortenTo(80)} ({sb.Length})");
                    }
                }

                // git-grep emits an empty line last, should not be displayed.
                // Other occurrences should not occur, just print them to debug (no lineno to not add extra line).
                sb.Append(line);
                pendingSeparator = false;
                continue;
            }

            string grepText = match.Groups["text"].Value;
            string kind = match.Groups["kind"].Success ? match.Groups["kind"].Value : _grepResultKind_Unknown;

            skipNextSeparator = kind == _grepResultKind_FunctionHeader;
            if (pendingSeparator && !skipNextSeparator)
            {
                _diffLinesInfo.Add(GetDiffLineInfo(DiffLineInfo.NotApplicableLineNum, _grepResultKind_Separator));
                sb.Append('\n');
            }

            pendingSeparator = false;
            _diffLinesInfo.Add(GetDiffLineInfo(lineNo, kind));

            AnsiEscapeUtilities.ParseEscape(grepText, sb, _textMarkers);
            sb.Append('\n');
        }

        if (new EnvironmentAbstraction().GetEnvironmentVariable("GIT_EXTENSIONS_CONSOLE_COLORS") is not null)
        {
            // Debug printout the theme
            AnsiEscapeUtilities.PrintColors(sb, _textMarkers);
        }

        text = sb.ToString();
    }

    /// <summary>
    /// Get the type of line including the line number for the lines visible in the editor.
    /// This information is parsed already when the git-grep command was parsed,
    /// for git-diff this is parsed dynamically.
    /// </summary>
    /// <returns>The type of contents for all editor lines.</returns>
    private DiffLineInfo GetDiffLineInfo(int lineno, string kind)
        => new()
        {
            LineNumInDiff = _diffLinesInfo.DiffLines.Count + 1,
            LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
            RightLineNumber = lineno,
            LineType = lineno == DiffLineInfo.NotApplicableLineNum || kind == _grepResultKind_FunctionHeader
                    ? DiffLineType.Header
                    : kind == _grepResultKind_Match
                        ? DiffLineType.Grep
                        : DiffLineType.Context
        };
}

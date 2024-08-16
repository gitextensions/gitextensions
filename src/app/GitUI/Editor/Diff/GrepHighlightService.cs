using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class GrepHighlightService : TextHighlightService
{
    private readonly List<TextMarker> _textMarkers = [];
    private DiffLinesInfo _matchInfos = new();

    [GeneratedRegex(@"^(?<line>\d+)(?<kind>:|.)(?<text>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepLineRegex();

    public GrepHighlightService(ref string text)
        => SetText(ref text);

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is (DiffLineType.Minus or DiffLineType.Plus or DiffLineType.MinusPlus or DiffLineType.Grep);

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
        => lineNumbersControl.DisplayLineNum(_matchInfos, showLeftColumn: false);

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
        if (_matchInfos.DiffLines.TryGetValue(rowIndexInText, out DiffLineInfo lineInfo) && lineInfo.LineType == DiffLineType.Grep)
        {
            rowIndexInText += increase;
        }

        while (_matchInfos.DiffLines.TryGetValue(rowIndexInText, out lineInfo) && lineInfo.LineType != DiffLineType.Grep)
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
        commandConfiguration.Add(new GitConfigItem("color.grep.lineNumber", ""), "grep");
        commandConfiguration.Add(new GitConfigItem("color.grep.separator", ""), "grep");

        if (AppSettings.ReverseGitColoring.Value)
        {
            SetIfUnsetInGit(key: "color.grep.matchSelected", value: "red bold reverse");
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

    public override void AddTextHighlighting(IDocument document)
    {
        foreach (TextMarker tm in _textMarkers)
        {
            document.MarkerStrategy.AddMarker(tm);
        }
    }

    private void SetText(ref string text)
    {
        StringBuilder sb = new(text.Length);
        foreach (string line in text.LazySplit('\n'))
        {
            if (line == "--")
            {
                if (sb.Length > 0)
                {
                    _matchInfos.Add(GetDiffLineInfo(DiffLineInfo.NotApplicableLineNum, false));
                    sb.Append('\n');
                }

                continue;
            }

            // Parse line no and if match (must not have colors)
            Match match = GrepLineRegex().Match(line);
            if (!match.Success || !int.TryParse(match.Groups["line"].ValueSpan, out int lineNo))
            {
                if (line.Length > 0)
                {
                    Trace.WriteLine($"Cannot parse lineNo for grep {line} ({sb.Length})");
                    DebugHelpers.Fail($"Cannot parse lineNo for grep {line} ({sb.Length})");
                }

                // git-grep emits an empty line last, should not be displayed.
                // Other occurrences should not occur, just print them to debug.
                sb.Append(line);
                continue;
            }

            bool isMatch = match.Groups["kind"].Success && match.Groups["kind"].Value == ":";
            _matchInfos.Add(GetDiffLineInfo(lineNo, isMatch));
            string grepText = match.Groups["text"].Value;

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
    private DiffLineInfo GetDiffLineInfo(int lineno, bool match)
        => new()
        {
            LineNumInDiff = _matchInfos.DiffLines.Count + 1,
            LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
            RightLineNumber = lineno,
            LineType = lineno == DiffLineInfo.NotApplicableLineNum
                    ? DiffLineType.Header
                    : match
                        ? DiffLineType.Grep
                        : DiffLineType.Context
        };
}

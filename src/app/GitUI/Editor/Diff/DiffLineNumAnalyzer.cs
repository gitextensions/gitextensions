using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class DiffLineNumAnalyzer
{
    [GeneratedRegex(@"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex DiffRegex();

    public static DiffLinesInfo Analyze(TextEditorControl textEditor, bool isCombinedDiff, bool isGitWordDiff = false)
    {
        DiffLinesInfo ret = new();
        int lineNumInDiff = 0;
        int leftLineNum = DiffLineInfo.NotApplicableLineNum;
        int rightLineNum = DiffLineInfo.NotApplicableLineNum;
        bool isHeaderLineLocated = false;
        string[] lines = textEditor.Text.Split(Delimiters.LineFeed);
        int textOffset = 0; // for git-diff-word
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (i == lines.Length - 1 && string.IsNullOrEmpty(line))
            {
                break;
            }

            int textLength = lines[i].Length + 1;
            Lazy<List<TextMarker>> textMarkers = new(() => textEditor.Document.MarkerStrategy.GetMarkers(textOffset, textLength));
            lineNumInDiff++;
            if (line.StartsWith("@@"))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = DiffLineType.Header
                };

                Match lineNumbers = DiffRegex().Match(line);
                leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].Value);
                rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].Value);

                ret.Add(meta);
                isHeaderLineLocated = true;
            }
            else if (isHeaderLineLocated && isCombinedDiff)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                };

                if (IsMinusLineInCombinedDiff(line))
                {
                    meta.LineType = DiffLineType.Minus;
                }
                else if (IsPlusLineInCombinedDiff(line))
                {
                    meta.LineType = DiffLineType.Plus;
                    meta.RightLineNumber = rightLineNum;
                    rightLineNum++;
                }
                else
                {
                    meta.LineType = DiffLineType.Context;
                    meta.RightLineNumber = rightLineNum;
                    rightLineNum++;
                }

                ret.Add(meta);
            }
            else if (isHeaderLineLocated && ((!isGitWordDiff && IsMinusLine(line))

                // Heuristics: For GitWordDiff AppSettings.ReverseGitColoring is assumed, otherwise just DiffLineType.Mixed is detected
                || (isGitWordDiff && textMarkers.Value.Count > 0 && textMarkers.Value.All(i => i.Color == AppColor.AnsiTerminalRedBackNormal.GetThemeColor()))))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = isGitWordDiff ? DiffLineType.MinusLeft : DiffLineType.Minus
                };
                ret.Add(meta);

                leftLineNum++;
            }
            else if (isHeaderLineLocated && ((!isGitWordDiff && IsPlusLine(line))
                || (isGitWordDiff && textMarkers.Value.Count > 0 && textMarkers.Value.All(i => i.Color == AppColor.AnsiTerminalGreenBackNormal.GetThemeColor()))))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = isGitWordDiff ? DiffLineType.PlusRight : DiffLineType.Plus,
                };
                ret.Add(meta);
                rightLineNum++;
            }
            else if (isHeaderLineLocated && isGitWordDiff && textMarkers.Value.Count > 0)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = DiffLineType.MinusPlus,
                };
                ret.Add(meta);
                leftLineNum++;
                rightLineNum++;
            }
            else if (i == lines.Length - 1 && line.StartsWith(GitModule.NoNewLineAtTheEnd))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = DiffLineType.Header
                };
                ret.Add(meta);
            }
            else if (isHeaderLineLocated)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = DiffLineType.Context,
                };
                ret.Add(meta);

                leftLineNum++;
                rightLineNum++;
            }

            textOffset += textLength;
        }

        return ret;
    }

    private static bool IsMinusLine(string line)
    {
        return line.StartsWith('-');
    }

    private static bool IsPlusLine(string line)
    {
        return line.StartsWith('+');
    }

    private static bool IsPlusLineInCombinedDiff(string line)
    {
        return line.StartsWith("++") || line.StartsWith("+ ") || line.StartsWith(" +");
    }

    private static bool IsMinusLineInCombinedDiff(string line)
    {
        return line.StartsWith("--") || line.StartsWith("- ") || line.StartsWith(" -");
    }
}

using System;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Patches;

namespace GitUI.Editor.Diff
{
    public class DiffLineNumAnalyzer
    {
        private static readonly Regex regex = new(
            @"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public DiffLinesInfo Analyze(string diffContent)
        {
            DiffLinesInfo ret = new();
            var isCombinedDiff = PatchProcessor.IsCombinedDiff(diffContent);
            var lineNumInDiff = 0;
            var leftLineNum = DiffLineInfo.NotApplicableLineNum;
            var rightLineNum = DiffLineInfo.NotApplicableLineNum;
            var isHeaderLineLocated = false;
            string[] lines = diffContent.Split(Delimiters.LineFeed);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i == lines.Length - 1 && string.IsNullOrEmpty(line))
                {
                    break;
                }

                lineNumInDiff++;
                if (line.StartsWith("@"))
                {
                    DiffLineInfo meta = new()
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                        RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                        LineType = DiffLineType.Header
                    };

                    var lineNumbers = regex.Match(line);
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
                else if (isHeaderLineLocated && IsMinusLine(line))
                {
                    DiffLineInfo meta = new()
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNumber = leftLineNum,
                        RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                        LineType = DiffLineType.Minus
                    };
                    ret.Add(meta);

                    leftLineNum++;
                }
                else if (isHeaderLineLocated && IsPlusLine(line))
                {
                    DiffLineInfo meta = new()
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                        RightLineNumber = rightLineNum,
                        LineType = DiffLineType.Plus,
                    };
                    ret.Add(meta);
                    rightLineNum++;
                }
                else if (line.StartsWith(GitModule.NoNewLineAtTheEnd))
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
            }

            return ret;
        }

        private static bool IsMinusLine(string line)
        {
            return line.StartsWith("-");
        }

        private static bool IsPlusLine(string line)
        {
            return line.StartsWith("+");
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
}

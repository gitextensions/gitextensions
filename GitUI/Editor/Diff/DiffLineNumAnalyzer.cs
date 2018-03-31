using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Patches;
using JetBrains.Annotations;

namespace GitUI.Editor.Diff
{
    public class DiffLineNumAnalyzer
    {
        private static readonly Regex regex = new Regex(
            @"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public class Result
        {
            public Dictionary<int, DiffLineNum> LineNumbers { get; } = new Dictionary<int, DiffLineNum>();
            public int MaxLineNumber;
        }

        private static void AddToResult(Result result, DiffLineNum diffLine)
        {
            result.LineNumbers.Add(diffLine.LineNumInDiff, diffLine);
            result.MaxLineNumber = Math.Max(result.MaxLineNumber,
                Math.Max(diffLine.LeftLineNum, diffLine.RightLineNum));
        }

        [NotNull]
        public Result Analyze([NotNull] string diffContent)
        {
            var ret = new Result();
            var isCombinedDiff = PatchProcessor.IsCombinedDiff(diffContent);
            var lineNumInDiff = 0;
            var leftLineNum = DiffLineNum.NotApplicableLineNum;
            var rightLineNum = DiffLineNum.NotApplicableLineNum;
            var isHeaderLineLocated = false;
            string[] lines = diffContent.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i == lines.Length - 1 && line.IsNullOrEmpty())
                {
                    break;
                }

                lineNumInDiff++;
                if (line.StartsWith("@"))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = DiffLineNum.NotApplicableLineNum,
                        RightLineNum = DiffLineNum.NotApplicableLineNum,
                        Style = DiffLineNum.DiffLineStyle.Header
                    };

                    var lineNumbers = regex.Match(line);
                    leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].Value);
                    rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].Value);

                    AddToResult(ret, meta);
                    isHeaderLineLocated = true;
                }
                else if (isHeaderLineLocated && isCombinedDiff)
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = DiffLineNum.NotApplicableLineNum,
                        RightLineNum = DiffLineNum.NotApplicableLineNum,
                    };

                    if (IsMinusLineInCombinedDiff(line))
                    {
                        meta.Style = DiffLineNum.DiffLineStyle.Minus;
                    }
                    else if (IsPlusLineInCombinedDiff(line))
                    {
                        meta.Style = DiffLineNum.DiffLineStyle.Plus;
                        meta.RightLineNum = rightLineNum;
                        rightLineNum++;
                    }
                    else
                    {
                        meta.Style = DiffLineNum.DiffLineStyle.Context;
                        meta.RightLineNum = rightLineNum;
                        rightLineNum++;
                    }

                    AddToResult(ret, meta);
                }
                else if (isHeaderLineLocated && IsMinusLine(line))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = leftLineNum,
                        RightLineNum = DiffLineNum.NotApplicableLineNum,
                        Style = DiffLineNum.DiffLineStyle.Minus
                    };
                    AddToResult(ret, meta);

                    leftLineNum++;
                }
                else if (isHeaderLineLocated && IsPlusLine(line))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = DiffLineNum.NotApplicableLineNum,
                        RightLineNum = rightLineNum,
                        Style = DiffLineNum.DiffLineStyle.Plus,
                    };
                    AddToResult(ret, meta);
                    rightLineNum++;
                }
                else if (line.StartsWith(GitModule.NoNewLineAtTheEnd))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = DiffLineNum.NotApplicableLineNum,
                        RightLineNum = DiffLineNum.NotApplicableLineNum,
                        Style = DiffLineNum.DiffLineStyle.Header
                    };
                    AddToResult(ret, meta);
                }
                else if (isHeaderLineLocated)
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = leftLineNum,
                        RightLineNum = rightLineNum,
                        Style = DiffLineNum.DiffLineStyle.Context,
                    };
                    AddToResult(ret, meta);

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

    public class DiffLineNum
    {
        public enum DiffLineStyle
        {
            Header,
            Plus,
            Minus,
            Context
        }

        public static readonly int NotApplicableLineNum = -1;
        public int LineNumInDiff { get; set; }
        public int LeftLineNum { get; set; }
        public int RightLineNum { get; set; }
        public DiffLineStyle Style { get; set; }
    }
}
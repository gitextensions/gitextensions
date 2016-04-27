using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using PatchApply;

namespace GitUI.Editor.Diff
{
    public class DiffLineNumAnalyzer
    {
        public delegate void EvLineNumAnalyzed(DiffLineNum diffLineNum);

        public event EvLineNumAnalyzed OnLineNumAnalyzed;

        private void BgWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            Start(doWorkEventArgs.Argument as string);
        }

        protected void FireLineAnalyzedEvent(DiffLineNum diffline)
        {
            var handler = OnLineNumAnalyzed;
            if (handler != null) handler(diffline);
        }

        BackgroundWorker _bgWorker = new BackgroundWorker();
        public void StartAsync(string diffContent, Action onCompleted)
        {
            if (_bgWorker.IsBusy)
            {
                _bgWorker.CancelAsync();
            }

            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += BgWorkerOnDoWork;
            _bgWorker.WorkerSupportsCancellation = true;

            _bgWorker.RunWorkerCompleted += (sender, args) => onCompleted();

            _bgWorker.RunWorkerAsync(diffContent);
        }

        public void Start(string diffContent)
        {
            var isCombinedDiff = PatchProcessor.IsCombinedDiff(diffContent);
            var lineNumInDiff = 0;
            var leftLineNum = DiffLineNum.NotApplicableLineNum;
            var rightLineNum = DiffLineNum.NotApplicableLineNum;
            var isHeaderLineLocated = false;
            foreach (var line in diffContent.Split('\n'))
            {
                if (_bgWorker.CancellationPending)
                {
                    return;
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
                    var regex =
                        new Regex(
                            @"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    var lineNumbers = regex.Match(line);
                    leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].Value);
                    rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].Value);

                    FireLineAnalyzedEvent(meta);
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

                    FireLineAnalyzedEvent(meta);
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
                    FireLineAnalyzedEvent(meta);

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
                    FireLineAnalyzedEvent(meta);
                    rightLineNum++;
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
                    FireLineAnalyzedEvent(meta);

                    leftLineNum++;
                    rightLineNum++;
                }
            }
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
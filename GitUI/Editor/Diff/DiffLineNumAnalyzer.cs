using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using PatchApply;

namespace GitUI.Editor.Diff
{
    public class DiffLineNumAnalyzer
    {
        private static Regex regex = new Regex(
            @"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public event Action<List<DiffLineNum>> OnLineNumAnalyzed;

        private void BgWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            Start(doWorkEventArgs.Argument as string, sender as BackgroundWorker);
        }

        protected void FireLineAnalyzedEvent(List<DiffLineNum> diffline)
        {
            var handler = OnLineNumAnalyzed;
            if (handler != null) handler(diffline);
            Thread.Sleep(100);// sleep for a while to improve the UI responsiveness
        }

        BackgroundWorker _bgWorker = new BackgroundWorker();
        public void StartAsync(string diffContent, Action onCompleted)
        {
            Cancel();

            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += BgWorkerOnDoWork;
            _bgWorker.WorkerSupportsCancellation = true;

            _bgWorker.RunWorkerCompleted += (sender, args) => onCompleted();

            _bgWorker.RunWorkerAsync(diffContent);
        }

        public void Start(string diffContent, BackgroundWorker worker)
        {
            var isCombinedDiff = PatchProcessor.IsCombinedDiff(diffContent);
            var lineNumInDiff = 0;
            var leftLineNum = DiffLineNum.NotApplicableLineNum;
            var rightLineNum = DiffLineNum.NotApplicableLineNum;
            var isHeaderLineLocated = false;
            var batch = new List<DiffLineNum>();
            foreach (var line in diffContent.Split('\n'))
            {
                if (worker.CancellationPending)
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

                    var lineNumbers = regex.Match(line);
                    leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].Value);
                    rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].Value);

                    batch.Add(meta);
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

                    batch.Add(meta);
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
                    batch.Add(meta);

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
                    batch.Add(meta);
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
                    batch.Add(meta);

                    leftLineNum++;
                    rightLineNum++;
                }

                if (lineNumInDiff % 100 == 0)
                {
                    FireLineAnalyzedEvent(batch);
                    batch = new List<DiffLineNum>();
                }
            }
            if (batch.Any())
            {
                FireLineAnalyzedEvent(batch);
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

        public void Cancel()
        {
            if (_bgWorker != null && _bgWorker.IsBusy)
            {
                _bgWorker.CancelAsync();
                _bgWorker = null;
            }
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
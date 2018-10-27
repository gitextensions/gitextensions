using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitStatistics.PieChart;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitStatistics
{
    public partial class FormGitStatistics : GitExtensionsFormBase
    {
        private readonly TranslationString _commits = new TranslationString("{0:N0} Commits");
        private readonly TranslationString _commitsBy = new TranslationString("{0:N0} Commits by {1}");
        private readonly TranslationString _linesOfCodeInFiles = new TranslationString("{0:N0} Lines of code in {1} files ({2:P1})");
        private readonly TranslationString _linesOfCode = new TranslationString("{0:N0} Lines of code");
        private readonly TranslationString _linesOfCodeP = new TranslationString("{0:N0} Lines of code ({1:P1})");
        private readonly TranslationString _linesOfTestCode = new TranslationString("{0:N0} Lines of test code");
        private readonly TranslationString _linesOfTestCodeP = new TranslationString("{0:N0} Lines of test code ({1:P1})");
        private readonly TranslationString _linesOfProductionCodeP = new TranslationString("{0:N0} Lines of production code ({1:P1})");
        private readonly TranslationString _blankLinesP = new TranslationString("{0:N0} Blank lines ({1:P1})");
        private readonly TranslationString _commentLinesP = new TranslationString("{0:N0} Comment lines ({1:P1})");
        private readonly TranslationString _linesOfDesignerFilesP = new TranslationString("{0:N0} Lines in designer files ({1:P1})");

        private readonly string _codeFilePattern;
        private readonly bool _countSubmodules;
        private readonly IGitModule _module;

        private LineCounter _lineCounter;

        protected Color[] DecentColors { get; } =
                {
                    Color.Red,
                    Color.Yellow,
                    Color.DodgerBlue,
                    Color.LightGreen,
                    Color.Coral,
                    Color.Goldenrod,
                    Color.YellowGreen,
                    Color.MediumPurple,
                    Color.LightGray,
                    Color.Brown,
                    Color.Pink,
                    Color.DarkBlue,
                    Color.Purple
                };

        public string DirectoriesToIgnore { get; set; }

        public FormGitStatistics(IGitModule module, string codeFilePattern, bool countSubmodules)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _module = module;
            _codeFilePattern = codeFilePattern;
            _countSubmodules = countSubmodules;
            InitializeComponent();

            splitContainer1.SplitterDistance = DpiUtil.Scale(splitContainer1.SplitterDistance);
            splitContainer2.SplitterDistance = DpiUtil.Scale(splitContainer2.SplitterDistance);
            splitContainer3.SplitterDistance = DpiUtil.Scale(splitContainer3.SplitterDistance);
            splitContainer4.SplitterDistance = DpiUtil.Scale(splitContainer4.SplitterDistance);
            splitContainer5.SplitterDistance = DpiUtil.Scale(splitContainer5.SplitterDistance);
            splitContainer6.SplitterDistance = DpiUtil.Scale(splitContainer6.SplitterDistance);
            splitContainer7.SplitterDistance = DpiUtil.Scale(splitContainer7.SplitterDistance);
            splitContainer8.SplitterDistance = DpiUtil.Scale(splitContainer8.SplitterDistance);

            TotalLinesOfCode.Font = new Font(TotalLinesOfCode.Font, FontStyle.Bold);
            TotalLinesOfCode2.Font = TotalLinesOfCode.Font;
            TotalLinesOfTestCode.Font = TotalLinesOfCode.Font;
            TotalCommits.Font = TotalLinesOfCode.Font;
            LoadingLabel.Font = TotalLinesOfCode.Font;

            InitializeComplete();
        }

        private void FormGitStatisticsSizeChanged(object sender, EventArgs e)
        {
            SetPieStyle(CommitCountPie);
            SetPieStyle(LinesOfCodeExtensionPie);
            SetPieStyle(LinesOfCodePie);
            SetPieStyle(TestCodePie);
        }

        public void Initialize()
        {
            InitializeCommitCount();
            InitializeLinesOfCode();
        }

        private void InitializeCommitCount()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                    var (totalCommits, commitsPerUser) = _module.GetCommitsByContributor();

                    await this.SwitchToMainThreadAsync();

                    TotalCommits.Text = string.Format(_commits.Text, totalCommits);

                    var builder = new StringBuilder();

                    var commitCountValues = new decimal[commitsPerUser.Count];
                    var commitCountLabels = new string[commitsPerUser.Count];
                    var n = 0;
                    foreach (var (user, commits) in commitsPerUser)
                    {
                        builder.AppendLine($"{commits:N0} {user}");

                        commitCountValues[n] = commits;
                        commitCountLabels[n] = string.Format(_commitsBy.Text, commits, user);
                        n++;
                    }

                    CommitCountPie.SetValues(commitCountValues);
                    CommitCountPie.ToolTips = commitCountLabels;

                    CommitStatistics.Text = builder.ToString();
                });
        }

        private void SetPieStyle(PieChartControl pie)
        {
            pie.SetLeftMargin(DpiUtil.Scale(10));
            pie.SetRightMargin(DpiUtil.Scale(10));
            pie.SetTopMargin(DpiUtil.Scale(10));
            pie.SetBottomMargin(DpiUtil.Scale(10));
            pie.SetFitChart(false);
            pie.SetEdgeColorType(EdgeColorType.DarkerThanSurface);
            pie.SetSliceRelativeHeight(0.20f);
            pie.SetColors(DecentColors);
            pie.SetShadowStyle(ShadowStyle.GradualShadow);

            if (pie.Parent.Width > pie.Parent.Height)
            {
                pie.Height = pie.Parent.Height;
                pie.Width = pie.Parent.Height;
            }
            else
            {
                pie.Height = pie.Parent.Width;
                pie.Width = pie.Parent.Width;
            }
        }

        private void InitializeLinesOfCode()
        {
            if (_lineCounter != null)
            {
                return;
            }

            _lineCounter = new LineCounter();
            _lineCounter.Updated += OnLineCounterUpdated;

            Task.Run(() => LoadLinesOfCode());
        }

        public void LoadLinesOfCode()
        {
            LoadLinesOfCodeForModule(_module);

            if (_countSubmodules)
            {
                var submodules = _module.GetSubmodulesInfo()
                    .Select(submodule => new GitModule(Path.Combine(_module.WorkingDir, submodule.LocalPath)));

                foreach (var submodule in submodules)
                {
                    LoadLinesOfCodeForModule(submodule);
                }
            }

            // Send 'changed' event when done
            OnLineCounterUpdated(_lineCounter, EventArgs.Empty);

            return;

            void LoadLinesOfCodeForModule(IGitModule module)
            {
                var filesToCheck = module
                    .GetTree(module.RevParse("HEAD"), full: true)
                    .Select(file => Path.Combine(module.WorkingDir, file.Name))
                    .ToList();

                _lineCounter.FindAndAnalyzeCodeFiles(_codeFilePattern, DirectoriesToIgnore, filesToCheck);
            }
        }

        private void OnLineCounterUpdated(object sender, EventArgs e)
        {
            // Must do this synchronously because lineCounter.LinesOfCodePerExtension might change while we are iterating over it otherwise.
            var extensionValues = new decimal[_lineCounter.LinesOfCodePerExtension.Count];
            var extensionLabels = new string[_lineCounter.LinesOfCodePerExtension.Count];

            var linesOfCodePerExtension = new List<KeyValuePair<string, int>>(_lineCounter.LinesOfCodePerExtension);
            linesOfCodePerExtension.Sort((first, next) => -first.Value.CompareTo(next.Value));

            var n = 0;
            var linesOfCodePerLanguageText = new StringBuilder();
            foreach (var (extension, loc) in linesOfCodePerExtension)
            {
                var percent = (double)loc / _lineCounter.CodeLineCount;
                var line = string.Format(_linesOfCodeInFiles.Text, loc, extension, percent);
                linesOfCodePerLanguageText.AppendLine(line);
                extensionValues[n] = loc;
                extensionLabels[n] = line;
                n++;
            }

            // Sync rest to UI thread
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await this.SwitchToMainThreadAsync();
                UpdateUI(_lineCounter, linesOfCodePerLanguageText.ToString(), extensionValues, extensionLabels);
            }).FileAndForget();
        }

        private void UpdateUI(LineCounter lineCounter, string linesOfCodePerLanguageText, decimal[] extensionValues,
                              string[] extensionLabels)
        {
            TotalLinesOfTestCode.Text = string.Format(_linesOfTestCode.Text, lineCounter.TestCodeLineCount);

            TestCodePie.SetValues(new decimal[]
                {
                    lineCounter.TestCodeLineCount,
                    lineCounter.CodeLineCount - lineCounter.TestCodeLineCount
                });

            var percentTest = (double)lineCounter.TestCodeLineCount / lineCounter.CodeLineCount;
            var percentProd = (double)(lineCounter.CodeLineCount - lineCounter.TestCodeLineCount) / lineCounter.CodeLineCount;
            TestCodePie.ToolTips =
                new[]
                    {
                        string.Format(_linesOfTestCodeP.Text, lineCounter.TestCodeLineCount, percentTest),
                        string.Format(_linesOfProductionCodeP.Text, lineCounter.CodeLineCount - lineCounter.TestCodeLineCount, percentProd)
                    };

            TestCodeText.Text = string.Format(_linesOfTestCodeP.Text, lineCounter.TestCodeLineCount, percentTest) + Environment.NewLine +
                string.Format(_linesOfProductionCodeP.Text, lineCounter.CodeLineCount - lineCounter.TestCodeLineCount, percentProd);

            var percentBlank = (double)lineCounter.BlankLineCount / lineCounter.TotalLineCount;
            var percentComments = (double)lineCounter.CommentLineCount / lineCounter.TotalLineCount;
            var percentCode = (double)lineCounter.CodeLineCount / lineCounter.TotalLineCount;
            var percentDesigner = (double)lineCounter.DesignerLineCount / lineCounter.TotalLineCount;
            LinesOfCodePie.SetValues(new decimal[]
                {
                    lineCounter.BlankLineCount,
                    lineCounter.CommentLineCount,
                    lineCounter.CodeLineCount,
                    lineCounter.DesignerLineCount
                });

            LinesOfCodePie.ToolTips =
                new[]
                    {
                        string.Format(_blankLinesP.Text, lineCounter.BlankLineCount, percentBlank),
                        string.Format(_commentLinesP.Text, lineCounter.CommentLineCount, percentComments),
                        string.Format(_linesOfCodeP.Text, lineCounter.CodeLineCount, percentCode),
                        string.Format(_linesOfDesignerFilesP.Text, lineCounter.DesignerLineCount, percentDesigner)
                    };

            LinesOfCodePerTypeText.Text = string.Join(Environment.NewLine, LinesOfCodePie.ToolTips);

            LinesOfCodePerLanguageText.Text = linesOfCodePerLanguageText;

            LinesOfCodeExtensionPie.SetValues(extensionValues);
            LinesOfCodeExtensionPie.ToolTips = extensionLabels;

            TotalLinesOfCode2.Text = TotalLinesOfCode.Text = string.Format(_linesOfCode.Text, lineCounter.CodeLineCount);
        }

        private void FormGitStatisticsShown(object sender, EventArgs e)
        {
            Initialize();

            Tabs.Visible = true;
            LoadingLabel.Visible = false;

            FormGitStatisticsSizeChanged(null, null);
            SizeChanged += FormGitStatisticsSizeChanged;
        }

        private void TabsSelectedIndexChanged(object sender, EventArgs e)
        {
            FormGitStatisticsSizeChanged(null, null);
        }

        private void FormGitStatistics_FormClosing(object sender, FormClosingEventArgs e)
        {
            _lineCounter.Updated -= OnLineCounterUpdated;
        }
    }
}
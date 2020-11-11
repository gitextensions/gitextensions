using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitExtensions.Core.Module;
using GitExtensions.Core.Utils.UI;
using GitStatistics.PieChart;
using GitStatistics.Properties;
using Microsoft.VisualStudio.Threading;

namespace GitStatistics
{
    public partial class FormGitStatistics : Form
    {
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

            Text = Strings.FormText;
            CommitStatistics.Text = Strings.CommitStatisticsText;
            LinesOfCodePerLanguageText.Text = Strings.LinesOfCodePerLanguageText;
            LinesOfCodePerTypeText.Text = Strings.LinesOfCodePerTypeText;
            LoadingLabel.Text = Strings.LoadingLabelText;
            TestCodeText.Text = Strings.TestCodeText;
            TotalCommits.Text = Strings.TotalCommitsText;
            TotalLinesOfCode.Text = Strings.TotalLinesOfCodeText;
            TotalLinesOfCode2.Text = Strings.TotalLinesOfCode2Text;
            TotalLinesOfTestCode.Text = Strings.TotalLinesOfTestCodeText;
            tabPage1.Text = Strings.TabPage1Text;
            tabPage2.Text = Strings.TabPage2Text;
            tabPage3.Text = Strings.TabPage3Text;
            tabPage4.Text = Strings.TabPage4Text;
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

                    TotalCommits.Text = string.Format(Strings.CommitsText, totalCommits);

                    var builder = new StringBuilder();

                    var commitCountValues = new decimal[commitsPerUser.Count];
                    var commitCountLabels = new string[commitsPerUser.Count];
                    var n = 0;
                    foreach (var item in commitsPerUser)
                    {
                        builder.AppendLine($"{item.Value:N0} {item.Key}");

                        commitCountValues[n] = item.Value;
                        commitCountLabels[n] = string.Format(Strings.CommitsByText, item.Value, item.Key);
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
                    .Where(submodule => submodule != null)
                    .Select(submodule => _module.GetSubmodule(submodule.Name));

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
            foreach (var item in linesOfCodePerExtension)
            {
                var percent = (double)item.Value / _lineCounter.CodeLineCount;
                var line = string.Format(Strings.LinesOfCodeInFilesText, item.Value, item.Key, percent);
                linesOfCodePerLanguageText.AppendLine(line);
                extensionValues[n] = item.Value;
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
            TotalLinesOfTestCode.Text = string.Format(Strings.LinesOfTestCodeText, lineCounter.TestCodeLineCount);

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
                        string.Format(Strings.LinesOfTestCodePText, lineCounter.TestCodeLineCount, percentTest),
                        string.Format(Strings.LinesOfProductionCodePText, lineCounter.CodeLineCount - lineCounter.TestCodeLineCount, percentProd)
                    };

            TestCodeText.Text = string.Format(Strings.LinesOfTestCodePText, lineCounter.TestCodeLineCount, percentTest) + Environment.NewLine +
                string.Format(Strings.LinesOfProductionCodePText, lineCounter.CodeLineCount - lineCounter.TestCodeLineCount, percentProd);

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
                        string.Format(Strings.BlankLinesPText, lineCounter.BlankLineCount, percentBlank),
                        string.Format(Strings.CommentLinesPText, lineCounter.CommentLineCount, percentComments),
                        string.Format(Strings.LinesOfCodePText, lineCounter.CodeLineCount, percentCode),
                        string.Format(Strings.LinesOfDesignerFilesPText, lineCounter.DesignerLineCount, percentDesigner)
                    };

            LinesOfCodePerTypeText.Text = string.Join(Environment.NewLine, LinesOfCodePie.ToolTips);

            LinesOfCodePerLanguageText.Text = linesOfCodePerLanguageText;

            LinesOfCodeExtensionPie.SetValues(extensionValues);
            LinesOfCodeExtensionPie.ToolTips = extensionLabels;

            TotalLinesOfCode2.Text = TotalLinesOfCode.Text = string.Format(Strings.LinesOfCodeText, lineCounter.CodeLineCount);
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

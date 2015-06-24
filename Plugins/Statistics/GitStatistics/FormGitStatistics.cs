using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitStatistics.PieChart;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitStatistics
{
    public partial class FormGitStatistics : GitExtensionsFormBase
    {
        private readonly TranslationString _commits = new TranslationString("{0} Commits");
        private readonly TranslationString _commitsBy = new TranslationString("{0} Commits by {1}");
        private readonly TranslationString _linesOfCodeInFiles = new TranslationString("{0} Lines of code in {1} files ({2})");
        private readonly TranslationString _linesOfCode = new TranslationString("{0} Lines of code");
        private readonly TranslationString _linesOfCodeP = new TranslationString("{0} Lines of code ({1})");
        private readonly TranslationString _linesOfTestCode = new TranslationString("{0} Lines of test code");
        private readonly TranslationString _linesOfTestCodeP = new TranslationString("{0} Lines of test code ({1})");
        private readonly TranslationString _linesOfProductionCodeP = new TranslationString("{0} Lines of production code ({1})");
        private readonly TranslationString _blankLinesP = new TranslationString("{0} Blank lines ({1})");
        private readonly TranslationString _commentLinesP = new TranslationString("{0} Comment lines ({1})");
        private readonly TranslationString _linesOfDesignerFilesP = new TranslationString("{0} Lines in designer files ({1})");

        private readonly string _codeFilePattern;

        protected Color[] DecentColors =
            new[]
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
        public DirectoryInfo WorkingDir { get; set; }
        private readonly SynchronizationContext _syncContext;
        private LineCounter _lineCounter;
        private Task _loadThread;
        private readonly IGitModule _module;

        public FormGitStatistics(IGitModule aModule, string codeFilePattern)
        {
            _module = aModule;
            _codeFilePattern = codeFilePattern;
            InitializeComponent();
            Translate();

            TotalLinesOfCode.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            TotalLinesOfCode2.Font = TotalLinesOfCode.Font;
            TotalLinesOfTestCode.Font = TotalLinesOfCode.Font;
            TotalCommits.Font = TotalLinesOfCode.Font;
            LoadingLabel.Font = TotalLinesOfCode.Font;

            _syncContext = SynchronizationContext.Current;
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
            Action<FormGitStatistics> a = sender =>
            {
                var allCommitsByUser = CommitCounter.GroupAllCommitsByContributor(_module);
                _syncContext.Post(o =>
                {
                    if (this.IsDisposed)
                        return;
                    var totalCommits = allCommitsByUser.Item2;
                    var commitsPerUser = allCommitsByUser.Item1;

                    TotalCommits.Text = string.Format(_commits.Text, totalCommits);

                    var builder = new StringBuilder();

                    var commitCountValues = new Decimal[commitsPerUser.Count];
                    var commitCountLabels = new string[commitsPerUser.Count];
                    var n = 0;
                    foreach (var keyValuePair in commitsPerUser)
                    {
                        var user = keyValuePair.Key;
                        var commits = keyValuePair.Value;

                        builder.AppendLine(commits + " " + user);

                        commitCountValues[n] = commits;
                        commitCountLabels[n] = string.Format(_commitsBy.Text, commits, user);
                        n++;
                    }
                    CommitCountPie.SetValues(commitCountValues);
                    CommitCountPie.ToolTips = commitCountLabels;

                    CommitStatistics.Text = builder.ToString();

                }, null);
            };
            a.BeginInvoke(null, null, this);
        }

        private void SetPieStyle(PieChartControl pie)
        {
            pie.SetLeftMargin(10);
            pie.SetRightMargin(10);
            pie.SetTopMargin(10);
            pie.SetBottomMargin(10);
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

        bool _initializeLinesOfCodeDone;
        private void InitializeLinesOfCode()
        {
            if (_initializeLinesOfCodeDone)
                return;

            _initializeLinesOfCodeDone = true;

            _lineCounter = new LineCounter(WorkingDir);
            _lineCounter.LinesOfCodeUpdated += lineCounter_LinesOfCodeUpdated;

            _loadThread = Task.Factory.StartNew(LoadLinesOfCode);
        }

        public void LoadLinesOfCode()
        {
            _lineCounter.FindAndAnalyzeCodeFiles(_codeFilePattern, DirectoriesToIgnore);
        }

        void lineCounter_LinesOfCodeUpdated(object sender, EventArgs e)
        {
            LineCounter lineCounter = (LineCounter)sender;

            //Must do this synchronously because lineCounter.LinesOfCodePerExtension might change while we are iterating over it otherwise.
            var extensionValues = new decimal[lineCounter.LinesOfCodePerExtension.Count];
            var extensionLabels = new string[lineCounter.LinesOfCodePerExtension.Count];

            var linesOfCodePerExtension = new List<KeyValuePair<string, int>>(lineCounter.LinesOfCodePerExtension);
            linesOfCodePerExtension.Sort((first, next) => -first.Value.CompareTo(next.Value));

            var n = 0;
            string linesOfCodePerLanguageText = "";
            foreach (var keyValuePair in linesOfCodePerExtension)
            {
                string percent = ((double)keyValuePair.Value / lineCounter.NumberCodeLines).ToString("P1");
                linesOfCodePerLanguageText += string.Format(_linesOfCodeInFiles.Text, keyValuePair.Value, keyValuePair.Key, percent) + Environment.NewLine;
                extensionValues[n] = keyValuePair.Value;
                extensionLabels[n] = string.Format(_linesOfCodeInFiles.Text, keyValuePair.Value, keyValuePair.Key, percent);
                n++;
            }

            //Sync rest to UI thread
            _syncContext.Post(o => UpdateUI(lineCounter, linesOfCodePerLanguageText, extensionValues, extensionLabels), null);
        }

        private void UpdateUI(LineCounter lineCounter, string linesOfCodePerLanguageText, decimal[] extensionValues,
                              string[] extensionLabels)
        {
            TotalLinesOfTestCode.Text = string.Format(_linesOfTestCode.Text, lineCounter.NumberTestCodeLines);

            TestCodePie.SetValues(new decimal[]
                {
                    lineCounter.NumberTestCodeLines,
                    lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines
                });

            string percent_t = ((double) lineCounter.NumberTestCodeLines/lineCounter.NumberCodeLines).ToString("P1");
            string percent_p =
                ((double) (lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines)/lineCounter.NumberCodeLines).ToString(
                    "P1");
            TestCodePie.ToolTips =
                new[]
                    {
                        string.Format(_linesOfTestCodeP.Text, lineCounter.NumberTestCodeLines, percent_t),
                        string.Format(_linesOfProductionCodeP.Text, lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines, percent_p)
                    };

            TestCodeText.Text = string.Format(_linesOfTestCodeP.Text, lineCounter.NumberTestCodeLines, percent_t) + Environment.NewLine +
                string.Format(_linesOfProductionCodeP.Text, (lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines), percent_p);


            string percentBlank = ((double) lineCounter.NumberBlankLines/lineCounter.NumberLines).ToString("P1");
            string percentComments = ((double) lineCounter.NumberCommentsLines/lineCounter.NumberLines).ToString("P1");
            string percentCode = ((double) lineCounter.NumberCodeLines/lineCounter.NumberLines).ToString("P1");
            string percentDesigner = ((double) lineCounter.NumberLinesInDesignerFiles/lineCounter.NumberLines).ToString("P1");
            LinesOfCodePie.SetValues(new decimal[]
                {
                    lineCounter.NumberBlankLines,
                    lineCounter.NumberCommentsLines,
                    lineCounter.NumberLines,
                    lineCounter.NumberLinesInDesignerFiles
                });

            LinesOfCodePie.ToolTips =
                new[]
                    {
                        string.Format(_blankLinesP.Text, lineCounter.NumberBlankLines, percentBlank),
                        string.Format(_commentLinesP.Text, lineCounter.NumberCommentsLines, percentComments),
                        string.Format(_linesOfCodeP.Text, lineCounter.NumberCodeLines, percentCode),
                        string.Format(_linesOfDesignerFilesP.Text, lineCounter.NumberLinesInDesignerFiles, percentDesigner)
                    };

            LinesOfCodePerTypeText.Text = LinesOfCodePie.ToolTips[0] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[1] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[2] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[3] + Environment.NewLine;

            LinesOfCodePerLanguageText.Text = linesOfCodePerLanguageText;

            LinesOfCodeExtensionPie.SetValues(extensionValues);
            LinesOfCodeExtensionPie.ToolTips = extensionLabels;

            TotalLinesOfCode2.Text = TotalLinesOfCode.Text = string.Format(_linesOfCode.Text, lineCounter.NumberCodeLines);
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
            _lineCounter.LinesOfCodeUpdated -= lineCounter_LinesOfCodeUpdated;
        }
    }
}
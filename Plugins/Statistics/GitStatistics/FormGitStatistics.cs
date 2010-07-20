using System;
using System.Drawing;
using System.Drawing.PieChart;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitStatistics
{
    public partial class FormGitStatistics : Form
    {
        private readonly IGitUIEventArgs _gitUiEventArgs;
        public string CodeFilePattern;

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

        public string DirectoriesToIgnore;
        public DirectoryInfo WorkingDir;

        public FormGitStatistics(IGitUIEventArgs gitUiEventArgs)
        {
            _gitUiEventArgs = gitUiEventArgs;
            InitializeComponent();
        }

        private void FormGitStatisticsSizeChanged(object sender, EventArgs e)
        {
            SetPieStyle(CommitCountPie);
            SetPieStyle(LinesOfCodeExtensionPie);
            SetPieStyle(LinesOfCodePie);
            SetPieStyle(TestCodePie);
        }

        public void Initialize(IGitUIEventArgs gitUiEventArgs)
        {
            InitializeCommitCount(gitUiEventArgs);
            InitializeLinesOfCode();
        }

        private void InitializeCommitCount(IGitUIEventArgs gitUiEventArgs)
        {
            var commitCounter = new CommitCounter(gitUiEventArgs);
            commitCounter.Count();
            TotalCommits.Text = commitCounter.TotalCommits + " Commits";

            var commitStatisticsTest = new StringBuilder();


            var commitCountValues = new Decimal[commitCounter.UserCommitCount.Count];
            var commitCountLabels = new string[commitCounter.UserCommitCount.Count];
            var n = 0;
            foreach (var keyValuePair in commitCounter.UserCommitCount)
            {
                commitStatisticsTest.AppendLine(keyValuePair.Value + " " + keyValuePair.Key);

                commitCountValues[n] = keyValuePair.Value;
                commitCountLabels[n] = keyValuePair.Value + " Commits by " + keyValuePair.Key;
                n++;
            }
            CommitCountPie.Values = commitCountValues;
            CommitCountPie.ToolTips = commitCountLabels;


            CommitStatistics.Text = commitStatisticsTest.ToString();
        }

        private void SetPieStyle(PieChartControl pie)
        {
            pie.LeftMargin = 10;
            pie.RightMargin = 10;
            pie.TopMargin = 10;
            pie.BottomMargin = 10;
            pie.FitChart = false;
            pie.EdgeColorType = EdgeColorType.DarkerThanSurface;
            pie.InitialAngle = -30;
            pie.SliceRelativeHeight = 0.20f;
            pie.Colors = DecentColors;
            pie.ShadowStyle = ShadowStyle.GradualShadow;
            pie.Dock = DockStyle.None;
            pie.Height = pie.Parent.Height;
            pie.Width = pie.Parent.Width;
            if (pie.Width > pie.Height)
                pie.Width = pie.Height;
            else
                pie.Height = pie.Width;
        }

        private void InitializeLinesOfCode()
        {
            var lineCounter = new LineCounter(WorkingDir);
            lineCounter.Count(CodeFilePattern, DirectoriesToIgnore);

            TotalLinesOfTestCode.Text = lineCounter.NumberTestCodeLines + " Lines of test code";

            TestCodePie.Values =
                new Decimal[]
                    {
                        lineCounter.NumberTestCodeLines,
                        lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines
                    };
            TestCodePie.ToolTips =
                new[]
                    {
                        lineCounter.NumberTestCodeLines + " Lines of testcode",
                        lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines +
                        " Lines of production code"
                    };

            TestCodeText.Text = lineCounter.NumberTestCodeLines + " Lines of testcode" + Environment.NewLine +
                                (lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines) +
                                " Lines of production code";

            LinesOfCodePie.Values =
                new Decimal[]
                    {
                        lineCounter.NumberBlankLines,
                        lineCounter.NumberCommentsLines,
                        lineCounter.NumberLines,
                        lineCounter.NumberLinesInDesignerFiles
                    };
            LinesOfCodePie.ToolTips =
                new[]
                    {
                        lineCounter.NumberBlankLines + " Blank lines",
                        lineCounter.NumberCommentsLines + " Comment lines",
                        lineCounter.NumberCodeLines + " Lines of code",
                        lineCounter.NumberLinesInDesignerFiles + " Lines in designer files"
                    };

            LinesOfCodePerTypeText.Text = LinesOfCodePie.ToolTips[0] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[1] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[2] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[3] + Environment.NewLine;

            var extensionValues = new Decimal[lineCounter.LinesOfCodePerExtension.Count];
            var extensionLabels = new string[lineCounter.LinesOfCodePerExtension.Count];
            var n = 0;
            LinesOfCodePerLanguageText.Text = "";
            foreach (var keyValuePair in lineCounter.LinesOfCodePerExtension)
            {
                LinesOfCodePerLanguageText.Text += keyValuePair.Value + " Lines of code in " + keyValuePair.Key +
                                                   " files" + Environment.NewLine;
                extensionValues[n] = keyValuePair.Value;
                extensionLabels[n] = keyValuePair.Value + " Lines of code in " + keyValuePair.Key + " files";
                n++;
            }

            LinesOfCodeExtensionPie.Values = extensionValues;
            LinesOfCodeExtensionPie.ToolTips = extensionLabels;

            TotalLinesOfCode2.Text = TotalLinesOfCode.Text = lineCounter.NumberCodeLines + " Lines of code";
        }

        private void FormGitStatisticsShown(object sender, EventArgs e)
        {
            Initialize(_gitUiEventArgs);

            Tabs.Visible = true;
            LoadingLabel.Visible = false;


            FormGitStatisticsSizeChanged(null, null);
            SizeChanged += FormGitStatisticsSizeChanged;
        }


        private void TabsSelectedIndexChanged(object sender, EventArgs e)
        {
            FormGitStatisticsSizeChanged(null, null);
        }
    }
}
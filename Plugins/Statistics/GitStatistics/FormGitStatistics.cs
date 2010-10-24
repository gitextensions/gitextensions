using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitStatistics.PieChart;

namespace GitStatistics
{
    public partial class FormGitStatistics : Form
    {
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

        public string DirectoriesToIgnore;
        public DirectoryInfo WorkingDir;

        public FormGitStatistics(string codeFilePattern)
        {
            _codeFilePattern = codeFilePattern;
            InitializeComponent();

            TotalLinesOfCode.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            TotalLinesOfCode2.Font = TotalLinesOfCode.Font;
            TotalLinesOfTestCode.Font = TotalLinesOfCode.Font;
            TotalCommits.Font = TotalLinesOfCode.Font;
            LoadingLabel.Font = TotalLinesOfCode.Font;
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
            var allCommitsByUser = CommitCounter.GroupAllCommitsByContributor();
            var totalCommits = allCommitsByUser.Item2;
            var commitsPerUser = allCommitsByUser.Item1;

            TotalCommits.Text = totalCommits + " Commits";

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
                commitCountLabels[n] = commits + " Commits by " + user;
                n++;
            }
            CommitCountPie.SetValues(commitCountValues);
            CommitCountPie.ToolTips = commitCountLabels;

            CommitStatistics.Text = builder.ToString();
        }

        private void SetPieStyle(PieChartControl pie)
        {
            pie.SetLeftMargin(10);
            pie.SetRightMargin(10);
            pie.SetTopMargin(10);
            pie.SetBottomMargin(10);
            pie.SetFitChart(false);
            pie.SetEdgeColorType(EdgeColorType.DarkerThanSurface);
            pie.InitialAngle = -30;
            pie.SetSliceRelativeHeight(0.20f);
            pie.SetColors(DecentColors);
            pie.SetShadowStyle(ShadowStyle.GradualShadow);
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

            lineCounter.FindAndAnalyzeCodeFiles(_codeFilePattern, DirectoriesToIgnore);

            TotalLinesOfTestCode.Text = lineCounter.NumberTestCodeLines + " Lines of test code";

            TestCodePie.SetValues(new Decimal[]
                                      {
                                          lineCounter.NumberTestCodeLines,
                                          lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines
                                      });
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

            LinesOfCodePie.SetValues(new Decimal[]
                                         {
                                             lineCounter.NumberBlankLines,
                                             lineCounter.NumberCommentsLines,
                                             lineCounter.NumberLines,
                                             lineCounter.NumberLinesInDesignerFiles
                                         });
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

            LinesOfCodeExtensionPie.SetValues(extensionValues);
            LinesOfCodeExtensionPie.ToolTips = extensionLabels;

            TotalLinesOfCode2.Text = TotalLinesOfCode.Text = lineCounter.NumberCodeLines + " Lines of code";
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
    }
}
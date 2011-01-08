using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitStatistics.PieChart;
using System.Threading;
using System.Collections.Generic;

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
        private SynchronizationContext syncContext;
        private LineCounter lineCounter;
        private Thread loadThread;

        public FormGitStatistics(string codeFilePattern)
        {
            _codeFilePattern = codeFilePattern;
            InitializeComponent();

            TotalLinesOfCode.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            TotalLinesOfCode2.Font = TotalLinesOfCode.Font;
            TotalLinesOfTestCode.Font = TotalLinesOfCode.Font;
            TotalCommits.Font = TotalLinesOfCode.Font;
            LoadingLabel.Font = TotalLinesOfCode.Font;

            syncContext = SynchronizationContext.Current;
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
            Action<FormGitStatistics> a = (FormGitStatistics sender) =>
            {
                var allCommitsByUser = CommitCounter.GroupAllCommitsByContributor();
                syncContext.Post(o =>
                {
                    if (this.IsDisposed)
                        return;
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

        bool initializeLinesOfCodeDone = false;
        private void InitializeLinesOfCode()
        {
            if (initializeLinesOfCodeDone)
                return;
            
            initializeLinesOfCodeDone = true;

            lineCounter = new LineCounter(WorkingDir);
            lineCounter.LinesOfCodeUpdated += new EventHandler(lineCounter_LinesOfCodeUpdated);

            loadThread = new Thread(new ThreadStart(LoadLinesOfCode));

            loadThread.Start();
        }
        
        public void LoadLinesOfCode()
        {
            lineCounter.FindAndAnalyzeCodeFiles(_codeFilePattern, DirectoriesToIgnore);
        }

        void lineCounter_LinesOfCodeUpdated(object sender, EventArgs e)
        {
            LineCounter lineCounter = (LineCounter)sender;

            //Must do this synchronously becuase lineCounter.LinesOfCodePerExtension might change while we are iterating over it otherwise.
            var extensionValues = new Decimal[lineCounter.LinesOfCodePerExtension.Count];
            var extensionLabels = new string[lineCounter.LinesOfCodePerExtension.Count];

            List<KeyValuePair<string, int>> LinesOfCodePerExtension = new List<KeyValuePair<string, int>>(lineCounter.LinesOfCodePerExtension);
            LinesOfCodePerExtension.Sort(
                delegate(KeyValuePair<string, int> first, KeyValuePair<string, int> next)
                {
                    return -first.Value.CompareTo(next.Value);
                }
            );

            var n = 0;
            string linesOfCodePerLanguageText = "";
            foreach (var keyValuePair in LinesOfCodePerExtension)
            {
                string percent = ((double)keyValuePair.Value / lineCounter.NumberCodeLines).ToString("P1");
                linesOfCodePerLanguageText += keyValuePair.Value + " Lines of code in " + keyValuePair.Key + " files (" + percent + ")" + Environment.NewLine;
                extensionValues[n] = keyValuePair.Value;
                extensionLabels[n] = keyValuePair.Value + " Lines of code in " + keyValuePair.Key + " files (" + percent + ")";
                n++;
            }

            //Sync rest to UI thread
            syncContext.Post((o) =>
            {
                TotalLinesOfTestCode.Text = lineCounter.NumberTestCodeLines + " Lines of test code";

                TestCodePie.SetValues(new Decimal[]
                                      {
                                          lineCounter.NumberTestCodeLines,
                                          lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines
                                      });
 
                string percent_t = ((double)lineCounter.NumberTestCodeLines / lineCounter.NumberCodeLines).ToString("P1");
                string percent_p = ((double)(lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines) / lineCounter.NumberCodeLines).ToString("P1");
                TestCodePie.ToolTips =
                    new[]
                    {
                        lineCounter.NumberTestCodeLines + " Lines of testcode (" + percent_t + ")",
                        lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines +
                        " Lines of production code (" + percent_p + ")"
                    };

                TestCodeText.Text = lineCounter.NumberTestCodeLines + " Lines of testcode (" + percent_t + ")" + Environment.NewLine +
                                    (lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines) +
                                    " Lines of production code (" + percent_p + ")";


                string percent_blank = ((double)lineCounter.NumberBlankLines / lineCounter.NumberLines).ToString("P1");
                string percent_comments = ((double)lineCounter.NumberCommentsLines / lineCounter.NumberLines).ToString("P1");
                string percent_code = ((double)lineCounter.NumberCodeLines / lineCounter.NumberLines).ToString("P1");
                string percent_designer = ((double)lineCounter.NumberLinesInDesignerFiles / lineCounter.NumberLines).ToString("P1");
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
                        lineCounter.NumberBlankLines + " Blank lines (" + percent_blank + ")",
                        lineCounter.NumberCommentsLines + " Comment lines (" + percent_comments + ")",
                        lineCounter.NumberCodeLines + " Lines of code (" + percent_code + ")",
                        lineCounter.NumberLinesInDesignerFiles + " Lines in designer files (" + percent_designer + ")"
                    };

                LinesOfCodePerTypeText.Text = LinesOfCodePie.ToolTips[0] + Environment.NewLine;
                LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[1] + Environment.NewLine;
                LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[2] + Environment.NewLine;
                LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[3] + Environment.NewLine;

                LinesOfCodePerLanguageText.Text = linesOfCodePerLanguageText;

                LinesOfCodeExtensionPie.SetValues(extensionValues);
                LinesOfCodeExtensionPie.ToolTips = extensionLabels;

                TotalLinesOfCode2.Text = TotalLinesOfCode.Text = lineCounter.NumberCodeLines + " Lines of code";
            }, null);
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
            try
            {
                if (loadThread != null)
                    loadThread.Abort();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
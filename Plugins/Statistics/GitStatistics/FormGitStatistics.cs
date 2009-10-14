using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitUIPluginInterfaces;
using System.Threading;

namespace GitStatistics
{
    public partial class FormGitStatistics : Form
    {
        public FormGitStatistics(IGitUIEventArgs gitUIEventArgs)
        {
            this.GitUIEventArgs = gitUIEventArgs;
            InitializeComponent();
        }

        private IGitUIEventArgs GitUIEventArgs;
        public DirectoryInfo WorkingDir;
        public string CodeFilePattern;
        public string DirectoriesToIgnore;

        protected Color[] decentColors = new Color[]{ Color.Red, 
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
																	  Color.Purple };

        private void FormGitStatistics_Load(object sender, EventArgs e)
        {

        }

        void FormGitStatistics_SizeChanged(object sender, EventArgs e)
        {
            SetPieStyle(CommitCountPie);
            SetPieStyle(LinesOfCodeExtensionPie);
            SetPieStyle(LinesOfCodePie);
            SetPieStyle(TestCodePie);
        }

        public void Initialize(IGitUIEventArgs gitUIEventArgs)
        {
            InitializeCommitCount(gitUIEventArgs);
            InitializeLinesOfCode();
        }

        private void InitializeCommitCount(IGitUIEventArgs gitUIEventArgs)
        {
            CommitCounter commitCounter = new CommitCounter(gitUIEventArgs);
            commitCounter.Count();
            TotalCommits.Text = commitCounter.TotalCommits + " Commits";

            StringBuilder commitStatisticsTest = new StringBuilder();


            Decimal[] commitCountValues = new Decimal[commitCounter.UserCommitCount.Count];
            string[] CommitCountLabels = new string[commitCounter.UserCommitCount.Count];
            int n = 0;
            foreach (KeyValuePair<string, int> keyValuePair in commitCounter.UserCommitCount)
            {
                commitStatisticsTest.AppendLine(keyValuePair.Value + " " + keyValuePair.Key);

                commitCountValues[n] = keyValuePair.Value;
                CommitCountLabels[n] = keyValuePair.Value + " Commits by " + keyValuePair.Key;
                n++;
            }
            CommitCountPie.Values = commitCountValues;
            CommitCountPie.ToolTips = CommitCountLabels;

            

            CommitStatistics.Text = commitStatisticsTest.ToString();
        }

        private void SetPieStyle(System.Drawing.PieChart.PieChartControl pie)
        {
            pie.LeftMargin = 10;
            pie.RightMargin = 10;
            pie.TopMargin = 10;
            pie.BottomMargin = 10;
            pie.FitChart = false;
            pie.EdgeColorType = System.Drawing.PieChart.EdgeColorType.DarkerThanSurface;
            pie.InitialAngle = -30;
            pie.SliceRelativeHeight = 0.20f;
            pie.Colors = decentColors;
            pie.ShadowStyle = System.Drawing.PieChart.ShadowStyle.GradualShadow;
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
            LineCounter lineCounter = new LineCounter(WorkingDir);
            lineCounter.Count(CodeFilePattern, DirectoriesToIgnore);

            TotalLinesOfTestCode.Text = lineCounter.NumberTestCodeLines + " Lines of test code";

            TestCodePie.Values = new Decimal[] { lineCounter.NumberTestCodeLines, 
                                                 lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines};
            TestCodePie.ToolTips = new string[] { lineCounter.NumberTestCodeLines + " Lines of testcode", 
                                                  lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines + " Lines of production code"};

            TestCodeText.Text = lineCounter.NumberTestCodeLines + " Lines of testcode" + Environment.NewLine + (lineCounter.NumberCodeLines - lineCounter.NumberTestCodeLines) + " Lines of production code";

            LinesOfCodePie.Values = new Decimal[] { lineCounter.NumberBlankLines, 
                                                    lineCounter.NumberCommentsLines, 
                                                    lineCounter.NumberLines, 
                                                    lineCounter.NumberLinesInDesignerFiles };
            LinesOfCodePie.ToolTips = new string[] { lineCounter.NumberBlankLines + " Blank lines",
                                                     lineCounter.NumberCommentsLines + " Comment lines",
                                                     lineCounter.NumberCodeLines + " Lines of code",
                                                     lineCounter.NumberLinesInDesignerFiles + " Lines in designer files"};

            LinesOfCodePerTypeText.Text  = LinesOfCodePie.ToolTips[0] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[1] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[2] + Environment.NewLine;
            LinesOfCodePerTypeText.Text += LinesOfCodePie.ToolTips[3] + Environment.NewLine;

            Decimal[] extensionValues = new Decimal[lineCounter.LinesOfCodePerExtension.Count];
            string[] extensionLabels = new string[lineCounter.LinesOfCodePerExtension.Count];
            int n = 0;
            LinesOfCodePerLanguageText.Text = "";
            foreach (KeyValuePair<string, int> keyValuePair in lineCounter.LinesOfCodePerExtension)
            {
                LinesOfCodePerLanguageText.Text += keyValuePair.Value + " Lines of code in " + keyValuePair.Key + " files" + Environment.NewLine;
                extensionValues[n] = keyValuePair.Value;
                extensionLabels[n] = keyValuePair.Value + " Lines of code in " + keyValuePair.Key + " files";
                n++;
            }

            LinesOfCodeExtensionPie.Values = extensionValues;
            LinesOfCodeExtensionPie.ToolTips = extensionLabels;

            TotalLinesOfCode2.Text = TotalLinesOfCode.Text = lineCounter.NumberCodeLines + " Lines of code";
        }

        void directoryLineCounter_FilesCompleted(object sender, int filesCompleted)
        {
        }

        private void FormGitStatistics_Shown(object sender, EventArgs e)
        {
            Initialize(GitUIEventArgs);

            Tabs.Visible = true;
            LoadingLabel.Visible = false;
                      

            FormGitStatistics_SizeChanged(null, null);
            SizeChanged += new EventHandler(FormGitStatistics_SizeChanged);
        }

        private void Tabs_TabIndexChanged(object sender, EventArgs e)
        {

        }

        private void Tabs_Selected(object sender, TabControlEventArgs e)
        {
        }

        private void Tabs_Selecting(object sender, TabControlCancelEventArgs e)
        {


        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            FormGitStatistics_SizeChanged(null, null);
        }
    }
}

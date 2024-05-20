using GitExtensions.Plugins.GitStatistics.PieChart;

namespace GitExtensions.Plugins.GitStatistics
{
    partial class FormGitStatistics
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            Tabs = new GitUI.CommandsDialogs.FullBleedTabControl();
            tabPage2 = new TabPage();
            splitContainer3 = new SplitContainer();
            TotalCommits = new Label();
            splitContainer4 = new SplitContainer();
            CommitStatistics = new Label();
            CommitCountPie = new global::GitExtensions.Plugins.GitStatistics.PieChart.PieChartControl();
            tabPage1 = new TabPage();
            splitContainer2 = new SplitContainer();
            TotalLinesOfCode = new Label();
            splitContainer1 = new SplitContainer();
            LinesOfCodePerLanguageText = new Label();
            LinesOfCodeExtensionPie = new global::GitExtensions.Plugins.GitStatistics.PieChart.PieChartControl();
            tabPage3 = new TabPage();
            splitContainer5 = new SplitContainer();
            TotalLinesOfCode2 = new Label();
            splitContainer6 = new SplitContainer();
            LinesOfCodePerTypeText = new Label();
            LinesOfCodePie = new global::GitExtensions.Plugins.GitStatistics.PieChart.PieChartControl();
            tabPage4 = new TabPage();
            splitContainer7 = new SplitContainer();
            TotalLinesOfTestCode = new Label();
            splitContainer8 = new SplitContainer();
            TestCodeText = new Label();
            TestCodePie = new global::GitExtensions.Plugins.GitStatistics.PieChart.PieChartControl();
            LoadingLabel = new Label();
            Tabs.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer3)).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer4)).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer5)).BeginInit();
            splitContainer5.Panel1.SuspendLayout();
            splitContainer5.Panel2.SuspendLayout();
            splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer6)).BeginInit();
            splitContainer6.Panel1.SuspendLayout();
            splitContainer6.Panel2.SuspendLayout();
            splitContainer6.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer7)).BeginInit();
            splitContainer7.Panel1.SuspendLayout();
            splitContainer7.Panel2.SuspendLayout();
            splitContainer7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer8)).BeginInit();
            splitContainer8.Panel1.SuspendLayout();
            splitContainer8.Panel2.SuspendLayout();
            splitContainer8.SuspendLayout();
            SuspendLayout();
            // 
            // Tabs
            // 
            Tabs.Controls.Add(tabPage2);
            Tabs.Controls.Add(tabPage1);
            Tabs.Controls.Add(tabPage3);
            Tabs.Controls.Add(tabPage4);
            Tabs.Dock = DockStyle.Fill;
            Tabs.Location = new Point(0, 0);
            Tabs.Margin = new Padding(0);
            Tabs.Name = "Tabs";
            Tabs.Padding = new Point(0, 0);
            Tabs.SelectedIndex = 0;
            Tabs.Size = new Size(751, 465);
            Tabs.TabIndex = 0;
            Tabs.Visible = false;
            Tabs.SelectedIndexChanged += TabsSelectedIndexChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(splitContainer3);
            tabPage2.Location = new Point(4, 22);
            tabPage2.Margin = new Padding(0);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(743, 439);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Commits per contributor";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.FixedPanel = FixedPanel.Panel1;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Margin = new Padding(0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(TotalCommits);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(splitContainer4);
            splitContainer3.Size = new Size(737, 433);
            splitContainer3.SplitterDistance = 29;
            splitContainer3.TabIndex = 0;
            // 
            // TotalCommits
            // 
            TotalCommits.AutoSize = true;
            TotalCommits.Location = new Point(5, 2);
            TotalCommits.Margin = new Padding(0, 0, 0, 0);
            TotalCommits.Name = "TotalCommits";
            TotalCommits.Size = new Size(72, 13);
            TotalCommits.TabIndex = 1;
            TotalCommits.Text = "Total commits";
            // 
            // splitContainer4
            // 
            splitContainer4.Dock = DockStyle.Fill;
            splitContainer4.Location = new Point(0, 0);
            splitContainer4.Margin = new Padding(0, 0, 0, 0);
            splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.AutoScroll = true;
            splitContainer4.Panel1.Controls.Add(CommitStatistics);
            splitContainer4.Panel1MinSize = 250;
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.Controls.Add(CommitCountPie);
            splitContainer4.Size = new Size(737, 400);
            splitContainer4.SplitterDistance = 286;
            splitContainer4.TabIndex = 1;
            // 
            // CommitStatistics
            // 
            CommitStatistics.AutoSize = true;
            CommitStatistics.Location = new Point(9, 4);
            CommitStatistics.Margin = new Padding(0);
            CommitStatistics.Name = "CommitStatistics";
            CommitStatistics.Size = new Size(51, 13);
            CommitStatistics.TabIndex = 0;
            CommitStatistics.Text = "Loading..";
            // 
            // CommitCountPie
            // 
            CommitCountPie.Dock = DockStyle.None;
            CommitCountPie.InitialAngle = -30;
            CommitCountPie.Location = new Point(0, 0);
            CommitCountPie.Margin = new Padding(0);
            CommitCountPie.Name = "CommitCountPie";
            CommitCountPie.Size = new Size(447, 400);
            CommitCountPie.TabIndex = 0;
            CommitCountPie.ToolTips = null;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer2);
            tabPage1.Location = new Point(4, 22);
            tabPage1.Margin = new Padding(0, 0, 0, 0);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(743, 439);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Lines of code per language";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel1;
            splitContainer2.Location = new Point(3, 3);
            splitContainer2.Margin = new Padding(0, 0, 0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(TotalLinesOfCode);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainer1);
            splitContainer2.Size = new Size(737, 433);
            splitContainer2.SplitterDistance = 29;
            splitContainer2.TabIndex = 2;
            // 
            // TotalLinesOfCode
            // 
            TotalLinesOfCode.AutoSize = true;
            TotalLinesOfCode.Location = new Point(5, 2);
            TotalLinesOfCode.Margin = new Padding(0, 0, 0, 0);
            TotalLinesOfCode.Name = "TotalLinesOfCode";
            TotalLinesOfCode.Size = new Size(94, 13);
            TotalLinesOfCode.TabIndex = 0;
            TotalLinesOfCode.Text = "Total lines of code";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(0, 0, 0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(LinesOfCodePerLanguageText);
            splitContainer1.Panel1MinSize = 250;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(LinesOfCodeExtensionPie);
            splitContainer1.Size = new Size(737, 400);
            splitContainer1.SplitterDistance = 286;
            splitContainer1.TabIndex = 1;
            // 
            // LinesOfCodePerLanguageText
            // 
            LinesOfCodePerLanguageText.AutoSize = true;
            LinesOfCodePerLanguageText.Location = new Point(9, 4);
            LinesOfCodePerLanguageText.Margin = new Padding(0, 0, 0, 0);
            LinesOfCodePerLanguageText.Name = "LinesOfCodePerLanguageText";
            LinesOfCodePerLanguageText.Size = new Size(54, 13);
            LinesOfCodePerLanguageText.TabIndex = 1;
            LinesOfCodePerLanguageText.Text = "Loading...";
            // 
            // LinesOfCodeExtensionPie
            // 
            LinesOfCodeExtensionPie.Dock = DockStyle.None;
            LinesOfCodeExtensionPie.InitialAngle = -30;
            LinesOfCodeExtensionPie.Location = new Point(0, 0);
            LinesOfCodeExtensionPie.Margin = new Padding(0, 0, 0, 0);
            LinesOfCodeExtensionPie.Name = "LinesOfCodeExtensionPie";
            LinesOfCodeExtensionPie.Size = new Size(447, 400);
            LinesOfCodeExtensionPie.TabIndex = 0;
            LinesOfCodeExtensionPie.ToolTips = null;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(splitContainer5);
            tabPage3.Location = new Point(4, 22);
            tabPage3.Margin = new Padding(0, 0, 0, 0);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(743, 439);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Lines of code per type";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // splitContainer5
            // 
            splitContainer5.Dock = DockStyle.Fill;
            splitContainer5.FixedPanel = FixedPanel.Panel1;
            splitContainer5.Location = new Point(0, 0);
            splitContainer5.Margin = new Padding(0, 0, 0, 0);
            splitContainer5.Name = "splitContainer5";
            splitContainer5.Orientation = Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            splitContainer5.Panel1.Controls.Add(TotalLinesOfCode2);
            // 
            // splitContainer5.Panel2
            // 
            splitContainer5.Panel2.Controls.Add(splitContainer6);
            splitContainer5.Size = new Size(737, 433);
            splitContainer5.SplitterDistance = 29;
            splitContainer5.TabIndex = 0;
            // 
            // TotalLinesOfCode2
            // 
            TotalLinesOfCode2.AutoSize = true;
            TotalLinesOfCode2.Location = new Point(5, 2);
            TotalLinesOfCode2.Margin = new Padding(0, 0, 0, 0);
            TotalLinesOfCode2.Name = "TotalLinesOfCode2";
            TotalLinesOfCode2.Size = new Size(94, 13);
            TotalLinesOfCode2.TabIndex = 1;
            TotalLinesOfCode2.Text = "Total lines of code";
            // 
            // splitContainer6
            // 
            splitContainer6.Dock = DockStyle.Fill;
            splitContainer6.Location = new Point(0, 0);
            splitContainer6.Margin = new Padding(0, 0, 0, 0);
            splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            splitContainer6.Panel1.Controls.Add(LinesOfCodePerTypeText);
            splitContainer6.Panel1MinSize = 250;
            // 
            // splitContainer6.Panel2
            // 
            splitContainer6.Panel2.Controls.Add(LinesOfCodePie);
            splitContainer6.Size = new Size(737, 400);
            splitContainer6.SplitterDistance = 286;
            splitContainer6.TabIndex = 0;
            // 
            // LinesOfCodePerTypeText
            // 
            LinesOfCodePerTypeText.AutoSize = true;
            LinesOfCodePerTypeText.Location = new Point(9, 4);
            LinesOfCodePerTypeText.Margin = new Padding(0, 0, 0, 0);
            LinesOfCodePerTypeText.Name = "LinesOfCodePerTypeText";
            LinesOfCodePerTypeText.Size = new Size(54, 13);
            LinesOfCodePerTypeText.TabIndex = 0;
            LinesOfCodePerTypeText.Text = "Loading...";
            // 
            // LinesOfCodePie
            // 
            LinesOfCodePie.Dock = DockStyle.None;
            LinesOfCodePie.InitialAngle = -30;
            LinesOfCodePie.Location = new Point(0, 0);
            LinesOfCodePie.Margin = new Padding(0, 0, 0, 0);
            LinesOfCodePie.Name = "LinesOfCodePie";
            LinesOfCodePie.Size = new Size(447, 400);
            LinesOfCodePie.TabIndex = 0;
            LinesOfCodePie.ToolTips = null;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(splitContainer7);
            tabPage4.Location = new Point(4, 22);
            tabPage4.Margin = new Padding(0, 0, 0, 0);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(743, 439);
            tabPage4.TabIndex = 0;
            tabPage4.Text = "Lines of test code";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // splitContainer7
            // 
            splitContainer7.Dock = DockStyle.Fill;
            splitContainer7.FixedPanel = FixedPanel.Panel1;
            splitContainer7.Location = new Point(0, 0);
            splitContainer7.Margin = new Padding(0, 0, 0, 0);
            splitContainer7.Name = "splitContainer7";
            splitContainer7.Orientation = Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            splitContainer7.Panel1.Controls.Add(TotalLinesOfTestCode);
            // 
            // splitContainer7.Panel2
            // 
            splitContainer7.Panel2.Controls.Add(splitContainer8);
            splitContainer7.Size = new Size(737, 433);
            splitContainer7.SplitterDistance = 29;
            splitContainer7.TabIndex = 0;
            // 
            // TotalLinesOfTestCode
            // 
            TotalLinesOfTestCode.AutoSize = true;
            TotalLinesOfTestCode.Location = new Point(5, 2);
            TotalLinesOfTestCode.Margin = new Padding(0, 0, 0, 0);
            TotalLinesOfTestCode.Name = "TotalLinesOfTestCode";
            TotalLinesOfTestCode.Size = new Size(94, 13);
            TotalLinesOfTestCode.TabIndex = 2;
            TotalLinesOfTestCode.Text = "Total lines of code";
            // 
            // splitContainer8
            // 
            splitContainer8.Dock = DockStyle.Fill;
            splitContainer8.Location = new Point(0, 0);
            splitContainer8.Margin = new Padding(0, 0, 0, 0);
            splitContainer8.Name = "splitContainer8";
            // 
            // splitContainer8.Panel1
            // 
            splitContainer8.Panel1.Controls.Add(TestCodeText);
            splitContainer8.Panel1MinSize = 250;
            // 
            // splitContainer8.Panel2
            // 
            splitContainer8.Panel2.Controls.Add(TestCodePie);
            splitContainer8.Size = new Size(737, 400);
            splitContainer8.SplitterDistance = 286;
            splitContainer8.TabIndex = 0;
            // 
            // TestCodeText
            // 
            TestCodeText.AutoSize = true;
            TestCodeText.Location = new Point(9, 4);
            TestCodeText.Margin = new Padding(0, 0, 0, 0);
            TestCodeText.Name = "TestCodeText";
            TestCodeText.Size = new Size(54, 13);
            TestCodeText.TabIndex = 0;
            TestCodeText.Text = "Loading...";
            // 
            // TestCodePie
            // 
            TestCodePie.Dock = DockStyle.None;
            TestCodePie.InitialAngle = -30;
            TestCodePie.Location = new Point(0, 0);
            TestCodePie.Margin = new Padding(0, 0, 0, 0);
            TestCodePie.Name = "TestCodePie";
            TestCodePie.Size = new Size(447, 400);
            TestCodePie.TabIndex = 0;
            TestCodePie.ToolTips = null;
            // 
            // LoadingLabel
            // 
            LoadingLabel.AutoSize = true;
            LoadingLabel.Location = new Point(315, 26);
            LoadingLabel.Margin = new Padding(0, 0, 0, 0);
            LoadingLabel.Name = "LoadingLabel";
            LoadingLabel.Size = new Size(54, 13);
            LoadingLabel.TabIndex = 1;
            LoadingLabel.Text = "Loading...";
            // 
            // FormGitStatistics
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(751, 465);
            Controls.Add(LoadingLabel);
            Controls.Add(Tabs);
            MinimumSize = new Size(350, 250);
            Margin = new Padding(0, 0, 0, 0);
            Name = "FormGitStatistics";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Statistics";
            FormClosing += FormGitStatistics_FormClosing;
            Shown += FormGitStatisticsShown;
            Tabs.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer3)).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel1.PerformLayout();
            splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer4)).EndInit();
            splitContainer4.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).EndInit();
            splitContainer2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            splitContainer5.Panel1.ResumeLayout(false);
            splitContainer5.Panel1.PerformLayout();
            splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer5)).EndInit();
            splitContainer5.ResumeLayout(false);
            splitContainer6.Panel1.ResumeLayout(false);
            splitContainer6.Panel1.PerformLayout();
            splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer6)).EndInit();
            splitContainer6.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            splitContainer7.Panel1.ResumeLayout(false);
            splitContainer7.Panel1.PerformLayout();
            splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer7)).EndInit();
            splitContainer7.ResumeLayout(false);
            splitContainer8.Panel1.ResumeLayout(false);
            splitContainer8.Panel1.PerformLayout();
            splitContainer8.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer8)).EndInit();
            splitContainer8.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GitUI.CommandsDialogs.FullBleedTabControl Tabs;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private PieChartControl LinesOfCodePie;
        private SplitContainer splitContainer1;
        private PieChartControl LinesOfCodeExtensionPie;
        private SplitContainer splitContainer2;
        private Label TotalLinesOfCode;
        private SplitContainer splitContainer3;
        private Label TotalCommits;
        private PieChartControl CommitCountPie;
        private SplitContainer splitContainer4;
        private Label CommitStatistics;
        private TabPage tabPage3;
        private SplitContainer splitContainer5;
        private Label TotalLinesOfCode2;
        private SplitContainer splitContainer6;
        private Label LinesOfCodePerTypeText;
        private Label LinesOfCodePerLanguageText;
        private TabPage tabPage4;
        private SplitContainer splitContainer7;
        private Label TotalLinesOfTestCode;
        private SplitContainer splitContainer8;
        private PieChartControl TestCodePie;
        private Label TestCodeText;
        private Label LoadingLabel;
    }
}

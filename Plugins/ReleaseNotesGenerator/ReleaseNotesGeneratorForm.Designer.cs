namespace GitExtensions.Plugins.ReleaseNotesGenerator
{
    partial class ReleaseNotesGeneratorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            _NO_TRANSLATE_textBoxRevTo = new TextBox();
            textBoxRevFrom = new TextBox();
            label3 = new Label();
            _NO_TRANSLATE_textBoxGitLogArguments = new TextBox();
            label4 = new Label();
            groupBox1 = new GroupBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label6 = new Label();
            labelRevCount = new Label();
            label5 = new Label();
            groupBoxCopy = new GroupBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            flowLayoutPanel3 = new FlowLayoutPanel();
            buttonCopyOrigOutput = new Button();
            label8 = new Label();
            flowLayoutPanel4 = new FlowLayoutPanel();
            buttonCopyAsTextTableTab = new Button();
            label9 = new Label();
            flowLayoutPanel5 = new FlowLayoutPanel();
            buttonCopyAsTextTableSpace = new Button();
            label10 = new Label();
            flowLayoutPanel6 = new FlowLayoutPanel();
            buttonCopyAsHtml = new Button();
            label11 = new Label();
            textBoxResult = new TextBox();
            buttonGenerate = new Button();
            label7 = new Label();
            groupBox1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBoxCopy.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            flowLayoutPanel4.SuspendLayout();
            flowLayoutPanel5.SuspendLayout();
            flowLayoutPanel6.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 41);
            label1.Name = "label1";
            label1.Size = new Size(176, 13);
            label1.TabIndex = 0;
            label1.Text = "Commit expression \"To\" (including):";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(192, 13);
            label2.TabIndex = 1;
            label2.Text = "Commit expression \"From\" (excluding):";
            // 
            // textBoxRevTo
            // 
            _NO_TRANSLATE_textBoxRevTo.Location = new Point(214, 38);
            _NO_TRANSLATE_textBoxRevTo.Name = "textBoxRevTo";
            _NO_TRANSLATE_textBoxRevTo.Size = new Size(162, 21);
            _NO_TRANSLATE_textBoxRevTo.TabIndex = 1;
            _NO_TRANSLATE_textBoxRevTo.Text = "HEAD";
            // 
            // textBoxRevFrom
            // 
            textBoxRevFrom.Location = new Point(214, 6);
            textBoxRevFrom.Name = "textBoxRevFrom";
            textBoxRevFrom.Size = new Size(162, 21);
            textBoxRevFrom.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 76);
            label3.Name = "label3";
            label3.Size = new Size(94, 13);
            label3.TabIndex = 4;
            label3.Text = "git log arguments:";
            // 
            // _NO_TRANSLATE_textBoxGitLogArguments
            // 
            _NO_TRANSLATE_textBoxGitLogArguments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textBoxGitLogArguments.Location = new Point(118, 73);
            _NO_TRANSLATE_textBoxGitLogArguments.Name = "_NO_TRANSLATE_textBoxGitLogArguments";
            _NO_TRANSLATE_textBoxGitLogArguments.Size = new Size(366, 21);
            _NO_TRANSLATE_textBoxGitLogArguments.TabIndex = 2;
            _NO_TRANSLATE_textBoxGitLogArguments.Text = "--pretty=\"format:%h@%s%b\" --abbrev-commit {0}..{1}";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(490, 76);
            label4.Name = "label4";
            label4.Size = new Size(106, 13);
            label4.TabIndex = 6;
            label4.Text = "{0} = from; {1} = to";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(flowLayoutPanel1);
            groupBox1.Controls.Add(groupBoxCopy);
            groupBox1.Controls.Add(textBoxResult);
            groupBox1.Location = new Point(15, 156);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(587, 379);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Result";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(label6);
            flowLayoutPanel1.Controls.Add(labelRevCount);
            flowLayoutPanel1.Controls.Add(label5);
            flowLayoutPanel1.Location = new Point(6, 21);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(575, 24);
            flowLayoutPanel1.TabIndex = 11;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(3, 0);
            label6.Margin = new Padding(3, 0, 0, 0);
            label6.Name = "label6";
            label6.Size = new Size(96, 13);
            label6.TabIndex = 10;
            label6.Text = "Revisions count = ";
            // 
            // labelRevCount
            // 
            labelRevCount.AutoSize = true;
            labelRevCount.Location = new Point(99, 0);
            labelRevCount.Margin = new Padding(0, 0, 3, 0);
            labelRevCount.Name = "labelRevCount";
            labelRevCount.Size = new Size(23, 13);
            labelRevCount.TabIndex = 11;
            labelRevCount.Text = "n/a";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(128, 0);
            label5.Name = "label5";
            label5.Size = new Size(235, 13);
            label5.TabIndex = 9;
            label5.Text = "Sorting: Most recent revisions are listed on top.";
            // 
            // groupBoxCopy
            // 
            groupBoxCopy.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxCopy.Controls.Add(flowLayoutPanel2);
            groupBoxCopy.Location = new Point(6, 172);
            groupBoxCopy.Name = "groupBoxCopy";
            groupBoxCopy.Size = new Size(575, 201);
            groupBoxCopy.TabIndex = 1;
            groupBoxCopy.TabStop = false;
            groupBoxCopy.Text = "Copy to clipboard";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(flowLayoutPanel3);
            flowLayoutPanel2.Controls.Add(flowLayoutPanel4);
            flowLayoutPanel2.Controls.Add(flowLayoutPanel5);
            flowLayoutPanel2.Controls.Add(flowLayoutPanel6);
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel2.Location = new Point(3, 17);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(569, 181);
            flowLayoutPanel2.TabIndex = 2;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.Controls.Add(buttonCopyOrigOutput);
            flowLayoutPanel3.Controls.Add(label8);
            flowLayoutPanel3.Location = new Point(3, 3);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(400, 35);
            flowLayoutPanel3.TabIndex = 4;
            // 
            // buttonCopyOrigOutput
            // 
            buttonCopyOrigOutput.Anchor = AnchorStyles.Left;
            buttonCopyOrigOutput.Location = new Point(3, 3);
            buttonCopyOrigOutput.Name = "buttonCopyOrigOutput";
            buttonCopyOrigOutput.Size = new Size(125, 25);
            buttonCopyOrigOutput.TabIndex = 2;
            buttonCopyOrigOutput.Text = "original output";
            buttonCopyOrigOutput.UseVisualStyleBackColor = true;
            buttonCopyOrigOutput.Click += buttonCopyOrigOutput_Click;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Left;
            label8.AutoSize = true;
            label8.Location = new Point(134, 9);
            label8.Name = "label8";
            label8.Size = new Size(28, 13);
            label8.TabIndex = 3;
            label8.Text = "as is";
            // 
            // flowLayoutPanel4
            // 
            flowLayoutPanel4.Controls.Add(buttonCopyAsTextTableTab);
            flowLayoutPanel4.Controls.Add(label9);
            flowLayoutPanel4.Location = new Point(3, 44);
            flowLayoutPanel4.Name = "flowLayoutPanel4";
            flowLayoutPanel4.Size = new Size(400, 35);
            flowLayoutPanel4.TabIndex = 5;
            // 
            // buttonCopyAsTextTableTab
            // 
            buttonCopyAsTextTableTab.Location = new Point(3, 3);
            buttonCopyAsTextTableTab.Name = "buttonCopyAsTextTableTab";
            buttonCopyAsTextTableTab.Size = new Size(125, 25);
            buttonCopyAsTextTableTab.TabIndex = 0;
            buttonCopyAsTextTableTab.Text = "as text table (tabs)";
            buttonCopyAsTextTableTab.UseVisualStyleBackColor = true;
            buttonCopyAsTextTableTab.Click += buttonCopyAsPlainText_Click;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Left;
            label9.AutoSize = true;
            label9.Location = new Point(134, 9);
            label9.Name = "label9";
            label9.Size = new Size(138, 13);
            label9.TabIndex = 4;
            label9.Text = "separate columns with tabs";
            // 
            // flowLayoutPanel5
            // 
            flowLayoutPanel5.Controls.Add(buttonCopyAsTextTableSpace);
            flowLayoutPanel5.Controls.Add(label10);
            flowLayoutPanel5.Location = new Point(3, 85);
            flowLayoutPanel5.Name = "flowLayoutPanel5";
            flowLayoutPanel5.Size = new Size(400, 35);
            flowLayoutPanel5.TabIndex = 6;
            // 
            // buttonCopyAsTextTableSpace
            // 
            buttonCopyAsTextTableSpace.Location = new Point(3, 3);
            buttonCopyAsTextTableSpace.Name = "buttonCopyAsTextTableSpace";
            buttonCopyAsTextTableSpace.Size = new Size(125, 25);
            buttonCopyAsTextTableSpace.TabIndex = 3;
            buttonCopyAsTextTableSpace.Text = "as text table (spaces)";
            buttonCopyAsTextTableSpace.UseVisualStyleBackColor = true;
            buttonCopyAsTextTableSpace.Click += buttonCopyAsTextTableSpace_Click;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Left;
            label10.AutoSize = true;
            label10.Location = new Point(134, 9);
            label10.Name = "label10";
            label10.Size = new Size(150, 13);
            label10.TabIndex = 5;
            label10.Text = "separate columns with spaces";
            // 
            // flowLayoutPanel6
            // 
            flowLayoutPanel6.Controls.Add(buttonCopyAsHtml);
            flowLayoutPanel6.Controls.Add(label11);
            flowLayoutPanel6.Location = new Point(3, 126);
            flowLayoutPanel6.Name = "flowLayoutPanel6";
            flowLayoutPanel6.Size = new Size(521, 43);
            flowLayoutPanel6.TabIndex = 7;
            // 
            // buttonCopyAsHtml
            // 
            buttonCopyAsHtml.Location = new Point(3, 3);
            buttonCopyAsHtml.Name = "buttonCopyAsHtml";
            buttonCopyAsHtml.Size = new Size(125, 25);
            buttonCopyAsHtml.TabIndex = 1;
            buttonCopyAsHtml.Text = "as HTML table";
            buttonCopyAsHtml.UseVisualStyleBackColor = true;
            buttonCopyAsHtml.Click += buttonCopyAsHtml_Click;
            // 
            // label11
            // 
            label11.Anchor = AnchorStyles.Left;
            label11.AutoSize = true;
            label11.Location = new Point(134, 2);
            label11.Name = "label11";
            label11.Size = new Size(335, 26);
            label11.TabIndex = 4;
            label11.Text = "Clipboard will contain HTML code (plain text) and HTML format\r\nwhich can be paste" +
    "d to programs like MS Word or LibreOffice Writer.";
            // 
            // textBoxResult
            // 
            textBoxResult.AcceptsReturn = true;
            textBoxResult.AcceptsTab = true;
            textBoxResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxResult.Location = new Point(6, 50);
            textBoxResult.Multiline = true;
            textBoxResult.Name = "textBoxResult";
            textBoxResult.ReadOnly = true;
            textBoxResult.ScrollBars = ScrollBars.Both;
            textBoxResult.Size = new Size(575, 116);
            textBoxResult.TabIndex = 0;
            textBoxResult.WordWrap = false;
            textBoxResult.TextChanged += textBoxResult_TextChanged;
            // 
            // buttonGenerate
            // 
            buttonGenerate.Location = new Point(15, 113);
            buttonGenerate.Name = "buttonGenerate";
            buttonGenerate.Size = new Size(105, 25);
            buttonGenerate.TabIndex = 3;
            buttonGenerate.Text = "Generate";
            buttonGenerate.UseVisualStyleBackColor = true;
            buttonGenerate.Click += buttonGenerate_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(389, 9);
            label7.Name = "label7";
            label7.Size = new Size(218, 26);
            label7.TabIndex = 8;
            label7.Text = "(Commit expressions can be commit hashes,\r\nbranch names, tag names)";
            // 
            // ReleaseNotesGeneratorForm
            // 
            AcceptButton = buttonGenerate;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(614, 547);
            Controls.Add(label7);
            Controls.Add(buttonGenerate);
            Controls.Add(groupBox1);
            Controls.Add(label4);
            Controls.Add(_NO_TRANSLATE_textBoxGitLogArguments);
            Controls.Add(label3);
            Controls.Add(textBoxRevFrom);
            Controls.Add(_NO_TRANSLATE_textBoxRevTo);
            Controls.Add(label2);
            Controls.Add(label1);
            MinimizeBox = false;
            MinimumSize = new Size(630, 470);
            Name = "ReleaseNotesGeneratorForm";
            Text = "Release Notes Generator";
            Load += ReleaseNotesGeneratorForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            groupBoxCopy.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            flowLayoutPanel4.ResumeLayout(false);
            flowLayoutPanel4.PerformLayout();
            flowLayoutPanel5.ResumeLayout(false);
            flowLayoutPanel5.PerformLayout();
            flowLayoutPanel6.ResumeLayout(false);
            flowLayoutPanel6.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox _NO_TRANSLATE_textBoxRevTo;
        private TextBox textBoxRevFrom;
        private Label label3;
        private TextBox _NO_TRANSLATE_textBoxGitLogArguments;
        private Label label4;
        private GroupBox groupBox1;
        private TextBox textBoxResult;
        private Button buttonGenerate;
        private Label label5;
        private GroupBox groupBoxCopy;
        private Button buttonCopyAsHtml;
        private Button buttonCopyAsTextTableTab;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label6;
        private Label labelRevCount;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button buttonCopyOrigOutput;
        private Button buttonCopyAsTextTableSpace;
        private Label label7;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label label8;
        private FlowLayoutPanel flowLayoutPanel4;
        private Label label9;
        private FlowLayoutPanel flowLayoutPanel5;
        private Label label10;
        private FlowLayoutPanel flowLayoutPanel6;
        private Label label11;
    }
}

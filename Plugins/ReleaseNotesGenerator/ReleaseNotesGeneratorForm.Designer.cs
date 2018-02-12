namespace ReleaseNotesGenerator
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
            if (disposing && (components != null))
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxRevTo = new System.Windows.Forms.TextBox();
            this.textBoxRevFrom = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_textBoxGitLogArguments = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.labelRevCount = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBoxCopy = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCopyOrigOutput = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCopyAsTextTableTab = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCopyAsTextTableSpace = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCopyAsHtml = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBoxCopy.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.flowLayoutPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Commit expression \"To\" (including):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(192, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Commit expression \"From\" (excluding):";
            // 
            // textBoxRevTo
            // 
            this.textBoxRevTo.Location = new System.Drawing.Point(214, 38);
            this.textBoxRevTo.Name = "textBoxRevTo";
            this.textBoxRevTo.Size = new System.Drawing.Size(162, 21);
            this.textBoxRevTo.TabIndex = 1;
            // 
            // textBoxRevFrom
            // 
            this.textBoxRevFrom.Location = new System.Drawing.Point(214, 6);
            this.textBoxRevFrom.Name = "textBoxRevFrom";
            this.textBoxRevFrom.Size = new System.Drawing.Size(162, 21);
            this.textBoxRevFrom.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "git log arguments:";
            // 
            // _NO_TRANSLATE_textBoxGitLogArguments
            // 
            this._NO_TRANSLATE_textBoxGitLogArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textBoxGitLogArguments.Location = new System.Drawing.Point(118, 73);
            this._NO_TRANSLATE_textBoxGitLogArguments.Name = "_NO_TRANSLATE_textBoxGitLogArguments";
            this._NO_TRANSLATE_textBoxGitLogArguments.Size = new System.Drawing.Size(366, 21);
            this._NO_TRANSLATE_textBoxGitLogArguments.TabIndex = 2;
            this._NO_TRANSLATE_textBoxGitLogArguments.Text = "--pretty=\"format:%h@%s%b\" --abbrev-commit {0}..{1}";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(490, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "{0} = from; {1} = to";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Controls.Add(this.groupBoxCopy);
            this.groupBox1.Controls.Add(this.textBoxResult);
            this.groupBox1.Location = new System.Drawing.Point(15, 156);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(587, 379);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Result";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.label6);
            this.flowLayoutPanel1.Controls.Add(this.labelRevCount);
            this.flowLayoutPanel1.Controls.Add(this.label5);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 21);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(575, 24);
            this.flowLayoutPanel1.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Revisions count = ";
            // 
            // labelRevCount
            // 
            this.labelRevCount.AutoSize = true;
            this.labelRevCount.Location = new System.Drawing.Point(99, 0);
            this.labelRevCount.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.labelRevCount.Name = "labelRevCount";
            this.labelRevCount.Size = new System.Drawing.Size(23, 13);
            this.labelRevCount.TabIndex = 11;
            this.labelRevCount.Text = "n/a";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(128, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(235, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Sorting: Most recent revisions are listed on top.";
            // 
            // groupBoxCopy
            // 
            this.groupBoxCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCopy.Controls.Add(this.flowLayoutPanel2);
            this.groupBoxCopy.Location = new System.Drawing.Point(6, 172);
            this.groupBoxCopy.Name = "groupBoxCopy";
            this.groupBoxCopy.Size = new System.Drawing.Size(575, 201);
            this.groupBoxCopy.TabIndex = 1;
            this.groupBoxCopy.TabStop = false;
            this.groupBoxCopy.Text = "Copy to clipboard";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel4);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel5);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel6);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(569, 181);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.buttonCopyOrigOutput);
            this.flowLayoutPanel3.Controls.Add(this.label8);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(400, 35);
            this.flowLayoutPanel3.TabIndex = 4;
            // 
            // buttonCopyOrigOutput
            // 
            this.buttonCopyOrigOutput.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonCopyOrigOutput.Location = new System.Drawing.Point(3, 3);
            this.buttonCopyOrigOutput.Name = "buttonCopyOrigOutput";
            this.buttonCopyOrigOutput.Size = new System.Drawing.Size(125, 25);
            this.buttonCopyOrigOutput.TabIndex = 2;
            this.buttonCopyOrigOutput.Text = "original output";
            this.buttonCopyOrigOutput.UseVisualStyleBackColor = true;
            this.buttonCopyOrigOutput.Click += new System.EventHandler(this.buttonCopyOrigOutput_Click);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(134, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(28, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "as is";
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.buttonCopyAsTextTableTab);
            this.flowLayoutPanel4.Controls.Add(this.label9);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(3, 44);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(400, 35);
            this.flowLayoutPanel4.TabIndex = 5;
            // 
            // buttonCopyAsTextTableTab
            // 
            this.buttonCopyAsTextTableTab.Location = new System.Drawing.Point(3, 3);
            this.buttonCopyAsTextTableTab.Name = "buttonCopyAsTextTableTab";
            this.buttonCopyAsTextTableTab.Size = new System.Drawing.Size(125, 25);
            this.buttonCopyAsTextTableTab.TabIndex = 0;
            this.buttonCopyAsTextTableTab.Text = "as text table (tabs)";
            this.buttonCopyAsTextTableTab.UseVisualStyleBackColor = true;
            this.buttonCopyAsTextTableTab.Click += new System.EventHandler(this.buttonCopyAsPlainText_Click);
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(134, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(138, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "separate columns with tabs";
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.buttonCopyAsTextTableSpace);
            this.flowLayoutPanel5.Controls.Add(this.label10);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(3, 85);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(400, 35);
            this.flowLayoutPanel5.TabIndex = 6;
            // 
            // buttonCopyAsTextTableSpace
            // 
            this.buttonCopyAsTextTableSpace.Location = new System.Drawing.Point(3, 3);
            this.buttonCopyAsTextTableSpace.Name = "buttonCopyAsTextTableSpace";
            this.buttonCopyAsTextTableSpace.Size = new System.Drawing.Size(125, 25);
            this.buttonCopyAsTextTableSpace.TabIndex = 3;
            this.buttonCopyAsTextTableSpace.Text = "as text table (spaces)";
            this.buttonCopyAsTextTableSpace.UseVisualStyleBackColor = true;
            this.buttonCopyAsTextTableSpace.Click += new System.EventHandler(this.buttonCopyAsTextTableSpace_Click);
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(134, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(150, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "separate columns with spaces";
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.Controls.Add(this.buttonCopyAsHtml);
            this.flowLayoutPanel6.Controls.Add(this.label11);
            this.flowLayoutPanel6.Location = new System.Drawing.Point(3, 126);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(521, 43);
            this.flowLayoutPanel6.TabIndex = 7;
            // 
            // buttonCopyAsHtml
            // 
            this.buttonCopyAsHtml.Location = new System.Drawing.Point(3, 3);
            this.buttonCopyAsHtml.Name = "buttonCopyAsHtml";
            this.buttonCopyAsHtml.Size = new System.Drawing.Size(125, 25);
            this.buttonCopyAsHtml.TabIndex = 1;
            this.buttonCopyAsHtml.Text = "as HTML table";
            this.buttonCopyAsHtml.UseVisualStyleBackColor = true;
            this.buttonCopyAsHtml.Click += new System.EventHandler(this.buttonCopyAsHtml_Click);
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(134, 2);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(335, 26);
            this.label11.TabIndex = 4;
            this.label11.Text = "Clipboard will contain HTML code (plain text) and HTML format\r\nwhich can be paste" +
    "d to programs like MS Word or LibreOffice Writer.";
            // 
            // textBoxResult
            // 
            this.textBoxResult.AcceptsReturn = true;
            this.textBoxResult.AcceptsTab = true;
            this.textBoxResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxResult.Location = new System.Drawing.Point(6, 50);
            this.textBoxResult.Multiline = true;
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxResult.Size = new System.Drawing.Size(575, 116);
            this.textBoxResult.TabIndex = 0;
            this.textBoxResult.WordWrap = false;
            this.textBoxResult.TextChanged += new System.EventHandler(this.textBoxResult_TextChanged);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(15, 113);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(105, 25);
            this.buttonGenerate.TabIndex = 3;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(389, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(218, 26);
            this.label7.TabIndex = 8;
            this.label7.Text = "(Commit expressions can be commit hashes,\r\nbranch names, tag names)";
            // 
            // ReleaseNotesGeneratorForm
            // 
            this.AcceptButton = this.buttonGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(614, 547);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._NO_TRANSLATE_textBoxGitLogArguments);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxRevFrom);
            this.Controls.Add(this.textBoxRevTo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(630, 470);
            this.Name = "ReleaseNotesGeneratorForm";
            this.Text = "Release Notes Generator";
            this.Load += new System.EventHandler(this.ReleaseNotesGeneratorForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBoxCopy.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.flowLayoutPanel6.ResumeLayout(false);
            this.flowLayoutPanel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxRevTo;
        private System.Windows.Forms.TextBox textBoxRevFrom;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_textBoxGitLogArguments;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBoxCopy;
        private System.Windows.Forms.Button buttonCopyAsHtml;
        private System.Windows.Forms.Button buttonCopyAsTextTableTab;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelRevCount;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button buttonCopyOrigOutput;
        private System.Windows.Forms.Button buttonCopyAsTextTableSpace;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
        private System.Windows.Forms.Label label11;
    }
}
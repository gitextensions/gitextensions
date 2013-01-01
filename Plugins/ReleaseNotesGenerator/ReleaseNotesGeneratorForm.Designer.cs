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
            this.textBoxGitLogArguments = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxCopy = new System.Windows.Forms.GroupBox();
            this.buttonCopyAsHtml = new System.Windows.Forms.Button();
            this.buttonCopyAsPlainText = new System.Windows.Forms.Button();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBoxCopy.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(174, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Commit expression \"To\" (including):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Commit expression \"From\" (excluding):";
            // 
            // textBoxRevTo
            // 
            this.textBoxRevTo.Location = new System.Drawing.Point(214, 38);
            this.textBoxRevTo.Name = "textBoxRevTo";
            this.textBoxRevTo.Size = new System.Drawing.Size(162, 20);
            this.textBoxRevTo.TabIndex = 1;
            // 
            // textBoxRevFrom
            // 
            this.textBoxRevFrom.Location = new System.Drawing.Point(214, 6);
            this.textBoxRevFrom.Name = "textBoxRevFrom";
            this.textBoxRevFrom.Size = new System.Drawing.Size(162, 20);
            this.textBoxRevFrom.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "git log arguments:";
            // 
            // textBoxGitLogArguments
            // 
            this.textBoxGitLogArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGitLogArguments.Location = new System.Drawing.Point(118, 73);
            this.textBoxGitLogArguments.Name = "textBoxGitLogArguments";
            this.textBoxGitLogArguments.Size = new System.Drawing.Size(294, 20);
            this.textBoxGitLogArguments.TabIndex = 2;
            this.textBoxGitLogArguments.Text = "--pretty=\"format:%h@%s%b\" --abbrev-commit {0}..{1}";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(418, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "{0} = from; {1} = to";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBoxCopy);
            this.groupBox1.Controls.Add(this.textBoxResult);
            this.groupBox1.Location = new System.Drawing.Point(15, 156);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(515, 263);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Result";
            // 
            // groupBoxCopy
            // 
            this.groupBoxCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCopy.Controls.Add(this.buttonCopyAsHtml);
            this.groupBoxCopy.Controls.Add(this.buttonCopyAsPlainText);
            this.groupBoxCopy.Location = new System.Drawing.Point(6, 197);
            this.groupBoxCopy.Name = "groupBoxCopy";
            this.groupBoxCopy.Size = new System.Drawing.Size(503, 60);
            this.groupBoxCopy.TabIndex = 1;
            this.groupBoxCopy.TabStop = false;
            this.groupBoxCopy.Text = "Copy to clipboard";
            // 
            // buttonCopyAsHtml
            // 
            this.buttonCopyAsHtml.Location = new System.Drawing.Point(123, 24);
            this.buttonCopyAsHtml.Name = "buttonCopyAsHtml";
            this.buttonCopyAsHtml.Size = new System.Drawing.Size(100, 25);
            this.buttonCopyAsHtml.TabIndex = 1;
            this.buttonCopyAsHtml.Text = "as HTML table";
            this.buttonCopyAsHtml.UseVisualStyleBackColor = true;
            this.buttonCopyAsHtml.Click += new System.EventHandler(this.buttonCopyAsHtml_Click);
            // 
            // buttonCopyAsPlainText
            // 
            this.buttonCopyAsPlainText.Location = new System.Drawing.Point(15, 24);
            this.buttonCopyAsPlainText.Name = "buttonCopyAsPlainText";
            this.buttonCopyAsPlainText.Size = new System.Drawing.Size(100, 25);
            this.buttonCopyAsPlainText.TabIndex = 0;
            this.buttonCopyAsPlainText.Text = "as plain text";
            this.buttonCopyAsPlainText.UseVisualStyleBackColor = true;
            this.buttonCopyAsPlainText.Click += new System.EventHandler(this.buttonCopyAsPlainText_Click);
            // 
            // textBoxResult
            // 
            this.textBoxResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxResult.Location = new System.Drawing.Point(6, 19);
            this.textBoxResult.Multiline = true;
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxResult.Size = new System.Drawing.Size(503, 167);
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
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(141, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "most recent is listed above";
            // 
            // ReleaseNotesGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(542, 431);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxGitLogArguments);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxRevFrom);
            this.Controls.Add(this.textBoxRevTo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ReleaseNotesGeneratorForm";
            this.Text = "Release Notes Generator";
            this.Load += new System.EventHandler(this.ReleaseNotesGeneratorForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxCopy.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxRevTo;
        private System.Windows.Forms.TextBox textBoxRevFrom;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxGitLogArguments;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBoxCopy;
        private System.Windows.Forms.Button buttonCopyAsHtml;
        private System.Windows.Forms.Button buttonCopyAsPlainText;
    }
}
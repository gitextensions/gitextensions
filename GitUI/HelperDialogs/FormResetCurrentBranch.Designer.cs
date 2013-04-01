namespace GitUI.HelperDialogs
{
    partial class FormResetCurrentBranch
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
            this._NO_TRANSLATE_BranchInfo = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Hard = new System.Windows.Forms.RadioButton();
            this.Mixed = new System.Windows.Forms.RadioButton();
            this.Soft = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_BranchInfo
            // 
            this._NO_TRANSLATE_BranchInfo.AutoSize = true;
            this._NO_TRANSLATE_BranchInfo.Location = new System.Drawing.Point(13, 13);
            this._NO_TRANSLATE_BranchInfo.Name = "_NO_TRANSLATE_BranchInfo";
            this._NO_TRANSLATE_BranchInfo.Size = new System.Drawing.Size(129, 15);
            this._NO_TRANSLATE_BranchInfo.TabIndex = 0;
            this._NO_TRANSLATE_BranchInfo.Text = "##Reset branch \'{0}\' to:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Hard);
            this.groupBox1.Controls.Add(this.Mixed);
            this.groupBox1.Controls.Add(this.Soft);
            this.groupBox1.Location = new System.Drawing.Point(16, 217);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(380, 150);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reset type";
            // 
            // Hard
            // 
            this.Hard.AutoSize = true;
            this.Hard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.Hard.Location = new System.Drawing.Point(13, 102);
            this.Hard.Name = "Hard";
            this.Hard.Size = new System.Drawing.Size(323, 34);
            this.Hard.TabIndex = 2;
            this.Hard.Text = "Hard: reset working dir and index\r\n(discard ALL local changes, even uncommitted c" +
    "hanges)";
            this.Hard.UseVisualStyleBackColor = false;
            // 
            // Mixed
            // 
            this.Mixed.AutoSize = true;
            this.Mixed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Mixed.Checked = true;
            this.Mixed.Location = new System.Drawing.Point(13, 64);
            this.Mixed.Name = "Mixed";
            this.Mixed.Size = new System.Drawing.Size(276, 19);
            this.Mixed.TabIndex = 1;
            this.Mixed.TabStop = true;
            this.Mixed.Text = "Mixed: leave working dir untouched, reset index";
            this.Mixed.UseVisualStyleBackColor = false;
            // 
            // Soft
            // 
            this.Soft.AutoSize = true;
            this.Soft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Soft.Location = new System.Drawing.Point(13, 28);
            this.Soft.Name = "Soft";
            this.Soft.Size = new System.Drawing.Size(257, 19);
            this.Soft.TabIndex = 0;
            this.Soft.Text = "Soft: leave working dir and index untouched";
            this.Soft.UseVisualStyleBackColor = false;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(302, 373);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(91, 25);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(399, 373);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(91, 25);
            this.Cancel.TabIndex = 7;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(16, 41);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(440, 160);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(477, 160);
            this.commitSummaryUserControl1.TabIndex = 8;
            // 
            // FormResetCurrentBranch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(502, 410);
            this.Controls.Add(this.commitSummaryUserControl1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._NO_TRANSLATE_BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormResetCurrentBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset current branch";
            this.Load += new System.EventHandler(this.FormResetCurrentBranch_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _NO_TRANSLATE_BranchInfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Hard;
        private System.Windows.Forms.RadioButton Mixed;
        private System.Windows.Forms.RadioButton Soft;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl1;
    }
}
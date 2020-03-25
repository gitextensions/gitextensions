namespace GitUI.HelperDialogs
{
    partial class FormResetAnotherBranch
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
            this.BranchInfo = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.commitSummaryUserControl = new GitUI.UserControls.CommitSummaryUserControl();
            this.ForceReset = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelResetBranchWarning = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.ResetTo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(13, 13);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(99, 13);
            this.BranchInfo.TabIndex = 0;
            this.BranchInfo.Text = "Reset local &branch:";
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(321, 335);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(91, 25);
            this.Ok.TabIndex = 3;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(418, 335);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(91, 25);
            this.Cancel.TabIndex = 4;
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // commitSummaryUserControl
            // 
            this.commitSummaryUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl.AutoSize = true;
            this.commitSummaryUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitSummaryUserControl.Location = new System.Drawing.Point(16, 85);
            this.commitSummaryUserControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.commitSummaryUserControl.MinimumSize = new System.Drawing.Size(493, 150);
            this.commitSummaryUserControl.Name = "commitSummaryUserControl";
            this.commitSummaryUserControl.Revision = null;
            this.commitSummaryUserControl.Size = new System.Drawing.Size(493, 150);
            this.commitSummaryUserControl.TabIndex = 0;
            // 
            // ForceReset
            // 
            this.ForceReset.AutoSize = true;
            this.ForceReset.Location = new System.Drawing.Point(46, 296);
            this.ForceReset.Name = "ForceReset";
            this.ForceReset.Size = new System.Drawing.Size(208, 17);
            this.ForceReset.TabIndex = 2;
            this.ForceReset.Text = "&Force reset for a non-fast-forward reset";
            this.ForceReset.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Images.Warning;
            this.pictureBox1.InitialImage = global::GitUI.Properties.Images.Warning;
            this.pictureBox1.Location = new System.Drawing.Point(16, 258);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(21, 20);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // labelResetBranchWarning
            // 
            this.labelResetBranchWarning.AutoSize = true;
            this.labelResetBranchWarning.ForeColor = System.Drawing.Color.Black;
            this.labelResetBranchWarning.Location = new System.Drawing.Point(43, 255);
            this.labelResetBranchWarning.Name = "labelResetBranchWarning";
            this.labelResetBranchWarning.Size = new System.Drawing.Size(435, 26);
            this.labelResetBranchWarning.TabIndex = 0;
            this.labelResetBranchWarning.Text = "You can only reset a branch safely if there is a direct path from it to selected " +
    "revision.\r\nForcing a branch to reset if it has not been merged might leave some " +
    "commits unreachable.";
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.Location = new System.Drawing.Point(16, 33);
            this.Branches.Margin = new System.Windows.Forms.Padding(0);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(493, 21);
            this.Branches.TabIndex = 1;
            // 
            // ResetTo
            // 
            this.ResetTo.AutoSize = true;
            this.ResetTo.Location = new System.Drawing.Point(13, 63);
            this.ResetTo.Name = "ResetTo";
            this.ResetTo.Size = new System.Drawing.Size(52, 13);
            this.ResetTo.TabIndex = 0;
            this.ResetTo.Text = "to commit";
            // 
            // FormResetAnotherBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(521, 368);
            this.Controls.Add(this.Branches);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelResetBranchWarning);
            this.Controls.Add(this.ForceReset);
            this.Controls.Add(this.commitSummaryUserControl);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.ResetTo);
            this.Controls.Add(this.BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormResetAnotherBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset branch";
            this.Load += new System.EventHandler(this.FormResetAnotherBranch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl;
        private System.Windows.Forms.CheckBox ForceReset;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelResetBranchWarning;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label ResetTo;
    }
}

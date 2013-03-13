namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class BehaviourSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkCommitKeepSelection = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkCommitKeepSelection);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(346, 56);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Commit";
            // 
            // chkCommitKeepSelection
            // 
            this.chkCommitKeepSelection.AutoSize = true;
            this.chkCommitKeepSelection.Location = new System.Drawing.Point(7, 23);
            this.chkCommitKeepSelection.Name = "chkCommitKeepSelection";
            this.chkCommitKeepSelection.Size = new System.Drawing.Size(265, 19);
            this.chkCommitKeepSelection.TabIndex = 0;
            this.chkCommitKeepSelection.Text = "Keep selection on files when stage or unstage";
            this.chkCommitKeepSelection.UseVisualStyleBackColor = true;
            // 
            // BehaviourSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "BehaviourSettingsPage";
            this.Size = new System.Drawing.Size(353, 212);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkCommitKeepSelection;
    }
}

using GitUI.Editor;

namespace GitUI.HelperDialogs
{
    partial class FormCommitDiff
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
            this.CommitDiff = new GitUI.UserControls.CommitDiff();
            this.SuspendLayout();
            // 
            // CommitDiff
            // 
            this.CommitDiff.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommitDiff.Location = new System.Drawing.Point(0, 0);
            this.CommitDiff.MinimumSize = new System.Drawing.Size(150, 148);
            this.CommitDiff.Name = "CommitDiff";
            this.CommitDiff.Size = new System.Drawing.Size(716, 528);
            this.CommitDiff.TabIndex = 0;
            // 
            // FormCommitDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(717, 529);
            this.Controls.Add(this.CommitDiff);
            this.MinimumSize = new System.Drawing.Size(150, 148);
            this.Name = "FormCommitDiff";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Diff";
            this.ResumeLayout(false);

        }

        #endregion

        private UserControls.CommitDiff CommitDiff;
    }
}
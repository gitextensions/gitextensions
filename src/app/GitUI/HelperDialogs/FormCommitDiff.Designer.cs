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
            CommitDiff = new GitUI.UserControls.CommitDiff();
            SuspendLayout();
            // 
            // CommitDiff
            // 
            CommitDiff.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CommitDiff.Location = new Point(0, 0);
            CommitDiff.MinimumSize = new Size(150, 148);
            CommitDiff.Name = "CommitDiff";
            CommitDiff.Size = new Size(716, 528);
            CommitDiff.TabIndex = 0;
            // 
            // FormCommitDiff
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(717, 529);
            Controls.Add(CommitDiff);
            MinimumSize = new Size(150, 148);
            Name = "FormCommitDiff";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Diff";
            ResumeLayout(false);

        }

        #endregion

        private UserControls.CommitDiff CommitDiff;
    }
}
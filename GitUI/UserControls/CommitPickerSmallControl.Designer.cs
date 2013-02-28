namespace GitUI.UserControls
{
    partial class CommitPickerSmallControl
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
            this.textBoxCommitHash = new System.Windows.Forms.TextBox();
            this.buttonPickCommit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxCommitHash
            // 
            this.textBoxCommitHash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommitHash.Location = new System.Drawing.Point(0, 0);
            this.textBoxCommitHash.Name = "textBoxCommitHash";
            this.textBoxCommitHash.Size = new System.Drawing.Size(97, 23);
            this.textBoxCommitHash.TabIndex = 31;
            this.textBoxCommitHash.TextChanged += new System.EventHandler(this.textBoxCommitHash_TextChanged);
            // 
            // buttonPickCommit
            // 
            this.buttonPickCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPickCommit.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.buttonPickCommit.Location = new System.Drawing.Point(101, 0);
            this.buttonPickCommit.Name = "buttonPickCommit";
            this.buttonPickCommit.Size = new System.Drawing.Size(25, 24);
            this.buttonPickCommit.TabIndex = 32;
            this.buttonPickCommit.UseVisualStyleBackColor = true;
            this.buttonPickCommit.Click += new System.EventHandler(this.buttonPickCommit_Click);
            // 
            // CommitPickerSmallControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.textBoxCommitHash);
            this.Controls.Add(this.buttonPickCommit);
            this.MinimumSize = new System.Drawing.Size(100, 26);
            this.Name = "CommitPickerSmallControl";
            this.Size = new System.Drawing.Size(129, 26);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCommitHash;
        private System.Windows.Forms.Button buttonPickCommit;
    }
}

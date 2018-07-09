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
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            this.lbCommits = new System.Windows.Forms.Label();
            this.textBoxCommitHash = new System.Windows.Forms.TextBox();
            this.buttonPickCommit = new System.Windows.Forms.Button();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(this.lbCommits, 2, 0);
            tableLayoutPanel1.Controls.Add(this.textBoxCommitHash, 0, 0);
            tableLayoutPanel1.Controls.Add(this.buttonPickCommit, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(156, 26);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // lbCommits
            // 
            this.lbCommits.AutoEllipsis = true;
            this.lbCommits.AutoSize = true;
            this.lbCommits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCommits.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbCommits.Location = new System.Drawing.Point(138, 0);
            this.lbCommits.Name = "lbCommits";
            this.lbCommits.Size = new System.Drawing.Size(15, 26);
            this.lbCommits.TabIndex = 2;
            this.lbCommits.Text = "=";
            this.lbCommits.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxCommitHash
            // 
            this.textBoxCommitHash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxCommitHash.Location = new System.Drawing.Point(0, 2);
            this.textBoxCommitHash.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.textBoxCommitHash.MaxLength = 100;
            this.textBoxCommitHash.Name = "textBoxCommitHash";
            this.textBoxCommitHash.Size = new System.Drawing.Size(104, 21);
            this.textBoxCommitHash.TabIndex = 0;
            this.textBoxCommitHash.Leave += new System.EventHandler(this.textBoxCommitHash_TextLeave);
            // 
            // buttonPickCommit
            // 
            this.buttonPickCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPickCommit.Image = global::GitUI.Properties.Images.SelectRevision;
            this.buttonPickCommit.Location = new System.Drawing.Point(107, 0);
            this.buttonPickCommit.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonPickCommit.Name = "buttonPickCommit";
            this.buttonPickCommit.Size = new System.Drawing.Size(25, 24);
            this.buttonPickCommit.TabIndex = 1;
            this.buttonPickCommit.UseVisualStyleBackColor = true;
            this.buttonPickCommit.Click += new System.EventHandler(this.buttonPickCommit_Click);
            // 
            // CommitPickerSmallControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(100, 26);
            this.Name = "CommitPickerSmallControl";
            this.Size = new System.Drawing.Size(156, 26);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCommitHash;
        private System.Windows.Forms.Button buttonPickCommit;
        private System.Windows.Forms.Label lbCommits;
    }
}

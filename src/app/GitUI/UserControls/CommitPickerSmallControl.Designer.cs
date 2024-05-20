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
            if (disposing && (components is not null))
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
            TableLayoutPanel tableLayoutPanel1;
            lbCommits = new Label();
            textBoxCommitHash = new TextBox();
            buttonPickCommit = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(lbCommits, 2, 0);
            tableLayoutPanel1.Controls.Add(textBoxCommitHash, 0, 0);
            tableLayoutPanel1.Controls.Add(buttonPickCommit, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(156, 26);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // lbCommits
            // 
            lbCommits.AutoEllipsis = true;
            lbCommits.AutoSize = true;
            lbCommits.Dock = DockStyle.Fill;
            lbCommits.ForeColor = SystemColors.GrayText;
            lbCommits.Location = new Point(138, 0);
            lbCommits.Name = "lbCommits";
            lbCommits.Size = new Size(15, 26);
            lbCommits.TabIndex = 2;
            lbCommits.Text = "=";
            lbCommits.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBoxCommitHash
            // 
            textBoxCommitHash.Dock = DockStyle.Fill;
            textBoxCommitHash.Location = new Point(0, 2);
            textBoxCommitHash.Margin = new Padding(0, 2, 0, 0);
            textBoxCommitHash.MaxLength = 100;
            textBoxCommitHash.Name = "textBoxCommitHash";
            textBoxCommitHash.Size = new Size(104, 21);
            textBoxCommitHash.TabIndex = 0;
            textBoxCommitHash.Leave += textBoxCommitHash_TextLeave;
            // 
            // buttonPickCommit
            // 
            buttonPickCommit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonPickCommit.Image = Properties.Images.SelectRevision;
            buttonPickCommit.Location = new Point(107, 0);
            buttonPickCommit.Margin = new Padding(3, 0, 3, 0);
            buttonPickCommit.Name = "buttonPickCommit";
            buttonPickCommit.Size = new Size(25, 24);
            buttonPickCommit.TabIndex = 1;
            buttonPickCommit.UseVisualStyleBackColor = true;
            buttonPickCommit.Click += buttonPickCommit_Click;
            // 
            // CommitPickerSmallControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(100, 26);
            Name = "CommitPickerSmallControl";
            Size = new Size(156, 26);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox textBoxCommitHash;
        private Button buttonPickCommit;
        private Label lbCommits;
    }
}

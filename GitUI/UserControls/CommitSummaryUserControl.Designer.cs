namespace GitUI.UserControls
{
    partial class CommitSummaryUserControl
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
            labelDate = new Label();
            labelMessage = new Label();
            labelAuthor = new Label();
            labelTags = new Label();
            labelBranches = new Label();
            labelTagsCaption = new Label();
            labelBranchesCaption = new Label();
            labelAuthorCaption = new Label();
            groupBox1 = new GroupBox();
            labelDateCaption = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // labelDate
            // 
            labelDate.AutoSize = true;
            labelDate.Dock = DockStyle.Fill;
            labelDate.Location = new Point(114, 68);
            labelDate.Name = "labelDate";
            labelDate.Size = new Size(291, 28);
            labelDate.TabIndex = 4;
            labelDate.Text = "...";
            labelDate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelMessage
            // 
            labelMessage.AutoEllipsis = true;
            labelMessage.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(labelMessage, 2);
            labelMessage.Dock = DockStyle.Fill;
            labelMessage.Location = new Point(3, 0);
            labelMessage.MaximumSize = new Size(1000, 50);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(402, 20);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "...";
            labelMessage.TextAlign = ContentAlignment.MiddleLeft;
            labelMessage.SizeChanged += labelMessage_SizeChanged;
            // 
            // labelAuthor
            // 
            labelAuthor.AutoSize = true;
            labelAuthor.Dock = DockStyle.Fill;
            labelAuthor.Location = new Point(114, 40);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(291, 28);
            labelAuthor.TabIndex = 2;
            labelAuthor.Text = "...";
            labelAuthor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelTags
            // 
            labelTags.AutoSize = true;
            labelTags.BackColor = Color.LightSteelBlue;
            labelTags.Dock = DockStyle.Fill;
            labelTags.Location = new Point(114, 124);
            labelTags.Name = "labelTags";
            labelTags.Size = new Size(291, 28);
            labelTags.TabIndex = 8;
            labelTags.Text = "...";
            labelTags.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelBranches
            // 
            labelBranches.AutoSize = true;
            labelBranches.Dock = DockStyle.Fill;
            labelBranches.Location = new Point(114, 96);
            labelBranches.Name = "labelBranches";
            labelBranches.Size = new Size(291, 28);
            labelBranches.TabIndex = 6;
            labelBranches.Text = "...";
            labelBranches.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelTagsCaption
            // 
            labelTagsCaption.AutoSize = true;
            labelTagsCaption.Dock = DockStyle.Fill;
            labelTagsCaption.Location = new Point(4, 128);
            labelTagsCaption.Margin = new Padding(4);
            labelTagsCaption.Name = "labelTagsCaption";
            labelTagsCaption.Size = new Size(103, 20);
            labelTagsCaption.TabIndex = 7;
            labelTagsCaption.Text = "Tag(s):";
            labelTagsCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelBranchesCaption
            // 
            labelBranchesCaption.AutoSize = true;
            labelBranchesCaption.Dock = DockStyle.Fill;
            labelBranchesCaption.Location = new Point(4, 100);
            labelBranchesCaption.Margin = new Padding(4);
            labelBranchesCaption.Name = "labelBranchesCaption";
            labelBranchesCaption.Size = new Size(103, 20);
            labelBranchesCaption.TabIndex = 5;
            labelBranchesCaption.Text = "Branch(es):";
            labelBranchesCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelAuthorCaption
            // 
            labelAuthorCaption.AutoSize = true;
            labelAuthorCaption.Dock = DockStyle.Fill;
            labelAuthorCaption.Location = new Point(4, 44);
            labelAuthorCaption.Margin = new Padding(4);
            labelAuthorCaption.Name = "labelAuthorCaption";
            labelAuthorCaption.Size = new Size(103, 20);
            labelAuthorCaption.TabIndex = 1;
            labelAuthorCaption.Text = "Author:";
            labelAuthorCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(tableLayoutPanel1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(16);
            groupBox1.Size = new Size(440, 203);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "...";
            groupBox1.Resize += groupBox1_Resize;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(labelDateCaption, 0, 3);
            tableLayoutPanel1.Controls.Add(labelAuthorCaption, 0, 2);
            tableLayoutPanel1.Controls.Add(labelAuthor, 1, 2);
            tableLayoutPanel1.Controls.Add(labelMessage, 0, 0);
            tableLayoutPanel1.Controls.Add(labelBranches, 1, 4);
            tableLayoutPanel1.Controls.Add(labelBranchesCaption, 0, 4);
            tableLayoutPanel1.Controls.Add(labelTagsCaption, 0, 5);
            tableLayoutPanel1.Controls.Add(labelTags, 1, 5);
            tableLayoutPanel1.Controls.Add(labelDate, 1, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(16, 35);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(408, 152);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // labelDateCaption
            // 
            labelDateCaption.AutoSize = true;
            labelDateCaption.Dock = DockStyle.Fill;
            labelDateCaption.Location = new Point(4, 72);
            labelDateCaption.Margin = new Padding(4);
            labelDateCaption.Name = "labelDateCaption";
            labelDateCaption.Size = new Size(103, 20);
            labelDateCaption.TabIndex = 3;
            labelDateCaption.Text = "Commit date:";
            labelDateCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CommitSummaryUserControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(groupBox1);
            MinimumSize = new Size(440, 160);
            Name = "CommitSummaryUserControl";
            Size = new Size(440, 203);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label labelDate;
        private Label labelMessage;
        private Label labelAuthor;
        private Label labelTags;
        private Label labelBranches;
        private Label labelDateCaption;
        private Label labelAuthorCaption;
        private Label labelBranchesCaption;
        private Label labelTagsCaption;
        private GroupBox groupBox1;
    }
}

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
            this.labelDate = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.labelTags = new System.Windows.Forms.Label();
            this.labelBranches = new System.Windows.Forms.Label();
            this.labelTagsCaption = new System.Windows.Forms.Label();
            this.labelBranchesCaption = new System.Windows.Forms.Label();
            this.labelAuthorCaption = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelDateCaption = new System.Windows.Forms.Label();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDate.Location = new System.Drawing.Point(114, 68);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(291, 28);
            this.labelDate.TabIndex = 4;
            this.labelDate.Text = "...";
            this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoEllipsis = true;
            this.labelMessage.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(this.labelMessage, 2);
            this.labelMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelMessage.Location = new System.Drawing.Point(3, 0);
            this.labelMessage.MaximumSize = new System.Drawing.Size(1000, 50);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(402, 20);
            this.labelMessage.TabIndex = 0;
            this.labelMessage.Text = "...";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelMessage.SizeChanged += new System.EventHandler(this.labelMessage_SizeChanged);
            // 
            // labelAuthor
            // 
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAuthor.Location = new System.Drawing.Point(114, 40);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(291, 28);
            this.labelAuthor.TabIndex = 2;
            this.labelAuthor.Text = "...";
            this.labelAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTags
            // 
            this.labelTags.AutoSize = true;
            this.labelTags.BackColor = System.Drawing.Color.LightSteelBlue;
            this.labelTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTags.Location = new System.Drawing.Point(114, 124);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(291, 28);
            this.labelTags.TabIndex = 8;
            this.labelTags.Text = "...";
            this.labelTags.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelBranches
            // 
            this.labelBranches.AutoSize = true;
            this.labelBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelBranches.Location = new System.Drawing.Point(114, 96);
            this.labelBranches.Name = "labelBranches";
            this.labelBranches.Size = new System.Drawing.Size(291, 28);
            this.labelBranches.TabIndex = 6;
            this.labelBranches.Text = "...";
            this.labelBranches.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTagsCaption
            // 
            this.labelTagsCaption.AutoSize = true;
            this.labelTagsCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTagsCaption.Location = new System.Drawing.Point(4, 128);
            this.labelTagsCaption.Margin = new System.Windows.Forms.Padding(4);
            this.labelTagsCaption.Name = "labelTagsCaption";
            this.labelTagsCaption.Size = new System.Drawing.Size(103, 20);
            this.labelTagsCaption.TabIndex = 7;
            this.labelTagsCaption.Text = "Tag(s):";
            this.labelTagsCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelBranchesCaption
            // 
            this.labelBranchesCaption.AutoSize = true;
            this.labelBranchesCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelBranchesCaption.Location = new System.Drawing.Point(4, 100);
            this.labelBranchesCaption.Margin = new System.Windows.Forms.Padding(4);
            this.labelBranchesCaption.Name = "labelBranchesCaption";
            this.labelBranchesCaption.Size = new System.Drawing.Size(103, 20);
            this.labelBranchesCaption.TabIndex = 5;
            this.labelBranchesCaption.Text = "Branch(es):";
            this.labelBranchesCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelAuthorCaption
            // 
            this.labelAuthorCaption.AutoSize = true;
            this.labelAuthorCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAuthorCaption.Location = new System.Drawing.Point(4, 44);
            this.labelAuthorCaption.Margin = new System.Windows.Forms.Padding(4);
            this.labelAuthorCaption.Name = "labelAuthorCaption";
            this.labelAuthorCaption.Size = new System.Drawing.Size(103, 20);
            this.labelAuthorCaption.TabIndex = 1;
            this.labelAuthorCaption.Text = "Author:";
            this.labelAuthorCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(16);
            this.groupBox1.Size = new System.Drawing.Size(440, 203);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "...";
            this.groupBox1.Resize += new System.EventHandler(this.groupBox1_Resize);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(this.labelDateCaption, 0, 3);
            tableLayoutPanel1.Controls.Add(this.labelAuthorCaption, 0, 2);
            tableLayoutPanel1.Controls.Add(this.labelAuthor, 1, 2);
            tableLayoutPanel1.Controls.Add(this.labelMessage, 0, 0);
            tableLayoutPanel1.Controls.Add(this.labelBranches, 1, 4);
            tableLayoutPanel1.Controls.Add(this.labelBranchesCaption, 0, 4);
            tableLayoutPanel1.Controls.Add(this.labelTagsCaption, 0, 5);
            tableLayoutPanel1.Controls.Add(this.labelTags, 1, 5);
            tableLayoutPanel1.Controls.Add(this.labelDate, 1, 3);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(16, 35);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(408, 152);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // labelDateCaption
            // 
            this.labelDateCaption.AutoSize = true;
            this.labelDateCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDateCaption.Location = new System.Drawing.Point(4, 72);
            this.labelDateCaption.Margin = new System.Windows.Forms.Padding(4);
            this.labelDateCaption.Name = "labelDateCaption";
            this.labelDateCaption.Size = new System.Drawing.Size(103, 20);
            this.labelDateCaption.TabIndex = 3;
            this.labelDateCaption.Text = "Commit date:";
            this.labelDateCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CommitSummaryUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(440, 160);
            this.Name = "CommitSummaryUserControl";
            this.Size = new System.Drawing.Size(440, 203);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.Label labelBranches;
        private System.Windows.Forms.Label labelDateCaption;
        private System.Windows.Forms.Label labelAuthorCaption;
        private System.Windows.Forms.Label labelBranchesCaption;
        private System.Windows.Forms.Label labelTagsCaption;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

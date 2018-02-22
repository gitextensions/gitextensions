namespace GitUI.CommandsDialogs
{
    partial class FormRevertCommit
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
            this.commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.ParentsList = new UserControls.NativeListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.Revert = new System.Windows.Forms.Button();
            this.BranchInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.parentListPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.parentListPanel.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(20, 27);
            this.commitSummaryUserControl1.Margin = new System.Windows.Forms.Padding(20, 4, 4, 4);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(550, 200);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(658, 200);
            this.commitSummaryUserControl1.TabIndex = 16;
            // 
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(4, 0);
            this.ParentsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(298, 23);
            this.ParentsLabel.TabIndex = 15;
            this.ParentsLabel.Text = "This commit is a merge, select parent:";
            // 
            // ParentsList
            // 
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.ParentsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(4, 27);
            this.ParentsList.Margin = new System.Windows.Forms.Padding(4);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(668, 75);
            this.ParentsList.TabIndex = 14;
            this.ParentsList.UseCompatibleStateImageBehavior = false;
            this.ParentsList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No.";
            this.columnHeader1.Width = 50;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Message";
            this.columnHeader2.Width = 291;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Author";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Date";
            this.columnHeader4.Width = 80;
            // 
            // AutoCommit
            // 
            this.AutoCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(4, 10);
            this.AutoCommit.Margin = new System.Windows.Forms.Padding(4);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(265, 27);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically create a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // Revert
            // 
            this.Revert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Revert.AutoSize = true;
            this.Revert.Location = new System.Drawing.Point(501, 4);
            this.Revert.Margin = new System.Windows.Forms.Padding(4);
            this.Revert.Name = "Revert";
            this.Revert.Size = new System.Drawing.Size(171, 33);
            this.Revert.TabIndex = 10;
            this.Revert.Text = "Revert this commit";
            this.Revert.UseVisualStyleBackColor = true;
            this.Revert.Click += new System.EventHandler(this.Revert_Click);
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(4, 0);
            this.BranchInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(157, 23);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Revert this commit:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.BranchInfo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.commitSummaryUserControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.parentListPanel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(682, 390);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // parentListPanel
            // 
            this.parentListPanel.AutoSize = true;
            this.parentListPanel.ColumnCount = 1;
            this.parentListPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.parentListPanel.Controls.Add(this.ParentsLabel, 0, 0);
            this.parentListPanel.Controls.Add(this.ParentsList, 0, 1);
            this.parentListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parentListPanel.Location = new System.Drawing.Point(3, 234);
            this.parentListPanel.Name = "parentListPanel";
            this.parentListPanel.RowCount = 2;
            this.parentListPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.parentListPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.parentListPanel.Size = new System.Drawing.Size(676, 106);
            this.parentListPanel.TabIndex = 17;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.Revert, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.AutoCommit, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 346);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(676, 41);
            this.tableLayoutPanel3.TabIndex = 18;
            // 
            // FormRevertCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(682, 390);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 330);
            this.Name = "FormRevertCommit";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Revert commit";
            this.Load += new System.EventHandler(this.FormRevertCommit_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.parentListPanel.ResumeLayout(false);
            this.parentListPanel.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Revert;
        private System.Windows.Forms.CheckBox AutoCommit;
        private System.Windows.Forms.Label ParentsLabel;
        private UserControls.NativeListView ParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label BranchInfo;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel parentListPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}
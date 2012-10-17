namespace GitUI
{
    partial class FormCherryPickCommitSmall
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
            this.Pick = new System.Windows.Forms.Button();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.ParentsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.checkAddReference = new System.Windows.Forms.CheckBox();
            this.commitSummaryUserControl1 = new GitUI.CommitSummaryUserControl();
            this.SuspendLayout();
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(12, 9);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(137, 15);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Cherry pick this commit:";
            // 
            // Pick
            // 
            this.Pick.Location = new System.Drawing.Point(453, 226);
            this.Pick.Name = "Pick";
            this.Pick.Size = new System.Drawing.Size(109, 25);
            this.Pick.TabIndex = 10;
            this.Pick.Text = "Pick this commit";
            this.Pick.UseVisualStyleBackColor = true;
            this.Pick.Click += new System.EventHandler(this.Revert_Click);
            // 
            // AutoCommit
            // 
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(12, 216);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(194, 19);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically creates a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // ParentsList
            // 
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(12, 145);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(550, 65);
            this.ParentsList.TabIndex = 12;
            this.ParentsList.UseCompatibleStateImageBehavior = false;
            this.ParentsList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No.";
            this.columnHeader1.Width = 38;
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
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(12, 129);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(207, 15);
            this.ParentsLabel.TabIndex = 13;
            this.ParentsLabel.Text = "This commit is a merge, select parent:";
            // 
            // checkAddReference
            // 
            this.checkAddReference.AutoSize = true;
            this.checkAddReference.Location = new System.Drawing.Point(12, 236);
            this.checkAddReference.Name = "checkAddReference";
            this.checkAddReference.Size = new System.Drawing.Size(145, 19);
            this.checkAddReference.TabIndex = 14;
            this.checkAddReference.Text = "Add commit reference";
            this.checkAddReference.UseVisualStyleBackColor = true;
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(11, 27);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(549, 101);
            this.commitSummaryUserControl1.TabIndex = 15;
            // 
            // FormCherryPickCommitSmall
            // 
            this.AcceptButton = this.Pick;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(574, 262);
            this.Controls.Add(this.commitSummaryUserControl1);
            this.Controls.Add(this.checkAddReference);
            this.Controls.Add(this.ParentsLabel);
            this.Controls.Add(this.ParentsList);
            this.Controls.Add(this.AutoCommit);
            this.Controls.Add(this.Pick);
            this.Controls.Add(this.BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCherryPickCommitSmall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cherry pick commit";
            this.Load += new System.EventHandler(this.FormCherryPickCommitSmall_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Pick;
        private System.Windows.Forms.CheckBox AutoCommit;
        private System.Windows.Forms.ListView ParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label ParentsLabel;
        private System.Windows.Forms.CheckBox checkAddReference;
        private CommitSummaryUserControl commitSummaryUserControl1;
    }
}
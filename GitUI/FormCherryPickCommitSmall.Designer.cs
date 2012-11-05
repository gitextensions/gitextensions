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
            this.Pick = new System.Windows.Forms.Button();
            this.btnChooseRevision = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panelParentsList = new System.Windows.Forms.Panel();
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.ParentsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkAddReference = new System.Windows.Forms.CheckBox();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.BranchInfo = new System.Windows.Forms.Label();
            this.commitSummaryUserControl1 = new GitUI.CommitSummaryUserControl();
            this.panelParentsList.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pick
            // 
            this.Pick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Pick.Location = new System.Drawing.Point(533, 326);
            this.Pick.Name = "Pick";
            this.Pick.Size = new System.Drawing.Size(109, 25);
            this.Pick.TabIndex = 10;
            this.Pick.Text = "Cherry pick";
            this.Pick.UseVisualStyleBackColor = true;
            this.Pick.Click += new System.EventHandler(this.Revert_Click);
            // 
            // btnChooseRevision
            // 
            this.btnChooseRevision.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnChooseRevision.Location = new System.Drawing.Point(564, 60);
            this.btnChooseRevision.Name = "btnChooseRevision";
            this.btnChooseRevision.Size = new System.Drawing.Size(25, 24);
            this.btnChooseRevision.TabIndex = 33;
            this.btnChooseRevision.UseVisualStyleBackColor = true;
            this.btnChooseRevision.Click += new System.EventHandler(this.btnChooseRevision_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(561, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 30);
            this.label2.TabIndex = 34;
            this.label2.Text = "Choose another\r\nrevision:";
            // 
            // panelParentsList
            // 
            this.panelParentsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelParentsList.Controls.Add(this.ParentsLabel);
            this.panelParentsList.Controls.Add(this.ParentsList);
            this.panelParentsList.Location = new System.Drawing.Point(6, 202);
            this.panelParentsList.Name = "panelParentsList";
            this.panelParentsList.Size = new System.Drawing.Size(645, 95);
            this.panelParentsList.TabIndex = 16;
            this.panelParentsList.Visible = false;
            // 
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(6, 8);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(207, 15);
            this.ParentsLabel.TabIndex = 13;
            this.ParentsLabel.Text = "This commit is a merge, select parent:";
            // 
            // ParentsList
            // 
            this.ParentsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ParentsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ParentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.ParentsList.FullRowSelect = true;
            this.ParentsList.HideSelection = false;
            this.ParentsList.Location = new System.Drawing.Point(6, 24);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(630, 65);
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
            // checkAddReference
            // 
            this.checkAddReference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkAddReference.AutoSize = true;
            this.checkAddReference.Location = new System.Drawing.Point(12, 340);
            this.checkAddReference.Name = "checkAddReference";
            this.checkAddReference.Size = new System.Drawing.Size(253, 19);
            this.checkAddReference.TabIndex = 14;
            this.checkAddReference.Text = "Add commit reference to commit message";
            this.checkAddReference.UseVisualStyleBackColor = true;
            // 
            // AutoCommit
            // 
            this.AutoCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(12, 315);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(189, 19);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically create a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
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
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(11, 27);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(545, 166);
            this.commitSummaryUserControl1.TabIndex = 15;
            // 
            // FormCherryPickCommitSmall
            // 
            this.AcceptButton = this.Pick;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(654, 362);
            this.Controls.Add(this.btnChooseRevision);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panelParentsList);
            this.Controls.Add(this.commitSummaryUserControl1);
            this.Controls.Add(this.checkAddReference);
            this.Controls.Add(this.AutoCommit);
            this.Controls.Add(this.Pick);
            this.Controls.Add(this.BranchInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 400);
            this.Name = "FormCherryPickCommitSmall";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cherry pick commit";
            this.Load += new System.EventHandler(this.FormCherryPickCommitSmall_Load);
            this.panelParentsList.ResumeLayout(false);
            this.panelParentsList.PerformLayout();
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
        private System.Windows.Forms.Panel panelParentsList;
        private System.Windows.Forms.Button btnChooseRevision;
        private System.Windows.Forms.Label label2;
    }
}
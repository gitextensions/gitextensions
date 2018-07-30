using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    partial class FormCherryPick
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
            this.ParentsLabel = new System.Windows.Forms.Label();
            this.ParentsList = new UserControls.NativeListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkAddReference = new System.Windows.Forms.CheckBox();
            this.AutoCommit = new System.Windows.Forms.CheckBox();
            this.BranchInfo = new System.Windows.Forms.Label();
            this.commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.commitInformationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panelParentsList = new System.Windows.Forms.TableLayoutPanel();
            this.mainTableLayoutPanel.SuspendLayout();
            this.commitInformationPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panelParentsList.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pick
            // 
            this.Pick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Pick.Location = new System.Drawing.Point(586, 35);
            this.Pick.Margin = new System.Windows.Forms.Padding(4);
            this.Pick.Name = "Pick";
            this.tableLayoutPanel3.SetRowSpan(this.Pick, 2);
            this.Pick.Size = new System.Drawing.Size(136, 31);
            this.Pick.TabIndex = 10;
            this.Pick.Text = "Cherry pick";
            this.Pick.UseVisualStyleBackColor = true;
            this.Pick.Click += new System.EventHandler(this.Revert_Click);
            // 
            // btnChooseRevision
            // 
            this.btnChooseRevision.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseRevision.Image = global::GitUI.Properties.Images.SelectRevision;
            this.btnChooseRevision.Location = new System.Drawing.Point(105, 50);
            this.btnChooseRevision.Margin = new System.Windows.Forms.Padding(4);
            this.btnChooseRevision.Name = "btnChooseRevision";
            this.btnChooseRevision.Size = new System.Drawing.Size(31, 30);
            this.btnChooseRevision.TabIndex = 33;
            this.btnChooseRevision.UseVisualStyleBackColor = true;
            this.btnChooseRevision.Click += new System.EventHandler(this.btnChooseRevision_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 46);
            this.label2.TabIndex = 34;
            this.label2.Text = "Choose another\r\nrevision:";
            // 
            // ParentsLabel
            // 
            this.ParentsLabel.AutoSize = true;
            this.ParentsLabel.Location = new System.Drawing.Point(4, 0);
            this.ParentsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ParentsLabel.Name = "ParentsLabel";
            this.ParentsLabel.Size = new System.Drawing.Size(298, 23);
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
            this.ParentsList.Location = new System.Drawing.Point(0, 23);
            this.ParentsList.Margin = new System.Windows.Forms.Padding(0);
            this.ParentsList.MultiSelect = false;
            this.ParentsList.Name = "ParentsList";
            this.ParentsList.Size = new System.Drawing.Size(726, 109);
            this.ParentsList.TabIndex = 12;
            this.ParentsList.UseCompatibleStateImageBehavior = false;
            this.ParentsList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "No.";
            this.columnHeader1.Width = 43;
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
            this.checkAddReference.Location = new System.Drawing.Point(4, 39);
            this.checkAddReference.Margin = new System.Windows.Forms.Padding(4);
            this.checkAddReference.Name = "checkAddReference";
            this.checkAddReference.Size = new System.Drawing.Size(357, 27);
            this.checkAddReference.TabIndex = 14;
            this.checkAddReference.Text = "Add commit reference to commit message";
            this.checkAddReference.UseVisualStyleBackColor = true;
            // 
            // AutoCommit
            // 
            this.AutoCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoCommit.AutoSize = true;
            this.AutoCommit.Location = new System.Drawing.Point(4, 4);
            this.AutoCommit.Margin = new System.Windows.Forms.Padding(4);
            this.AutoCommit.Name = "AutoCommit";
            this.AutoCommit.Size = new System.Drawing.Size(265, 27);
            this.AutoCommit.TabIndex = 11;
            this.AutoCommit.Text = "Automatically create a commit";
            this.AutoCommit.UseVisualStyleBackColor = true;
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(4, 0);
            this.BranchInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(194, 23);
            this.BranchInfo.TabIndex = 5;
            this.BranchInfo.Text = "Cherry pick this commit:";
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(20, 4);
            this.commitSummaryUserControl1.Margin = new System.Windows.Forms.Padding(20, 4, 4, 4);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(550, 200);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(556, 200);
            this.commitSummaryUserControl1.TabIndex = 15;
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.AutoSize = true;
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.BranchInfo, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.commitInformationPanel, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.tableLayoutPanel3, 0, 3);
            this.mainTableLayoutPanel.Controls.Add(this.panelParentsList, 0, 2);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(732, 451);
            this.mainTableLayoutPanel.TabIndex = 35;
            // 
            // commitInformationPanel
            // 
            this.commitInformationPanel.AutoSize = true;
            this.commitInformationPanel.ColumnCount = 2;
            this.commitInformationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.commitInformationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.commitInformationPanel.Controls.Add(this.commitSummaryUserControl1, 0, 0);
            this.commitInformationPanel.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.commitInformationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitInformationPanel.Location = new System.Drawing.Point(3, 26);
            this.commitInformationPanel.Name = "commitInformationPanel";
            this.commitInformationPanel.RowCount = 1;
            this.commitInformationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.commitInformationPanel.Size = new System.Drawing.Size(726, 208);
            this.commitInformationPanel.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.btnChooseRevision);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(583, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(140, 202);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.Pick, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.checkAddReference, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.AutoCommit, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 378);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(726, 70);
            this.tableLayoutPanel3.TabIndex = 17;
            // 
            // panelParentsList
            // 
            this.panelParentsList.ColumnCount = 1;
            this.panelParentsList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelParentsList.Controls.Add(this.ParentsLabel, 0, 0);
            this.panelParentsList.Controls.Add(this.ParentsList, 0, 1);
            this.panelParentsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelParentsList.Location = new System.Drawing.Point(3, 240);
            this.panelParentsList.Name = "panelParentsList";
            this.panelParentsList.RowCount = 2;
            this.panelParentsList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelParentsList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelParentsList.Size = new System.Drawing.Size(726, 132);
            this.panelParentsList.TabIndex = 18;
            // 
            // FormCherryPick
            // 
            this.AcceptButton = this.Pick;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(732, 451);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(750, 360);
            this.Name = "FormCherryPick";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cherry pick commit";
            this.Load += new System.EventHandler(this.Form_Load);
            this.FormClosing += new FormClosingEventHandler(this.Form_Closing);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.commitInformationPanel.ResumeLayout(false);
            this.commitInformationPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.panelParentsList.ResumeLayout(false);
            this.panelParentsList.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Pick;
        private System.Windows.Forms.CheckBox AutoCommit;
        private UserControls.NativeListView ParentsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label ParentsLabel;
        private System.Windows.Forms.CheckBox checkAddReference;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private System.Windows.Forms.Button btnChooseRevision;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel commitInformationPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel panelParentsList;
    }
}
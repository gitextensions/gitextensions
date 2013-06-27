namespace Stash
{
    partial class StashPullRequestForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSourceBranch = new System.Windows.Forms.TextBox();
            this.txtTargetBranch = new System.Windows.Forms.TextBox();
            this.lblReviewers = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.ReviewersDataGrid = new System.Windows.Forms.DataGridView();
            this.GridColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ddlRepositorySource = new System.Windows.Forms.ComboBox();
            this.lblCommitInfoSource = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ddlRepositoryTarget = new System.Windows.Forms.ComboBox();
            this.lblCommitInfoTarget = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.ReviewersDataGrid)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Branch";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Branch";
            // 
            // txtSourceBranch
            // 
            this.txtSourceBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtSourceBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtSourceBranch.Location = new System.Drawing.Point(133, 56);
            this.txtSourceBranch.Name = "txtSourceBranch";
            this.txtSourceBranch.Size = new System.Drawing.Size(184, 20);
            this.txtSourceBranch.TabIndex = 1;
            // 
            // txtTargetBranch
            // 
            this.txtTargetBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTargetBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtTargetBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtTargetBranch.Location = new System.Drawing.Point(134, 56);
            this.txtTargetBranch.Name = "txtTargetBranch";
            this.txtTargetBranch.Size = new System.Drawing.Size(184, 20);
            this.txtTargetBranch.TabIndex = 1;
            // 
            // lblReviewers
            // 
            this.lblReviewers.AutoSize = true;
            this.lblReviewers.Location = new System.Drawing.Point(15, 38);
            this.lblReviewers.Name = "lblReviewers";
            this.lblReviewers.Size = new System.Drawing.Size(63, 13);
            this.lblReviewers.TabIndex = 4;
            this.lblReviewers.Text = "Reviewer(s)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 224);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Description";
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(128, 221);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(583, 104);
            this.txtDescription.TabIndex = 2;
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(647, 570);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(134, 23);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.BtnCreateClick);
            // 
            // ReviewersDataGrid
            // 
            this.ReviewersDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReviewersDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ReviewersDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GridColumnName});
            this.ReviewersDataGrid.Location = new System.Drawing.Point(129, 38);
            this.ReviewersDataGrid.Name = "ReviewersDataGrid";
            this.ReviewersDataGrid.Size = new System.Drawing.Size(583, 137);
            this.ReviewersDataGrid.TabIndex = 0;
            this.ReviewersDataGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.ReviewersDataGridEditingControlShowing);
            // 
            // GridColumnName
            // 
            this.GridColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.GridColumnName.DataPropertyName = "Slug";
            this.GridColumnName.HeaderText = "Name";
            this.GridColumnName.Name = "GridColumnName";
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtTitle.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtTitle.Location = new System.Drawing.Point(129, 189);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(583, 20);
            this.txtTitle.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Title";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Repository";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ddlRepositorySource);
            this.groupBox1.Controls.Add(this.lblCommitInfoSource);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtSourceBranch);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(15, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(372, 151);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // ddlRepositorySource
            // 
            this.ddlRepositorySource.DisplayMember = "DisplayName";
            this.ddlRepositorySource.FormattingEnabled = true;
            this.ddlRepositorySource.Location = new System.Drawing.Point(134, 24);
            this.ddlRepositorySource.Name = "ddlRepositorySource";
            this.ddlRepositorySource.Size = new System.Drawing.Size(183, 21);
            this.ddlRepositorySource.TabIndex = 0;
            this.ddlRepositorySource.SelectedValueChanged += new System.EventHandler(this.DdlRepositorySourceSelectedValueChanged);
            // 
            // lblCommitInfoSource
            // 
            this.lblCommitInfoSource.AutoSize = true;
            this.lblCommitInfoSource.Location = new System.Drawing.Point(131, 90);
            this.lblCommitInfoSource.Name = "lblCommitInfoSource";
            this.lblCommitInfoSource.Size = new System.Drawing.Size(0, 13);
            this.lblCommitInfoSource.TabIndex = 14;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ddlRepositoryTarget);
            this.groupBox2.Controls.Add(this.lblCommitInfoTarget);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtTargetBranch);
            this.groupBox2.Location = new System.Drawing.Point(409, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(372, 151);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Target";
            // 
            // ddlRepositoryTarget
            // 
            this.ddlRepositoryTarget.DisplayMember = "DisplayName";
            this.ddlRepositoryTarget.FormattingEnabled = true;
            this.ddlRepositoryTarget.Location = new System.Drawing.Point(134, 24);
            this.ddlRepositoryTarget.Name = "ddlRepositoryTarget";
            this.ddlRepositoryTarget.Size = new System.Drawing.Size(183, 21);
            this.ddlRepositoryTarget.TabIndex = 0;
            this.ddlRepositoryTarget.SelectedValueChanged += new System.EventHandler(this.DdlRepositoryTargetSelectedValueChanged);
            // 
            // lblCommitInfoTarget
            // 
            this.lblCommitInfoTarget.AutoSize = true;
            this.lblCommitInfoTarget.Location = new System.Drawing.Point(131, 90);
            this.lblCommitInfoTarget.Name = "lblCommitInfoTarget";
            this.lblCommitInfoTarget.Size = new System.Drawing.Size(0, 13);
            this.lblCommitInfoTarget.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Repository";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblReviewers);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.txtDescription);
            this.groupBox3.Controls.Add(this.txtTitle);
            this.groupBox3.Controls.Add(this.ReviewersDataGrid);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(15, 187);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(766, 364);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Pull Request Info";
            // 
            // StashPullRequestForm
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 626);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCreate);
            this.Name = "StashPullRequestForm";
            this.Text = "Create Pull Request";
            this.Load += new System.EventHandler(this.StashPullRequestFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.ReviewersDataGrid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSourceBranch;
        private System.Windows.Forms.TextBox txtTargetBranch;
        private System.Windows.Forms.Label lblReviewers;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.DataGridView ReviewersDataGrid;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridColumnName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblCommitInfoSource;
        private System.Windows.Forms.Label lblCommitInfoTarget;
        private System.Windows.Forms.ComboBox ddlRepositorySource;
        private System.Windows.Forms.ComboBox ddlRepositoryTarget;
    }
}
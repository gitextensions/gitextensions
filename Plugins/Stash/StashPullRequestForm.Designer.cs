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
            ((System.ComponentModel.ISupportInitialize)(this.ReviewersDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Branch";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Target Branch";
            // 
            // txtSourceBranch
            // 
            this.txtSourceBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtSourceBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtSourceBranch.Location = new System.Drawing.Point(131, 21);
            this.txtSourceBranch.Name = "txtSourceBranch";
            this.txtSourceBranch.Size = new System.Drawing.Size(349, 20);
            this.txtSourceBranch.TabIndex = 0;
            // 
            // txtTargetBranch
            // 
            this.txtTargetBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTargetBranch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtTargetBranch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtTargetBranch.Location = new System.Drawing.Point(131, 47);
            this.txtTargetBranch.Name = "txtTargetBranch";
            this.txtTargetBranch.Size = new System.Drawing.Size(349, 20);
            this.txtTargetBranch.TabIndex = 1;
            // 
            // lblReviewers
            // 
            this.lblReviewers.AutoSize = true;
            this.lblReviewers.Location = new System.Drawing.Point(12, 73);
            this.lblReviewers.Name = "lblReviewers";
            this.lblReviewers.Size = new System.Drawing.Size(63, 13);
            this.lblReviewers.TabIndex = 4;
            this.lblReviewers.Text = "Reviewer(s)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 245);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Description";
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(131, 242);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(349, 104);
            this.txtDescription.TabIndex = 4;
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(346, 370);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(134, 23);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "Create Pull Request";
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
            this.ReviewersDataGrid.Location = new System.Drawing.Point(131, 73);
            this.ReviewersDataGrid.Name = "ReviewersDataGrid";
            this.ReviewersDataGrid.Size = new System.Drawing.Size(349, 137);
            this.ReviewersDataGrid.TabIndex = 2;
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
            this.txtTitle.Location = new System.Drawing.Point(131, 216);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(349, 20);
            this.txtTitle.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 219);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Title";
            // 
            // StashPullRequestForm
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 418);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ReviewersDataGrid);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblReviewers);
            this.Controls.Add(this.txtTargetBranch);
            this.Controls.Add(this.txtSourceBranch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "StashPullRequestForm";
            this.Text = "Pull Request";
            this.Load += new System.EventHandler(this.StashPullRequestFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.ReviewersDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
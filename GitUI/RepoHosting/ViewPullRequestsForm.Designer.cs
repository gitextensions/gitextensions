namespace GitUI.RepoHosting
{
    partial class ViewPullRequestsForm
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
            this._selectedOwner = new System.Windows.Forms.ComboBox();
            this._pullRequestsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._pullRequestBody = new System.Windows.Forms.TextBox();
            this._chooseRepo = new System.Windows.Forms.Label();
            this._fetchBtn = new System.Windows.Forms.Button();
            this._diffViewer = new GitUI.Editor.FileViewer();
            this.SuspendLayout();
            // 
            // _selectedOwner
            // 
            this._selectedOwner.DisplayMember = "DisplayData";
            this._selectedOwner.FormattingEnabled = true;
            this._selectedOwner.Location = new System.Drawing.Point(112, 9);
            this._selectedOwner.Name = "_selectedOwner";
            this._selectedOwner.Size = new System.Drawing.Size(258, 21);
            this._selectedOwner.TabIndex = 0;
            this._selectedOwner.SelectedIndexChanged += new System.EventHandler(this._selectedOwner_SelectedIndexChanged);
            // 
            // _pullRequestsList
            // 
            this._pullRequestsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._pullRequestsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this._pullRequestsList.FullRowSelect = true;
            this._pullRequestsList.HideSelection = false;
            this._pullRequestsList.Location = new System.Drawing.Point(12, 36);
            this._pullRequestsList.MultiSelect = false;
            this._pullRequestsList.Name = "_pullRequestsList";
            this._pullRequestsList.Size = new System.Drawing.Size(725, 116);
            this._pullRequestsList.TabIndex = 1;
            this._pullRequestsList.UseCompatibleStateImageBehavior = false;
            this._pullRequestsList.View = System.Windows.Forms.View.Details;
            this._pullRequestsList.SelectedIndexChanged += new System.EventHandler(this._pullRequestsList_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "#";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Heading";
            this.columnHeader2.Width = 402;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "By";
            this.columnHeader3.Width = 121;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Created";
            this.columnHeader4.Width = 136;
            // 
            // _pullRequestBody
            // 
            this._pullRequestBody.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pullRequestBody.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._pullRequestBody.Location = new System.Drawing.Point(743, 36);
            this._pullRequestBody.Multiline = true;
            this._pullRequestBody.Name = "_pullRequestBody";
            this._pullRequestBody.ReadOnly = true;
            this._pullRequestBody.Size = new System.Drawing.Size(446, 116);
            this._pullRequestBody.TabIndex = 2;
            // 
            // _chooseRepo
            // 
            this._chooseRepo.AutoSize = true;
            this._chooseRepo.Location = new System.Drawing.Point(12, 12);
            this._chooseRepo.Name = "_chooseRepo";
            this._chooseRepo.Size = new System.Drawing.Size(94, 13);
            this._chooseRepo.TabIndex = 4;
            this._chooseRepo.Text = "Choose repository:";
            // 
            // _fetchBtn
            // 
            this._fetchBtn.Location = new System.Drawing.Point(603, 158);
            this._fetchBtn.Name = "_fetchBtn";
            this._fetchBtn.Size = new System.Drawing.Size(134, 29);
            this._fetchBtn.TabIndex = 5;
            this._fetchBtn.Text = "Fetch and Review";
            this._fetchBtn.UseVisualStyleBackColor = true;
            this._fetchBtn.Click += new System.EventHandler(this._fetchBtn_Click);
            // 
            // _diffViewer
            // 
            this._diffViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._diffViewer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._diffViewer.IgnoreWhitespaceChanges = false;
            this._diffViewer.IsReadOnly = true;
            this._diffViewer.Location = new System.Drawing.Point(12, 193);
            this._diffViewer.Name = "_diffViewer";
            this._diffViewer.NumberOfVisibleLines = 3;
            this._diffViewer.ScrollPos = 0;
            this._diffViewer.ShowEntireFile = false;
            this._diffViewer.ShowLineNumbers = true;
            this._diffViewer.Size = new System.Drawing.Size(1177, 402);
            this._diffViewer.TabIndex = 0;
            this._diffViewer.TreatAllFilesAsText = false;
            // 
            // ViewPullRequestsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1199, 607);
            this.Controls.Add(this._fetchBtn);
            this.Controls.Add(this._diffViewer);
            this.Controls.Add(this._chooseRepo);
            this.Controls.Add(this._pullRequestBody);
            this.Controls.Add(this._pullRequestsList);
            this.Controls.Add(this._selectedOwner);
            this.Name = "ViewPullRequestsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View Pull Requests";
            this.Load += new System.EventHandler(this.ViewPullRequestsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _selectedOwner;
        private System.Windows.Forms.ListView _pullRequestsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TextBox _pullRequestBody;
        private Editor.FileViewer _diffViewer;
        private System.Windows.Forms.Label _chooseRepo;
        private System.Windows.Forms.Button _fetchBtn;
    }
}
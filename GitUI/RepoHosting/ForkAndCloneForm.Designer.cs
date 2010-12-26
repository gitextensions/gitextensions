namespace GitUI.RepoHosting
{
    partial class ForkAndCloneForm
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
            System.Windows.Forms.ColumnHeader columnHeader1;
            System.Windows.Forms.ColumnHeader columnHeader3;
            System.Windows.Forms.ColumnHeader columnHeader4;
            System.Windows.Forms.ColumnHeader columnHeader5;
            System.Windows.Forms.ColumnHeader columnHeader7;
            System.Windows.Forms.ColumnHeader columnHeader8;
            this._searchBtn = new System.Windows.Forms.Button();
            this._searchTB = new System.Windows.Forms.TextBox();
            this._forkBtn = new System.Windows.Forms.Button();
            this._myReposLV = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._searchResultsLV = new System.Windows.Forms.ListView();
            this._cloneBtn = new System.Windows.Forms.Button();
            this._cloneToTB = new System.Windows.Forms.TextBox();
            this._browseForCloneToDirbtn = new System.Windows.Forms.Button();
            this._cloneForeignBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._descriptionLbl = new System.Windows.Forms.Label();
            this._searchResultItemDescription = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._openGitupPageBtn = new System.Windows.Forms.Button();
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Name";
            columnHeader1.Width = 180;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Is fork";
            columnHeader3.Width = 45;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "# Forks";
            columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Name";
            columnHeader5.Width = 180;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "Forks";
            columnHeader7.Width = 40;
            // 
            // columnHeader8
            // 
            columnHeader8.Text = "Owner";
            columnHeader8.Width = 130;
            // 
            // _searchBtn
            // 
            this._searchBtn.Location = new System.Drawing.Point(210, 17);
            this._searchBtn.Name = "_searchBtn";
            this._searchBtn.Size = new System.Drawing.Size(75, 23);
            this._searchBtn.TabIndex = 1;
            this._searchBtn.Text = "Search";
            this._searchBtn.UseVisualStyleBackColor = true;
            this._searchBtn.Click += new System.EventHandler(this._searchBtn_Click);
            // 
            // _searchTB
            // 
            this._searchTB.Location = new System.Drawing.Point(6, 19);
            this._searchTB.Name = "_searchTB";
            this._searchTB.Size = new System.Drawing.Size(198, 20);
            this._searchTB.TabIndex = 2;
            this._searchTB.Enter += new System.EventHandler(this._searchTB_Enter);
            this._searchTB.Leave += new System.EventHandler(this._searchTB_Leave);
            // 
            // _forkBtn
            // 
            this._forkBtn.Location = new System.Drawing.Point(6, 239);
            this._forkBtn.Name = "_forkBtn";
            this._forkBtn.Size = new System.Drawing.Size(75, 23);
            this._forkBtn.TabIndex = 6;
            this._forkBtn.Text = "Fork";
            this._forkBtn.UseVisualStyleBackColor = true;
            this._forkBtn.Click += new System.EventHandler(this._forkBtn_Click);
            // 
            // _myReposLV
            // 
            this._myReposLV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._myReposLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader3,
            columnHeader4,
            this.columnHeader2});
            this._myReposLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._myReposLV.Location = new System.Drawing.Point(6, 19);
            this._myReposLV.MultiSelect = false;
            this._myReposLV.Name = "_myReposLV";
            this._myReposLV.ShowGroups = false;
            this._myReposLV.Size = new System.Drawing.Size(391, 129);
            this._myReposLV.TabIndex = 7;
            this._myReposLV.UseCompatibleStateImageBehavior = false;
            this._myReposLV.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Private";
            this.columnHeader2.Width = 45;
            // 
            // _searchResultsLV
            // 
            this._searchResultsLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader5,
            columnHeader8,
            columnHeader7});
            this._searchResultsLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._searchResultsLV.Location = new System.Drawing.Point(6, 46);
            this._searchResultsLV.MultiSelect = false;
            this._searchResultsLV.Name = "_searchResultsLV";
            this._searchResultsLV.ShowGroups = false;
            this._searchResultsLV.Size = new System.Drawing.Size(397, 187);
            this._searchResultsLV.TabIndex = 8;
            this._searchResultsLV.UseCompatibleStateImageBehavior = false;
            this._searchResultsLV.View = System.Windows.Forms.View.Details;
            this._searchResultsLV.SelectedIndexChanged += new System.EventHandler(this._searchResultsLV_SelectedIndexChanged);
            // 
            // _cloneBtn
            // 
            this._cloneBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cloneBtn.Location = new System.Drawing.Point(6, 154);
            this._cloneBtn.Name = "_cloneBtn";
            this._cloneBtn.Size = new System.Drawing.Size(75, 23);
            this._cloneBtn.TabIndex = 9;
            this._cloneBtn.Text = "Clone";
            this._cloneBtn.UseVisualStyleBackColor = true;
            this._cloneBtn.Click += new System.EventHandler(this._cloneBtn_Click);
            // 
            // _cloneToTB
            // 
            this._cloneToTB.Location = new System.Drawing.Point(5, 105);
            this._cloneToTB.Name = "_cloneToTB";
            this._cloneToTB.Size = new System.Drawing.Size(304, 20);
            this._cloneToTB.TabIndex = 10;
            // 
            // _browseForCloneToDirbtn
            // 
            this._browseForCloneToDirbtn.Location = new System.Drawing.Point(5, 131);
            this._browseForCloneToDirbtn.Name = "_browseForCloneToDirbtn";
            this._browseForCloneToDirbtn.Size = new System.Drawing.Size(75, 23);
            this._browseForCloneToDirbtn.TabIndex = 11;
            this._browseForCloneToDirbtn.Text = "Browse...";
            this._browseForCloneToDirbtn.UseVisualStyleBackColor = true;
            this._browseForCloneToDirbtn.Click += new System.EventHandler(this._browseForCloneToDirbtn_Click);
            // 
            // _cloneForeignBtn
            // 
            this._cloneForeignBtn.Location = new System.Drawing.Point(87, 239);
            this._cloneForeignBtn.Name = "_cloneForeignBtn";
            this._cloneForeignBtn.Size = new System.Drawing.Size(117, 23);
            this._cloneForeignBtn.TabIndex = 13;
            this._cloneForeignBtn.Text = "Clone without fork";
            this._cloneForeignBtn.UseVisualStyleBackColor = true;
            this._cloneForeignBtn.Click += new System.EventHandler(this._cloneForeignBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._browseForCloneToDirbtn);
            this.groupBox1.Controls.Add(this._cloneToTB);
            this.groupBox1.Location = new System.Drawing.Point(431, 305);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(316, 164);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Clone setup";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._myReposLV);
            this.groupBox2.Controls.Add(this._cloneBtn);
            this.groupBox2.Location = new System.Drawing.Point(12, 286);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(403, 183);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Your repositories:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._openGitupPageBtn);
            this.groupBox3.Controls.Add(this._searchResultItemDescription);
            this.groupBox3.Controls.Add(this._descriptionLbl);
            this.groupBox3.Controls.Add(this._searchTB);
            this.groupBox3.Controls.Add(this._searchBtn);
            this.groupBox3.Controls.Add(this._cloneForeignBtn);
            this.groupBox3.Controls.Add(this._forkBtn);
            this.groupBox3.Controls.Add(this._searchResultsLV);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(728, 268);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Search for repository:";
            // 
            // _descriptionLbl
            // 
            this._descriptionLbl.AutoSize = true;
            this._descriptionLbl.Location = new System.Drawing.Point(409, 46);
            this._descriptionLbl.Name = "_descriptionLbl";
            this._descriptionLbl.Size = new System.Drawing.Size(63, 13);
            this._descriptionLbl.TabIndex = 17;
            this._descriptionLbl.Text = "Description:";
            // 
            // _searchResultItemDescription
            // 
            this._searchResultItemDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._searchResultItemDescription.Enabled = false;
            this._searchResultItemDescription.Location = new System.Drawing.Point(412, 71);
            this._searchResultItemDescription.Multiline = true;
            this._searchResultItemDescription.Name = "_searchResultItemDescription";
            this._searchResultItemDescription.Size = new System.Drawing.Size(310, 162);
            this._searchResultItemDescription.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Destination folder:";
            // 
            // _openGitupPageBtn
            // 
            this._openGitupPageBtn.Location = new System.Drawing.Point(606, 239);
            this._openGitupPageBtn.Name = "_openGitupPageBtn";
            this._openGitupPageBtn.Size = new System.Drawing.Size(116, 23);
            this._openGitupPageBtn.TabIndex = 19;
            this._openGitupPageBtn.Text = "Open github page";
            this._openGitupPageBtn.UseVisualStyleBackColor = true;
            this._openGitupPageBtn.Click += new System.EventHandler(this._openGitupPageBtn_Click);
            // 
            // ForkAndCloneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 482);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ForkAndCloneForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Remote repository fork and clone";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _searchBtn;
        private System.Windows.Forms.TextBox _searchTB;
        private System.Windows.Forms.Button _forkBtn;
        private System.Windows.Forms.ListView _myReposLV;
        private System.Windows.Forms.ListView _searchResultsLV;
        private System.Windows.Forms.Button _cloneBtn;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox _cloneToTB;
        private System.Windows.Forms.Button _browseForCloneToDirbtn;
        private System.Windows.Forms.Button _cloneForeignBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label _descriptionLbl;
        private System.Windows.Forms.TextBox _searchResultItemDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _openGitupPageBtn;
    }
}
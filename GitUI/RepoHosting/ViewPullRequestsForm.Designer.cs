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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _selectedOwner
            // 
            this._selectedOwner.FormattingEnabled = true;
            this._selectedOwner.Location = new System.Drawing.Point(12, 12);
            this._selectedOwner.Name = "_selectedOwner";
            this._selectedOwner.Size = new System.Drawing.Size(121, 21);
            this._selectedOwner.TabIndex = 0;
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
            this._pullRequestsList.HideSelection = false;
            this._pullRequestsList.Location = new System.Drawing.Point(12, 53);
            this._pullRequestsList.MultiSelect = false;
            this._pullRequestsList.Name = "_pullRequestsList";
            this._pullRequestsList.Size = new System.Drawing.Size(696, 116);
            this._pullRequestsList.TabIndex = 1;
            this._pullRequestsList.UseCompatibleStateImageBehavior = false;
            this._pullRequestsList.View = System.Windows.Forms.View.Details;
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
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox1.Location = new System.Drawing.Point(12, 188);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(696, 178);
            this.textBox1.TabIndex = 2;
            // 
            // ViewPullRequestsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 635);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this._pullRequestsList);
            this.Controls.Add(this._selectedOwner);
            this.Name = "ViewPullRequestsForm";
            this.Text = "ViewPullRequestsForm";
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
        private System.Windows.Forms.TextBox textBox1;
    }
}
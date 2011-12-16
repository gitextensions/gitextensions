namespace GitUI
{
    partial class FormFormatPullRequest
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonFormatPullRequest = new System.Windows.Forms.Button();
            this.lblURL = new System.Windows.Forms.Label();
            this.pullURL = new System.Windows.Forms.TextBox();
            this.requestText = new System.Windows.Forms.TextBox();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonFormatPullRequest);
            this.splitContainer1.Panel1.Controls.Add(this.lblURL);
            this.splitContainer1.Panel1.Controls.Add(this.pullURL);
            this.splitContainer1.Panel1.Controls.Add(this.requestText);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.RevisionGrid);
            this.splitContainer1.Size = new System.Drawing.Size(814, 522);
            this.splitContainer1.SplitterDistance = 271;
            this.splitContainer1.TabIndex = 1;
            // 
            // buttonFormatPullRequest
            // 
            this.buttonFormatPullRequest.Location = new System.Drawing.Point(666, 236);
            this.buttonFormatPullRequest.Name = "buttonFormatPullRequest";
            this.buttonFormatPullRequest.Size = new System.Drawing.Size(136, 23);
            this.buttonFormatPullRequest.TabIndex = 3;
            this.buttonFormatPullRequest.Text = "Format Pull Request";
            this.buttonFormatPullRequest.UseVisualStyleBackColor = true;
            this.buttonFormatPullRequest.Click += new System.EventHandler(this.buttonFormatPullRequest_Click);
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(7, 36);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(26, 13);
            this.lblURL.TabIndex = 2;
            this.lblURL.Text = "URL";
            // 
            // pullURL
            // 
            this.pullURL.Location = new System.Drawing.Point(41, 33);
            this.pullURL.Name = "pullURL";
            this.pullURL.Size = new System.Drawing.Size(601, 21);
            this.pullURL.TabIndex = 1;
            // 
            // requestText
            // 
            this.requestText.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.requestText.Location = new System.Drawing.Point(41, 62);
            this.requestText.Multiline = true;
            this.requestText.Name = "requestText";
            this.requestText.Size = new System.Drawing.Size(601, 197);
            this.requestText.TabIndex = 0;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.AllowGraphWithFilter = false;
            this.RevisionGrid.BranchFilter = "";
            this.RevisionGrid.CurrentCheckout = null;
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.RevisionGrid.InMemAuthorFilter = "";
            this.RevisionGrid.InMemCommitterFilter = "";
            this.RevisionGrid.InMemFilterIgnoreCase = false;
            this.RevisionGrid.InMemMessageFilter = "";
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 3);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Tahoma", 8.75F);
            this.RevisionGrid.Size = new System.Drawing.Size(814, 244);
            this.RevisionGrid.SuperprojectCurrentCheckout = null;
            this.RevisionGrid.TabIndex = 1;
            // 
            // FormFormatPullRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 522);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormFormatPullRequest";
            this.Text = "FormFormatPullRequest";
            this.Load += new System.EventHandler(this.FormFormatPullRequest_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.TextBox requestText;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.TextBox pullURL;
        private System.Windows.Forms.Button buttonFormatPullRequest;

    }
}
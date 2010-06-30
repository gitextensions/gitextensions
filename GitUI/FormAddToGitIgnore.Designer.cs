namespace GitUI
{
    partial class FormAddToGitIgnore
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
            this.FilePattern = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ShowPreview = new System.Windows.Forms.Button();
            this.AddToIngore = new System.Windows.Forms.Button();
            this.Preview = new System.Windows.Forms.ListBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File pattern to ignore";
            // 
            // FilePattern
            // 
            this.FilePattern.Location = new System.Drawing.Point(12, 39);
            this.FilePattern.Name = "FilePattern";
            this.FilePattern.Size = new System.Drawing.Size(460, 20);
            this.FilePattern.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ShowPreview);
            this.splitContainer1.Panel1.Controls.Add(this.AddToIngore);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.FilePattern);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Preview);
            this.splitContainer1.Size = new System.Drawing.Size(635, 328);
            this.splitContainer1.SplitterDistance = 82;
            this.splitContainer1.TabIndex = 2;
            // 
            // ShowPreview
            // 
            this.ShowPreview.Location = new System.Drawing.Point(510, 37);
            this.ShowPreview.Name = "ShowPreview";
            this.ShowPreview.Size = new System.Drawing.Size(113, 23);
            this.ShowPreview.TabIndex = 3;
            this.ShowPreview.Text = "Show preview";
            this.ShowPreview.UseVisualStyleBackColor = true;
            this.ShowPreview.Click += new System.EventHandler(this.ShowPreview_Click);
            // 
            // AddToIngore
            // 
            this.AddToIngore.Location = new System.Drawing.Point(510, 8);
            this.AddToIngore.Name = "AddToIngore";
            this.AddToIngore.Size = new System.Drawing.Size(113, 23);
            this.AddToIngore.TabIndex = 2;
            this.AddToIngore.Text = "Add to .gitignore";
            this.AddToIngore.UseVisualStyleBackColor = true;
            this.AddToIngore.Click += new System.EventHandler(this.AddToIngore_Click);
            // 
            // Preview
            // 
            this.Preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Preview.FormattingEnabled = true;
            this.Preview.Location = new System.Drawing.Point(0, 0);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(635, 238);
            this.Preview.TabIndex = 0;
            // 
            // FormAddToGitIgnore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 328);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAddToGitIgnore";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add files(s) to .gitIgnore";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FilePattern;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button ShowPreview;
        private System.Windows.Forms.Button AddToIngore;
        private System.Windows.Forms.ListBox Preview;
    }
}
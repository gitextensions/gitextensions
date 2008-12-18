namespace GitUI
{
    partial class FormPull
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPull));
            this.label1 = new System.Windows.Forms.Label();
            this.BrowseSource = new System.Windows.Forms.Button();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Pull = new System.Windows.Forms.Button();
            this.Output = new System.Windows.Forms.RichTextBox();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Merge = new System.Windows.Forms.RadioButton();
            this.Rebase = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Fetch = new System.Windows.Forms.RadioButton();
            this.PullSource = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source";
            // 
            // BrowseSource
            // 
            this.BrowseSource.Location = new System.Drawing.Point(457, 7);
            this.BrowseSource.Name = "BrowseSource";
            this.BrowseSource.Size = new System.Drawing.Size(104, 23);
            this.BrowseSource.TabIndex = 4;
            this.BrowseSource.Text = "Browse";
            this.BrowseSource.UseVisualStyleBackColor = true;
            this.BrowseSource.Click += new System.EventHandler(this.BrowseSource_Click);
            // 
            // Branches
            // 
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(105, 37);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(345, 21);
            this.Branches.TabIndex = 5;
            this.Branches.DropDown += new System.EventHandler(this.Branches_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Remote branch";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // Pull
            // 
            this.Pull.Location = new System.Drawing.Point(459, 35);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(102, 23);
            this.Pull.TabIndex = 7;
            this.Pull.Text = "Pull";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.Pull_Click);
            // 
            // Output
            // 
            this.Output.Location = new System.Drawing.Point(8, 172);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(442, 192);
            this.Output.TabIndex = 10;
            this.Output.Text = "";
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(456, 341);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(104, 23);
            this.Mergetool.TabIndex = 11;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Merge
            // 
            this.Merge.AutoSize = true;
            this.Merge.Checked = true;
            this.Merge.Location = new System.Drawing.Point(7, 20);
            this.Merge.Name = "Merge";
            this.Merge.Size = new System.Drawing.Size(210, 17);
            this.Merge.TabIndex = 0;
            this.Merge.TabStop = true;
            this.Merge.Text = "Merge remote branch to current branch";
            this.Merge.UseVisualStyleBackColor = true;
            // 
            // Rebase
            // 
            this.Rebase.AutoSize = true;
            this.Rebase.Location = new System.Drawing.Point(7, 41);
            this.Rebase.Name = "Rebase";
            this.Rebase.Size = new System.Drawing.Size(408, 17);
            this.Rebase.TabIndex = 1;
            this.Rebase.Text = "Rebase remote branch to current branch, creates linear history. (use with caution" +
                ")";
            this.Rebase.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Fetch);
            this.groupBox1.Controls.Add(this.Rebase);
            this.groupBox1.Controls.Add(this.Merge);
            this.groupBox1.Location = new System.Drawing.Point(12, 64);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 102);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Merge options";
            // 
            // Fetch
            // 
            this.Fetch.AutoSize = true;
            this.Fetch.Location = new System.Drawing.Point(7, 62);
            this.Fetch.Name = "Fetch";
            this.Fetch.Size = new System.Drawing.Size(212, 17);
            this.Fetch.TabIndex = 2;
            this.Fetch.TabStop = true;
            this.Fetch.Text = "Do not merge, only fetch remote branch";
            this.Fetch.UseVisualStyleBackColor = true;
            // 
            // PullSource
            // 
            this.PullSource.FormattingEnabled = true;
            this.PullSource.Location = new System.Drawing.Point(106, 9);
            this.PullSource.Name = "PullSource";
            this.PullSource.Size = new System.Drawing.Size(345, 21);
            this.PullSource.TabIndex = 13;
            this.PullSource.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.PullSource_DrawItem);
            this.PullSource.DropDown += new System.EventHandler(this.PullSource_DropDown);
            // 
            // FormPull
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 368);
            this.Controls.Add(this.PullSource);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Mergetool);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.Pull);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Branches);
            this.Controls.Add(this.BrowseSource);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPull";
            this.Text = "Pull";
            this.Load += new System.EventHandler(this.FormPull_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.RadioButton Merge;
        private System.Windows.Forms.RadioButton Rebase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Fetch;
        private System.Windows.Forms.ComboBox PullSource;
    }
}
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
            this.PullSource = new System.Windows.Forms.TextBox();
            this.BrowseSource = new System.Windows.Forms.Button();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Pull = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Output = new System.Windows.Forms.RichTextBox();
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
            // PullSource
            // 
            this.PullSource.Location = new System.Drawing.Point(105, 10);
            this.PullSource.Name = "PullSource";
            this.PullSource.Size = new System.Drawing.Size(345, 20);
            this.PullSource.TabIndex = 3;
            this.PullSource.TextChanged += new System.EventHandler(this.PullSource_TextChanged);
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
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(457, 73);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(104, 23);
            this.Mergetool.TabIndex = 11;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Output
            // 
            this.Output.Location = new System.Drawing.Point(8, 73);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(442, 185);
            this.Output.TabIndex = 10;
            this.Output.Text = "";
            // 
            // FormPull
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 274);
            this.Controls.Add(this.Mergetool);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.Pull);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Branches);
            this.Controls.Add(this.BrowseSource);
            this.Controls.Add(this.PullSource);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPull";
            this.Text = "Pull";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PullSource;
        private System.Windows.Forms.Button BrowseSource;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Pull;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.RichTextBox Output;
    }
}
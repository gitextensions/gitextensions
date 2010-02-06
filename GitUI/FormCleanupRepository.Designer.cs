namespace GitUI
{
    partial class FormCleanupRepository
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCleanupRepository));
            this.Preview = new System.Windows.Forms.Button();
            this.Cleanup = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RemoveIngnored = new System.Windows.Forms.RadioButton();
            this.RemoveNonIgnored = new System.Windows.Forms.RadioButton();
            this.RemoveAll = new System.Windows.Forms.RadioButton();
            this.RemoveDirectories = new System.Windows.Forms.CheckBox();
            this.PreviewOutput = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Preview
            // 
            this.Preview.Location = new System.Drawing.Point(305, 12);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(75, 23);
            this.Preview.TabIndex = 0;
            this.Preview.Text = "Preview";
            this.Preview.UseVisualStyleBackColor = true;
            this.Preview.Click += new System.EventHandler(this.Preview_Click);
            // 
            // Cleanup
            // 
            this.Cleanup.Location = new System.Drawing.Point(305, 42);
            this.Cleanup.Name = "Cleanup";
            this.Cleanup.Size = new System.Drawing.Size(75, 23);
            this.Cleanup.TabIndex = 1;
            this.Cleanup.Text = "Cleanup";
            this.Cleanup.UseVisualStyleBackColor = true;
            this.Cleanup.Click += new System.EventHandler(this.Cleanup_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(305, 71);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RemoveIngnored);
            this.groupBox1.Controls.Add(this.RemoveNonIgnored);
            this.groupBox1.Controls.Add(this.RemoveAll);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(278, 100);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cleanup repository";
            // 
            // RemoveIngnored
            // 
            this.RemoveIngnored.AutoSize = true;
            this.RemoveIngnored.Location = new System.Drawing.Point(7, 67);
            this.RemoveIngnored.Name = "RemoveIngnored";
            this.RemoveIngnored.Size = new System.Drawing.Size(203, 17);
            this.RemoveIngnored.TabIndex = 2;
            this.RemoveIngnored.Text = "Remove only ingnored untracked files";
            this.RemoveIngnored.UseVisualStyleBackColor = true;
            // 
            // RemoveNonIgnored
            // 
            this.RemoveNonIgnored.AutoSize = true;
            this.RemoveNonIgnored.Location = new System.Drawing.Point(7, 43);
            this.RemoveNonIgnored.Name = "RemoveNonIgnored";
            this.RemoveNonIgnored.Size = new System.Drawing.Size(218, 17);
            this.RemoveNonIgnored.TabIndex = 1;
            this.RemoveNonIgnored.Text = "Remove only non-ignored untracked files";
            this.RemoveNonIgnored.UseVisualStyleBackColor = true;
            // 
            // RemoveAll
            // 
            this.RemoveAll.AutoSize = true;
            this.RemoveAll.Checked = true;
            this.RemoveAll.Location = new System.Drawing.Point(7, 20);
            this.RemoveAll.Name = "RemoveAll";
            this.RemoveAll.Size = new System.Drawing.Size(150, 17);
            this.RemoveAll.TabIndex = 0;
            this.RemoveAll.TabStop = true;
            this.RemoveAll.Text = "Remove all untracked files";
            this.RemoveAll.UseVisualStyleBackColor = true;
            // 
            // RemoveDirectories
            // 
            this.RemoveDirectories.AutoSize = true;
            this.RemoveDirectories.Checked = true;
            this.RemoveDirectories.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RemoveDirectories.Location = new System.Drawing.Point(19, 119);
            this.RemoveDirectories.Name = "RemoveDirectories";
            this.RemoveDirectories.Size = new System.Drawing.Size(168, 17);
            this.RemoveDirectories.TabIndex = 4;
            this.RemoveDirectories.Text = "Remove untracked directories";
            this.RemoveDirectories.UseVisualStyleBackColor = true;
            // 
            // PreviewOutput
            // 
            this.PreviewOutput.Location = new System.Drawing.Point(12, 142);
            this.PreviewOutput.Multiline = true;
            this.PreviewOutput.Name = "PreviewOutput";
            this.PreviewOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PreviewOutput.Size = new System.Drawing.Size(372, 207);
            this.PreviewOutput.TabIndex = 5;
            // 
            // FormCleanupRepository
            // 
            this.AcceptButton = this.Preview;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(396, 361);
            this.Controls.Add(this.PreviewOutput);
            this.Controls.Add(this.RemoveDirectories);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Cleanup);
            this.Controls.Add(this.Preview);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCleanupRepository";
            this.Text = "Cleanup repository";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Preview;
        private System.Windows.Forms.Button Cleanup;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RemoveIngnored;
        private System.Windows.Forms.RadioButton RemoveNonIgnored;
        private System.Windows.Forms.RadioButton RemoveAll;
        private System.Windows.Forms.CheckBox RemoveDirectories;
        private System.Windows.Forms.TextBox PreviewOutput;
    }
}
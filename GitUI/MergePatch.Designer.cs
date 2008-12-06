namespace GitUI
{
    partial class MergePatch
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
            this.BrowsePatch = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.PatchFile = new System.Windows.Forms.TextBox();
            this.Apply = new System.Windows.Forms.Button();
            this.Output = new System.Windows.Forms.RichTextBox();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Resolved = new System.Windows.Forms.Button();
            this.AddFiles = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BrowsePatch
            // 
            this.BrowsePatch.Location = new System.Drawing.Point(370, 10);
            this.BrowsePatch.Name = "BrowsePatch";
            this.BrowsePatch.Size = new System.Drawing.Size(75, 23);
            this.BrowsePatch.TabIndex = 0;
            this.BrowsePatch.Text = "Browse";
            this.BrowsePatch.UseVisualStyleBackColor = true;
            this.BrowsePatch.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Patch file";
            // 
            // PatchFile
            // 
            this.PatchFile.Location = new System.Drawing.Point(83, 13);
            this.PatchFile.Name = "PatchFile";
            this.PatchFile.Size = new System.Drawing.Size(281, 20);
            this.PatchFile.TabIndex = 2;
            this.PatchFile.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Apply
            // 
            this.Apply.Location = new System.Drawing.Point(464, 9);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(104, 23);
            this.Apply.TabIndex = 3;
            this.Apply.Text = "Apply patch";
            this.Apply.UseVisualStyleBackColor = true;
            this.Apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // Output
            // 
            this.Output.Location = new System.Drawing.Point(15, 39);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(430, 185);
            this.Output.TabIndex = 4;
            this.Output.Text = "";
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(464, 39);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(104, 23);
            this.Mergetool.TabIndex = 5;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(463, 172);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(103, 23);
            this.Skip.TabIndex = 6;
            this.Skip.Text = "Skip patch";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(463, 201);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(104, 23);
            this.Abort.TabIndex = 7;
            this.Abort.Text = "Abort patch";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // Resolved
            // 
            this.Resolved.Location = new System.Drawing.Point(464, 143);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(103, 23);
            this.Resolved.TabIndex = 8;
            this.Resolved.Text = "Conflicts resolved";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(464, 94);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(102, 23);
            this.AddFiles.TabIndex = 9;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFiles_Click);
            // 
            // MergePatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 262);
            this.Controls.Add(this.AddFiles);
            this.Controls.Add(this.Resolved);
            this.Controls.Add(this.Abort);
            this.Controls.Add(this.Skip);
            this.Controls.Add(this.Mergetool);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.Apply);
            this.Controls.Add(this.PatchFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BrowsePatch);
            this.Name = "MergePatch";
            this.Text = "Apply patch";
            this.Load += new System.EventHandler(this.MergePatch_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MergePatch_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BrowsePatch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PatchFile;
        private System.Windows.Forms.Button Apply;
        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Resolved;
        private System.Windows.Forms.Button AddFiles;
    }
}
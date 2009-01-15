namespace GitUI
{
    partial class FormRebase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRebase));
            this.label1 = new System.Windows.Forms.Label();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AddFiles = new System.Windows.Forms.Button();
            this.Resolved = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.Output = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(236, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rebase current branch on top of another branch";
            // 
            // Currentbranch
            // 
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(14, 36);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(41, 13);
            this.Currentbranch.TabIndex = 2;
            this.Currentbranch.Text = "Current";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(431, 55);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(102, 23);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "Rebase";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Branches
            // 
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(98, 57);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(327, 21);
            this.Branches.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Rebase on";
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(431, 144);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(102, 23);
            this.AddFiles.TabIndex = 14;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFiles_Click);
            // 
            // Resolved
            // 
            this.Resolved.Location = new System.Drawing.Point(431, 193);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(103, 23);
            this.Resolved.TabIndex = 13;
            this.Resolved.Text = "Continue rebase";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.Resolved_Click);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(430, 251);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(104, 23);
            this.Abort.TabIndex = 12;
            this.Abort.Text = "Abort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(430, 222);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(103, 23);
            this.Skip.TabIndex = 11;
            this.Skip.Text = "Skip this commit";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.Skip_Click);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(431, 89);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(104, 23);
            this.Mergetool.TabIndex = 10;
            this.Mergetool.Text = "Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.Mergetool_Click);
            // 
            // Output
            // 
            this.Output.Location = new System.Drawing.Point(17, 90);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(408, 185);
            this.Output.TabIndex = 15;
            this.Output.Text = "";
            // 
            // FormRebase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 287);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.AddFiles);
            this.Controls.Add(this.Resolved);
            this.Controls.Add(this.Abort);
            this.Controls.Add(this.Skip);
            this.Controls.Add(this.Mergetool);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Branches);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Currentbranch);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormRebase";
            this.Text = "Rebase";
            this.Load += new System.EventHandler(this.FormRebase_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button Resolved;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Mergetool;
        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.ComboBox Branches;
    }
}
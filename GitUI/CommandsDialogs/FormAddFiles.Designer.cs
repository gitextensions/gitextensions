namespace GitUI.CommandsDialogs
{
    partial class FormAddFiles
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
            this.force = new System.Windows.Forms.CheckBox();
            this.ShowFiles = new System.Windows.Forms.Button();
            this.AddFiles = new System.Windows.Forms.Button();
            this.Filter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // force
            // 
            this.force.AutoSize = true;
            this.force.Location = new System.Drawing.Point(89, 40);
            this.force.Margin = new System.Windows.Forms.Padding(4);
            this.force.Name = "force";
            this.force.Size = new System.Drawing.Size(73, 27);
            this.force.TabIndex = 4;
            this.force.Text = "Force";
            this.force.UseVisualStyleBackColor = true;
            // 
            // ShowFiles
            // 
            this.ShowFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowFiles.Location = new System.Drawing.Point(340, 4);
            this.ShowFiles.Margin = new System.Windows.Forms.Padding(4);
            this.ShowFiles.Name = "ShowFiles";
            this.ShowFiles.Size = new System.Drawing.Size(94, 31);
            this.ShowFiles.TabIndex = 3;
            this.ShowFiles.Text = "Show files";
            this.ShowFiles.UseVisualStyleBackColor = true;
            this.ShowFiles.Click += new System.EventHandler(this.ShowFilesClick);
            // 
            // AddFiles
            // 
            this.AddFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddFiles.Location = new System.Drawing.Point(442, 4);
            this.AddFiles.Margin = new System.Windows.Forms.Padding(4);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(94, 31);
            this.AddFiles.TabIndex = 2;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFilesClick);
            // 
            // Filter
            // 
            this.Filter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Filter.Location = new System.Drawing.Point(89, 6);
            this.Filter.Margin = new System.Windows.Forms.Padding(4);
            this.Filter.Name = "Filter";
            this.Filter.Size = new System.Drawing.Size(217, 30);
            this.Filter.TabIndex = 1;
            this.Filter.Text = ".";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter";
            // 
            // FormAddFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(552, 73);
            this.Controls.Add(this.force);
            this.Controls.Add(this.ShowFiles);
            this.Controls.Add(this.AddFiles);
            this.Controls.Add(this.Filter);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(10000, 120);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(570, 120);
            this.Name = "FormAddFiles";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add files";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Filter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button ShowFiles;
        private System.Windows.Forms.CheckBox force;
    }
}
namespace GitUI
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
            this.AddFiles = new System.Windows.Forms.Button();
            this.Filter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ShowFiles = new System.Windows.Forms.Button();
            this.force = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(355, 3);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(75, 25);
            this.AddFiles.TabIndex = 2;
            this.AddFiles.Text = "Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFilesClick);
            // 
            // Filter
            // 
            this.Filter.Location = new System.Drawing.Point(71, 5);
            this.Filter.Name = "Filter";
            this.Filter.Size = new System.Drawing.Size(176, 21);
            this.Filter.TabIndex = 1;
            this.Filter.Text = ".";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter";
            // 
            // ShowFiles
            // 
            this.ShowFiles.Location = new System.Drawing.Point(274, 3);
            this.ShowFiles.Name = "ShowFiles";
            this.ShowFiles.Size = new System.Drawing.Size(75, 25);
            this.ShowFiles.TabIndex = 3;
            this.ShowFiles.Text = "Show files";
            this.ShowFiles.UseVisualStyleBackColor = true;
            this.ShowFiles.Click += new System.EventHandler(this.ShowFilesClick);
            // 
            // force
            // 
            this.force.AutoSize = true;
            this.force.Location = new System.Drawing.Point(71, 32);
            this.force.Name = "force";
            this.force.Size = new System.Drawing.Size(53, 17);
            this.force.TabIndex = 4;
            this.force.Text = "Force";
            this.force.UseVisualStyleBackColor = true;
            // 
            // FormAddFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 56);
            this.Controls.Add(this.force);
            this.Controls.Add(this.ShowFiles);
            this.Controls.Add(this.AddFiles);
            this.Controls.Add(this.Filter);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
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
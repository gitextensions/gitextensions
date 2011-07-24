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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Preview = new System.Windows.Forms.ListBox();
            this.AddToIngore = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FilePattern = new System.Windows.Forms.TextBox();
            this.noMatchPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.noMatchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.noMatchPanel);
            this.groupBox1.Controls.Add(this.Preview);
            this.groupBox1.Location = new System.Drawing.Point(15, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(12);
            this.groupBox1.Size = new System.Drawing.Size(552, 200);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Preview";
            // 
            // Preview
            // 
            this.Preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Preview.FormattingEnabled = true;
            this.Preview.ItemHeight = 15;
            this.Preview.Location = new System.Drawing.Point(12, 28);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(528, 160);
            this.Preview.TabIndex = 1;
            // 
            // AddToIngore
            // 
            this.AddToIngore.Location = new System.Drawing.Point(451, 16);
            this.AddToIngore.Name = "AddToIngore";
            this.AddToIngore.Size = new System.Drawing.Size(91, 25);
            this.AddToIngore.TabIndex = 7;
            this.AddToIngore.Text = "Ignore";
            this.AddToIngore.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Enter a file pattern to ignore:";
            // 
            // FilePattern
            // 
            this.FilePattern.Location = new System.Drawing.Point(175, 16);
            this.FilePattern.Name = "FilePattern";
            this.FilePattern.Size = new System.Drawing.Size(270, 23);
            this.FilePattern.TabIndex = 6;
            this.FilePattern.TextChanged += new System.EventHandler(this.FilePattern_TextChanged);
            // 
            // noMatchPanel
            // 
            this.noMatchPanel.BackColor = System.Drawing.Color.White;
            this.noMatchPanel.Controls.Add(this.label2);
            this.noMatchPanel.Controls.Add(this.pictureBox1);
            this.noMatchPanel.Location = new System.Drawing.Point(160, 84);
            this.noMatchPanel.Name = "noMatchPanel";
            this.noMatchPanel.Size = new System.Drawing.Size(233, 26);
            this.noMatchPanel.TabIndex = 2;
            this.noMatchPanel.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Resources.Conflict;
            this.pictureBox1.Location = new System.Drawing.Point(10, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(25, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(195, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "No existing files match that pattern.";
            // 
            // FormAddToGitIgnore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 267);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.AddToIngore);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FilePattern);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAddToGitIgnore";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add files(s) to .gitIgnore";
            this.groupBox1.ResumeLayout(false);
            this.noMatchPanel.ResumeLayout(false);
            this.noMatchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox Preview;
        private System.Windows.Forms.Button AddToIngore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FilePattern;
        private System.Windows.Forms.Panel noMatchPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;

    }
}
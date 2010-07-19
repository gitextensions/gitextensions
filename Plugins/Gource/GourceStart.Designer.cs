namespace Gource
{
    partial class GourceStart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GourceStart));
            this.button1 = new System.Windows.Forms.Button();
            this.ArgumentsLabel = new System.Windows.Forms.Label();
            this.Arguments = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.GourcePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.WorkingDir = new System.Windows.Forms.TextBox();
            this.GourceBrowse = new System.Windows.Forms.Button();
            this.WorkingDirBrowse = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(697, 364);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // ArgumentsLabel
            // 
            this.ArgumentsLabel.AutoSize = true;
            this.ArgumentsLabel.Location = new System.Drawing.Point(12, 59);
            this.ArgumentsLabel.Name = "ArgumentsLabel";
            this.ArgumentsLabel.Size = new System.Drawing.Size(57, 13);
            this.ArgumentsLabel.TabIndex = 1;
            this.ArgumentsLabel.Text = "Arguments";
            // 
            // Arguments
            // 
            this.Arguments.Location = new System.Drawing.Point(113, 56);
            this.Arguments.Name = "Arguments";
            this.Arguments.Size = new System.Drawing.Size(659, 20);
            this.Arguments.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(12, 97);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(760, 251);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Path to gource";
            // 
            // GourcePath
            // 
            this.GourcePath.Location = new System.Drawing.Point(113, 6);
            this.GourcePath.Name = "GourcePath";
            this.GourcePath.Size = new System.Drawing.Size(546, 20);
            this.GourcePath.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Repository";
            // 
            // WorkingDir
            // 
            this.WorkingDir.Location = new System.Drawing.Point(113, 31);
            this.WorkingDir.Name = "WorkingDir";
            this.WorkingDir.Size = new System.Drawing.Size(546, 20);
            this.WorkingDir.TabIndex = 7;
            // 
            // GourceBrowse
            // 
            this.GourceBrowse.Location = new System.Drawing.Point(665, 4);
            this.GourceBrowse.Name = "GourceBrowse";
            this.GourceBrowse.Size = new System.Drawing.Size(107, 23);
            this.GourceBrowse.TabIndex = 8;
            this.GourceBrowse.Text = "Browse";
            this.GourceBrowse.UseVisualStyleBackColor = true;
            this.GourceBrowse.Click += new System.EventHandler(this.GourceBrowseClick);
            // 
            // WorkingDirBrowse
            // 
            this.WorkingDirBrowse.Location = new System.Drawing.Point(665, 29);
            this.WorkingDirBrowse.Name = "WorkingDirBrowse";
            this.WorkingDirBrowse.Size = new System.Drawing.Size(107, 23);
            this.WorkingDirBrowse.TabIndex = 9;
            this.WorkingDirBrowse.Text = "Browse";
            this.WorkingDirBrowse.UseVisualStyleBackColor = true;
            this.WorkingDirBrowse.Click += new System.EventHandler(this.WorkingDirBrowseClick);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(14, 369);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(75, 13);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "gource project";
            this.linkLabel1.Click += new System.EventHandler(LinkLabel1Click);
            // 
            // GourceStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 399);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.WorkingDirBrowse);
            this.Controls.Add(this.GourceBrowse);
            this.Controls.Add(this.WorkingDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.GourcePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Arguments);
            this.Controls.Add(this.ArgumentsLabel);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "GourceStart";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gource";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label ArgumentsLabel;
        private System.Windows.Forms.TextBox Arguments;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox GourcePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox WorkingDir;
        private System.Windows.Forms.Button GourceBrowse;
        private System.Windows.Forms.Button WorkingDirBrowse;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}
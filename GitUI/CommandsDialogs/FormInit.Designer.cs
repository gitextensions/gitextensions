namespace GitUI.CommandsDialogs
{
    partial class FormInit
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
            this.Directory = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Central = new System.Windows.Forms.RadioButton();
            this.Personal = new System.Windows.Forms.RadioButton();
            this.Init = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Directory";
            // 
            // Directory
            // 
            this.Directory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Directory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.Directory.FormattingEnabled = true;
            this.Directory.Location = new System.Drawing.Point(122, 8);
            this.Directory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Directory.Name = "Directory";
            this.Directory.Size = new System.Drawing.Size(422, 31);
            this.Directory.TabIndex = 1;
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(552, 5);
            this.Browse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(126, 31);
            this.Browse.TabIndex = 2;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.BrowseClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Central);
            this.groupBox1.Controls.Add(this.Personal);
            this.groupBox1.Location = new System.Drawing.Point(19, 41);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(526, 85);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repository type";
            // 
            // Central
            // 
            this.Central.AutoSize = true;
            this.Central.Location = new System.Drawing.Point(8, 52);
            this.Central.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Central.Name = "Central";
            this.Central.Size = new System.Drawing.Size(453, 27);
            this.Central.TabIndex = 1;
            this.Central.Text = "Central repository, no working directory  (--bare --shared=all)";
            this.Central.UseVisualStyleBackColor = true;
            // 
            // Personal
            // 
            this.Personal.AutoSize = true;
            this.Personal.Checked = true;
            this.Personal.Location = new System.Drawing.Point(8, 24);
            this.Personal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Personal.Name = "Personal";
            this.Personal.Size = new System.Drawing.Size(177, 27);
            this.Personal.TabIndex = 0;
            this.Personal.TabStop = true;
            this.Personal.Text = "Personal repository";
            this.Personal.UseVisualStyleBackColor = true;
            // 
            // Init
            // 
            this.Init.Location = new System.Drawing.Point(552, 98);
            this.Init.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Init.Name = "Init";
            this.Init.Size = new System.Drawing.Size(126, 31);
            this.Init.TabIndex = 4;
            this.Init.Text = "Create";
            this.Init.UseVisualStyleBackColor = true;
            this.Init.Click += new System.EventHandler(this.InitClick);
            // 
            // FormInit
            // 
            this.AcceptButton = this.Init;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(685, 141);
            this.Controls.Add(this.Init);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.Directory);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormInit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create new repository";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox Directory;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Central;
        private System.Windows.Forms.RadioButton Personal;
        private System.Windows.Forms.Button Init;
    }
}
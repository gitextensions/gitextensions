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
            if (disposing && (components is not null))
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
            this._NO_TRANSLATE_Directory = new System.Windows.Forms.ComboBox();
            this.Browse = new GitUI.UserControls.FolderBrowserButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Central = new System.Windows.Forms.RadioButton();
            this.Personal = new System.Windows.Forms.RadioButton();
            this.Init = new System.Windows.Forms.Button();
            this.MainPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.groupBox1);
            this.MainPanel.Controls.Add(this.Init);
            this.MainPanel.Controls.Add(this.Browse);
            this.MainPanel.Controls.Add(this._NO_TRANSLATE_Directory);
            this.MainPanel.Controls.Add(this.label1);
            this.MainPanel.Size = new System.Drawing.Size(542, 141);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Directory";
            // 
            // _NO_TRANSLATE_Directory
            // 
            this._NO_TRANSLATE_Directory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_Directory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_Directory.FormattingEnabled = true;
            this._NO_TRANSLATE_Directory.Location = new System.Drawing.Point(95, 14);
            this._NO_TRANSLATE_Directory.Name = "_NO_TRANSLATE_Directory";
            this._NO_TRANSLATE_Directory.Size = new System.Drawing.Size(323, 21);
            this._NO_TRANSLATE_Directory.TabIndex = 1;
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(424, 12);
            this.Browse.Name = "Browse";
            this.Browse.PathShowingControl = this._NO_TRANSLATE_Directory;
            this.Browse.Size = new System.Drawing.Size(106, 25);
            this.Browse.TabIndex = 2;
            this.Browse.Click += new System.EventHandler(this.BrowseClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Central);
            this.groupBox1.Controls.Add(this.Personal);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(518, 78);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repository type";
            // 
            // Central
            // 
            this.Central.AutoSize = true;
            this.Central.Location = new System.Drawing.Point(19, 48);
            this.Central.Name = "Central";
            this.Central.Size = new System.Drawing.Size(303, 17);
            this.Central.TabIndex = 1;
            this.Central.Text = "Central repository, no working directory  (--bare --shared=all)";
            this.Central.UseVisualStyleBackColor = true;
            // 
            // Personal
            // 
            this.Personal.AutoSize = true;
            this.Personal.Checked = true;
            this.Personal.Location = new System.Drawing.Point(19, 25);
            this.Personal.Name = "Personal";
            this.Personal.Size = new System.Drawing.Size(114, 17);
            this.Personal.TabIndex = 0;
            this.Personal.TabStop = true;
            this.Personal.Text = "Personal repository";
            this.Personal.UseVisualStyleBackColor = true;
            // 
            // Init
            // 
            this.Init.AutoSize = true;
            this.Init.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Init.Location = new System.Drawing.Point(31, 7);
            this.Init.MinimumSize = new System.Drawing.Size(75, 23);
            this.Init.Name = "Init";
            this.Init.Size = new System.Drawing.Size(75, 23);
            this.Init.TabIndex = 4;
            this.Init.Text = "Create";
            this.Init.UseVisualStyleBackColor = true;
            this.Init.Click += new System.EventHandler(this.InitClick);
            // 
            // FormInit
            // 
            this.AcceptButton = this.Init;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(542, 173);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.ManualSectionAnchorName = "create-new-repository";
            this.ManualSectionSubfolder = "getting_started";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormInit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create new repository";
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Directory;
        private UserControls.FolderBrowserButton Browse;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Central;
        private System.Windows.Forms.RadioButton Personal;
        private System.Windows.Forms.Button Init;
    }
}

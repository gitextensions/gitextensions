namespace GitUI
{
    partial class FormClone
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.brachLabel = new System.Windows.Forms.Label();
            this.Info = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_NewDirectory = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CentralRepository = new System.Windows.Forms.RadioButton();
            this.PersonalRepository = new System.Windows.Forms.RadioButton();
            this._NO_TRANSLATE_From = new System.Windows.Forms.ComboBox();
            this._NO_TRANSLATE_To = new System.Windows.Forms.ComboBox();
            this.ToBrowse = new System.Windows.Forms.Button();
            this.FromBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.Central = new System.Windows.Forms.RadioButton();
            this.Personal = new System.Windows.Forms.RadioButton();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Branches);
            this.splitContainer1.Panel1.Controls.Add(this.brachLabel);
            this.splitContainer1.Panel1.Controls.Add(this.Info);
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_NewDirectory);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_From);
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_To);
            this.splitContainer1.Panel1.Controls.Add(this.ToBrowse);
            this.splitContainer1.Panel1.Controls.Add(this.FromBrowse);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LoadSSHKey);
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Size = new System.Drawing.Size(525, 300);
            this.splitContainer1.SplitterDistance = 260;
            this.splitContainer1.TabIndex = 0;
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(158, 99);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(255, 21);
            this.Branches.TabIndex = 12;
            this.Branches.DropDown += new System.EventHandler(this.Branches_DropDown);
            // 
            // brachLabel
            // 
            this.brachLabel.AutoSize = true;
            this.brachLabel.Location = new System.Drawing.Point(12, 102);
            this.brachLabel.Name = "brachLabel";
            this.brachLabel.Size = new System.Drawing.Size(40, 13);
            this.brachLabel.TabIndex = 13;
            this.brachLabel.Text = "Branch";
            // 
            // Info
            // 
            this.Info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Info.BackColor = System.Drawing.SystemColors.Info;
            this.Info.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Info.Location = new System.Drawing.Point(12, 128);
            this.Info.Name = "Info";
            this.Info.Size = new System.Drawing.Size(488, 42);
            this.Info.TabIndex = 11;
            this.Info.Text = "The repository will be cloned to a new directory located here:\r\n      [destinatio" +
                "n]\\[directory]\r\n";
            this.Info.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_NewDirectory
            // 
            this._NO_TRANSLATE_NewDirectory.Location = new System.Drawing.Point(158, 72);
            this._NO_TRANSLATE_NewDirectory.Name = "_NO_TRANSLATE_NewDirectory";
            this._NO_TRANSLATE_NewDirectory.Size = new System.Drawing.Size(152, 21);
            this._NO_TRANSLATE_NewDirectory.TabIndex = 3;
            this._NO_TRANSLATE_NewDirectory.TextChanged += new System.EventHandler(this.NewDirectoryTextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Subdirectory to create";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.CentralRepository);
            this.groupBox1.Controls.Add(this.PersonalRepository);
            this.groupBox1.Location = new System.Drawing.Point(15, 173);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(485, 68);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repository type";
            // 
            // CentralRepository
            // 
            this.CentralRepository.AutoSize = true;
            this.CentralRepository.Location = new System.Drawing.Point(6, 42);
            this.CentralRepository.Name = "CentralRepository";
            this.CentralRepository.Size = new System.Drawing.Size(213, 17);
            this.CentralRepository.TabIndex = 1;
            this.CentralRepository.Text = "Public repository, no working dir  (--bare)";
            this.CentralRepository.UseVisualStyleBackColor = true;
            // 
            // PersonalRepository
            // 
            this.PersonalRepository.AutoSize = true;
            this.PersonalRepository.Checked = true;
            this.PersonalRepository.Location = new System.Drawing.Point(6, 19);
            this.PersonalRepository.Name = "PersonalRepository";
            this.PersonalRepository.Size = new System.Drawing.Size(114, 17);
            this.PersonalRepository.TabIndex = 0;
            this.PersonalRepository.TabStop = true;
            this.PersonalRepository.Text = "Personal repository";
            this.PersonalRepository.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_From
            // 
            this._NO_TRANSLATE_From.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_From.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_From.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_From.FormattingEnabled = true;
            this._NO_TRANSLATE_From.Location = new System.Drawing.Point(158, 17);
            this._NO_TRANSLATE_From.Name = "_NO_TRANSLATE_From";
            this._NO_TRANSLATE_From.Size = new System.Drawing.Size(255, 21);
            this._NO_TRANSLATE_From.TabIndex = 1;
            this._NO_TRANSLATE_From.DropDown += new System.EventHandler(this.FromDropDown);
            this._NO_TRANSLATE_From.SelectedIndexChanged += new System.EventHandler(this.FromSelectedIndexChanged);
            this._NO_TRANSLATE_From.TextUpdate += new System.EventHandler(this.FromTextUpdate);
            // 
            // _NO_TRANSLATE_To
            // 
            this._NO_TRANSLATE_To.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_To.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_To.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_To.FormattingEnabled = true;
            this._NO_TRANSLATE_To.Location = new System.Drawing.Point(158, 45);
            this._NO_TRANSLATE_To.Name = "_NO_TRANSLATE_To";
            this._NO_TRANSLATE_To.Size = new System.Drawing.Size(255, 21);
            this._NO_TRANSLATE_To.TabIndex = 2;
            this._NO_TRANSLATE_To.DropDown += new System.EventHandler(this.ToDropDown);
            this._NO_TRANSLATE_To.SelectedIndexChanged += new System.EventHandler(this.ToSelectedIndexChanged);
            this._NO_TRANSLATE_To.TextUpdate += new System.EventHandler(this.ToTextUpdate);
            // 
            // ToBrowse
            // 
            this.ToBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToBrowse.Location = new System.Drawing.Point(425, 44);
            this.ToBrowse.Name = "ToBrowse";
            this.ToBrowse.Size = new System.Drawing.Size(75, 25);
            this.ToBrowse.TabIndex = 6;
            this.ToBrowse.Text = "Browse";
            this.ToBrowse.UseVisualStyleBackColor = true;
            this.ToBrowse.Click += new System.EventHandler(this.ToBrowseClick);
            // 
            // FromBrowse
            // 
            this.FromBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FromBrowse.Location = new System.Drawing.Point(425, 16);
            this.FromBrowse.Name = "FromBrowse";
            this.FromBrowse.Size = new System.Drawing.Size(75, 25);
            this.FromBrowse.TabIndex = 5;
            this.FromBrowse.Text = "Browse";
            this.FromBrowse.UseVisualStyleBackColor = true;
            this.FromBrowse.Click += new System.EventHandler(this.FromBrowseClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Destination";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Repository to clone";
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(0, 2);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(155, 25);
            this.LoadSSHKey.TabIndex = 25;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(425, 2);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 25);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Clone";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // Central
            // 
            this.Central.AutoSize = true;
            this.Central.Location = new System.Drawing.Point(6, 42);
            this.Central.Name = "Central";
            this.Central.Size = new System.Drawing.Size(274, 17);
            this.Central.TabIndex = 1;
            this.Central.Text = "Central repository, no working dir  (--bare --shared=all)";
            this.Central.UseVisualStyleBackColor = true;
            // 
            // Personal
            // 
            this.Personal.AutoSize = true;
            this.Personal.Checked = true;
            this.Personal.Location = new System.Drawing.Point(6, 19);
            this.Personal.Name = "Personal";
            this.Personal.Size = new System.Drawing.Size(114, 17);
            this.Personal.TabIndex = 0;
            this.Personal.TabStop = true;
            this.Personal.Text = "Personal repository";
            this.Personal.UseVisualStyleBackColor = true;
            // 
            // FormClone
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 300);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(933, 327);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(433, 327);
            this.Name = "FormClone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clone";
            this.Load += new System.EventHandler(this.FormCloneLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button ToBrowse;
        private System.Windows.Forms.Button FromBrowse;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_From;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_To;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton CentralRepository;
        private System.Windows.Forms.RadioButton PersonalRepository;
        private System.Windows.Forms.RadioButton Central;
        private System.Windows.Forms.RadioButton Personal;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_NewDirectory;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Info;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Label brachLabel;
    }
}

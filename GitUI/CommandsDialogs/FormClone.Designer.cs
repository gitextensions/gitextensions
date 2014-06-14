﻿namespace GitUI.CommandsDialogs
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
            _branchListLoader.Cancel();

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
            this.Central = new System.Windows.Forms.RadioButton();
            this.Personal = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_From = new System.Windows.Forms.ComboBox();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.FromBrowse = new System.Windows.Forms.Button();
            this.ToBrowse = new System.Windows.Forms.Button();
            this.brachLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_NewDirectory = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_To = new System.Windows.Forms.ComboBox();
            this.cbIntializeAllSubmodules = new System.Windows.Forms.CheckBox();
            this.Info = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CentralRepository = new System.Windows.Forms.RadioButton();
            this.PersonalRepository = new System.Windows.Forms.RadioButton();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // Central
            // 
            this.Central.AutoSize = true;
            this.Central.Location = new System.Drawing.Point(6, 42);
            this.Central.Name = "Central";
            this.Central.Size = new System.Drawing.Size(274, 17);
            this.Central.TabIndex = 1;
            this.Central.Text = "Central repository, no working directory  (--bare --shared=all)";
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
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(488, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(115, 25);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Clone";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_From, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Branches, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.FromBrowse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.ToBrowse, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.brachLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_NewDirectory, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_To, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(610, 122);
            this.tableLayoutPanel1.TabIndex = 26;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 30);
            this.label1.TabIndex = 1;
            this.label1.Text = "&Repository to clone:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_From
            // 
            this._NO_TRANSLATE_From.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_From.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_From.FormattingEnabled = true;
            this._NO_TRANSLATE_From.Location = new System.Drawing.Point(135, 3);
            this._NO_TRANSLATE_From.Name = "_NO_TRANSLATE_From";
            this._NO_TRANSLATE_From.Size = new System.Drawing.Size(372, 23);
            this._NO_TRANSLATE_From.TabIndex = 2;
            this._NO_TRANSLATE_From.SelectedIndexChanged += new System.EventHandler(this.FromSelectedIndexChanged);
            this._NO_TRANSLATE_From.TextUpdate += new System.EventHandler(this.FromTextUpdate);
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(135, 93);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(372, 23);
            this.Branches.TabIndex = 9;
            this.Branches.DropDown += new System.EventHandler(this.Branches_DropDown);
            // 
            // FromBrowse
            // 
            this.FromBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FromBrowse.Location = new System.Drawing.Point(513, 3);
            this.FromBrowse.Name = "FromBrowse";
            this.FromBrowse.Size = new System.Drawing.Size(94, 24);
            this.FromBrowse.TabIndex = 2;
            this.FromBrowse.Text = "&Browse";
            this.FromBrowse.UseVisualStyleBackColor = true;
            this.FromBrowse.Click += new System.EventHandler(this.FromBrowseClick);
            // 
            // ToBrowse
            // 
            this.ToBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToBrowse.Location = new System.Drawing.Point(513, 33);
            this.ToBrowse.Name = "ToBrowse";
            this.ToBrowse.Size = new System.Drawing.Size(94, 24);
            this.ToBrowse.TabIndex = 5;
            this.ToBrowse.Text = "B&rowse";
            this.ToBrowse.UseVisualStyleBackColor = true;
            this.ToBrowse.Click += new System.EventHandler(this.ToBrowseClick);
            // 
            // brachLabel
            // 
            this.brachLabel.AutoSize = true;
            this.brachLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.brachLabel.Location = new System.Drawing.Point(3, 90);
            this.brachLabel.Name = "brachLabel";
            this.brachLabel.Size = new System.Drawing.Size(47, 32);
            this.brachLabel.TabIndex = 8;
            this.brachLabel.Text = "&Branch:";
            this.brachLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_NewDirectory
            // 
            this._NO_TRANSLATE_NewDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_NewDirectory.Location = new System.Drawing.Point(135, 63);
            this._NO_TRANSLATE_NewDirectory.Name = "_NO_TRANSLATE_NewDirectory";
            this._NO_TRANSLATE_NewDirectory.Size = new System.Drawing.Size(372, 23);
            this._NO_TRANSLATE_NewDirectory.TabIndex = 7;
            this._NO_TRANSLATE_NewDirectory.TextChanged += new System.EventHandler(this.NewDirectoryTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(3, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 30);
            this.label2.TabIndex = 3;
            this.label2.Text = "&Destination:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(3, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 30);
            this.label3.TabIndex = 6;
            this.label3.Text = "&Subdirectory to create:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_To
            // 
            this._NO_TRANSLATE_To.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_To.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_To.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_To.FormattingEnabled = true;
            this._NO_TRANSLATE_To.Location = new System.Drawing.Point(135, 33);
            this._NO_TRANSLATE_To.Name = "_NO_TRANSLATE_To";
            this._NO_TRANSLATE_To.Size = new System.Drawing.Size(372, 23);
            this._NO_TRANSLATE_To.TabIndex = 4;
            this._NO_TRANSLATE_To.DropDown += new System.EventHandler(this.ToDropDown);
            this._NO_TRANSLATE_To.SelectedIndexChanged += new System.EventHandler(this.ToSelectedIndexChanged);
            this._NO_TRANSLATE_To.TextUpdate += new System.EventHandler(this.ToTextUpdate);
            // 
            // cbIntializeAllSubmodules
            // 
            this.cbIntializeAllSubmodules.AutoSize = true;
            this.cbIntializeAllSubmodules.Checked = true;
            this.cbIntializeAllSubmodules.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIntializeAllSubmodules.Location = new System.Drawing.Point(15, 126);
            this.cbIntializeAllSubmodules.Name = "cbIntializeAllSubmodules";
            this.cbIntializeAllSubmodules.Size = new System.Drawing.Size(152, 19);
            this.cbIntializeAllSubmodules.TabIndex = 25;
            this.cbIntializeAllSubmodules.Text = "Initialize all submodules";
            this.cbIntializeAllSubmodules.UseVisualStyleBackColor = true;
            // 
            // Info
            // 
            this.Info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Info.BackColor = System.Drawing.SystemColors.Info;
            this.Info.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Info.Location = new System.Drawing.Point(6, 10);
            this.Info.Name = "Info";
            this.Info.Size = new System.Drawing.Size(597, 42);
            this.Info.TabIndex = 10;
            this.Info.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.CentralRepository);
            this.groupBox1.Controls.Add(this.PersonalRepository);
            this.groupBox1.Location = new System.Drawing.Point(9, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(594, 68);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repository type";
            // 
            // CentralRepository
            // 
            this.CentralRepository.AutoSize = true;
            this.CentralRepository.Location = new System.Drawing.Point(6, 42);
            this.CentralRepository.Name = "CentralRepository";
            this.CentralRepository.Size = new System.Drawing.Size(244, 19);
            this.CentralRepository.TabIndex = 1;
            this.CentralRepository.Text = "P&ublic repository, no working directory  (--bare)";
            this.CentralRepository.UseVisualStyleBackColor = true;
            // 
            // PersonalRepository
            // 
            this.PersonalRepository.AutoSize = true;
            this.PersonalRepository.Checked = true;
            this.PersonalRepository.Location = new System.Drawing.Point(6, 19);
            this.PersonalRepository.Name = "PersonalRepository";
            this.PersonalRepository.Size = new System.Drawing.Size(126, 19);
            this.PersonalRepository.TabIndex = 0;
            this.PersonalRepository.TabStop = true;
            this.PersonalRepository.Text = "&Personal repository";
            this.PersonalRepository.UseVisualStyleBackColor = true;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(3, 3);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(180, 25);
            this.LoadSSHKey.TabIndex = 0;
            this.LoadSSHKey.Text = "&Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(616, 322);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbIntializeAllSubmodules);
            this.panel1.Controls.Add(this.Info);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 131);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(610, 147);
            this.panel1.TabIndex = 27;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.Ok, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.LoadSSHKey, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 286);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(606, 31);
            this.tableLayoutPanel3.TabIndex = 28;
            // 
            // FormClone
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(616, 322);
            this.Controls.Add(this.tableLayoutPanel2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(933, 360);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(430, 360);
            this.Name = "FormClone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clone";
            this.Load += new System.EventHandler(this.FormCloneLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button ToBrowse;
        private System.Windows.Forms.Button FromBrowse;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_To;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton CentralRepository;
        private System.Windows.Forms.RadioButton PersonalRepository;
        private System.Windows.Forms.RadioButton Central;
        private System.Windows.Forms.RadioButton Personal;
        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_NewDirectory;
        private System.Windows.Forms.Label Info;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.CheckBox cbIntializeAllSubmodules;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_From;
        private System.Windows.Forms.Label brachLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}

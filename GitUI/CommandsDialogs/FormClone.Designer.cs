namespace GitUI.CommandsDialogs
{
    partial class FormClone
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormClone));
            this.Central = new System.Windows.Forms.RadioButton();
            this.Personal = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.repositoryLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_From = new System.Windows.Forms.ComboBox();
            this.FromBrowse = new System.Windows.Forms.Button();
            this.destinationLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_To = new System.Windows.Forms.ComboBox();
            this.ToBrowse = new System.Windows.Forms.Button();
            this.subdirectoryLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_NewDirectory = new System.Windows.Forms.TextBox();
            this.brachLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Branches = new System.Windows.Forms.ComboBox();
            this.cbIntializeAllSubmodules = new System.Windows.Forms.CheckBox();
            this.cbDownloadFullHistory = new System.Windows.Forms.CheckBox();
            this.Info = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CentralRepository = new System.Windows.Forms.RadioButton();
            this.PersonalRepository = new System.Windows.Forms.RadioButton();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.tpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.optionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ttHints = new System.Windows.Forms.ToolTip(this.components);
            this.MainPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tpnlMain.SuspendLayout();
            this.optionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.Ok);
            this.MainPanel.Controls.Add(this.LoadSSHKey);
            this.MainPanel.Controls.Add(this.tpnlMain);
            this.MainPanel.Size = new System.Drawing.Size(647, 319);
            // 
            // Central
            // 
            this.Central.AutoSize = true;
            this.Central.Location = new System.Drawing.Point(6, 42);
            this.Central.Name = "Central";
            this.Central.Size = new System.Drawing.Size(274, 17);
            this.Central.TabIndex = 0;
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
            this.Ok.AutoSize = true;
            this.Ok.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Ok.Location = new System.Drawing.Point(10, 3);
            this.Ok.MinimumSize = new System.Drawing.Size(75, 23);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
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
            this.tableLayoutPanel1.Controls.Add(this.repositoryLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_From, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.FromBrowse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.destinationLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_To, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ToBrowse, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.subdirectoryLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_NewDirectory, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.brachLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_Branches, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(625, 125);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // repositoryLabel
            // 
            this.repositoryLabel.AutoSize = true;
            this.repositoryLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.repositoryLabel.Location = new System.Drawing.Point(3, 0);
            this.repositoryLabel.Name = "repositoryLabel";
            this.repositoryLabel.Size = new System.Drawing.Size(101, 31);
            this.repositoryLabel.TabIndex = 0;
            this.repositoryLabel.Text = "Repository to &clone:";
            this.repositoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_From
            // 
            this._NO_TRANSLATE_From.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_From.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._NO_TRANSLATE_From.FormattingEnabled = true;
            this._NO_TRANSLATE_From.Location = new System.Drawing.Point(123, 3);
            this._NO_TRANSLATE_From.Name = "_NO_TRANSLATE_From";
            this._NO_TRANSLATE_From.Size = new System.Drawing.Size(399, 21);
            this._NO_TRANSLATE_From.TabIndex = 1;
            this._NO_TRANSLATE_From.SelectedIndexChanged += new System.EventHandler(this.FromSelectedIndexChanged);
            this._NO_TRANSLATE_From.TextUpdate += new System.EventHandler(this.FromTextUpdate);
            // 
            // FromBrowse
            // 
            this.FromBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FromBrowse.Location = new System.Drawing.Point(528, 3);
            this.FromBrowse.Name = "FromBrowse";
            this.FromBrowse.Size = new System.Drawing.Size(94, 24);
            this.FromBrowse.TabIndex = 2;
            this.FromBrowse.Text = "&Browse";
            this.FromBrowse.UseVisualStyleBackColor = true;
            this.FromBrowse.Click += new System.EventHandler(this.FromBrowseClick);
            // 
            // destinationLabel
            // 
            this.destinationLabel.AutoSize = true;
            this.destinationLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.destinationLabel.Location = new System.Drawing.Point(3, 31);
            this.destinationLabel.Name = "destinationLabel";
            this.destinationLabel.Size = new System.Drawing.Size(63, 31);
            this.destinationLabel.TabIndex = 3;
            this.destinationLabel.Text = "&Destination:";
            this.destinationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_To
            // 
            this._NO_TRANSLATE_To.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_To.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._NO_TRANSLATE_To.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this._NO_TRANSLATE_To.FormattingEnabled = true;
            this._NO_TRANSLATE_To.Location = new System.Drawing.Point(123, 34);
            this._NO_TRANSLATE_To.Name = "_NO_TRANSLATE_To";
            this._NO_TRANSLATE_To.Size = new System.Drawing.Size(399, 21);
            this._NO_TRANSLATE_To.TabIndex = 4;
            this._NO_TRANSLATE_To.SelectedIndexChanged += new System.EventHandler(this.ToSelectedIndexChanged);
            this._NO_TRANSLATE_To.TextUpdate += new System.EventHandler(this.ToTextUpdate);
            // 
            // ToBrowse
            // 
            this.ToBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToBrowse.Location = new System.Drawing.Point(528, 34);
            this.ToBrowse.Name = "ToBrowse";
            this.ToBrowse.Size = new System.Drawing.Size(94, 24);
            this.ToBrowse.TabIndex = 5;
            this.ToBrowse.Text = "B&rowse";
            this.ToBrowse.UseVisualStyleBackColor = true;
            this.ToBrowse.Click += new System.EventHandler(this.ToBrowseClick);
            // 
            // subdirectoryLabel
            // 
            this.subdirectoryLabel.AutoSize = true;
            this.subdirectoryLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.subdirectoryLabel.Location = new System.Drawing.Point(3, 62);
            this.subdirectoryLabel.Name = "subdirectoryLabel";
            this.subdirectoryLabel.Size = new System.Drawing.Size(114, 31);
            this.subdirectoryLabel.TabIndex = 6;
            this.subdirectoryLabel.Text = "&Subdirectory to create:";
            this.subdirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_NewDirectory
            // 
            this._NO_TRANSLATE_NewDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_NewDirectory.Location = new System.Drawing.Point(123, 65);
            this._NO_TRANSLATE_NewDirectory.Name = "_NO_TRANSLATE_NewDirectory";
            this._NO_TRANSLATE_NewDirectory.Size = new System.Drawing.Size(399, 20);
            this._NO_TRANSLATE_NewDirectory.TabIndex = 7;
            this._NO_TRANSLATE_NewDirectory.TextChanged += new System.EventHandler(this.NewDirectoryTextChanged);
            // 
            // brachLabel
            // 
            this.brachLabel.AutoSize = true;
            this.brachLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.brachLabel.Location = new System.Drawing.Point(3, 93);
            this.brachLabel.Name = "brachLabel";
            this.brachLabel.Size = new System.Drawing.Size(44, 32);
            this.brachLabel.TabIndex = 8;
            this.brachLabel.Text = "&Branch:";
            this.brachLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_Branches
            // 
            this._NO_TRANSLATE_Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Branches.FormattingEnabled = true;
            this._NO_TRANSLATE_Branches.Location = new System.Drawing.Point(123, 96);
            this._NO_TRANSLATE_Branches.Name = "_NO_TRANSLATE_Branches";
            this._NO_TRANSLATE_Branches.Size = new System.Drawing.Size(399, 21);
            this._NO_TRANSLATE_Branches.TabIndex = 9;
            this._NO_TRANSLATE_Branches.DropDown += new System.EventHandler(this.Branches_DropDown);
            // 
            // cbIntializeAllSubmodules
            // 
            this.cbIntializeAllSubmodules.AutoSize = true;
            this.cbIntializeAllSubmodules.Checked = true;
            this.cbIntializeAllSubmodules.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIntializeAllSubmodules.Location = new System.Drawing.Point(15, 3);
            this.cbIntializeAllSubmodules.Margin = new System.Windows.Forms.Padding(15, 3, 9, 3);
            this.cbIntializeAllSubmodules.Name = "cbIntializeAllSubmodules";
            this.cbIntializeAllSubmodules.Size = new System.Drawing.Size(135, 17);
            this.cbIntializeAllSubmodules.TabIndex = 3;
            this.cbIntializeAllSubmodules.Text = "Initialize all submodules";
            this.cbIntializeAllSubmodules.UseVisualStyleBackColor = true;
            // 
            // cbDownloadFullHistory
            // 
            this.cbDownloadFullHistory.AutoSize = true;
            this.cbDownloadFullHistory.Checked = true;
            this.cbDownloadFullHistory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDownloadFullHistory.Location = new System.Drawing.Point(174, 3);
            this.cbDownloadFullHistory.Margin = new System.Windows.Forms.Padding(15, 3, 9, 3);
            this.cbDownloadFullHistory.Name = "cbDownloadFullHistory";
            this.cbDownloadFullHistory.Size = new System.Drawing.Size(123, 17);
            this.cbDownloadFullHistory.TabIndex = 4;
            this.cbDownloadFullHistory.Text = "Download full &history";
            this.ttHints.SetToolTip(this.cbDownloadFullHistory, resources.GetString("cbDownloadFullHistory.ToolTip"));
            // 
            // Info
            // 
            this.Info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Info.BackColor = System.Drawing.SystemColors.Info;
            this.Info.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Info.Location = new System.Drawing.Point(3, 138);
            this.Info.Margin = new System.Windows.Forms.Padding(3, 13, 3, 0);
            this.Info.Name = "Info";
            this.Info.Size = new System.Drawing.Size(619, 42);
            this.Info.TabIndex = 1;
            this.Info.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.CentralRepository);
            this.groupBox1.Controls.Add(this.PersonalRepository);
            this.groupBox1.Location = new System.Drawing.Point(3, 184);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(619, 78);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repository type";
            // 
            // CentralRepository
            // 
            this.CentralRepository.AutoSize = true;
            this.CentralRepository.Location = new System.Drawing.Point(20, 48);
            this.CentralRepository.Name = "CentralRepository";
            this.CentralRepository.Size = new System.Drawing.Size(242, 17);
            this.CentralRepository.TabIndex = 0;
            this.CentralRepository.Text = "P&ublic repository, no working directory  (--bare)";
            this.CentralRepository.UseVisualStyleBackColor = true;
            // 
            // PersonalRepository
            // 
            this.PersonalRepository.AutoSize = true;
            this.PersonalRepository.Checked = true;
            this.PersonalRepository.Location = new System.Drawing.Point(20, 25);
            this.PersonalRepository.Name = "PersonalRepository";
            this.PersonalRepository.Size = new System.Drawing.Size(114, 17);
            this.PersonalRepository.TabIndex = 1;
            this.PersonalRepository.TabStop = true;
            this.PersonalRepository.Text = "&Personal repository";
            this.PersonalRepository.UseVisualStyleBackColor = true;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.AutoSize = true;
            this.LoadSSHKey.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LoadSSHKey.Image = global::GitUI.Properties.Images.Putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(101, 3);
            this.LoadSSHKey.MinimumSize = new System.Drawing.Size(75, 23);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.LoadSSHKey.Size = new System.Drawing.Size(118, 23);
            this.LoadSSHKey.TabIndex = 1;
            this.LoadSSHKey.Text = "&Load SSH key";
            this.LoadSSHKey.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSshKeyClick);
            // 
            // tpnlMain
            // 
            this.tpnlMain.AutoSize = true;
            this.tpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tpnlMain.ColumnCount = 1;
            this.tpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tpnlMain.Controls.Add(this.tableLayoutPanel1);
            this.tpnlMain.Controls.Add(this.Info);
            this.tpnlMain.Controls.Add(this.groupBox1);
            this.tpnlMain.Controls.Add(this.optionsPanel);
            this.tpnlMain.Location = new System.Drawing.Point(12, 12);
            this.tpnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.tpnlMain.Name = "tpnlMain";
            this.tpnlMain.RowCount = 5;
            this.tpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlMain.Size = new System.Drawing.Size(625, 295);
            this.tpnlMain.TabIndex = 0;
            // 
            // optionsPanel
            // 
            this.optionsPanel.AutoSize = true;
            this.optionsPanel.Controls.Add(this.cbIntializeAllSubmodules);
            this.optionsPanel.Controls.Add(this.cbDownloadFullHistory);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsPanel.Location = new System.Drawing.Point(0, 272);
            this.optionsPanel.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(625, 23);
            this.optionsPanel.TabIndex = 2;
            // 
            // FormClone
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(647, 351);
            this.HelpButton = true;
            this.ManualSectionAnchorName = "clone-repository";
            this.ManualSectionSubfolder = "getting_started";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormClone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clone";
            this.Load += new System.EventHandler(this.FormCloneLoad);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tpnlMain.ResumeLayout(false);
            this.tpnlMain.PerformLayout();
            this.optionsPanel.ResumeLayout(false);
            this.optionsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_Branches;
        private System.Windows.Forms.CheckBox cbIntializeAllSubmodules;
        private System.Windows.Forms.CheckBox cbDownloadFullHistory;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label repositoryLabel;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_From;
        private System.Windows.Forms.Label brachLabel;
        private System.Windows.Forms.Label destinationLabel;
        private System.Windows.Forms.Label subdirectoryLabel;
        private System.Windows.Forms.TableLayoutPanel tpnlMain;
        private System.Windows.Forms.ToolTip ttHints;
        private System.Windows.Forms.FlowLayoutPanel optionsPanel;
    }
}

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormClone));
            Central = new RadioButton();
            Personal = new RadioButton();
            Ok = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            repositoryLabel = new Label();
            _NO_TRANSLATE_From = new ComboBox();
            FromBrowse = new Button();
            destinationLabel = new Label();
            _NO_TRANSLATE_To = new ComboBox();
            ToBrowse = new Button();
            subdirectoryLabel = new Label();
            _NO_TRANSLATE_NewDirectory = new TextBox();
            brachLabel = new Label();
            _NO_TRANSLATE_Branches = new ComboBox();
            cbIntializeAllSubmodules = new CheckBox();
            cbDownloadFullHistory = new CheckBox();
            Info = new Label();
            groupBox1 = new GroupBox();
            CentralRepository = new RadioButton();
            PersonalRepository = new RadioButton();
            LoadSSHKey = new Button();
            tpnlMain = new TableLayoutPanel();
            optionsPanel = new FlowLayoutPanel();
            ttHints = new ToolTip(components);
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            tpnlMain.SuspendLayout();
            optionsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tpnlMain);
            MainPanel.Size = new Size(647, 318);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Ok);
            ControlsPanel.Controls.Add(LoadSSHKey);
            ControlsPanel.Location = new Point(0, 318);
            ControlsPanel.Size = new Size(647, 41);
            // 
            // Central
            // 
            Central.AutoSize = true;
            Central.Location = new Point(6, 42);
            Central.Name = "Central";
            Central.Size = new Size(274, 17);
            Central.TabIndex = 0;
            Central.Text = "Central repository, no working directory  (--bare --shared=all)";
            Central.UseVisualStyleBackColor = true;
            // 
            // Personal
            // 
            Personal.AutoSize = true;
            Personal.Checked = true;
            Personal.Location = new Point(6, 19);
            Personal.Name = "Personal";
            Personal.Size = new Size(114, 17);
            Personal.TabIndex = 0;
            Personal.TabStop = true;
            Personal.Text = "Personal repository";
            Personal.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Ok.AutoSize = true;
            Ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Ok.Location = new Point(559, 8);
            Ok.MinimumSize = new Size(75, 23);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 25);
            Ok.TabIndex = 0;
            Ok.Text = "Clone";
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += OkClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.Controls.Add(repositoryLabel, 0, 0);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_From, 1, 0);
            tableLayoutPanel1.Controls.Add(FromBrowse, 2, 0);
            tableLayoutPanel1.Controls.Add(destinationLabel, 0, 1);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_To, 1, 1);
            tableLayoutPanel1.Controls.Add(ToBrowse, 2, 1);
            tableLayoutPanel1.Controls.Add(subdirectoryLabel, 0, 2);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_NewDirectory, 1, 2);
            tableLayoutPanel1.Controls.Add(brachLabel, 0, 3);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_Branches, 1, 3);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new Size(623, 125);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // repositoryLabel
            // 
            repositoryLabel.AutoSize = true;
            repositoryLabel.Dock = DockStyle.Left;
            repositoryLabel.Location = new Point(3, 0);
            repositoryLabel.Name = "repositoryLabel";
            repositoryLabel.Size = new Size(112, 31);
            repositoryLabel.TabIndex = 0;
            repositoryLabel.Text = "Repository to &clone:";
            repositoryLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_From
            // 
            _NO_TRANSLATE_From.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_From.AutoCompleteSource = AutoCompleteSource.ListItems;
            _NO_TRANSLATE_From.FormattingEnabled = true;
            _NO_TRANSLATE_From.Location = new Point(135, 3);
            _NO_TRANSLATE_From.Name = "_NO_TRANSLATE_From";
            _NO_TRANSLATE_From.Size = new Size(385, 23);
            _NO_TRANSLATE_From.TabIndex = 1;
            _NO_TRANSLATE_From.SelectedIndexChanged += FromSelectedIndexChanged;
            _NO_TRANSLATE_From.TextUpdate += FromTextUpdate;
            // 
            // FromBrowse
            // 
            FromBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FromBrowse.Image = Properties.Images.BrowseFileExplorer;
            FromBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            FromBrowse.Location = new Point(526, 3);
            FromBrowse.Name = "FromBrowse";
            FromBrowse.Size = new Size(94, 24);
            FromBrowse.TabIndex = 2;
            FromBrowse.Text = "&Browse";
            FromBrowse.UseVisualStyleBackColor = true;
            FromBrowse.Click += FromBrowseClick;
            // 
            // destinationLabel
            // 
            destinationLabel.AutoSize = true;
            destinationLabel.Dock = DockStyle.Left;
            destinationLabel.Location = new Point(3, 31);
            destinationLabel.Name = "destinationLabel";
            destinationLabel.Size = new Size(70, 31);
            destinationLabel.TabIndex = 3;
            destinationLabel.Text = "&Destination:";
            destinationLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_To
            // 
            _NO_TRANSLATE_To.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_To.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_To.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            _NO_TRANSLATE_To.FormattingEnabled = true;
            _NO_TRANSLATE_To.Location = new Point(135, 34);
            _NO_TRANSLATE_To.Name = "_NO_TRANSLATE_To";
            _NO_TRANSLATE_To.Size = new Size(385, 23);
            _NO_TRANSLATE_To.TabIndex = 4;
            _NO_TRANSLATE_To.SelectedIndexChanged += ToSelectedIndexChanged;
            _NO_TRANSLATE_To.TextUpdate += ToTextUpdate;
            // 
            // ToBrowse
            // 
            ToBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ToBrowse.Image = Properties.Images.BrowseFileExplorer;
            ToBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            ToBrowse.Location = new Point(526, 34);
            ToBrowse.Name = "ToBrowse";
            ToBrowse.Size = new Size(94, 24);
            ToBrowse.TabIndex = 5;
            ToBrowse.Text = "B&rowse";
            ToBrowse.UseVisualStyleBackColor = true;
            ToBrowse.Click += ToBrowseClick;
            // 
            // subdirectoryLabel
            // 
            subdirectoryLabel.AutoSize = true;
            subdirectoryLabel.Dock = DockStyle.Left;
            subdirectoryLabel.Location = new Point(3, 62);
            subdirectoryLabel.Name = "subdirectoryLabel";
            subdirectoryLabel.Size = new Size(126, 31);
            subdirectoryLabel.TabIndex = 6;
            subdirectoryLabel.Text = "&Subdirectory to create:";
            subdirectoryLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_NewDirectory
            // 
            _NO_TRANSLATE_NewDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_NewDirectory.Location = new Point(135, 65);
            _NO_TRANSLATE_NewDirectory.Name = "_NO_TRANSLATE_NewDirectory";
            _NO_TRANSLATE_NewDirectory.Size = new Size(385, 23);
            _NO_TRANSLATE_NewDirectory.TabIndex = 7;
            _NO_TRANSLATE_NewDirectory.TextChanged += NewDirectoryTextChanged;
            // 
            // brachLabel
            // 
            brachLabel.AutoSize = true;
            brachLabel.Dock = DockStyle.Left;
            brachLabel.Location = new Point(3, 93);
            brachLabel.Name = "brachLabel";
            brachLabel.Size = new Size(47, 32);
            brachLabel.TabIndex = 8;
            brachLabel.Text = "&Branch:";
            brachLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_Branches
            // 
            _NO_TRANSLATE_Branches.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_Branches.FormattingEnabled = true;
            _NO_TRANSLATE_Branches.Location = new Point(135, 96);
            _NO_TRANSLATE_Branches.Name = "_NO_TRANSLATE_Branches";
            _NO_TRANSLATE_Branches.Size = new Size(385, 23);
            _NO_TRANSLATE_Branches.TabIndex = 9;
            _NO_TRANSLATE_Branches.DropDown += Branches_DropDown;
            // 
            // cbIntializeAllSubmodules
            // 
            cbIntializeAllSubmodules.AutoSize = true;
            cbIntializeAllSubmodules.Checked = true;
            cbIntializeAllSubmodules.CheckState = CheckState.Checked;
            cbIntializeAllSubmodules.Location = new Point(15, 3);
            cbIntializeAllSubmodules.Margin = new Padding(15, 3, 9, 3);
            cbIntializeAllSubmodules.Name = "cbIntializeAllSubmodules";
            cbIntializeAllSubmodules.Size = new Size(152, 19);
            cbIntializeAllSubmodules.TabIndex = 3;
            cbIntializeAllSubmodules.Text = "Initialize all submodules";
            cbIntializeAllSubmodules.UseVisualStyleBackColor = true;
            // 
            // cbDownloadFullHistory
            // 
            cbDownloadFullHistory.AutoSize = true;
            cbDownloadFullHistory.Checked = true;
            cbDownloadFullHistory.CheckState = CheckState.Checked;
            cbDownloadFullHistory.Location = new Point(191, 3);
            cbDownloadFullHistory.Margin = new Padding(15, 3, 9, 3);
            cbDownloadFullHistory.Name = "cbDownloadFullHistory";
            cbDownloadFullHistory.Size = new Size(139, 19);
            cbDownloadFullHistory.TabIndex = 4;
            cbDownloadFullHistory.Text = "Download full &history";
            ttHints.SetToolTip(cbDownloadFullHistory, resources.GetString("cbDownloadFullHistory.ToolTip"));
            // 
            // Info
            // 
            Info.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Info.BackColor = SystemColors.Info;
            Info.BorderStyle = BorderStyle.FixedSingle;
            Info.Location = new Point(3, 138);
            Info.Margin = new Padding(3, 13, 3, 0);
            Info.Name = "Info";
            Info.Size = new Size(617, 42);
            Info.TabIndex = 1;
            Info.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(CentralRepository);
            groupBox1.Controls.Add(PersonalRepository);
            groupBox1.Location = new Point(3, 184);
            groupBox1.Margin = new Padding(3, 4, 3, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(617, 78);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Repository type";
            // 
            // CentralRepository
            // 
            CentralRepository.AutoSize = true;
            CentralRepository.Location = new Point(20, 48);
            CentralRepository.Name = "CentralRepository";
            CentralRepository.Size = new Size(277, 19);
            CentralRepository.TabIndex = 0;
            CentralRepository.Text = "P&ublic repository, no working directory  (--bare)";
            CentralRepository.UseVisualStyleBackColor = true;
            // 
            // PersonalRepository
            // 
            PersonalRepository.AutoSize = true;
            PersonalRepository.Checked = true;
            PersonalRepository.Location = new Point(20, 25);
            PersonalRepository.Name = "PersonalRepository";
            PersonalRepository.Size = new Size(126, 19);
            PersonalRepository.TabIndex = 1;
            PersonalRepository.TabStop = true;
            PersonalRepository.Text = "&Personal repository";
            PersonalRepository.UseVisualStyleBackColor = true;
            // 
            // LoadSSHKey
            // 
            LoadSSHKey.AutoSize = true;
            LoadSSHKey.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            LoadSSHKey.Image = Properties.Images.Putty;
            LoadSSHKey.ImageAlign = ContentAlignment.MiddleLeft;
            LoadSSHKey.Location = new Point(433, 8);
            LoadSSHKey.MinimumSize = new Size(75, 23);
            LoadSSHKey.Name = "LoadSSHKey";
            LoadSSHKey.Padding = new Padding(8, 0, 8, 0);
            LoadSSHKey.Size = new Size(120, 25);
            LoadSSHKey.TabIndex = 1;
            LoadSSHKey.Text = "&Load SSH key";
            LoadSSHKey.TextImageRelation = TextImageRelation.ImageBeforeText;
            LoadSSHKey.UseVisualStyleBackColor = true;
            LoadSSHKey.Click += LoadSshKeyClick;
            // 
            // tpnlMain
            // 
            tpnlMain.AutoSize = true;
            tpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tpnlMain.ColumnCount = 1;
            tpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tpnlMain.Controls.Add(tableLayoutPanel1);
            tpnlMain.Controls.Add(Info);
            tpnlMain.Controls.Add(groupBox1);
            tpnlMain.Controls.Add(optionsPanel);
            tpnlMain.Dock = DockStyle.Fill;
            tpnlMain.Location = new Point(12, 12);
            tpnlMain.Margin = new Padding(0);
            tpnlMain.Name = "tpnlMain";
            tpnlMain.RowCount = 5;
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.Size = new Size(623, 294);
            tpnlMain.TabIndex = 0;
            // 
            // optionsPanel
            // 
            optionsPanel.AutoSize = true;
            optionsPanel.Controls.Add(cbIntializeAllSubmodules);
            optionsPanel.Controls.Add(cbDownloadFullHistory);
            optionsPanel.Dock = DockStyle.Fill;
            optionsPanel.Location = new Point(0, 272);
            optionsPanel.Margin = new Padding(0, 10, 0, 0);
            optionsPanel.Name = "optionsPanel";
            optionsPanel.Size = new Size(623, 25);
            optionsPanel.TabIndex = 2;
            // 
            // FormClone
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(647, 359);
            HelpButton = true;
            ManualSectionAnchorName = "clone-repository";
            ManualSectionSubfolder = "getting_started";
            MaximizeBox = false;
            MinimizeBox = false;
            MaximumSize = new Size(950, 398);
            MinimumSize = new Size(450, 398);
            Name = "FormClone";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Clone";
            Load += FormCloneLoad;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tpnlMain.ResumeLayout(false);
            tpnlMain.PerformLayout();
            optionsPanel.ResumeLayout(false);
            optionsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button Ok;
        private Button ToBrowse;
        private Button FromBrowse;
        private ComboBox _NO_TRANSLATE_To;
        private GroupBox groupBox1;
        private RadioButton CentralRepository;
        private RadioButton PersonalRepository;
        private RadioButton Central;
        private RadioButton Personal;
        private Button LoadSSHKey;
        private TextBox _NO_TRANSLATE_NewDirectory;
        private Label Info;
        private ComboBox _NO_TRANSLATE_Branches;
        private CheckBox cbIntializeAllSubmodules;
        private CheckBox cbDownloadFullHistory;
        private TableLayoutPanel tableLayoutPanel1;
        private Label repositoryLabel;
        private ComboBox _NO_TRANSLATE_From;
        private Label brachLabel;
        private Label destinationLabel;
        private Label subdirectoryLabel;
        private TableLayoutPanel tpnlMain;
        private ToolTip ttHints;
        private FlowLayoutPanel optionsPanel;
    }
}

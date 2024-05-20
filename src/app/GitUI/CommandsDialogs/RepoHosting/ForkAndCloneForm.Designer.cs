namespace GitUI.CommandsDialogs.RepoHosting
{
    partial class ForkAndCloneForm
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
            columnHeaderMyReposName = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderMyReposIsFork = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderMyReposForks = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderSearchName = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderSearchOwner = ((ColumnHeader)(new ColumnHeader()));
            columnHeaderSearchForks = ((ColumnHeader)(new ColumnHeader()));
            cloneBtn = new Button();
            _NO_TRANSLATE_closeBtn = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            tabControl = new TabControl();
            myReposPage = new TabPage();
            tableLayoutPanel5 = new TableLayoutPanel();
            helpTextLbl = new Label();
            myReposLV = new GitUI.UserControls.NativeListView();
            columnHeaderMyReposIsPrivate = ((ColumnHeader)(new ColumnHeader()));
            searchReposPage = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            searchTB = new TextBox();
            searchBtn = new Button();
            orLbl = new Label();
            getFromUserBtn = new Button();
            forkBtn = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            searchResultsLV = new GitUI.UserControls.NativeListView();
            columnHeaderSearchIsFork = ((ColumnHeader)(new ColumnHeader()));
            tableLayoutPanel4 = new TableLayoutPanel();
            openGitupPageBtn = new Button();
            searchResultItemDescription = new TextBox();
            descriptionLbl = new Label();
            cloneSetupGB = new GroupBox();
            depthLabel = new Label();
            ProtocolDropdownList = new ComboBox();
            ProtocolLabel = new Label();
            cloneInfoText = new Label();
            addUpstreamRemoteAsCB = new ComboBox();
            label3 = new Label();
            createDirTB = new TextBox();
            createDirectoryLbl = new Label();
            label1 = new Label();
            browseForCloneToDirbtn = new Button();
            destinationTB = new TextBox();
            depthUpDown = new NumericUpDown();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tableLayoutPanel2.SuspendLayout();
            tabControl.SuspendLayout();
            myReposPage.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            searchReposPage.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            cloneSetupGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(depthUpDown)).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // columnHeaderMyReposName
            // 
            columnHeaderMyReposName.Text = "Name";
            columnHeaderMyReposName.Width = 180;
            // 
            // columnHeaderMyReposIsFork
            // 
            columnHeaderMyReposIsFork.Text = "Is fork";
            columnHeaderMyReposIsFork.TextAlign = HorizontalAlignment.Center;
            columnHeaderMyReposIsFork.Width = 45;
            // 
            // columnHeaderMyReposForks
            // 
            columnHeaderMyReposForks.Text = "# Forks";
            columnHeaderMyReposForks.TextAlign = HorizontalAlignment.Right;
            columnHeaderMyReposForks.Width = 50;
            // 
            // columnHeaderSearchName
            // 
            columnHeaderSearchName.Text = "Name";
            columnHeaderSearchName.Width = 180;
            // 
            // columnHeaderSearchOwner
            // 
            columnHeaderSearchOwner.Text = "Owner";
            columnHeaderSearchOwner.Width = 110;
            // 
            // columnHeaderSearchForks
            // 
            columnHeaderSearchForks.Text = "# Forks";
            columnHeaderSearchForks.TextAlign = HorizontalAlignment.Right;
            columnHeaderSearchForks.Width = 40;
            // 
            // cloneBtn
            // 
            cloneBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cloneBtn.Location = new Point(489, 3);
            cloneBtn.Name = "cloneBtn";
            cloneBtn.Size = new Size(120, 30);
            cloneBtn.TabIndex = 0;
            cloneBtn.Text = "Clone";
            cloneBtn.UseVisualStyleBackColor = true;
            cloneBtn.Click += _cloneBtn_Click;
            // 
            // _NO_TRANSLATE_closeBtn
            // 
            _NO_TRANSLATE_closeBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _NO_TRANSLATE_closeBtn.Location = new Point(615, 3);
            _NO_TRANSLATE_closeBtn.Name = "closeBtn";
            _NO_TRANSLATE_closeBtn.Size = new Size(120, 30);
            _NO_TRANSLATE_closeBtn.TabIndex = 1;
            _NO_TRANSLATE_closeBtn.Text = TranslatedStrings.Close;
            _NO_TRANSLATE_closeBtn.UseVisualStyleBackColor = true;
            _NO_TRANSLATE_closeBtn.Click += _closeBtn_Click;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tabControl, 0, 0);
            tableLayoutPanel2.Controls.Add(cloneSetupGB, 0, 1);
            tableLayoutPanel2.Controls.Add(flowLayoutPanel1, 0, 2);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(744, 552);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(myReposPage);
            tabControl.Controls.Add(searchReposPage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(3, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(738, 323);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += _tabControl_SelectedIndexChanged;
            // 
            // myReposPage
            // 
            myReposPage.Controls.Add(tableLayoutPanel5);
            myReposPage.Location = new Point(4, 22);
            myReposPage.Name = "myReposPage";
            myReposPage.Padding = new Padding(3);
            myReposPage.Size = new Size(730, 297);
            myReposPage.TabIndex = 0;
            myReposPage.Text = "My repositories";
            myReposPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 2;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutPanel5.Controls.Add(helpTextLbl, 1, 0);
            tableLayoutPanel5.Controls.Add(myReposLV, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(724, 291);
            tableLayoutPanel5.TabIndex = 0;
            // 
            // helpTextLbl
            // 
            helpTextLbl.Dock = DockStyle.Fill;
            helpTextLbl.Location = new Point(509, 0);
            helpTextLbl.Name = "helpTextLbl";
            helpTextLbl.Size = new Size(212, 291);
            helpTextLbl.TabIndex = 0;
            helpTextLbl.Text = "If you want to fork a repository owned by somebody else, go to the Search for rep" +
    "ositories tab.";
            helpTextLbl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // myReposLV
            // 
            myReposLV.Columns.AddRange(new ColumnHeader[] {
            columnHeaderMyReposName,
            columnHeaderMyReposIsFork,
            columnHeaderMyReposForks,
            columnHeaderMyReposIsPrivate});
            myReposLV.Dock = DockStyle.Fill;
            myReposLV.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            myReposLV.HideSelection = false;
            myReposLV.Location = new Point(3, 3);
            myReposLV.MultiSelect = false;
            myReposLV.Name = "myReposLV";
            myReposLV.ShowGroups = false;
            myReposLV.Size = new Size(500, 285);
            myReposLV.TabIndex = 0;
            myReposLV.UseCompatibleStateImageBehavior = false;
            myReposLV.View = View.Details;
            myReposLV.SelectedIndexChanged += _myReposLV_SelectedIndexChanged;
            // 
            // columnHeaderMyReposIsPrivate
            // 
            columnHeaderMyReposIsPrivate.Text = "Private";
            columnHeaderMyReposIsPrivate.TextAlign = HorizontalAlignment.Center;
            columnHeaderMyReposIsPrivate.Width = 45;
            // 
            // searchReposPage
            // 
            searchReposPage.Controls.Add(tableLayoutPanel1);
            searchReposPage.Location = new Point(4, 22);
            searchReposPage.Name = "searchReposPage";
            searchReposPage.Padding = new Padding(3);
            searchReposPage.Size = new Size(730, 297);
            searchReposPage.TabIndex = 1;
            searchReposPage.Text = "Search for repositories";
            searchReposPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(forkBtn, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(724, 291);
            tableLayoutPanel1.TabIndex = 23;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(searchTB);
            flowLayoutPanel2.Controls.Add(searchBtn);
            flowLayoutPanel2.Controls.Add(orLbl);
            flowLayoutPanel2.Controls.Add(getFromUserBtn);
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.Location = new Point(3, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(718, 35);
            flowLayoutPanel2.TabIndex = 0;
            // 
            // searchTB
            // 
            searchTB.Location = new Point(3, 3);
            searchTB.Name = "searchTB";
            searchTB.Size = new Size(198, 20);
            searchTB.TabIndex = 0;
            searchTB.Enter += _searchTB_Enter;
            searchTB.Leave += _searchTB_Leave;
            // 
            // searchBtn
            // 
            searchBtn.Location = new Point(207, 3);
            searchBtn.Name = "searchBtn";
            searchBtn.Size = new Size(93, 23);
            searchBtn.TabIndex = 1;
            searchBtn.Text = "Search";
            searchBtn.UseVisualStyleBackColor = true;
            searchBtn.Click += _searchBtn_Click;
            // 
            // orLbl
            // 
            orLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            orLbl.AutoSize = true;
            orLbl.Location = new Point(306, 0);
            orLbl.Name = "orLbl";
            orLbl.Size = new Size(16, 13);
            orLbl.TabIndex = 22;
            orLbl.Text = "or";
            // 
            // getFromUserBtn
            // 
            getFromUserBtn.Location = new Point(328, 3);
            getFromUserBtn.Name = "getFromUserBtn";
            getFromUserBtn.Size = new Size(124, 23);
            getFromUserBtn.TabIndex = 2;
            getFromUserBtn.Text = "Get from user";
            getFromUserBtn.UseVisualStyleBackColor = true;
            getFromUserBtn.Click += _getFromUserBtn_Click;
            // 
            // forkBtn
            // 
            forkBtn.Location = new Point(3, 265);
            forkBtn.Name = "forkBtn";
            forkBtn.Size = new Size(150, 23);
            forkBtn.TabIndex = 4;
            forkBtn.Text = "Fork!";
            forkBtn.UseVisualStyleBackColor = true;
            forkBtn.Click += _forkBtn_Click;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel3.Controls.Add(searchResultsLV, 0, 0);
            tableLayoutPanel3.Controls.Add(tableLayoutPanel4, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 44);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(718, 215);
            tableLayoutPanel3.TabIndex = 5;
            // 
            // searchResultsLV
            // 
            searchResultsLV.Columns.AddRange(new ColumnHeader[] {
            columnHeaderSearchName,
            columnHeaderSearchOwner,
            columnHeaderSearchIsFork,
            columnHeaderSearchForks});
            searchResultsLV.Dock = DockStyle.Fill;
            searchResultsLV.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            searchResultsLV.HideSelection = false;
            searchResultsLV.Location = new Point(3, 3);
            searchResultsLV.MultiSelect = false;
            searchResultsLV.Name = "searchResultsLV";
            searchResultsLV.ShowGroups = false;
            searchResultsLV.Size = new Size(424, 209);
            searchResultsLV.TabIndex = 3;
            searchResultsLV.UseCompatibleStateImageBehavior = false;
            searchResultsLV.View = View.Details;
            searchResultsLV.SelectedIndexChanged += _searchResultsLV_SelectedIndexChanged;
            // 
            // columnHeaderSearchIsFork
            // 
            columnHeaderSearchIsFork.Text = "Is fork";
            columnHeaderSearchIsFork.TextAlign = HorizontalAlignment.Center;
            columnHeaderSearchIsFork.Width = 41;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(openGitupPageBtn, 0, 2);
            tableLayoutPanel4.Controls.Add(searchResultItemDescription, 0, 1);
            tableLayoutPanel4.Controls.Add(descriptionLbl, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(433, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 3;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(282, 209);
            tableLayoutPanel4.TabIndex = 4;
            // 
            // openGitupPageBtn
            // 
            openGitupPageBtn.Location = new Point(3, 183);
            openGitupPageBtn.Name = "openGitupPageBtn";
            openGitupPageBtn.Size = new Size(116, 23);
            openGitupPageBtn.TabIndex = 5;
            openGitupPageBtn.Text = "Open github page";
            openGitupPageBtn.UseVisualStyleBackColor = true;
            openGitupPageBtn.Click += _openGitupPageBtn_Click;
            // 
            // searchResultItemDescription
            // 
            searchResultItemDescription.BackColor = SystemColors.ControlLightLight;
            searchResultItemDescription.Dock = DockStyle.Fill;
            searchResultItemDescription.Location = new Point(3, 16);
            searchResultItemDescription.Multiline = true;
            searchResultItemDescription.Name = "searchResultItemDescription";
            searchResultItemDescription.ReadOnly = true;
            searchResultItemDescription.Size = new Size(276, 161);
            searchResultItemDescription.TabIndex = 18;
            // 
            // descriptionLbl
            // 
            descriptionLbl.AutoSize = true;
            descriptionLbl.Location = new Point(3, 0);
            descriptionLbl.Name = "descriptionLbl";
            descriptionLbl.Size = new Size(63, 13);
            descriptionLbl.TabIndex = 17;
            descriptionLbl.Text = "Description:";
            // 
            // cloneSetupGB
            // 
            cloneSetupGB.Controls.Add(depthLabel);
            cloneSetupGB.Controls.Add(ProtocolDropdownList);
            cloneSetupGB.Controls.Add(ProtocolLabel);
            cloneSetupGB.Controls.Add(cloneInfoText);
            cloneSetupGB.Controls.Add(addUpstreamRemoteAsCB);
            cloneSetupGB.Controls.Add(label3);
            cloneSetupGB.Controls.Add(createDirTB);
            cloneSetupGB.Controls.Add(createDirectoryLbl);
            cloneSetupGB.Controls.Add(label1);
            cloneSetupGB.Controls.Add(browseForCloneToDirbtn);
            cloneSetupGB.Controls.Add(destinationTB);
            cloneSetupGB.Controls.Add(depthUpDown);
            cloneSetupGB.Dock = DockStyle.Fill;
            cloneSetupGB.Location = new Point(4, 333);
            cloneSetupGB.Margin = new Padding(4);
            cloneSetupGB.Name = "cloneSetupGB";
            cloneSetupGB.Padding = new Padding(4);
            cloneSetupGB.Size = new Size(736, 175);
            cloneSetupGB.TabIndex = 1;
            cloneSetupGB.TabStop = false;
            cloneSetupGB.Text = "Clone";
            // 
            // depthLabel
            // 
            depthLabel.AutoSize = true;
            depthLabel.Location = new Point(7, 135);
            depthLabel.Name = "depthLabel";
            depthLabel.Size = new Size(63, 13);
            depthLabel.TabIndex = 10;
            depthLabel.Text = "Limit Depth:";
            // 
            // ProtocolDropdownList
            // 
            ProtocolDropdownList.DropDownStyle = ComboBoxStyle.DropDownList;
            ProtocolDropdownList.FormattingEnabled = true;
            ProtocolDropdownList.Location = new Point(470, 32);
            ProtocolDropdownList.Name = "ProtocolDropdownList";
            ProtocolDropdownList.Size = new Size(121, 21);
            ProtocolDropdownList.TabIndex = 4;
            ProtocolDropdownList.SelectedIndexChanged += ProtocolSelectionChanged;
            // 
            // ProtocolLabel
            // 
            ProtocolLabel.AutoSize = true;
            ProtocolLabel.Location = new Point(418, 36);
            ProtocolLabel.Name = "ProtocolLabel";
            ProtocolLabel.Size = new Size(49, 13);
            ProtocolLabel.TabIndex = 3;
            ProtocolLabel.Text = "Protocol:";
            // 
            // cloneInfoText
            // 
            cloneInfoText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cloneInfoText.Location = new Point(10, 97);
            cloneInfoText.Name = "cloneInfoText";
            cloneInfoText.Size = new Size(719, 35);
            cloneInfoText.TabIndex = 9;
            // 
            // addUpstreamRemoteAsCB
            // 
            addUpstreamRemoteAsCB.Location = new Point(212, 71);
            addUpstreamRemoteAsCB.Name = "addUpstreamRemoteAsCB";
            addUpstreamRemoteAsCB.Size = new Size(200, 21);
            addUpstreamRemoteAsCB.TabIndex = 8;
            addUpstreamRemoteAsCB.TextChanged += _addRemoteAsTB_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(211, 55);
            label3.Name = "label3";
            label3.Size = new Size(124, 13);
            label3.TabIndex = 7;
            label3.Text = "Add upstream remote as:";
            // 
            // createDirTB
            // 
            createDirTB.Location = new Point(10, 71);
            createDirTB.Name = "createDirTB";
            createDirTB.Size = new Size(183, 20);
            createDirTB.TabIndex = 6;
            createDirTB.TextChanged += _createDirTB_TextChanged;
            createDirTB.Validating += _createDirTB_Validating;
            // 
            // createDirectoryLbl
            // 
            createDirectoryLbl.AutoSize = true;
            createDirectoryLbl.Location = new Point(7, 55);
            createDirectoryLbl.Name = "createDirectoryLbl";
            createDirectoryLbl.Size = new Size(84, 13);
            createDirectoryLbl.TabIndex = 5;
            createDirectoryLbl.Text = "Create directory:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 16);
            label1.Name = "label1";
            label1.Size = new Size(92, 13);
            label1.TabIndex = 0;
            label1.Text = "Destination folder:";
            // 
            // browseForCloneToDirbtn
            // 
            browseForCloneToDirbtn.Location = new Point(310, 32);
            browseForCloneToDirbtn.Name = "browseForCloneToDirbtn";
            browseForCloneToDirbtn.Size = new Size(102, 23);
            browseForCloneToDirbtn.TabIndex = 2;
            browseForCloneToDirbtn.Text = "Browse...";
            browseForCloneToDirbtn.UseVisualStyleBackColor = true;
            browseForCloneToDirbtn.Click += _browseForCloneToDirbtn_Click;
            // 
            // destinationTB
            // 
            destinationTB.Location = new Point(10, 32);
            destinationTB.Name = "destinationTB";
            destinationTB.Size = new Size(294, 20);
            destinationTB.TabIndex = 1;
            destinationTB.TextChanged += _destinationTB_TextChanged;
            destinationTB.Validating += _destinationTB_Validating;
            // 
            // depthUpDown
            // 
            depthUpDown.Location = new Point(10, 151);
            depthUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            depthUpDown.Name = "depthUpDown";
            depthUpDown.Size = new Size(100, 20);
            depthUpDown.TabIndex = 11;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(_NO_TRANSLATE_closeBtn);
            flowLayoutPanel1.Controls.Add(cloneBtn);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(3, 515);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(738, 34);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // ForkAndCloneForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(744, 552);
            Controls.Add(tableLayoutPanel2);
            Name = "ForkAndCloneForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Remote repository fork and clone";
            Load += ForkAndCloneForm_Load;
            tableLayoutPanel2.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            myReposPage.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            searchReposPage.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            cloneSetupGB.ResumeLayout(false);
            cloneSetupGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(depthUpDown)).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Button cloneBtn;
        private Button _NO_TRANSLATE_closeBtn;
        private TableLayoutPanel tableLayoutPanel2;
        private TabControl tabControl;
        private TabPage myReposPage;
        private Label helpTextLbl;
        private UserControls.NativeListView myReposLV;
        private ColumnHeader columnHeaderMyReposIsPrivate;
        private TabPage searchReposPage;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private TextBox searchTB;
        private Button searchBtn;
        private Label orLbl;
        private Button getFromUserBtn;
        private Button forkBtn;
        private TableLayoutPanel tableLayoutPanel3;
        private UserControls.NativeListView searchResultsLV;
        private ColumnHeader columnHeaderSearchIsFork;
        private TableLayoutPanel tableLayoutPanel4;
        private Button openGitupPageBtn;
        private TextBox searchResultItemDescription;
        private Label descriptionLbl;
        private GroupBox cloneSetupGB;
        private ComboBox addUpstreamRemoteAsCB;
        private Label label3;
        private TextBox createDirTB;
        private Label createDirectoryLbl;
        private Label label1;
        private Button browseForCloneToDirbtn;
        private TextBox destinationTB;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label cloneInfoText;
        private TableLayoutPanel tableLayoutPanel5;
        private ComboBox ProtocolDropdownList;
        private Label ProtocolLabel;
        private NumericUpDown depthUpDown;
        private Label depthLabel;
        private ColumnHeader columnHeaderMyReposName;
        private ColumnHeader columnHeaderMyReposIsFork;
        private ColumnHeader columnHeaderMyReposForks;
        private ColumnHeader columnHeaderSearchName;
        private ColumnHeader columnHeaderSearchOwner;
        private ColumnHeader columnHeaderSearchForks;
    }
}

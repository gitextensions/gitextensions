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
            this.columnHeaderMyReposName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMyReposIsFork = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMyReposForks = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSearchName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSearchOwner = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSearchForks = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cloneBtn = new System.Windows.Forms.Button();
            this.closeBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.myReposPage = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.helpTextLbl = new System.Windows.Forms.Label();
            this.myReposLV = new GitUI.UserControls.NativeListView();
            this.columnHeaderMyReposIsPrivate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.searchReposPage = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.searchTB = new System.Windows.Forms.TextBox();
            this.searchBtn = new System.Windows.Forms.Button();
            this.orLbl = new System.Windows.Forms.Label();
            this.getFromUserBtn = new System.Windows.Forms.Button();
            this.forkBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.searchResultsLV = new GitUI.UserControls.NativeListView();
            this.columnHeaderSearchIsFork = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.openGitupPageBtn = new System.Windows.Forms.Button();
            this.searchResultItemDescription = new System.Windows.Forms.TextBox();
            this.descriptionLbl = new System.Windows.Forms.Label();
            this.cloneSetupGB = new System.Windows.Forms.GroupBox();
            this.depthLabel = new System.Windows.Forms.Label();
            this.ProtocolDropdownList = new System.Windows.Forms.ComboBox();
            this.ProtocolLabel = new System.Windows.Forms.Label();
            this.cloneInfoText = new System.Windows.Forms.Label();
            this.addUpstreamRemoteAsCB = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.createDirTB = new System.Windows.Forms.TextBox();
            this.createDirectoryLbl = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.browseForCloneToDirbtn = new System.Windows.Forms.Button();
            this.destinationTB = new System.Windows.Forms.TextBox();
            this.depthUpDown = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.myReposPage.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.searchReposPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.cloneSetupGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.depthUpDown)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeaderMyReposName
            // 
            this.columnHeaderMyReposName.Text = "Name";
            this.columnHeaderMyReposName.Width = 180;
            // 
            // columnHeaderMyReposIsFork
            // 
            this.columnHeaderMyReposIsFork.Text = "Is fork";
            this.columnHeaderMyReposIsFork.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderMyReposIsFork.Width = 45;
            // 
            // columnHeaderMyReposForks
            // 
            this.columnHeaderMyReposForks.Text = "# Forks";
            this.columnHeaderMyReposForks.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderMyReposForks.Width = 50;
            // 
            // columnHeaderSearchName
            // 
            this.columnHeaderSearchName.Text = "Name";
            this.columnHeaderSearchName.Width = 180;
            // 
            // columnHeaderSearchOwner
            // 
            this.columnHeaderSearchOwner.Text = "Owner";
            this.columnHeaderSearchOwner.Width = 110;
            // 
            // columnHeaderSearchForks
            // 
            this.columnHeaderSearchForks.Text = "# Forks";
            this.columnHeaderSearchForks.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderSearchForks.Width = 40;
            // 
            // cloneBtn
            // 
            this.cloneBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cloneBtn.Location = new System.Drawing.Point(489, 3);
            this.cloneBtn.Name = "cloneBtn";
            this.cloneBtn.Size = new System.Drawing.Size(120, 30);
            this.cloneBtn.TabIndex = 0;
            this.cloneBtn.Text = "Clone";
            this.cloneBtn.UseVisualStyleBackColor = true;
            this.cloneBtn.Click += new System.EventHandler(this._cloneBtn_Click);
            // 
            // closeBtn
            // 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.Location = new System.Drawing.Point(615, 3);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(120, 30);
            this.closeBtn.TabIndex = 1;
            this.closeBtn.Text = "Close";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this._closeBtn_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tabControl, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cloneSetupGB, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(744, 552);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.myReposPage);
            this.tabControl.Controls.Add(this.searchReposPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(738, 323);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this._tabControl_SelectedIndexChanged);
            // 
            // myReposPage
            // 
            this.myReposPage.Controls.Add(this.tableLayoutPanel5);
            this.myReposPage.Location = new System.Drawing.Point(4, 22);
            this.myReposPage.Name = "myReposPage";
            this.myReposPage.Padding = new System.Windows.Forms.Padding(3);
            this.myReposPage.Size = new System.Drawing.Size(730, 297);
            this.myReposPage.TabIndex = 0;
            this.myReposPage.Text = "My repositories";
            this.myReposPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel5.Controls.Add(this.helpTextLbl, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.myReposLV, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(724, 291);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // helpTextLbl
            // 
            this.helpTextLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpTextLbl.Location = new System.Drawing.Point(509, 0);
            this.helpTextLbl.Name = "helpTextLbl";
            this.helpTextLbl.Size = new System.Drawing.Size(212, 291);
            this.helpTextLbl.TabIndex = 0;
            this.helpTextLbl.Text = "If you want to fork a repository owned by somebody else, go to the Search for rep" +
    "ositories tab.";
            this.helpTextLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // myReposLV
            // 
            this.myReposLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMyReposName,
            this.columnHeaderMyReposIsFork,
            this.columnHeaderMyReposForks,
            this.columnHeaderMyReposIsPrivate});
            this.myReposLV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myReposLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.myReposLV.HideSelection = false;
            this.myReposLV.Location = new System.Drawing.Point(3, 3);
            this.myReposLV.MultiSelect = false;
            this.myReposLV.Name = "myReposLV";
            this.myReposLV.ShowGroups = false;
            this.myReposLV.Size = new System.Drawing.Size(500, 285);
            this.myReposLV.TabIndex = 0;
            this.myReposLV.UseCompatibleStateImageBehavior = false;
            this.myReposLV.View = System.Windows.Forms.View.Details;
            this.myReposLV.SelectedIndexChanged += new System.EventHandler(this._myReposLV_SelectedIndexChanged);
            // 
            // columnHeaderMyReposIsPrivate
            // 
            this.columnHeaderMyReposIsPrivate.Text = "Private";
            this.columnHeaderMyReposIsPrivate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderMyReposIsPrivate.Width = 45;
            // 
            // searchReposPage
            // 
            this.searchReposPage.Controls.Add(this.tableLayoutPanel1);
            this.searchReposPage.Location = new System.Drawing.Point(4, 22);
            this.searchReposPage.Name = "searchReposPage";
            this.searchReposPage.Padding = new System.Windows.Forms.Padding(3);
            this.searchReposPage.Size = new System.Drawing.Size(730, 297);
            this.searchReposPage.TabIndex = 1;
            this.searchReposPage.Text = "Search for repositories";
            this.searchReposPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.forkBtn, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(724, 291);
            this.tableLayoutPanel1.TabIndex = 23;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.searchTB);
            this.flowLayoutPanel2.Controls.Add(this.searchBtn);
            this.flowLayoutPanel2.Controls.Add(this.orLbl);
            this.flowLayoutPanel2.Controls.Add(this.getFromUserBtn);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(718, 35);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // searchTB
            // 
            this.searchTB.Location = new System.Drawing.Point(3, 3);
            this.searchTB.Name = "searchTB";
            this.searchTB.Size = new System.Drawing.Size(198, 20);
            this.searchTB.TabIndex = 0;
            this.searchTB.Enter += new System.EventHandler(this._searchTB_Enter);
            this.searchTB.Leave += new System.EventHandler(this._searchTB_Leave);
            // 
            // searchBtn
            // 
            this.searchBtn.Location = new System.Drawing.Point(207, 3);
            this.searchBtn.Name = "searchBtn";
            this.searchBtn.Size = new System.Drawing.Size(93, 23);
            this.searchBtn.TabIndex = 1;
            this.searchBtn.Text = "Search";
            this.searchBtn.UseVisualStyleBackColor = true;
            this.searchBtn.Click += new System.EventHandler(this._searchBtn_Click);
            // 
            // orLbl
            // 
            this.orLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.orLbl.AutoSize = true;
            this.orLbl.Location = new System.Drawing.Point(306, 0);
            this.orLbl.Name = "orLbl";
            this.orLbl.Size = new System.Drawing.Size(16, 13);
            this.orLbl.TabIndex = 22;
            this.orLbl.Text = "or";
            // 
            // getFromUserBtn
            // 
            this.getFromUserBtn.Location = new System.Drawing.Point(328, 3);
            this.getFromUserBtn.Name = "getFromUserBtn";
            this.getFromUserBtn.Size = new System.Drawing.Size(124, 23);
            this.getFromUserBtn.TabIndex = 2;
            this.getFromUserBtn.Text = "Get from user";
            this.getFromUserBtn.UseVisualStyleBackColor = true;
            this.getFromUserBtn.Click += new System.EventHandler(this._getFromUserBtn_Click);
            // 
            // forkBtn
            // 
            this.forkBtn.Location = new System.Drawing.Point(3, 265);
            this.forkBtn.Name = "forkBtn";
            this.forkBtn.Size = new System.Drawing.Size(150, 23);
            this.forkBtn.TabIndex = 4;
            this.forkBtn.Text = "Fork!";
            this.forkBtn.UseVisualStyleBackColor = true;
            this.forkBtn.Click += new System.EventHandler(this._forkBtn_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.Controls.Add(this.searchResultsLV, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 44);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(718, 215);
            this.tableLayoutPanel3.TabIndex = 5;
            // 
            // searchResultsLV
            // 
            this.searchResultsLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSearchName,
            this.columnHeaderSearchOwner,
            this.columnHeaderSearchIsFork,
            this.columnHeaderSearchForks});
            this.searchResultsLV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchResultsLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.searchResultsLV.HideSelection = false;
            this.searchResultsLV.Location = new System.Drawing.Point(3, 3);
            this.searchResultsLV.MultiSelect = false;
            this.searchResultsLV.Name = "searchResultsLV";
            this.searchResultsLV.ShowGroups = false;
            this.searchResultsLV.Size = new System.Drawing.Size(424, 209);
            this.searchResultsLV.TabIndex = 3;
            this.searchResultsLV.UseCompatibleStateImageBehavior = false;
            this.searchResultsLV.View = System.Windows.Forms.View.Details;
            this.searchResultsLV.SelectedIndexChanged += new System.EventHandler(this._searchResultsLV_SelectedIndexChanged);
            // 
            // columnHeaderSearchIsFork
            // 
            this.columnHeaderSearchIsFork.Text = "Is fork";
            this.columnHeaderSearchIsFork.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderSearchIsFork.Width = 41;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.openGitupPageBtn, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.searchResultItemDescription, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.descriptionLbl, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(433, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(282, 209);
            this.tableLayoutPanel4.TabIndex = 4;
            // 
            // openGitupPageBtn
            // 
            this.openGitupPageBtn.Location = new System.Drawing.Point(3, 183);
            this.openGitupPageBtn.Name = "openGitupPageBtn";
            this.openGitupPageBtn.Size = new System.Drawing.Size(116, 23);
            this.openGitupPageBtn.TabIndex = 5;
            this.openGitupPageBtn.Text = "Open github page";
            this.openGitupPageBtn.UseVisualStyleBackColor = true;
            this.openGitupPageBtn.Click += new System.EventHandler(this._openGitupPageBtn_Click);
            // 
            // searchResultItemDescription
            // 
            this.searchResultItemDescription.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.searchResultItemDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchResultItemDescription.Location = new System.Drawing.Point(3, 16);
            this.searchResultItemDescription.Multiline = true;
            this.searchResultItemDescription.Name = "searchResultItemDescription";
            this.searchResultItemDescription.ReadOnly = true;
            this.searchResultItemDescription.Size = new System.Drawing.Size(276, 161);
            this.searchResultItemDescription.TabIndex = 18;
            // 
            // descriptionLbl
            // 
            this.descriptionLbl.AutoSize = true;
            this.descriptionLbl.Location = new System.Drawing.Point(3, 0);
            this.descriptionLbl.Name = "descriptionLbl";
            this.descriptionLbl.Size = new System.Drawing.Size(63, 13);
            this.descriptionLbl.TabIndex = 17;
            this.descriptionLbl.Text = "Description:";
            // 
            // cloneSetupGB
            // 
            this.cloneSetupGB.Controls.Add(this.depthLabel);
            this.cloneSetupGB.Controls.Add(this.ProtocolDropdownList);
            this.cloneSetupGB.Controls.Add(this.ProtocolLabel);
            this.cloneSetupGB.Controls.Add(this.cloneInfoText);
            this.cloneSetupGB.Controls.Add(this.addUpstreamRemoteAsCB);
            this.cloneSetupGB.Controls.Add(this.label3);
            this.cloneSetupGB.Controls.Add(this.createDirTB);
            this.cloneSetupGB.Controls.Add(this.createDirectoryLbl);
            this.cloneSetupGB.Controls.Add(this.label1);
            this.cloneSetupGB.Controls.Add(this.browseForCloneToDirbtn);
            this.cloneSetupGB.Controls.Add(this.destinationTB);
            this.cloneSetupGB.Controls.Add(this.depthUpDown);
            this.cloneSetupGB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cloneSetupGB.Location = new System.Drawing.Point(4, 333);
            this.cloneSetupGB.Margin = new System.Windows.Forms.Padding(4);
            this.cloneSetupGB.Name = "cloneSetupGB";
            this.cloneSetupGB.Padding = new System.Windows.Forms.Padding(4);
            this.cloneSetupGB.Size = new System.Drawing.Size(736, 175);
            this.cloneSetupGB.TabIndex = 1;
            this.cloneSetupGB.TabStop = false;
            this.cloneSetupGB.Text = "Clone";
            // 
            // depthLabel
            // 
            this.depthLabel.AutoSize = true;
            this.depthLabel.Location = new System.Drawing.Point(7, 135);
            this.depthLabel.Name = "depthLabel";
            this.depthLabel.Size = new System.Drawing.Size(63, 13);
            this.depthLabel.TabIndex = 10;
            this.depthLabel.Text = "Limit Depth:";
            // 
            // ProtocolDropdownList
            // 
            this.ProtocolDropdownList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProtocolDropdownList.FormattingEnabled = true;
            this.ProtocolDropdownList.Location = new System.Drawing.Point(470, 32);
            this.ProtocolDropdownList.Name = "ProtocolDropdownList";
            this.ProtocolDropdownList.Size = new System.Drawing.Size(121, 21);
            this.ProtocolDropdownList.TabIndex = 4;
            this.ProtocolDropdownList.SelectedIndexChanged += new System.EventHandler(this.ProtocolSelectionChanged);
            // 
            // ProtocolLabel
            // 
            this.ProtocolLabel.AutoSize = true;
            this.ProtocolLabel.Location = new System.Drawing.Point(418, 36);
            this.ProtocolLabel.Name = "ProtocolLabel";
            this.ProtocolLabel.Size = new System.Drawing.Size(49, 13);
            this.ProtocolLabel.TabIndex = 3;
            this.ProtocolLabel.Text = "Protocol:";
            // 
            // cloneInfoText
            // 
            this.cloneInfoText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cloneInfoText.Location = new System.Drawing.Point(10, 97);
            this.cloneInfoText.Name = "cloneInfoText";
            this.cloneInfoText.Size = new System.Drawing.Size(719, 35);
            this.cloneInfoText.TabIndex = 9;
            // 
            // addUpstreamRemoteAsCB
            // 
            this.addUpstreamRemoteAsCB.Location = new System.Drawing.Point(212, 71);
            this.addUpstreamRemoteAsCB.Name = "addUpstreamRemoteAsCB";
            this.addUpstreamRemoteAsCB.Size = new System.Drawing.Size(200, 21);
            this.addUpstreamRemoteAsCB.TabIndex = 8;
            this.addUpstreamRemoteAsCB.TextChanged += new System.EventHandler(this._addRemoteAsTB_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(211, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Add upstream remote as:";
            // 
            // createDirTB
            // 
            this.createDirTB.Location = new System.Drawing.Point(10, 71);
            this.createDirTB.Name = "createDirTB";
            this.createDirTB.Size = new System.Drawing.Size(183, 20);
            this.createDirTB.TabIndex = 6;
            this.createDirTB.TextChanged += new System.EventHandler(this._createDirTB_TextChanged);
            this.createDirTB.Validating += new System.ComponentModel.CancelEventHandler(this._createDirTB_Validating);
            // 
            // createDirectoryLbl
            // 
            this.createDirectoryLbl.AutoSize = true;
            this.createDirectoryLbl.Location = new System.Drawing.Point(7, 55);
            this.createDirectoryLbl.Name = "createDirectoryLbl";
            this.createDirectoryLbl.Size = new System.Drawing.Size(84, 13);
            this.createDirectoryLbl.TabIndex = 5;
            this.createDirectoryLbl.Text = "Create directory:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Destination folder:";
            // 
            // browseForCloneToDirbtn
            // 
            this.browseForCloneToDirbtn.Location = new System.Drawing.Point(310, 32);
            this.browseForCloneToDirbtn.Name = "browseForCloneToDirbtn";
            this.browseForCloneToDirbtn.Size = new System.Drawing.Size(102, 23);
            this.browseForCloneToDirbtn.TabIndex = 2;
            this.browseForCloneToDirbtn.Text = "Browse...";
            this.browseForCloneToDirbtn.UseVisualStyleBackColor = true;
            this.browseForCloneToDirbtn.Click += new System.EventHandler(this._browseForCloneToDirbtn_Click);
            // 
            // destinationTB
            // 
            this.destinationTB.Location = new System.Drawing.Point(10, 32);
            this.destinationTB.Name = "destinationTB";
            this.destinationTB.Size = new System.Drawing.Size(294, 20);
            this.destinationTB.TabIndex = 1;
            this.destinationTB.TextChanged += new System.EventHandler(this._destinationTB_TextChanged);
            this.destinationTB.Validating += new System.ComponentModel.CancelEventHandler(this._destinationTB_Validating);
            // 
            // depthUpDown
            // 
            this.depthUpDown.Location = new System.Drawing.Point(10, 151);
            this.depthUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.depthUpDown.Name = "depthUpDown";
            this.depthUpDown.Size = new System.Drawing.Size(100, 20);
            this.depthUpDown.TabIndex = 11;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.closeBtn);
            this.flowLayoutPanel1.Controls.Add(this.cloneBtn);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 515);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(738, 34);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // ForkAndCloneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(744, 552);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "ForkAndCloneForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote repository fork and clone";
            this.Load += new System.EventHandler(this.ForkAndCloneForm_Load);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.myReposPage.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.searchReposPage.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.cloneSetupGB.ResumeLayout(false);
            this.cloneSetupGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.depthUpDown)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cloneBtn;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage myReposPage;
        private System.Windows.Forms.Label helpTextLbl;
        private UserControls.NativeListView myReposLV;
        private System.Windows.Forms.ColumnHeader columnHeaderMyReposIsPrivate;
        private System.Windows.Forms.TabPage searchReposPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TextBox searchTB;
        private System.Windows.Forms.Button searchBtn;
        private System.Windows.Forms.Label orLbl;
        private System.Windows.Forms.Button getFromUserBtn;
        private System.Windows.Forms.Button forkBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private UserControls.NativeListView searchResultsLV;
        private System.Windows.Forms.ColumnHeader columnHeaderSearchIsFork;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button openGitupPageBtn;
        private System.Windows.Forms.TextBox searchResultItemDescription;
        private System.Windows.Forms.Label descriptionLbl;
        private System.Windows.Forms.GroupBox cloneSetupGB;
        private System.Windows.Forms.ComboBox addUpstreamRemoteAsCB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox createDirTB;
        private System.Windows.Forms.Label createDirectoryLbl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button browseForCloneToDirbtn;
        private System.Windows.Forms.TextBox destinationTB;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label cloneInfoText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.ComboBox ProtocolDropdownList;
        private System.Windows.Forms.Label ProtocolLabel;
        private System.Windows.Forms.NumericUpDown depthUpDown;
        private System.Windows.Forms.Label depthLabel;
        private System.Windows.Forms.ColumnHeader columnHeaderMyReposName;
        private System.Windows.Forms.ColumnHeader columnHeaderMyReposIsFork;
        private System.Windows.Forms.ColumnHeader columnHeaderMyReposForks;
        private System.Windows.Forms.ColumnHeader columnHeaderSearchName;
        private System.Windows.Forms.ColumnHeader columnHeaderSearchOwner;
        private System.Windows.Forms.ColumnHeader columnHeaderSearchForks;
    }
}
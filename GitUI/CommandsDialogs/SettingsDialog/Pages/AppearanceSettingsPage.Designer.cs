﻿namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class AppearanceSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TableLayoutPanel tlpnlMain;
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.tlpnlGeneral = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowRelativeDate = new System.Windows.Forms.CheckBox();
            this.truncatePathMethod = new System.Windows.Forms.ComboBox();
            this.truncateLongFilenames = new System.Windows.Forms.Label();
            this.chkEnableAutoScale = new System.Windows.Forms.CheckBox();
            this.chkShowRepoCurrentBranch = new System.Windows.Forms.CheckBox();
            this.chkShowCurrentBranchInVisualStudio = new System.Windows.Forms.CheckBox();
            this.gbLanguages = new System.Windows.Forms.GroupBox();
            this.tlpnlLanguage = new System.Windows.Forms.TableLayoutPanel();
            this.Dictionary = new System.Windows.Forms.ComboBox();
            this.downloadDictionary = new System.Windows.Forms.LinkLabel();
            this.lblSpellingDictionary = new System.Windows.Forms.Label();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.Language = new System.Windows.Forms.ComboBox();
            this.helpTranslate = new System.Windows.Forms.LinkLabel();
            this.gbAuthorImages = new System.Windows.Forms.GroupBox();
            this.tlpnlAuthor = new System.Windows.Forms.TableLayoutPanel();
            this.lblAvatarProvider = new System.Windows.Forms.Label();
            this.AvatarProvider = new System.Windows.Forms.ComboBox();
            this.ShowAuthorAvatarInCommitGraph = new System.Windows.Forms.CheckBox();
            this.ShowAuthorAvatarInCommitInfo = new System.Windows.Forms.CheckBox();
            this.ClearImageCache = new System.Windows.Forms.Button();
            this.txtCustomAvatarTemplate = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_NoImageService = new System.Windows.Forms.ComboBox();
            this.lblCacheDays = new System.Windows.Forms.Label();
            this.lblNoImageService = new System.Windows.Forms.Label();
            this.lblCustomAvatarTemplate = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_DaysToCacheImages = new System.Windows.Forms.NumericUpDown();
            this.RevisionSortOrderHelp = new System.Windows.Forms.PictureBox();
            this.pictureAvatarHelp = new System.Windows.Forms.PictureBox();
            this.avatarProviderHelp = new System.Windows.Forms.PictureBox();
            this.fixedWidthFontDialog = new System.Windows.Forms.FontDialog();
            this.applicationDialog = new System.Windows.Forms.FontDialog();
            this.commitFontDialog = new System.Windows.Forms.FontDialog();
            this.lblRevisionsSortBy = new System.Windows.Forms.Label();
            this.lblBranchesSortBy = new System.Windows.Forms.Label();
            this.lblBranchesOrder = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_cmbRevisionsSortBy = new System.Windows.Forms.ComboBox();
            this._NO_TRANSLATE_cmbBranchesSortBy = new System.Windows.Forms.ComboBox();
            this._NO_TRANSLATE_cmbBranchesOrder = new System.Windows.Forms.ComboBox();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.gbGeneral.SuspendLayout();
            this.tlpnlGeneral.SuspendLayout();
            this.gbLanguages.SuspendLayout();
            this.tlpnlLanguage.SuspendLayout();
            this.gbAuthorImages.SuspendLayout();
            this.tlpnlAuthor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionSortOrderHelp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureAvatarHelp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.avatarProviderHelp)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlpnlMain.Controls.Add(this.gbGeneral, 0, 0);
            tlpnlMain.Controls.Add(this.gbLanguages, 0, 2);
            tlpnlMain.Controls.Add(this.gbAuthorImages, 0, 1);
            tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpnlMain.Location = new System.Drawing.Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            tlpnlMain.Size = new System.Drawing.Size(1565, 1339);
            tlpnlMain.TabIndex = 0;
            // 
            // gbGeneral
            // 
            this.gbGeneral.AutoSize = true;
            this.gbGeneral.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbGeneral.Controls.Add(this.tlpnlGeneral);
            this.gbGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbGeneral.Location = new System.Drawing.Point(3, 3);
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.Padding = new System.Windows.Forms.Padding(8);
            this.gbGeneral.Size = new System.Drawing.Size(1559, 227);
            this.gbGeneral.TabIndex = 0;
            this.gbGeneral.TabStop = false;
            this.gbGeneral.Text = "General";
            // 
            // tlpnlGeneral
            // 
            this.tlpnlGeneral.AutoSize = true;
            this.tlpnlGeneral.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlGeneral.ColumnCount = 3;
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlGeneral.Controls.Add(this._NO_TRANSLATE_cmbBranchesOrder, 0, 6);
            this.tlpnlGeneral.Controls.Add(this._NO_TRANSLATE_cmbBranchesSortBy, 0, 5);
            this.tlpnlGeneral.Controls.Add(this._NO_TRANSLATE_cmbRevisionsSortBy, 0, 4);
            this.tlpnlGeneral.Controls.Add(this.RevisionSortOrderHelp, 1, 4);
            this.tlpnlGeneral.Controls.Add(this.lblBranchesOrder, 0, 6);
            this.tlpnlGeneral.Controls.Add(this.lblBranchesSortBy, 0, 5);
            this.tlpnlGeneral.Controls.Add(this.lblRevisionsSortBy, 0, 4);
            this.tlpnlGeneral.Controls.Add(this.chkShowRelativeDate, 0, 0);
            this.tlpnlGeneral.Controls.Add(this.chkShowRepoCurrentBranch, 0, 1);
            this.tlpnlGeneral.Controls.Add(this.chkShowCurrentBranchInVisualStudio, 0, 2);
            this.tlpnlGeneral.Controls.Add(this.chkEnableAutoScale, 0, 3);
            this.tlpnlGeneral.Controls.Add(this.truncateLongFilenames, 0, 7);
            this.tlpnlGeneral.Controls.Add(this.truncatePathMethod, 1, 7);
            this.tlpnlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlGeneral.Location = new System.Drawing.Point(8, 21);
            this.tlpnlGeneral.Name = "tlpnlGeneral";
            this.tlpnlGeneral.RowCount = 8;
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.Size = new System.Drawing.Size(1543, 196);
            this.tlpnlGeneral.TabIndex = 0;
            // 
            // chkShowRelativeDate
            // 
            this.chkShowRelativeDate.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkShowRelativeDate, 2);
            this.chkShowRelativeDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowRelativeDate.Location = new System.Drawing.Point(3, 3);
            this.chkShowRelativeDate.Name = "chkShowRelativeDate";
            this.chkShowRelativeDate.Size = new System.Drawing.Size(448, 17);
            this.chkShowRelativeDate.TabIndex = 0;
            this.chkShowRelativeDate.Text = "Show relative date instead of full date";
            this.chkShowRelativeDate.UseVisualStyleBackColor = true;
            // 
            // chkShowRepoCurrentBranch
            // 
            this.chkShowRepoCurrentBranch.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkShowRepoCurrentBranch, 2);
            this.chkShowRepoCurrentBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowRepoCurrentBranch.Location = new System.Drawing.Point(3, 26);
            this.chkShowRepoCurrentBranch.Name = "chkShowRepoCurrentBranch";
            this.chkShowRepoCurrentBranch.Size = new System.Drawing.Size(448, 17);
            this.chkShowRepoCurrentBranch.TabIndex = 5;
            this.chkShowRepoCurrentBranch.Text = "Show current branch names in the dashboard and the recent repositories dropdown menu";
            this.chkShowRepoCurrentBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentBranchInVisualStudio
            // 
            this.chkShowCurrentBranchInVisualStudio.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkShowCurrentBranchInVisualStudio, 2);
            this.chkShowCurrentBranchInVisualStudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowCurrentBranchInVisualStudio.Location = new System.Drawing.Point(3, 49);
            this.chkShowCurrentBranchInVisualStudio.Name = "chkShowCurrentBranchInVisualStudio";
            this.chkShowCurrentBranchInVisualStudio.Size = new System.Drawing.Size(448, 17);
            this.chkShowCurrentBranchInVisualStudio.TabIndex = 1;
            this.chkShowCurrentBranchInVisualStudio.Text = "Show current branch in Visual Studio";
            this.chkShowCurrentBranchInVisualStudio.UseVisualStyleBackColor = true;
            // 
            // chkEnableAutoScale
            // 
            this.chkEnableAutoScale.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkEnableAutoScale, 2);
            this.chkEnableAutoScale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkEnableAutoScale.Location = new System.Drawing.Point(3, 72);
            this.chkEnableAutoScale.Name = "chkEnableAutoScale";
            this.chkEnableAutoScale.Size = new System.Drawing.Size(448, 17);
            this.chkEnableAutoScale.TabIndex = 2;
            this.chkEnableAutoScale.Text = "Auto scale user interface when high DPI is used";
            this.chkEnableAutoScale.UseVisualStyleBackColor = true;
            // 
            // truncateLongFilenames
            // 
            this.truncateLongFilenames.AutoSize = true;
            this.truncateLongFilenames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.truncateLongFilenames.Location = new System.Drawing.Point(3, 169);
            this.truncateLongFilenames.Name = "truncateLongFilenames";
            this.truncateLongFilenames.Size = new System.Drawing.Size(120, 27);
            this.truncateLongFilenames.TabIndex = 4;
            this.truncateLongFilenames.Text = "Truncate long filenames";
            this.truncateLongFilenames.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // truncatePathMethod
            // 
            this.truncatePathMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.truncatePathMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.truncatePathMethod.FormattingEnabled = true;
            this.truncatePathMethod.Items.AddRange(new object[] {
            "None",
            "Compact",
            "Trim start",
            "Filename only"});
            this.truncatePathMethod.Location = new System.Drawing.Point(129, 172);
            this.truncatePathMethod.Name = "truncatePathMethod";
            this.truncatePathMethod.Size = new System.Drawing.Size(322, 21);
            this.truncatePathMethod.TabIndex = 4;
            // 
            // gbAuthorImages
            // 
            this.gbAuthorImages.AutoSize = true;
            this.gbAuthorImages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbAuthorImages.Controls.Add(this.tlpnlAuthor);
            this.gbAuthorImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAuthorImages.Location = new System.Drawing.Point(3, 234);
            this.gbAuthorImages.Name = "gbAuthorImages";
            this.gbAuthorImages.Padding = new System.Windows.Forms.Padding(8);
            this.gbAuthorImages.Size = new System.Drawing.Size(1559, 184);
            this.gbAuthorImages.TabIndex = 1;
            this.gbAuthorImages.TabStop = false;
            this.gbAuthorImages.Text = "Author images";
            //
            // txtCustomAvatarTemplate
            //
            this.txtCustomAvatarTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCustomAvatarTemplate.Text = "";
            this.txtCustomAvatarTemplate.Name = "txtCustomAvatarTemplate";
            // 
            // tlpnlAuthor
            // 
            this.tlpnlAuthor.AutoSize = true;
            this.tlpnlAuthor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlAuthor.ColumnCount = 3;
            this.tlpnlAuthor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlAuthor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlAuthor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlAuthor.Controls.Add(this.ShowAuthorAvatarInCommitGraph, 0, 0);
            this.tlpnlAuthor.Controls.Add(this.ShowAuthorAvatarInCommitInfo, 0, 1);
            this.tlpnlAuthor.Controls.Add(this.lblCacheDays, 0, 2);
            this.tlpnlAuthor.Controls.Add(this._NO_TRANSLATE_DaysToCacheImages, 1, 2);
            this.tlpnlAuthor.Controls.Add(this.lblAvatarProvider, 0, 3);
            this.tlpnlAuthor.Controls.Add(this.AvatarProvider, 1, 3);
            this.tlpnlAuthor.Controls.Add(this.avatarProviderHelp, 2, 3);
            this.tlpnlAuthor.Controls.Add(this.lblNoImageService, 0, 4);
            this.tlpnlAuthor.Controls.Add(this.pictureAvatarHelp, 2, 4);
            this.tlpnlAuthor.Controls.Add(this._NO_TRANSLATE_NoImageService, 1, 4);
            this.tlpnlAuthor.Controls.Add(this.lblCustomAvatarTemplate, 0, 5);
            this.tlpnlAuthor.Controls.Add(this.txtCustomAvatarTemplate, 1, 5);
            this.tlpnlAuthor.Controls.Add(this.ClearImageCache, 1, 6);
            this.tlpnlAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlAuthor.Location = new System.Drawing.Point(8, 21);
            this.tlpnlAuthor.Name = "tlpnlAuthor";
            this.tlpnlAuthor.RowCount = 6;
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.Size = new System.Drawing.Size(1543, 155);
            this.tlpnlAuthor.TabIndex = 0;
            // 
            // lblAvatarProvider
            // 
            this.lblAvatarProvider.AutoSize = true;
            this.lblAvatarProvider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAvatarProvider.Location = new System.Drawing.Point(3, 72);
            this.lblAvatarProvider.Name = "lblAvatarProvider";
            this.lblAvatarProvider.Size = new System.Drawing.Size(155, 27);
            this.lblAvatarProvider.TabIndex = 11;
            this.lblAvatarProvider.Text = "Avatar provider";
            this.lblAvatarProvider.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AvatarProvider
            // 
            this.AvatarProvider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AvatarProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AvatarProvider.FormattingEnabled = true;
            this.AvatarProvider.Location = new System.Drawing.Point(164, 75);
            this.AvatarProvider.Name = "AvatarProvider";
            this.AvatarProvider.Size = new System.Drawing.Size(183, 21);
            this.AvatarProvider.TabIndex = 3;
            this.AvatarProvider.SelectedIndexChanged += new System.EventHandler(this.AvatarProvider_SelectedIndexChanged);
            // 
            // ShowAuthorAvatarInCommitGraph
            // 
            this.ShowAuthorAvatarInCommitGraph.AutoSize = true;
            this.tlpnlAuthor.SetColumnSpan(this.ShowAuthorAvatarInCommitGraph, 2);
            this.ShowAuthorAvatarInCommitGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowAuthorAvatarInCommitGraph.Location = new System.Drawing.Point(3, 3);
            this.ShowAuthorAvatarInCommitGraph.Name = "ShowAuthorAvatarInCommitGraph";
            this.ShowAuthorAvatarInCommitGraph.Size = new System.Drawing.Size(344, 17);
            this.ShowAuthorAvatarInCommitGraph.TabIndex = 0;
            this.ShowAuthorAvatarInCommitGraph.Text = "Show author\'s avatar column in the commit graph";
            this.ShowAuthorAvatarInCommitGraph.UseVisualStyleBackColor = true;
            // 
            // ShowAuthorAvatarInCommitInfo
            // 
            this.ShowAuthorAvatarInCommitInfo.AutoSize = true;
            this.tlpnlAuthor.SetColumnSpan(this.ShowAuthorAvatarInCommitInfo, 2);
            this.ShowAuthorAvatarInCommitInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowAuthorAvatarInCommitInfo.Location = new System.Drawing.Point(3, 26);
            this.ShowAuthorAvatarInCommitInfo.Name = "ShowAuthorAvatarInCommitInfo";
            this.ShowAuthorAvatarInCommitInfo.Size = new System.Drawing.Size(344, 17);
            this.ShowAuthorAvatarInCommitInfo.TabIndex = 1;
            this.ShowAuthorAvatarInCommitInfo.Text = "Show author\'s avatar in the commit info view";
            this.ShowAuthorAvatarInCommitInfo.UseVisualStyleBackColor = true;
            // 
            // ClearImageCache
            // 
            this.ClearImageCache.AutoSize = true;
            this.ClearImageCache.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClearImageCache.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClearImageCache.Location = new System.Drawing.Point(164, 129);
            this.ClearImageCache.Name = "ClearImageCache";
            this.ClearImageCache.Size = new System.Drawing.Size(183, 23);
            this.ClearImageCache.TabIndex = 6;
            this.ClearImageCache.Text = "Clear image cache";
            this.ClearImageCache.UseVisualStyleBackColor = true;
            this.ClearImageCache.Click += new System.EventHandler(this.ClearImageCache_Click);
            // 
            // gbLanguages
            // 
            this.gbLanguages.AutoSize = true;
            this.gbLanguages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbLanguages.Controls.Add(this.tlpnlLanguage);
            this.gbLanguages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLanguages.Location = new System.Drawing.Point(3, 424);
            this.gbLanguages.Name = "gbLanguages";
            this.gbLanguages.Padding = new System.Windows.Forms.Padding(8);
            this.gbLanguages.Size = new System.Drawing.Size(1559, 83);
            this.gbLanguages.TabIndex = 2;
            this.gbLanguages.TabStop = false;
            this.gbLanguages.Text = "Language";
            // 
            // tlpnlLanguage
            // 
            this.tlpnlLanguage.AutoSize = true;
            this.tlpnlLanguage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlLanguage.ColumnCount = 4;
            this.tlpnlLanguage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlLanguage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlLanguage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlLanguage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlLanguage.Controls.Add(this.Dictionary, 1, 1);
            this.tlpnlLanguage.Controls.Add(this.downloadDictionary, 2, 1);
            this.tlpnlLanguage.Controls.Add(this.lblSpellingDictionary, 0, 1);
            this.tlpnlLanguage.Controls.Add(this.lblLanguage, 0, 0);
            this.tlpnlLanguage.Controls.Add(this.Language, 1, 0);
            this.tlpnlLanguage.Controls.Add(this.helpTranslate, 2, 0);
            this.tlpnlLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlLanguage.Location = new System.Drawing.Point(8, 21);
            this.tlpnlLanguage.Name = "tlpnlLanguage";
            this.tlpnlLanguage.RowCount = 2;
            this.tlpnlLanguage.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlLanguage.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlLanguage.Size = new System.Drawing.Size(1543, 54);
            this.tlpnlLanguage.TabIndex = 0;
            // 
            // Dictionary
            // 
            this.Dictionary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Dictionary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Dictionary.FormattingEnabled = true;
            this.Dictionary.Location = new System.Drawing.Point(158, 30);
            this.Dictionary.Name = "Dictionary";
            this.Dictionary.Size = new System.Drawing.Size(86, 21);
            this.Dictionary.TabIndex = 3;
            this.Dictionary.DropDown += new System.EventHandler(this.Dictionary_DropDown);
            // 
            // downloadDictionary
            // 
            this.downloadDictionary.AutoSize = true;
            this.downloadDictionary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadDictionary.Location = new System.Drawing.Point(250, 27);
            this.downloadDictionary.Name = "downloadDictionary";
            this.downloadDictionary.Size = new System.Drawing.Size(103, 27);
            this.downloadDictionary.TabIndex = 4;
            this.downloadDictionary.TabStop = true;
            this.downloadDictionary.Text = "Download dictionary";
            this.downloadDictionary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.downloadDictionary.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadDictionary_LinkClicked);
            // 
            // lblSpellingDictionary
            // 
            this.lblSpellingDictionary.AutoSize = true;
            this.lblSpellingDictionary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpellingDictionary.Location = new System.Drawing.Point(3, 27);
            this.lblSpellingDictionary.Name = "lblSpellingDictionary";
            this.lblSpellingDictionary.Size = new System.Drawing.Size(149, 27);
            this.lblSpellingDictionary.TabIndex = 3;
            this.lblSpellingDictionary.Text = "Dictionary for spelling checker";
            this.lblSpellingDictionary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLanguage.Location = new System.Drawing.Point(3, 0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(149, 27);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Text = "Language (restart required)";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Language
            // 
            this.Language.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Language.FormattingEnabled = true;
            this.Language.Location = new System.Drawing.Point(158, 3);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(86, 21);
            this.Language.TabIndex = 0;
            // 
            // helpTranslate
            // 
            this.helpTranslate.AutoSize = true;
            this.helpTranslate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpTranslate.Location = new System.Drawing.Point(250, 0);
            this.helpTranslate.Name = "helpTranslate";
            this.helpTranslate.Size = new System.Drawing.Size(103, 27);
            this.helpTranslate.TabIndex = 1;
            this.helpTranslate.TabStop = true;
            this.helpTranslate.Text = "Help translate";
            this.helpTranslate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.helpTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpTranslate_LinkClicked);
            // 
            // _NO_TRANSLATE_NoImageService
            // 
            this._NO_TRANSLATE_NoImageService.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_NoImageService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_NoImageService.FormattingEnabled = true;
            this._NO_TRANSLATE_NoImageService.Location = new System.Drawing.Point(164, 102);
            this._NO_TRANSLATE_NoImageService.Name = "_NO_TRANSLATE_NoImageService";
            this._NO_TRANSLATE_NoImageService.Size = new System.Drawing.Size(183, 21);
            this._NO_TRANSLATE_NoImageService.TabIndex = 4;
            // 
            // lblCacheDays
            // 
            this.lblCacheDays.AutoSize = true;
            this.lblCacheDays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCacheDays.Location = new System.Drawing.Point(3, 46);
            this.lblCacheDays.Name = "lblCacheDays";
            this.lblCacheDays.Size = new System.Drawing.Size(155, 26);
            this.lblCacheDays.TabIndex = 2;
            this.lblCacheDays.Text = "Cache images (days)";
            this.lblCacheDays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblCustomAvatarTemplate
            //
            this.lblCustomAvatarTemplate.AutoSize = true;
            this.lblCustomAvatarTemplate.Name = "lblCustomAvatarTemplate";
            this.lblCustomAvatarTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCustomAvatarTemplate.TabIndex = 4;
            this.lblCustomAvatarTemplate.Text = "Custom avatar template";
            this.lblCustomAvatarTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNoImageService
            // 
            this.lblNoImageService.AutoSize = true;
            this.lblNoImageService.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoImageService.Location = new System.Drawing.Point(3, 99);
            this.lblNoImageService.Name = "lblNoImageService";
            this.lblNoImageService.Size = new System.Drawing.Size(155, 27);
            this.lblNoImageService.TabIndex = 4;
            this.lblNoImageService.Text = "Fallback generated avatar style";
            this.lblNoImageService.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_DaysToCacheImages
            // 
            this._NO_TRANSLATE_DaysToCacheImages.Location = new System.Drawing.Point(164, 49);
            this._NO_TRANSLATE_DaysToCacheImages.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._NO_TRANSLATE_DaysToCacheImages.Name = "_NO_TRANSLATE_DaysToCacheImages";
            this._NO_TRANSLATE_DaysToCacheImages.Size = new System.Drawing.Size(38, 20);
            this._NO_TRANSLATE_DaysToCacheImages.TabIndex = 2;
            this._NO_TRANSLATE_DaysToCacheImages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // RevisionSortOrderHelp
            // 
            this.RevisionSortOrderHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RevisionSortOrderHelp.Image = global::GitUI.Properties.Resources.information;
            this.RevisionSortOrderHelp.Location = new System.Drawing.Point(353, 104);
            this.RevisionSortOrderHelp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.RevisionSortOrderHelp.Name = "RevisionSortOrderHelp";
            this.RevisionSortOrderHelp.Size = new System.Drawing.Size(16, 16);
            this.RevisionSortOrderHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.RevisionSortOrderHelp.TabStop = false;
            // 
            // customAvatarHelp
            // 
            this.avatarProviderHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.avatarProviderHelp.Image = global::GitUI.Properties.Resources.information;
            this.avatarProviderHelp.Location = new System.Drawing.Point(353, 104);
            this.avatarProviderHelp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.avatarProviderHelp.Name = "customAvatarHelp";
            this.avatarProviderHelp.Size = new System.Drawing.Size(16, 16);
            this.avatarProviderHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.avatarProviderHelp.TabStop = false;
            this.avatarProviderHelp.Click += new System.EventHandler(this.customAvatarHelp_Click);
            // 
            // pictureAvatarHelp
            // 
            this.pictureAvatarHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureAvatarHelp.Image = global::GitUI.Properties.Resources.information;
            this.pictureAvatarHelp.Location = new System.Drawing.Point(353, 104);
            this.pictureAvatarHelp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.pictureAvatarHelp.Name = "pictureAvatarHelp";
            this.pictureAvatarHelp.Size = new System.Drawing.Size(16, 16);
            this.pictureAvatarHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureAvatarHelp.TabIndex = 3;
            this.pictureAvatarHelp.TabStop = false;
            this.pictureAvatarHelp.Click += new System.EventHandler(this.pictureAvatarHelp_Click);
            // 
            // fixedWidthFontDialog
            // 
            this.fixedWidthFontDialog.AllowVerticalFonts = false;
            this.fixedWidthFontDialog.Color = System.Drawing.SystemColors.ControlText;
            this.fixedWidthFontDialog.FixedPitchOnly = true;
            // 
            // applicationDialog
            // 
            this.applicationDialog.AllowVerticalFonts = false;
            this.applicationDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // commitFontDialog
            // 
            this.commitFontDialog.AllowVerticalFonts = false;
            this.commitFontDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // lblRevisionsSortBy
            // 
            this.lblRevisionsSortBy.AutoSize = true;
            this.lblRevisionsSortBy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRevisionsSortBy.Location = new System.Drawing.Point(3, 115);
            this.lblRevisionsSortBy.Name = "lblRevisionsSortBy";
            this.lblRevisionsSortBy.Size = new System.Drawing.Size(120, 27);
            this.lblRevisionsSortBy.TabIndex = 3;
            this.lblRevisionsSortBy.Text = "Sort revisions by";
            this.lblRevisionsSortBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBranchesSortBy
            // 
            this.lblBranchesSortBy.AutoSize = true;
            this.lblBranchesSortBy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBranchesSortBy.Location = new System.Drawing.Point(3, 115);
            this.lblBranchesSortBy.Name = "lblBranchesSortBy";
            this.lblBranchesSortBy.Size = new System.Drawing.Size(120, 27);
            this.lblBranchesSortBy.TabIndex = 6;
            this.lblBranchesSortBy.Text = "Sort branches by";
            this.lblBranchesSortBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBranchesOrder
            // 
            this.lblBranchesOrder.AutoSize = true;
            this.lblBranchesOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBranchesOrder.Location = new System.Drawing.Point(3, 142);
            this.lblBranchesOrder.Name = "lblBranchesOrder";
            this.lblBranchesOrder.Size = new System.Drawing.Size(120, 27);
            this.lblBranchesOrder.TabIndex = 7;
            this.lblBranchesOrder.Text = "Order branches";
            this.lblBranchesOrder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_cmbRevisionsSortBy
            // 
            this._NO_TRANSLATE_cmbRevisionsSortBy.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_cmbRevisionsSortBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_cmbRevisionsSortBy.FormattingEnabled = true;
            this._NO_TRANSLATE_cmbRevisionsSortBy.Items.AddRange(new object[] {
            "Git Default",
            "Author Date",
            "Topo (Ancestor)"});
            this._NO_TRANSLATE_cmbRevisionsSortBy.Location = new System.Drawing.Point(129, 118);
            this._NO_TRANSLATE_cmbRevisionsSortBy.Name = "_NO_TRANSLATE_cmbRevisionsSortBy";
            this._NO_TRANSLATE_cmbRevisionsSortBy.Size = new System.Drawing.Size(322, 21);
            this._NO_TRANSLATE_cmbRevisionsSortBy.TabIndex = 3;
            // 
            // _NO_TRANSLATE_cmbBranchesSortBy
            // 
            this._NO_TRANSLATE_cmbBranchesSortBy.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_cmbBranchesSortBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_cmbBranchesSortBy.FormattingEnabled = true;
            this._NO_TRANSLATE_cmbBranchesSortBy.Items.AddRange(new object[] {
            "None",
            "Compact",
            "Trim start",
            "Filename only"});
            this._NO_TRANSLATE_cmbBranchesSortBy.Location = new System.Drawing.Point(129, 118);
            this._NO_TRANSLATE_cmbBranchesSortBy.Name = "_NO_TRANSLATE_cmbBranchesSortBy";
            this._NO_TRANSLATE_cmbBranchesSortBy.Size = new System.Drawing.Size(322, 21);
            this._NO_TRANSLATE_cmbBranchesSortBy.TabIndex = 8;
            // 
            // _NO_TRANSLATE_cmbBranchesOrder
            // 
            this._NO_TRANSLATE_cmbBranchesOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_cmbBranchesOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_cmbBranchesOrder.FormattingEnabled = true;
            this._NO_TRANSLATE_cmbBranchesOrder.Items.AddRange(new object[] {
            "None",
            "Compact",
            "Trim start",
            "Filename only"});
            this._NO_TRANSLATE_cmbBranchesOrder.Location = new System.Drawing.Point(129, 145);
            this._NO_TRANSLATE_cmbBranchesOrder.Name = "_NO_TRANSLATE_cmbBranchesOrder";
            this._NO_TRANSLATE_cmbBranchesOrder.Size = new System.Drawing.Size(322, 21);
            this._NO_TRANSLATE_cmbBranchesOrder.TabIndex = 9;
            // 
            // AppearanceSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tlpnlMain);
            this.MinimumSize = new System.Drawing.Size(258, 255);
            this.Name = "AppearanceSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1581, 1355);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.tlpnlGeneral.ResumeLayout(false);
            this.tlpnlGeneral.PerformLayout();
            this.gbLanguages.ResumeLayout(false);
            this.gbLanguages.PerformLayout();
            this.tlpnlLanguage.ResumeLayout(false);
            this.tlpnlLanguage.PerformLayout();
            this.gbAuthorImages.ResumeLayout(false);
            this.gbAuthorImages.PerformLayout();
            this.tlpnlAuthor.ResumeLayout(false);
            this.tlpnlAuthor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RevisionSortOrderHelp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureAvatarHelp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.avatarProviderHelp)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLanguages;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblSpellingDictionary;
        private System.Windows.Forms.ComboBox Dictionary;
        private System.Windows.Forms.LinkLabel downloadDictionary;
        private System.Windows.Forms.ComboBox Language;
        private System.Windows.Forms.LinkLabel helpTranslate;
        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.CheckBox chkShowRelativeDate;
        private System.Windows.Forms.CheckBox chkShowRepoCurrentBranch;
        private System.Windows.Forms.CheckBox chkShowCurrentBranchInVisualStudio;
        private System.Windows.Forms.CheckBox chkEnableAutoScale;
        private System.Windows.Forms.Label truncateLongFilenames;
        private System.Windows.Forms.ComboBox truncatePathMethod;
        private System.Windows.Forms.GroupBox gbAuthorImages;
        private System.Windows.Forms.TextBox txtCustomAvatarTemplate;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_NoImageService;
        private System.Windows.Forms.Label lblNoImageService;
        private System.Windows.Forms.Label lblCustomAvatarTemplate;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_DaysToCacheImages;
        private System.Windows.Forms.Label lblCacheDays;
        private System.Windows.Forms.Button ClearImageCache;
        private System.Windows.Forms.CheckBox ShowAuthorAvatarInCommitInfo;
        private System.Windows.Forms.FontDialog fixedWidthFontDialog;
        private System.Windows.Forms.FontDialog applicationDialog;
        private System.Windows.Forms.FontDialog commitFontDialog;
        private System.Windows.Forms.TableLayoutPanel tlpnlLanguage;
        private System.Windows.Forms.TableLayoutPanel tlpnlGeneral;
        private System.Windows.Forms.TableLayoutPanel tlpnlAuthor;
        private System.Windows.Forms.CheckBox ShowAuthorAvatarInCommitGraph;
        private System.Windows.Forms.PictureBox RevisionSortOrderHelp;
        private System.Windows.Forms.PictureBox pictureAvatarHelp;
        private System.Windows.Forms.PictureBox avatarProviderHelp;
        private System.Windows.Forms.Label lblAvatarProvider;
        private System.Windows.Forms.ComboBox AvatarProvider;
        private System.Windows.Forms.Label lblRevisionsSortBy;
        private System.Windows.Forms.Label lblBranchesSortBy;
        private System.Windows.Forms.Label lblBranchesOrder;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cmbRevisionsSortBy;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cmbBranchesSortBy;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cmbBranchesOrder;
    }
}

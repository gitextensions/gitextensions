namespace GitUI.CommandsDialogs.SettingsDialog.Pages
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
            TableLayoutPanel tlpnlMain;
            gbGeneral = new GroupBox();
            tlpnlGeneral = new TableLayoutPanel();
            chkShowRelativeDate = new CheckBox();
            chkShowRepoCurrentBranch = new CheckBox();
            chkShowCurrentBranchInVisualStudio = new CheckBox();
            chkEnableAutoScale = new CheckBox();
            truncateLongFilenames = new Label();
            truncatePathMethod = new ComboBox();
            gbLanguages = new GroupBox();
            tlpnlLanguage = new TableLayoutPanel();
            Dictionary = new ComboBox();
            downloadDictionary = new LinkLabel();
            lblSpellingDictionary = new Label();
            lblLanguage = new Label();
            Language = new ComboBox();
            helpTranslate = new LinkLabel();
            gbAuthorImages = new GroupBox();
            tlpnlAuthor = new TableLayoutPanel();
            ShowAuthorAvatarInCommitGraph = new CheckBox();
            ShowAuthorAvatarInCommitInfo = new CheckBox();
            lblCacheDays = new Label();
            _NO_TRANSLATE_DaysToCacheImages = new NumericUpDown();
            lblAvatarProvider = new Label();
            AvatarProvider = new ComboBox();
            avatarProviderHelp = new PictureBox();
            lblNoImageService = new Label();
            pictureAvatarHelp = new PictureBox();
            _NO_TRANSLATE_NoImageService = new ComboBox();
            lblCustomAvatarTemplate = new Label();
            txtCustomAvatarTemplate = new TextBox();
            ClearImageCache = new Button();
            fixedWidthFontDialog = new FontDialog();
            applicationDialog = new FontDialog();
            commitFontDialog = new FontDialog();
            tlpnlMain = new TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            gbGeneral.SuspendLayout();
            tlpnlGeneral.SuspendLayout();
            gbLanguages.SuspendLayout();
            tlpnlLanguage.SuspendLayout();
            gbAuthorImages.SuspendLayout();
            tlpnlAuthor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_DaysToCacheImages).BeginInit();
            ((System.ComponentModel.ISupportInitialize)avatarProviderHelp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureAvatarHelp).BeginInit();
            SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(gbGeneral, 0, 0);
            tlpnlMain.Controls.Add(gbLanguages, 0, 2);
            tlpnlMain.Controls.Add(gbAuthorImages, 0, 1);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.Size = new Size(1486, 583);
            tlpnlMain.TabIndex = 0;
            // 
            // gbGeneral
            // 
            gbGeneral.AutoSize = true;
            gbGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbGeneral.Controls.Add(tlpnlGeneral);
            gbGeneral.Dock = DockStyle.Fill;
            gbGeneral.Location = new Point(3, 3);
            gbGeneral.Name = "gbGeneral";
            gbGeneral.Padding = new Padding(8);
            gbGeneral.Size = new Size(1480, 161);
            gbGeneral.TabIndex = 0;
            gbGeneral.TabStop = false;
            gbGeneral.Text = "&General";
            // 
            // tlpnlGeneral
            // 
            tlpnlGeneral.AutoSize = true;
            tlpnlGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlGeneral.ColumnCount = 3;
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlGeneral.Controls.Add(chkShowRelativeDate, 0, 0);
            tlpnlGeneral.Controls.Add(chkShowRepoCurrentBranch, 0, 1);
            tlpnlGeneral.Controls.Add(chkShowCurrentBranchInVisualStudio, 0, 2);
            tlpnlGeneral.Controls.Add(chkEnableAutoScale, 0, 3);
            tlpnlGeneral.Controls.Add(truncateLongFilenames, 0, 4);
            tlpnlGeneral.Controls.Add(truncatePathMethod, 1, 4);
            tlpnlGeneral.Dock = DockStyle.Fill;
            tlpnlGeneral.Location = new Point(8, 24);
            tlpnlGeneral.Name = "tlpnlGeneral";
            tlpnlGeneral.RowCount = 5;
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.Size = new Size(1464, 129);
            tlpnlGeneral.TabIndex = 0;
            // 
            // chkShowRelativeDate
            // 
            chkShowRelativeDate.AutoSize = true;
            tlpnlGeneral.SetColumnSpan(chkShowRelativeDate, 2);
            chkShowRelativeDate.Dock = DockStyle.Fill;
            chkShowRelativeDate.Location = new Point(3, 3);
            chkShowRelativeDate.Name = "chkShowRelativeDate";
            chkShowRelativeDate.Size = new Size(501, 19);
            chkShowRelativeDate.TabIndex = 0;
            chkShowRelativeDate.Text = "Show relative date instead of full date";
            chkShowRelativeDate.UseVisualStyleBackColor = true;
            // 
            // chkShowRepoCurrentBranch
            // 
            chkShowRepoCurrentBranch.AutoSize = true;
            tlpnlGeneral.SetColumnSpan(chkShowRepoCurrentBranch, 2);
            chkShowRepoCurrentBranch.Dock = DockStyle.Fill;
            chkShowRepoCurrentBranch.Location = new Point(3, 28);
            chkShowRepoCurrentBranch.Name = "chkShowRepoCurrentBranch";
            chkShowRepoCurrentBranch.Size = new Size(501, 19);
            chkShowRepoCurrentBranch.TabIndex = 1;
            chkShowRepoCurrentBranch.Text = "Show current branch names in the dashboard and the recent repositories dropdown menu";
            chkShowRepoCurrentBranch.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentBranchInVisualStudio
            // 
            chkShowCurrentBranchInVisualStudio.AutoSize = true;
            tlpnlGeneral.SetColumnSpan(chkShowCurrentBranchInVisualStudio, 2);
            chkShowCurrentBranchInVisualStudio.Dock = DockStyle.Fill;
            chkShowCurrentBranchInVisualStudio.Location = new Point(3, 53);
            chkShowCurrentBranchInVisualStudio.Name = "chkShowCurrentBranchInVisualStudio";
            chkShowCurrentBranchInVisualStudio.Size = new Size(501, 19);
            chkShowCurrentBranchInVisualStudio.TabIndex = 2;
            chkShowCurrentBranchInVisualStudio.Text = "Show current branch in Visual Studio";
            chkShowCurrentBranchInVisualStudio.UseVisualStyleBackColor = true;
            // 
            // chkEnableAutoScale
            // 
            chkEnableAutoScale.AutoSize = true;
            tlpnlGeneral.SetColumnSpan(chkEnableAutoScale, 2);
            chkEnableAutoScale.Dock = DockStyle.Fill;
            chkEnableAutoScale.Location = new Point(3, 78);
            chkEnableAutoScale.Name = "chkEnableAutoScale";
            chkEnableAutoScale.Size = new Size(501, 19);
            chkEnableAutoScale.TabIndex = 3;
            chkEnableAutoScale.Text = "Auto scale user interface when high DPI is used";
            chkEnableAutoScale.UseVisualStyleBackColor = true;
            // 
            // truncateLongFilenames
            // 
            truncateLongFilenames.AutoSize = true;
            truncateLongFilenames.Dock = DockStyle.Fill;
            truncateLongFilenames.Location = new Point(3, 100);
            truncateLongFilenames.Name = "truncateLongFilenames";
            truncateLongFilenames.Size = new Size(133, 29);
            truncateLongFilenames.TabIndex = 4;
            truncateLongFilenames.Text = "Truncate long filenames";
            truncateLongFilenames.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // truncatePathMethod
            // 
            truncatePathMethod.Dock = DockStyle.Fill;
            truncatePathMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            truncatePathMethod.FormattingEnabled = true;
            truncatePathMethod.Items.AddRange(new object[] { "None", "Compact", "Trim start", "Filename only" });
            truncatePathMethod.Location = new Point(142, 103);
            truncatePathMethod.Name = "truncatePathMethod";
            truncatePathMethod.Size = new Size(362, 23);
            truncatePathMethod.TabIndex = 4;
            // 
            // gbLanguages
            // 
            gbLanguages.AutoSize = true;
            gbLanguages.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbLanguages.Controls.Add(tlpnlLanguage);
            gbLanguages.Dock = DockStyle.Fill;
            gbLanguages.Location = new Point(3, 405);
            gbLanguages.Name = "gbLanguages";
            gbLanguages.Padding = new Padding(8);
            gbLanguages.Size = new Size(1480, 90);
            gbLanguages.TabIndex = 2;
            gbLanguages.TabStop = false;
            gbLanguages.Text = "&Language";
            // 
            // tlpnlLanguage
            // 
            tlpnlLanguage.AutoSize = true;
            tlpnlLanguage.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlLanguage.ColumnCount = 4;
            tlpnlLanguage.ColumnStyles.Add(new ColumnStyle());
            tlpnlLanguage.ColumnStyles.Add(new ColumnStyle());
            tlpnlLanguage.ColumnStyles.Add(new ColumnStyle());
            tlpnlLanguage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlLanguage.Controls.Add(Dictionary, 1, 1);
            tlpnlLanguage.Controls.Add(downloadDictionary, 2, 1);
            tlpnlLanguage.Controls.Add(lblSpellingDictionary, 0, 1);
            tlpnlLanguage.Controls.Add(lblLanguage, 0, 0);
            tlpnlLanguage.Controls.Add(Language, 1, 0);
            tlpnlLanguage.Controls.Add(helpTranslate, 2, 0);
            tlpnlLanguage.Dock = DockStyle.Fill;
            tlpnlLanguage.Location = new Point(8, 24);
            tlpnlLanguage.Name = "tlpnlLanguage";
            tlpnlLanguage.RowCount = 2;
            tlpnlLanguage.RowStyles.Add(new RowStyle());
            tlpnlLanguage.RowStyles.Add(new RowStyle());
            tlpnlLanguage.Size = new Size(1464, 58);
            tlpnlLanguage.TabIndex = 0;
            // 
            // Dictionary
            // 
            Dictionary.Dock = DockStyle.Fill;
            Dictionary.DropDownStyle = ComboBoxStyle.DropDownList;
            Dictionary.FormattingEnabled = true;
            Dictionary.Location = new Point(176, 32);
            Dictionary.Name = "Dictionary";
            Dictionary.Size = new Size(86, 23);
            Dictionary.TabIndex = 2;
            Dictionary.DropDown += Dictionary_DropDown;
            // 
            // downloadDictionary
            // 
            downloadDictionary.AutoSize = true;
            downloadDictionary.Dock = DockStyle.Fill;
            downloadDictionary.Location = new Point(268, 29);
            downloadDictionary.Name = "downloadDictionary";
            downloadDictionary.Size = new Size(117, 29);
            downloadDictionary.TabIndex = 3;
            downloadDictionary.TabStop = true;
            downloadDictionary.Text = "Download dictionary";
            downloadDictionary.TextAlign = ContentAlignment.MiddleLeft;
            downloadDictionary.LinkClicked += downloadDictionary_LinkClicked;
            // 
            // lblSpellingDictionary
            // 
            lblSpellingDictionary.AutoSize = true;
            lblSpellingDictionary.Dock = DockStyle.Fill;
            lblSpellingDictionary.Location = new Point(3, 29);
            lblSpellingDictionary.Name = "lblSpellingDictionary";
            lblSpellingDictionary.Size = new Size(167, 29);
            lblSpellingDictionary.TabIndex = 2;
            lblSpellingDictionary.Text = "Dictionary for spelling checker";
            lblSpellingDictionary.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLanguage
            // 
            lblLanguage.AutoSize = true;
            lblLanguage.Dock = DockStyle.Fill;
            lblLanguage.Location = new Point(3, 0);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new Size(167, 29);
            lblLanguage.TabIndex = 0;
            lblLanguage.Text = "Language (restart required)";
            lblLanguage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Language
            // 
            Language.Dock = DockStyle.Fill;
            Language.DropDownStyle = ComboBoxStyle.DropDownList;
            Language.FormattingEnabled = true;
            Language.Location = new Point(176, 3);
            Language.Name = "Language";
            Language.Size = new Size(86, 23);
            Language.TabIndex = 0;
            // 
            // helpTranslate
            // 
            helpTranslate.AutoSize = true;
            helpTranslate.Dock = DockStyle.Fill;
            helpTranslate.Location = new Point(268, 0);
            helpTranslate.Name = "helpTranslate";
            helpTranslate.Size = new Size(117, 29);
            helpTranslate.TabIndex = 1;
            helpTranslate.TabStop = true;
            helpTranslate.Text = "Help translate";
            helpTranslate.TextAlign = ContentAlignment.MiddleLeft;
            helpTranslate.LinkClicked += helpTranslate_LinkClicked;
            // 
            // gbAuthorImages
            // 
            gbAuthorImages.AutoSize = true;
            gbAuthorImages.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbAuthorImages.Controls.Add(tlpnlAuthor);
            gbAuthorImages.Dock = DockStyle.Fill;
            gbAuthorImages.Location = new Point(3, 170);
            gbAuthorImages.Name = "gbAuthorImages";
            gbAuthorImages.Padding = new Padding(8);
            gbAuthorImages.Size = new Size(1480, 229);
            gbAuthorImages.TabIndex = 1;
            gbAuthorImages.TabStop = false;
            gbAuthorImages.Text = "&Author images";
            // 
            // tlpnlAuthor
            // 
            tlpnlAuthor.AutoSize = true;
            tlpnlAuthor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlAuthor.ColumnCount = 3;
            tlpnlAuthor.ColumnStyles.Add(new ColumnStyle());
            tlpnlAuthor.ColumnStyles.Add(new ColumnStyle());
            tlpnlAuthor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlAuthor.Controls.Add(ShowAuthorAvatarInCommitGraph, 0, 0);
            tlpnlAuthor.Controls.Add(ShowAuthorAvatarInCommitInfo, 0, 1);
            tlpnlAuthor.Controls.Add(lblCacheDays, 0, 2);
            tlpnlAuthor.Controls.Add(_NO_TRANSLATE_DaysToCacheImages, 1, 2);
            tlpnlAuthor.Controls.Add(lblAvatarProvider, 0, 3);
            tlpnlAuthor.Controls.Add(AvatarProvider, 1, 3);
            tlpnlAuthor.Controls.Add(avatarProviderHelp, 2, 3);
            tlpnlAuthor.Controls.Add(lblNoImageService, 0, 4);
            tlpnlAuthor.Controls.Add(pictureAvatarHelp, 2, 4);
            tlpnlAuthor.Controls.Add(_NO_TRANSLATE_NoImageService, 1, 4);
            tlpnlAuthor.Controls.Add(lblCustomAvatarTemplate, 0, 5);
            tlpnlAuthor.Controls.Add(txtCustomAvatarTemplate, 1, 5);
            tlpnlAuthor.Controls.Add(ClearImageCache, 1, 6);
            tlpnlAuthor.Dock = DockStyle.Fill;
            tlpnlAuthor.Location = new Point(8, 24);
            tlpnlAuthor.Name = "tlpnlAuthor";
            tlpnlAuthor.RowCount = 7;
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.RowStyles.Add(new RowStyle());
            tlpnlAuthor.Size = new Size(1464, 197);
            tlpnlAuthor.TabIndex = 0;
            // 
            // ShowAuthorAvatarInCommitGraph
            // 
            ShowAuthorAvatarInCommitGraph.AutoSize = true;
            tlpnlAuthor.SetColumnSpan(ShowAuthorAvatarInCommitGraph, 2);
            ShowAuthorAvatarInCommitGraph.Dock = DockStyle.Fill;
            ShowAuthorAvatarInCommitGraph.Location = new Point(3, 3);
            ShowAuthorAvatarInCommitGraph.Name = "ShowAuthorAvatarInCommitGraph";
            ShowAuthorAvatarInCommitGraph.Size = new Size(357, 19);
            ShowAuthorAvatarInCommitGraph.TabIndex = 0;
            ShowAuthorAvatarInCommitGraph.Text = "Show author's avatar column in the commit graph";
            ShowAuthorAvatarInCommitGraph.UseVisualStyleBackColor = true;
            // 
            // ShowAuthorAvatarInCommitInfo
            // 
            ShowAuthorAvatarInCommitInfo.AutoSize = true;
            tlpnlAuthor.SetColumnSpan(ShowAuthorAvatarInCommitInfo, 2);
            ShowAuthorAvatarInCommitInfo.Dock = DockStyle.Fill;
            ShowAuthorAvatarInCommitInfo.Location = new Point(3, 28);
            ShowAuthorAvatarInCommitInfo.Name = "ShowAuthorAvatarInCommitInfo";
            ShowAuthorAvatarInCommitInfo.Size = new Size(357, 19);
            ShowAuthorAvatarInCommitInfo.TabIndex = 1;
            ShowAuthorAvatarInCommitInfo.Text = "Show author's avatar in the commit info view";
            ShowAuthorAvatarInCommitInfo.UseVisualStyleBackColor = true;
            // 
            // lblCacheDays
            // 
            lblCacheDays.AutoSize = true;
            lblCacheDays.Dock = DockStyle.Fill;
            lblCacheDays.Location = new Point(3, 50);
            lblCacheDays.Name = "lblCacheDays";
            lblCacheDays.Size = new Size(168, 29);
            lblCacheDays.TabIndex = 2;
            lblCacheDays.Text = "Cache images (days)";
            lblCacheDays.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_DaysToCacheImages
            // 
            _NO_TRANSLATE_DaysToCacheImages.Location = new Point(177, 53);
            _NO_TRANSLATE_DaysToCacheImages.Maximum = new decimal(new int[] { 400, 0, 0, 0 });
            _NO_TRANSLATE_DaysToCacheImages.Name = "_NO_TRANSLATE_DaysToCacheImages";
            _NO_TRANSLATE_DaysToCacheImages.Size = new Size(38, 23);
            _NO_TRANSLATE_DaysToCacheImages.TabIndex = 2;
            _NO_TRANSLATE_DaysToCacheImages.TextAlign = HorizontalAlignment.Right;
            // 
            // lblAvatarProvider
            // 
            lblAvatarProvider.AutoSize = true;
            lblAvatarProvider.Dock = DockStyle.Fill;
            lblAvatarProvider.Location = new Point(3, 79);
            lblAvatarProvider.Name = "lblAvatarProvider";
            lblAvatarProvider.Size = new Size(168, 29);
            lblAvatarProvider.TabIndex = 11;
            lblAvatarProvider.Text = "Avatar provider";
            lblAvatarProvider.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AvatarProvider
            // 
            AvatarProvider.Dock = DockStyle.Fill;
            AvatarProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            AvatarProvider.FormattingEnabled = true;
            AvatarProvider.Location = new Point(177, 82);
            AvatarProvider.Name = "AvatarProvider";
            AvatarProvider.Size = new Size(183, 23);
            AvatarProvider.TabIndex = 3;
            AvatarProvider.SelectedIndexChanged += AvatarProvider_SelectedIndexChanged;
            // 
            // avatarProviderHelp
            // 
            avatarProviderHelp.Cursor = Cursors.Hand;
            avatarProviderHelp.Image = Properties.Resources.information;
            avatarProviderHelp.Location = new Point(366, 84);
            avatarProviderHelp.Margin = new Padding(3, 5, 3, 3);
            avatarProviderHelp.Name = "avatarProviderHelp";
            avatarProviderHelp.Size = new Size(16, 16);
            avatarProviderHelp.SizeMode = PictureBoxSizeMode.AutoSize;
            avatarProviderHelp.TabIndex = 12;
            avatarProviderHelp.TabStop = false;
            avatarProviderHelp.Click += customAvatarHelp_Click;
            // 
            // lblNoImageService
            // 
            lblNoImageService.AutoSize = true;
            lblNoImageService.Dock = DockStyle.Fill;
            lblNoImageService.Location = new Point(3, 108);
            lblNoImageService.Name = "lblNoImageService";
            lblNoImageService.Size = new Size(168, 29);
            lblNoImageService.TabIndex = 4;
            lblNoImageService.Text = "Fallback generated avatar style";
            lblNoImageService.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureAvatarHelp
            // 
            pictureAvatarHelp.Cursor = Cursors.Hand;
            pictureAvatarHelp.Image = Properties.Resources.information;
            pictureAvatarHelp.Location = new Point(366, 113);
            pictureAvatarHelp.Margin = new Padding(3, 5, 3, 3);
            pictureAvatarHelp.Name = "pictureAvatarHelp";
            pictureAvatarHelp.Size = new Size(16, 16);
            pictureAvatarHelp.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureAvatarHelp.TabIndex = 3;
            pictureAvatarHelp.TabStop = false;
            pictureAvatarHelp.Click += pictureAvatarHelp_Click;
            // 
            // _NO_TRANSLATE_NoImageService
            // 
            _NO_TRANSLATE_NoImageService.Dock = DockStyle.Fill;
            _NO_TRANSLATE_NoImageService.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_NoImageService.FormattingEnabled = true;
            _NO_TRANSLATE_NoImageService.Location = new Point(177, 111);
            _NO_TRANSLATE_NoImageService.Name = "_NO_TRANSLATE_NoImageService";
            _NO_TRANSLATE_NoImageService.Size = new Size(183, 23);
            _NO_TRANSLATE_NoImageService.TabIndex = 4;
            // 
            // lblCustomAvatarTemplate
            // 
            lblCustomAvatarTemplate.AutoSize = true;
            lblCustomAvatarTemplate.Dock = DockStyle.Fill;
            lblCustomAvatarTemplate.Location = new Point(3, 137);
            lblCustomAvatarTemplate.Name = "lblCustomAvatarTemplate";
            lblCustomAvatarTemplate.Size = new Size(168, 29);
            lblCustomAvatarTemplate.TabIndex = 5;
            lblCustomAvatarTemplate.Text = "Custom avatar template";
            lblCustomAvatarTemplate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCustomAvatarTemplate
            // 
            txtCustomAvatarTemplate.Dock = DockStyle.Fill;
            txtCustomAvatarTemplate.Location = new Point(177, 140);
            txtCustomAvatarTemplate.Name = "txtCustomAvatarTemplate";
            txtCustomAvatarTemplate.Size = new Size(183, 23);
            txtCustomAvatarTemplate.TabIndex = 5;
            // 
            // ClearImageCache
            // 
            ClearImageCache.AutoSize = true;
            ClearImageCache.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClearImageCache.Dock = DockStyle.Fill;
            ClearImageCache.Location = new Point(177, 169);
            ClearImageCache.Name = "ClearImageCache";
            ClearImageCache.Size = new Size(183, 25);
            ClearImageCache.TabIndex = 6;
            ClearImageCache.Text = "Clear image cache";
            ClearImageCache.UseVisualStyleBackColor = true;
            ClearImageCache.Click += ClearImageCache_Click;
            // 
            // fixedWidthFontDialog
            // 
            fixedWidthFontDialog.AllowVerticalFonts = false;
            fixedWidthFontDialog.Color = SystemColors.ControlText;
            fixedWidthFontDialog.FixedPitchOnly = true;
            // 
            // applicationDialog
            // 
            applicationDialog.AllowVerticalFonts = false;
            applicationDialog.Color = SystemColors.ControlText;
            // 
            // commitFontDialog
            // 
            commitFontDialog.AllowVerticalFonts = false;
            commitFontDialog.Color = SystemColors.ControlText;
            // 
            // AppearanceSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tlpnlMain);
            MinimumSize = new Size(258, 255);
            Name = "AppearanceSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1502, 599);
            Text = "Appearance";
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbGeneral.ResumeLayout(false);
            gbGeneral.PerformLayout();
            tlpnlGeneral.ResumeLayout(false);
            tlpnlGeneral.PerformLayout();
            gbLanguages.ResumeLayout(false);
            gbLanguages.PerformLayout();
            tlpnlLanguage.ResumeLayout(false);
            tlpnlLanguage.PerformLayout();
            gbAuthorImages.ResumeLayout(false);
            gbAuthorImages.PerformLayout();
            tlpnlAuthor.ResumeLayout(false);
            tlpnlAuthor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_DaysToCacheImages).EndInit();
            ((System.ComponentModel.ISupportInitialize)avatarProviderHelp).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureAvatarHelp).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gbLanguages;
        private Label lblLanguage;
        private Label lblSpellingDictionary;
        private ComboBox Dictionary;
        private LinkLabel downloadDictionary;
        private ComboBox Language;
        private LinkLabel helpTranslate;
        private GroupBox gbGeneral;
        private CheckBox chkShowRelativeDate;
        private CheckBox chkShowRepoCurrentBranch;
        private CheckBox chkShowCurrentBranchInVisualStudio;
        private CheckBox chkEnableAutoScale;
        private Label truncateLongFilenames;
        private ComboBox truncatePathMethod;
        private GroupBox gbAuthorImages;
        private TextBox txtCustomAvatarTemplate;
        private ComboBox _NO_TRANSLATE_NoImageService;
        private Label lblNoImageService;
        private Label lblCustomAvatarTemplate;
        private NumericUpDown _NO_TRANSLATE_DaysToCacheImages;
        private Label lblCacheDays;
        private Button ClearImageCache;
        private CheckBox ShowAuthorAvatarInCommitInfo;
        private FontDialog fixedWidthFontDialog;
        private FontDialog applicationDialog;
        private FontDialog commitFontDialog;
        private TableLayoutPanel tlpnlLanguage;
        private TableLayoutPanel tlpnlGeneral;
        private TableLayoutPanel tlpnlAuthor;
        private CheckBox ShowAuthorAvatarInCommitGraph;
        private PictureBox pictureAvatarHelp;
        private PictureBox avatarProviderHelp;
        private Label lblAvatarProvider;
        private ComboBox AvatarProvider;
    }
}

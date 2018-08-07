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
            if (disposing && (components != null))
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
            this.ShowAuthorAvatar = new System.Windows.Forms.CheckBox();
            this.ClearImageCache = new System.Windows.Forms.Button();
            this.NoImageService = new System.Windows.Forms.ComboBox();
            this.lblCacheDays = new System.Windows.Forms.Label();
            this.lblNoImageService = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_DaysToCacheImages = new System.Windows.Forms.NumericUpDown();
            this.fixedWidthFontDialog = new System.Windows.Forms.FontDialog();
            this.applicationDialog = new System.Windows.Forms.FontDialog();
            this.commitFontDialog = new System.Windows.Forms.FontDialog();
            tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            this.gbGeneral.SuspendLayout();
            this.tlpnlGeneral.SuspendLayout();
            this.gbLanguages.SuspendLayout();
            this.tlpnlLanguage.SuspendLayout();
            this.gbAuthorImages.SuspendLayout();
            this.tlpnlAuthor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).BeginInit();
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
            tlpnlMain.Size = new System.Drawing.Size(757, 515);
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
            this.gbGeneral.Size = new System.Drawing.Size(751, 153);
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
            this.tlpnlGeneral.Controls.Add(this.chkShowRelativeDate, 0, 0);
            this.tlpnlGeneral.Controls.Add(this.truncatePathMethod, 1, 3);
            this.tlpnlGeneral.Controls.Add(this.truncateLongFilenames, 0, 3);
            this.tlpnlGeneral.Controls.Add(this.chkEnableAutoScale, 0, 2);
            this.tlpnlGeneral.Controls.Add(this.chkShowCurrentBranchInVisualStudio, 0, 1);
            this.tlpnlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlGeneral.Location = new System.Drawing.Point(8, 22);
            this.tlpnlGeneral.Name = "tlpnlGeneral";
            this.tlpnlGeneral.RowCount = 4;
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.Size = new System.Drawing.Size(735, 123);
            this.tlpnlGeneral.TabIndex = 0;
            // 
            // chkShowRelativeDate
            // 
            this.chkShowRelativeDate.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkShowRelativeDate, 2);
            this.chkShowRelativeDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowRelativeDate.Location = new System.Drawing.Point(3, 3);
            this.chkShowRelativeDate.Name = "chkShowRelativeDate";
            this.chkShowRelativeDate.Size = new System.Drawing.Size(281, 17);
            this.chkShowRelativeDate.TabIndex = 0;
            this.chkShowRelativeDate.Text = "Show relative date instead of full date";
            this.chkShowRelativeDate.UseVisualStyleBackColor = true;
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
            this.truncatePathMethod.Location = new System.Drawing.Point(130, 99);
            this.truncatePathMethod.Name = "truncatePathMethod";
            this.truncatePathMethod.Size = new System.Drawing.Size(154, 21);
            this.truncatePathMethod.TabIndex = 4;
            // 
            // truncateLongFilenames
            // 
            this.truncateLongFilenames.AutoSize = true;
            this.truncateLongFilenames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.truncateLongFilenames.Location = new System.Drawing.Point(3, 96);
            this.truncateLongFilenames.Name = "truncateLongFilenames";
            this.truncateLongFilenames.Size = new System.Drawing.Size(121, 27);
            this.truncateLongFilenames.TabIndex = 3;
            this.truncateLongFilenames.Text = "Truncate long filenames";
            this.truncateLongFilenames.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkEnableAutoScale
            // 
            this.chkEnableAutoScale.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkEnableAutoScale, 2);
            this.chkEnableAutoScale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkEnableAutoScale.Location = new System.Drawing.Point(3, 49);
            this.chkEnableAutoScale.Name = "chkEnableAutoScale";
            this.chkEnableAutoScale.Size = new System.Drawing.Size(281, 17);
            this.chkEnableAutoScale.TabIndex = 2;
            this.chkEnableAutoScale.Text = "Auto scale user interface when high DPI is used";
            this.chkEnableAutoScale.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentBranchInVisualStudio
            // 
            this.chkShowCurrentBranchInVisualStudio.AutoSize = true;
            this.tlpnlGeneral.SetColumnSpan(this.chkShowCurrentBranchInVisualStudio, 2);
            this.chkShowCurrentBranchInVisualStudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowCurrentBranchInVisualStudio.Location = new System.Drawing.Point(3, 26);
            this.chkShowCurrentBranchInVisualStudio.Name = "chkShowCurrentBranchInVisualStudio";
            this.chkShowCurrentBranchInVisualStudio.Size = new System.Drawing.Size(281, 17);
            this.chkShowCurrentBranchInVisualStudio.TabIndex = 1;
            this.chkShowCurrentBranchInVisualStudio.Text = "Show current branch in Visual Studio";
            this.chkShowCurrentBranchInVisualStudio.UseVisualStyleBackColor = true;
            // 
            // gbLanguages
            // 
            this.gbLanguages.AutoSize = true;
            this.gbLanguages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbLanguages.Controls.Add(this.tlpnlLanguage);
            this.gbLanguages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLanguages.Location = new System.Drawing.Point(3, 304);
            this.gbLanguages.Name = "gbLanguages";
            this.gbLanguages.Padding = new System.Windows.Forms.Padding(8);
            this.gbLanguages.Size = new System.Drawing.Size(751, 84);
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
            this.tlpnlLanguage.Location = new System.Drawing.Point(8, 22);
            this.tlpnlLanguage.Name = "tlpnlLanguage";
            this.tlpnlLanguage.RowCount = 2;
            this.tlpnlLanguage.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlLanguage.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlLanguage.Size = new System.Drawing.Size(735, 54);
            this.tlpnlLanguage.TabIndex = 0;
            // 
            // Dictionary
            // 
            this.Dictionary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Dictionary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Dictionary.FormattingEnabled = true;
            this.Dictionary.Location = new System.Drawing.Point(159, 30);
            this.Dictionary.Name = "Dictionary";
            this.Dictionary.Size = new System.Drawing.Size(86, 21);
            this.Dictionary.TabIndex = 4;
            this.Dictionary.DropDown += new System.EventHandler(this.Dictionary_DropDown);
            // 
            // downloadDictionary
            // 
            this.downloadDictionary.AutoSize = true;
            this.downloadDictionary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadDictionary.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.downloadDictionary.Location = new System.Drawing.Point(251, 27);
            this.downloadDictionary.Name = "downloadDictionary";
            this.downloadDictionary.Size = new System.Drawing.Size(104, 27);
            this.downloadDictionary.TabIndex = 5;
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
            this.lblSpellingDictionary.Size = new System.Drawing.Size(150, 27);
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
            this.lblLanguage.Size = new System.Drawing.Size(150, 27);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Text = "Language (restart required)";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Language
            // 
            this.Language.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Language.FormattingEnabled = true;
            this.Language.Location = new System.Drawing.Point(159, 3);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(86, 21);
            this.Language.TabIndex = 1;
            // 
            // helpTranslate
            // 
            this.helpTranslate.AutoSize = true;
            this.helpTranslate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpTranslate.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.helpTranslate.Location = new System.Drawing.Point(251, 0);
            this.helpTranslate.Name = "helpTranslate";
            this.helpTranslate.Size = new System.Drawing.Size(104, 27);
            this.helpTranslate.TabIndex = 2;
            this.helpTranslate.TabStop = true;
            this.helpTranslate.Text = "Help translate";
            this.helpTranslate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.helpTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpTranslate_LinkClicked);
            // 
            // gbAuthorImages
            // 
            this.gbAuthorImages.AutoSize = true;
            this.gbAuthorImages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbAuthorImages.Controls.Add(this.tlpnlAuthor);
            this.gbAuthorImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAuthorImages.Location = new System.Drawing.Point(3, 162);
            this.gbAuthorImages.Name = "gbAuthorImages";
            this.gbAuthorImages.Padding = new System.Windows.Forms.Padding(8);
            this.gbAuthorImages.Size = new System.Drawing.Size(751, 136);
            this.gbAuthorImages.TabIndex = 1;
            this.gbAuthorImages.TabStop = false;
            this.gbAuthorImages.Text = "Author images";
            // 
            // tlpnlAuthor
            // 
            this.tlpnlAuthor.AutoSize = true;
            this.tlpnlAuthor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlAuthor.ColumnCount = 3;
            this.tlpnlAuthor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlAuthor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlAuthor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlAuthor.Controls.Add(this.ShowAuthorAvatar, 0, 0);
            this.tlpnlAuthor.Controls.Add(this.ClearImageCache, 1, 3);
            this.tlpnlAuthor.Controls.Add(this.NoImageService, 1, 2);
            this.tlpnlAuthor.Controls.Add(this.lblCacheDays, 0, 1);
            this.tlpnlAuthor.Controls.Add(this.lblNoImageService, 0, 2);
            this.tlpnlAuthor.Controls.Add(this._NO_TRANSLATE_DaysToCacheImages, 1, 1);
            this.tlpnlAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlAuthor.Location = new System.Drawing.Point(8, 22);
            this.tlpnlAuthor.Name = "tlpnlAuthor";
            this.tlpnlAuthor.RowCount = 4;
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlAuthor.Size = new System.Drawing.Size(735, 106);
            this.tlpnlAuthor.TabIndex = 0;
            // 
            // ShowAuthorAvatar
            // 
            this.ShowAuthorAvatar.AutoSize = true;
            this.tlpnlAuthor.SetColumnSpan(this.ShowAuthorAvatar, 2);
            this.ShowAuthorAvatar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowAuthorAvatar.Location = new System.Drawing.Point(3, 3);
            this.ShowAuthorAvatar.Name = "ShowAuthorAvatar";
            this.ShowAuthorAvatar.Size = new System.Drawing.Size(217, 17);
            this.ShowAuthorAvatar.TabIndex = 0;
            this.ShowAuthorAvatar.Text = "Show author's avatar in the commit info view";
            this.ShowAuthorAvatar.UseVisualStyleBackColor = true;
            // 
            // ClearImageCache
            // 
            this.ClearImageCache.AutoSize = true;
            this.ClearImageCache.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClearImageCache.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClearImageCache.Location = new System.Drawing.Point(116, 80);
            this.ClearImageCache.Name = "ClearImageCache";
            this.ClearImageCache.Size = new System.Drawing.Size(104, 23);
            this.ClearImageCache.TabIndex = 5;
            this.ClearImageCache.Text = "Clear image cache";
            this.ClearImageCache.UseVisualStyleBackColor = true;
            this.ClearImageCache.Click += new System.EventHandler(this.ClearImageCache_Click);
            // 
            // NoImageService
            // 
            this.NoImageService.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoImageService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NoImageService.FormattingEnabled = true;
            this.NoImageService.Location = new System.Drawing.Point(116, 53);
            this.NoImageService.Name = "NoImageService";
            this.NoImageService.Size = new System.Drawing.Size(104, 21);
            this.NoImageService.TabIndex = 4;
            // 
            // lblCacheDays
            // 
            this.lblCacheDays.AutoSize = true;
            this.lblCacheDays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCacheDays.Location = new System.Drawing.Point(3, 23);
            this.lblCacheDays.Name = "lblCacheDays";
            this.lblCacheDays.Size = new System.Drawing.Size(107, 27);
            this.lblCacheDays.TabIndex = 1;
            this.lblCacheDays.Text = "Cache images (days)";
            this.lblCacheDays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNoImageService
            // 
            this.lblNoImageService.AutoSize = true;
            this.lblNoImageService.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoImageService.Location = new System.Drawing.Point(3, 50);
            this.lblNoImageService.Name = "lblNoImageService";
            this.lblNoImageService.Size = new System.Drawing.Size(107, 27);
            this.lblNoImageService.TabIndex = 3;
            this.lblNoImageService.Text = "No image service";
            this.lblNoImageService.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_DaysToCacheImages
            // 
            this._NO_TRANSLATE_DaysToCacheImages.Location = new System.Drawing.Point(116, 26);
            this._NO_TRANSLATE_DaysToCacheImages.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._NO_TRANSLATE_DaysToCacheImages.Name = "_NO_TRANSLATE_DaysToCacheImages";
            this._NO_TRANSLATE_DaysToCacheImages.Size = new System.Drawing.Size(38, 21);
            this._NO_TRANSLATE_DaysToCacheImages.TabIndex = 2;
            this._NO_TRANSLATE_DaysToCacheImages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
            // AppearanceSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(tlpnlMain);
            this.MinimumSize = new System.Drawing.Size(258, 255);
            this.Name = "AppearanceSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(773, 531);
            this.Text = "Appearance";
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
        private System.Windows.Forms.CheckBox chkShowCurrentBranchInVisualStudio;
        private System.Windows.Forms.CheckBox chkEnableAutoScale;
        private System.Windows.Forms.Label truncateLongFilenames;
        private System.Windows.Forms.ComboBox truncatePathMethod;
        private System.Windows.Forms.GroupBox gbAuthorImages;
        private System.Windows.Forms.ComboBox NoImageService;
        private System.Windows.Forms.Label lblNoImageService;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_DaysToCacheImages;
        private System.Windows.Forms.Label lblCacheDays;
        private System.Windows.Forms.Button ClearImageCache;
        private System.Windows.Forms.CheckBox ShowAuthorAvatar;
        private System.Windows.Forms.FontDialog fixedWidthFontDialog;
        private System.Windows.Forms.FontDialog applicationDialog;
        private System.Windows.Forms.FontDialog commitFontDialog;
        private System.Windows.Forms.TableLayoutPanel tlpnlLanguage;
        private System.Windows.Forms.TableLayoutPanel tlpnlGeneral;
        private System.Windows.Forms.TableLayoutPanel tlpnlAuthor;
    }
}

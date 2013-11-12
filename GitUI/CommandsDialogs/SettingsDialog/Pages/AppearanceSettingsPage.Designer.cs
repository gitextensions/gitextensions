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
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.label49 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.Dictionary = new System.Windows.Forms.ComboBox();
            this.downloadDictionary = new System.Windows.Forms.LinkLabel();
            this.Language = new System.Windows.Forms.ComboBox();
            this.helpTranslate = new System.Windows.Forms.LinkLabel();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.commitFontChangeButton = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.diffFontChangeButton = new System.Windows.Forms.Button();
            this.applicationFontChangeButton = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.chkShowRelativeDate = new System.Windows.Forms.CheckBox();
            this.chkShowCurrentBranchInVisualStudio = new System.Windows.Forms.CheckBox();
            this.chkEnableAutoScale = new System.Windows.Forms.CheckBox();
            this.truncatePathMethod = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_truncatePathMethod = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.AuthorImageSize = new System.Windows.Forms.ComboBox();
            this.NoImageService = new System.Windows.Forms.ComboBox();
            this.label53 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_DaysToCacheImages = new System.Windows.Forms.NumericUpDown();
            this.label46 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.ClearImageCache = new System.Windows.Forms.Button();
            this.ShowAuthorGravatar = new System.Windows.Forms.CheckBox();
            this.diffFontDialog = new System.Windows.Forms.FontDialog();
            this.applicationDialog = new System.Windows.Forms.FontDialog();
            this.commitFontDialog = new System.Windows.Forms.FontDialog();
            this.groupBox13.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox13
            // 
            this.groupBox13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox13.Controls.Add(this.label49);
            this.groupBox13.Controls.Add(this.label22);
            this.groupBox13.Controls.Add(this.Dictionary);
            this.groupBox13.Controls.Add(this.downloadDictionary);
            this.groupBox13.Controls.Add(this.Language);
            this.groupBox13.Controls.Add(this.helpTranslate);
            this.groupBox13.Location = new System.Drawing.Point(3, 443);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(597, 84);
            this.groupBox13.TabIndex = 60;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Language";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(8, 21);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(167, 16);
            this.label49.TabIndex = 30;
            this.label49.Text = "Language (restart required)";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(8, 50);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(179, 16);
            this.label22.TabIndex = 33;
            this.label22.Text = "Dictionary for spelling checker";
            // 
            // Dictionary
            // 
            this.Dictionary.FormattingEnabled = true;
            this.Dictionary.Location = new System.Drawing.Point(283, 47);
            this.Dictionary.Name = "Dictionary";
            this.Dictionary.Size = new System.Drawing.Size(169, 24);
            this.Dictionary.TabIndex = 34;
            this.Dictionary.DropDown += new System.EventHandler(this.Dictionary_DropDown);
            // 
            // downloadDictionary
            // 
            this.downloadDictionary.AutoSize = true;
            this.downloadDictionary.Location = new System.Drawing.Point(458, 50);
            this.downloadDictionary.Name = "downloadDictionary";
            this.downloadDictionary.Size = new System.Drawing.Size(123, 16);
            this.downloadDictionary.TabIndex = 35;
            this.downloadDictionary.TabStop = true;
            this.downloadDictionary.Text = "Download dictionary";
            this.downloadDictionary.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadDictionary_LinkClicked);
            // 
            // Language
            // 
            this.Language.FormattingEnabled = true;
            this.Language.Items.AddRange(new object[] {
            "en-US",
            "ja-JP",
            "nl-NL"});
            this.Language.Location = new System.Drawing.Point(283, 18);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(169, 24);
            this.Language.TabIndex = 31;
            // 
            // helpTranslate
            // 
            this.helpTranslate.AutoSize = true;
            this.helpTranslate.Location = new System.Drawing.Point(458, 21);
            this.helpTranslate.Name = "helpTranslate";
            this.helpTranslate.Size = new System.Drawing.Size(87, 16);
            this.helpTranslate.TabIndex = 32;
            this.helpTranslate.TabStop = true;
            this.helpTranslate.Text = "Help translate";
            this.helpTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.helpTranslate_LinkClicked);
            // 
            // groupBox15
            // 
            this.groupBox15.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox15.Controls.Add(this.commitFontChangeButton);
            this.groupBox15.Controls.Add(this.label34);
            this.groupBox15.Controls.Add(this.diffFontChangeButton);
            this.groupBox15.Controls.Add(this.applicationFontChangeButton);
            this.groupBox15.Controls.Add(this.label26);
            this.groupBox15.Controls.Add(this.label56);
            this.groupBox15.Location = new System.Drawing.Point(3, 322);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(594, 115);
            this.groupBox15.TabIndex = 59;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "Fonts";
            // 
            // commitFontChangeButton
            // 
            this.commitFontChangeButton.AutoSize = true;
            this.commitFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitFontChangeButton.Location = new System.Drawing.Point(221, 79);
            this.commitFontChangeButton.Name = "commitFontChangeButton";
            this.commitFontChangeButton.Size = new System.Drawing.Size(76, 26);
            this.commitFontChangeButton.TabIndex = 25;
            this.commitFontChangeButton.Text = "font name";
            this.commitFontChangeButton.UseVisualStyleBackColor = true;
            this.commitFontChangeButton.Click += new System.EventHandler(this.commitFontChangeButton_Click);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(10, 84);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(78, 16);
            this.label34.TabIndex = 24;
            this.label34.Text = "Commit font";
            // 
            // diffFontChangeButton
            // 
            this.diffFontChangeButton.AutoSize = true;
            this.diffFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.diffFontChangeButton.Location = new System.Drawing.Point(221, 15);
            this.diffFontChangeButton.Name = "diffFontChangeButton";
            this.diffFontChangeButton.Size = new System.Drawing.Size(76, 26);
            this.diffFontChangeButton.TabIndex = 21;
            this.diffFontChangeButton.Text = "font name";
            this.diffFontChangeButton.UseVisualStyleBackColor = true;
            this.diffFontChangeButton.Click += new System.EventHandler(this.diffFontChangeButton_Click);
            // 
            // applicationFontChangeButton
            // 
            this.applicationFontChangeButton.AutoSize = true;
            this.applicationFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.applicationFontChangeButton.Location = new System.Drawing.Point(221, 47);
            this.applicationFontChangeButton.Name = "applicationFontChangeButton";
            this.applicationFontChangeButton.Size = new System.Drawing.Size(76, 26);
            this.applicationFontChangeButton.TabIndex = 23;
            this.applicationFontChangeButton.Text = "font name";
            this.applicationFontChangeButton.UseVisualStyleBackColor = true;
            this.applicationFontChangeButton.Click += new System.EventHandler(this.applicationFontChangeButton_Click);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(10, 52);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(96, 16);
            this.label26.TabIndex = 22;
            this.label26.Text = "Application font";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(10, 22);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(63, 16);
            this.label56.TabIndex = 20;
            this.label56.Text = "Code font";
            // 
            // groupBox14
            // 
            this.groupBox14.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox14.Controls.Add(this.chkShowRelativeDate);
            this.groupBox14.Controls.Add(this.chkShowCurrentBranchInVisualStudio);
            this.groupBox14.Controls.Add(this.chkEnableAutoScale);
            this.groupBox14.Controls.Add(this.truncatePathMethod);
            this.groupBox14.Controls.Add(this._NO_TRANSLATE_truncatePathMethod);
            this.groupBox14.Location = new System.Drawing.Point(3, 3);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(594, 136);
            this.groupBox14.TabIndex = 57;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "General";
            // 
            // chkShowRelativeDate
            // 
            this.chkShowRelativeDate.AutoSize = true;
            this.chkShowRelativeDate.Location = new System.Drawing.Point(10, 22);
            this.chkShowRelativeDate.Name = "chkShowRelativeDate";
            this.chkShowRelativeDate.Size = new System.Drawing.Size(244, 20);
            this.chkShowRelativeDate.TabIndex = 1;
            this.chkShowRelativeDate.Text = "Show relative date instead of full date";
            this.chkShowRelativeDate.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentBranchInVisualStudio
            // 
            this.chkShowCurrentBranchInVisualStudio.AutoSize = true;
            this.chkShowCurrentBranchInVisualStudio.Location = new System.Drawing.Point(10, 48);
            this.chkShowCurrentBranchInVisualStudio.Name = "chkShowCurrentBranchInVisualStudio";
            this.chkShowCurrentBranchInVisualStudio.Size = new System.Drawing.Size(239, 20);
            this.chkShowCurrentBranchInVisualStudio.TabIndex = 2;
            this.chkShowCurrentBranchInVisualStudio.Text = "Show current branch in Visual Studio";
            this.chkShowCurrentBranchInVisualStudio.UseVisualStyleBackColor = true;
            // 
            // chkEnableAutoScale
            // 
            this.chkEnableAutoScale.AutoSize = true;
            this.chkEnableAutoScale.Location = new System.Drawing.Point(10, 75);
            this.chkEnableAutoScale.Margin = new System.Windows.Forms.Padding(4);
            this.chkEnableAutoScale.Name = "chkEnableAutoScale";
            this.chkEnableAutoScale.Size = new System.Drawing.Size(299, 20);
            this.chkEnableAutoScale.TabIndex = 3;
            this.chkEnableAutoScale.Text = "Auto scale user interface when high DPI is used";
            this.chkEnableAutoScale.UseVisualStyleBackColor = true;
            // 
            // truncatePathMethod
            // 
            this.truncatePathMethod.AutoSize = true;
            this.truncatePathMethod.Location = new System.Drawing.Point(10, 104);
            this.truncatePathMethod.Name = "truncatePathMethod";
            this.truncatePathMethod.Size = new System.Drawing.Size(146, 16);
            this.truncatePathMethod.TabIndex = 5;
            this.truncatePathMethod.Text = "Truncate long filenames";
            // 
            // _NO_TRANSLATE_truncatePathMethod
            // 
            this._NO_TRANSLATE_truncatePathMethod.FormattingEnabled = true;
            this._NO_TRANSLATE_truncatePathMethod.Items.AddRange(new object[] {
            "none",
            "compact",
            "trimstart"});
            this._NO_TRANSLATE_truncatePathMethod.Location = new System.Drawing.Point(221, 101);
            this._NO_TRANSLATE_truncatePathMethod.Name = "_NO_TRANSLATE_truncatePathMethod";
            this._NO_TRANSLATE_truncatePathMethod.Size = new System.Drawing.Size(242, 24);
            this._NO_TRANSLATE_truncatePathMethod.TabIndex = 6;
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.AuthorImageSize);
            this.groupBox6.Controls.Add(this.NoImageService);
            this.groupBox6.Controls.Add(this.label53);
            this.groupBox6.Controls.Add(this.label47);
            this.groupBox6.Controls.Add(this._NO_TRANSLATE_DaysToCacheImages);
            this.groupBox6.Controls.Add(this.label46);
            this.groupBox6.Controls.Add(this.label44);
            this.groupBox6.Controls.Add(this.ClearImageCache);
            this.groupBox6.Controls.Add(this.ShowAuthorGravatar);
            this.groupBox6.Location = new System.Drawing.Point(3, 145);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(594, 171);
            this.groupBox6.TabIndex = 58;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Author images";
            // 
            // AuthorImageSize
            // 
            this.AuthorImageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AuthorImageSize.FormattingEnabled = true;
            this.AuthorImageSize.Items.AddRange(new object[] {
            "Small (80x80)",
            "Normal (160x160)",
            "Large (240x240)",
            "Extra Large (320x320)"});
            this.AuthorImageSize.Location = new System.Drawing.Point(221, 45);
            this.AuthorImageSize.Margin = new System.Windows.Forms.Padding(2);
            this.AuthorImageSize.Name = "AuthorImageSize";
            this.AuthorImageSize.Size = new System.Drawing.Size(142, 24);
            this.AuthorImageSize.TabIndex = 12;
            // 
            // NoImageService
            // 
            this.NoImageService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NoImageService.FormattingEnabled = true;
            this.NoImageService.Location = new System.Drawing.Point(221, 100);
            this.NoImageService.Name = "NoImageService";
            this.NoImageService.Size = new System.Drawing.Size(142, 24);
            this.NoImageService.TabIndex = 17;
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(10, 103);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(106, 16);
            this.label53.TabIndex = 16;
            this.label53.Text = "No image service";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(304, 74);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(34, 16);
            this.label47.TabIndex = 15;
            this.label47.Text = "days";
            // 
            // _NO_TRANSLATE_DaysToCacheImages
            // 
            this._NO_TRANSLATE_DaysToCacheImages.Location = new System.Drawing.Point(221, 73);
            this._NO_TRANSLATE_DaysToCacheImages.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this._NO_TRANSLATE_DaysToCacheImages.Name = "_NO_TRANSLATE_DaysToCacheImages";
            this._NO_TRANSLATE_DaysToCacheImages.Size = new System.Drawing.Size(77, 23);
            this._NO_TRANSLATE_DaysToCacheImages.TabIndex = 14;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(10, 75);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(88, 16);
            this.label46.TabIndex = 13;
            this.label46.Text = "Cache images";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(10, 48);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(70, 16);
            this.label44.TabIndex = 11;
            this.label44.Text = "Image size";
            // 
            // ClearImageCache
            // 
            this.ClearImageCache.Location = new System.Drawing.Point(10, 131);
            this.ClearImageCache.Name = "ClearImageCache";
            this.ClearImageCache.Size = new System.Drawing.Size(142, 25);
            this.ClearImageCache.TabIndex = 18;
            this.ClearImageCache.Text = "Clear image cache";
            this.ClearImageCache.UseVisualStyleBackColor = true;
            this.ClearImageCache.Click += new System.EventHandler(this.ClearImageCache_Click);
            // 
            // ShowAuthorGravatar
            // 
            this.ShowAuthorGravatar.AutoSize = true;
            this.ShowAuthorGravatar.Location = new System.Drawing.Point(10, 22);
            this.ShowAuthorGravatar.Name = "ShowAuthorGravatar";
            this.ShowAuthorGravatar.Size = new System.Drawing.Size(237, 20);
            this.ShowAuthorGravatar.TabIndex = 10;
            this.ShowAuthorGravatar.Text = "Get author image from gravatar.com";
            this.ShowAuthorGravatar.UseVisualStyleBackColor = true;
            // 
            // diffFontDialog
            // 
            this.diffFontDialog.AllowVerticalFonts = false;
            this.diffFontDialog.Color = System.Drawing.SystemColors.ControlText;
            this.diffFontDialog.FixedPitchOnly = true;
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
            this.Controls.Add(this.groupBox13);
            this.Controls.Add(this.groupBox15);
            this.Controls.Add(this.groupBox14);
            this.Controls.Add(this.groupBox6);
            this.MinimumSize = new System.Drawing.Size(515, 510);
            this.Name = "AppearanceSettingsPage";
            this.Size = new System.Drawing.Size(600, 527);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_DaysToCacheImages)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ComboBox Dictionary;
        private System.Windows.Forms.LinkLabel downloadDictionary;
        private System.Windows.Forms.ComboBox Language;
        private System.Windows.Forms.LinkLabel helpTranslate;
        private System.Windows.Forms.GroupBox groupBox15;
        private System.Windows.Forms.Button diffFontChangeButton;
        private System.Windows.Forms.Button applicationFontChangeButton;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.CheckBox chkShowRelativeDate;
        private System.Windows.Forms.CheckBox chkShowCurrentBranchInVisualStudio;
        private System.Windows.Forms.CheckBox chkEnableAutoScale;
        private System.Windows.Forms.Label truncatePathMethod;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_truncatePathMethod;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.ComboBox NoImageService;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_DaysToCacheImages;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.Button ClearImageCache;
        private System.Windows.Forms.CheckBox ShowAuthorGravatar;
        private System.Windows.Forms.FontDialog diffFontDialog;
        private System.Windows.Forms.FontDialog applicationDialog;
        private System.Windows.Forms.Button commitFontChangeButton;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.FontDialog commitFontDialog;
        private System.Windows.Forms.ComboBox AuthorImageSize;
    }
}

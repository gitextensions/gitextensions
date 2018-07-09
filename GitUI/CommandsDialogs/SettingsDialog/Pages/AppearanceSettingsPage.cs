﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitUI.Avatars;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AppearanceSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _noDictFile = new TranslationString("None");
        private readonly TranslationString _noDictFilesFound = new TranslationString("No dictionary files found in: {0}");

        public AppearanceSettingsPage()
        {
            InitializeComponent();
            Translate();

            NoImageService.Items.AddRange(Enum.GetNames(typeof(DefaultImageType)));
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            // align 1st columns across all tables
            tlpnlGeneral.AdjustWidthToSize(0, truncateLongFilenames, lblCacheDays, lblNoImageService, lblLanguage, lblSpellingDictionary);
            tlpnlAuthor.AdjustWidthToSize(0, truncateLongFilenames, lblCacheDays, lblNoImageService, lblLanguage, lblSpellingDictionary);
            tlpnlLanguage.AdjustWidthToSize(0, truncateLongFilenames, lblCacheDays, lblNoImageService, lblLanguage, lblSpellingDictionary);

            // align 2nd columns across all tables
            cbBranchOrderingCriteria.AdjustWidthToFitContent();
            truncatePathMethod.AdjustWidthToFitContent();
            Language.AdjustWidthToFitContent();
            tlpnlGeneral.AdjustWidthToSize(1, truncatePathMethod, NoImageService, Language);
            tlpnlAuthor.AdjustWidthToSize(1, truncatePathMethod, NoImageService, Language);
            tlpnlLanguage.AdjustWidthToSize(1, truncatePathMethod, NoImageService, Language);
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AppearanceSettingsPage));
        }

        private static int GetTruncatePathMethodIndex(string text)
        {
            switch (text.ToLowerInvariant())
            {
                case "compact":
                    return 1;
                case "trimstart":
                    return 2;
                case "filenameonly":
                    return 3;
                default:
                    return 0;
            }
        }

        private static string GetTruncatePathMethodString(int index)
        {
            switch (index)
            {
                case 1:
                    return "compact";
                case 2:
                    return "trimstart";
                case 3:
                    return "fileNameOnly";
                default:
                    return "none";
            }
        }

        protected override void SettingsToPage()
        {
            chkEnableAutoScale.Checked = AppSettings.EnableAutoScale;

            chkShowCurrentBranchInVisualStudio.Checked = AppSettings.ShowCurrentBranchInVisualStudio;
            _NO_TRANSLATE_DaysToCacheImages.Value = AppSettings.AvatarImageCacheDays;
            ShowAuthorAvatar.Checked = AppSettings.ShowAuthorAvatarInCommitInfo;
            NoImageService.Text = AppSettings.GravatarDefaultImageType.ToString();

            Language.Items.Clear();
            Language.Items.Add("English");
            Language.Items.AddRange(Translator.GetAllTranslations());
            Language.Text = AppSettings.Translation;

            truncatePathMethod.SelectedIndex = GetTruncatePathMethodIndex(AppSettings.TruncatePathMethod);
            cbBranchOrderingCriteria.SelectedIndex = (int)AppSettings.BranchOrderingCriteria;

            Dictionary.Items.Clear();
            Dictionary.Items.Add(_noDictFile.Text);
            if (AppSettings.Dictionary.Equals("none", StringComparison.InvariantCultureIgnoreCase))
            {
                Dictionary.SelectedIndex = 0;
            }
            else
            {
                string dictionaryFile = string.Concat(Path.Combine(AppSettings.GetDictionaryDir(), AppSettings.Dictionary), ".dic");
                if (File.Exists(dictionaryFile))
                {
                    Dictionary.Items.Add(AppSettings.Dictionary);
                    Dictionary.Text = AppSettings.Dictionary;
                }
                else
                {
                    Dictionary.SelectedIndex = 0;
                }
            }

            chkShowRelativeDate.Checked = AppSettings.RelativeDate;
        }

        protected override void PageToSettings()
        {
            AppSettings.EnableAutoScale = chkEnableAutoScale.Checked;
            AppSettings.TruncatePathMethod = GetTruncatePathMethodString(truncatePathMethod.SelectedIndex);
            AppSettings.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.Checked;

            AppSettings.Translation = Language.Text;
            Strings.Reinit();

            AppSettings.AvatarImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            AppSettings.ShowAuthorAvatarInCommitInfo = ShowAuthorAvatar.Checked;

            if (Enum.TryParse<DefaultImageType>(NoImageService.Text, ignoreCase: true, out var type))
            {
                AppSettings.GravatarDefaultImageType = type;
            }

            AppSettings.RelativeDate = chkShowRelativeDate.Checked;

            AppSettings.Dictionary = Dictionary.SelectedIndex == 0 ? "none" : Dictionary.Text;
            AppSettings.BranchOrderingCriteria = (GitRefsOrder)cbBranchOrderingCriteria.SelectedIndex;
        }

        private void Dictionary_DropDown(object sender, EventArgs e)
        {
            try
            {
                string currentDictionary = Dictionary.Text;

                Dictionary.Items.Clear();
                Dictionary.Items.Add(_noDictFile.Text);
                foreach (
                    string fileName in
                        Directory.GetFiles(AppSettings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);
                    Dictionary.Items.Add(file.Name.Replace(".dic", ""));
                }

                Dictionary.Text = currentDictionary;
            }
            catch
            {
                MessageBox.Show(this, string.Format(_noDictFilesFound.Text, AppSettings.GetDictionaryDir()));
            }
        }

        private void ClearImageCache_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(AvatarService.Default.ClearCacheAsync);
        }

        private void helpTranslate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "TranslationApp.exe"));
        }

        private void downloadDictionary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/gitextensions/gitextensions/wiki/Spelling");
        }
    }
}

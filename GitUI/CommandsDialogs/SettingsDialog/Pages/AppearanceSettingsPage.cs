using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using Gravatar;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AppearanceSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _noDictFile =
            new TranslationString("None");
        private readonly TranslationString _noDictFilesFound =
            new TranslationString("No dictionary files found in: {0}");
        private readonly IImageCache _avatarCache;

        public AppearanceSettingsPage()
        {
            InitializeComponent();
            Text = "Appearance";
            Translate();

            _avatarCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);

            NoImageService.Items.AddRange(Enum.GetNames(typeof(DefaultImageType)));
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            // align 1st columns across all tables
            tlpnlGeneral.AdjustWidthToSize(0, truncateLongFilenames.Width, lblCacheDays.Width, lblLanguage.Width, lblSpellingDictionary.Width);
            tlpnlAuthor.AdjustWidthToSize(0, truncateLongFilenames.Width, lblCacheDays.Width, lblLanguage.Width, lblSpellingDictionary.Width);
            tlpnlLanguage.AdjustWidthToSize(0, truncateLongFilenames.Width, lblCacheDays.Width, lblLanguage.Width, lblSpellingDictionary.Width);

            // align 2nd columns across all tables
            truncatePathMethod.AdjustWidthToFitContent();
            Language.AdjustWidthToFitContent();
            tlpnlGeneral.AdjustWidthToSize(1, truncatePathMethod.Width, NoImageService.Width, Language.Width);
            tlpnlAuthor.AdjustWidthToSize(1, truncatePathMethod.Width, NoImageService.Width, Language.Width);
            tlpnlLanguage.AdjustWidthToSize(1, truncatePathMethod.Width, NoImageService.Width, Language.Width);
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
            _NO_TRANSLATE_DaysToCacheImages.Value = AppSettings.AuthorImageCacheDays;
            ShowAuthorGravatar.Checked = AppSettings.ShowAuthorGravatar;
            NoImageService.Text = AppSettings.GravatarDefaultImageType;

            Language.Items.Clear();
            Language.Items.Add("English");
            Language.Items.AddRange(Translator.GetAllTranslations());
            Language.Text = AppSettings.Translation;

            truncatePathMethod.SelectedIndex = GetTruncatePathMethodIndex(AppSettings.TruncatePathMethod);

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

            AppSettings.AuthorImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            AppSettings.ShowAuthorGravatar = ShowAuthorGravatar.Checked;
            AppSettings.GravatarDefaultImageType = NoImageService.Text;

            AppSettings.RelativeDate = chkShowRelativeDate.Checked;

            AppSettings.Dictionary = Dictionary.SelectedIndex == 0 ? "none" : Dictionary.Text;
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
            _avatarCache.ClearAsync();
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

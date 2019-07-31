using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI.Avatars;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AppearanceSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _noDictFile = new TranslationString("None");
        private readonly TranslationString _noDictFilesFound = new TranslationString("No dictionary files found in: {0}");
        private readonly TranslationString _noImageServiceTooltip = new TranslationString("A default image, if an email address has no matching Gravatar image.\r\nSee http://en.gravatar.com/site/implement/images#default-image for more details.");
        private readonly TranslationString _authorDateSortWarningTooltip = new TranslationString("Sorting by author date may delay rendering of the revision graph.");

        public AppearanceSettingsPage()
        {
            InitializeComponent();
            InitializeComplete();

            FillComboBoxWithEnumValues<AvatarProvider>(AvatarProvider);
            FillComboBoxWithEnumValues<GravatarFallbackAvatarType>(_NO_TRANSLATE_NoImageService);
        }

        private void FillComboBoxWithEnumValues<T>(ComboBox comboBox) where T : Enum
        {
            comboBox.DisplayMember = nameof(ComboBoxItem<T>.Text);
            comboBox.ValueMember = nameof(ComboBoxItem<T>.Value);
            comboBox.DataSource = EnumHelper.GetValues<T>()
                .Select(e => new ComboBoxItem<T> { Text = e.GetDescription(), Value = e })
                .ToArray();
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            ToolTip.SetToolTip(_NO_TRANSLATE_NoImageService, _noImageServiceTooltip.Text);
            ToolTip.SetToolTip(pictureAvatarHelp, _noImageServiceTooltip.Text);
            ToolTip.SetToolTip(chkSortByAuthorDate, _authorDateSortWarningTooltip.Text);
            pictureAvatarHelp.Size = DpiUtil.Scale(pictureAvatarHelp.Size);

            // align 1st columns across all tables
            tlpnlGeneral.AdjustWidthToSize(0, truncateLongFilenames, lblCacheDays, lblNoImageService, lblLanguage, lblSpellingDictionary);
            tlpnlAuthor.AdjustWidthToSize(0, truncateLongFilenames, lblCacheDays, lblNoImageService, lblLanguage, lblSpellingDictionary);
            tlpnlLanguage.AdjustWidthToSize(0, truncateLongFilenames, lblCacheDays, lblNoImageService, lblLanguage, lblSpellingDictionary);

            // align 2nd columns across all tables
            truncatePathMethod.AdjustWidthToFitContent();
            Language.AdjustWidthToFitContent();
            tlpnlGeneral.AdjustWidthToSize(1, truncatePathMethod, _NO_TRANSLATE_NoImageService, Language);
            tlpnlAuthor.AdjustWidthToSize(1, truncatePathMethod, _NO_TRANSLATE_NoImageService, Language);
            tlpnlLanguage.AdjustWidthToSize(1, truncatePathMethod, _NO_TRANSLATE_NoImageService, Language);
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AppearanceSettingsPage));
        }

        protected override void SettingsToPage()
        {
            chkEnableAutoScale.Checked = AppSettings.EnableAutoScale;

            chkShowCurrentBranchInVisualStudio.Checked = AppSettings.ShowCurrentBranchInVisualStudio;
            _NO_TRANSLATE_DaysToCacheImages.Value = AppSettings.AvatarImageCacheDays;
            ShowAuthorAvatarInCommitInfo.Checked = AppSettings.ShowAuthorAvatarInCommitInfo;
            ShowAuthorAvatarInCommitGraph.Checked = AppSettings.ShowAuthorAvatarColumn;
            chkSortByAuthorDate.Checked = AppSettings.SortByAuthorDate;
            AvatarProvider.SelectedValue = AppSettings.AvatarProvider;
            _NO_TRANSLATE_NoImageService.SelectedValue = AppSettings.GravatarFallbackAvatarType;
            ManageGravatarOptionsDisplay();

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

            return;

            int GetTruncatePathMethodIndex(TruncatePathMethod method)
            {
                switch (method)
                {
                    case TruncatePathMethod.Compact:
                        return 1;
                    case TruncatePathMethod.TrimStart:
                        return 2;
                    case TruncatePathMethod.FileNameOnly:
                        return 3;
                    default:
                        return 0;
                }
            }
        }

        protected override void PageToSettings()
        {
            AppSettings.EnableAutoScale = chkEnableAutoScale.Checked;
            AppSettings.TruncatePathMethod = GetTruncatePathMethodString(truncatePathMethod.SelectedIndex);
            AppSettings.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.Checked;
            AppSettings.ShowAuthorAvatarColumn = ShowAuthorAvatarInCommitGraph.Checked;
            AppSettings.ShowAuthorAvatarInCommitInfo = ShowAuthorAvatarInCommitInfo.Checked;
            AppSettings.AvatarImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;
            AppSettings.SortByAuthorDate = chkSortByAuthorDate.Checked;

            AppSettings.Translation = Language.Text;
            ResourceManager.Strings.Reinitialize();
            Strings.Reinitialize();

            var shouldClearCache =
                AppSettings.AvatarProvider != (AvatarProvider)AvatarProvider.SelectedValue
                || AppSettings.GravatarFallbackAvatarType != (GravatarFallbackAvatarType)_NO_TRANSLATE_NoImageService.SelectedValue;

            AppSettings.AvatarProvider = (AvatarProvider)AvatarProvider.SelectedValue;

            if (_NO_TRANSLATE_NoImageService.SelectedValue is GravatarFallbackAvatarType imageType)
            {
                AppSettings.GravatarFallbackAvatarType = imageType;
            }

            if (shouldClearCache)
            {
                new AvatarControl().ClearCache();
            }

            AppSettings.RelativeDate = chkShowRelativeDate.Checked;

            AppSettings.Dictionary = Dictionary.SelectedIndex == 0 ? "none" : Dictionary.Text;

            return;

            TruncatePathMethod GetTruncatePathMethodString(int index)
            {
                switch (index)
                {
                    case 1:
                        return TruncatePathMethod.Compact;
                    case 2:
                        return TruncatePathMethod.TrimStart;
                    case 3:
                        return TruncatePathMethod.FileNameOnly;
                    default:
                        return TruncatePathMethod.None;
                }
            }
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
            Process.Start(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
        }

        private void downloadDictionary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/gitextensions/gitextensions/wiki/Spelling");
        }

        private void pictureAvatarHelp_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://en.gravatar.com/site/implement/images#default-image");
        }

        private void AvatarProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            ManageGravatarOptionsDisplay();
        }

        private void ManageGravatarOptionsDisplay()
        {
            var showAvatarOptions = (AvatarProvider)AvatarProvider.SelectedValue == GitCommands.AvatarProvider.Gravatar;
            lblNoImageService.Visible = showAvatarOptions;
            _NO_TRANSLATE_NoImageService.Visible = showAvatarOptions;
            pictureAvatarHelp.Visible = showAvatarOptions;
        }

        private class ComboBoxItem<T>
        {
            public string Text { get; set; }
            public T Value { get; set; }
        }
    }
}

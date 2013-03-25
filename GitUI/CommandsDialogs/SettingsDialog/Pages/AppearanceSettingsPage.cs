using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Properties;
using ResourceManager.Translation;
using Gravatar;
using System.IO;
using System.Diagnostics;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AppearanceSettingsPage : SettingsPageBase
    {
        private readonly TranslationString _noDictFilesFound =
            new TranslationString("No dictionary files found in: {0}");

        private Font _diffFont;
        private Font _applicationFont;
        private Font commitFont;

        public AppearanceSettingsPage()
        {
            InitializeComponent();
            Text = "Appearance";
            Translate();

            noImageService.Items.AddRange(GravatarService.DynamicServices.Cast<object>().ToArray());
        }

        protected override string GetCommaSeparatedKeywordList()
        {
            return "graph,visual studio,author,image,font,lang,language,spell,spelling";
        }

        // TODO: needed?
        ////protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        ////{
        ////    if (!e.Cancel)
        ////    {
        ////        diffFontDialog.Dispose();
        ////        applicationDialog.Dispose();
        ////    }
        ////}

        protected override void OnLoadSettings()
        {
            chkEnableAutoScale.Checked = Settings.Default.EnableAutoScale;

            chkShowCurrentBranchInVisualStudio.Checked = Settings.Default.ShowCurrentBranchInVisualStudio;
            _NO_TRANSLATE_DaysToCacheImages.Value = Settings.Default.AuthorImageCacheDays;
            _NO_TRANSLATE_authorImageSize.Value = Settings.Default.AuthorImageSize;
            ShowAuthorGravatar.Checked = Settings.Default.ShowAuthorGravatar;
            noImageService.Text = Settings.Default.GravatarFallbackService;

            Language.Items.Clear();
            Language.Items.Add("English");
            Language.Items.AddRange(Translator.GetAllTranslations());
            Language.Text = Settings.Default.Translation;

            _NO_TRANSLATE_truncatePathMethod.Text = Settings.Default.TruncatePathMethod;

            Dictionary.Text = Settings.Default.Dictionary;

            chkShowRelativeDate.Checked = Settings.Default.RelativeDate;

            SetCurrentApplicationFont(Settings.Default.Font);
            SetCurrentDiffFont(Settings.Default.DiffFont);
            SetCurrentCommitFont(Settings.Default.CommitFont);

        }

        public override void SaveSettings()
        {
            Settings.Default.EnableAutoScale = chkEnableAutoScale.Checked;
            Settings.Default.TruncatePathMethod = _NO_TRANSLATE_truncatePathMethod.Text;
            Settings.Default.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.Checked;

            if ((int)_NO_TRANSLATE_authorImageSize.Value != Settings.Default.AuthorImageSize)
            {
                Settings.Default.AuthorImageSize = (int)_NO_TRANSLATE_authorImageSize.Value;
                GravatarService.ClearImageCache();
            }

            Settings.Default.Translation = Language.Text;
            Strings.Reinit();

            Settings.Default.AuthorImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            Settings.Default.ShowAuthorGravatar = ShowAuthorGravatar.Checked;
            Settings.Default.GravatarFallbackService = noImageService.Text;

            Settings.Default.RelativeDate = chkShowRelativeDate.Checked;

            Settings.Default.Dictionary = Dictionary.Text;

            Settings.Default.DiffFont = _diffFont;
            Settings.Default.Font = _applicationFont;
            Settings.Default.CommitFont = commitFont;
        }

        private void Dictionary_DropDown(object sender, EventArgs e)
        {
            try
            {
                Dictionary.Items.Clear();
                Dictionary.Items.Add("None");
                foreach (
                    string fileName in
                        Directory.GetFiles(Settings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);
                    Dictionary.Items.Add(file.Name.Replace(".dic", ""));
                }
            }
            catch
            {
                MessageBox.Show(this, String.Format(_noDictFilesFound.Text, Settings.GetDictionaryDir()));
            }
        }

        private void diffFontChangeButton_Click(object sender, EventArgs e)
        {
            diffFontDialog.Font = _diffFont;
            DialogResult result = diffFontDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentDiffFont(diffFontDialog.Font);
            }
        }

        private void applicationFontChangeButton_Click(object sender, EventArgs e)
        {
            applicationDialog.Font = _applicationFont;
            DialogResult result = applicationDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentApplicationFont(applicationDialog.Font);
            }
        }

        private void commitFontChangeButton_Click(object sender, EventArgs e)
        {
            commitFontDialog.Font = commitFont;
            DialogResult result = commitFontDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentCommitFont(commitFontDialog.Font);
            }
        }

        private void SetCurrentDiffFont(Font newFont)
        {
            this._diffFont = newFont;
            SetFontButtonText(newFont, diffFontChangeButton);
        }

        private void SetCurrentApplicationFont(Font newFont)
        {
            this._applicationFont = newFont;
            SetFontButtonText(newFont, applicationFontChangeButton);
        }

        private void SetCurrentCommitFont(Font newFont)
        {
            this.commitFont = newFont;
            SetFontButtonText(newFont, commitFontChangeButton);
        }

        private void SetFontButtonText(Font font, Button button)
        {
            button.Text = string.Format("{0}, {1}", font.FontFamily.Name, (int)(font.Size + 0.5f));
        }

        private void ClearImageCache_Click(object sender, EventArgs e)
        {
            GravatarService.ClearImageCache();
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

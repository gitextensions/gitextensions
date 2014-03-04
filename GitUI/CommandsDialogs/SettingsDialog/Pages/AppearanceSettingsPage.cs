using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using Gravatar;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AppearanceSettingsPage : SettingsPageWithHeader
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

            NoImageService.Items.AddRange(GravatarService.DynamicServices.Cast<object>().ToArray());
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

        protected override void SettingsToPage()
        {
            chkEnableAutoScale.Checked = AppSettings.EnableAutoScale;

            chkShowCurrentBranchInVisualStudio.Checked = AppSettings.ShowCurrentBranchInVisualStudio;
            _NO_TRANSLATE_DaysToCacheImages.Value = AppSettings.AuthorImageCacheDays;
            if (AppSettings.AuthorImageSize <= 120)
                AuthorImageSize.SelectedIndex = 0;
            else if (AppSettings.AuthorImageSize <= 200)
                AuthorImageSize.SelectedIndex = 1;
            else if (AppSettings.AuthorImageSize <= 280)
                AuthorImageSize.SelectedIndex = 2;
            else
                AuthorImageSize.SelectedIndex = 3;
            ShowAuthorGravatar.Checked = AppSettings.ShowAuthorGravatar;
            NoImageService.Text = AppSettings.GravatarFallbackService;

            Language.Items.Clear();
            Language.Items.Add("English");
            Language.Items.AddRange(Translator.GetAllTranslations());
            Language.Text = AppSettings.Translation;

            _NO_TRANSLATE_truncatePathMethod.Text = AppSettings.TruncatePathMethod;

            Dictionary.Text = AppSettings.Dictionary;

            chkShowRelativeDate.Checked = AppSettings.RelativeDate;

            SetCurrentApplicationFont(AppSettings.Font);
            SetCurrentDiffFont(AppSettings.DiffFont);
            SetCurrentCommitFont(AppSettings.CommitFont);

        }

        protected override void PageToSettings()
        {
            AppSettings.EnableAutoScale = chkEnableAutoScale.Checked;
            AppSettings.TruncatePathMethod = _NO_TRANSLATE_truncatePathMethod.Text;
            AppSettings.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.Checked;

            int authorImageSize;
            switch (AuthorImageSize.SelectedIndex)
            {
                case 1:
                    authorImageSize = 160;
                    break;
                case 2:
                    authorImageSize = 240;
                    break;
                case 3:
                    authorImageSize = 320;
                    break;
                default:
                    authorImageSize = 80;
                    break;
            }
            if (authorImageSize != AppSettings.AuthorImageSize)
            {
                AppSettings.AuthorImageSize = authorImageSize;
                GravatarService.ClearImageCache();
            }

            AppSettings.Translation = Language.Text;
            Strings.Reinit();

            AppSettings.AuthorImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            AppSettings.ShowAuthorGravatar = ShowAuthorGravatar.Checked;
            AppSettings.GravatarFallbackService = NoImageService.Text;

            AppSettings.RelativeDate = chkShowRelativeDate.Checked;

            AppSettings.Dictionary = Dictionary.Text;

            AppSettings.DiffFont = _diffFont;
            AppSettings.Font = _applicationFont;
            AppSettings.CommitFont = commitFont;
        }

        private void Dictionary_DropDown(object sender, EventArgs e)
        {
            try
            {
                Dictionary.Items.Clear();
                Dictionary.Items.Add("None");
                foreach (
                    string fileName in
                        Directory.GetFiles(AppSettings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);
                    Dictionary.Items.Add(file.Name.Replace(".dic", ""));
                }
            }
            catch
            {
                MessageBox.Show(this, String.Format(_noDictFilesFound.Text, AppSettings.GetDictionaryDir()));
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

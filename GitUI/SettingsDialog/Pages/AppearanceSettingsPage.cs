using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using Gravatar;
using System.IO;
using System.Diagnostics;

namespace GitUI.SettingsDialog.Pages
{
    public partial class AppearanceSettingsPage : SettingsPageBase
    {
        private readonly TranslationString _noDictFilesFound =
            new TranslationString("No dictionary files found in: {0}");

        private Font _diffFont;
        private Font _applicationFont;

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
            chkEnableAutoScale.Checked = Settings.EnableAutoScale;

            chkShowCurrentBranchInVisualStudio.Checked = Settings.ShowCurrentBranchInVisualStudio;
            _NO_TRANSLATE_DaysToCacheImages.Value = Settings.AuthorImageCacheDays;
            _NO_TRANSLATE_authorImageSize.Value = Settings.AuthorImageSize;
            ShowAuthorGravatar.Checked = Settings.ShowAuthorGravatar;
            noImageService.Text = Settings.GravatarFallbackService;

            Language.Items.Clear();
            Language.Items.Add("English");
            Language.Items.AddRange(Translator.GetAllTranslations());
            Language.Text = Settings.Translation;

            _NO_TRANSLATE_truncatePathMethod.Text = Settings.TruncatePathMethod;

            Dictionary.Text = Settings.Dictionary;

            chkShowRelativeDate.Checked = Settings.RelativeDate;

            SetCurrentDiffFont(Settings.Font, Settings.DiffFont);
        }

        public override void SaveSettings()
        {
            Settings.EnableAutoScale = chkEnableAutoScale.Checked;
            Settings.TruncatePathMethod = _NO_TRANSLATE_truncatePathMethod.Text;
            Settings.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.Checked;

            if ((int)_NO_TRANSLATE_authorImageSize.Value != Settings.AuthorImageSize)
            {
                Settings.AuthorImageSize = (int)_NO_TRANSLATE_authorImageSize.Value;
                GravatarService.ClearImageCache();
            }

            Settings.Translation = Language.Text;
            Strings.Reinit();

            Settings.AuthorImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            Settings.ShowAuthorGravatar = ShowAuthorGravatar.Checked;
            Settings.GravatarFallbackService = noImageService.Text;

            Settings.RelativeDate = chkShowRelativeDate.Checked;

            Settings.Dictionary = Dictionary.Text;

            Settings.DiffFont = _diffFont;
            Settings.Font = _applicationFont;
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
                SetCurrentDiffFont(_applicationFont, diffFontDialog.Font);
            }
        }

        private void applicationFontChangeButton_Click(object sender, EventArgs e)
        {
            applicationDialog.Font = _applicationFont;
            DialogResult result = applicationDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentDiffFont(applicationDialog.Font, _diffFont);
            }
        }

        private void SetCurrentDiffFont(Font applicationFont, Font diffFont)
        {
            this._diffFont = diffFont;
            this._applicationFont = applicationFont;

            diffFontChangeButton.Text =
                string.Format("{0}, {1}", this._diffFont.FontFamily.Name, (int)(this._diffFont.Size + 0.5f));
            applicationFontChangeButton.Text =
                string.Format("{0}, {1}", this._applicationFont.FontFamily.Name, (int)(this._applicationFont.Size + 0.5f));
        }

        private void ClearImageCache_Click(object sender, EventArgs e)
        {
            GravatarService.ClearImageCache();
        }

        private void helpTranslate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var frm = new FormTranslate()) frm.ShowDialog(this);
        }

        private void downloadDictionary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/gitextensions/gitextensions/wiki/Spelling");
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AppearanceFontsSettingsPage : SettingsPageWithHeader
    {
        private Font _diffFont;
        private Font _applicationFont;
        private Font _commitFont;
        private Font _monospaceFont;

        public AppearanceFontsSettingsPage()
        {
            InitializeComponent();
            Text = "Fonts";
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            SetCurrentApplicationFont(AppSettings.Font);
            SetCurrentDiffFont(AppSettings.FixedWidthFont);
            SetCurrentCommitFont(AppSettings.CommitFont);
            SetCurrentMonospaceFont(AppSettings.MonospaceFont);
        }

        protected override void PageToSettings()
        {
            AppSettings.FixedWidthFont = _diffFont;
            AppSettings.Font = _applicationFont;
            AppSettings.CommitFont = _commitFont;
            AppSettings.MonospaceFont = _monospaceFont;
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
            commitFontDialog.Font = _commitFont;
            DialogResult result = commitFontDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentCommitFont(commitFontDialog.Font);
            }
        }

        private void monospaceFontChangeButton_Click(object sender, EventArgs e)
        {
            monospaceFontDialog.Font = _monospaceFont;
            DialogResult result = monospaceFontDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentMonospaceFont(monospaceFontDialog.Font);
            }
        }

        private void SetCurrentDiffFont(Font newFont)
        {
            _diffFont = newFont;
            SetFontButtonText(newFont, diffFontChangeButton);
        }

        private void SetCurrentApplicationFont(Font newFont)
        {
            _applicationFont = newFont;
            SetFontButtonText(newFont, applicationFontChangeButton);
        }

        private void SetCurrentCommitFont(Font newFont)
        {
            _commitFont = newFont;
            SetFontButtonText(newFont, commitFontChangeButton);
        }

        private void SetCurrentMonospaceFont(Font newFont)
        {
            _monospaceFont = newFont;
            SetFontButtonText(newFont, monospaceFontChangeButton);
        }

        private static void SetFontButtonText(Font font, Button button)
        {
            button.Text = string.Format("{0}, {1}", font.FontFamily.Name, (int)(font.Size + 0.5f));
            button.Font = font;
        }
    }
}

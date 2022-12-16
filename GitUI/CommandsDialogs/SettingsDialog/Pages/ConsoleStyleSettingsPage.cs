using GitCommands;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ConsoleStyleSettingsPage : SettingsPageWithHeader
    {
        private Font? _consoleFont;

        public ConsoleStyleSettingsPage()
        {
            InitializeComponent();
            Text = "Console style";
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            // Bind settings with controls
            AddSettingBinding(AppSettings.ConEmuStyle, _NO_TRANSLATE_cboStyle);
            SetCurrentConsoleFont(AppSettings.ConEmuConsoleFont);

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            Validates.NotNull(_consoleFont);
            AppSettings.ConEmuConsoleFont = _consoleFont;

            base.PageToSettings();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ConsoleStyleSettingsPage));
        }

        private void consoleFontChangeButton_Click(object sender, System.EventArgs e)
        {
            consoleFontDialog.Font = _consoleFont;
            DialogResult result = consoleFontDialog.ShowDialog(this);

            if (result is (DialogResult.OK or DialogResult.Yes))
            {
                Validates.NotNull(consoleFontDialog.Font);
                SetCurrentConsoleFont(consoleFontDialog.Font);
            }
        }

        private void SetCurrentConsoleFont(Font newFont)
        {
            _consoleFont = newFont;
            SetFontButtonText(newFont, consoleFontChangeButton);
        }

        private static void SetFontButtonText(Font font, Button button)
        {
            button.Text = string.Format("{0}, {1}", font.FontFamily.Name, (int)(font.Size + 0.5f));
            button.Font = font;
        }
    }
}

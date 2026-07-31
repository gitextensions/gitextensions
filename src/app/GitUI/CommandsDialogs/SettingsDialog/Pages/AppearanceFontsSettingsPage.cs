using GitCommands;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AppearanceFontsSettingsPage : SettingsPageWithHeader
{
    private Font? _diffFont;
    private Font? _applicationFont;
    private Font? _commitFont;
    private Font? _monospaceFont;
    private Font? _menuFont;

    public AppearanceFontsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        SetCurrentApplicationFont(AppSettings.Font);
        SetCurrentDiffFont(AppSettings.FixedWidthFont);
        SetCurrentCommitFont(AppSettings.CommitFont);
        SetCurrentMonospaceFont(AppSettings.MonospaceFont);
        SetCurrentMenuFont(AppSettings.MenuFont);

        ShowEolMarkerAsGlyph.Checked = AppSettings.ShowEolMarkerAsGlyph;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        Validates.NotNull(_diffFont);
        Validates.NotNull(_applicationFont);
        Validates.NotNull(_commitFont);
        Validates.NotNull(_monospaceFont);
        Validates.NotNull(_menuFont);

        AppSettings.FixedWidthFont = _diffFont;
        AppSettings.Font = _applicationFont;
        AppSettings.CommitFont = _commitFont;
        AppSettings.MonospaceFont = _monospaceFont;
        AppSettings.MenuFont = _menuFont;

        AppSettings.ShowEolMarkerAsGlyph = ShowEolMarkerAsGlyph.Checked;

        base.PageToSettings();
    }

    private void diffFontChangeButton_Click(object sender, EventArgs e)
    {
        diffFontDialog.Font = _diffFont!;
        if (ShowFontDialog(diffFontDialog))
        {
            SetCurrentDiffFont(diffFontDialog.Font);
        }
    }

    private void applicationFontChangeButton_Click(object sender, EventArgs e)
    {
        applicationDialog.Font = _applicationFont!;
        if (ShowFontDialog(applicationDialog))
        {
            SetCurrentApplicationFont(applicationDialog.Font);
        }
    }

    private void commitFontChangeButton_Click(object sender, EventArgs e)
    {
        commitFontDialog.Font = _commitFont!;
        if (ShowFontDialog(commitFontDialog))
        {
            SetCurrentCommitFont(commitFontDialog.Font);
        }
    }

    private void monospaceFontChangeButton_Click(object sender, EventArgs e)
    {
        monospaceFontDialog.Font = _monospaceFont!;
        if (ShowFontDialog(monospaceFontDialog))
        {
            SetCurrentMonospaceFont(monospaceFontDialog.Font);
        }
    }

    private bool ShowFontDialog(FontDialog fontDialog)
    {
        try
        {
            DialogResult result = fontDialog.ShowDialog(this);
            if (result is (DialogResult.OK or DialogResult.Yes))
            {
                Validates.NotNull(fontDialog.Font);
                return true;
            }
        }
        catch (ArgumentException ex)
        {
            MessageBoxes.ShowError(this, ex.Message);
        }

        return false;
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

    private void menuFontChangeButton_Click(object sender, EventArgs e)
    {
        menuFontDialog.Font = _menuFont!;
        DialogResult result = menuFontDialog.ShowDialog(this);

        if (result is (DialogResult.OK or DialogResult.Yes))
        {
            Validates.NotNull(menuFontDialog.Font);
            SetCurrentMenuFont(menuFontDialog.Font);
        }
    }

    private void SetCurrentMenuFont(Font newFont)
    {
        _menuFont = newFont;
        SetFontButtonText(newFont, menuFontChangeButton);
    }

    private static void SetFontButtonText(Font font, Button button)
    {
        button.Text = string.Format("{0}, {1}", font.FontFamily.Name, (int)(font.Size + 0.5f));
        button.Font = font;
    }
}

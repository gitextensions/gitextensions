using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI.ConsoleEmulation;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ConsoleStyleSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _defaultThemeDisplayName = new("Default");
    private readonly TranslationString _consoleDefaultFontText = new("Console Default");

    private Font? _consoleFont;

    public ConsoleStyleSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();

        cboConsoleEmulator.DisplayMember = nameof(IConsoleEmulator.DisplayName);
        cboConsoleEmulator.ValueMember = nameof(IConsoleEmulator.Name);

        IReadOnlyCollection<IConsoleEmulator> emulators = serviceProvider.GetRequiredService<IConsoleEmulatorsRegistry>().AvailableConsoleEmulators;
        foreach (IConsoleEmulator emulator in emulators)
        {
            cboConsoleEmulator.Items.Add(emulator);
        }

        cboConsoleEmulator.SelectedIndexChanged += (_, _) => RefreshThemeDropdown();

        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        cboConsoleEmulator.SelectedItem = cboConsoleEmulator.Items
            .OfType<IConsoleEmulator>()
            .FirstOrDefault(e => string.Equals(e.Name, AppSettings.ConsoleEmulatorName.Value, StringComparison.OrdinalIgnoreCase));
        if (cboConsoleEmulator.SelectedIndex < 0 && cboConsoleEmulator.Items.Count > 0)
        {
            cboConsoleEmulator.SelectedIndex = 0;
        }

        RefreshThemeDropdown();
        SetCurrentConsoleFont(AppSettings.ConEmuConsoleFont);

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.ConsoleEmulatorName.Value = (cboConsoleEmulator.SelectedItem as IConsoleEmulator)?.Name ?? "";
        AppSettings.ConEmuStyle.Value = (string)_NO_TRANSLATE_cboStyle.SelectedItem!;
        AppSettings.ConEmuConsoleFont = _consoleFont;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(ConsoleStyleSettingsPage));
    }

    private void RefreshThemeDropdown()
    {
        _NO_TRANSLATE_cboStyle.Items.Clear();

        if (cboConsoleEmulator.SelectedItem is not IConsoleEmulator emulator)
        {
            _NO_TRANSLATE_cboStyle.Enabled = false;
            return;
        }

        _NO_TRANSLATE_cboStyle.Items.Add(_defaultThemeDisplayName.Text);
        foreach (string theme in emulator.AvailableThemes)
        {
            _NO_TRANSLATE_cboStyle.Items.Add(theme);
        }

        _NO_TRANSLATE_cboStyle.Enabled = emulator.AvailableThemes.Count > 0;

        string? saved = AppSettings.ConEmuStyle.Value;
        int matchIndex = string.IsNullOrEmpty(saved)
            ? 0
            : FindThemeIndex(saved);
        _NO_TRANSLATE_cboStyle.SelectedIndex = matchIndex >= 0 ? matchIndex : 0;
        return;

        int FindThemeIndex(string theme)
        {
            int index = 1; // skip "Default"
            foreach (string available in emulator.AvailableThemes)
            {
                if (string.Equals(available, theme, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }

    private void consoleFontResetButton_Click(object sender, EventArgs e)
    {
        SetCurrentConsoleFont(null);
    }

    private void consoleFontChangeButton_Click(object sender, EventArgs e)
    {
        consoleFontDialog.Font = _consoleFont ?? new Font("Consolas", 12);
        DialogResult result = consoleFontDialog.ShowDialog(this);

        if (result is (DialogResult.OK or DialogResult.Yes))
        {
            Validates.NotNull(consoleFontDialog.Font);
            SetCurrentConsoleFont(consoleFontDialog.Font);
        }
    }

    private void SetCurrentConsoleFont(Font? font)
    {
        _consoleFont = font;
        consoleFontResetButton.Visible = font is not null;
        if (font is null)
        {
            consoleFontChangeButton.Text = _consoleDefaultFontText.Text;
            consoleFontChangeButton.ResetFont();
        }
        else
        {
            consoleFontChangeButton.Text = $"{font.FontFamily.Name}, {(int)(font.Size + 0.5f)}";
            consoleFontChangeButton.Font = font;
        }
    }
}

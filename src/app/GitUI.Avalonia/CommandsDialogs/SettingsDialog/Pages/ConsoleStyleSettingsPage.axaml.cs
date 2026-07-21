using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ConsoleStyleSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _defaultThemeDisplayName = new("Default");
    private readonly TranslationString _consoleDefaultFontText = new("Console Default");

    private WinFormsShims.Font? _consoleFont;
#pragma warning disable SX1309 // Preserve the original designer field name for port parity.
    private readonly FontDialog consoleFontDialog = new();
#pragma warning restore SX1309

    public ConsoleStyleSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ConsoleStyleSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();

        cboConsoleEmulator.ItemTemplate = new FuncDataTemplate<IConsoleEmulator>(
            (emulator, _) => new TextBlock { Text = emulator.DisplayName });
        IConsoleEmulatorsRegistry registry = serviceProvider.GetService(typeof(IConsoleEmulatorsRegistry))
            as IConsoleEmulatorsRegistry
            ?? PlainTextConsoleEmulatorsRegistry.Instance;
        foreach (IConsoleEmulator emulator in registry.AvailableConsoleEmulators)
        {
            cboConsoleEmulator.Items.Add(emulator);
        }

        WireEvents();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ConsoleStyleSettingsPage));

    protected override void SettingsToPage()
    {
        cboConsoleEmulator.SelectedItem = cboConsoleEmulator.Items
            .OfType<IConsoleEmulator>()
            .FirstOrDefault(emulator => string.Equals(
                emulator.Name,
                AppSettings.ConsoleEmulatorName.Value,
                StringComparison.OrdinalIgnoreCase));
        if (cboConsoleEmulator.SelectedIndex < 0 && cboConsoleEmulator.ItemCount > 0)
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
        AppSettings.ConEmuStyle.Value = _NO_TRANSLATE_cboStyle.SelectedItem as string
            ?? _defaultThemeDisplayName.Text;
        AppSettings.ConEmuConsoleFont = _consoleFont;

        base.PageToSettings();
    }

    private void WireEvents()
    {
        cboConsoleEmulator.SelectionChanged += (_, _) => RefreshThemeDropdown();
        consoleFontResetButton.Click += consoleFontResetButton_Click;
        consoleFontChangeButton.Click += consoleFontChangeButton_Click;
    }

    private void RefreshThemeDropdown()
    {
        _NO_TRANSLATE_cboStyle.Items.Clear();

        if (cboConsoleEmulator.SelectedItem is not IConsoleEmulator emulator)
        {
            _NO_TRANSLATE_cboStyle.IsEnabled = false;
            return;
        }

        _NO_TRANSLATE_cboStyle.Items.Add(_defaultThemeDisplayName.Text);
        foreach (string theme in emulator.AvailableThemes)
        {
            _NO_TRANSLATE_cboStyle.Items.Add(theme);
        }

        _NO_TRANSLATE_cboStyle.IsEnabled = emulator.AvailableThemes.Count > 0;

        string? saved = AppSettings.ConEmuStyle.Value;
        int matchIndex = string.IsNullOrEmpty(saved) ? 0 : FindThemeIndex(saved);
        _NO_TRANSLATE_cboStyle.SelectedIndex = matchIndex >= 0 ? matchIndex : 0;
        return;

        int FindThemeIndex(string theme)
        {
            int index = 1;
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

    private void consoleFontResetButton_Click(object? sender, EventArgs e)
        => SetCurrentConsoleFont(null);

    private void consoleFontChangeButton_Click(object? sender, EventArgs e)
    {
        consoleFontDialog.Font = _consoleFont ?? new WinFormsShims.Font("Consolas", 12);
        consoleFontDialog.Text = GetTitle();
        WinFormsShims.DialogResult result = consoleFontDialog.ShowDialog(
            TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window);

        if (result is WinFormsShims.DialogResult.OK or WinFormsShims.DialogResult.Yes)
        {
            SetCurrentConsoleFont(consoleFontDialog.Font);
        }
    }

    private void SetCurrentConsoleFont(WinFormsShims.Font? font)
    {
        _consoleFont = font;
        consoleFontResetButton.IsVisible = font is not null;
        if (font is null)
        {
            consoleFontChangeButton.Content = _consoleDefaultFontText.Text;
            consoleFontChangeButton.ClearValue(TemplatedControl.FontFamilyProperty);
            consoleFontChangeButton.ClearValue(TemplatedControl.FontSizeProperty);
            consoleFontChangeButton.ClearValue(TemplatedControl.FontStyleProperty);
            consoleFontChangeButton.ClearValue(TemplatedControl.FontWeightProperty);
            return;
        }

        consoleFontChangeButton.Content = $"{font.FontFamily.Name}, {(int)(font.Size + 0.5F)}";
        consoleFontChangeButton.FontFamily = new FontFamily(font.Name);
        consoleFontChangeButton.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(font.Size);
        consoleFontChangeButton.FontStyle = font.Italic ? FontStyle.Italic : FontStyle.Normal;
        consoleFontChangeButton.FontWeight = font.Bold ? FontWeight.Bold : FontWeight.Normal;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ConsoleStyleSettingsPage page)
    {
        public ComboBox ConsoleEmulator => page.cboConsoleEmulator;

        public ComboBox ConsoleStyle => page._NO_TRANSLATE_cboStyle;

        public Button ConsoleFont => page.consoleFontChangeButton;

        public Button ConsoleFontReset => page.consoleFontResetButton;

        public void SetConsoleFont(WinFormsShims.Font? font) => page.SetCurrentConsoleFont(font);
    }
}

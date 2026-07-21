using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AppearanceFontsSettingsPage : SettingsPageWithHeader
{
    private WinFormsShims.Font? _diffFont;
    private WinFormsShims.Font? _applicationFont;
    private WinFormsShims.Font? _commitFont;
    private WinFormsShims.Font? _monospaceFont;
#pragma warning disable SX1309 // Preserve the original designer field names for port parity.
    private readonly FontDialog diffFontDialog = new() { FixedPitchOnly = true };
    private readonly FontDialog applicationDialog = new();
    private readonly FontDialog monospaceFontDialog = new();
    private readonly FontDialog commitFontDialog = new();
#pragma warning restore SX1309

    public AppearanceFontsSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public AppearanceFontsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(AppearanceFontsSettingsPage));

    protected override void SettingsToPage()
    {
        SetCurrentApplicationFont(AppSettings.Font);
        SetCurrentDiffFont(AppSettings.FixedWidthFont);
        SetCurrentCommitFont(AppSettings.CommitFont);
        SetCurrentMonospaceFont(AppSettings.MonospaceFont);
        ShowEolMarkerAsGlyph.IsChecked = AppSettings.ShowEolMarkerAsGlyph;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.FixedWidthFont = _diffFont
            ?? throw new InvalidOperationException("The code font was not loaded.");
        AppSettings.Font = _applicationFont
            ?? throw new InvalidOperationException("The application font was not loaded.");
        AppSettings.CommitFont = _commitFont
            ?? throw new InvalidOperationException("The commit font was not loaded.");
        AppSettings.MonospaceFont = _monospaceFont
            ?? throw new InvalidOperationException("The monospace font was not loaded.");
        AppSettings.ShowEolMarkerAsGlyph = ShowEolMarkerAsGlyph.IsChecked == true;

        base.PageToSettings();
    }

    private void diffFontChangeButton_Click(object? sender, EventArgs e)
    {
        diffFontDialog.Font = _diffFont
            ?? throw new InvalidOperationException("The code font was not loaded.");
        if (ShowFontDialog(diffFontDialog))
        {
            SetCurrentDiffFont(diffFontDialog.Font);
        }
    }

    private void applicationFontChangeButton_Click(object? sender, EventArgs e)
    {
        applicationDialog.Font = _applicationFont
            ?? throw new InvalidOperationException("The application font was not loaded.");
        if (ShowFontDialog(applicationDialog))
        {
            SetCurrentApplicationFont(applicationDialog.Font);
        }
    }

    private void commitFontChangeButton_Click(object? sender, EventArgs e)
    {
        commitFontDialog.Font = _commitFont
            ?? throw new InvalidOperationException("The commit font was not loaded.");
        if (ShowFontDialog(commitFontDialog))
        {
            SetCurrentCommitFont(commitFontDialog.Font);
        }
    }

    private void monospaceFontChangeButton_Click(object? sender, EventArgs e)
    {
        monospaceFontDialog.Font = _monospaceFont
            ?? throw new InvalidOperationException("The monospace font was not loaded.");
        if (ShowFontDialog(monospaceFontDialog))
        {
            SetCurrentMonospaceFont(monospaceFontDialog.Font);
        }
    }

    private bool ShowFontDialog(FontDialog fontDialog)
    {
        fontDialog.Text = GetTitle();
        WinFormsShims.DialogResult result = fontDialog.ShowDialog(
            TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window);
        return result is WinFormsShims.DialogResult.OK or WinFormsShims.DialogResult.Yes;
    }

    private void SetCurrentDiffFont(WinFormsShims.Font newFont)
    {
        _diffFont = newFont;
        SetFontButtonText(newFont, diffFontChangeButton);
    }

    private void SetCurrentApplicationFont(WinFormsShims.Font newFont)
    {
        _applicationFont = newFont;
        SetFontButtonText(newFont, applicationFontChangeButton);
    }

    private void SetCurrentCommitFont(WinFormsShims.Font newFont)
    {
        _commitFont = newFont;
        SetFontButtonText(newFont, commitFontChangeButton);
    }

    private void SetCurrentMonospaceFont(WinFormsShims.Font newFont)
    {
        _monospaceFont = newFont;
        SetFontButtonText(newFont, monospaceFontChangeButton);
    }

    private static void SetFontButtonText(WinFormsShims.Font font, Button button)
    {
        button.Content = $"{font.FontFamily.Name}, {(int)(font.Size + 0.5F)}";
        button.FontFamily = new FontFamily(font.Name);
        button.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(font.Size);
        button.FontStyle = font.Italic ? FontStyle.Italic : FontStyle.Normal;
        button.FontWeight = font.Bold ? FontWeight.Bold : FontWeight.Normal;
    }

    private void WireEvents()
    {
        diffFontChangeButton.Click += diffFontChangeButton_Click;
        applicationFontChangeButton.Click += applicationFontChangeButton_Click;
        commitFontChangeButton.Click += commitFontChangeButton_Click;
        monospaceFontChangeButton.Click += monospaceFontChangeButton_Click;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(AppearanceFontsSettingsPage page)
    {
        public Button DiffFont => page.diffFontChangeButton;

        public Button ApplicationFont => page.applicationFontChangeButton;

        public Button CommitFont => page.commitFontChangeButton;

        public Button MonospaceFont => page.monospaceFontChangeButton;

        public CheckBox ShowEolMarkerAsGlyph => page.ShowEolMarkerAsGlyph;

        public bool DiffDialogFixedPitchOnly => page.diffFontDialog.FixedPitchOnly;

        public void SetFonts(
            WinFormsShims.Font diff,
            WinFormsShims.Font application,
            WinFormsShims.Font commit,
            WinFormsShims.Font monospace)
        {
            page.SetCurrentDiffFont(diff);
            page.SetCurrentApplicationFont(application);
            page.SetCurrentCommitFont(commit);
            page.SetCurrentMonospaceFont(monospace);
        }
    }
}

using GitCommands;
using GitExtensions.Extensibility.Translations;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Reduced twin: generic overloads forward to the shared approved wrapper; domain-specific
// translated helpers join this class as their Avalonia consumers are ported.
public class MessageBoxes : Translate
{
    private readonly TranslationString _failedToRunShell = new("Failed to run shell");
    private readonly TranslationString _reason = new("Reason");

    internal MessageBoxes()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    private static MessageBoxes Instance => field ??= new();

    public static void FailedToRunShell(WinFormsShims.IWin32Window? owner, string shell, Exception ex)
        => ShowError(owner, $"{Instance._failedToRunShell.Text} {shell.Quote()}.{Environment.NewLine}"
                            + $"{Instance._reason.Text}: {ex.Message}");

    public static void ShowError(WinFormsShims.IWin32Window? owner, string text, string? caption = null)
        => GitExtensions.Extensibility.MessageBoxes.ShowError(owner, text, caption);

    public static WinFormsShims.DialogResult Show(
        WinFormsShims.IWin32Window? owner,
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons,
        WinFormsShims.MessageBoxIcon icon,
        WinFormsShims.MessageBoxDefaultButton defaultButton = WinFormsShims.MessageBoxDefaultButton.Button1)
        => GitExtensions.Extensibility.MessageBoxes.Show(owner, text, caption, buttons, icon, defaultButton);

    public static WinFormsShims.DialogResult Show(
        WinFormsShims.IWin32Window? owner,
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons)
        => GitExtensions.Extensibility.MessageBoxes.Show(owner, text, caption, buttons);

    public static WinFormsShims.DialogResult Show(
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons,
        WinFormsShims.MessageBoxIcon icon,
        WinFormsShims.MessageBoxDefaultButton defaultButton)
        => GitExtensions.Extensibility.MessageBoxes.Show(text, caption, buttons, icon, defaultButton);

    public static WinFormsShims.DialogResult Show(
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons,
        WinFormsShims.MessageBoxIcon icon)
        => GitExtensions.Extensibility.MessageBoxes.Show(text, caption, buttons, icon);

    public static WinFormsShims.DialogResult Show(
        string text,
        string caption,
        WinFormsShims.MessageBoxButtons buttons)
        => GitExtensions.Extensibility.MessageBoxes.Show(text, caption, buttons);
}

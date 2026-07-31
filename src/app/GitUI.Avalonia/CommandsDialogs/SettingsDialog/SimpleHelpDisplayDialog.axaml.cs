using Avalonia.Controls;
using GitUI.Compat;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog;

// The original is a plain Form whose title and content are supplied by its caller.
[Untranslated]
public sealed partial class SimpleHelpDisplayDialog : GitExtensionsFormBase
{
    public SimpleHelpDisplayDialog()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public string? DialogTitle { get; set; }

    public string? ContentText { get; set; }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Title = DialogTitle ?? string.Empty;
        textBox1.Text = ContentText;
        textBox1.CaretIndex = 0;
    }

    internal TextBox ContentTextBox => textBox1;
}

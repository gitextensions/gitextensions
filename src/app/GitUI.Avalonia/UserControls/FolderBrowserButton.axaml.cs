using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Translations.Xliff;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.UserControls;

// Twin of GitUI/UserControls/FolderBrowserButton.cs. PathShowingControl retains the
// original any-Control boundary for the TextBox and ComboBox consumers in ported dialogs.
[LocalizableProperties]
public partial class FolderBrowserButton : GitExtensionsControl
{
    private string _text = string.Empty;

    public FolderBrowserButton()
    {
        InitializeComponent();
        buttonBrowse.Click += buttonBrowse_Click;
        InitializeComplete();
    }

    /// <summary>
    /// The Text property of this control will be filled with the selected path
    /// and the Text property is used as path to initialize the folder browser's default selection.
    /// </summary>
    public Control? PathShowingControl { get; set; }

    // Avalonia buttons do not bubble Click through a UserControl; forward it so original
    // host forms can retain their Designer-wired handler instead of opening two pickers.
    public event EventHandler? Click;

    /// <summary>
    /// Gets or sets the host-form text using the original WinForms mnemonic syntax.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            buttonBrowse.Content = GitUI.Compat.AvaloniaTranslationUtils.ToAvaloniaMnemonics(value);
        }
    }

    /// <summary>
    /// Opens a a folder picker dialog with the path in "getter" preselected and
    /// if OK is returned uses "setter" to set the path.
    /// </summary>
    public void ShowFolderBrowserDialogWithPreselectedPath(Func<string> getter, Action<string> setter)
    {
        string? directoryInfoPath = null;
        try
        {
            directoryInfoPath = new DirectoryInfo(getter()).FullName;
        }
        catch
        {
            // since the DirectoryInfo stuff is for convenience we swallow exceptions
        }

        // if we do not use the DirectoryInfo then a path with slashes instead of backslashes won't work
        directoryInfoPath ??= getter();

        string? userSelectedPath = OsShellUtil.PickFolder((TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window)!, directoryInfoPath);

        if (userSelectedPath is not null)
        {
            setter(userSelectedPath);
        }
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
        if (Click is not null)
        {
            Click(this, e);
            return;
        }

        Validates.NotNull(PathShowingControl);

        ShowFolderBrowserDialogWithPreselectedPath(
            () => PathShowingControl switch
            {
                ComboBox comboBox => comboBox.SelectedItem as string ?? comboBox.Text ?? string.Empty,
                TextBox textBox => textBox.Text ?? string.Empty,
                _ => throw new InvalidOperationException($"{PathShowingControl.GetType().Name} does not expose editable text."),
            },
            path =>
            {
                switch (PathShowingControl)
                {
                    case ComboBox comboBox:
                        comboBox.Text = path;
                        break;
                    case TextBox textBox:
                        textBox.Text = path;
                        break;
                    default:
                        throw new InvalidOperationException($"{PathShowingControl.GetType().Name} does not expose editable text.");
                }
            });
    }
}

using System.Diagnostics.CodeAnalysis;
using GitExtensions.Shims.WinForms;

namespace GitUI;

/// <summary>
/// A form that explains that the command needed authentication, and offers to load a private key.
/// </summary>
public partial class FormPuttyError : GitExtensionsForm
{
    /// <summary>Shows the "SSH error" dialog modally, and returns the path to the key, if one was loaded.</summary>
    public static bool AskForKey(IWin32Window parent, [NotNullWhen(returnValue: true)] out string? keyPath)
    {
        using FormPuttyError form = new();
        DialogResult result = form.ShowDialog(parent);
        keyPath = form.KeyPath;
        return result == DialogResult.Retry;
    }

    public string? KeyPath { get; private set; }

    public FormPuttyError()
    {
        InitializeComponent();
        LoadSSHKey.Click += LoadSSHKey_Click;
        Cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Retry.Click += (_, _) => DialogResult = DialogResult.Retry;
        InitializeComplete();
    }

    private void LoadSSHKey_Click(object? sender, EventArgs e)
    {
        string? pathLoaded = BrowseForPrivateKey.BrowseAndLoad(this);
        if (!string.IsNullOrEmpty(pathLoaded))
        {
            KeyPath = pathLoaded;
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}

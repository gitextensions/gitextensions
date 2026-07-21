using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitExtUtils;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.ScriptsEngine;

// Avalonia twin of GitUI/ScriptsEngine/FormFilePrompt.cs.
internal sealed partial class FormFilePrompt : GitExtensionsForm, IUserInputPrompt
{
    public string UserInput { get; private set; } = string.Empty;

    public FormFilePrompt()
    {
        InitializeComponent();
        AcceptButton = btnOk;
        btnBrowse.Click += btnBrowse_Click;
        btnOk.Click += btnOk_Click;
        InitializeComplete();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        txtFilePath.Focus();
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
        => this.InvokeAndForget(BrowseAsync);

    private async Task BrowseAsync()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = true,
            Title = Text,
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync("."),
        };
        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        string[] paths = [.. files.Select(file => file.TryGetLocalPath()).OfType<string>()];
        if (paths.Length > 0)
        {
            txtFilePath.Text = string.Join(' ', paths.Select(path => path.Quote()));
        }
    }

    private void btnOk_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtFilePath.Text))
        {
            UserInput = txtFilePath.Text;
            DialogResult = WinFormsShims.DialogResult.OK;
        }
        else
        {
            DialogResult = WinFormsShims.DialogResult.Cancel;
        }
    }
}

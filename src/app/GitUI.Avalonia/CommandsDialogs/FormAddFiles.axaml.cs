using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;

namespace GitUI.CommandsDialogs;

public sealed partial class FormAddFiles : GitModuleForm
{
    public FormAddFiles()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormAddFiles(IGitUICommands commands, string? addFile = null)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        ShowFiles.Click += ShowFilesClick;
        AddFiles.Click += AddFilesClick;
        Filter.Text = addFile ?? ".";
        AcceptButton = AddFiles;

        InitializeComplete();
    }

    private void ShowFilesClick(object? sender, EventArgs e)
    {
        string arguments = string.Format("add --dry-run{0} {1}", force.IsChecked == true ? " -f" : string.Empty, Filter.Text);
        FormProcess.ShowDialog(this, UICommands, arguments, Module.WorkingDir, input: null, useDialogSettings: false);
    }

    private void AddFilesClick(object? sender, EventArgs e)
    {
        string arguments = string.Format("add{0} {1}", force.IsChecked == true ? " -f" : string.Empty, Filter.Text);
        if (FormProcess.ShowDialog(this, UICommands, arguments, Module.WorkingDir, input: null, useDialogSettings: false))
        {
            Close();
        }
    }
}

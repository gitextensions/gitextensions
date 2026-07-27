using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensions.Plugins.CreateLocalBranches;

public partial class CreateLocalBranchesForm : ResourceManager.GitExtensionsFormBase
{
    private readonly GitUIEventArgs? _gitUiCommands;

    public CreateLocalBranchesForm()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public CreateLocalBranchesForm(GitUIEventArgs gitUiCommands)
    {
        _gitUiCommands = gitUiCommands;

        InitializeComponent();
        button1.Click += button1_Click;
        InitializeComplete();
    }

    private void button1_Click(object? sender, EventArgs e)
    {
        if (_gitUiCommands is null)
        {
            throw new InvalidOperationException($"{nameof(CreateLocalBranchesForm)} was constructed incorrectly.");
        }

        GitArgumentBuilder args = new("branch") { "-a" };
        string[] references = _gitUiCommands.GitModule.GitExecutable.GetOutput(args)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (references.Length == 0)
        {
            MessageBoxes.ShowError(this, "No remote branches found.");
            DialogResult = WinFormsShims.DialogResult.Cancel;
            return;
        }

        foreach (string reference in references)
        {
            try
            {
                string branchName = reference.Trim(Delimiters.GitOutput);

                if (branchName.StartsWith("remotes/" + _NO_TRANSLATE_Remote.Text + "/"))
                {
                    args = new GitArgumentBuilder("branch")
                    {
                        "--track",
                        branchName.Replace($"remotes/{_NO_TRANSLATE_Remote.Text}/", ""),
                        branchName
                    };
                    _gitUiCommands.GitModule.GitExecutable.GetOutput(args);
                }
            }
            catch
            {
            }
        }

        MessageBoxes.Show(this, string.Format("{0} local tracking branches have been created/updated.", references.Length),
            "Information", WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);
        Close();
    }
}

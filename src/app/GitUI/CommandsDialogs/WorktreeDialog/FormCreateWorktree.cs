using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitUI.CommandsDialogs.WorktreeDialog;

public sealed partial class FormCreateWorktree : GitExtensionsDialog
{
    private readonly AsyncLoader _branchesLoader = new();
    private readonly char[] _invalidCharsInPath = Path.GetInvalidFileNameChars();

    private readonly string? _initialDirectoryPath;

    public string WorktreeDirectory => txtWorktreeDirectory.Text;
    public bool OpenWorktree => chkOpenWorktree.Checked;

    public IReadOnlyList<IGitRef>? ExistingBranches { get; set; }

    public FormCreateWorktree(IGitUICommands commands, string? path)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        tlpnlMain.AdjustWidthToSize(0, rbCheckoutExistingBranch, rbCreateNewBranch, lblNewWorktreeFolder);
        tlpnlCheckout.AdjustWidthToSize(0, rbCheckoutExistingBranch, rbCreateNewBranch, lblNewWorktreeFolder);

        MinimumSize = new Size(Width, PreferredMinimumHeight);

        InitializeComplete();
        _initialDirectoryPath = path;
    }

    private void FormCreateWorktree_Load(object sender, EventArgs e)
    {
        LoadBranchesAsync();

        UpdateWorktreePathAndValidateWorktreeOptions();

        Task LoadBranchesAsync()
        {
            string selectedBranch = UICommands.Module.GetSelectedBranch();
            ExistingBranches = Module.GetRefs(RefsFilter.Heads);
            cbxBranches.Text = TranslatedStrings.LoadingData;
            ThreadHelper.FileAndForget(async () =>
            {
                await _branchesLoader.LoadAsync(
                    () => ExistingBranches.Where(r => r.Name != selectedBranch).ToList(),
                    list =>
                    {
                        cbxBranches.Text = string.Empty;
                        cbxBranches.DataSource = list;
                        cbxBranches.DisplayMember = nameof(IGitRef.LocalName);
                    });

                await this.SwitchToMainThreadAsync();
                if (cbxBranches.Items.Count == 0)
                {
                    rbCreateNewBranch.Checked = true;
                    rbCheckoutExistingBranch.Enabled = false;
                }
                else
                {
                    rbCheckoutExistingBranch.Checked = true;
                }

                ValidateWorktreeOptions();
            });

            return Task.CompletedTask;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _branchesLoader.Dispose();

            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void cbxBranches_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            CreateWorktree();
        }
    }

    private void btnCreateWorktree_Click(object sender, EventArgs e)
    {
        CreateWorktree();
    }

    private void CreateWorktree()
    {
        string relativePath = Path.GetRelativePath(Module.WorkingDir, WorktreeDirectory).ToPosixPath().Quote();
        string? newBranchOption =
            rbCreateNewBranch.Checked
            ? $"-b {txtNewBranchName.Text}"
            : (cbxBranches.SelectedItem as GitRef)?.Name;
        DialogResult = UICommands.StartGitCommandProcessDialog(this, CreateWorktreeCommand(Module, relativePath, newBranchOption!)) ? DialogResult.OK : DialogResult.None;
    }

    private GitArgumentBuilder CreateWorktreeCommand(IGitModule module, string relativePath, string newBranchOption)
    {
        // https://git-scm.com/docs/git-worktree

        // Get the default value, set if unset in config.
        // Similar in DiffHighlightService.
        const string command = "worktree";
        GitCommandConfiguration commandConfiguration = new();
        IReadOnlyList<GitConfigItem> items = GitCommandConfiguration.Default.Get(command);
        foreach (GitConfigItem cfg in items)
        {
            commandConfiguration.Add(cfg, command);
        }

        SetIfUnsetInGit("worktree.useRelativePaths", "true");
        GitArgumentBuilder args = new(command, commandConfiguration)
        {
            "add",
            relativePath,
            newBranchOption,
        };

        return args;

        void SetIfUnsetInGit(string key, string value)
        {
            if (string.IsNullOrEmpty(module.GetEffectiveSetting(key)))
            {
                commandConfiguration.Add(new GitConfigItem(key, value), command);
            }
        }
    }

    private void ValidateWorktreeOptions()
    {
        cbxBranches.Enabled = rbCheckoutExistingBranch.Checked;
        txtNewBranchName.Enabled = rbCreateNewBranch.Checked;
        if (rbCheckoutExistingBranch.Checked)
        {
            btnCreateWorktree.Enabled = cbxBranches.SelectedItem is not null;
        }
        else
        {
            btnCreateWorktree.Enabled = !(string.IsNullOrWhiteSpace(txtNewBranchName.Text)
                                             || ExistingBranches!.Any(b => b.Name == txtNewBranchName.Text));
        }

        if (btnCreateWorktree.Enabled)
        {
            btnCreateWorktree.Enabled = IsTargetFolderValid();
        }

        return;

        bool IsTargetFolderValid()
        {
            if (string.IsNullOrWhiteSpace(txtWorktreeDirectory.Text))
            {
                return false;
            }

            try
            {
                DirectoryInfo directoryInfo = new(txtWorktreeDirectory.Text);
                return !directoryInfo.Exists || (!directoryInfo.EnumerateFiles().Any() && !directoryInfo.EnumerateDirectories().Any());
            }
            catch
            {
                return false;
            }
        }
    }

    private void txtWorktreeDirectory_TextChanged(object sender, EventArgs e)
    {
        ValidateWorktreeOptions();
    }

    private void UpdateWorktreePathAndValidateWorktreeOptions(object sender, EventArgs e)
        => UpdateWorktreePathAndValidateWorktreeOptions();

    private void UpdateWorktreePathAndValidateWorktreeOptions()
    {
        UpdateWorktreePath();

        ValidateWorktreeOptions();

        return;

        void UpdateWorktreePath()
        {
            string branchNameNormalized = NormalizeBranchName(rbCheckoutExistingBranch.Checked
                ? ((IGitRef?)cbxBranches.SelectedItem)?.Name ?? string.Empty
                : txtNewBranchName.Text);
            txtWorktreeDirectory.Text = $"{_initialDirectoryPath}_{branchNameNormalized}";
        }

        string NormalizeBranchName(string branchName) => string.Join("_", branchName.Split(_invalidCharsInPath, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}

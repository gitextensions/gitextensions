using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitUI.CommandsDialogs.WorktreeDialog;

public sealed partial class FormCreateWorktree : GitModuleForm
{
    private readonly AsyncLoader _branchesLoader = new();
    private readonly char[] _invalidCharsInPath = Path.GetInvalidFileNameChars();

    private readonly string? _initialDirectoryPath;

    public string WorktreeDirectory => newWorktreeDirectory.Text;
    public bool OpenWorktree => openWorktreeCheckBox.Checked;

    public IReadOnlyList<IGitRef>? ExistingBranches { get; set; }

    public FormCreateWorktree(IGitUICommands commands, string? path)
        : base(commands)
    {
        InitializeComponent();
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
            comboBoxBranches.Text = TranslatedStrings.LoadingData;
            ThreadHelper.FileAndForget(async () =>
            {
                await _branchesLoader.LoadAsync(
                    () => ExistingBranches.Where(r => r.Name != selectedBranch).ToList(),
                    list =>
                    {
                        comboBoxBranches.Text = string.Empty;
                        comboBoxBranches.DataSource = list;
                        comboBoxBranches.DisplayMember = nameof(IGitRef.LocalName);
                    });

                await this.SwitchToMainThreadAsync();
                if (comboBoxBranches.Items.Count == 0)
                {
                    radioButtonCreateNewBranch.Checked = true;
                    radioButtonCheckoutExistingBranch.Enabled = false;
                }
                else
                {
                    radioButtonCheckoutExistingBranch.Checked = true;
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

    private void comboBoxBranches_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            CreateWorktree();
        }
    }

    private void createWorktreeButton_Click(object sender, EventArgs e)
    {
        CreateWorktree();
    }

    private void CreateWorktree()
    {
        string relativePath = Path.GetRelativePath(Module.WorkingDir, WorktreeDirectory).ToPosixPath().Quote();
        string newBranchOption =
            radioButtonCreateNewBranch.Checked
            ? $"-b {textBoxNewBranchName.Text}"
            : (comboBoxBranches.SelectedItem as GitRef)?.Name;
        DialogResult = UICommands.StartGitCommandProcessDialog(this, CreateWorktreeCommand(Module, relativePath, newBranchOption)) ? DialogResult.OK : DialogResult.None;
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
        comboBoxBranches.Enabled = radioButtonCheckoutExistingBranch.Checked;
        textBoxNewBranchName.Enabled = radioButtonCreateNewBranch.Checked;
        if (radioButtonCheckoutExistingBranch.Checked)
        {
            createWorktreeButton.Enabled = comboBoxBranches.SelectedItem is not null;
        }
        else
        {
            createWorktreeButton.Enabled = !(string.IsNullOrWhiteSpace(textBoxNewBranchName.Text)
                                             || ExistingBranches.Any(b => b.Name == textBoxNewBranchName.Text));
        }

        if (createWorktreeButton.Enabled)
        {
            createWorktreeButton.Enabled = IsTargetFolderValid();
        }

        return;

        bool IsTargetFolderValid()
        {
            if (string.IsNullOrWhiteSpace(newWorktreeDirectory.Text))
            {
                return false;
            }

            try
            {
                DirectoryInfo directoryInfo = new(newWorktreeDirectory.Text);
                return !directoryInfo.Exists || (!directoryInfo.EnumerateFiles().Any() && !directoryInfo.EnumerateDirectories().Any());
            }
            catch
            {
                return false;
            }
        }
    }

    private void ValidateWorktreeOptions(object sender, EventArgs e)
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
            string branchNameNormalized = NormalizeBranchName(radioButtonCheckoutExistingBranch.Checked
                ? ((IGitRef)comboBoxBranches.SelectedItem)?.Name ?? string.Empty
                : textBoxNewBranchName.Text);
            newWorktreeDirectory.Text = $"{_initialDirectoryPath}_{branchNameNormalized}";
        }

        string NormalizeBranchName(string branchName) => string.Join("_", branchName.Split(_invalidCharsInPath, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}

using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitUI.CommandsDialogs.WorktreeDialog;

public sealed partial class FormCreateWorktree : GitExtensionsDialog
{
    private readonly AsyncLoader _branchesLoader = new();
    private readonly char[] _invalidCharsInPath = Path.GetInvalidFileNameChars();
    private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);
    private readonly IGitBranchNameNormaliser _branchNameNormaliser;

    private readonly string? _initialDirectoryPath;

    public string WorktreeDirectory => txtWorktreeDirectory.Text;

    public IReadOnlyList<IGitRef>? ExistingBranches { get; set; }

    public IReadOnlyList<IGitRef>? RemoteBranches { get; set; }

    public FormCreateWorktree(IGitUICommands commands, string? path)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        _branchNameNormaliser = commands.GetRequiredService<IGitBranchNameNormaliser>();

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
            RemoteBranches = Module.GetRefs(RefsFilter.Remotes);
            cbxBranches.Text = TranslatedStrings.LoadingData;
            ThreadHelper.FileAndForget(async () =>
            {
                await _branchesLoader.LoadAsync(
                    () => GetCheckoutableBranches(ExistingBranches, RemoteBranches, selectedBranch),
                    list =>
                    {
                        cbxBranches.Text = string.Empty;
                        cbxBranches.DataSource = list;
                        cbxBranches.DisplayMember = nameof(IGitRef.Name);
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

    /// <summary>
    ///  Combines local and remote branches for the checkout list. Local branches come
    ///  first, excluding the branch already checked out in this worktree (git refuses
    ///  to check out a branch in a second worktree), followed by remote branches,
    ///  which git resolves to a new tracking branch on checkout. Remote branches whose
    ///  tracking name already exists as a local branch are excluded, as checking them
    ///  out would fail.
    /// </summary>
    private static IReadOnlyList<IGitRef> GetCheckoutableBranches(
        IReadOnlyList<IGitRef> localBranches,
        IReadOnlyList<IGitRef> remoteBranches,
        string selectedBranch)
    {
        List<IGitRef> branches = [];
        branches.AddRange(localBranches.Where(b => b.Name != selectedBranch));

        // Exclude remote branches whose local (tracking) name already exists as a local
        // branch; checking them out would fail, as git cannot create a branch that
        // already exists.
        branches.AddRange(remoteBranches.Where(r => !localBranches.Any(l => l.Name == r.LocalName)));
        return branches;
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

    private void txtNewBranchName_Leave(object sender, EventArgs e)
    {
        NormaliseNewBranchName();
    }

    private void NormaliseNewBranchName()
    {
        if (!AppSettings.AutoNormaliseBranchName || !txtNewBranchName.Text.Any(PathUtil.IsValidPathChar))
        {
            return;
        }

        int caretPosition = txtNewBranchName.SelectionStart;
        txtNewBranchName.Text = _branchNameNormaliser.Normalise(txtNewBranchName.Text, _gitBranchNameOptions);
        txtNewBranchName.SelectionStart = caretPosition;
    }

    private void btnCreateWorktree_Click(object sender, EventArgs e)
    {
        CreateWorktree();
    }

    private void CreateWorktree()
    {
        // The branch name may not have lost focus yet (e.g. when triggered via the accept button),
        // so normalise it here to avoid passing an invalid name (e.g. containing spaces) to git.
        if (rbCreateNewBranch.Checked)
        {
            NormaliseNewBranchName();
        }

        string relativePath = Path.GetRelativePath(Module.WorkingDir, WorktreeDirectory).ToPosixPath().Quote();
        string? newBranchOption =
            rbCreateNewBranch.Checked
            ? $"-b {txtNewBranchName.Text}"
            : (cbxBranches.SelectedItem as GitRef)?.Name;
        DialogResult = UICommands.StartGitCommandProcessDialog(this, CreateWorktreeCommand(Module, relativePath, newBranchOption!)) ? DialogResult.OK : DialogResult.None;
    }

    private static GitArgumentBuilder CreateWorktreeCommand(IGitModule module, string relativePath, string newBranchOption)
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
            // LocalName keeps the directory suffix short for remote branches
            // (e.g. origin/feature -> feature) while staying identical for local ones.
            string branchNameNormalized = NormalizeBranchName(rbCheckoutExistingBranch.Checked
                ? ((IGitRef?)cbxBranches.SelectedItem)?.LocalName ?? string.Empty
                : txtNewBranchName.Text);
            txtWorktreeDirectory.Text = $"{_initialDirectoryPath}_{branchNameNormalized}";
        }

        string NormalizeBranchName(string branchName) => string.Join("_", branchName.Split(_invalidCharsInPath, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    internal static TestAccessor GetTestAccessor() => new();

    internal readonly struct TestAccessor
    {
        public IReadOnlyList<IGitRef> GetCheckoutableBranches(IReadOnlyList<IGitRef> localBranches, IReadOnlyList<IGitRef> remoteBranches, string selectedBranch) => FormCreateWorktree.GetCheckoutableBranches(localBranches, remoteBranches, selectedBranch);

        public GitArgumentBuilder CreateWorktreeCommand(IGitModule module, string relativePath, string newBranchOption) => FormCreateWorktree.CreateWorktreeCommand(module, relativePath, newBranchOption);
    }
}

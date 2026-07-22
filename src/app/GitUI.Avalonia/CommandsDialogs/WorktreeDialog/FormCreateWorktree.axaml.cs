using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.WorktreeDialog;

// Twin of GitUI/CommandsDialogs/WorktreeDialog/FormCreateWorktree.cs. The branch list
// remains code-behind driven and the original worktree command construction is retained.
public sealed partial class FormCreateWorktree : GitExtensionsDialog
{
    private readonly CancellationTokenSequence _branchesLoadSequence = new();
    private readonly char[] _invalidCharsInPath = Path.GetInvalidFileNameChars();

    private readonly string? _initialDirectoryPath;

    public string WorktreeDirectory => txtWorktreeDirectory.Text ?? string.Empty;
    public bool OpenWorktree => chkOpenWorktree.IsChecked == true;

    public IReadOnlyList<IGitRef>? ExistingBranches { get; set; }

    public FormCreateWorktree()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormCreateWorktree(IGitUICommands commands, string? path)
        : base(commands, enablePositionRestore: false)
    {
        _initialDirectoryPath = path;
        InitializeComponent();
        WireControls();
        AcceptButton = btnCreateWorktree;
        InitializeComplete();
    }

    private void WireControls()
    {
        cbxBranches.ItemTemplate = new FuncDataTemplate<IGitRef>(
            (branch, _) => new TextBlock { Text = branch?.LocalName ?? string.Empty },
            supportsRecycling: false);
        btnBrowseWorktreeDir.PathShowingControl = txtWorktreeDirectory;
        btnCreateWorktree.Click += btnCreateWorktree_Click;
        cbxBranches.SelectionChanged += UpdateWorktreePathAndValidateWorktreeOptions;
        cbxBranches.KeyUp += cbxBranches_KeyUp;
        rbCheckoutExistingBranch.IsCheckedChanged += UpdateWorktreePathAndValidateWorktreeOptions;
        rbCreateNewBranch.IsCheckedChanged += UpdateWorktreePathAndValidateWorktreeOptions;
        txtNewBranchName.TextChanged += UpdateWorktreePathAndValidateWorktreeOptions;
        txtWorktreeDirectory.TextChanged += txtWorktreeDirectory_TextChanged;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (!TryGetUICommands(out _))
        {
            return;
        }

        LoadBranches();
        UpdateWorktreePathAndValidateWorktreeOptions();
    }

    protected override void OnClosed(EventArgs e)
    {
        _branchesLoadSequence.Dispose();
        base.OnClosed(e);
    }

    private void LoadBranches()
    {
        string selectedBranch = UICommands.Module.GetSelectedBranch();
        ExistingBranches = Module.GetRefs(RefsFilter.Heads);
        cbxBranches.PlaceholderText = TranslatedStrings.LoadingData;
        CancellationToken cancellationToken = _branchesLoadSequence.Next();

        ThreadHelper.FileAndForget(async () =>
        {
            List<IGitRef> branches = await Task.Run(
                () => ExistingBranches
                    .Where(branch => branch.Name != selectedBranch)
                    .ToList(),
                cancellationToken);

            await this.SwitchToMainThreadAsync();
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            ApplyBranches(branches);
        });
    }

    private void ApplyBranches(IReadOnlyList<IGitRef> branches)
    {
        ExistingBranches = branches;
        cbxBranches.PlaceholderText = string.Empty;
        cbxBranches.ItemsSource = branches;
        cbxBranches.SelectedIndex = branches.Count > 0 ? 0 : -1;

        if (branches.Count == 0)
        {
            rbCreateNewBranch.IsChecked = true;
            rbCheckoutExistingBranch.IsEnabled = false;
        }
        else
        {
            rbCheckoutExistingBranch.IsChecked = true;
        }

        ValidateWorktreeOptions();
    }

    private void cbxBranches_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && btnCreateWorktree.IsEnabled)
        {
            CreateWorktree();
        }
    }

    private void btnCreateWorktree_Click(object? sender, EventArgs e)
    {
        CreateWorktree();
    }

    private void CreateWorktree()
    {
        string relativePath = Path.GetRelativePath(Module.WorkingDir, WorktreeDirectory).ToPosixPath().Quote();
        string? newBranchOption = rbCreateNewBranch.IsChecked == true
            ? $"-b {txtNewBranchName.Text}"
            : (cbxBranches.SelectedItem as IGitRef)?.Name;

        if (string.IsNullOrWhiteSpace(newBranchOption))
        {
            return;
        }

        DialogResult = UICommands.StartGitCommandProcessDialog(this, CreateWorktreeCommand(Module, relativePath, newBranchOption))
            ? WinFormsShims.DialogResult.OK
            : WinFormsShims.DialogResult.None;
    }

    private GitArgumentBuilder CreateWorktreeCommand(IGitModule module, string relativePath, string newBranchOption)
    {
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
        cbxBranches.IsEnabled = rbCheckoutExistingBranch.IsChecked == true;
        txtNewBranchName.IsEnabled = rbCreateNewBranch.IsChecked == true;
        if (rbCheckoutExistingBranch.IsChecked == true)
        {
            btnCreateWorktree.IsEnabled = cbxBranches.SelectedItem is not null;
        }
        else
        {
            btnCreateWorktree.IsEnabled = !(string.IsNullOrWhiteSpace(txtNewBranchName.Text)
                                             || (ExistingBranches ?? []).Any(branch => branch.Name == txtNewBranchName.Text));
        }

        if (btnCreateWorktree.IsEnabled)
        {
            btnCreateWorktree.IsEnabled = IsTargetFolderValid();
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

    private void txtWorktreeDirectory_TextChanged(object? sender, EventArgs e)
    {
        ValidateWorktreeOptions();
    }

    private void UpdateWorktreePathAndValidateWorktreeOptions(object? sender, EventArgs e)
        => UpdateWorktreePathAndValidateWorktreeOptions();

    private void UpdateWorktreePathAndValidateWorktreeOptions()
    {
        UpdateWorktreePath();
        ValidateWorktreeOptions();

        return;

        void UpdateWorktreePath()
        {
            string branchNameNormalized = NormalizeBranchName(rbCheckoutExistingBranch.IsChecked == true
                ? (cbxBranches.SelectedItem as IGitRef)?.Name ?? string.Empty
                : txtNewBranchName.Text ?? string.Empty);
            txtWorktreeDirectory.Text = $"{_initialDirectoryPath}_{branchNameNormalized}";
        }

        string NormalizeBranchName(string branchName)
            => string.Join("_", branchName.Split(_invalidCharsInPath, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormCreateWorktree form)
    {
        public Button Create => form.btnCreateWorktree;
        public CheckBox Open => form.chkOpenWorktree;
        public ComboBox Branches => form.cbxBranches;
        public RadioButton CheckoutExisting => form.rbCheckoutExistingBranch;
        public RadioButton CreateNew => form.rbCreateNewBranch;
        public TextBox NewBranchName => form.txtNewBranchName;
        public TextBox WorktreeDirectory => form.txtWorktreeDirectory;

        public void SetBranches(IReadOnlyList<IGitRef> branches) => form.ApplyBranches(branches);

        public void SelectNewBranch(string name)
        {
            form.rbCheckoutExistingBranch.IsChecked = false;
            form.rbCreateNewBranch.IsChecked = true;
            form.txtNewBranchName.Text = name;
            form.UpdateWorktreePathAndValidateWorktreeOptions();
        }

        public ArgumentString CreateCommand(IGitModule module, string relativePath, string newBranchOption)
            => form.CreateWorktreeCommand(module, relativePath, newBranchOption);
    }
}

using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using MessageBoxes = GitUI.MessageBoxes;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensions.Plugins.DeleteUnusedBranches;

public sealed partial class DeleteUnusedBranchesForm : GitExtensionsFormBase
{
    private readonly TranslationString _deleteCaption = new("Delete");
    private readonly TranslationString _selectBranchesToDelete = new("Select branches to delete using checkboxes in '{0}' column.");
    private readonly TranslationString _areYouSureToDelete = new("Are you sure to delete {0} selected branches?");
    private readonly TranslationString _dangerousAction = new("DANGEROUS ACTION!\nBranches will be deleted on the remote '{0}'. This can not be undone.\nAre you sure you want to continue?");
    private readonly TranslationString _deletingBranches = new("Deleting branches...");
    private readonly TranslationString _deletingUnmergedBranches = new("Deleting unmerged branches will result in dangling commits. Use with caution!");
    private readonly TranslationString _chooseBranchesToDelete = new("Choose branches to delete. Only branches that are fully merged in '{0}' will be deleted.");
    private readonly TranslationString _pressToSearch = new("Press '{0}' to search for branches to delete.");
    private readonly TranslationString _cancel = new("Cancel");
    private readonly TranslationString _searchBranches = new("Search branches");
    private readonly TranslationString _loading = new("Loading...");
    private readonly TranslationString _branchesSelected = new("{0}/{1} branches selected.");
    private readonly DeleteUnusedBranchesFormSettings _settings;

    private readonly SortableBranchesList _branches = [];
    private readonly IGitModule? _gitCommands;
    private readonly IGitUICommands? _gitUiCommands;
    private readonly IGitPlugin? _gitPlugin;
    private readonly GitBranchOutputCommandParser _commandOutputParser;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _operations = GitUI.Compat.DesignTimeTaskManager.Create();
    private CancellationTokenSource? _refreshCancellation;
    private string? _sortColumn;
    private bool _sortAscending = true;
    private bool _updatingHeader;

    public bool HasDeletedBranch { get; internal set; }

    public DeleteUnusedBranchesForm()
    {
        _settings = new DeleteUnusedBranchesFormSettings(
            daysOlderThan: 30,
            mergedInBranch: "HEAD",
            removeDeleteRemoteBranchesFromFlag: false,
            remoteName: "origin",
            userRegexToFilterBranchesFlag: false,
            regexFilter: "/(feature|develop)/",
            regexCaseInsensitiveFlag: false,
            regexInvertedFlag: false,
            includeUnmergedBranchesFlag: false);
        _commandOutputParser = new GitBranchOutputCommandParser();

        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public DeleteUnusedBranchesForm(DeleteUnusedBranchesFormSettings settings, IGitModule gitCommands, IGitUICommands? gitUiCommands, IGitPlugin gitPlugin)
    {
        _settings = settings;
        _gitCommands = gitCommands;
        _gitUiCommands = gitUiCommands;
        _gitPlugin = gitPlugin;
        _commandOutputParser = new GitBranchOutputCommandParser();

        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        BranchesGrid.ItemTemplate = new FuncDataTemplate<Branch>(CreateBranchRow, supportsRecycling: false);
        _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.IsCheckedChanged += CheckBoxHeader_OnCheckBoxClicked;
        nameDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        dateDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        Author.PointerReleased += Header_PointerReleased;
        Delete.Click += Delete_Click;
        Cancel.Click += Cancel_Click;
        buttonSettings.Click += buttonSettings_Click;
        RefreshBtn.Click += Refresh_Click;
        IncludeRemoteBranches.IsCheckedChanged += ClearResults;
        _NO_TRANSLATE_Remote.TextChanged += ClearResults;
        useRegexFilter.IsCheckedChanged += ClearResults;
        regexFilter.TextChanged += ClearResults;
        mergedIntoBranch.TextChanged += ClearResults;
        olderThanDays.ValueChanged += ClearResults;
        includeUnmergedBranches.IsCheckedChanged += includeUnmergedBranches_CheckedChanged;
        useRegexCaseInsensitive.IsCheckedChanged += ClearResults;
        regexDoesNotMatch.IsCheckedChanged += ClearResults;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        LoadSettings();
        if (_gitUiCommands is not null)
        {
            _operations.FileAndForget(RefreshObsoleteBranchesAsync);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshCancellation?.Cancel();
        _operations.JoinPendingOperations();
        _refreshCancellation?.Dispose();
        _refreshCancellation = null;
        base.OnClosed(e);
    }

    private void LoadSettings()
    {
        mergedIntoBranch.Text = _settings.MergedInBranch;
        olderThanDays.Value = _settings.DaysOlderThan;
        IncludeRemoteBranches.IsChecked = _settings.DeleteRemoteBranchesFromFlag;
        _NO_TRANSLATE_Remote.Text = _settings.RemoteName;
        useRegexFilter.IsChecked = _settings.UseRegexToFilterBranchesFlag;
        regexFilter.Text = _settings.RegexFilter;
        useRegexCaseInsensitive.IsChecked = _settings.RegexCaseInsensitiveFlag;
        regexDoesNotMatch.IsChecked = _settings.RegexInvertedFlag;
        includeUnmergedBranches.IsChecked = _settings.IncludeUnmergedBranchesFlag;
        ClearResults(this, EventArgs.Empty);
    }

    private Avalonia.Controls.Control CreateBranchRow(Branch? branch, INameScope? nameScope)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("50,180,175,120,*"),
        };
        if (branch is null)
        {
            return row;
        }

        Avalonia.Controls.CheckBox delete = new()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsChecked = branch.Delete,
        };
        delete.IsCheckedChanged += (_, _) =>
        {
            branch.Delete = delete.IsChecked == true;
            SetHeaderChecked(_branches.All(item => item.Delete));
            lblStatus.Text = GetDefaultStatusText();
        };
        row.Children.Add(delete);
        row.Children.Add(CreateCell(branch.Name, column: 1));
        row.Children.Add(CreateCell(branch.Date.ToString(), column: 2));
        row.Children.Add(CreateCell(branch.Author, column: 3));
        row.Children.Add(CreateCell(branch.Message, column: 4));
        return row;

        static TextBlock CreateCell(string text, int column)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Avalonia.Thickness(6, 2),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private IEnumerable<Branch> GetObsoleteBranches(RefreshContext context, string curBranch)
    {
        DateTime oldBranchLimitDate = DateTime.Now - context.ObsolescenceDuration;
        foreach (string branchName in GetObsoleteBranchNames(context, curBranch))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            GitArgumentBuilder args = new("log")
            {
                "--pretty=\"format:%ci\n%an\n%s\"",
                "--max-count=1",
                branchName.Quote(),
                "--",
            };

            string[] commitLog = context.Commands.GitExecutable.GetOutput(args).Split('\n');
            if (!DateTime.TryParse(commitLog[0], out DateTime commitDate))
            {
                Trace.WriteLine($"Failed to parse commit date from git log output: '{commitLog[0]}' from {commitLog}");
                commitDate = DateTime.MinValue;
            }

            string authorName = commitLog.Length > 1 ? commitLog[1] : string.Empty;
            string message = commitLog.Length > 2 ? commitLog[2] : string.Empty;

            yield return new Branch(branchName, commitDate, authorName, message, commitDate < oldBranchLimitDate);
        }
    }

    private IEnumerable<string> GetObsoleteBranchNames(RefreshContext context, string curBranch)
    {
        RegexOptions options = context.RegexIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        bool regexMustMatch = !context.RegexDoesNotMatch;

        GitArgumentBuilder args = new("branch")
        {
            "--list",
            { context.RemoteBranches, "-r" },
            { !context.IncludeUnmerged, $"--merged {context.ReferenceBranch}" },
        };

        ExecutionResult result = context.Commands.GitExecutable.Execute(args, throwOnErrorExit: false);
        if (!result.ExitedSuccessfully)
        {
            throw new InvalidOperationException($"git {args}{Environment.NewLine}{result.AllOutput}");
        }

        bool withoutRegexFilter = string.IsNullOrEmpty(context.RegexFilter);
        return _commandOutputParser.GetBranchNames(result.StandardOutput, context.RemoteBranches)
            .Where(branchName => branchName != curBranch && branchName != context.ReferenceBranch)
            .Where(branchName => (!context.RemoteBranches || branchName.StartsWith(context.RemoteRepositoryName + "/"))
                && (withoutRegexFilter || Regex.IsMatch(branchName, context.RegexFilter!, options) == regexMustMatch));
    }

    private void Delete_Click(object? sender, EventArgs e)
    {
        _operations.FileAndForget(DeleteSelectedBranchesAsync);
    }

    private async Task DeleteSelectedBranchesAsync()
    {
        List<Branch> selectedBranches = [.. _branches.Where(branch => branch.Delete)];
        if (selectedBranches.Count == 0)
        {
            MessageBoxes.Show(
                string.Format(_selectBranchesToDelete.Text, string.Empty),
                _deleteCaption.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        if (MessageBoxes.Show(
                this,
                string.Format(_areYouSureToDelete.Text, selectedBranches.Count),
                _deleteCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Warning) != WinFormsShims.DialogResult.Yes)
        {
            return;
        }

        string remoteName = _NO_TRANSLATE_Remote.Text ?? string.Empty;
        string remoteBranchPrefix = remoteName + "/";
        List<Branch> remoteBranches = IncludeRemoteBranches.IsChecked == true
            ? [.. selectedBranches.Where(branch => branch.Name.StartsWith(remoteBranchPrefix))]
            : [];

        if (remoteBranches.Count > 0
            && MessageBoxes.Show(
                this,
                string.Format(_dangerousAction.Text, remoteName),
                _deleteCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Warning) != WinFormsShims.DialogResult.Yes)
        {
            return;
        }

        IGitModule gitCommands = GetGitCommands();
        bool includeUnmerged = includeUnmergedBranches.IsChecked == true;
        HasDeletedBranch = true;
        List<Branch> localBranches = [.. selectedBranches.Except(remoteBranches)];
        SetWorkingState(isWorking: true);
        lblStatus.Text = _deletingBranches.Text;

        await TaskScheduler.Default.SwitchTo(alwaysYield: true);
        try
        {
            foreach (Branch remoteBranch in remoteBranches)
            {
                int remoteBranchNameOffset = remoteBranchPrefix.Length;
                GitArgumentBuilder args = new("push")
                {
                    remoteName,
                    $":{remoteBranch.Name[remoteBranchNameOffset..]}",
                };
                gitCommands.GitExecutable.GetOutput(args);
            }

            foreach (Branch localBranch in localBranches)
            {
                GitArgumentBuilder args = new("branch")
                {
                    includeUnmerged ? "-D" : "-d",
                    localBranch.Name,
                };
                gitCommands.GitExecutable.GetOutput(args);
            }
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            MessageBoxes.ShowError(this, ex.Message);
        }

        Validates.NotNull(_gitUiCommands);
        _gitUiCommands.RepoChangedNotifier.Notify();

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
        SetWorkingState(isWorking: false);
        await RefreshObsoleteBranchesAsync();
    }

    private void SetWorkingState(bool isWorking)
    {
        imgLoading.IsVisible = isWorking;
        tableLayoutPanel2.IsEnabled = tableLayoutPanel3.IsEnabled = !isWorking;
    }

    private void buttonSettings_Click(object? sender, EventArgs e)
    {
        Hide();
        Close();
        Validates.NotNull(_gitUiCommands);
        Validates.NotNull(_gitPlugin);
        _gitUiCommands.StartSettingsDialog(_gitPlugin);
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    private void includeUnmergedBranches_CheckedChanged(object? sender, EventArgs e)
    {
        ClearResults(sender, e);

        if (includeUnmergedBranches.IsChecked == true)
        {
            MessageBoxes.Show(
                this,
                _deletingUnmergedBranches.Text,
                _deleteCaption.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Warning);
        }
    }

    private void ClearResults(object? sender, EventArgs e)
    {
        instructionLabel.Text = string.Format(_chooseBranchesToDelete.Text, mergedIntoBranch.Text);
        lblStatus.Text = string.Format(_pressToSearch.Text, RefreshBtn.Content);
        _branches.Clear();
        RefreshRows();
    }

    private void Refresh_Click(object? sender, EventArgs e)
    {
        _operations.FileAndForget(RefreshObsoleteBranchesAsync);
    }

    private void CheckBoxHeader_OnCheckBoxClicked(object? sender, EventArgs e)
    {
        if (_updatingHeader)
        {
            return;
        }

        bool isChecked = _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.IsChecked == true;
        foreach (Branch branch in _branches)
        {
            branch.Delete = isChecked;
        }

        RefreshRows();
    }

    private void Header_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        string column = ReferenceEquals(sender, nameDataGridViewTextBoxColumn)
            ? nameof(Branch.Name)
            : ReferenceEquals(sender, dateDataGridViewTextBoxColumn)
                ? nameof(Branch.Date)
                : nameof(Branch.Author);
        SortBy(column);
        e.Handled = true;
    }

    private void SortBy(string column)
    {
        _sortAscending = _sortColumn != column || !_sortAscending;
        _sortColumn = column;
        RefreshRows();
    }

    private async Task RefreshObsoleteBranchesAsync()
    {
        if (IsRefreshing)
        {
            Validates.NotNull(_refreshCancellation);
            await _refreshCancellation.CancelAsync();
            IsRefreshing = false;
            return;
        }

        Validates.NotNull(_gitUiCommands);
        IGitModule gitCommands = GetGitCommands();
        IsRefreshing = true;
        Validates.NotNull(_refreshCancellation);

        string curBranch = _gitUiCommands.Module.GetSelectedBranch();
        RefreshContext context = new(
            gitCommands,
            IncludeRemoteBranches.IsChecked == true,
            includeUnmergedBranches.IsChecked == true,
            mergedIntoBranch.Text ?? string.Empty,
            _NO_TRANSLATE_Remote.Text ?? string.Empty,
            useRegexFilter.IsChecked == true ? regexFilter.Text : null,
            useRegexCaseInsensitive.IsChecked == true,
            regexDoesNotMatch.IsChecked == true,
            TimeSpan.FromDays((int)(olderThanDays.Value ?? 0)),
            _refreshCancellation.Token);

        await TaskScheduler.Default.SwitchTo(alwaysYield: true);
        Branch[] branches;
        try
        {
            branches = [.. GetObsoleteBranches(context, curBranch)];
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            MessageBoxes.ShowError(this, ex.Message);
            branches = [];
        }

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        _branches.Clear();
        _branches.AddRange(branches);
        _sortColumn = null;
        _sortAscending = true;
        SetHeaderChecked(_branches.All(branch => branch.Delete));
        RefreshRows();
        IsRefreshing = false;
    }

    private bool IsRefreshing
    {
        get => _refreshCancellation is not null;
        set
        {
            if (value == IsRefreshing)
            {
                return;
            }

            CancellationTokenSource? previous = _refreshCancellation;
            _refreshCancellation = value ? new CancellationTokenSource() : null;
            previous?.Dispose();
            RefreshBtn.Content = value ? _cancel.Text : _searchBranches.Text;
            imgLoading.IsVisible = value;
            lblStatus.Text = value ? _loading.Text : GetDefaultStatusText();
        }
    }

    private void RefreshRows()
    {
        BranchesGrid.ItemsSource = _branches.GetSorted(_sortColumn, _sortAscending);
        lblStatus.Text = GetDefaultStatusText();
    }

    private void SetHeaderChecked(bool value)
    {
        _updatingHeader = true;
        _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.IsChecked = value;
        _updatingHeader = false;
    }

    private string GetDefaultStatusText()
    {
        return string.Format(_branchesSelected.Text, _branches.Count(branch => branch.Delete), _branches.Count);
    }

    private IGitModule GetGitCommands()
        => _gitCommands ?? throw new InvalidOperationException($"{nameof(DeleteUnusedBranchesForm)} was constructed incorrectly.");

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(nameDataGridViewTextBoxColumn), "Name");
        AddHeaderTranslationItem(translation, nameof(dateDataGridViewTextBoxColumn), "Last activity");
        AddHeaderTranslationItem(translation, nameof(Author), "Last author");
        AddHeaderTranslationItem(translation, nameof(Message), "Last message");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, nameDataGridViewTextBoxColumn, nameof(nameDataGridViewTextBoxColumn), "Name");
        TranslateHeader(translation, dateDataGridViewTextBoxColumn, nameof(dateDataGridViewTextBoxColumn), "Last activity");
        TranslateHeader(translation, Author, nameof(Author), "Last author");
        TranslateHeader(translation, Message, nameof(Message), "Last message");
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
        => translation.AddTranslationItem(nameof(DeleteUnusedBranchesForm), fieldName, "HeaderText", text);

    private static void TranslateHeader(ITranslation translation, Border header, string fieldName, string defaultText)
    {
        string? text = translation.TranslateItem(
            nameof(DeleteUnusedBranchesForm),
            fieldName,
            "HeaderText",
            () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(DeleteUnusedBranchesForm form)
    {
        public IReadOnlyList<Branch> Branches => form._branches;
        public Avalonia.Controls.ListBox BranchesGrid => form.BranchesGrid;
        public Avalonia.Controls.CheckBox HeaderCheckBox => form._NO_TRANSLATE_deleteDataGridViewCheckBoxColumn;
        public TextBlock Status => form.lblStatus;

        public Task DeleteSelectedBranchesAsync() => form.DeleteSelectedBranchesAsync();
        public void LoadSettings() => form.LoadSettings();
        public Task RefreshObsoleteBranchesAsync() => form.RefreshObsoleteBranchesAsync();
        public void SortByName() => form.SortBy(nameof(Branch.Name));
    }

    private readonly struct RefreshContext
    {
        public RefreshContext(
            IGitModule commands,
            bool includeRemotes,
            bool includeUnmerged,
            string referenceBranch,
            string remoteRepositoryName,
            string? regexFilter,
            bool regexIgnoreCase,
            bool regexDoesNotMatch,
            TimeSpan obsolescenceDuration,
            CancellationToken cancellationToken)
        {
            Commands = commands;
            RemoteBranches = includeRemotes;
            IncludeUnmerged = includeUnmerged;
            ReferenceBranch = referenceBranch;
            RemoteRepositoryName = remoteRepositoryName;
            RegexFilter = regexFilter;
            RegexIgnoreCase = regexIgnoreCase;
            RegexDoesNotMatch = regexDoesNotMatch;
            ObsolescenceDuration = obsolescenceDuration;
            CancellationToken = cancellationToken;
        }

        public IGitModule Commands { get; }
        public bool RemoteBranches { get; }
        public bool IncludeUnmerged { get; }
        public string ReferenceBranch { get; }
        public string RemoteRepositoryName { get; }
        public string? RegexFilter { get; }
        public bool RegexIgnoreCase { get; }
        public bool RegexDoesNotMatch { get; }
        public TimeSpan ObsolescenceDuration { get; }
        public CancellationToken CancellationToken { get; }
    }
}

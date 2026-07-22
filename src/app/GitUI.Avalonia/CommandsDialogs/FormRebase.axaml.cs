using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public partial class FormRebase : GitExtensionsDialog
{
    private const string IgnoreDateToolTip =
        "Sets the author date to the current date (same as" + "\n" +
        "commit date), ignoring the original author date.";
    private const string CommitterDateToolTip =
        "Sets the commit date to the original author date" + "\n" +
        "(instead of the current date).";

    private readonly TranslationString _continueRebaseText = new("&Continue rebase");
    private readonly TranslationString _solveConflictsText = new("&Solve conflicts");
    private readonly TranslationString _solveConflictsText2 = new(">&Solve conflicts<");
    private readonly TranslationString _continueRebaseText2 = new(">&Continue rebase<");
    private readonly TranslationString _noBranchSelectedText = new("Please select a branch");
    private readonly TranslationString _branchUpToDateText =
        new("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.");
    private readonly TranslationString _branchUpToDateCaption = new("Rebase");
    private readonly TranslationString _hoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");

    private static readonly List<PatchFile> Skipped = [];

    private readonly string? _defaultBranch;
    private readonly string? _defaultToBranch;
    private readonly bool _startRebaseImmediately;

    public FormRebase()
    {
        InitializeComponent();
        InitializeStaticContent();
        InitializeComplete();
    }

    public FormRebase(IGitUICommands commands, string? defaultBranch)
        : this(
            commands,
            from: string.Empty,
            to: null,
            defaultBranch,
            interactive: false,
            startRebaseImmediately: false)
    {
    }

    public FormRebase(
        IGitUICommands commands,
        string? from,
        string? to,
        string? defaultBranch,
        bool interactive = false,
        bool startRebaseImmediately = true)
        : base(commands, enablePositionRestore: false)
    {
        _defaultBranch = defaultBranch;
        _defaultToBranch = to;
        _startRebaseImmediately = startRebaseImmediately;

        InitializeComponent();
        InitializeStaticContent();

        btnRebase.Click += OkClick;
        btnContinueRebase.Click += ResolvedClick;
        btnAbort.Click += AbortClick;
        btnSkip.Click += SkipClick;
        btnSolveConflicts.Click += MergetoolClick;
        btnSolveMergeconflicts.Click += SolveMergeConflictsClick;
        btnAddFiles.Click += AddFilesClick;
        btnCommit.Click += Commit_Click;
        btnEditTodo.Click += EditTodoClick;
        btnChooseFromRevision.Click += btnChooseFromRevision_Click;
        llblShowOptions.Click += ShowOptions_LinkClicked;
        chkInteractive.IsCheckedChanged += chkInteractive_CheckedChanged;
        chkIgnoreDate.IsCheckedChanged += chkIgnoreDate_CheckedChanged;
        chkCommitterDateIsAuthorDate.IsCheckedChanged += chkCommitterDateIsAuthorDate_CheckedChanged;
        chkSpecificRange.IsCheckedChanged += chkUseFromOnto_CheckedChanged;

        txtFrom.Text = from ?? string.Empty;
        chkSpecificRange.IsChecked = !string.IsNullOrEmpty(from);
        chkInteractive.IsChecked = interactive;
        chkAutosquash.IsEnabled = interactive;
        PanelLeftImage.IsVisible = !AppSettings.DontShowHelpImages;
        checkBoxUpdateRefs.IsVisible = Module.GitVersion.SupportUpdateRefs;
        PatchGrid.UICommandsSource = this;
        PatchGrid.IsManagingRebase = true;
        PatchGrid.SetSkipped(Skipped);

        AcceptButton = btnRebase;
        InitializeComplete();
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormRebase), nameof(chkIgnoreDate), "toolTip1", IgnoreDateToolTip.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        translation.AddTranslationItem(nameof(FormRebase), nameof(chkCommitterDateIsAuthorDate), "toolTip1", CommitterDateToolTip.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);

        PanelLeftImage.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;
        string? ignoreDateToolTip = translation.TranslateItem(
            nameof(FormRebase),
            nameof(chkIgnoreDate),
            "toolTip1",
            () => IgnoreDateToolTip.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        string? committerDateToolTip = translation.TranslateItem(
            nameof(FormRebase),
            nameof(chkCommitterDateIsAuthorDate),
            "toolTip1",
            () => CommitterDateToolTip.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        ToolTip.SetTip(chkIgnoreDate, ignoreDateToolTip);
        ToolTip.SetTip(chkCommitterDateIsAuthorDate, committerDateToolTip);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        string selectedHead = Module.GetSelectedBranch();
        Currentbranch.Text = selectedHead;

        IReadOnlyList<IGitRef> refs = _startRebaseImmediately
            ? []
            : Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes | RefsFilter.Tags);
        cboBranches.ItemsSource = refs;
        if (_defaultBranch is not null)
        {
            SetComboText(cboBranches, refs, _defaultBranch);
        }

        IReadOnlyList<IGitRef> heads = refs.Where(gitRef => gitRef.IsHead).ToList();
        cboTo.ItemsSource = heads;
        SetComboText(cboTo, heads, _defaultToBranch ?? selectedHead);

        chkStash.IsChecked = AppSettings.RebaseAutoStash;
        chkStash.IsEnabled = !Module.InTheMiddleOfRebase() && Module.IsDirtyDir();
        chkPreserveMerges.IsEnabled = Module.GitVersion.SupportRebaseMerges;
        chkAutosquash.IsChecked = Module.GetEffectiveSetting<bool>("rebase.autosquash") is true;
        if (Module.GetEffectiveSetting<bool>("rebase.updaterefs") is bool updateRefs)
        {
            checkBoxUpdateRefs.IsChecked = updateRefs;
        }

        rebasePanel.IsVisible = !Module.InTheMiddleOfRebase();
        EnableButtons();
        if (Module.InTheMiddleOfRebase())
        {
            PatchGrid.Initialize();
        }

        if (_startRebaseImmediately)
        {
            OkClick(this, EventArgs.Empty);
        }
        else
        {
            ShowOptions_LinkClicked(this, EventArgs.Empty);
            cboBranches.Focus();
        }
    }

    private void InitializeStaticContent()
    {
        cboBranches.ItemTemplate = CreateRefTemplate();
        cboTo.ItemTemplate = CreateRefTemplate();
        PanelLeftImage.Image1 = Properties.Images.HelpCommandRebase;
        PanelLeftImage.IsOnHoverShowImage2 = false;
        btnSolveMergeconflicts.Content = $"There are unresolved merge conflicts{Environment.NewLine}";
    }

    private static FuncDataTemplate<IGitRef> CreateRefTemplate()
        => new(
            (gitRef, _) => new TextBlock { Text = gitRef?.Name ?? string.Empty },
            supportsRecycling: false);

    private static void SetComboText(ComboBox comboBox, IReadOnlyList<IGitRef> refs, string text)
    {
        comboBox.SelectedItem = refs.FirstOrDefault(gitRef => gitRef.Name == text);
        comboBox.Text = text;
    }

    private static string GetComboText(ComboBox comboBox)
    {
        string text = comboBox.Text ?? string.Empty;
        return !string.IsNullOrWhiteSpace(text)
            ? text
            : (comboBox.SelectedItem as IGitRef)?.Name ?? string.Empty;
    }

    private void EnableButtons()
    {
        bool conflictedMerge = Module.InTheMiddleOfConflictedMerge();
        bool inRebase = Module.InTheMiddleOfRebase();

        cboBranches.IsEnabled = !inRebase;
        btnRebase.IsVisible = !inRebase;
        chkStash.IsEnabled = !inRebase && Module.IsDirtyDir();

        btnCommit.IsVisible = inRebase;
        btnAddFiles.IsVisible = inRebase;
        btnEditTodo.IsVisible = inRebase;
        btnContinueRebase.IsVisible = inRebase && !conflictedMerge;
        btnSolveConflicts.IsVisible = inRebase && conflictedMerge;
        btnSkip.IsVisible = inRebase;
        btnAbort.IsVisible = inRebase;
        btnSolveMergeconflicts.IsVisible = conflictedMerge;
        PatchGrid.IsVisible = inRebase;
        lblCommitsToReapply.IsVisible = inRebase;

        btnContinueRebase.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_continueRebaseText.Text);
        btnSolveConflicts.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_solveConflictsText.Text);
        MergeToolPanel.Background = null;

        if (conflictedMerge)
        {
            AcceptButton = btnSolveConflicts;
            btnSolveConflicts.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_solveConflictsText2.Text);
            MergeToolPanel.Background = new SolidColorBrush(Colors.Goldenrod);
            btnSolveConflicts.Focus();
        }
        else if (inRebase)
        {
            AcceptButton = btnContinueRebase;
            btnContinueRebase.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_continueRebaseText2.Text);
            btnContinueRebase.Focus();
        }
        else
        {
            AcceptButton = btnRebase;
        }
    }

    private void chkInteractive_CheckedChanged(object? sender, EventArgs e)
    {
        chkAutosquash.IsEnabled = chkInteractive.IsChecked == true;
    }

    private void MergetoolClick(object? sender, EventArgs e)
    {
        UICommands.StartResolveConflictsDialog(this);
        EnableButtons();
    }

    private void chkIgnoreDate_CheckedChanged(object? sender, EventArgs e)
    {
        ToggleDateCheckboxMutualExclusions();
    }

    private void chkCommitterDateIsAuthorDate_CheckedChanged(object? sender, EventArgs e)
    {
        ToggleDateCheckboxMutualExclusions();
    }

    private void ToggleDateCheckboxMutualExclusions()
    {
        chkCommitterDateIsAuthorDate.IsEnabled = chkIgnoreDate.IsChecked != true;
        chkIgnoreDate.IsEnabled = chkCommitterDateIsAuthorDate.IsChecked != true;
        chkPreserveMerges.IsEnabled =
            chkIgnoreDate.IsChecked != true
            && chkCommitterDateIsAuthorDate.IsChecked != true
            && Module.GitVersion.SupportRebaseMerges;
        chkInteractive.IsEnabled =
            chkIgnoreDate.IsChecked != true
            && chkCommitterDateIsAuthorDate.IsChecked != true;
        chkAutosquash.IsEnabled =
            chkInteractive.IsChecked == true
            && chkIgnoreDate.IsChecked != true
            && chkCommitterDateIsAuthorDate.IsChecked != true;
    }

    private void AddFilesClick(object? sender, EventArgs e)
    {
        UICommands.StartAddFilesDialog(this);
    }

    private void ResolvedClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: Commands.ContinueRebase(),
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            if (!Module.InTheMiddleOfRebase())
            {
                Close();
            }

            EnableButtons();
            if (Module.InTheMiddleOfRebase())
            {
                PatchGrid.Initialize();
            }
        }
    }

    private void SkipClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            PatchFile? applyingPatch = PatchGrid.PatchFiles?.FirstOrDefault(patchFile => patchFile.IsNext);
            if (applyingPatch is not null)
            {
                applyingPatch.IsSkipped = true;
                Skipped.Add(applyingPatch);
            }

            FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: Commands.SkipRebase(),
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            if (!Module.InTheMiddleOfRebase())
            {
                Close();
            }

            EnableButtons();
            if (Module.InTheMiddleOfRebase())
            {
                PatchGrid.RefreshGrid();
            }
        }
    }

    private void AbortClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: Commands.AbortRebase(),
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            if (!Module.InTheMiddleOfRebase())
            {
                Skipped.Clear();
                Close();
            }

            EnableButtons();
            if (Module.InTheMiddleOfRebase())
            {
                PatchGrid.Initialize();
            }
        }
    }

    private void EditTodoClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: Commands.EditTodoRebase(),
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            if (!Module.InTheMiddleOfRebase())
            {
                Skipped.Clear();
                Close();
            }

            EnableButtons();
            if (Module.InTheMiddleOfRebase())
            {
                PatchGrid.Initialize();
            }
        }
    }

    private void OkClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            string onto = GetComboText(cboBranches);
            if (string.IsNullOrEmpty(onto))
            {
                MessageBoxes.Show(
                    this,
                    _noBranchSelectedText.Text,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
                return;
            }

            AppSettings.RebaseAutoStash = chkStash.IsChecked == true;
            Skipped.Clear();

            bool? updateRefChoice = null;
            if (Module.GitVersion.SupportUpdateRefs
                && Module.GetEffectiveSetting<bool>("rebase.updaterefs") != checkBoxUpdateRefs.IsChecked)
            {
                updateRefChoice = checkBoxUpdateRefs.IsChecked == true;
            }

            Commands.RebaseOptions rebaseOptions = new()
            {
                Interactive = chkInteractive.IsChecked == true,
                PreserveMerges = chkPreserveMerges.IsChecked == true,
                AutoSquash = chkAutosquash.IsChecked == true,
                AutoStash = chkStash.IsChecked == true,
                IgnoreDate = chkIgnoreDate.IsChecked == true,
                CommitterDateIsAuthorDate = chkCommitterDateIsAuthorDate.IsChecked == true,
                UpdateRefs = updateRefChoice,
                SupportRebaseMerges = Module.GitVersion.SupportRebaseMerges,
            };

            string from = txtFrom.Text ?? string.Empty;
            string to = GetComboText(cboTo);
            if (chkSpecificRange.IsChecked == true
                && !string.IsNullOrWhiteSpace(from)
                && !string.IsNullOrWhiteSpace(to))
            {
                rebaseOptions.OnTo = onto;
                rebaseOptions.From = from;
                rebaseOptions.BranchName = to;
            }
            else
            {
                rebaseOptions.BranchName = onto;
            }

            string command = Commands.Rebase(rebaseOptions);
            string commandOutput = FormProcess.ReadDialog(
                this,
                UICommands,
                arguments: command,
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);
            if (commandOutput.Trim() == "Current branch a is up to date.")
            {
                MessageBoxes.Show(
                    this,
                    _branchUpToDateText.Text,
                    _branchUpToDateCaption.Text,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Information);
            }

            if (!Module.InTheMiddleOfAction() && !Module.InTheMiddleOfPatch())
            {
                UICommands.RepoChangedNotifier.Notify();
                Close();
            }

            EnableButtons();
            if (Module.InTheMiddleOfRebase())
            {
                PatchGrid.Initialize();
            }
        }
    }

    private void SolveMergeConflictsClick(object? sender, EventArgs e)
    {
        MergetoolClick(sender, e);
    }

    private void ShowOptions_LinkClicked(object? sender, EventArgs e)
    {
        llblShowOptions.IsVisible = false;
        flpnlOptionsPanelTop.IsVisible = true;
        flpnlOptionsPanelBottom.IsVisible = true;
    }

    private void chkUseFromOnto_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = chkSpecificRange.IsChecked == true;
        txtFrom.IsEnabled = enabled;
        cboTo.IsEnabled = enabled;
        btnChooseFromRevision.IsEnabled = enabled;
    }

    private void btnChooseFromRevision_Click(object? sender, EventArgs e)
    {
        bool previousValueBranchFilterEnabled = AppSettings.BranchFilterEnabled;
        bool previousValueShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
        bool previousValueShowReflogReferences = AppSettings.ShowReflogReferences;
        bool previousValueShowStashes = AppSettings.ShowStashes;

        try
        {
            AppSettings.ShowStashes = false;
            ObjectId firstParent = Module.RevParse("HEAD~");
            string preSelectedCommit = !string.IsNullOrWhiteSpace(txtFrom.Text)
                ? txtFrom.Text
                : firstParent.IsZero
                    ? string.Empty
                    : firstParent.ToString();
            string? mergeBaseCommitId = null;

            string onto = GetComboText(cboBranches);
            if (!string.IsNullOrWhiteSpace(onto))
            {
                try
                {
                    ObjectId commit1 = Module.RevParse(onto);
                    ObjectId commit2 = Module.RevParse("HEAD");
                    ObjectId mergeBase = Module.GetMergeBase(commit1, commit2);
                    mergeBaseCommitId = mergeBase.IsZero ? null : mergeBase.ToString();
                }
                catch (Exception)
                {
                    // If the target cannot be resolved, show the unfiltered current branch.
                }
            }

            using FormChooseCommit chooseForm = new(
                UICommands,
                preSelectedCommit,
                showCurrentBranchOnly: true,
                lastRevisionToDisplayHash: mergeBaseCommitId);
            if (chooseForm.ShowDialog(this) == WinFormsShims.DialogResult.OK
                && chooseForm.SelectedRevision is not null)
            {
                txtFrom.Text = chooseForm.SelectedRevision.ObjectId.ToShortString();
            }
        }
        finally
        {
            AppSettings.ShowStashes = previousValueShowStashes;
            AppSettings.BranchFilterEnabled.Value = previousValueBranchFilterEnabled;
            AppSettings.ShowCurrentBranchOnly.Value = previousValueShowCurrentBranchOnly;
            AppSettings.ShowReflogReferences.Value = previousValueShowReflogReferences;
        }
    }

    private void Commit_Click(object? sender, EventArgs e)
    {
        UICommands.StartCommitDialog(this);
        EnableButtons();
    }
}

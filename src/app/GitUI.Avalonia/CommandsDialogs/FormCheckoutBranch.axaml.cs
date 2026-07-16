using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
using GitUI.Compat;
using ResourceManager;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormCheckoutBranch.cs with the full local and remote flow.
// Deviations: the WinForms ErrorProvider validation becomes an on-checkout message box,
// Avalonia's SizeToContent replaces the ApplyLayout/RecalculateSizeConstraints row math,
// and the before/after-checkout event scripts wait for the ScriptsEngine phase (3.23).
public partial class FormCheckoutBranch : GitExtensionsDialog
{
    #region Translation
    private readonly TranslationString _customBranchNameIsEmpty =
        new("Custom branch name is empty.\nEnter valid branch name or select predefined value.");
    private readonly TranslationString _customBranchNameIsNotValid =
        new("“{0}” is not valid branch name.\nEnter valid branch name or select predefined value.");
    private readonly TranslationString _createBranch =
        new("Cr&eate local branch with same name:");
    private readonly TranslationString _applyStashedItemsAgainCaption =
        new("Auto stash");
    private readonly TranslationString _applyStashedItemsAgain =
        new("Apply stashed items to working directory again?");

    private readonly TranslationString _resetNonFastForwardBranch =
        new("You are going to reset the “{0}” branch to a new location discarding ALL the commited changes since the {1} revision.\n\nAre you sure?");
    private readonly TranslationString _resetCaption = new("Reset branch");
    #endregion

    private readonly IReadOnlyList<ObjectId>? _containObjectIds;
    private readonly bool _isLoading;
    private readonly string _rbResetBranchDefaultText = string.Empty;
    private TranslationString _invalidBranchName = new("An existing branch must be selected.");
    private bool? _isDirtyDir;
    private string _remoteName = "";
    private string _newLocalBranchName = "";
    private string _localBranchName = "";
    private readonly IGitBranchNameNormaliser _branchNameNormaliser = null!;
    private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);

    private IReadOnlyList<IGitRef>? _localBranches;
    private IReadOnlyList<IGitRef>? _remoteBranches;

    public FormCheckoutBranch()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormCheckoutBranch(IGitUICommands commands, string branch, bool remote, IReadOnlyList<ObjectId>? containObjectIds = null)
        : base(commands, true)
    {
        ArgumentNullException.ThrowIfNull(commands);

        _branchNameNormaliser = commands.GetRequiredService<IGitBranchNameNormaliser>();

        InitializeComponent();

        Ok.Click += OkClick;
        Branches.SelectionChanged += Branches_SelectedIndexChanged;
        LocalBranch.IsCheckedChanged += LocalBranchCheckedChanged;
        Remotebranch.IsCheckedChanged += RemoteBranchCheckedChanged;
        rbCreateBranchWithCustomName.IsCheckedChanged += rbCreateBranchWithCustomName_CheckedChanged;
        txtCustomBranchName.LostFocus += txtCustomBranchName_Leave;
        rbReset.IsCheckedChanged += rbReset_CheckedChanged;
        Activated += FormCheckoutBranch_Activated;

        AcceptButton = Ok;
        ManualSectionAnchorName = "checkout-branch";
        ManualSectionSubfolder = "branches";

        InitializeComplete();
        _rbResetBranchDefaultText = rbResetBranch.Content as string ?? string.Empty;
        _isLoading = true;

        try
        {
            _containObjectIds = containObjectIds;

            LocalBranch.IsChecked = !remote;
            Remotebranch.IsChecked = remote;

            PopulateBranches();

            // Set current branch after initialize, because initialize will reset it
            if (!string.IsNullOrEmpty(branch))
            {
                List<string> branchNames = [.. GetBranchNames()];
                if (!branchNames.Contains(branch, StringComparer.Ordinal))
                {
                    branchNames.Add(branch);
                    Branches.ItemsSource = branchNames;
                }

                Branches.SelectedItem = branch;
            }

            if (_containObjectIds is not null)
            {
                if (Branches.ItemCount == 0)
                {
                    LocalBranch.IsChecked = remote;
                    Remotebranch.IsChecked = !remote;
                    PopulateBranches();
                }
            }

            // The dirty check is very expensive on large repositories. Without this setting
            // the checkout branch dialog is too slow.
            _isDirtyDir = AppSettings.CheckForUncommittedChangesInCheckoutBranch
                ? Module.IsDirtyDir()
                : null;

            localChangesGB.IsVisible = HasUncommittedChanges;
            ChangesMode = AppSettings.CheckoutBranchAction;
            rbCreateBranchWithCustomName.IsChecked = AppSettings.CreateLocalBranchForRemote;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private LocalChangesAction ChangesMode
    {
        get
        {
            if (rbReset.IsChecked == true)
            {
                return LocalChangesAction.Reset;
            }

            if (rbMerge.IsChecked == true)
            {
                return LocalChangesAction.Merge;
            }

            if (rbStash.IsChecked == true)
            {
                return LocalChangesAction.Stash;
            }

            return LocalChangesAction.DontChange;
        }
        set
        {
            rbReset.IsChecked = value == LocalChangesAction.Reset;
            rbMerge.IsChecked = value == LocalChangesAction.Merge;
            rbStash.IsChecked = value == LocalChangesAction.Stash;
            rbDontChange.IsChecked = value == LocalChangesAction.DontChange;
        }
    }

    private bool HasUncommittedChanges => _isDirtyDir ?? true;

    public DialogResult DoDefaultActionOrShow(IWin32Window? owner)
    {
        bool localBranchSelected = !string.IsNullOrWhiteSpace(GetBranchText()) && Remotebranch.IsChecked != true;
        if (!AppSettings.AlwaysShowCheckoutBranchDlg && localBranchSelected &&
            (!HasUncommittedChanges || AppSettings.UseDefaultCheckoutBranchAction))
        {
            return PerformCheckout(owner);
        }

        return ShowDialog(owner);
    }

    private void PopulateBranches()
    {
        IEnumerable<string> branchNames;

        if (_containObjectIds is null)
        {
            // Keyed off Remotebranch: when it becomes checked, Avalonia has not
            // unchecked LocalBranch yet (unlike WinForms).
            IEnumerable<IGitRef> branches = Remotebranch.IsChecked == true ? GetRemoteBranches() : GetLocalBranches();

            branchNames = branches.Select(b => b.Name);
        }
        else
        {
            branchNames = GetContainsObjectIdBranches();
        }

        Branches.ItemsSource = branchNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (_containObjectIds is not null && Branches.ItemCount == 1)
        {
            Branches.SelectedIndex = 0;
        }
        else
        {
            Branches.SelectedItem = null;
        }
    }

    private IReadOnlyList<string> GetContainsObjectIdBranches()
    {
        HashSet<string> result = [];
        if (_containObjectIds is null || _containObjectIds.Count == 0)
        {
            return result.ToList();
        }

        result.UnionWith(GetBranchesContaining(_containObjectIds[0]));
        for (int index = 1; index < _containObjectIds.Count; index++)
        {
            result.IntersectWith(GetBranchesContaining(_containObjectIds[index]));
        }

        return result.ToList();

        IEnumerable<string> GetBranchesContaining(ObjectId objectId)
            => Module.GetAllBranchesWhichContainGivenCommit(
                    objectId,
                    getLocal: Remotebranch.IsChecked != true,
                    getRemote: Remotebranch.IsChecked == true,
                    cancellationToken: default)
                .Where(name => !DetachedHeadParser.IsDetachedHead(name) && !name.EndsWith("/HEAD"));
    }

    private void OkClick(object? sender, EventArgs e)
    {
        DialogResult result = PerformCheckout(this);
        if (result == DialogResult.OK)
        {
            DialogResult = result;
        }
    }

    private DialogResult PerformCheckout(IWin32Window? owner)
    {
        // Ok button set as the "AcceptButton" for the form
        // if the user hits [Enter] at any point, we need to trigger txtCustomBranchName Leave event
        Ok.Focus();

        string branchName = GetBranchText().Trim();
        bool isRemote = Remotebranch.IsChecked == true;
        string? newBranchName = null;
        CheckoutNewBranchMode newBranchMode = CheckoutNewBranchMode.DontCreate;

        // The WinForms ErrorProvider validation is a message box here.
        if (string.IsNullOrWhiteSpace(branchName) || !GetBranchNames().Contains(branchName, StringComparer.Ordinal))
        {
            MessageBoxes.Show(this, _invalidBranchName.Text, Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return DialogResult.None;
        }

        if (isRemote)
        {
            if (rbCreateBranchWithCustomName.IsChecked == true)
            {
                newBranchName = txtCustomBranchName.Text?.Trim();
                newBranchMode = CheckoutNewBranchMode.Create;
                if (string.IsNullOrWhiteSpace(newBranchName))
                {
                    MessageBoxes.Show(_customBranchNameIsEmpty.Text, Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return DialogResult.None;
                }

                if (!Module.CheckBranchFormat(newBranchName))
                {
                    MessageBoxes.Show(string.Format(_customBranchNameIsNotValid.Text, newBranchName), Text ?? string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return DialogResult.None;
                }
            }
            else if (rbResetBranch.IsChecked == true)
            {
                IGitRef? localBranchRef = GetLocalBranchRef(_localBranchName);
                IGitRef? remoteBranchRef = GetRemoteBranchRef(branchName);
                if (localBranchRef is not null && remoteBranchRef is not null && !localBranchRef.ObjectId.IsZero && !remoteBranchRef.ObjectId.IsZero)
                {
                    ObjectId mergeBaseId = Module.GetMergeBase(localBranchRef.ObjectId, remoteBranchRef.ObjectId);
                    bool isResetFastForward = localBranchRef.ObjectId == mergeBaseId;

                    if (!isResetFastForward)
                    {
                        string mergeBaseText = mergeBaseId.IsZero
                            ? "merge base"
                            : mergeBaseId.ToShortString();

                        string warningMessage = string.Format(_resetNonFastForwardBranch.Text, _localBranchName, mergeBaseText);

                        if (MessageBoxes.Show(this, warningMessage, _resetCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                        {
                            return DialogResult.None;
                        }
                    }
                }

                newBranchMode = CheckoutNewBranchMode.Reset;
                newBranchName = _localBranchName;
            }
            else
            {
                newBranchMode = CheckoutNewBranchMode.DontCreate;
            }
        }

        LocalChangesAction localChanges = ChangesMode;
        if (localChanges != LocalChangesAction.Reset && chkSetLocalChangesActionAsDefault.IsChecked == true)
        {
            AppSettings.CheckoutBranchAction = localChanges;
        }

        if ((!IsVisible && !AppSettings.UseDefaultCheckoutBranchAction) || !HasUncommittedChanges)
        {
            localChanges = LocalChangesAction.DontChange;
        }

        bool stash = false;
        if (localChanges == LocalChangesAction.Stash)
        {
            if (_isDirtyDir is null && IsVisible)
            {
                _isDirtyDir = Module.IsDirtyDir();
            }

            stash = _isDirtyDir == true;
            if (stash)
            {
                UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInAutoStash);
            }
        }

        ObjectId originalId = Module.GetCurrentCheckout();

        // TODO(avalonia-port): the BeforeCheckout/AfterCheckout event scripts arrive with
        // the ScriptsEngine port.

        if (UICommands.StartCommandLineProcessDialog(owner, Commands.CheckoutBranch(branchName, isRemote, localChanges, newBranchMode, newBranchName)))
        {
            if (stash)
            {
                bool? messageBoxResult = AppSettings.AutoPopStashAfterCheckoutBranch;
                if (messageBoxResult is null)
                {
                    TaskDialogPage page = new()
                    {
                        Text = _applyStashedItemsAgain.Text,
                        Caption = _applyStashedItemsAgainCaption.Text,
                        Icon = TaskDialogIcon.Information,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        Verification = new TaskDialogVerificationCheckBox
                        {
                            Text = TranslatedStrings.DontShowAgain
                        },
                        SizeToContent = true
                    };

                    messageBoxResult = TaskDialog.ShowDialog(this, page) == TaskDialogButton.Yes;

                    if (page.Verification.Checked)
                    {
                        AppSettings.AutoPopStashAfterCheckoutBranch = messageBoxResult;
                    }
                }

                if (messageBoxResult ?? false)
                {
                    UICommands.StashPop(this);
                }
            }

            ObjectId currentId = Module.GetCurrentCheckout();

            if (originalId != currentId)
            {
                UICommands.UpdateSubmodules(this);
            }

            return DialogResult.OK;
        }

        return DialogResult.None;

        IGitRef? GetLocalBranchRef(string name)
        {
            return GetLocalBranches().FirstOrDefault(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        IGitRef? GetRemoteBranchRef(string name)
        {
            return GetRemoteBranches().FirstOrDefault(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void LocalBranchCheckedChanged(object? sender, EventArgs e)
    {
        // We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
        ////BranchTypeChanged();
    }

    private void RemoteBranchCheckedChanged(object? sender, EventArgs e)
    {
        RecalculateSizeConstraints();

        if (!_isLoading)
        {
            PopulateBranches();
        }

        Branches_SelectedIndexChanged(sender, e);
    }

    private void rbCreateBranchWithCustomName_CheckedChanged(object? sender, EventArgs e)
    {
        txtCustomBranchName.IsEnabled = rbCreateBranchWithCustomName.IsChecked == true;
        if (rbCreateBranchWithCustomName.IsChecked == true)
        {
            txtCustomBranchName.SelectAll();
        }
    }

    private void Branches_SelectedIndexChanged(object? sender, EventArgs e)
    {
        lbChanges.Text = "";

        string branch = GetBranchText();
        if (string.IsNullOrWhiteSpace(branch) || Remotebranch.IsChecked != true)
        {
            _remoteName = string.Empty;
            _localBranchName = string.Empty;
            _newLocalBranchName = string.Empty;
        }
        else
        {
            _remoteName = GitRefName.GetRemoteName(branch, Module.GetRemoteNames());
            _localBranchName = Module.GetLocalTrackingBranchName(_remoteName, branch) ?? "";
            string remoteBranchName = _remoteName.Length > 0 ? branch[(_remoteName.Length + 1)..] : branch;
            _newLocalBranchName = string.Concat(_remoteName, "_", remoteBranchName);
            int i = 2;
            while (LocalBranchExists(_newLocalBranchName))
            {
                _newLocalBranchName = string.Concat(_remoteName, "_", _localBranchName, "_", i.ToString());
                i++;
            }
        }

        bool existsLocalBranch = LocalBranchExists(_localBranchName);

        rbResetBranch.Content = existsLocalBranch
            ? _rbResetBranchDefaultText
            : AvaloniaTranslationUtils.ToAvaloniaMnemonics(_createBranch.Text);
        branchName.Text = "'" + _localBranchName + "'";
        txtCustomBranchName.Text = _newLocalBranchName;

        if (string.IsNullOrWhiteSpace(branch))
        {
            lbChanges.Text = "";
        }
        else
        {
            ThreadHelper.FileAndForget(async () =>
            {
                // not applicable if there is no checkout yet
                string aheadBehindInfo = "";

                ObjectId currentCheckout = Module.GetCurrentCheckout();
                if (!currentCheckout.IsZero)
                {
                    aheadBehindInfo = Module.GetCommitCountString(currentCheckout, branch);
                }

                await this.SwitchToMainThreadAsync();

                if (GetBranchText() == branch)
                {
                    lbChanges.Text = aheadBehindInfo;
                }
            });
        }

        return;

        bool LocalBranchExists(string name)
        {
            return GetLocalBranches().Any(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    private IEnumerable<IGitRef> GetLocalBranches()
        => _localBranches ??= Module.GetRefs(RefsFilter.Heads);

    private IEnumerable<IGitRef> GetRemoteBranches()
        => _remoteBranches ??= Module.GetRefs(RefsFilter.Remotes);

    private IReadOnlyList<string> GetBranchNames()
        => Branches.ItemsSource?.OfType<string>().ToList() ?? [];

    /// <summary>
    ///  The WinForms Branches.Text: the editable combo box may raise SelectionChanged
    ///  before its Text catches up, so prefer the selected item.
    /// </summary>
    private string GetBranchText()
        => Branches.SelectedItem as string ?? Branches.Text ?? string.Empty;

    private void FormCheckoutBranch_Activated(object? sender, EventArgs e)
    {
        Branches.Focus();
    }

    private void RecalculateSizeConstraints()
    {
        // SizeToContent tracks the visibility change; the WinForms row math is unnecessary.
        tlpnlRemoteOptions.IsVisible = Remotebranch.IsChecked == true;
    }

    private void rbReset_CheckedChanged(object? sender, EventArgs e)
    {
        chkSetLocalChangesActionAsDefault.IsEnabled = rbReset.IsChecked != true;
        if (rbReset.IsChecked == true)
        {
            chkSetLocalChangesActionAsDefault.IsChecked = false;
        }
    }

    private void txtCustomBranchName_Leave(object? sender, EventArgs e)
    {
        string customBranchName = txtCustomBranchName.Text ?? string.Empty;
        if (!AppSettings.AutoNormaliseBranchName || !customBranchName.Any(PathUtil.IsValidPathChar))
        {
            return;
        }

        int caretPosition = txtCustomBranchName.CaretIndex;
        txtCustomBranchName.Text = _branchNameNormaliser.Normalise(customBranchName, _gitBranchNameOptions);
        txtCustomBranchName.CaretIndex = Math.Min(caretPosition, txtCustomBranchName.Text.Length);
    }
}

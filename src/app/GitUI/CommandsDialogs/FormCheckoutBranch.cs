using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
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

        private readonly IReadOnlyList<ObjectId>? _containRevisions;
        private readonly bool _isLoading;
        private readonly string _rbResetBranchDefaultText;
        private TranslationString _invalidBranchName = new("An existing branch must be selected.");
        private bool? _isDirtyDir;
        private string _remoteName = "";
        private string _newLocalBranchName = "";
        private string _localBranchName = "";
        private readonly IGitBranchNameNormaliser _branchNameNormaliser;
        private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);
        private readonly Dictionary<Control, int> _controls = [];

        private IReadOnlyList<IGitRef>? _localBranches;
        private IReadOnlyList<IGitRef>? _remoteBranches;

        public FormCheckoutBranch(IGitUICommands commands, string branch, bool remote, IReadOnlyList<ObjectId>? containRevisions = null)
            : base(commands, true)
        {
            _branchNameNormaliser = new GitBranchNameNormaliser();
            InitializeComponent();
            InitializeComplete();
            _rbResetBranchDefaultText = rbResetBranch.Text;

            ApplyLayout();
            Shown += FormCheckoutBranch_Shown;
            _isLoading = true;

            try
            {
                _containRevisions = containRevisions;

                LocalBranch.Checked = !remote;
                Remotebranch.Checked = remote;

                PopulateBranches();

                // Set current branch after initialize, because initialize will reset it
                if (!string.IsNullOrEmpty(branch))
                {
                    Branches.Items.Add(branch);
                    Branches.SelectedItem = branch;
                }

                if (containRevisions is not null)
                {
                    if (Branches.Items.Count == 0)
                    {
                        LocalBranch.Checked = remote;
                        Remotebranch.Checked = !remote;
                        PopulateBranches();
                    }
                }

                // The dirty check is very expensive on large repositories. Without this setting
                // the checkout branch dialog is too slow.
                if (AppSettings.CheckForUncommittedChangesInCheckoutBranch)
                {
                    _isDirtyDir = Module.IsDirtyDir();
                }
                else
                {
                    _isDirtyDir = null;
                }

                localChangesGB.Visible = HasUncommittedChanges;
                ChangesMode = AppSettings.CheckoutBranchAction;
                rbCreateBranchWithCustomName.Checked = AppSettings.CreateLocalBranchForRemote;
            }
            finally
            {
                _isLoading = false;
            }

            return;

            void FormCheckoutBranch_Shown(object sender, EventArgs e)
            {
                Shown -= FormCheckoutBranch_Shown;
                RecalculateSizeConstraints();
            }
        }

        private LocalChangesAction ChangesMode
        {
            get
            {
                if (rbReset.Checked)
                {
                    return LocalChangesAction.Reset;
                }

                if (rbMerge.Checked)
                {
                    return LocalChangesAction.Merge;
                }

                if (rbStash.Checked)
                {
                    return LocalChangesAction.Stash;
                }

                return LocalChangesAction.DontChange;
            }
            set
            {
                rbReset.Checked = value == LocalChangesAction.Reset;
                rbMerge.Checked = value == LocalChangesAction.Merge;
                rbStash.Checked = value == LocalChangesAction.Stash;
                rbDontChange.Checked = value == LocalChangesAction.DontChange;
            }
        }

        private bool HasUncommittedChanges => _isDirtyDir ?? true;

        public DialogResult DoDefaultActionOrShow(IWin32Window? owner)
        {
            bool localBranchSelected = !string.IsNullOrWhiteSpace(Branches.Text) && !Remotebranch.Checked;
            if (!AppSettings.AlwaysShowCheckoutBranchDlg && localBranchSelected &&
                (!HasUncommittedChanges || AppSettings.UseDefaultCheckoutBranchAction))
            {
                return PerformCheckout(owner);
            }

            return ShowDialog(owner);
        }

        private void ApplyLayout()
        {
            Control[] controls1 = new Control[]
            {
                tlpnlBranches,
                horLine,
                tlpnlRemoteOptions,
                localChangesGB
            };

            localChangesGB.AutoSize = true;
            localChangesGB.Dock = DockStyle.Fill;

            Width = tlpnlMain.PreferredSize.Width + 50;

            int height;
            for (int i = 0; i < controls1.Length; i++)
            {
                Padding margin = controls1[i].Margin;
                height = controls1[i].Height + margin.Top + margin.Bottom;
                _controls.Add(controls1[i], height);

                tlpnlMain.RowStyles[i].Height = height;
                tlpnlMain.RowStyles[i].SizeType = SizeType.Absolute;

                controls1[i].Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                controls1[i].AutoSize = true;
            }

            tlpnlMain.RowStyles[2].Height = Remotebranch.Checked ? _controls[tlpnlRemoteOptions] : 0;
            tlpnlMain.Height = tlpnlMain.Height - _controls[tlpnlRemoteOptions];
        }

        private void PopulateBranches()
        {
            Branches.Items.Clear();

            IEnumerable<string> branchNames;

            if (_containRevisions is null)
            {
                IEnumerable<IGitRef> branches = LocalBranch.Checked ? GetLocalBranches() : GetRemoteBranches();

                branchNames = branches.Select(b => b.Name);
            }
            else
            {
                branchNames = GetContainsRevisionBranches();
            }

            Branches.Items.AddRange(branchNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray<object>());

            if (_containRevisions is not null && Branches.Items.Count == 1)
            {
                Branches.SelectedIndex = 0;
            }
            else
            {
                Branches.Text = null;
            }

            IReadOnlyList<string> GetContainsRevisionBranches()
            {
                HashSet<string> result = [];
                if (_containRevisions.Count > 0)
                {
                    IEnumerable<string> branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisions[0],
                                                                                                getLocal: LocalBranch.Checked,
                                                                                                getRemote: !LocalBranch.Checked,
                                                                                                cancellationToken: default)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                    result.UnionWith(branches);
                }

                for (int index = 1; index < _containRevisions.Count; index++)
                {
                    ObjectId containRevision = _containRevisions[index];
                    IEnumerable<string> branches =
                        Module.GetAllBranchesWhichContainGivenCommit(containRevision,
                                                                     getLocal: LocalBranch.Checked,
                                                                     getRemote: !LocalBranch.Checked,
                                                                     cancellationToken: default)
                            .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                        !a.EndsWith("/HEAD"));
                    result.IntersectWith(branches);
                }

                return result.ToList();
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            DialogResult = PerformCheckout(this);
            if (DialogResult == DialogResult.OK)
            {
                Close();
            }
        }

        private DialogResult PerformCheckout(IWin32Window? owner)
        {
            // Ok button set as the "AcceptButton" for the form
            // if the user hits [Enter] at any point, we need to trigger txtCustomBranchName Leave event
            Ok.Focus();

            string branchName = Branches.Text.Trim();
            bool isRemote = Remotebranch.Checked;
            string newBranchName = (string?)null;
            CheckoutNewBranchMode newBranchMode = CheckoutNewBranchMode.DontCreate;

            if (isRemote)
            {
                if (rbCreateBranchWithCustomName.Checked)
                {
                    newBranchName = txtCustomBranchName.Text.Trim();
                    newBranchMode = CheckoutNewBranchMode.Create;
                    if (string.IsNullOrWhiteSpace(newBranchName))
                    {
                        MessageBox.Show(_customBranchNameIsEmpty.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }

                    if (!Module.CheckBranchFormat(newBranchName))
                    {
                        MessageBox.Show(string.Format(_customBranchNameIsNotValid.Text, newBranchName), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }
                }
                else if (rbResetBranch.Checked)
                {
                    IGitRef localBranchRef = GetLocalBranchRef(_localBranchName);
                    IGitRef remoteBranchRef = GetRemoteBranchRef(branchName);
                    if (localBranchRef is not null && remoteBranchRef is not null && localBranchRef.ObjectId is not null && remoteBranchRef.ObjectId is not null)
                    {
                        ObjectId mergeBaseGuid = Module.GetMergeBase(localBranchRef.ObjectId, remoteBranchRef.ObjectId);
                        bool isResetFastForward = localBranchRef.ObjectId == mergeBaseGuid;

                        if (!isResetFastForward)
                        {
                            string mergeBaseText = mergeBaseGuid is null
                                ? "merge base"
                                : mergeBaseGuid.ToShortString();

                            string warningMessage = string.Format(_resetNonFastForwardBranch.Text, _localBranchName, mergeBaseText);

                            if (MessageBox.Show(this, warningMessage, _resetCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                            {
                                DialogResult = DialogResult.None;
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
            if (localChanges != LocalChangesAction.Reset && chkSetLocalChangesActionAsDefault.Checked)
            {
                AppSettings.CheckoutBranchAction = localChanges;
            }

            if ((!Visible && !AppSettings.UseDefaultCheckoutBranchAction) || !HasUncommittedChanges)
            {
                localChanges = LocalChangesAction.DontChange;
            }

            bool stash = false;
            if (localChanges == LocalChangesAction.Stash)
            {
                if (_isDirtyDir is null && Visible)
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

            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeCheckout, this);
            if (!success)
            {
                return DialogResult.Cancel;
            }

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

                        messageBoxResult = TaskDialog.ShowDialog(Handle, page) == TaskDialogButton.Yes;

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

                ScriptsRunner.RunEventScripts(ScriptEvent.AfterCheckout, this);

                return DialogResult.OK;
            }

            return DialogResult.None;

            IGitRef GetLocalBranchRef(string name)
            {
                return GetLocalBranches().FirstOrDefault(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            IGitRef GetRemoteBranchRef(string name)
            {
                return GetRemoteBranches().FirstOrDefault(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void LocalBranchCheckedChanged(object sender, EventArgs e)
        {
            // We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
            ////BranchTypeChanged();
        }

        private void RemoteBranchCheckedChanged(object sender, EventArgs e)
        {
            RecalculateSizeConstraints();

            if (!_isLoading)
            {
                PopulateBranches();
            }

            Branches_SelectedIndexChanged(sender, e);
        }

        private void rbCreateBranchWithCustomName_CheckedChanged(object sender, EventArgs e)
        {
            txtCustomBranchName.Enabled = rbCreateBranchWithCustomName.Checked;
            if (rbCreateBranchWithCustomName.Checked)
            {
                txtCustomBranchName.SelectAll();
            }
        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbChanges.Text = "";

            string branch = Branches.Text;
            if (string.IsNullOrWhiteSpace(branch) || !Remotebranch.Checked)
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

            rbResetBranch.Text = existsLocalBranch ? _rbResetBranchDefaultText : _createBranch.Text;
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

                    ObjectId? currentCheckout = Module.GetCurrentCheckout();
                    if (currentCheckout is not null)
                    {
                        aheadBehindInfo = Module.GetCommitCountString(currentCheckout, branch);
                    }

                    await this.SwitchToMainThreadAsync();

                    lbChanges.Text = aheadBehindInfo;
                });
            }

            return;

            bool LocalBranchExists(string name)
            {
                return GetLocalBranches().Any(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        private IEnumerable<IGitRef> GetLocalBranches()
        {
            return _localBranches ??= Module.GetRefs(RefsFilter.Heads);
        }

        private IEnumerable<IGitRef> GetRemoteBranches()
        {
            return _remoteBranches ??= Module.GetRefs(RefsFilter.Remotes);
        }

        private void FormCheckoutBranch_Activated(object sender, EventArgs e)
        {
            Branches.Focus();
        }

        private void RecalculateSizeConstraints()
        {
            SuspendLayout();
            MinimumSize = MaximumSize = Size.Empty;

            tlpnlRemoteOptions.Visible = Remotebranch.Checked;
            tlpnlMain.RowStyles[2].Height = Remotebranch.Checked ? _controls[tlpnlRemoteOptions] : 0;
            tlpnlMain.Height = _controls.Select(c => c.Key.Visible ? c.Value : 0).Sum() + tlpnlMain.Padding.Top + tlpnlMain.Padding.Bottom;
            int height = ControlsPanel.Height + MainPanel.Padding.Top + MainPanel.Padding.Bottom
                       + tlpnlMain.Height + tlpnlMain.Margin.Top + tlpnlMain.Margin.Bottom + DpiUtil.Scale(30);

            MinimumSize = new Size(tlpnlMain.PreferredSize.Width + DpiUtil.Scale(70), height);
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, height);
            Size = new Size(Width, height);
            ResumeLayout();
        }

        private void rbReset_CheckedChanged(object sender, EventArgs e)
        {
            chkSetLocalChangesActionAsDefault.Enabled = !rbReset.Checked;
            if (rbReset.Checked)
            {
                chkSetLocalChangesActionAsDefault.Checked = false;
            }
        }

        private void txtCustomBranchName_Leave(object sender, EventArgs e)
        {
            if (!AppSettings.AutoNormaliseBranchName || !txtCustomBranchName.Text.Any(GitBranchNameNormaliser.IsValidChar))
            {
                return;
            }

            int caretPosition = txtCustomBranchName.SelectionStart;
            string normalisedBranchName = _branchNameNormaliser.Normalise(txtCustomBranchName.Text, _gitBranchNameOptions);
            txtCustomBranchName.Text = normalisedBranchName;
            txtCustomBranchName.SelectionStart = caretPosition;
        }

        private void Branches_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = Branches.SelectedIndex == -1 || !Branches.Items.Contains(Branches.Text);
            Errors.SetError(Branches, e.Cancel ? _invalidBranchName.ToString() : "");
        }

        private void Branches_TextChanged(object sender, EventArgs e)
        {
            Errors.SetError(Branches, "");
        }
    }
}

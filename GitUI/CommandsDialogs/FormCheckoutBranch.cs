using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.Script;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using PSTaskDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCheckoutBranch : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _customBranchNameIsEmpty =
            new TranslationString("Custom branch name is empty.\nEnter valid branch name or select predefined value.");
        private readonly TranslationString _customBranchNameIsNotValid =
            new TranslationString("“{0}” is not valid branch name.\nEnter valid branch name or select predefined value.");
        private readonly TranslationString _createBranch =
            new TranslationString("Create local branch with the name:");
        private readonly TranslationString _applyStashedItemsAgainCaption =
            new TranslationString("Auto stash");
        private readonly TranslationString _applyStashedItemsAgain =
            new TranslationString("Apply stashed items to working directory again?");

        private readonly TranslationString _dontShowAgain =
            new TranslationString("Don't show me this message again.");

        private readonly TranslationString _resetNonFastForwardBranch =
            new TranslationString("You are going to reset the “{0}” branch to a new location discarding ALL the commited changes since the {1} revision.\n\nAre you sure?");
        private readonly TranslationString _resetCaption = new TranslationString("Reset branch");
        #endregion

        private readonly IReadOnlyList<ObjectId> _containRevisions;
        private readonly bool _isLoading;
        private readonly string _rbResetBranchDefaultText;
        private TranslationString _invalidBranchName = new TranslationString("An existing branch must be selected.");
        private bool? _isDirtyDir;
        private string _remoteName = "";
        private string _newLocalBranchName = "";
        private string _localBranchName = "";
        private readonly IGitBranchNameNormaliser _branchNameNormaliser;
        private readonly GitBranchNameOptions _gitBranchNameOptions = new GitBranchNameOptions(AppSettings.AutoNormaliseSymbol);
        private readonly Dictionary<Control, int> _controls = new Dictionary<Control, int>();

        private IReadOnlyList<IGitRef> _localBranches;
        private IReadOnlyList<IGitRef> _remoteBranches;

        private IScriptManager _scriptManager;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCheckoutBranch()
        {
            InitializeComponent();
        }

        internal FormCheckoutBranch(GitUICommands commands)
            : base(commands)
        {
            _branchNameNormaliser = new GitBranchNameNormaliser();
            InitializeComponent();
            InitializeComplete();
            _rbResetBranchDefaultText = rbResetBranch.Text;

            ApplyLayout();
            Shown += FormCheckoutBranch_Shown;

            _scriptManager = new ScriptManager();

            return;

            void FormCheckoutBranch_Shown(object sender, EventArgs e)
            {
                Shown -= FormCheckoutBranch_Shown;
                RecalculateSizeConstraints();
            }
        }

        public FormCheckoutBranch(GitUICommands commands, string branch, bool remote, IReadOnlyList<ObjectId> containRevisions = null)
            : this(commands)
        {
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

                if (containRevisions != null)
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

        public DialogResult DoDefaultActionOrShow(IWin32Window owner)
        {
            bool localBranchSelected = !Branches.Text.IsNullOrWhiteSpace() && !Remotebranch.Checked;
            if (!AppSettings.AlwaysShowCheckoutBranchDlg && localBranchSelected &&
                (!HasUncommittedChanges || AppSettings.UseDefaultCheckoutBranchAction))
            {
                return OkClick();
            }

            return ShowDialog(owner);
        }

        private void ApplyLayout()
        {
            var controls1 = new Control[]
            {
                setBranchPanel,
                horLine,
                remoteOptionsPanel,
                localChangesPanel
            };

            Ok.Anchor = AnchorStyles.Right;

            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Dock = DockStyle.Fill;
            localChangesGB.AutoSize = true;
            localChangesGB.Height = (flowLayoutPanel2.Height * 2) + localChangesGB.Padding.Top + localChangesGB.Padding.Bottom;
            localChangesPanel.ColumnStyles[1].Width = Ok.Width + 10;
            var height = localChangesGB.Height + localChangesGB.Margin.Top + localChangesGB.Margin.Bottom;
            localChangesPanel.RowStyles[0].Height = height;
            localChangesPanel.Height = height;

            Width = tableLayoutPanel1.PreferredSize.Width + 40;

            for (var i = 0; i < controls1.Length; i++)
            {
                var margin = controls1[i].Margin;
                height = controls1[i].Height + margin.Top + margin.Bottom;
                _controls.Add(controls1[i], height);

                tableLayoutPanel1.RowStyles[i].Height = height;
                tableLayoutPanel1.RowStyles[i].SizeType = SizeType.Absolute;
            }

            tableLayoutPanel1.RowStyles[2].Height = Remotebranch.Checked ? _controls[remoteOptionsPanel] : 0;
            tableLayoutPanel1.Height = tableLayoutPanel1.Height - _controls[remoteOptionsPanel];
        }

        private void PopulateBranches()
        {
            Branches.Items.Clear();

            IEnumerable<string> branchNames;

            if (_containRevisions == null)
            {
                var branches = LocalBranch.Checked ? GetLocalBranches() : GetRemoteBranches();

                branchNames = branches.Select(b => b.Name);
            }
            else
            {
                branchNames = GetContainsRevisionBranches();
            }

            Branches.Items.AddRange(branchNames.Where(name => name.IsNotNullOrWhitespace()).ToArray<object>());

            if (_containRevisions != null && Branches.Items.Count == 1)
            {
                Branches.SelectedIndex = 0;
            }
            else
            {
                Branches.Text = null;
            }

            IReadOnlyList<string> GetContainsRevisionBranches()
            {
                var result = new HashSet<string>();
                if (_containRevisions.Count > 0)
                {
                    var branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisions[0], LocalBranch.Checked,
                            !LocalBranch.Checked)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                    result.UnionWith(branches);
                }

                for (int index = 1; index < _containRevisions.Count; index++)
                {
                    var containRevision = _containRevisions[index];
                    var branches =
                        Module.GetAllBranchesWhichContainGivenCommit(containRevision, LocalBranch.Checked,
                                !LocalBranch.Checked)
                            .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                        !a.EndsWith("/HEAD"));
                    result.IntersectWith(branches);
                }

                return result.ToList();
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            DialogResult = OkClick();
            if (DialogResult == DialogResult.OK)
            {
                Close();
            }
        }

        private DialogResult OkClick()
        {
            // Ok button set as the "AcceptButton" for the form
            // if the user hits [Enter] at any point, we need to trigger txtCustomBranchName Leave event
            Ok.Focus();

            var branchName = Branches.Text.Trim();
            var isRemote = Remotebranch.Checked;
            var newBranchName = (string)null;
            var newBranchMode = CheckoutNewBranchMode.DontCreate;

            if (isRemote)
            {
                if (rbCreateBranchWithCustomName.Checked)
                {
                    newBranchName = txtCustomBranchName.Text.Trim();
                    newBranchMode = CheckoutNewBranchMode.Create;
                    if (newBranchName.IsNullOrWhiteSpace())
                    {
                        MessageBox.Show(_customBranchNameIsEmpty.Text, Text);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }

                    if (!Module.CheckBranchFormat(newBranchName))
                    {
                        MessageBox.Show(string.Format(_customBranchNameIsNotValid.Text, newBranchName), Text);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }
                }
                else if (rbResetBranch.Checked)
                {
                    IGitRef localBranchRef = GetLocalBranchRef(_localBranchName);
                    IGitRef remoteBranchRef = GetRemoteBranchRef(branchName);
                    if (localBranchRef != null && remoteBranchRef != null)
                    {
                        var mergeBaseGuid = Module.GetMergeBase(localBranchRef.ObjectId, remoteBranchRef.ObjectId);
                        var isResetFastForward = localBranchRef.Guid == mergeBaseGuid?.ToString();

                        if (!isResetFastForward)
                        {
                            string mergeBaseText = mergeBaseGuid == null
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

            var localChanges = ChangesMode;
            if (localChanges != LocalChangesAction.Reset && chkSetLocalChangesActionAsDefault.Checked)
            {
                AppSettings.CheckoutBranchAction = localChanges;
            }

            if ((!Visible && !AppSettings.UseDefaultCheckoutBranchAction) || !HasUncommittedChanges)
            {
                localChanges = LocalChangesAction.DontChange;
            }

            IWin32Window owner = Visible ? this : Owner;

            bool stash = false;
            if (localChanges == LocalChangesAction.Stash)
            {
                if (_isDirtyDir == null && Visible)
                {
                    _isDirtyDir = Module.IsDirtyDir();
                }

                stash = _isDirtyDir == true;
                if (stash)
                {
                    UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInAutoStash);
                }
            }

            var originalId = Module.GetCurrentCheckout();

            Debug.Assert(originalId != null, "originalId != null");

            _scriptManager.RunEventScripts(this, ScriptEvent.BeforeCheckout);

            if (UICommands.StartCommandLineProcessDialog(owner, new GitCheckoutBranchCmd(branchName, isRemote, localChanges, newBranchMode, newBranchName)))
            {
                if (stash)
                {
                    bool? messageBoxResult = AppSettings.AutoPopStashAfterCheckoutBranch;
                    if (messageBoxResult == null)
                    {
                        DialogResult res = cTaskDialog.MessageBox(
                            this,
                            _applyStashedItemsAgainCaption.Text,
                            "",
                            _applyStashedItemsAgain.Text,
                            "",
                            "",
                            _dontShowAgain.Text,
                            eTaskDialogButtons.YesNo,
                            eSysIcons.Question,
                            eSysIcons.Question);
                        messageBoxResult = res == DialogResult.Yes;
                        if (cTaskDialog.VerificationChecked)
                        {
                            AppSettings.AutoPopStashAfterCheckoutBranch = messageBoxResult;
                        }
                    }

                    if (messageBoxResult ?? false)
                    {
                        UICommands.StashPop(this);
                    }
                }

                var currentId = Module.GetCurrentCheckout();

                if (originalId != currentId)
                {
                    UICommands.UpdateSubmodules(this);
                }

                _scriptManager.RunEventScripts(this, ScriptEvent.AfterCheckout);

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

            var branch = Branches.Text;
            if (branch.IsNullOrWhiteSpace() || !Remotebranch.Checked)
            {
                _remoteName = string.Empty;
                _localBranchName = string.Empty;
                _newLocalBranchName = string.Empty;
            }
            else
            {
                _remoteName = GitRefName.GetRemoteName(branch, Module.GetRemoteNames());
                _localBranchName = Module.GetLocalTrackingBranchName(_remoteName, branch);
                var remoteBranchName = _remoteName.Length > 0 ? branch.Substring(_remoteName.Length + 1) : branch;
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

            if (branch.IsNullOrWhiteSpace())
            {
                lbChanges.Text = "";
            }
            else
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default;

                        var currentCheckout = Module.GetCurrentCheckout();

                        Debug.Assert(currentCheckout != null, "currentCheckout != null");

                        var text = Module.GetCommitCountString(currentCheckout.ToString(), branch);

                        await this.SwitchToMainThreadAsync();

                        lbChanges.Text = text;
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
            if (_localBranches == null)
            {
                _localBranches = Module.GetRefs(false);
            }

            return _localBranches;
        }

        private IEnumerable<IGitRef> GetRemoteBranches()
        {
            if (_remoteBranches == null)
            {
                _remoteBranches = Module.GetRefs(true, true).Where(h => h.IsRemote && !h.IsTag).ToList();
            }

            return _remoteBranches;
        }

        private void FormCheckoutBranch_Activated(object sender, EventArgs e)
        {
            Branches.Focus();
        }

        private void RecalculateSizeConstraints()
        {
            MinimumSize = MaximumSize = new Size(0, 0);

            remoteOptionsPanel.Visible = Remotebranch.Checked;
            tableLayoutPanel1.RowStyles[2].Height = Remotebranch.Checked ? _controls[remoteOptionsPanel] : 0;
            tableLayoutPanel1.Height = _controls.Select(c => c.Key.Visible ? c.Value : 0).Sum() + tableLayoutPanel1.Padding.Top + tableLayoutPanel1.Padding.Bottom;
            Height = tableLayoutPanel1.Height + tableLayoutPanel1.Margin.Top + tableLayoutPanel1.Margin.Bottom + 40;

            MinimumSize = new Size(tableLayoutPanel1.PreferredSize.Width + 40, Height);
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Height);
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

            var caretPosition = txtCustomBranchName.SelectionStart;
            var normalisedBranchName = _branchNameNormaliser.Normalise(txtCustomBranchName.Text, _gitBranchNameOptions);
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
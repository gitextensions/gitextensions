using System;
using System.Collections.Generic;
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
        private readonly TranslationString _applyShashedItemsAgainCaption =
            new TranslationString("Auto stash");
        private readonly TranslationString _applyShashedItemsAgain =
            new TranslationString("Apply stashed items to working directory again?");

        private readonly TranslationString _dontShowAgain =
            new TranslationString("Don't show me this message again.");

        private readonly TranslationString _resetNonFastForwardBranch =
            new TranslationString("You are going to reset the “{0}” branch to a new location\n" +
                "discarding ALL the commited changes since the {1} revision.\n\nAre you sure?");
        private readonly TranslationString _resetCaption = new TranslationString("Reset branch");
        #endregion

        private readonly string[] _containRevisons;
        private readonly bool _isLoading;
        private readonly string _rbResetBranchDefaultText;
        private bool? _isDirtyDir;
        private string _remoteName = "";
        private string _newLocalBranchName = "";
        private string _localBranchName = "";
        private readonly IGitBranchNameNormaliser _branchNameNormaliser;
        private readonly GitBranchNameOptions _gitBranchNameOptions = new GitBranchNameOptions(AppSettings.AutoNormaliseSymbol);
        private readonly Dictionary<Control, int> _controls = new Dictionary<Control, int>();

        private IEnumerable<IGitRef> _localBranches;
        private IEnumerable<IGitRef> _remoteBranches;

        private FormCheckoutBranch()
            : this(null)
        {
        }

        internal FormCheckoutBranch(GitUICommands commands)
            : base(commands)
        {
            _branchNameNormaliser = new GitBranchNameNormaliser();
            InitializeComponent();
            Translate();
            _rbResetBranchDefaultText = rbResetBranch.Text;

            ApplyLayout();
            Shown += FormCheckoutBranch_Shown;
        }

        public FormCheckoutBranch(GitUICommands commands, string branch, bool remote, string[] containRevisons = null)
            : this(commands)
        {
            _isLoading = true;

            try
            {
                _containRevisons = containRevisons;

                LocalBranch.Checked = !remote;
                Remotebranch.Checked = remote;

                PopulateBranches();

                // Set current branch after initialize, because initialize will reset it
                if (!string.IsNullOrEmpty(branch))
                {
                    Branches.Items.Add(branch);
                    Branches.SelectedItem = branch;
                }

                if (containRevisons != null)
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
            if (IsUICommandsInitialized)
            {
                Branches.Items.Clear();

                IEnumerable<string> branchNames;

                if (_containRevisons == null)
                {
                    var branches = LocalBranch.Checked ? GetLocalBranches() : GetRemoteBranches();

                    branchNames = branches.Select(b => b.Name);
                }
                else
                {
                    branchNames = GetContainsRevisionBranches();
                }

                Branches.Items.AddRange(branchNames.Where(name => name.IsNotNullOrWhitespace()).ToArray());

                if (_containRevisons != null && Branches.Items.Count == 1)
                {
                    Branches.SelectedIndex = 0;
                }
                else
                {
                    Branches.Text = null;
                }
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

            GitCheckoutBranchCmd cmd = new GitCheckoutBranchCmd(Branches.Text.Trim(), Remotebranch.Checked);

            if (Remotebranch.Checked)
            {
                if (rbCreateBranchWithCustomName.Checked)
                {
                    cmd.NewBranchName = txtCustomBranchName.Text.Trim();
                    cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                    if (cmd.NewBranchName.IsNullOrWhiteSpace())
                    {
                        MessageBox.Show(_customBranchNameIsEmpty.Text, Text);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }

                    if (!Module.CheckBranchFormat(cmd.NewBranchName))
                    {
                        MessageBox.Show(string.Format(_customBranchNameIsNotValid.Text, cmd.NewBranchName), Text);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }
                }
                else if (rbResetBranch.Checked)
                {
                    IGitRef localBranchRef = GetLocalBranchRef(_localBranchName);
                    IGitRef remoteBranchRef = GetRemoteBranchRef(cmd.BranchName);
                    if (localBranchRef != null && remoteBranchRef != null)
                    {
                        var mergeBaseGuid = Module.GetMergeBase(localBranchRef.Guid, remoteBranchRef.Guid);
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

                    cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;
                    cmd.NewBranchName = _localBranchName;
                }
                else
                {
                    cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.DontCreate;
                    cmd.NewBranchName = null;
                }
            }

            LocalChangesAction changes = ChangesMode;
            if (changes != LocalChangesAction.Reset &&
                chkSetLocalChangesActionAsDefault.Checked)
            {
                AppSettings.CheckoutBranchAction = changes;
            }

            if ((Visible || AppSettings.UseDefaultCheckoutBranchAction) && HasUncommittedChanges)
            {
                cmd.LocalChanges = changes;
            }
            else
            {
                cmd.LocalChanges = LocalChangesAction.DontChange;
            }

            IWin32Window owner = Visible ? this : Owner;

            bool stash = false;
            if (changes == LocalChangesAction.Stash)
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

            var originalHash = Module.GetCurrentCheckout();

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforeCheckout);

            if (UICommands.StartCommandLineProcessDialog(owner, cmd))
            {
                if (stash)
                {
                    bool? messageBoxResult = AppSettings.AutoPopStashAfterCheckoutBranch;
                    if (messageBoxResult == null)
                    {
                        DialogResult res = cTaskDialog.MessageBox(
                            this,
                            _applyShashedItemsAgainCaption.Text,
                            "",
                            _applyShashedItemsAgain.Text,
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

                var currentHash = Module.GetCurrentCheckout();
                if (!string.Equals(originalHash, currentHash, StringComparison.OrdinalIgnoreCase))
                {
                    UICommands.UpdateSubmodules(this);
                }

                ScriptManager.RunEventScripts(this, ScriptEvent.AfterCheckout);

                return DialogResult.OK;
            }

            return DialogResult.None;
        }

        private void BranchTypeChanged()
        {
            if (!_isLoading)
            {
                PopulateBranches();
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
            BranchTypeChanged();
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

        private bool LocalBranchExists(string name)
        {
            return GetLocalBranches().Any(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private IGitRef GetLocalBranchRef(string name)
        {
            return GetLocalBranches().FirstOrDefault(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private IGitRef GetRemoteBranchRef(string name)
        {
            return GetRemoteBranches().FirstOrDefault(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
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
                _remoteName = GitRefName.GetRemoteName(branch, Module.GetRemotes(false));
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

                        var text = Module.GetCommitCountString(Module.GetCurrentCheckout(), branch);

                        await this.SwitchToMainThreadAsync();

                        lbChanges.Text = text;
                    });
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
                _remoteBranches = Module.GetRefs(true, true).Where(h => h.IsRemote && !h.IsTag);
            }

            return _remoteBranches;
        }

        private IReadOnlyList<string> GetContainsRevisionBranches()
        {
            var result = new HashSet<string>();
            if (_containRevisons.Length > 0)
            {
                var branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisons[0], LocalBranch.Checked,
                        !LocalBranch.Checked)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.UnionWith(branches);
            }

            for (int index = 1; index < _containRevisons.Length; index++)
            {
                var containRevison = _containRevisons[index];
                var branches =
                    Module.GetAllBranchesWhichContainGivenCommit(containRevison, LocalBranch.Checked,
                        !LocalBranch.Checked)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.IntersectWith(branches);
            }

            return result.ToList();
        }

        private void FormCheckoutBranch_Activated(object sender, EventArgs e)
        {
            Branches.Focus();
        }

        private void FormCheckoutBranch_Shown(object sender, EventArgs e)
        {
            Shown -= FormCheckoutBranch_Shown;
            RecalculateSizeConstraints();
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
    }
}
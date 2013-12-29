using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using ResourceManager.Translation;

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
            new TranslationString("Apply stashed items to working dir again?");

        private readonly TranslationString _dontShowAgain =
            new TranslationString("Don't show me this message again.");
        #endregion

        private readonly string[] _containRevisons;
        private readonly bool _isLoading;
        private readonly string _rbResetBranchDefaultText;
        private bool? _isDirtyDir;
        private string _remoteName = "";
        private string _newLocalBranchName = "";
        private string _localBranchName = "";

        private List<string> _localBranches;
        private List<string> _remoteBranches;

        private FormCheckoutBranch()
            : this(null)
        {
        }

        internal FormCheckoutBranch(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            _rbResetBranchDefaultText = rbResetBranch.Text;
        }

        public FormCheckoutBranch(GitUICommands aCommands, string branch, bool remote)
            : this(aCommands, branch, remote, null)
        {
        }

        public FormCheckoutBranch(GitUICommands aCommands, string branch, bool remote, string[] containRevisons)
            : this(aCommands)
        {
            _isLoading = true;

            try
            {
                _containRevisons = containRevisons;

                LocalBranch.Checked = !remote;
                Remotebranch.Checked = remote;

                Initialize();

                //Set current branch after initialize, because initialize will reset it
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
                        Initialize();
                    }
                }

                //The dirty check is very expensive on large repositories. Without this setting
                //the checkout branch dialog is too slow.
                if (AppSettings.CheckForUncommittedChangesInCheckoutBranch)
                    _isDirtyDir = Module.IsDirtyDir();
                else
                    _isDirtyDir = null;

                localChangesGB.Visible = IsThereUncommittedChanges();
                ChangesMode = AppSettings.CheckoutBranchAction;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private bool IsThereUncommittedChanges()
        {
            return _isDirtyDir ?? true;
        }

        public DialogResult DoDefaultActionOrShow(IWin32Window owner)
        {
            bool localBranchSelected = !Branches.Text.IsNullOrWhiteSpace() && !Remotebranch.Checked;
            if (!AppSettings.AlwaysShowCheckoutBranchDlg && localBranchSelected &&
                (!IsThereUncommittedChanges() || AppSettings.UseDefaultCheckoutBranchAction))
                return OkClick();
            else
                return ShowDialog(owner);
        }

        private void Initialize()
        {
            Branches.Items.Clear();

            if (_containRevisons == null)
            {
                if (LocalBranch.Checked)
                {
                    Branches.Items.AddRange(GetLocalBranches().ToArray());
                }
                else
                {
                    Branches.Items.AddRange(GetRemoteBranches().ToArray());
                }
            }
            else
            {
                Branches.Items.AddRange(GetContainsRevisionBranches().ToArray());
            }

            if (_containRevisons != null && Branches.Items.Count == 1)
                Branches.SelectedIndex = 0;
            else
                Branches.Text = null;
            remoteOptionsPanel.Visible = Remotebranch.Checked;

            rbCreateBranchWithCustomName.Checked = AppSettings.CreateLocalBranchForRemote;
        }

        private LocalChangesAction ChangesMode
        {
            get
            {
                if (rbReset.Checked)
                    return LocalChangesAction.Reset;
                else if (rbMerge.Checked)
                    return LocalChangesAction.Merge;
                else if (rbStash.Checked)
                    return LocalChangesAction.Stash;
                else
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

        private void OkClick(object sender, EventArgs e)
        {
            DialogResult = OkClick();
            if (DialogResult == DialogResult.OK)
                Close();
        }

        private DialogResult OkClick()
        {
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
            AppSettings.CheckoutBranchAction = changes;

            if ((Visible || AppSettings.UseDefaultCheckoutBranchAction) && IsThereUncommittedChanges())
                cmd.LocalChanges = changes;
            else
                cmd.LocalChanges = LocalChangesAction.DontChange;

            IWin32Window owner = Visible ? this : Owner;

            bool stash = false;
            if (changes == LocalChangesAction.Stash)
            {
                if (_isDirtyDir == null && Visible)
                    _isDirtyDir = Module.IsDirtyDir();
                stash = _isDirtyDir == true;
                if (stash)
                    UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInAutoStash);
            }

            if (UICommands.StartCommandLineProcessDialog(cmd, owner))
            {
                if (stash)
                {
                    bool? messageBoxResult = AppSettings.AutoPopStashAfterCheckoutBranch;
                    if (messageBoxResult == null)
                    {
                        DialogResult res = PSTaskDialog.cTaskDialog.MessageBox(
                            this,
                            _applyShashedItemsAgainCaption.Text,
                            "",
                            _applyShashedItemsAgain.Text,
                            "",
                            "",
                            _dontShowAgain.Text,
                            PSTaskDialog.eTaskDialogButtons.YesNo,
                            PSTaskDialog.eSysIcons.Question,
                            PSTaskDialog.eSysIcons.Question);
                        messageBoxResult = (res == DialogResult.Yes);
                        if (PSTaskDialog.cTaskDialog.VerificationChecked)
                            AppSettings.AutoPopStashAfterCheckoutBranch = messageBoxResult;
                    }
                    if (messageBoxResult ?? false)
                    {
                        UICommands.StashPop(this);
                    }
                }

                UICommands.UpdateSubmodules(this);

                return DialogResult.OK;
            }

            return DialogResult.None;
        }

        private void BranchTypeChanged()
        {
            if (!_isLoading)
                Initialize();
        }

        private void LocalBranchCheckedChanged(object sender, EventArgs e)
        {
            //We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
            //BranchTypeChanged();
        }

        private void RemoteBranchCheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
            Branches_SelectedIndexChanged(sender, e);
        }

        private void rbCreateBranchWithCustomName_CheckedChanged(object sender, EventArgs e)
        {
            txtCustomBranchName.Enabled = rbCreateBranchWithCustomName.Checked;
            if (rbCreateBranchWithCustomName.Checked)
                txtCustomBranchName.SelectAll();
        }

        private bool LocalBranchExists(string name)
        {
            return GetLocalBranches().Any(head => head.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lbChanges.Text = "";

            var _branch = Branches.Text;

            if (_branch.IsNullOrWhiteSpace() || !Remotebranch.Checked)
            {
                _remoteName = string.Empty;
                _localBranchName = string.Empty;
                _newLocalBranchName = string.Empty;
            }
            else
            {
                _remoteName = GitCommandHelpers.GetRemoteName(_branch, Module.GetRemotes(false));
                _localBranchName = _remoteName.Length > 0 ? _branch.Substring(_remoteName.Length + 1) : _branch;
                _newLocalBranchName = string.Concat(_remoteName, "_", _localBranchName);
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

            if (_branch.IsNullOrWhiteSpace())
                lbChanges.Text = "";
            else
            {
                Task.Factory.StartNew(() => this.Module.GetCommitCountString(this.Module.GetCurrentCheckout(), _branch))
                    .ContinueWith(t => lbChanges.Text = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private IList<string> GetLocalBranches()
        {
            if (_localBranches == null)
                _localBranches = Module.GetRefs(false).Select(b => b.Name).ToList();

            return _localBranches;
        }

        private IList<string> GetRemoteBranches()
        {
            if (_remoteBranches == null)
                _remoteBranches = Module.GetRefs(true, true).Where(h => h.IsRemote && !h.IsTag).Select(b => b.Name).ToList();

            return _remoteBranches;
        }

        private IList<string> GetContainsRevisionBranches()
        {
            var result = new HashSet<string>();
            if (_containRevisons.Length > 0)
            {
                var branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisons[0], LocalBranch.Checked,
                        !LocalBranch.Checked)
                        .Where(a => !GitModule.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.UnionWith(branches);
                
            }
            for (int index = 1; index < _containRevisons.Length; index++)
            {
                var containRevison = _containRevisons[index];
                var branches =
                    Module.GetAllBranchesWhichContainGivenCommit(containRevison, LocalBranch.Checked,
                        !LocalBranch.Checked)
                        .Where(a => !GitModule.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.IntersectWith(branches);
            }
            return result.ToList();
        }

        private void FormCheckoutBranch_Activated(object sender, EventArgs e)
        {
            Branches.Focus();
        }
    }
}
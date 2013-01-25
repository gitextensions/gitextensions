using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using ResourceManager.Translation;

namespace GitUI
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
        #endregion

        private string _containRevison;
        private bool isDirtyDir;
        private bool isLoading;
        private string _remoteName = "";
        private string _newLocalBranchName = "";
        private string _localBranchName = "";
        private readonly string rbResetBranchText;

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
            rbResetBranchText = rbResetBranch.Text;
        }

        public FormCheckoutBranch(GitUICommands aCommands, string branch, bool remote)
            : this(aCommands, branch, remote, null)
        {
        }

        public FormCheckoutBranch(GitUICommands aCommands, string branch, bool remote, string containRevison)
            : this(aCommands)
        {
            isLoading = true;

            try
            {
                _containRevison = containRevison;

                LocalBranch.Checked = !remote;
                Remotebranch.Checked = remote;

                Initialize();

                //Set current branch after initialize, because initialize will reset it
                if (!string.IsNullOrEmpty(branch))
                {
                    Branches.Items.Add(branch);
                    Branches.SelectedItem = branch;
                }

                if (containRevison != null)
                {
                    if (Branches.Items.Count == 0)
                    {
                        Remotebranch.Checked = true;
                        Initialize();
                    }
                    if (Branches.Items.Count == 1)
                        Branches.SelectedIndex = 0;
                }

                //The dirty check is very expensive on large repositories. Without this setting
                //the checkout branch dialog is too slow.
                if (Settings.CheckForUncommittedChangesInCheckoutBranch)
                    isDirtyDir = Module.IsDirtyDir();
                else
                    isDirtyDir = false;

                localChangesGB.Visible = ShowLocalChangesGB();
                ChangesMode = Settings.CheckoutBranchAction;
                defaultActionChx.Checked = Settings.UseDefaultCheckoutBranchAction;
            }
            finally
            {
                isLoading = false;
            }
        }

        private bool ShowLocalChangesGB()
        {
            return isDirtyDir || !Settings.CheckForUncommittedChangesInCheckoutBranch;
        }

        public DialogResult DoDefaultActionOrShow(IWin32Window owner)
        {
            if (Settings.AlwaysShowCheckoutBranchDlg ||
                Branches.Text.IsNullOrWhiteSpace() || Remotebranch.Checked
                || (isDirtyDir && !Settings.UseDefaultCheckoutBranchAction))
                return ShowDialog(owner);
            else
                return OkClick();
        }


        private void Initialize()
        {
            Branches.Items.Clear();

            if (_containRevison == null)
            {
                if (LocalBranch.Checked)
                {
                    Branches.Items.AddRange(getLocalBranches().ToArray());
                }
                else
                {
                    Branches.Items.AddRange(getRemoteBranches().ToArray());
                }
            }
            else
            {
                Branches.Items.AddRange(getContainsRevisionBranches().ToArray());
            }

            Branches.Text = null;
            remoteOptionsPanel.Visible = Remotebranch.Checked;
            rbCreateBranchWithCustomName.Checked = Settings.CreateLocalBranchForRemote;
        }

        private Settings.LocalChanges ChangesMode
        {
            get
            {
                if (rbReset.Checked)
                    return Settings.LocalChanges.Reset;
                else if (rbMerge.Checked)
                    return Settings.LocalChanges.Merge;
                else if (rbStash.Checked)
                    return Settings.LocalChanges.Stash;
                else
                    return Settings.LocalChanges.DontChange;
            }
            set
            {
                rbReset.Checked = value == Settings.LocalChanges.Reset;
                rbMerge.Checked = value == Settings.LocalChanges.Merge;
                rbStash.Checked = value == Settings.LocalChanges.Stash;
                rbDontChange.Checked = value == Settings.LocalChanges.DontChange;
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

            Settings.LocalChanges changes = ChangesMode;
            Settings.CheckoutBranchAction = changes;
            Settings.UseDefaultCheckoutBranchAction = defaultActionChx.Checked;

            if (ShowLocalChangesGB())
                cmd.SetLocalChangesFromSettings(changes);
            else
                cmd.SetLocalChangesFromSettings(Settings.LocalChanges.DontChange);

            IWin32Window _owner = Visible ? this : Owner;

            //Stash local changes, but only if the setting CheckForUncommittedChangesInCheckoutBranch is true
            if (Settings.CheckForUncommittedChangesInCheckoutBranch &&
                changes == Settings.LocalChanges.Stash && Module.IsDirtyDir())
            {
                UICommands.Stash(_owner);
            }

            {
                var successfullyCheckedOut = UICommands.StartCommandLineProcessDialog(cmd, _owner);

                if (successfullyCheckedOut)
                    return DialogResult.OK;
                else
                    return DialogResult.None;
            }        
        }

        private void BranchTypeChanged()
        {
            if (!isLoading)
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
            return getLocalBranches().Any(head => head.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            var _branch = Branches.Text;

            if (_branch.IsNullOrWhiteSpace() || !Remotebranch.Checked)
            {
                _remoteName = string.Empty;
                _localBranchName = string.Empty;
                _newLocalBranchName = string.Empty;
            }
            else
            {
                _remoteName = GitModule.GetRemoteName(_branch, Module.GetRemotes(false));
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

            rbResetBranch.Text = existsLocalBranch ? rbResetBranchText : _createBranch.Text;
            branchName.Text = "'" + _localBranchName + "'";
            txtCustomBranchName.Text = _newLocalBranchName;
        }

        private IList<string> getLocalBranches()
        {
            if (_localBranches == null)
                _localBranches = Module.GetHeads(false).Select(b => b.Name).ToList();

            return _localBranches;
        }

        private IList<string> getRemoteBranches()
        {
            if (_remoteBranches == null)
                _remoteBranches = Module.GetHeads(true, true).Where(h => h.IsRemote && !h.IsTag).Select(b => b.Name).ToList();

            return _remoteBranches;
        }

        private IList<string> getContainsRevisionBranches()
        {
            return CommitInformation
                        .GetAllBranchesWhichContainGivenCommit(Module, _containRevison, LocalBranch.Checked, !LocalBranch.Checked)
                        .Where(a => !a.Equals("(no branch)", StringComparison.OrdinalIgnoreCase) && 
                            !a.EndsWith("/HEAD")).ToList();
        }

        private void FormCheckoutBranch_Activated(object sender, EventArgs e)
        {
            Branches.Focus();
        }
    }
}
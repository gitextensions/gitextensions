using System;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using System.Collections.Generic;
using System.Windows.Forms;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {

        #region Translation
        private readonly TranslationString _applyShashedItemsAgain =
            new TranslationString("Apply stashed items to working dir again?");

        private readonly TranslationString _applyShashedItemsAgainCaption =
            new TranslationString("Auto stash");

        private readonly TranslationString _customBranchNameIsEmpty =
            new TranslationString("Custom branch name is empty.\nEnter valid branch name or select predefined value.");
        private readonly TranslationString _customBranchNameIsNotValid =
            new TranslationString("“{0}” is not valid branch name.\nEnter valid branch name or select predefined value.");
        #endregion

        private string _containRevison;
        private bool isDirtyDir;
        private bool isLoading = false;
        private string _remoteName = "";
        private string _newLocalBranchName = "";
        private string _localBranchName = "";
        private readonly string rbResetBranchText;
        private readonly string rbCreateBranchText;


        internal FormCheckoutBranch()
        {
            InitializeComponent();
            Translate();
            rbResetBranchText = rbResetBranch.Text;
            rbCreateBranchText = rbCreateBranch.Text;
        }

        public FormCheckoutBranch(string branch, bool remote)
            : this(branch, remote, null)
        {
        }

        public FormCheckoutBranch(string branch, bool remote, string containRevison)
            : this()
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
                    Branches.Text = branch;

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
                    isDirtyDir = GitModule.Current.IsDirtyDir();
                else
                    isDirtyDir = false;

                localChangesGB.Visible = isDirtyDir;
                ChangesMode = Settings.CheckoutBranchAction;
                defaultActionChx.Checked = Settings.UseDefaultCheckoutBranchAction;
            }
            finally
            {
                isLoading = false;
            }
        }

        public DialogResult DoDefaultActionOrShow(IWin32Window owner)
        {
            if (Branches.Text.IsNullOrWhiteSpace() || Remotebranch.Checked
                || (isDirtyDir && !Settings.UseDefaultCheckoutBranchAction))
                return ShowDialog(owner);
            else
                return OkClick();
        }


        private void Initialize()
        {
            Branches.DisplayMember = "Name";

            if (_containRevison == null)
            {
                if (LocalBranch.Checked)
                {
                    Branches.DataSource = GitModule.Current.GetHeads(false).Select(a => a.Name).ToList();
                }
                else
                {
                    var heads = GitModule.Current.GetHeads(true, true);

                    var remoteHeads = new List<GitHead>();

                    foreach (var head in heads)
                    {
                        if (head.IsRemote && !head.IsTag)
                            remoteHeads.Add(head);
                    }

                    Branches.DataSource = remoteHeads.Select(a => a.Name).ToList();
                }
            }
            else
            {
                var branches = CommitInformation
                    .GetAllBranchesWhichContainGivenCommit(_containRevison, LocalBranch.Checked, !LocalBranch.Checked)
                    .Where(a => !a.Equals("(no branch)", StringComparison.OrdinalIgnoreCase));
                Branches.DataSource = branches.ToList();
            }

            Branches.Text = null;
            remoteOptionsPanel.Visible = Remotebranch.Checked;
            rbCreateBranch.Checked = Settings.CreateLocalBranchForRemote;
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
                    return Settings.LocalChanges.Merge;
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
                    if (!GitModule.Current.CheckRefFormat(cmd.NewBranchName))
                    {
                        MessageBox.Show(string.Format(_customBranchNameIsNotValid.Text, cmd.NewBranchName), Text);
                        DialogResult = DialogResult.None;
                        return DialogResult.None;
                    }
                }
                else if (rbCreateBranch.Checked)
                {
                    cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                    cmd.NewBranchName = _newLocalBranchName;
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
            cmd.SetLocalChangesFromSettings(changes);

            IWin32Window _owner = Visible ? this : Owner;

            if (changes == Settings.LocalChanges.Stash && GitModule.Current.IsDirtyDir())
                GitUICommands.Instance.Stash(_owner);

            {
                var successfullyCheckedOut = GitUICommands.Instance.StartCommandLineProcessDialog(cmd, _owner);

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

        private static bool LocalBranchExists(string name)
        {
            return GitModule.Current.GetHeads(false).Any(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
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
                _remoteName = GitModule.GetRemoteName(_branch, GitModule.Current.GetRemotes(false));
                _localBranchName = _branch.Substring(_remoteName.Length + 1);
                _newLocalBranchName = string.Concat(_remoteName, "_", _localBranchName);
                int i = 2;
                while (LocalBranchExists(_newLocalBranchName))
                {
                    _newLocalBranchName = string.Concat(_remoteName, "_", _localBranchName, "_", i.ToString());
                    i++;
                }
            }
            bool existsLocalBranch = LocalBranchExists(_localBranchName);

            rbResetBranch.Text = String.Format(existsLocalBranch ? rbResetBranchText : rbCreateBranchText, _localBranchName);
            rbCreateBranch.Text = String.Format(rbCreateBranchText, _newLocalBranchName);
            txtCustomBranchName.Text = _localBranchName;
        }
        
    }
}
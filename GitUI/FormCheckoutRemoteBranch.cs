using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCheckoutRemoteBranch : GitExtensionsForm
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

        string _branch = "";
        string _remoteName = "";
        string _newLocalBranchName = "";
        string _localBranchName = "";

        // for translation only
        internal FormCheckoutRemoteBranch()
        {
            InitializeComponent();
            Translate();
        }

        public FormCheckoutRemoteBranch(string branch)
        {
            InitializeComponent();
            Translate();

            _branch = branch;
            Initialize();
        }

        public FormCheckoutRemoteBranch(string branch, Settings.LocalChanges changes)
            : this(branch)
        {
            rbReset.Checked = changes == Settings.LocalChanges.Reset;
            rbMerge.Checked = changes == Settings.LocalChanges.Merge;
            rbStash.Checked = changes == Settings.LocalChanges.Stash;
            rbDontChange.Checked = changes == Settings.LocalChanges.DontChange;
        }

        private void Initialize()
        {
            Text = String.Format(Text, _branch);
            _remoteName = GitModule.GetRemoteName(_branch, Settings.Module.GetRemotes(false));
            _localBranchName = _branch.Substring(_remoteName.Length + 1);
            _newLocalBranchName = string.Concat(_remoteName, "_", _localBranchName);
            int i = 2;
            while (LocalBranchExists(_newLocalBranchName))
            {
                _newLocalBranchName = string.Concat(_remoteName, "_", _localBranchName, "_", i.ToString());
                i++;
            }

            bool existsLocalBranch = LocalBranchExists(_localBranchName);

            rbCreateBranch.Checked = Settings.CreateLocalBranchForRemote;

            rbResetBranch.Text = String.Format(existsLocalBranch ? rbResetBranch.Text : rbCreateBranch.Text, _localBranchName);
            rbCreateBranch.Text = String.Format(rbCreateBranch.Text, _newLocalBranchName);
            txtCustomBranchName.Text = _localBranchName;
            ChangesMode = Settings.CheckoutBranchAction;
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
            var customBranchName = txtCustomBranchName.Text.Trim();
            if (rbCreateBranchWithCustomName.Checked)
            {
                if (customBranchName.IsNullOrWhiteSpace())
                {
                    MessageBox.Show(_customBranchNameIsEmpty.Text, Text);
                    DialogResult = DialogResult.None;
                    return;
                }
                if (!Settings.Module.CheckRefFormat(customBranchName))
                {
                    MessageBox.Show(string.Format(_customBranchNameIsNotValid.Text, customBranchName), Text);
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            try
            {
                Settings.LocalChanges changes = ChangesMode;
                Settings.CheckoutBranchAction = changes;
                Settings.CreateLocalBranchForRemote = rbCreateBranch.Checked;

                if (changes == Settings.LocalChanges.Stash)
                    GitUICommands.Instance.Stash(this);

                var command = "checkout";

                //Get a localbranch name
                if (rbCreateBranch.Checked)
                    command += string.Format(" -b {0}", _newLocalBranchName);
                else if (rbResetBranch.Checked)
                    command += string.Format(" -B {0}", _localBranchName);
                else if (rbCreateBranchWithCustomName.Checked)
                    command += string.Format(" -b {0}", customBranchName);

                if (changes == Settings.LocalChanges.Merge)
                    command += " --merge";
                else if (changes == Settings.LocalChanges.Reset)
                    command += " --force";

                command += " \"" + _branch + "\"";
                var successfullyCheckedOut = FormProcess.ShowDialog(this, command);
                if (successfullyCheckedOut)
                    Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static bool LocalBranchExists(string name)
        {
            return Settings.Module.GetHeads(false).Any(head => head.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private void lnkSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            localChangesGB.Show();
            lnkSettings.Hide();
            Height += (localChangesGB.Height - lnkSettings.Height) / 2;
        }

        private void rbCreateBranchWithCustomName_CheckedChanged(object sender, EventArgs e)
        {
            txtCustomBranchName.Enabled = rbCreateBranchWithCustomName.Checked;
            if (rbCreateBranchWithCustomName.Checked)
                txtCustomBranchName.SelectAll();
        }
    }
}
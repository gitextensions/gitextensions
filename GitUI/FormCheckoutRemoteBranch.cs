using System;
using System.Diagnostics;
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

        public FormCheckoutRemoteBranch(string branch, LocalChanges changes)
            : this(branch)
        {
            rbReset.Checked = changes == LocalChanges.Reset;
            rbMerge.Checked = changes == LocalChanges.Merge;
            rbDontChange.Checked = changes == LocalChanges.DontChange;
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
            rbReset.Checked = Settings.CheckoutBranchAction == (int)LocalChanges.Reset;
            rbMerge.Checked = Settings.CheckoutBranchAction == (int)LocalChanges.Merge;
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                LocalChanges changes;
                if (rbReset.Checked)
                    changes = LocalChanges.Reset;
                else if (rbMerge.Checked)
                    changes = LocalChanges.Merge;
                else
                    changes = LocalChanges.DontChange;
                Settings.CheckoutBranchAction = (int)changes;
                Settings.CreateLocalBranchForRemote = rbCreateBranch.Checked;

                var command = "checkout";

                //Get a localbranch name
                if (rbCreateBranch.Checked)
                    command += string.Format(" -b {0}", _newLocalBranchName);
                else if (rbResetBranch.Checked)
                    command += string.Format(" -B {0}", _localBranchName);

                if (changes == LocalChanges.Merge)
                    command += " --merge";
                else if (changes == LocalChanges.Reset)
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
            foreach (GitHead head in Settings.Module.GetHeads(false))
            {
                if (head.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void lnkSettings_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            localChangesGB.Show();
            lnkSettings.Hide();
            Height += (localChangesGB.Height - lnkSettings.Height) / 2;
        }
    }
}
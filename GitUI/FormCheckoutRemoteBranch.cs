using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Collections.Generic;

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

        public FormCheckoutRemoteBranch(string branch, bool force)
            : this(branch)
        {
            if (force)
                rbReset.Checked = true;
            else
                rbMerge.Checked = true;
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
            rbMerge.Checked = Settings.MergeAtCheckout;
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                Settings.MergeAtCheckout = rbMerge.Checked;
                Settings.CreateLocalBranchForRemote = rbCreateBranch.Checked;
                var command = "checkout";

                //Get a localbranch name
                if (rbCreateBranch.Checked)
                    command += string.Format(" -b {0}", _newLocalBranchName);
                else if (rbResetBranch.Checked)
                    command += string.Format(" -B {0}", _localBranchName);

                if (rbMerge.Checked)
                    command += " -m";

                if (rbReset.Checked)
                    command += " --force";

                command += " \"" + _branch + "\"";
                var form = new FormProcess(command);
                form.ShowDialog(this);
                if (!form.ErrorOccurred())
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
    }
}
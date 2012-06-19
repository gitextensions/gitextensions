using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace Gerrit
{
    public partial class FormGerritPublish : FormGerritBase
    {
        private readonly IGitUICommands _uiCommand;
        private string _currentBranchRemote;

        #region Translation
        private readonly TranslationString _downloadGerritChangeCaption = new TranslationString("Download Gerrit Change");

        private readonly TranslationString _publishCaption = new TranslationString("Publish change");

        private readonly TranslationString _selectRemote = new TranslationString("Please select a remote repository");
        private readonly TranslationString _selectBranch = new TranslationString("Please enter a branch");
        #endregion

        public FormGerritPublish(IGitUICommands uiCommand)
        {
            _uiCommand = uiCommand;

            InitializeComponent();
            Translate();
        }

        private void PublishClick(object sender, EventArgs e)
        {
            if (PublishChange(this))
                Close();
        }

        private bool PublishChange(IWin32Window owner)
        {
            string branch = _NO_TRANSLATE_Branch.Text.Trim();

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text))
            {
                MessageBox.Show(owner, _selectRemote.Text);
                return false;
            }
            if (string.IsNullOrEmpty(branch))
            {
                MessageBox.Show(owner, _selectBranch.Text);
                return false;
            }

            StartAgent(owner, _NO_TRANSLATE_Remotes.Text);

            string targetRef = PublishDraft.Checked ? "drafts" : "publish";

            var pushCommand = _uiCommand.CreateRemoteCommand();

            string targetBranch = "refs/" + targetRef + "/" + branch;
            string topic = _NO_TRANSLATE_Topic.Text.Trim();

            if (!string.IsNullOrEmpty(topic))
                targetBranch += "/" + topic;

            pushCommand.CommandText = GitCommandHelpers.PushCmd(
                _NO_TRANSLATE_Remotes.Text,
                targetBranch,
                false
            );
            pushCommand.Remote = _NO_TRANSLATE_Remotes.Text;
            pushCommand.Title = _publishCaption.Text;

            pushCommand.Execute();

            if (!pushCommand.ErrorOccurred)
            {
                bool hadNewChanges = false;
                string change = null;

                foreach (string line in pushCommand.CommandText.Split('\n'))
                {
                    if (hadNewChanges)
                    {
                        change = line;
                        const string remotePrefix = "remote:";

                        if (change.StartsWith(remotePrefix))
                            change = change.Substring(remotePrefix.Length);

                        int escapePos = change.LastIndexOf((char)27);
                        if (escapePos != -1)
                            change = change.Substring(0, escapePos);

                        change = change.Trim();

                        int spacePos = change.IndexOf(' ');
                        if (spacePos != -1)
                            change = change.Substring(0, spacePos);

                        break;
                    }
                    else if (line.Contains("New Changes"))
                    {
                        hadNewChanges = true;
                    }
                }

                if (change != null)
                    FormGerritChangeSubmitted.ShowSubmitted(owner, change);
            }

            return true;
        }

        private string GetTopic(string targetBranch)
        {
            string branchName = GetBranchName(targetBranch);

            string[] branchParts = branchName.Split('/');

            if (branchParts.Length >= 3 && branchParts[0] == "review")
                return String.Join("/", branchParts.Skip(2).ToArray());

            return branchName;
        }

        private string GetBranchName(string targetBranch)
        {
            string branch = GitCommands.Settings.Module.GetSelectedBranch();

            if (branch.StartsWith("(no"))
                return targetBranch;

            return branch;
        }

        private void FormGerritPublishLoad(object sender, EventArgs e)
        {
            RestorePosition("public-gerrit-change");

            _NO_TRANSLATE_Remotes.DataSource = GitCommands.Settings.Module.GetRemotes();

            _currentBranchRemote = Settings.DefaultRemote;

            IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
            int i = remotes.IndexOf(_currentBranchRemote);
            _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 0;

            _NO_TRANSLATE_Branch.Text = Settings.DefaultBranch;

            if (!string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
                _NO_TRANSLATE_Topic.Text = GetTopic(_NO_TRANSLATE_Branch.Text);

            if (_NO_TRANSLATE_Topic.Text == _NO_TRANSLATE_Branch.Text)
                _NO_TRANSLATE_Topic.Text = null;

            _NO_TRANSLATE_Branch.Select();

            Text = string.Concat(_downloadGerritChangeCaption.Text, " (", GitCommands.Settings.WorkingDir, ")");
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            _uiCommand.StartRemotesDialog();
            _NO_TRANSLATE_Remotes.DataSource = GitCommands.Settings.Module.GetRemotes();
        }

        private void FormGerritPublish_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("public-gerrit-change");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace Gerrit
{
    public partial class FormGerritPublish : FormGerritBase
    {
        private string _currentBranchRemote;

        #region Translation
        private readonly TranslationString _publishGerritChangeCaption = new TranslationString("Publish Gerrit Change");

        private readonly TranslationString _publishCaption = new TranslationString("Publish change");

        private readonly TranslationString _selectRemote = new TranslationString("Please select a remote repository");
        private readonly TranslationString _selectBranch = new TranslationString("Please enter a branch");
        #endregion

        public FormGerritPublish(IGitUICommands uiCommand)
            : base(uiCommand)
        {
            InitializeComponent();
            InitializeComplete();

            Publish.Image = Images.Push;
        }

        private void PublishClick(object sender, EventArgs e)
        {
            if (PublishChange(this))
            {
                Close();
            }
        }

        private static ArgumentString PushCmd(string remote, string toBranch)
        {
            return new GitArgumentBuilder("push")
            {
                { GitVersion.Current.PushCanAskForProgress, "--progress" },
                remote.ToPosixPath().Trim().Quote(),
                $"HEAD:{GitRefName.GetFullBranchName(toBranch)?.Replace(" ", "")}"
            };
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

            GerritUtil.StartAgent(owner, Module, _NO_TRANSLATE_Remotes.Text);

            string targetRef = PublishDraft.Checked ? "drafts" : "publish";

            var pushCommand = UICommands.CreateRemoteCommand();

            string targetBranch = "refs/" + targetRef + "/" + branch;
            string topic = _NO_TRANSLATE_Topic.Text.Trim();

            if (!string.IsNullOrEmpty(topic))
            {
                targetBranch += "/" + topic;
            }

            string reviewers = _NO_TRANSLATE_Reviewers.Text.Trim();
            if (!string.IsNullOrEmpty(reviewers))
            {
                string formattedReviewers = string.Join(",", reviewers.Split(' ')
                                                                      .Where(r => !string.IsNullOrEmpty(r))
                                                                      .Select(r => "r=" + r));
                if (!formattedReviewers.IsNullOrEmpty())
                {
                    targetBranch += "%" + formattedReviewers;
                }
            }

            pushCommand.CommandText = PushCmd(
                _NO_TRANSLATE_Remotes.Text,
                targetBranch);
            pushCommand.Remote = _NO_TRANSLATE_Remotes.Text;
            pushCommand.Title = _publishCaption.Text;

            pushCommand.Execute();

            if (!pushCommand.ErrorOccurred)
            {
                bool hadNewChanges = false;
                string change = null;

                foreach (string line in pushCommand.CommandOutput.Split('\n'))
                {
                    if (hadNewChanges)
                    {
                        const char esc = (char)27;
                        change = line
                            .RemovePrefix("remote:")
                            .SubstringUntilLast(esc)
                            .Trim()
                            .SubstringUntil(' ');
                        break;
                    }
                    else if (line.Contains("New Changes"))
                    {
                        hadNewChanges = true;
                    }
                }

                if (change != null)
                {
                    FormGerritChangeSubmitted.ShowSubmitted(owner, change);
                }
            }

            return true;
        }

        [CanBeNull]
        private string GetTopic(string targetBranch)
        {
            string branchName = GetBranchName(targetBranch);

            string[] branchParts = branchName.Split('/');

            if (branchParts.Length >= 3 && branchParts[0] == "review")
            {
                branchName = string.Join("/", branchParts.Skip(2));

                // Don't use the Gerrit change number as a topic branch.

                if (int.TryParse(branchName, out _))
                {
                    branchName = null;
                }
            }

            return branchName;
        }

        private string GetBranchName(string targetBranch)
        {
            string branch = Module.GetSelectedBranch();

            if (branch.StartsWith("(no"))
            {
                return targetBranch;
            }

            return branch;
        }

        private void FormGerritPublishLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.DataSource = Module.GetRemoteNames();

            _currentBranchRemote = Settings.DefaultRemote;

            var remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
            int i = remotes.IndexOf(_currentBranchRemote);
            _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 0;

            _NO_TRANSLATE_Branch.Text = Settings.DefaultBranch;

            if (!string.IsNullOrEmpty(_NO_TRANSLATE_Branch.Text))
            {
                _NO_TRANSLATE_Topic.Text = GetTopic(_NO_TRANSLATE_Branch.Text);
            }

            if (_NO_TRANSLATE_Topic.Text == _NO_TRANSLATE_Branch.Text)
            {
                _NO_TRANSLATE_Topic.Text = null;
            }

            _NO_TRANSLATE_Branch.Select();

            Text = string.Concat(_publishGerritChangeCaption.Text, " (", Module.WorkingDir, ")");
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            UICommands.StartRemotesDialog();
            _NO_TRANSLATE_Remotes.DataSource = Module.GetRemoteNames();
        }
    }
}
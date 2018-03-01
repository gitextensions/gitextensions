using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using Newtonsoft.Json.Linq;
using ResourceManager;

namespace Gerrit
{
    public partial class FormGerritDownload : FormGerritBase
    {
        private string _currentBranchRemote;

        #region Translation
        private readonly TranslationString _downloadGerritChangeCaption = new TranslationString("Download Gerrit Change");

        private readonly TranslationString _downloadCaption = new TranslationString("Download change {0}");

        private readonly TranslationString _selectRemote = new TranslationString("Please select a remote repository");
        private readonly TranslationString _selectChange = new TranslationString("Please enter a change");
        private readonly TranslationString _cannotGetChangeDetails = new TranslationString("Could not retrieve the change details");
        #endregion

        public FormGerritDownload(IGitUICommands uiCommand)
            : base(uiCommand)
        {
            InitializeComponent();
            Translate();
        }

        public void PushAndShowDialogWhenFailed(IWin32Window owner)
        {
            if (!DownloadChange(owner))
                ShowDialog(owner);
        }

        public void PushAndShowDialogWhenFailed()
        {
            PushAndShowDialogWhenFailed(null);
        }

        private void DownloadClick(object sender, EventArgs e)
        {
            if (DownloadChange(this))
                Close();
        }

        private bool DownloadChange(IWin32Window owner)
        {
            string change = _NO_TRANSLATE_Change.Text.Trim();

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Remotes.Text))
            {
                MessageBox.Show(owner, _selectRemote.Text);
                return false;
            }
            if (string.IsNullOrEmpty(change))
            {
                MessageBox.Show(owner, _selectChange.Text);
                return false;
            }

            GerritUtil.StartAgent(owner, Module, _NO_TRANSLATE_Remotes.Text);

            var reviewInfo = LoadReviewInfo();

            if (reviewInfo == null || reviewInfo["id"] == null)
            {
                MessageBox.Show(owner, _cannotGetChangeDetails.Text);
                return false;
            }

            // The user can enter both the Change-Id or the number. Here we
            // force the number to get prettier branches.

            change = (string)reviewInfo["number"];

            string topic = _NO_TRANSLATE_TopicBranch.Text.Trim();

            if (string.IsNullOrEmpty(topic))
            {
                var topicNode = (JValue)reviewInfo["topic"];

                topic = topicNode == null ? change : (string)topicNode.Value;
            }

            string authorValue = (string)((JValue)reviewInfo["owner"]["name"]).Value;
            string author = Regex.Replace(authorValue.ToLowerInvariant(), "\\W+", "_");
            string branchName = "review/" + author + "/" + topic;
            string refspec = (string)((JValue)reviewInfo["currentPatchSet"]["ref"]).Value;

            var fetchCommand = UICommands.CreateRemoteCommand();

            fetchCommand.CommandText = FetchCommand(_NO_TRANSLATE_Remotes.Text, refspec);

            if (!RunCommand(fetchCommand, change))
                return false;

            var checkoutCommand = UICommands.CreateRemoteCommand();

            checkoutCommand.CommandText = GitCommandHelpers.BranchCmd(branchName, "FETCH_HEAD", true);
            checkoutCommand.Completed += (s, e) =>
            {
                if (e.IsError)
                {
                    if (e.Command.CommandText.Contains("already exists"))
                    {
                        // Recycle the current review branch.

                        var recycleCommand = UICommands.CreateRemoteCommand();

                        recycleCommand.CommandText = "checkout " + branchName;

                        if (!RunCommand(recycleCommand, change))
                            return;

                        var resetCommand = UICommands.CreateRemoteCommand();

                        resetCommand.CommandText = GitCommandHelpers.ResetHardCmd("FETCH_HEAD");

                        if (!RunCommand(resetCommand, change))
                            return;

                        e.IsError = false;
                    }
                }
            };

            return RunCommand(checkoutCommand, change);
        }

        private bool RunCommand(IGitRemoteCommand command, string change)
        {
            command.OwnerForm = this;
            command.Title = string.Format(_downloadCaption.Text, change);
            command.Remote = _NO_TRANSLATE_Remotes.Text;

            command.Execute();

            return !command.ErrorOccurred;
        }

        private string FetchCommand(string remote, string remoteBranch)
        {
            var progressOption = "";
            if (GitCommandHelpers.VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            remote = FixPath(remote);

            // Remove spaces...
            remoteBranch = remoteBranch?.Replace(" ", "");

            return "fetch " + progressOption + "\"" + remote.Trim() + "\" " + remoteBranch;
        }

        private static string FixPath(string path)
        {
            path = path.Trim();
            return path.ToPosixPath();
        }

        private JObject LoadReviewInfo()
        {
            var fetchUrl = GerritUtil.GetFetchUrl(Module, _currentBranchRemote);

            string projectName = fetchUrl.AbsolutePath.TrimStart('/');

            if (projectName.EndsWith(".git"))
                projectName = projectName.Substring(0, projectName.Length - 4);

            string change = GerritUtil.RunGerritCommand(
                this,
                Module,
                String.Format(
                    "gerrit query --format=JSON project:{0} --current-patch-set change:{1}",
                    projectName,
                    _NO_TRANSLATE_Change.Text),
                fetchUrl,
                _currentBranchRemote,
                null);

            foreach (string line in change.Split('\n'))
            {
                try
                {
                    return JObject.Parse(line);
                }
                catch
                {
                    // Ignore exceptions.
                }
            }

            return null;
        }

        private void FormGerritDownloadLoad(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Remotes.DataSource = Module.GetRemotes(true);

            _currentBranchRemote = Settings.DefaultRemote;

            IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
            int i = remotes.IndexOf(_currentBranchRemote);
            _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 0;

            _NO_TRANSLATE_Change.Select();

            Text = string.Concat(_downloadGerritChangeCaption.Text, " (", Module.WorkingDir, ")");
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            UICommands.StartRemotesDialog();
            _NO_TRANSLATE_Remotes.DataSource = Module.GetRemotes(true);
        }
    }
}
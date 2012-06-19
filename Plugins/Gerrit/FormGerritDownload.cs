using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using Newtonsoft.Json.Linq;
using ResourceManager.Translation;

namespace Gerrit
{
    public partial class FormGerritDownload : FormGerritBase
    {
        private readonly IGitUICommands _uiCommand;
        private string _currentBranchRemote;

        #region Translation
        private readonly TranslationString _downloadGerritChangeCaption = new TranslationString("Download Gerrit Change");

        private readonly TranslationString _downloadCaption = new TranslationString("Download change {0}");

        private readonly TranslationString _selectRemote = new TranslationString("Please select a remote repository");
        private readonly TranslationString _selectChange = new TranslationString("Please enter a change");
        private readonly TranslationString _cannotGetChangeDetails = new TranslationString("Could not retrieve the change details");
        #endregion

        public FormGerritDownload(IGitUICommands uiCommand)
        {
            _uiCommand = uiCommand;

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

            StartAgent(owner, _NO_TRANSLATE_Remotes.Text);

            var reviewInfo = LoadReviewInfo();

            if (reviewInfo == null || reviewInfo["id"] == null)
            {
                MessageBox.Show(owner, _cannotGetChangeDetails.Text);
                return false;
            }

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

            var fetchCommand = _uiCommand.CreateRemoteCommand();

            fetchCommand.CommandText = FetchCommand(_NO_TRANSLATE_Remotes.Text, refspec);

            if (!RunCommand(fetchCommand, change))
                return false;

            var checkoutCommand = _uiCommand.CreateRemoteCommand();

            checkoutCommand.CommandText = GitCommandHelpers.BranchCmd(branchName, "FETCH_HEAD", true);
            checkoutCommand.Completed += (s, e) =>
            {
                if (e.IsError)
                {
                    if (e.Command.CommandText.Contains("already exists"))
                    {
                        // Recycle the current review branch.

                        var recycleCommand = _uiCommand.CreateRemoteCommand();

                        recycleCommand.CommandText = "checkout " + branchName;

                        if (!RunCommand(recycleCommand, change))
                            return;

                        var resetCommand = _uiCommand.CreateRemoteCommand();

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

            //Remove spaces... 
            if (remoteBranch != null)
                remoteBranch = remoteBranch.Replace(" ", "");

            return "fetch " + progressOption + "\"" + remote.Trim() + "\" " + remoteBranch;
        }

        private static string FixPath(string path)
        {
            path = path.Trim();
            return path.Replace('\\', '/');
        }

        private JObject LoadReviewInfo()
        {
            string remotes = GitCommands.Settings.Module.RunGitCmd("remote show -n \"" + _currentBranchRemote + "\"");

            string fetchUrlLine = remotes.Split('\n').Select(p => p.Trim()).First(p => p.StartsWith("Push"));
            var fetchUrl = new Uri(fetchUrlLine.Split(new[] { ':' }, 2)[1].Trim());

            string projectName = fetchUrl.AbsolutePath.TrimStart('/');

            if (projectName.EndsWith(".git"))
                projectName = projectName.Substring(0, projectName.Length - 4);

            string hostname = fetchUrl.Host;
            string username = fetchUrl.UserInfo;
            int port = fetchUrl.Port;

            if (port == -1 && fetchUrl.Scheme == "ssh")
                port = 22;

            var sb = new StringBuilder();

            sb.Append("\"");

            if (!string.IsNullOrEmpty(username))
            {
                sb.Append(username);
                sb.Append('@');
            }

            sb.Append(hostname);
            sb.Append("\" -P ");
            sb.Append(port);

            sb.Append(" \"gerrit query --format=JSON project:");
            sb.Append(projectName);
            sb.Append(" --current-patch-set change:");
            sb.Append(_NO_TRANSLATE_Change.Text);
            sb.Append('"');

            string change = GitCommands.Settings.Module.RunCmd(
                GitCommands.Settings.Plink,
                sb.ToString()
            );

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
            RestorePosition("download-gerrit-change");

            _NO_TRANSLATE_Remotes.DataSource = GitCommands.Settings.Module.GetRemotes();

            _currentBranchRemote = Settings.DefaultRemote;

            IList<string> remotes = (IList<string>)_NO_TRANSLATE_Remotes.DataSource;
            int i = remotes.IndexOf(_currentBranchRemote);
            _NO_TRANSLATE_Remotes.SelectedIndex = i >= 0 ? i : 0;

            _NO_TRANSLATE_Change.Select();

            Text = string.Concat(_downloadGerritChangeCaption.Text, " (", GitCommands.Settings.WorkingDir, ")");
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            _uiCommand.StartRemotesDialog();
            _NO_TRANSLATE_Remotes.DataSource = GitCommands.Settings.Module.GetRemotes();
        }

        private void FormGerritDownload_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("download-gerrit-change");
        }
    }
}
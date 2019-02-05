using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormClone : GitModuleForm
    {
        private readonly TranslationString _infoNewRepositoryLocation = new TranslationString("The repository will be cloned to a new directory located here:" + Environment.NewLine + "{0}");
        private readonly TranslationString _infoDirectoryExists = new TranslationString("(Directory already exists)");
        private readonly TranslationString _infoDirectoryNew = new TranslationString("(New directory)");
        private readonly TranslationString _questionOpenRepo = new TranslationString("The repository has been cloned successfully." + Environment.NewLine + "Do you want to open the new repository \"{0}\" now?");
        private readonly TranslationString _questionOpenRepoCaption = new TranslationString("Open");
        private readonly TranslationString _branchDefaultRemoteHead = new TranslationString("(default: remote HEAD)" /* Has a colon, so won't alias with any valid branch name */);
        private readonly TranslationString _branchNone = new TranslationString("(none: don't checkout after clone)" /* Has a colon, so won't alias with any valid branch name */);
        private readonly TranslationString _errorDestinationNotSupplied = new TranslationString("You need to specify destination folder.");
        private readonly TranslationString _errorDestinationNotRooted = new TranslationString("Destination folder must be an absolute path.");
        private readonly TranslationString _errorCloneFailed = new TranslationString("Clone Failed");

        private readonly bool _openedFromProtocolHandler;
        private readonly string _url;
        private readonly EventHandler<GitModuleEventArgs> _gitModuleChanged;
        private readonly IReadOnlyList<string> _defaultBranchItems;
        private string _puttySshKey;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormClone()
        {
            InitializeComponent();
        }

        public FormClone(GitUICommands commands, string url, bool openedFromProtocolHandler, EventHandler<GitModuleEventArgs> gitModuleChanged)
            : base(commands)
        {
            _gitModuleChanged = gitModuleChanged;
            InitializeComponent();
            InitializeComplete();
            _openedFromProtocolHandler = openedFromProtocolHandler;
            _url = url;
            _defaultBranchItems = new[] { _branchDefaultRemoteHead.Text, _branchNone.Text };
            _NO_TRANSLATE_Branches.DataSource = _defaultBranchItems;

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                _NO_TRANSLATE_To.DataSource = repositoryHistory;
                _NO_TRANSLATE_To.DisplayMember = nameof(Repository.Path);
            });
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            // scale up for hi DPI
            MaximumSize = DpiUtil.Scale(new Size(950, 375));
            MinimumSize = DpiUtil.Scale(new Size(450, 375));

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                _NO_TRANSLATE_From.DataSource = repositoryHistory;
                _NO_TRANSLATE_From.DisplayMember = nameof(Repository.Path);
            });

            _NO_TRANSLATE_To.Text = AppSettings.DefaultCloneDestinationPath;

            if (CanBeGitURL(_url) || GitModule.IsValidGitWorkingDir(_url))
            {
                _NO_TRANSLATE_From.Text = _url;
            }
            else
            {
                if (!string.IsNullOrEmpty(_url) && Directory.Exists(_url))
                {
                    _NO_TRANSLATE_To.Text = _url;
                }

                // Try to be more helpful to the user.
                // Use the clipboard text as a potential source URL.
                try
                {
                    if (Clipboard.ContainsText(TextDataFormat.Text))
                    {
                        string text = Clipboard.GetText(TextDataFormat.Text) ?? string.Empty;

                        // See if it's a valid URL.
                        if (CanBeGitURL(text))
                        {
                            _NO_TRANSLATE_From.Text = text;
                        }
                    }
                }
                catch
                {
                    // We tried.
                }

                // if the From field is empty, then fill it with the current repository remote URL in hope
                // that the cloned repository is hosted on the same server
                if (_NO_TRANSLATE_From.Text.IsNullOrWhiteSpace())
                {
                    var currentBranchRemote = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, Module.GetSelectedBranch()));
                    if (currentBranchRemote.IsNullOrEmpty())
                    {
                        var remotes = Module.GetRemoteNames();

                        if (remotes.Any(s => s.Equals("origin", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            currentBranchRemote = "origin";
                        }
                        else
                        {
                            currentBranchRemote = remotes.FirstOrDefault();
                        }
                    }

                    string pushUrl = Module.GetSetting(string.Format(SettingKeyString.RemotePushUrl, currentBranchRemote));
                    if (pushUrl.IsNullOrEmpty())
                    {
                        pushUrl = Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentBranchRemote));
                    }

                    _NO_TRANSLATE_From.Text = pushUrl;

                    try
                    {
                        // If the from directory is filled with the pushUrl from current working directory, set the destination directory to the parent
                        if (pushUrl.IsNotNullOrWhitespace() && _NO_TRANSLATE_To.Text.IsNullOrWhiteSpace() && Module.WorkingDir.IsNotNullOrWhitespace())
                        {
                            _NO_TRANSLATE_To.Text = Path.GetDirectoryName(Module.WorkingDir.TrimEnd(Path.DirectorySeparatorChar));
                        }
                    }
                    catch
                    {
                        // Exceptions on setting the destination directory can be ignored
                    }
                }
            }

            // if there is no destination directory, then use the parent of the current working directory
            // this would clone the new repo at the same level as the current one by default
            if (_NO_TRANSLATE_To.Text.IsNullOrWhiteSpace() && Module.WorkingDir.IsNotNullOrWhitespace())
            {
                if (Module.IsValidGitWorkingDir())
                {
                    if (Path.GetPathRoot(Module.WorkingDir) != Module.WorkingDir)
                    {
                        _NO_TRANSLATE_To.Text = Path.GetDirectoryName(Module.WorkingDir.TrimEnd(Path.DirectorySeparatorChar));
                    }
                }
                else
                {
                    _NO_TRANSLATE_To.Text = Module.WorkingDir;
                }
            }

            FromTextUpdate(null, null);

            cbLfs.Visible = !GitVersion.Current.DepreciatedLfsClone;
            cbLfs.Enabled = Module.HasLfsSupport();
            if (!cbLfs.Enabled || !cbLfs.Visible)
            {
                cbLfs.Checked = false;
            }
        }

        private static bool CanBeGitURL(string url)
        {
            if (url == null)
            {
                return false;
            }

            string urlLowered = url.ToLowerInvariant();

            return urlLowered.StartsWith("http") ||
                urlLowered.StartsWith("git") ||
                urlLowered.StartsWith("ssh");
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.Default;
                _branchListLoader.Cancel();

                // validate if destination path is supplied
                var destination = _NO_TRANSLATE_To.Text;
                if (string.IsNullOrWhiteSpace(destination))
                {
                    MessageBox.Show(this, _errorDestinationNotSupplied.Text, _errorCloneFailed.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _NO_TRANSLATE_To.Focus();
                    return;
                }

                if (!Path.IsPathRooted(destination))
                {
                    MessageBox.Show(this, _errorDestinationNotRooted.Text, _errorCloneFailed.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _NO_TRANSLATE_To.Focus();
                    return;
                }

                var dirTo = Path.Combine(destination, _NO_TRANSLATE_NewDirectory.Text);

                // this will fail if the path is anyhow invalid
                dirTo = new Uri(dirTo).LocalPath;

                if (!Directory.Exists(dirTo))
                {
                    Directory.CreateDirectory(dirTo);
                }

                // Shallow clone params
                int? depth = null;
                bool? isSingleBranch = null;
                if (!cbDownloadFullHistory.Checked)
                {
                    depth = 1;

                    // Single branch considerations:
                    // If neither depth nor single-branch family params are specified, then it's like no-single-branch by default.
                    // If depth is specified, then single-branch is assumed.
                    // But with single-branch it's really nontrivial to switch to another branch in the GUI, and it's very hard in cmdline (obvious choices to fetch another branch lead to local repo corruption).
                    // So let's reset it to no-single-branch to (a) have the same branches behavior as with full clone, and (b) make it easier for users when switching branches.
                    isSingleBranch = false;
                }

                // Branch name param
                string branch = _NO_TRANSLATE_Branches.Text;
                if (branch == _branchDefaultRemoteHead.Text)
                {
                    branch = "";
                }
                else if (branch == _branchNone.Text)
                {
                    branch = null;
                }

                var cloneCmd = GitCommandHelpers.CloneCmd(_NO_TRANSLATE_From.Text, dirTo,
                            CentralRepository.Checked, cbIntializeAllSubmodules.Checked, branch, depth, isSingleBranch, cbLfs.Checked);
                using (var fromProcess = new FormRemoteProcess(Module, AppSettings.GitCommand, cloneCmd))
                {
                    fromProcess.SetUrlTryingToConnect(_NO_TRANSLATE_From.Text);
                    fromProcess.ShowDialog(this);

                    if (fromProcess.ErrorOccurred() || Module.InTheMiddleOfPatch())
                    {
                        return;
                    }
                }

                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(dirTo));
                if (!string.IsNullOrEmpty(_puttySshKey))
                {
                    var clonedGitModule = new GitModule(dirTo);
                    clonedGitModule.SetSetting(string.Format(SettingKeyString.RemotePuttySshKey, "origin"), _puttySshKey);
                    clonedGitModule.LocalConfigFile.Save();
                }

                if (_openedFromProtocolHandler && AskIfNewRepositoryShouldBeOpened(dirTo))
                {
                    Hide();
                    var uiCommands = new GitUICommands(dirTo);
                    uiCommands.StartBrowseDialog();
                }
                else if (ShowInTaskbar == false && _gitModuleChanged != null &&
                    AskIfNewRepositoryShouldBeOpened(dirTo))
                {
                    _gitModuleChanged(this, new GitModuleEventArgs(new GitModule(dirTo)));
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Exception: " + ex.Message, _errorCloneFailed.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AskIfNewRepositoryShouldBeOpened(string dirTo)
        {
            return MessageBox.Show(this, string.Format(_questionOpenRepo.Text, dirTo), _questionOpenRepoCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void FromBrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this, _NO_TRANSLATE_From.Text);

            if (userSelectedPath != null)
            {
                _NO_TRANSLATE_From.Text = userSelectedPath;
            }

            FromTextUpdate(sender, e);
        }

        private void ToBrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this, _NO_TRANSLATE_To.Text);

            if (userSelectedPath != null)
            {
                _NO_TRANSLATE_To.Text = userSelectedPath;
            }

            ToTextUpdate(sender, e);
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            _puttySshKey = BrowseForPrivateKey.BrowseAndLoad(this);
        }

        private void FormCloneLoad(object sender, EventArgs e)
        {
            if (!GitCommandHelpers.Plink())
            {
                LoadSSHKey.Visible = false;
            }
        }

        private void FromSelectedIndexChanged(object sender, EventArgs e)
        {
            FromTextUpdate(sender, e);
        }

        private void FromTextUpdate(object sender, EventArgs e)
        {
            string path = PathUtil.GetRepositoryName(_NO_TRANSLATE_From.Text);

            if (path != "")
            {
                _NO_TRANSLATE_NewDirectory.Text = path;
            }

            _NO_TRANSLATE_Branches.DataSource = _defaultBranchItems;
            _NO_TRANSLATE_Branches.Select(0, 0);   // Kill full selection on the default branch text

            ToTextUpdate(sender, e);
        }

        private void ToTextUpdate(object sender, EventArgs e)
        {
            string destinationPath = string.Empty;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_To.Text))
            {
                destinationPath += "[" + label2.Text + "]";
            }
            else
            {
                destinationPath += _NO_TRANSLATE_To.Text.TrimEnd('\\', '/');
            }

            destinationPath += "\\";

            if (string.IsNullOrEmpty(_NO_TRANSLATE_NewDirectory.Text))
            {
                destinationPath += "[" + label3.Text + "]";
            }
            else
            {
                destinationPath += _NO_TRANSLATE_NewDirectory.Text;
            }

            Info.Text = string.Format(_infoNewRepositoryLocation.Text, destinationPath);

            if (destinationPath.Contains("[") || destinationPath.Contains("]"))
            {
                Info.ForeColor = Color.Red;
                return;
            }

            if (Directory.Exists(destinationPath))
            {
                if (Directory.GetDirectories(destinationPath).Length > 0 || Directory.GetFiles(destinationPath).Length > 0)
                {
                    Info.Text += " " + _infoDirectoryExists.Text;
                    Info.ForeColor = Color.Red;
                }
                else
                {
                    Info.ForeColor = Color.Black;
                }
            }
            else
            {
                Info.Text += " " + _infoDirectoryNew.Text;
                Info.ForeColor = Color.Black;
            }
        }

        private void NewDirectoryTextChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private void ToSelectedIndexChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private readonly AsyncLoader _branchListLoader = new AsyncLoader();

        private void UpdateBranches(RemoteActionResult<IReadOnlyList<IGitRef>> branchList)
        {
            Cursor = Cursors.Default;

            if (branchList.HostKeyFail)
            {
                string remoteUrl = _NO_TRANSLATE_From.Text;

                if (FormRemoteProcess.AskForCacheHostkey(this, Module, remoteUrl))
                {
                    LoadBranches();
                }
            }
            else if (branchList.AuthenticationFail)
            {
                if (FormPuttyError.AskForKey(this, out _))
                {
                    LoadBranches();
                }
            }
            else
            {
                string text = _NO_TRANSLATE_Branches.Text;
                List<string> names = _defaultBranchItems.Concat(branchList.Result.Select(o => o.LocalName)).ToList();
                _NO_TRANSLATE_Branches.DataSource = names;
                if (names.Any(a => a == text))
                {
                    _NO_TRANSLATE_Branches.Text = text;
                }
            }
        }

        private void LoadBranches()
        {
            string from = _NO_TRANSLATE_From.Text;
            Cursor = Cursors.AppStarting;
            _branchListLoader.LoadAsync(() => Module.GetRemoteServerRefs(from, false, true), UpdateBranches);
        }

        private void Branches_DropDown(object sender, EventArgs e)
        {
            LoadBranches();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _branchListLoader.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}

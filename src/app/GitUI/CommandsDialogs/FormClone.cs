using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormClone : GitExtensionsDialog
    {
        private readonly TranslationString _infoNewRepositoryLocation = new("The repository will be cloned to a new directory located here:" + Environment.NewLine + "{0}");
        private readonly TranslationString _infoDirectoryExists = new("(Directory already exists)");
        private readonly TranslationString _infoDirectoryNew = new("(New directory)");
        private readonly TranslationString _questionOpenRepo = new("The repository has been cloned successfully." + Environment.NewLine + "Do you want to open the new repository \"{0}\" now?");
        private readonly TranslationString _questionOpenRepoCaption = new("Open");
        private readonly TranslationString _branchDefaultRemoteHead = new("(default: remote HEAD)" /* Has a colon, so won't alias with any valid branch name */);
        private readonly TranslationString _branchNone = new("(none: don't checkout after clone)" /* Has a colon, so won't alias with any valid branch name */);
        private readonly TranslationString _errorDestinationNotSupplied = new("You need to specify destination folder.");
        private readonly TranslationString _errorDestinationNotRooted = new("Destination folder must be an absolute path.");
        private readonly TranslationString _errorCloneFailed = new("Clone Failed");

        private readonly bool _openedFromProtocolHandler;
        private readonly string? _url;
        private readonly CancellationTokenSequence _branchLoaderSequence = new();
        private readonly EventHandler<GitModuleEventArgs>? _gitModuleChanged;
        private readonly IReadOnlyList<string> _defaultBranchItems;
        private string? _puttySshKey;

        public FormClone(IGitUICommands commands, string? url, bool openedFromProtocolHandler, EventHandler<GitModuleEventArgs>? gitModuleChanged)
            : base(commands, enablePositionRestore: false)
        {
            _gitModuleChanged = gitModuleChanged;
            InitializeComponent();

            MinimumSize = new Size(Width, PreferredMinimumHeight);

            InitializeComplete();
            _openedFromProtocolHandler = openedFromProtocolHandler;
            _url = url;
            _defaultBranchItems = new[] { _branchDefaultRemoteHead.Text, _branchNone.Text };
            _NO_TRANSLATE_Branches.DataSource = _defaultBranchItems;
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            base.OnRuntimeLoad(e);

            IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);
            _NO_TRANSLATE_From.DataSource = repositoryHistory;
            _NO_TRANSLATE_From.DisplayMember = nameof(Repository.Path);

            _NO_TRANSLATE_To.Text = AppSettings.DefaultCloneDestinationPath;

            if (PathUtil.CanBeGitURL(_url))
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
                        if (TryExtractUrl(text, out string possibleURL))
                        {
                            _NO_TRANSLATE_From.Text = possibleURL;
                        }
                    }
                }
                catch
                {
                    // We tried.
                }

                // if the From field is empty, then fill it with the current repository remote URL in hope
                // that the cloned repository is hosted on the same server
                if (string.IsNullOrWhiteSpace(_NO_TRANSLATE_From.Text) && Module.IsValidGitWorkingDir())
                {
                    string currentBranchRemote = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, Module.GetSelectedBranch()));
                    if (string.IsNullOrEmpty(currentBranchRemote))
                    {
                        IReadOnlyList<string> remotes = Module.GetRemoteNames();

                        if (remotes.Any(s => s.Equals("origin", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            currentBranchRemote = "origin";
                        }
                        else
                        {
                            currentBranchRemote = remotes.Count > 0 ? remotes[0] : null;
                        }
                    }

                    string pushUrl = Module.GetSetting(string.Format(SettingKeyString.RemotePushUrl, currentBranchRemote));
                    if (string.IsNullOrEmpty(pushUrl))
                    {
                        pushUrl = Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentBranchRemote));
                    }

                    _NO_TRANSLATE_From.Text = pushUrl;

                    try
                    {
                        // If the from directory is filled with the pushUrl from current working directory, set the destination directory to the parent
                        if (!string.IsNullOrWhiteSpace(pushUrl) && string.IsNullOrWhiteSpace(_NO_TRANSLATE_To.Text) && !string.IsNullOrWhiteSpace(Module.WorkingDir))
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
            if (string.IsNullOrWhiteSpace(_NO_TRANSLATE_To.Text) && !string.IsNullOrWhiteSpace(Module.WorkingDir))
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

            FromTextUpdate(this, EventArgs.Empty);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.Default;
                _branchLoaderSequence.CancelCurrent();

                // validate if destination path is supplied
                string destination = _NO_TRANSLATE_To.Text;
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

                string dirTo = Path.Combine(destination, _NO_TRANSLATE_NewDirectory.Text);

                // this will fail if the path is anyhow invalid
                dirTo = PathUtil.Resolve(dirTo);

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
                string? branch = _NO_TRANSLATE_Branches.Text;
                if (branch == _branchDefaultRemoteHead.Text)
                {
                    branch = "";
                }
                else if (branch == _branchNone.Text)
                {
                    branch = null;
                }

                ArgumentString cloneCmd = Commands.Clone(_NO_TRANSLATE_From.Text,
                    dirTo,
                    UICommands.Module.GetPathForGitExecution,
                    CentralRepository.Checked,
                    cbIntializeAllSubmodules.Checked,
                    branch, depth, isSingleBranch);
                using (FormRemoteProcess fromProcess = new(UICommands, cloneCmd))
                {
                    string sourceRepo = PathUtil.IsLocalFile(_NO_TRANSLATE_From.Text)
                        ? UICommands.Module.GetPathForGitExecution(_NO_TRANSLATE_From.Text)
                        : _NO_TRANSLATE_From.Text;
                    fromProcess.SetUrlTryingToConnect(sourceRepo);
                    fromProcess.ShowDialog(this);

                    if (fromProcess.ErrorOccurred() || Module.InTheMiddleOfPatch())
                    {
                        return;
                    }
                }

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(_NO_TRANSLATE_From.Text);
                    await RepositoryHistoryManager.Locals.AddAsMostRecentAsync(dirTo);
                });

                if (!string.IsNullOrEmpty(_puttySshKey))
                {
                    GitModule clonedGitModule = new(dirTo);
                    clonedGitModule.SetSetting(string.Format(SettingKeyString.RemotePuttySshKey, "origin"), _puttySshKey);
                    clonedGitModule.LocalConfigFile.Save();
                }

                if (_openedFromProtocolHandler && AskIfNewRepositoryShouldBeOpened(dirTo))
                {
                    Hide();
                    IGitUICommands uiCommands = UICommands.WithWorkingDirectory(dirTo);
                    uiCommands.StartBrowseDialog(owner: null);
                }
                else if (ShowInTaskbar == false && _gitModuleChanged is not null &&
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
            string userSelectedPath = OsShellUtil.PickFolder(this, _NO_TRANSLATE_From.Text);

            if (userSelectedPath is not null)
            {
                _NO_TRANSLATE_From.Text = userSelectedPath;
            }

            FromTextUpdate(sender, e);
        }

        private void ToBrowseClick(object sender, EventArgs e)
        {
            string userSelectedPath = OsShellUtil.PickFolder(this, _NO_TRANSLATE_To.Text);

            if (userSelectedPath is not null)
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
            if (!GitSshHelpers.IsPlink)
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
            bool destinationUnfilled = string.IsNullOrEmpty(_NO_TRANSLATE_To.Text) || _NO_TRANSLATE_To.Text.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0;
            bool subDirectoryUnfilled = string.IsNullOrEmpty(_NO_TRANSLATE_NewDirectory.Text) || _NO_TRANSLATE_NewDirectory.Text.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0;

            string destinationDirectory = destinationUnfilled ? $@"[{destinationLabel.Text}]" : _NO_TRANSLATE_To.Text;
            string destinationSubDirectory = subDirectoryUnfilled ? $@"[{subdirectoryLabel.Text}]" : _NO_TRANSLATE_NewDirectory.Text;

            string destinationPath = Path.Combine(destinationDirectory, destinationSubDirectory);

            string newRepositoryLocationInfo = string.Format(_infoNewRepositoryLocation.Text, destinationPath);

            if (destinationUnfilled || subDirectoryUnfilled)
            {
                Info.Text = newRepositoryLocationInfo;
                Info.ForeColor = Color.Red.AdaptTextColor();
                return;
            }

            if (Directory.Exists(destinationPath) && Directory.EnumerateFileSystemEntries(destinationPath).Any())
            {
                Info.Text = $@"{newRepositoryLocationInfo} {_infoDirectoryExists.Text}";
                Info.ForeColor = Color.Red.AdaptTextColor();
                return;
            }

            Info.Text = $@"{newRepositoryLocationInfo} {_infoDirectoryNew.Text}";
            Info.ForeColor = SystemColors.ControlText;
        }

        private void NewDirectoryTextChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private void ToSelectedIndexChanged(object sender, EventArgs e)
        {
            ToTextUpdate(sender, e);
        }

        private void UpdateBranches(RemoteActionResult<IReadOnlyList<IGitRef>> branchList)
        {
            Cursor = Cursors.Default;

            if (branchList.HostKeyFail)
            {
                string remoteUrl = _NO_TRANSLATE_From.Text;

                if (FormRemoteProcess.AskForCacheHostkey(this, remoteUrl))
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

            CancellationToken cancellationToken = _branchLoaderSequence.Next();
            ThreadHelper.FileAndForget(async () =>
            {
                RemoteActionResult<IReadOnlyList<IGitRef>> branchList;

                IReadOnlyList<IGitRef> refs = Module.GetRemoteServerRefs(from, false, true, out string? errorOutput, cancellationToken);

                if (string.IsNullOrEmpty(errorOutput))
                {
                    branchList = new(result: refs, authenticationFail: false, hostKeyFail: false);
                }
                else if (errorOutput.Contains("FATAL ERROR") && errorOutput.Contains("authentication"))
                {
                    // If the authentication failed because of a missing key, ask the user to supply one.
                    branchList = new(result: null, authenticationFail: true, hostKeyFail: false);
                }
                else if (errorOutput.Contains("the server's host key is not cached in the registry", StringComparison.InvariantCultureIgnoreCase))
                {
                    branchList = new(result: null, authenticationFail: false, hostKeyFail: true);
                }
                else
                {
                    throw new ExternalOperationException(workingDirectory: Module.WorkingDir, innerException: new Exception(errorOutput));
                }

                await this.SwitchToMainThreadAsync(cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    UpdateBranches(branchList);
                }
            });
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
                _branchLoaderSequence.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Check whether the given string contains one or more valid git URLs and extracts
        /// the first URL that exists, if any.
        /// </summary>
        /// <remarks>
        /// PathUtil.CanBeGitURL is used as a standard way to detect a git URL.
        /// The first URL extracted from <paramref name="contents"/> is assigned to
        /// <paramref name="url"/>. If <paramref name="contents"/> contains more than one URL,
        /// subsequent URLs are not extracted.
        /// </remarks>
        /// <param name="contents">A string to attempt to extract URLs from.</param>
        /// <param name="url">A <see cref="string"/> that contains the URL, if any, extracted from <paramref name="contents"/>.</param>
        /// <returns><see langword="true"/> if a URL was extracted; otherwise <see langword="false"/>.</returns>
        private static bool TryExtractUrl(string contents, out string url)
        {
            url = "";

            if (string.IsNullOrEmpty(contents))
            {
                return false;
            }

            string[] parts = contents.Split(' ');
            foreach (string s in parts)
            {
                if (PathUtil.CanBeGitURL(s))
                {
                    url = s;
                    break;
                }
            }

            return !string.IsNullOrEmpty(url);
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormClone _form;

            public TestAccessor(FormClone form)
            {
                _form = form;
            }

            public bool TryExtractUrl(string text, out string url) => FormClone.TryExtractUrl(text, out url);
        }
    }
}

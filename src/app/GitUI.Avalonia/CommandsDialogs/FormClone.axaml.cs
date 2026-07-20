using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormClone.cs. The clone runs in FormRemoteProcess through
// OpenSSH; the PuTTY pieces (Load SSH key button and registry/key-agent recovery prompts)
// are not ported. Remote-branch discovery keeps the original busy cursor and reports native
// Git/OpenSSH errors directly. The history combos hold the path strings rather than Repository
// objects, and the "opened as dialog" check replaces the WinForms ShowInTaskbar test.
public sealed partial class FormClone : GitExtensionsDialog
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

    public FormClone()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
        _defaultBranchItems = [];
    }

    public FormClone(IGitUICommands commands, string? url, bool openedFromProtocolHandler, EventHandler<GitModuleEventArgs>? gitModuleChanged)
        : base(commands, enablePositionRestore: false)
    {
        _gitModuleChanged = gitModuleChanged;
        InitializeComponent();
        WireControls();
        AcceptButton = Ok;

        InitializeComplete();
        _openedFromProtocolHandler = openedFromProtocolHandler;
        _url = url;
        _defaultBranchItems = new[] { _branchDefaultRemoteHead.Text, _branchNone.Text };
        _NO_TRANSLATE_Branches.ItemsSource = _defaultBranchItems;
    }

    private void WireControls()
    {
        Ok.Click += OkClick;
        FromBrowse.Click += FromBrowseClick;
        ToBrowse.Click += ToBrowseClick;
        _NO_TRANSLATE_From.SelectionChanged += FromSelectedIndexChanged;
        _NO_TRANSLATE_From.PropertyChanged += (s, args) =>
        {
            // The WinForms TextUpdate equivalent: react to typing in the editable combo.
            if (args.Property == ComboBox.TextProperty)
            {
                FromTextUpdate(s!, EventArgs.Empty);
            }
        };
        _NO_TRANSLATE_To.SelectionChanged += ToSelectedIndexChanged;
        _NO_TRANSLATE_To.PropertyChanged += (s, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                ToTextUpdate(s!, EventArgs.Empty);
            }
        };
        _NO_TRANSLATE_NewDirectory.TextChanged += NewDirectoryTextChanged;
        _NO_TRANSLATE_Branches.DropDownOpened += Branches_DropDown;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        base.OnRuntimeLoad(e);

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);
        _NO_TRANSLATE_From.ItemsSource = repositoryHistory.Select(repository => repository.Path).ToArray();

        IList<Repository> localsHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        string[] historicPaths = [.. localsHistory.Select(x => x.GetParentPath())
                                              .Where(x => !string.IsNullOrEmpty(x))
                                              .Distinct(StringComparer.CurrentCultureIgnoreCase)];
        _NO_TRANSLATE_To.ItemsSource = historicPaths;
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
                if (WinFormsShims.Clipboard.ContainsText())
                {
                    string text = WinFormsShims.Clipboard.GetText() ?? string.Empty;

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
                string? currentBranchRemote = Module.GetSetting(string.Format(SettingKeyString.BranchRemote, Module.GetSelectedBranch()));
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

    protected override void OnClosed(EventArgs e)
    {
        Cursor = null;
        _branchLoaderSequence.Dispose();
        base.OnClosed(e);
    }

    private string GetFromText() => _NO_TRANSLATE_From.SelectedItem as string ?? _NO_TRANSLATE_From.Text ?? string.Empty;

    private string GetToText() => _NO_TRANSLATE_To.SelectedItem as string ?? _NO_TRANSLATE_To.Text ?? string.Empty;

    private string GetBranchText() => _NO_TRANSLATE_Branches.SelectedItem as string ?? _NO_TRANSLATE_Branches.Text ?? string.Empty;

    private void OkClick(object sender, EventArgs e)
    {
        try
        {
            Cursor = null;
            _branchLoaderSequence.CancelCurrent();

            // validate if destination path is supplied
            string destination = GetToText();
            if (string.IsNullOrWhiteSpace(destination))
            {
                MessageBoxes.Show(this, _errorDestinationNotSupplied.Text, _errorCloneFailed.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
                _NO_TRANSLATE_To.Focus();
                return;
            }

            if (!Path.IsPathRooted(destination))
            {
                MessageBoxes.Show(this, _errorDestinationNotRooted.Text, _errorCloneFailed.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
                _NO_TRANSLATE_To.Focus();
                return;
            }

            string dirTo = Path.Combine(destination, _NO_TRANSLATE_NewDirectory.Text ?? string.Empty);

            // this will fail if the path is anyhow invalid
            dirTo = PathUtil.Resolve(dirTo);

            if (!Directory.Exists(dirTo))
            {
                Directory.CreateDirectory(dirTo);
            }

            // Shallow clone params
            int? depth = null;
            bool? isSingleBranch = null;
            if (cbDownloadFullHistory.IsChecked != true)
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
            string? branch = GetBranchText();
            if (branch == _branchDefaultRemoteHead.Text)
            {
                branch = "";
            }
            else if (branch == _branchNone.Text)
            {
                branch = null;
            }

            string from = GetFromText();
            ArgumentString cloneCmd = Commands.Clone(from,
                dirTo,
                UICommands.Module.GetPathForGitExecution,
                CentralRepository.IsChecked == true,
                cbIntializeAllSubmodules.IsChecked == true,
                branch, depth, isSingleBranch);

            bool success = FormRemoteProcess.ShowDialog(this, UICommands, cloneCmd);
            if (!success || Module.InTheMiddleOfPatch())
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(from);
                await RepositoryHistoryManager.Locals.AddAsMostRecentAsync(dirTo);
            });

            if (_openedFromProtocolHandler && AskIfNewRepositoryShouldBeOpened(dirTo))
            {
                Hide();
                IGitUICommands uiCommands = UICommands.WithWorkingDirectory(dirTo);
                uiCommands.StartBrowseDialog(owner: null);
            }
            else if (_gitModuleChanged is not null &&
                AskIfNewRepositoryShouldBeOpened(dirTo))
            {
                _gitModuleChanged(this, new GitModuleEventArgs(new GitModule(UICommands.GetRequiredService<IGitExecutorProvider>(), dirTo)));
            }

            Close();
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, "Exception: " + ex.Message, _errorCloneFailed.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
        }
    }

    private bool AskIfNewRepositoryShouldBeOpened(string dirTo)
    {
        return MessageBoxes.Show(this, string.Format(_questionOpenRepo.Text, dirTo), _questionOpenRepoCaption.Text,
            WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.Yes;
    }

    private void FromBrowseClick(object sender, EventArgs e)
    {
        string? userSelectedPath = OsShellUtil.PickFolder(this, GetFromText());

        if (userSelectedPath is not null)
        {
            _NO_TRANSLATE_From.Text = userSelectedPath;
        }

        FromTextUpdate(sender, e);
    }

    private void ToBrowseClick(object sender, EventArgs e)
    {
        string? userSelectedPath = OsShellUtil.PickFolder(this, GetToText());

        if (userSelectedPath is not null)
        {
            _NO_TRANSLATE_To.Text = userSelectedPath;
        }

        ToTextUpdate(sender, e);
    }

    private void FromSelectedIndexChanged(object sender, EventArgs e)
    {
        FromTextUpdate(sender, e);
    }

    private void FromTextUpdate(object sender, EventArgs e)
    {
        string path = PathUtil.GetRepositoryName(GetFromText());

        if (path != "")
        {
            _NO_TRANSLATE_NewDirectory.Text = path;
        }

        _NO_TRANSLATE_Branches.ItemsSource = _defaultBranchItems;

        ToTextUpdate(sender, e);
    }

    private void ToTextUpdate(object sender, EventArgs e)
    {
        string toText = GetToText();
        string newDirectoryText = _NO_TRANSLATE_NewDirectory.Text ?? string.Empty;
        bool destinationUnfilled = string.IsNullOrEmpty(toText) || toText.IndexOfAny(Delimiters.InvalidPathCharsSearchValues) >= 0;
        bool subDirectoryUnfilled = string.IsNullOrEmpty(newDirectoryText) || newDirectoryText.IndexOfAny(Delimiters.InvalidPathCharsSearchValues) >= 0;

        string destinationDirectory = destinationUnfilled ? $@"[{AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(destinationLabel.Text ?? "")}]" : toText;
        string destinationSubDirectory = subDirectoryUnfilled ? $@"[{AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(subdirectoryLabel.Text ?? "")}]" : newDirectoryText;

        string destinationPath = Path.Combine(destinationDirectory, destinationSubDirectory);

        string newRepositoryLocationInfo = string.Format(_infoNewRepositoryLocation.Text, destinationPath);

        if (destinationUnfilled || subDirectoryUnfilled)
        {
            Info.Text = newRepositoryLocationInfo;
            Info.Foreground = Brushes.Red;
            return;
        }

        if (Directory.Exists(destinationPath) && Directory.EnumerateFileSystemEntries(destinationPath).Any())
        {
            Info.Text = $@"{newRepositoryLocationInfo} {_infoDirectoryExists.Text}";
            Info.Foreground = Brushes.Red;
            return;
        }

        Info.Text = $@"{newRepositoryLocationInfo} {_infoDirectoryNew.Text}";
        Info.ClearValue(Avalonia.Controls.TextBlock.ForegroundProperty);
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
        string text = GetBranchText();
        List<string> names = [.. _defaultBranchItems, .. branchList.Result!.Select(o => o.LocalName!)];
        _NO_TRANSLATE_Branches.ItemsSource = names;
        if (names.Any(a => a == text))
        {
            _NO_TRANSLATE_Branches.Text = text;
        }
    }

    private void LoadBranches()
    {
        string from = GetFromText();
        Cursor = new Cursor(StandardCursorType.Wait);

        CancellationToken cancellationToken = _branchLoaderSequence.Next();
        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<IGitRef> refs = [];
            string? errorOutput;
            try
            {
                refs = Module.GetRemoteServerRefs(from, false, true, out errorOutput, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                errorOutput = ex.Message;
            }

            await this.SwitchToMainThreadAsync();
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Cursor = null;
            if (!string.IsNullOrWhiteSpace(errorOutput))
            {
                MessageBoxes.ShowError(this, errorOutput);
                return;
            }

            UpdateBranches(new RemoteActionResult<IReadOnlyList<IGitRef>>(
                result: refs,
                authenticationFail: false,
                hostKeyFail: false));
        });
    }

    private void Branches_DropDown(object sender, EventArgs e)
    {
        LoadBranches();
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

    public override void AddTranslationItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.AddTranslationItems(translation);

        // The WinForms Designer stores this tooltip under the ToolTip component's name.
        translation.AddTranslationItem(nameof(FormClone), nameof(cbDownloadFullHistory), "ttHints", CbDownloadFullHistoryToolTipText);
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);

        string? toolTip = translation.TranslateItem(nameof(FormClone), nameof(cbDownloadFullHistory), "ttHints", () => CbDownloadFullHistoryToolTipText);
        ToolTip.SetTip(cbDownloadFullHistory, toolTip);
    }

    private static string CbDownloadFullHistoryToolTipText =>
        "The default Git behavior is to download all historical revisions.\n" +
        "If you turn this off, we'll only download the latest revision for all branches.\n\n" +
        "Actual command line (if unchecked): --depth 1 --no-single-branch";

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormClone _form;

        public TestAccessor(FormClone form)
        {
            _form = form;
        }

        public ComboBox Branches => _form._NO_TRANSLATE_Branches;

        public ComboBox Source => _form._NO_TRANSLATE_From;

        public void LoadBranches() => _form.LoadBranches();

        public bool TryExtractUrl(string text, out string url) => FormClone.TryExtractUrl(text, out url);
    }
}

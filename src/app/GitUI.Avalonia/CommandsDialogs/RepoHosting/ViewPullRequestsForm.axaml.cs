using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.RepoHosting;

public partial class ViewPullRequestsForm : GitModuleForm
{
    #region Translation
    private readonly TranslationString _strFailedToFetchPullData = new("Failed to fetch pull data!");
    private readonly TranslationString _strFailedToLoadDiscussionItem = new("Failed to post discussion item!");
    private readonly TranslationString _strFailedToClosePullRequest = new("Failed to close pull request!");
    private readonly TranslationString _strFailedToLoadDiffData = new("Failed to load diff data!");
    private readonly TranslationString _strCouldNotLoadDiscussion = new("Could not load discussion!");
    private readonly TranslationString _strLoading = new(" : LOADING : ");
    private readonly TranslationString _strUnableUnderstandPatch = new("Error: Unable to understand patch");
    private readonly TranslationString _strRemoteAlreadyExist = new("ERROR: Remote with name {0} already exists but it points to a different repository!\r\nDetails: Is {1} expected {2}");
    private readonly TranslationString _strCouldNotAddRemote = new("Could not add remote with name {0} and URL {1}");
    private readonly TranslationString _strRemoteIgnore = new("Remote ignored");
    #endregion

    private GitProtocol _cloneGitProtocol;
    private IPullRequestInformation? _currentPullRequestInfo;
    private Dictionary<string, string>? _diffCache;
    private readonly IRepositoryHostPlugin _gitHoster = null!;
    private IReadOnlyList<IHostedRemote>? _hostedRemotes;
    private bool _isFirstLoad;
    private IReadOnlyList<IPullRequestInformation>? _pullRequestsInfo;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    // Framework constraint: the Avalonia AsyncLoader owns TaskManager-backed execution.
    private readonly AsyncLoader _loader = new();
    private readonly CancellationTokenSequence _pullRequestsSequence = new();
    private readonly CancellationTokenSequence _detailsSequence = new();
    private readonly CancellationTokenSequence _discussionSequence = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private string _currentRemoteName = string.Empty;
    private IReadOnlyList<Remote> _moduleRemotes = [];
    private IReadOnlyList<HostedRemoteRow> _hostedRemoteRows = [];
    private readonly double[] _pullRequestColumnWidths = new double[5];
    private bool _pullRequestColumnsSizedToContent;

    [GeneratedRegex(@"(?:\n|^)diff --git ", RegexOptions.ExplicitCapture)]
    private static partial Regex DiffCommandRegex { get; }

    [GeneratedRegex(@"^a/([^\n]+) b/(?<name>[^\n]+)\s*(?<value>.*)$", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex FilePartRegex { get; }

    public ViewPullRequestsForm()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public ViewPullRequestsForm(IGitUICommands commands, IRepositoryHostPlugin gitHoster)
        : base(commands, enablePositionRestore: true)
    {
        _gitHoster = gitHoster;
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        if (TryGetUICommands(out _))
        {
            // Framework constraint: WinForms GitModuleControls discover their containing form;
            // Avalonia embedded controls receive the same command source explicitly.
            _fileStatusList.UICommandsSource = this;
            _diffViewer.UICommandsSource = this;
        }

        _pullRequestsList.ItemTemplate = new FuncDataTemplate<PullRequestRow>(
            CreatePullRequestRow,
            supportsRecycling: false);
        _discussionWB.ItemTemplate = new FuncDataTemplate<DiscussionRow>(
            CreateDiscussionRow,
            supportsRecycling: false);
        ResizeColumns([]);

        // Framework constraint: WinForms remeasures native header glyphs when the resolved theme changes.
        ActualThemeVariantChanged += (_, _) => ResizeColumnsToFitContent();
        WinFormsSplitContainerSizer.Attach(splitContainer2, sourceHeight: 511, sourceSplitterDistance: 146);
        WinFormsSplitContainerSizer.Attach(splitContainer3, sourceHeight: 331, sourceSplitterDistance: 116);

        _selectHostedRepoCB.SelectionChanged += _selectedOwner_SelectedIndexChanged;
        _pullRequestsList.SelectionChanged += _pullRequestsList_SelectedIndexChanged;

        // Framework constraint: the source end-scroll runs after WebBrowser.DocumentCompleted;
        // an Avalonia list can finish loading before its hidden tab is materialized, so re-run it when shown.
        _discussionWB.ObserveVisibility(tabControl1, tabPage2);
        _loader.LoadingError += (sender, ex) =>
        {
            MessageBoxes.Show(
                this,
                ex.Exception.ToString(),
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            this.UnMask();
        };
        _pullRequestsList.SizeChanged += _pullRequestsList_Resize;
        _fileStatusList.SelectedIndexChanged += _fileStatusList_SelectedIndexChanged;
        _diffViewer.ExtraDiffArgumentsChanged += _fileStatusList_SelectedIndexChanged;
        _diffViewer.TopScrollReached += FileViewer_TopScrollReached;
        _diffViewer.BottomScrollReached += FileViewer_BottomScrollReached;
        _fetchBtn.Click += _fetchBtn_Click;
        _addAndFetchBtn.Click += _addAsRemoteAndFetch_Click;
        _closePullRequestBtn.Click += _closePullRequestBtn_Click;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        ViewPullRequestsForm_Load(this, e);

        // Framework constraint: WinForms activates the first eligible control by tab order.
        _selectHostedRepoCB.Focus();
    }

    private void ViewPullRequestsForm_Load(object sender, EventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        _discussionWB.DocumentCompleted += _discussionWB_DocumentCompleted;

        this.Mask();

        // load all hosted repositories.
        // We do this now because we want to do it in the async part.
        _loader.FileAndForget(() => InitializeAsync(_lifetimeCancellation.Token));
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="e">The window-closed event data.</param>
    protected override void OnClosed(EventArgs e)
    {
        _lifetimeCancellation.Cancel();
        _pullRequestsSequence.CancelCurrent();
        _detailsSequence.CancelCurrent();
        _discussionSequence.CancelCurrent();
        _loader.JoinPendingOperations();
        _pullRequestsSequence.Dispose();
        _detailsSequence.Dispose();
        _discussionSequence.Dispose();
        _lifetimeCancellation.Dispose();
        base.OnClosed(e);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            string currentRemote = await Task.Run(Module.GetCurrentRemote, cancellationToken);
            IReadOnlyList<Remote> remotes = await Module.GetRemotesAsync().WaitAsync(cancellationToken);

            // Load all hosted repositories.
            (IHostedRemote[] hostedRemotes, HostedRemoteRow[] hostedRemoteRows) = await Task.Run(
                () =>
                {
                    IHostedRemote[] remotesForModule = _gitHoster.GetHostedRemotesForModule().ToArray();
                    return (
                        remotesForModule,
                        remotesForModule.Select(HostedRemoteRow.Create).ToArray());
                },
                cancellationToken);

            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _currentRemoteName = currentRemote;
            _moduleRemotes = remotes;
            _hostedRemotes = hostedRemotes;
            _hostedRemoteRows = hostedRemoteRows;
            _isFirstLoad = true;
            _selectHostedRepoCB.ItemsSource = hostedRemoteRows;

            foreach (HostedRemoteRow remote in hostedRemoteRows.Where(remote => remote.Error is not null))
            {
                MessageBoxes.Show(
                    this,
                    string.Format(TranslatedStrings.RemoteInError, remote.Error!.Message, remote.DisplayData),
                    _strRemoteIgnore.Text,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }

            SelectHostedRepositoryForCurrentRemote();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    ex.ToString(),
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(this.UnMask);
        }
    }

    private void _selectedOwner_SelectedIndexChanged(object sender, EventArgs e)
    {
        // if fails to load this remote, select the next one
        CancellationToken cancellationToken = _pullRequestsSequence.Next();
        _detailsSequence.CancelCurrent();
        _discussionSequence.CancelCurrent();
        ResetAllAndShowLoadingPullRequests();
        bool transferFocus = _selectHostedRepoCB.IsKeyboardFocusWithin;
        _selectHostedRepoCB.IsEnabled = false;
        if (transferFocus)
        {
            // Framework constraint: WinForms transfers focus to the next eligible control
            // when the active repository selector is disabled for the asynchronous load.
            _fetchBtn.Focus();
        }

        _loader.FileAndForget(() => LoadPullRequestsAsync(cancellationToken));
    }

    private void FileViewer_TopScrollReached(object? sender, EventArgs e)
    {
        _fileStatusList.SelectPreviousVisibleItem();
        _diffViewer.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object? sender, EventArgs e)
    {
        _fileStatusList.SelectNextVisibleItem();
        _diffViewer.ScrollToTop();
    }

    private async Task LoadPullRequestsAsync(CancellationToken cancellationToken)
    {
        HostedRemoteRow? selectedRemote = null;
        await _loader.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        selectedRemote = _selectHostedRepoCB.SelectedItem as HostedRemoteRow;

        if (selectedRemote?.Repository is null)
        {
            // If loading this remote failed, select the next one.
            _pullRequestsList.ItemsSource = Array.Empty<PullRequestRow>();
            ResizeColumnsToFitContent();
            _selectHostedRepoCB.IsEnabled = true;
            if (_isFirstLoad)
            {
                SelectNextHostedRepository();
            }

            return;
        }

        try
        {
            IReadOnlyList<IPullRequestInformation> pullRequests = await Task.Run(
                selectedRemote.Repository.GetPullRequests,
                cancellationToken);

            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _selectHostedRepoCB.IsEnabled = true;
            SetPullRequestsData(pullRequests);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    _strFailedToFetchPullData.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void SetPullRequestsData(IReadOnlyList<IPullRequestInformation>? infos)
    {
        if (_isFirstLoad)
        {
            if (infos?.Count is 0 && _hostedRemoteRows.Count > 0)
            {
                SelectNextHostedRepository();
                return;
            }
            else
            {
                _isFirstLoad = false;
            }
        }

        _pullRequestsInfo = infos;
        _pullRequestsList.ItemsSource = Array.Empty<PullRequestRow>();
        if (_pullRequestsInfo is null)
        {
            return;
        }

        LoadListView();
    }

    private void SelectHostedRepositoryForCurrentRemote()
    {
        string currentRemote = _currentRemoteName;

        // Local branches have no current remote, return value is empty string.
        // In this case we fallback to the first remote in the list.
        // Currently, local git repo with no remote will show error message and can not open this dialog.
        // So there will always be at least 1 remote when this dialog is open
        Remote? selectedRemote = _moduleRemotes.FirstOrDefault(
            remote => string.IsNullOrEmpty(currentRemote)
                || string.Equals(remote.Name, currentRemote, StringComparison.OrdinalIgnoreCase));
        _cloneGitProtocol = selectedRemote is Remote currentGitRemote
            && !string.IsNullOrEmpty(currentGitRemote.FetchUrl)
            && currentGitRemote.FetchUrl.IsUrlUsingHttp()
            ? GitProtocol.Https
            : GitProtocol.Ssh;

        HostedRemoteRow? hostedRemote = _hostedRemoteRows.FirstOrDefault(
            remote => string.Equals(remote.Name, currentRemote, StringComparison.OrdinalIgnoreCase));
        _selectHostedRepoCB.SelectedItem = hostedRemote ?? _hostedRemoteRows.FirstOrDefault();
    }

    private void SelectNextHostedRepository()
    {
        if (_selectHostedRepoCB.ItemCount == 0)
        {
            return;
        }

        int i = _selectHostedRepoCB.SelectedIndex + 1;
        if (i >= _selectHostedRepoCB.ItemCount)
        {
            return;
        }

        _selectHostedRepoCB.SelectedIndex = i;

        // Framework constraint: Avalonia raises SelectionChanged synchronously for the index assignment,
        // so the source's explicit second handler call would start duplicate provider work.
    }

    private void ResetAllAndShowLoadingPullRequests()
    {
        ResetDetails();
        _pullRequestsInfo = null;
        _pullRequestsList.ItemsSource = new[] { PullRequestRow.Placeholder(_strLoading.Text) };
    }

    private void LoadListView()
    {
        IReadOnlyList<IPullRequestInformation> pullRequests = _pullRequestsInfo
            ?? throw new InvalidOperationException("Pull request data has not been loaded.");
        PullRequestRow[] rows = pullRequests.Select(PullRequestRow.FromPullRequest).ToArray();
        _pullRequestsList.ItemsSource = rows;
        ResizeColumnsToFitContent();
        _pullRequestsList.SelectedIndex = rows.Length > 0 ? 0 : -1;
    }

    private void ResizeColumnsToFitContent()
    {
        ResizeColumns(_pullRequestsList.Items.Cast<PullRequestRow>().ToArray());
    }

    private void _pullRequestsList_SelectedIndexChanged(object sender, EventArgs e)
    {
        IPullRequestInformation? previousPullRequest = _currentPullRequestInfo;
        _currentPullRequestInfo = (_pullRequestsList.SelectedItem as PullRequestRow)?.PullRequest;
        if (_currentPullRequestInfo is null)
        {
            _detailsSequence.CancelCurrent();
            _discussionSequence.CancelCurrent();
            _discussionWB.ItemsSource = Array.Empty<DiscussionRow>();
            _diffViewer.ViewText(string.Empty, string.Empty);
            return;
        }

        if (ReferenceEquals(previousPullRequest, _currentPullRequestInfo))
        {
            return;
        }

        _detailsSequence.CancelCurrent();
        _discussionSequence.CancelCurrent();
        ResetDetails(clearPullRequest: false);
        _currentPullRequestInfo.HeadRepo.CloneProtocol = _cloneGitProtocol;
        LoadDiffPatch();
        LoadDiscussion();
    }

    private void _pullRequestsList_Resize(object sender, EventArgs e)
    {
        Grid header = (Grid)(columnHeaderId.Parent
            ?? throw new InvalidOperationException("The pull-request header is not attached to its column grid."));
        int fillColumn = _pullRequestColumnsSizedToContent ? 1 : 4;
        header.ColumnDefinitions = WinFormsListViewColumnSizer.CreateColumns(_pullRequestColumnWidths, fillColumn);
    }

    private void LoadDiscussion()
    {
        // TODO make this operation async (requires change to Git.hub submodule)
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        CancellationToken cancellationToken = _discussionSequence.Next();
        _discussionWB.ItemsSource = new[] { DiscussionRow.Placeholder(_strLoading.Text) };
        _loader.FileAndForget(() => LoadDiscussionAsync(pullRequest, cancellationToken));
    }

    private async Task LoadDiscussionAsync(
        IPullRequestInformation pullRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            // The provider API is still synchronous, so keep this operation off the UI thread.
            IPullRequestDiscussion discussion = await Task.Run(
                pullRequest.GetDiscussion,
                cancellationToken);
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!ReferenceEquals(_currentPullRequestInfo, pullRequest))
            {
                return;
            }

            LoadDiscussion(discussion);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    _strCouldNotLoadDiscussion.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
                LoadDiscussion(null);
            }
        }
    }

    private void LoadDiscussion(IPullRequestDiscussion? discussion)
    {
        DiscussionRow[] rows = DiscussionHtmlCreator.CreateFor(discussion?.Entries)
            .Select(DiscussionRow.FromPresentation)
            .ToArray();
        _discussionWB.ItemsSource = rows;
        _discussionWB.NotifyDocumentCompleted();
    }

    private void _discussionWB_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
    {
        object? lastItem = _discussionWB.Items.Cast<object>().LastOrDefault();
        if (lastItem is null)
        {
            return;
        }

        _discussionWB.ScrollIntoView(lastItem);
        Dispatcher.UIThread.Post(
            () =>
            {
                ScrollViewer? scrollViewer = _discussionWB
                    .GetVisualDescendants()
                    .OfType<ScrollViewer>()
                    .FirstOrDefault();
                ItemsPresenter? itemsPresenter = _discussionWB
                    .GetVisualDescendants()
                    .OfType<ItemsPresenter>()
                    .FirstOrDefault();
                ListBoxItem? lastContainer = _discussionWB.ContainerFromIndex(_discussionWB.ItemCount - 1) as ListBoxItem;
                if (scrollViewer is null || itemsPresenter is null || lastContainer is null)
                {
                    return;
                }

                // Framework constraint: the source WebBrowser scrolls its body rectangle to the
                // end, including the final CSS entry margin. Preserve that trailing scroll range
                // even when Avalonia's items are shorter than the discussion viewport.
                itemsPresenter.MinHeight = Math.Max(
                    itemsPresenter.MinHeight,
                    scrollViewer.Viewport.Height + lastContainer.Margin.Bottom);
                scrollViewer.Offset = new Avalonia.Vector(scrollViewer.Offset.X, scrollViewer.Extent.Height);
            },
            DispatcherPriority.Loaded);
    }

    private async Task LoadDiffPatchAsync(
        IPullRequestInformation pullRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            string content = await pullRequest.GetDiffDataAsync().WaitAsync(cancellationToken);
            DiffSnapshot snapshot = ParseDiff(content, pullRequest.BaseSha, pullRequest.HeadSha);

            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!ReferenceEquals(_currentPullRequestInfo, pullRequest))
            {
                return;
            }

            _diffCache = snapshot.Patches;
            _fileStatusList.SetDiffs(snapshot.BaseRevision, snapshot.HeadRevision, snapshot.Items);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (InvalidDataException)
        {
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    _strUnableUnderstandPatch.Text,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    _strFailedToLoadDiffData.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void LoadDiffPatch()
    {
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        CancellationToken cancellationToken = _detailsSequence.Next();
        _loader.FileAndForget(() => LoadDiffPatchAsync(pullRequest, cancellationToken));
    }

    private void SplitAndLoadDiff(string diffData, string baseSha, string secondSha)
    {
        // baseSha is the sha of the merge to ("master") sha, the commit to be firstId
        DiffSnapshot snapshot = ParseDiff(diffData, baseSha, secondSha);
        _diffCache = snapshot.Patches;

        // Note: Commits in PR may not exist in the local repo
        _fileStatusList.SetDiffs(snapshot.BaseRevision, snapshot.HeadRevision, snapshot.Items);
    }

    private static DiffSnapshot ParseDiff(string diffData, string baseSha, string headSha)
    {
        GitRevision? baseRevision = ObjectId.TryParse(baseSha, out ObjectId baseId)
            ? new GitRevision(baseId)
            : null;
        if (!ObjectId.TryParse(headSha, out ObjectId headId))
        {
            throw new InvalidDataException("The pull request head revision is invalid.");
        }

        List<GitItemStatus> items = [];
        Dictionary<string, string> patches = [];
        IEnumerable<string> fileParts = DiffCommandRegex.Split(diffData)
            .Where(part => part.Trim().Length > 10);
        foreach (string part in fileParts)
        {
            Match match = FilePartRegex.Match(part);
            if (!match.Success)
            {
                throw new InvalidDataException("The pull request patch could not be parsed.");
            }

            GitItemStatus item = new(name: match.Groups["name"].Value.Trim())
            {
                IsChanged = true,
                IsNew = false,
                IsDeleted = false,
                IsTracked = true,
                Staged = StagedStatus.None,
            };
            items.Add(item);
            patches.Add(item.Name, match.Groups["value"].Value);
        }

        return new DiffSnapshot(baseRevision, new GitRevision(headId), items, patches);
    }

    private void _fetchBtn_Click(object sender, EventArgs e)
    {
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        ArgumentString command = Module.FetchCmd(
            pullRequest.HeadRepo.CloneUrl,
            pullRequest.HeadRef,
            pullRequest.FetchBranch,
            fetchTags: false);

        // Avalonia routes the modal Git process through the host command boundary.
        if (!UICommands.StartGitCommandProcessDialog(this, command))
        {
            return;
        }

        UICommands.RepoChangedNotifier.Notify();
        Close();
    }

    private void _addAsRemoteAndFetch_Click(object sender, EventArgs e)
    {
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        UICommands.RepoChangedNotifier.Lock();
        try
        {
            string remoteName = pullRequest.Owner;
            string remoteUrl = pullRequest.HeadRepo.CloneUrl;
            string remoteRef = pullRequest.HeadRef;
            IHostedRemote? existingRemote = _hostedRemotes?.FirstOrDefault(
                remote => string.Equals(remote.Name, remoteName, StringComparison.Ordinal));
            if (existingRemote is not null)
            {
                IHostedRepository hostedRepository;
                try
                {
                    hostedRepository = existingRemote.GetHostedRepository();
                }
                catch (Exception ex)
                {
                    MessageBoxes.Show(
                        this,
                        string.Format(
                            TranslatedStrings.RemoteInError,
                            ex.Message,
                            existingRemote.DisplayData),
                        _strRemoteIgnore.Text,
                        WinFormsShims.MessageBoxButtons.OK,
                        WinFormsShims.MessageBoxIcon.Error);
                    return;
                }

                hostedRepository.CloneProtocol = _cloneGitProtocol;
                if (!string.Equals(hostedRepository.CloneUrl, remoteUrl, StringComparison.Ordinal))
                {
                    MessageBoxes.Show(
                        this,
                        string.Format(_strRemoteAlreadyExist.Text, remoteName, hostedRepository.CloneUrl, remoteUrl),
                        TranslatedStrings.Error,
                        WinFormsShims.MessageBoxButtons.OK,
                        WinFormsShims.MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                string error = Module.AddRemote(remoteName, remoteUrl);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBoxes.Show(
                        this,
                        error,
                        string.Format(_strCouldNotAddRemote.Text, remoteName, remoteUrl),
                        WinFormsShims.MessageBoxButtons.OK,
                        WinFormsShims.MessageBoxIcon.Error);
                    return;
                }

                UICommands.RepoChangedNotifier.Notify();
            }

            ArgumentString fetchCommand = Module.FetchCmd(
                remoteName,
                remoteRef,
                $"{remoteName}/{remoteRef}",
                fetchTags: false);

            // Avalonia routes the modal Git process through the host command boundary.
            if (!UICommands.StartGitCommandProcessDialog(this, fetchCommand))
            {
                return;
            }

            UICommands.RepoChangedNotifier.Notify();
            ArgumentString checkoutCommand = Commands.Checkout(
                $"{remoteName}/{remoteRef}",
                LocalChangesAction.DontChange);
            if (UICommands.StartGitCommandProcessDialog(this, checkoutCommand))
            {
                UICommands.RepoChangedNotifier.Notify();
            }
        }
        finally
        {
            UICommands.RepoChangedNotifier.UnLock(false);
        }

        Close();
    }

    private void _fileStatusList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        GitItemStatus? item = _fileStatusList.SelectedItem?.Item;
        if (item is null || _diffCache is null || !_diffCache.TryGetValue(item.Name, out string? patch))
        {
            return;
        }

        if (item.IsSubmodule)
        {
            _diffViewer.ViewText(item.Name, patch);
        }
        else
        {
            _diffViewer.ViewFixedPatch(item.Name, patch);
        }
    }

    private void _closePullRequestBtn_Click(object? sender, EventArgs e)
    {
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        _closePullRequestBtn.IsEnabled = false;
        CancellationToken cancellationToken = _pullRequestsSequence.Next();
        _loader.FileAndForget(() => ClosePullRequestAsync(pullRequest, cancellationToken));
    }

    private async Task ClosePullRequestAsync(
        IPullRequestInformation pullRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(pullRequest.Close, cancellationToken);
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _selectedOwner_SelectedIndexChanged(this, EventArgs.Empty);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _loader.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                _closePullRequestBtn.IsEnabled = true;
                MessageBoxes.Show(
                    this,
                    _strFailedToClosePullRequest.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void ResetDetails(bool clearPullRequest = true)
    {
        if (clearPullRequest)
        {
            _currentPullRequestInfo = null;
        }

        _diffCache = null;
        _discussionWB.ItemsSource = Array.Empty<DiscussionRow>();
        _fileStatusList.ClearDiffs();
        _diffViewer.ViewText(string.Empty, string.Empty);
    }

    private Control CreatePullRequestRow(PullRequestRow? row, Avalonia.Controls.INameScope nameScope)
    {
        PullRequestRow item = row ?? PullRequestRow.Placeholder(string.Empty);
        return new Grid
        {
            ColumnDefinitions = WinFormsListViewColumnSizer.CreateColumns(_pullRequestColumnWidths, fillColumn: 1),
            Margin = new Avalonia.Thickness(0, 0, 4, 0),
            Children =
            {
                CreateCell(item.Id, 0, TextAlignment.Right),
                CreateCell(item.Title, 1),
                CreateCell(item.Owner, 2),
                CreateCell(item.Created, 3),
                CreateCell(item.Branch, 4),
            },
        };
    }

    private void ResizeColumns(IReadOnlyList<PullRequestRow> rows)
    {
        string[] headers =
        [
            columnHeaderId.Text,
            columnHeaderHeading.Text,
            columnHeaderBy.Text,
            columnHeaderCreated.Text,
            columnHeaderBranch.Text,
        ];
        _pullRequestColumnsSizedToContent = rows.Count > 0;
        for (int columnIndex = 0; columnIndex < _pullRequestColumnWidths.Length; columnIndex++)
        {
            bool sizeToHeader = rows.Count == 0;
            IEnumerable<string?> values = sizeToHeader
                ? [headers[columnIndex]]
                : rows.Select(row => columnIndex switch
                {
                    0 => row.Id,
                    1 => row.Title,
                    2 => row.Owner,
                    3 => row.Created,
                    4 => row.Branch,
                    _ => string.Empty,
                });

            // Framework constraint: native dark/custom ListView headers reserve two additional text pixels.
            _pullRequestColumnWidths[columnIndex] = WinFormsListViewColumnSizer.MeasureAutoSizedColumn(
                _pullRequestsList,
                values,
                sizeToHeader,
                firstColumn: columnIndex == 0,
                headerPadding: ActualThemeVariant == Avalonia.Styling.ThemeVariant.Light ? 12 : 14);
        }

        Grid header = (Grid)(columnHeaderId.Parent
            ?? throw new InvalidOperationException("The pull-request header is not attached to its column grid."));
        int fillColumn = _pullRequestColumnsSizedToContent ? 1 : 4;
        header.ColumnDefinitions = WinFormsListViewColumnSizer.CreateColumns(_pullRequestColumnWidths, fillColumn);
    }

    private static Control CreateDiscussionRow(DiscussionRow? row, Avalonia.Controls.INameScope nameScope)
    {
        DiscussionRow item = row ?? DiscussionRow.Placeholder(string.Empty);
        StackPanel content = new();
        if (!string.IsNullOrEmpty(item.Author) || !string.IsNullOrEmpty(item.Created))
        {
            TextBlock author = new()
            {
                FontWeight = FontWeight.SemiBold,
                Text = item.Author,
            };
            if (item.Commit is null)
            {
                author[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("GitExtensionsHighlightBackgroundBrush");
            }
            else
            {
                author.Foreground = Brushes.Red;
            }

            TextBlock created = new()
            {
                Text = item.Created,
                TextAlignment = TextAlignment.Right,
            };
            Grid.SetColumn(created, 1);
            Grid headingContent = new()
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children = { author, created },
            };
            if (!string.IsNullOrEmpty(item.Commit))
            {
                TextBlock commit = new()
                {
                    FontSize = 7d * 96 / 72,
                    Text = $"Commit:  {item.Commit}",
                };
                Grid.SetColumnSpan(commit, 2);
                Grid.SetRow(commit, 1);
                headingContent.RowDefinitions = new RowDefinitions("Auto,Auto");
                headingContent.Children.Add(commit);
            }

            Border heading = new()
            {
                BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
                Child = headingContent,
            };
            heading[!Border.BackgroundProperty] = new DynamicResourceExtension("GitExtensionsKnownColorControlLightBrush");
            heading[!Border.BorderBrushProperty] = new DynamicResourceExtension("GitExtensionsControlForegroundBrush");
            content.Children.Add(heading);
        }

        content.Children.Add(new TextBlock
        {
            Text = item.Body,
            TextWrapping = TextWrapping.Wrap,
        });

        return new Border
        {
            BorderThickness = new Avalonia.Thickness(0),
            Padding = new Avalonia.Thickness(0),
            Child = content,
        };
    }

    private static TextBlock CreateCell(
        string text,
        int column,
        TextAlignment alignment = TextAlignment.Left)
    {
        TextBlock cell = new()
        {
            Margin = new Avalonia.Thickness(6, 3),
            Text = text,
            TextAlignment = alignment,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(cell, column);
        return cell;
    }

    // parity-scaffolding: Exposes repository-host state and actions to the cross-platform parity suite.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ViewPullRequestsForm form)
    {
        public ComboBox HostedRepositories => form._selectHostedRepoCB;

        public ListBox PullRequests => form._pullRequestsList;

        public ListBox Discussion => form._discussionWB;

        public IReadOnlyList<GitItemStatus> DiffItems => form._fileStatusList.GitItemStatuses;

        public IReadOnlyList<string> PullRequestTitles
            => form._pullRequestsList.Items
                .Cast<PullRequestRow>()
                .Where(row => row.PullRequest is not null)
                .Select(row => row.Title)
                .ToArray();

        public IReadOnlyList<string> PullRequestDisplayTitles
            => form._pullRequestsList.Items
                .Cast<PullRequestRow>()
                .Select(row => row.Title)
                .ToArray();

        public bool HostedRepositorySelectionEnabled => form._selectHostedRepoCB.IsEnabled;

        public bool IsFirstLoad => form._isFirstLoad;

        // parity-scaffolding: Lets the paired capture wait for the selected patch, not just its file row.
        public string DiffText => form._diffViewer.TextEditor.Text;

        public bool CloseEnabled => form._closePullRequestBtn.IsEnabled;

        public bool PostEnabled => form._postComment.IsEnabled;

        public bool RefreshEnabled => form._refreshCommentsBtn.IsEnabled;

        public bool FetchEnabled => form._fetchBtn.IsEnabled;

        public bool AddAndFetchEnabled => form._addAndFetchBtn.IsEnabled;

        public Task LoadPullRequestsAsync(CancellationToken cancellationToken = default)
            => form.LoadPullRequestsAsync(cancellationToken);

        public Task LoadDiffAsync(CancellationToken cancellationToken = default)
            => form._currentPullRequestInfo is { } pullRequest
                ? form.LoadDiffPatchAsync(pullRequest, cancellationToken)
                : Task.CompletedTask;

        public Task JoinOperationsAsync(CancellationToken cancellationToken = default)
            => form._loader.JoinPendingOperationsAsync(cancellationToken);

        public Task InitializeAsync(CancellationToken cancellationToken = default)
            => form.InitializeAsync(cancellationToken);

        public void ClosePullRequest() => form._closePullRequestBtn_Click(form._closePullRequestBtn, EventArgs.Empty);

        public void FetchPullRequest() => form._fetchBtn_Click(form._fetchBtn, EventArgs.Empty);

        public void AddRemoteFetchAndCheckout()
            => form._addAsRemoteAndFetch_Click(form._addAndFetchBtn, EventArgs.Empty);

        public void SelectPullRequest(IPullRequestInformation pullRequest)
        {
            PullRequestRow row = PullRequestRow.FromPullRequest(pullRequest);
            form._pullRequestsList.ItemsSource = new[] { row };
            form._pullRequestsList.SelectedItem = row;
            form._pullRequestsList_SelectedIndexChanged(form._pullRequestsList, EventArgs.Empty);
        }

        public void SelectHostedRepository(int index)
            => form._selectHostedRepoCB.SelectedIndex = index;

        public void ClearHostedRepositorySelection()
        {
            form._selectHostedRepoCB.SelectedItem = null;
            form._selectedOwner_SelectedIndexChanged(form._selectHostedRepoCB, EventArgs.Empty);
        }

        public void ClearPullRequestSelection()
        {
            form._pullRequestsList.SelectedItem = null;
            form._pullRequestsList_SelectedIndexChanged(form._pullRequestsList, EventArgs.Empty);
        }

        public static IReadOnlyList<GitItemStatus> ParseDiffForTesting(
            string diff,
            string baseSha,
            string headSha)
            => ViewPullRequestsForm.ParseDiff(diff, baseSha, headSha).Items;
    }

    private sealed record HostedRemoteRow(
        IHostedRemote Remote,
        IHostedRepository? Repository,
        Exception? Error)
    {
        public string? Name => Remote.Name;

        public string DisplayData => Remote.DisplayData;

        public static HostedRemoteRow Create(IHostedRemote remote)
        {
            try
            {
                // Do this now because repository loading belongs in the asynchronous part.
                return new HostedRemoteRow(remote, remote.GetHostedRepository(), null);
            }
            catch (Exception ex)
            {
                return new HostedRemoteRow(remote, null, ex);
            }
        }

        public override string ToString() => DisplayData;
    }

    private sealed record PullRequestRow(
        IPullRequestInformation? PullRequest,
        string Id,
        string Title,
        string Owner,
        string Created,
        string Branch)
    {
        public static PullRequestRow Placeholder(string text)
            => new(null, string.Empty, text, string.Empty, string.Empty, string.Empty);

        public static PullRequestRow FromPullRequest(IPullRequestInformation pullRequest)
            => new(
                pullRequest,
                pullRequest.Id,
                pullRequest.Title,
                pullRequest.Owner,
                pullRequest.Created.ToString(),
                pullRequest.FetchBranch);
    }

    private sealed record DiscussionRow(string Author, string Created, string Body, string? Commit)
    {
        public static DiscussionRow Placeholder(string text) => new(string.Empty, string.Empty, text, null);

        public static DiscussionRow FromPresentation(DiscussionHtmlCreator.DiscussionEntryPresentation entry)
            => new(
                entry.Author,
                entry.Created,
                entry.Body,
                entry.Commit);
    }

    private sealed record DiffSnapshot(
        GitRevision? BaseRevision,
        GitRevision HeadRevision,
        IReadOnlyList<GitItemStatus> Items,
        Dictionary<string, string> Patches);
}

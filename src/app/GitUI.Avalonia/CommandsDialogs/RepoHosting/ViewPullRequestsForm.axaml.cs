using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.RepoHosting;

public partial class ViewPullRequestsForm : GitModuleForm
{
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

    private readonly IRepositoryHostPlugin? _gitHoster;
    private readonly TaskManager _operations = ThreadHelper.CreateTaskManager();
    private readonly CancellationTokenSequence _pullRequestsSequence = new();
    private readonly CancellationTokenSequence _detailsSequence = new();
    private readonly CancellationTokenSequence _discussionSequence = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();

    private GitProtocol _cloneGitProtocol;
    private IPullRequestInformation? _currentPullRequestInfo;
    private IPullRequestDiscussion? _currentDiscussion;
    private Dictionary<string, string> _diffCache = [];
    private IReadOnlyList<HostedRemoteRow> _hostedRemotes = [];
    private bool _isFirstLoad;
    private IReadOnlyList<IPullRequestInformation> _pullRequestsInfo = [];

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
        _pullRequestsList.ItemTemplate = new FuncDataTemplate<PullRequestRow>(
            CreatePullRequestRow,
            supportsRecycling: false);
        _discussionWB.ItemTemplate = new FuncDataTemplate<DiscussionRow>(
            CreateDiscussionRow,
            supportsRecycling: false);

        _selectHostedRepoCB.SelectionChanged += _selectedOwner_SelectedIndexChanged;
        _pullRequestsList.SelectionChanged += _pullRequestsList_SelectedIndexChanged;
        _fileStatusList.SelectedIndexChanged += _fileStatusList_SelectedIndexChanged;
        _diffViewer.ExtraDiffArgumentsChanged += _fileStatusList_SelectedIndexChanged;
        _diffViewer.TopScrollReached += FileViewer_TopScrollReached;
        _diffViewer.BottomScrollReached += FileViewer_BottomScrollReached;
        _fetchBtn.Click += _fetchBtn_Click;
        _addAndFetchBtn.Click += _addAsRemoteAndFetch_Click;
        _closePullRequestBtn.Click += _closePullRequestBtn_Click;
        _refreshCommentsBtn.Click += (_, _) => StartDiscussionRefresh();
        _postComment.Click += (_, _) => StartPostComment();

        SetActionState();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        if (_gitHoster is null)
        {
            return;
        }

        _operations.FileAndForget(() => InitializeAsync(_lifetimeCancellation.Token));
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifetimeCancellation.Cancel();
        _pullRequestsSequence.CancelCurrent();
        _detailsSequence.CancelCurrent();
        _discussionSequence.CancelCurrent();
        _operations.JoinPendingOperations();
        _pullRequestsSequence.Dispose();
        _detailsSequence.Dispose();
        _discussionSequence.Dispose();
        _lifetimeCancellation.Dispose();
        base.OnClosed(e);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string currentRemote = await Task.Run(Module.GetCurrentRemote, cancellationToken);
        IReadOnlyList<Remote> remotes = await Module.GetRemotesAsync().WaitAsync(cancellationToken);
        HostedRemoteRow[] hostedRemotes = await Task.Run(
            () => GetGitHoster().GetHostedRemotesForModule()
                .Select(HostedRemoteRow.Create)
                .ToArray(),
            cancellationToken);

        Remote? selectedRemote = remotes.FirstOrDefault(
            remote => string.IsNullOrEmpty(currentRemote)
                || string.Equals(remote.Name, currentRemote, StringComparison.OrdinalIgnoreCase));
        _cloneGitProtocol = selectedRemote is Remote currentGitRemote
            && !string.IsNullOrEmpty(currentGitRemote.FetchUrl)
            && currentGitRemote.FetchUrl.IsUrlUsingHttp()
            ? GitProtocol.Https
            : GitProtocol.Ssh;

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        _hostedRemotes = hostedRemotes;
        _isFirstLoad = true;
        _selectHostedRepoCB.ItemsSource = hostedRemotes;

        foreach (HostedRemoteRow remote in hostedRemotes.Where(remote => remote.Error is not null))
        {
            MessageBoxes.Show(
                this,
                string.Format(TranslatedStrings.RemoteInError, remote.Error!.Message, remote.DisplayData),
                _strRemoteIgnore.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
        }

        int selectedIndex = Array.FindIndex(
            hostedRemotes,
            remote => string.Equals(remote.Name, currentRemote, StringComparison.OrdinalIgnoreCase));
        _selectHostedRepoCB.SelectedIndex = selectedIndex >= 0 ? selectedIndex : hostedRemotes.Length > 0 ? 0 : -1;
    }

    private void StartPullRequestLoad()
    {
        CancellationToken cancellationToken = _pullRequestsSequence.Next();
        ResetDetails();
        _pullRequestsList.ItemsSource = new[] { PullRequestRow.Placeholder(_strLoading.Text) };
        _selectHostedRepoCB.IsEnabled = false;
        _operations.FileAndForget(() => LoadPullRequestsAsync(cancellationToken));
    }

    private void _selectedOwner_SelectedIndexChanged(object? sender, EventArgs e)
    {
        StartPullRequestLoad();
    }

    private async Task LoadPullRequestsAsync(CancellationToken cancellationToken)
    {
        HostedRemoteRow? selectedRemote = null;
        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        selectedRemote = _selectHostedRepoCB.SelectedItem as HostedRemoteRow;

        if (selectedRemote?.Repository is null)
        {
            _pullRequestsList.ItemsSource = Array.Empty<PullRequestRow>();
            _selectHostedRepoCB.IsEnabled = true;
            SelectNextHostedRepositoryIfFirstLoad();
            return;
        }

        try
        {
            IReadOnlyList<IPullRequestInformation> pullRequests = await Task.Run(
                selectedRemote.Repository.GetPullRequests,
                cancellationToken);
            PullRequestRow[] rows = pullRequests.Select(PullRequestRow.FromPullRequest).ToArray();

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _selectHostedRepoCB.IsEnabled = true;
            if (_isFirstLoad && rows.Length == 0 && SelectNextHostedRepository())
            {
                return;
            }

            _isFirstLoad = false;
            _pullRequestsInfo = pullRequests;
            _pullRequestsList.ItemsSource = rows;
            _pullRequestsList.SelectedIndex = rows.Length > 0 ? 0 : -1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                _selectHostedRepoCB.IsEnabled = true;
                _pullRequestsList.ItemsSource = Array.Empty<PullRequestRow>();
                MessageBoxes.Show(
                    this,
                    _strFailedToFetchPullData.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
                SelectNextHostedRepositoryIfFirstLoad();
            }
        }
    }

    private void SelectNextHostedRepositoryIfFirstLoad()
    {
        if (_isFirstLoad)
        {
            SelectNextHostedRepository();
        }
    }

    private bool SelectNextHostedRepository()
    {
        int nextIndex = _selectHostedRepoCB.SelectedIndex + 1;
        if (nextIndex < 0 || nextIndex >= _hostedRemotes.Count)
        {
            _isFirstLoad = false;
            return false;
        }

        _selectHostedRepoCB.SelectedIndex = nextIndex;
        return true;
    }

    private void StartSelectedPullRequestLoad()
    {
        IPullRequestInformation? previousPullRequest = _currentPullRequestInfo;
        _currentPullRequestInfo = (_pullRequestsList.SelectedItem as PullRequestRow)?.PullRequest;
        if (ReferenceEquals(previousPullRequest, _currentPullRequestInfo))
        {
            return;
        }

        _detailsSequence.CancelCurrent();
        _discussionSequence.CancelCurrent();
        ResetDetails(clearPullRequest: false);
        SetActionState();

        if (_currentPullRequestInfo is null)
        {
            return;
        }

        _currentPullRequestInfo.HeadRepo.CloneProtocol = _cloneGitProtocol;
        CancellationToken cancellationToken = _detailsSequence.Next();
        _operations.FileAndForget(() => LoadDiffPatchAsync(_currentPullRequestInfo, cancellationToken));
        StartDiscussionLoad(forceReload: false);
    }

    private void _pullRequestsList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        StartSelectedPullRequestLoad();
    }

    private async Task LoadDiffPatchAsync(
        IPullRequestInformation pullRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            string content = await pullRequest.GetDiffDataAsync().WaitAsync(cancellationToken);
            DiffSnapshot snapshot = ParseDiff(content, pullRequest.BaseSha, pullRequest.HeadSha);

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
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
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
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
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
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

    private void StartDiscussionLoad(bool forceReload)
    {
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        CancellationToken cancellationToken = _discussionSequence.Next();
        _discussionWB.ItemsSource = new[] { DiscussionRow.Placeholder(_strLoading.Text) };
        _operations.FileAndForget(() => LoadDiscussionAsync(pullRequest, forceReload, cancellationToken));
    }

    private async Task LoadDiscussionAsync(
        IPullRequestInformation pullRequest,
        bool forceReload,
        CancellationToken cancellationToken)
    {
        try
        {
            IPullRequestDiscussion discussion = await Task.Run(
                () =>
                {
                    IPullRequestDiscussion result = pullRequest.GetDiscussion();
                    if (forceReload)
                    {
                        result.ForceReload();
                    }

                    return result;
                },
                cancellationToken);
            DiscussionRow[] rows = discussion.Entries.Select(DiscussionRow.FromEntry).ToArray();

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!ReferenceEquals(_currentPullRequestInfo, pullRequest))
            {
                return;
            }

            _currentDiscussion = discussion;
            _discussionWB.ItemsSource = rows;
            if (rows.Length > 0)
            {
                _discussionWB.ScrollIntoView(rows[^1]);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                _discussionWB.ItemsSource = Array.Empty<DiscussionRow>();
                MessageBoxes.Show(
                    this,
                    _strCouldNotLoadDiscussion.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void StartDiscussionRefresh()
    {
        StartDiscussionLoad(forceReload: true);
    }

    private void StartPostComment()
    {
        string comment = _postCommentText.Text.Trim();
        if (_currentDiscussion is null || comment.Length == 0)
        {
            return;
        }

        CancellationToken cancellationToken = _discussionSequence.Next();
        _postComment.IsEnabled = false;
        _refreshCommentsBtn.IsEnabled = false;
        _operations.FileAndForget(() => PostCommentAsync(_currentDiscussion, comment, cancellationToken));
    }

    private async Task PostCommentAsync(
        IPullRequestDiscussion discussion,
        string comment,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(() => discussion.Post(comment), cancellationToken);
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _postCommentText.Text = string.Empty;
            SetActionState();
            StartDiscussionLoad(forceReload: true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                SetActionState();
                MessageBoxes.Show(
                    this,
                    _strFailedToLoadDiscussionItem.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void StartClosePullRequest()
    {
        if (_currentPullRequestInfo is not { } pullRequest)
        {
            return;
        }

        _closePullRequestBtn.IsEnabled = false;
        CancellationToken cancellationToken = _pullRequestsSequence.Next();
        _operations.FileAndForget(() => ClosePullRequestAsync(pullRequest, cancellationToken));
    }

    private void _closePullRequestBtn_Click(object? sender, EventArgs e)
    {
        StartClosePullRequest();
    }

    private async Task ClosePullRequestAsync(
        IPullRequestInformation pullRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(pullRequest.Close, cancellationToken);
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            StartPullRequestLoad();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
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

    private void _fetchBtn_Click(object? sender, EventArgs e)
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
        if (!FormRemoteProcess.ShowDialog(this, UICommands, command))
        {
            return;
        }

        UICommands.RepoChangedNotifier.Notify();
        Close();
    }

    private void _addAsRemoteAndFetch_Click(object? sender, EventArgs e)
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
            HostedRemoteRow? existingRemote = _hostedRemotes.FirstOrDefault(
                remote => string.Equals(remote.Name, remoteName, StringComparison.Ordinal));
            if (existingRemote is not null)
            {
                if (existingRemote.Repository is not { } hostedRepository)
                {
                    MessageBoxes.Show(
                        this,
                        string.Format(
                            TranslatedStrings.RemoteInError,
                            existingRemote.Error?.Message ?? _strRemoteIgnore.Text,
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
            if (!FormRemoteProcess.ShowDialog(this, UICommands, fetchCommand))
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
        GitItemStatus? item = _fileStatusList.SelectedItem;
        if (item is null || !_diffCache.TryGetValue(item.Name, out string? patch))
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

    private void ResetDetails(bool clearPullRequest = true)
    {
        if (clearPullRequest)
        {
            _currentPullRequestInfo = null;
        }

        _currentDiscussion = null;
        _diffCache = [];
        _discussionWB.ItemsSource = Array.Empty<DiscussionRow>();
        _fileStatusList.ClearDiffs();
        _diffViewer.ViewText(string.Empty, string.Empty);
        SetActionState();
    }

    private void SetActionState()
    {
        bool hasPullRequest = _currentPullRequestInfo is not null;
        _fetchBtn.IsEnabled = hasPullRequest;
        _addAndFetchBtn.IsEnabled = hasPullRequest;
        _closePullRequestBtn.IsEnabled = hasPullRequest;
        _refreshCommentsBtn.IsEnabled = hasPullRequest;
        _postComment.IsEnabled = hasPullRequest;
    }

    private static Control CreatePullRequestRow(PullRequestRow? row, Avalonia.Controls.INameScope nameScope)
    {
        PullRequestRow item = row ?? PullRequestRow.Placeholder(string.Empty);
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("42,*,90,150,170"),
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

    private static Control CreateDiscussionRow(DiscussionRow? row, Avalonia.Controls.INameScope nameScope)
    {
        DiscussionRow item = row ?? DiscussionRow.Placeholder(string.Empty);
        StackPanel content = new() { Spacing = 3 };
        if (!string.IsNullOrEmpty(item.AuthorAndDate))
        {
            content.Children.Add(new TextBlock
            {
                FontWeight = FontWeight.SemiBold,
                Text = item.AuthorAndDate,
            });
        }

        content.Children.Add(new TextBlock
        {
            Text = item.Body,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrEmpty(item.Commit))
        {
            content.Children.Add(new TextBlock
            {
                Opacity = 0.7,
                Text = item.Commit,
            });
        }

        return new Border
        {
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
            Padding = new Avalonia.Thickness(6),
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

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(columnHeaderHeading), "Heading");
        AddHeaderTranslationItem(translation, nameof(columnHeaderBy), "By");
        AddHeaderTranslationItem(translation, nameof(columnHeaderCreated), "Created");
        AddHeaderTranslationItem(translation, nameof(columnHeaderBranch), "Will be fetched to branch");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, columnHeaderHeading, nameof(columnHeaderHeading), "Heading");
        TranslateHeader(translation, columnHeaderBy, nameof(columnHeaderBy), "By");
        TranslateHeader(translation, columnHeaderCreated, nameof(columnHeaderCreated), "Created");
        TranslateHeader(translation, columnHeaderBranch, nameof(columnHeaderBranch), "Will be fetched to branch");
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
        => translation.AddTranslationItem(nameof(ViewPullRequestsForm), fieldName, "Text", text);

    private static void TranslateHeader(
        ITranslation translation,
        Border header,
        string fieldName,
        string defaultText)
    {
        string? translated = translation.TranslateItem(
            nameof(ViewPullRequestsForm),
            fieldName,
            "Text",
            () => defaultText);
        if (!string.IsNullOrEmpty(translated) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = translated;
        }
    }

    private IRepositoryHostPlugin GetGitHoster()
        => _gitHoster ?? throw new InvalidOperationException($"{nameof(ViewPullRequestsForm)} was constructed incorrectly.");

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ViewPullRequestsForm form)
    {
        public ComboBox HostedRepositories => form._selectHostedRepoCB;

        public ListBox PullRequests => form._pullRequestsList;

        public ListBox Discussion => form._discussionWB;

        public IReadOnlyList<GitItemStatus> DiffItems => form._fileStatusList.GitItemStatuses;

        public string Comment
        {
            get => form._postCommentText.Text;
            set => form._postCommentText.Text = value;
        }

        public Task LoadPullRequestsAsync(CancellationToken cancellationToken = default)
            => form.LoadPullRequestsAsync(cancellationToken);

        public Task LoadDiscussionAsync(bool forceReload, CancellationToken cancellationToken = default)
            => form._currentPullRequestInfo is { } pullRequest
                ? form.LoadDiscussionAsync(pullRequest, forceReload, cancellationToken)
                : Task.CompletedTask;

        public Task LoadDiffAsync(CancellationToken cancellationToken = default)
            => form._currentPullRequestInfo is { } pullRequest
                ? form.LoadDiffPatchAsync(pullRequest, cancellationToken)
                : Task.CompletedTask;

        public Task JoinOperationsAsync(CancellationToken cancellationToken = default)
            => form._operations.JoinPendingOperationsAsync(cancellationToken);

        public Task InitializeAsync(CancellationToken cancellationToken = default)
            => form.InitializeAsync(cancellationToken);

        public void PostComment() => form.StartPostComment();

        public void RefreshDiscussion() => form.StartDiscussionRefresh();

        public void SelectPullRequest(IPullRequestInformation pullRequest)
        {
            PullRequestRow row = PullRequestRow.FromPullRequest(pullRequest);
            form._pullRequestsList.ItemsSource = new[] { row };
            form._pullRequestsList.SelectedItem = row;
            form.StartSelectedPullRequestLoad();
        }

        public static IReadOnlyList<GitItemStatus> ParseDiff(
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

    private sealed record DiscussionRow(string AuthorAndDate, string Body, string? Commit)
    {
        public static DiscussionRow Placeholder(string text) => new(string.Empty, text, null);

        public static DiscussionRow FromEntry(IDiscussionEntry entry)
            => new(
                string.IsNullOrEmpty(entry.Author)
                    ? entry.Created.ToString()
                    : $"{entry.Author} — {entry.Created}",
                entry.Body ?? string.Empty,
                entry is ICommitDiscussionEntry commitEntry && !string.IsNullOrEmpty(commitEntry.Sha)
                    ? commitEntry.Sha
                    : null);
    }

    private sealed record DiffSnapshot(
        GitRevision? BaseRevision,
        GitRevision HeadRevision,
        IReadOnlyList<GitItemStatus> Items,
        Dictionary<string, string> Patches);
}

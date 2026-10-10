using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.Compat;
using ResourceManager;
using ColumnHeader = GitUI.Compat.WinFormsControls.ColumnHeader;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.RepoHosting;

public partial class ForkAndCloneForm : GitExtensionsForm
{
    #region Translation
    private readonly TranslationString _strLoading = new(" : LOADING : ");
    private readonly TranslationString _strFailedToGetRepos = new("Failed to get repositories. This most likely means you didn't configure {0}, please do so via the menu \"Plugins/{0}\".");
    private readonly TranslationString _strWillCloneWithPushAccess = new("Will clone {0} into {1}.\r\nYou will have push access. {2}");
    private readonly TranslationString _strWillCloneInfo = new("Will clone {0} into {1}.\r\nYou can not push unless you are a collaborator. {2}");
    private readonly TranslationString _strWillBeAddedAsARemote = new("\"{0}\" will be added as a remote.");
    private readonly TranslationString _strCouldNotAddRemote = new("Could not add remote");
    private readonly TranslationString _strNoHomepageDefined = new("No homepage defined");
    private readonly TranslationString _strFailedToFork = new("Failed to fork:");
    private readonly TranslationString _strSearchFailed = new("Search failed!");
    private readonly TranslationString _strUserNotFound = new("User not found!");
    private readonly TranslationString _strCouldNotFetchReposOfUser = new("Could not fetch repositories of user!");
    private readonly TranslationString _strSearching = new(" : SEARCHING : ");
    private readonly TranslationString _strSelectOneItem = new("You must select exactly one item");
    private readonly TranslationString _strCloneFolderCanNotBeEmpty = new("Clone folder can not be empty");
    #endregion

    private const string UpstreamRemoteName = "upstream";

    private readonly IGitUICommands _commands = null!;
    private readonly IRepositoryHostPlugin _gitHoster = null!;
    private readonly EventHandler<GitModuleEventArgs>? _gitModuleChanged;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _operations = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly CancellationTokenSequence _myReposSequence = new();
    private readonly CancellationTokenSequence _searchSequence = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private readonly double[] _myRepositoryColumnWidths = [180, 45, 50, 45];
    private readonly double[] _searchResultColumnWidths = [180, 110, 41, 40];

    public ForkAndCloneForm()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public ForkAndCloneForm(
        IGitUICommands commands,
        IRepositoryHostPlugin gitHoster,
        EventHandler<GitModuleEventArgs>? gitModuleChanged)
    {
        _gitModuleChanged = gitModuleChanged;
        _commands = commands;
        _gitHoster = gitHoster;
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        _NO_TRANSLATE_closeBtn.Content = TranslatedStrings.Close;
        browseForCloneToDirbtn.PathShowingControl = createDirTB;
        myReposLV.ItemTemplate = new FuncDataTemplate<HostedRepositoryRow>(
            (row, _) => CreateRepositoryRow(row, isSearchResult: false),
            supportsRecycling: false);
        searchResultsLV.ItemTemplate = new FuncDataTemplate<HostedRepositoryRow>(
            (row, _) => CreateRepositoryRow(row, isSearchResult: true),
            supportsRecycling: false);
        ApplyRepositoryColumnWidths(isSearchResult: false);
        ApplyRepositoryColumnWidths(isSearchResult: true);
        ConfigureColumnSizing(columnHeaderMyReposName, myReposLV, isSearchResult: false, columnIndex: 0);
        ConfigureColumnSizing(columnHeaderSearchName, searchResultsLV, isSearchResult: true, columnIndex: 0);
        ConfigureColumnSizing(columnHeaderSearchOwner, searchResultsLV, isSearchResult: true, columnIndex: 1);
        WinFormsTableLayoutSizer.AttachColumns(tableLayoutPanel5, firstColumnPercent: 70, totalPercent: 100);
        WinFormsTableLayoutSizer.AttachColumns(tableLayoutPanel3, firstColumnPercent: 60, totalPercent: 100);
        WinFormsAutoSizeTextBlock.Attach(orLbl);
        WinFormsAutoSizeTextBlock.Attach(descriptionLbl);

        searchBtn.Click += _searchBtn_Click;
        getFromUserBtn.Click += _getFromUserBtn_Click;
        forkBtn.Click += _forkBtn_Click;
        openGitupPageBtn.Click += _openGitupPageBtn_Click;
        browseForCloneToDirbtn.Click += _browseForCloneToDirbtn_Click;
        cloneBtn.Click += _cloneBtn_Click;
        _NO_TRANSLATE_closeBtn.Click += _closeBtn_Click;
        myReposLV.SelectionChanged += _myReposLV_SelectedIndexChanged;
        searchResultsLV.SelectionChanged += _searchResultsLV_SelectedIndexChanged;
        tabControl.SelectionChanged += _tabControl_SelectedIndexChanged;
        destinationTB.TextChanged += _destinationTB_TextChanged;
        createDirTB.TextChanged += _createDirTB_TextChanged;
        addUpstreamRemoteAsCB.TextChanged += _addRemoteAsTB_TextChanged;
        ProtocolDropdownList.SelectionChanged += ProtocolSelectionChanged;
        searchTB.GotFocus += _searchTB_Enter;
        searchTB.LostFocus += _searchTB_Leave;
        destinationTB.Validating += _destinationTB_Validating;
        createDirTB.Validating += _createDirTB_Validating;

        cloneBtn.IsEnabled = false;
        SetProtocolSelectionVisibility(false);

        void ConfigureColumnSizing(
            ColumnHeader column,
            ListBox list,
            bool isSearchResult,
            int columnIndex)
        {
            column.ResizeToFitContentAction = () =>
            {
                HostedRepositoryRow[] rows = list.ItemsSource?.OfType<HostedRepositoryRow>().ToArray() ?? [];
                string header = column.Text;
                int resizeStrategy = rows.Length == 0 ? ResizeOnHeader : ResizeOnContent;
                IEnumerable<string?> values = resizeStrategy == ResizeOnHeader
                    ? [header]
                    : rows.Select(row => columnIndex switch
                    {
                        0 => row.Name,
                        1 when isSearchResult => row.Owner,
                        _ => string.Empty,
                    });
                double[] widths = isSearchResult ? _searchResultColumnWidths : _myRepositoryColumnWidths;
                widths[columnIndex] = WinFormsListViewColumnSizer.Measure(list, values);
                ApplyRepositoryColumnWidths(isSearchResult);
            };
        }
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        ForkAndCloneForm_Load(this, e);

        // Framework constraint: WinForms activates the first eligible control by tab order.
        tabControl.Focus();
    }

    private void ForkAndCloneForm_Load(object sender, EventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        Init();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="e">The window-closed event data.</param>
    protected override void OnClosed(EventArgs e)
    {
        _lifetimeCancellation.Cancel();
        _myReposSequence.CancelCurrent();
        _searchSequence.CancelCurrent();
        _operations.JoinPendingOperations();
        _myReposSequence.Dispose();
        _searchSequence.Dispose();
        _lifetimeCancellation.Dispose();
        base.OnClosed(e);
    }

    private void Init()
    {
        // Framework constraint: portable settings history is asynchronous, so the source-shaped initializer owns the task boundary.
        _operations.FileAndForget(() => InitializeAsync(_lifetimeCancellation.Token));
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string? destination = AppSettings.DefaultCloneDestinationPath;
        if (string.IsNullOrEmpty(destination))
        {
            IList<Repository> history = await RepositoryHistoryManager.Locals
                .LoadRecentHistoryAsync()
                .WaitAsync(cancellationToken);
            string? lastPath = history.Count > 0 ? history[0].Path : null;
            if (!string.IsNullOrEmpty(lastPath))
            {
                destination = Path.GetDirectoryName(lastPath.Trim('/', '\\'));
            }
        }

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        destinationTB.Text = destination ?? string.Empty;
        Title = $"{_gitHoster.Name}: {Title}";
        UpdateCloneInfo();
        UpdateMyRepos();
    }

    private void UpdateMyRepos()
    {
        CancellationToken cancellationToken = _myReposSequence.Next();
        myReposLV.ItemsSource = new[] { HostedRepositoryRow.Placeholder(_strLoading.Text) };
        _operations.FileAndForget(() => LoadMyReposAsync(cancellationToken));
    }

    private const int ResizeOnContent = -1;
    private const int ResizeOnHeader = -2;

    private async Task LoadMyReposAsync(CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<IHostedRepository> repositories = await Task.Run(
                _gitHoster.GetMyRepos,
                cancellationToken);
            HostedRepositoryRow[] rows = repositories
                .OrderBy(repository => repository.Name)
                .Select(HostedRepositoryRow.FromRepository)
                .ToArray();

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            myReposLV.ItemsSource = rows;
            ResizeColumnToFitContent(columnHeaderMyReposName);
            myReposLV.SelectedIndex = -1;
            UpdateCloneInfo();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                myReposLV.ItemsSource = Array.Empty<HostedRepositoryRow>();
                TextBlock helpText = (TextBlock)helpTextLbl.Content!;
                helpText.Text = string.Format(_strFailedToGetRepos.Text, _gitHoster.Name)
                    + Environment.NewLine + Environment.NewLine
                    + "Exception: " + ex.Message
                    + Environment.NewLine + Environment.NewLine
                    + helpText.Text;
            }
        }
    }

    private static void ResizeColumnToFitContent(ColumnHeader column)
        => column.ResizeToFitContent();

    #region GUI Handlers

    private void _searchBtn_Click(object sender, EventArgs e)
    {
        string search = searchTB.Text ?? string.Empty;
        if (search.Trim().Length == 0)
        {
            return;
        }

        CancellationToken cancellationToken = _searchSequence.Next();
        PrepareSearch(sender, e);
        _operations.FileAndForget(() => SearchAsync(search, byUser: false, cancellationToken));
    }

    private void _getFromUserBtn_Click(object sender, EventArgs e)
    {
        string search = searchTB.Text ?? string.Empty;
        if (search.Trim().Length == 0)
        {
            return;
        }

        CancellationToken cancellationToken = _searchSequence.Next();
        PrepareSearch(sender, e);
        _operations.FileAndForget(() => SearchAsync(search.Trim(), byUser: true, cancellationToken));
    }

    private void PrepareSearch(object sender, EventArgs e)
    {
        searchResultsLV.ItemsSource = Array.Empty<HostedRepositoryRow>();
        _searchResultsLV_SelectedIndexChanged(sender, e);
        searchBtn.IsEnabled = false;
        searchResultsLV.ItemsSource = new[] { HostedRepositoryRow.Placeholder(_strSearching.Text) };
    }

    private async Task SearchAsync(string search, bool byUser, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<IHostedRepository> repositories = await Task.Run(
                () => byUser
                    ? _gitHoster.GetRepositoriesOfUser(search)
                    : _gitHoster.SearchForRepository(search),
                cancellationToken);

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            HandleSearchResult(repositories);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                string message = byUser
                    ? ex.Message.Contains("404", StringComparison.Ordinal)
                        ? _strUserNotFound.Text
                        : _strCouldNotFetchReposOfUser.Text
                    : _strSearchFailed.Text;
                MessageBoxes.Show(
                    this,
                    message + (message == _strUserNotFound.Text ? string.Empty : Environment.NewLine + ex.Message),
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
        finally
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                searchBtn.IsEnabled = true;
            }
        }
    }

    private Control CreateRepositoryRow(HostedRepositoryRow? row, bool isSearchResult)
    {
        Grid grid = new()
        {
            ColumnDefinitions = WinFormsListViewColumnSizer.CreateColumns(
                isSearchResult ? _searchResultColumnWidths : _myRepositoryColumnWidths),
        };
        if (row is null)
        {
            return grid;
        }

        grid.Children.Add(CreateCell(row.Name, 0));
        if (row.Repository is not null)
        {
            grid.Children.Add(CreateCell(
                isSearchResult ? row.Owner : row.IsFork,
                1,
                isSearchResult ? TextAlignment.Left : TextAlignment.Center));
            grid.Children.Add(CreateCell(
                isSearchResult ? row.IsFork : row.Forks,
                2,
                isSearchResult ? TextAlignment.Center : TextAlignment.Right));
            grid.Children.Add(CreateCell(
                isSearchResult ? row.Forks : row.IsPrivate,
                3,
                isSearchResult ? TextAlignment.Right : TextAlignment.Center));
        }

        return grid;

        static TextBlock CreateCell(
            string text,
            int column,
            TextAlignment textAlignment = TextAlignment.Left)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Avalonia.Thickness(6, 2),
                TextAlignment = textAlignment,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private void HandleSearchResult(IReadOnlyList<IHostedRepository> repos)
    {
        HostedRepositoryRow[] rows = repos
            .OrderBy(repository => repository.Name)
            .Select(HostedRepositoryRow.FromRepository)
            .ToArray();
        searchResultsLV.ItemsSource = rows;
        ResizeColumnToFitContent(columnHeaderSearchName);
        ResizeColumnToFitContent(columnHeaderSearchOwner);
    }

    private void ApplyRepositoryColumnWidths(bool isSearchResult)
    {
        ContentControl firstHeader = isSearchResult ? columnHeaderSearchName : columnHeaderMyReposName;
        Grid header = (Grid)(firstHeader.Parent
            ?? throw new InvalidOperationException("The repository list header is not attached to its column grid."));
        header.ColumnDefinitions = WinFormsListViewColumnSizer.CreateColumns(
            isSearchResult ? _searchResultColumnWidths : _myRepositoryColumnWidths);
    }

    private void _forkBtn_Click(object sender, EventArgs e)
    {
        IHostedRepository? repository = (searchResultsLV.SelectedItem as HostedRepositoryRow)?.Repository;
        if (repository is null)
        {
            MessageBoxes.Show(
                this,
                _strSelectOneItem.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        _operations.FileAndForget(() => ForkAsync(repository, _lifetimeCancellation.Token));
    }

    private async Task ForkAsync(IHostedRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(repository.Fork, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    _strFailedToFork.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        tabControl.SelectedItem = myReposPage;
        UpdateMyRepos();
    }

    private void _searchTB_Enter(object sender, EventArgs e)
    {
        AcceptButton = searchBtn;
    }

    private void _searchTB_Leave(object sender, EventArgs e)
    {
        AcceptButton = null;
    }

    private void _searchResultsLV_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCloneInfo();
        IHostedRepository? repository = (searchResultsLV.SelectedItem as HostedRepositoryRow)?.Repository;
        if (repository is null)
        {
            forkBtn.IsEnabled = false;
            return;
        }

        forkBtn.IsEnabled = true;
        searchResultItemDescription.Text = repository.Description;
    }

    private void _browseForCloneToDirbtn_Click(object sender, EventArgs e)
    {
        // Avalonia uses the current filesystem root because the original C:\ fallback is not portable.
        string initialDirectory = string.IsNullOrEmpty(destinationTB.Text)
            ? Path.GetPathRoot(Environment.CurrentDirectory) ?? Environment.CurrentDirectory
            : destinationTB.Text;
        string? selectedPath = OsShellUtil.PickFolder(this, initialDirectory);
        if (selectedPath is not null)
        {
            destinationTB.Text = selectedPath;
        }
    }

    private void _cloneBtn_Click(object sender, EventArgs e)
    {
        if (CurrentySelectedGitRepo is { } repository)
        {
            Clone(repository);
        }
    }

    private void _openGitupPageBtn_Click(object sender, EventArgs e)
    {
        IHostedRepository? repository = CurrentySelectedGitRepo;
        if (repository is null)
        {
            return;
        }

        string homepage = repository.Homepage;
        if (string.IsNullOrEmpty(homepage)
            || (!homepage.StartsWith("http://") && !homepage.StartsWith("https://")))
        {
            MessageBoxes.Show(
                this,
                _strNoHomepageDefined.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        OsShellUtil.OpenUrlInDefaultBrowser(homepage);
    }

    private void _closeBtn_Click(object sender, EventArgs e)
        => DialogResult = WinFormsShims.DialogResult.OK;

    private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCloneInfo();
        if (ReferenceEquals(tabControl.SelectedItem, searchReposPage))
        {
            searchTB.Focus();
        }
    }

    private void _myReposLV_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCloneInfo();
    }

    private void _createDirTB_TextChanged(object sender, EventArgs e)
    {
        UpdateCloneInfo(updateCreateDirTB: false, updateProtocols: false);
    }

    private void _destinationTB_TextChanged(object sender, EventArgs e)
    {
        UpdateCloneInfo(updateCreateDirTB: false, updateProtocols: false);
    }

    private void _addRemoteAsTB_TextChanged(object sender, EventArgs e)
    {
        UpdateCloneInfo(updateCreateDirTB: false, updateProtocols: false);
    }

    #endregion

    private void Clone(IHostedRepository repo)
    {
        string? targetDirectory = GetTargetDir();
        if (targetDirectory is null)
        {
            return;
        }

        IGitUICommands commands = _commands;
        ArgumentString command = Commands.Clone(
            repo.CloneUrl,
            targetDirectory,
            commands.Module.GetPathForGitExecution,
            depth: GetDepth());

        // Avalonia routes the modal Git process through the host command boundary.
        if (!commands.StartGitCommandProcessDialog(this, command))
        {
            return;
        }

        GitModule module = new(commands.GetRequiredService<IGitExecutorProvider>(), targetDirectory);
        string upstreamName = addUpstreamRemoteAsCB.Text?.Trim() ?? string.Empty;
        if (upstreamName.Length > 0 && !string.IsNullOrEmpty(repo.ParentUrl))
        {
            string error = module.AddRemote(upstreamName, repo.ParentUrl);
            if (!string.IsNullOrEmpty(error))
            {
                MessageBoxes.Show(
                    this,
                    error,
                    _strCouldNotAddRemote.Text,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }

        _gitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));
        Close();
    }

    private IHostedRepository? CurrentySelectedGitRepo
        => ReferenceEquals(tabControl.SelectedItem, searchReposPage)
            ? (searchResultsLV.SelectedItem as HostedRepositoryRow)?.Repository
            : (myReposLV.SelectedItem as HostedRepositoryRow)?.Repository;

    private void UpdateCloneInfo(bool updateCreateDirTB = true, bool updateProtocols = true)
    {
        IHostedRepository? repository = CurrentySelectedGitRepo;
        if (repository is null)
        {
            SetProtocolSelectionVisibility(false);
            cloneBtn.IsEnabled = false;
            cloneInfoText.Text = string.Empty;
            createDirTB.Text = string.Empty;
            return;
        }

        IReadOnlyList<GitProtocol> protocols = repository.SupportedCloneProtocols;
        bool hasProtocols = protocols.Count > 0;
        if (hasProtocols && updateProtocols)
        {
            GitProtocol currentSelection = ProtocolDropdownList.SelectedItem is GitProtocol selectedProtocol
                ? selectedProtocol
                : protocols[0];
            ProtocolDropdownList.ItemsSource = protocols;
            if (protocols.Contains(currentSelection))
            {
                repository.CloneProtocol = currentSelection;
            }

            ProtocolDropdownList.SelectedItem = repository.CloneProtocol;
        }

        SetProtocolSelectionVisibility(hasProtocols);
        if (updateCreateDirTB)
        {
            createDirTB.Text = repository.Name;
            addUpstreamRemoteAsCB.ItemsSource = repository.ParentOwner is null
                ? Array.Empty<string>()
                : [repository.ParentOwner, UpstreamRemoteName];
            addUpstreamRemoteAsCB.Text = repository.ParentOwner ?? string.Empty;
            addUpstreamRemoteAsCB.IsEnabled = repository.ParentOwner is not null;
        }

        cloneBtn.IsEnabled = true;
        SetCloneInfoText(repository);
    }

    private void SetCloneInfoText(IHostedRepository repo)
    {
        string upstreamName = addUpstreamRemoteAsCB.Text?.Trim() ?? string.Empty;
        string moreInfo = upstreamName.Length == 0
            ? string.Empty
            : string.Format(_strWillBeAddedAsARemote.Text, upstreamName);
        TranslationString format = ReferenceEquals(tabControl.SelectedItem, searchReposPage)
            ? _strWillCloneInfo
            : _strWillCloneWithPushAccess;
        cloneInfoText.Text = string.Format(
            format.Text,
            repo.CloneUrl,
            GetTargetDir(),
            moreInfo);
    }

    private void SetProtocolSelectionVisibility(bool multipleProtocols)
    {
        ProtocolLabel.IsVisible = multipleProtocols;
        ProtocolDropdownList.IsVisible = multipleProtocols;
    }

    private string? GetTargetDir()
    {
        string destination = destinationTB.Text?.Trim() ?? string.Empty;
        if (destination.Length == 0)
        {
            MessageBoxes.Show(
                this,
                _strCloneFolderCanNotBeEmpty.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return null;
        }

        string directory = createDirTB.Text ?? string.Empty;
        return Path.Combine(destination, directory);
    }

    private int? GetDepth()
        => depthUpDown.Value is > 0 ? (int)depthUpDown.Value.Value : null;

    private void _destinationTB_Validating(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = destinationTB.Text?.IndexOfAny(Delimiters.InvalidPathCharsSearchValues) is >= 0;
    }

    private void _createDirTB_Validating(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = createDirTB.Text?.IndexOfAny(Delimiters.InvalidPathCharsSearchValues) is >= 0;
    }

    private void ProtocolSelectionChanged(object sender, EventArgs e)
    {
        if (CurrentySelectedGitRepo is not { } repository
            || ProtocolDropdownList.SelectedItem is not GitProtocol protocol)
        {
            return;
        }

        repository.CloneProtocol = protocol;
        SetCloneInfoText(repository);
    }

    // parity-scaffolding: Exposes repository-host state and actions to the cross-platform parity suite.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ForkAndCloneForm form)
    {
        public ListBox MyRepositories => form.myReposLV;

        public ListBox SearchResults => form.searchResultsLV;

        public string Destination
        {
            get => form.destinationTB.Text ?? string.Empty;
            set => form.destinationTB.Text = value;
        }

        public string CreateDirectory
        {
            get => form.createDirTB.Text ?? string.Empty;
            set => form.createDirTB.Text = value;
        }

        public string UpstreamRemoteName
        {
            get => form.addUpstreamRemoteAsCB.Text ?? string.Empty;
            set => form.addUpstreamRemoteAsCB.Text = value;
        }

        public string CloneInfo => form.cloneInfoText.Text ?? string.Empty;

        public string Description => form.searchResultItemDescription.Text ?? string.Empty;

        public string HelpText => ((TextBlock)form.helpTextLbl.Content!).Text ?? string.Empty;

        public bool CloneEnabled => form.cloneBtn.IsEnabled;

        public bool ForkEnabled => form.forkBtn.IsEnabled;

        public bool SearchEnabled => form.searchBtn.IsEnabled;

        public bool GetFromUserEnabled => form.getFromUserBtn.IsEnabled;

        public GitProtocol? SelectedProtocol
        {
            get => form.ProtocolDropdownList.SelectedItem is GitProtocol protocol ? protocol : null;
            set => form.ProtocolDropdownList.SelectedItem = value;
        }

        public bool IsMyRepositoriesTabSelected => ReferenceEquals(form.tabControl.SelectedItem, form.myReposPage);

        public IReadOnlyList<string> MyRepositoryNames
            => form.myReposLV.Items.Cast<HostedRepositoryRow>().Select(row => row.Name).ToArray();

        public IReadOnlyList<string> SearchResultNames
            => form.searchResultsLV.Items.Cast<HostedRepositoryRow>().Select(row => row.Name).ToArray();

        public string? TargetDirectory
        {
            get
            {
                string destination = form.destinationTB.Text?.Trim() ?? string.Empty;
                return destination.Length == 0
                    ? null
                    : Path.Combine(destination, form.createDirTB.Text ?? string.Empty);
            }
        }

        public int? Depth => form.GetDepth();

        public void SetDepth(decimal value) => form.depthUpDown.Value = value;

        public void SelectMyRepository(int index) => form.myReposLV.SelectedIndex = index;

        public void SelectSearchResult(int index)
        {
            form.tabControl.SelectedItem = form.searchReposPage;
            form.searchResultsLV.SelectedIndex = index;
        }

        public Task LoadMyRepositoriesAsync(CancellationToken cancellationToken = default)
            => form.LoadMyReposAsync(cancellationToken);

        public Task SearchAsync(string search, bool byUser, CancellationToken cancellationToken = default)
            => form.SearchAsync(search, byUser, cancellationToken);

        public Task JoinOperationsAsync(CancellationToken cancellationToken = default)
            => form._operations.JoinPendingOperationsAsync(cancellationToken);

        public void StartSearch(string search, bool byUser)
        {
            form.searchTB.Text = search;
            if (byUser)
            {
                form._getFromUserBtn_Click(form.getFromUserBtn, EventArgs.Empty);
            }
            else
            {
                form._searchBtn_Click(form.searchBtn, EventArgs.Empty);
            }
        }

        public void ForkSelectedRepository()
            => form._forkBtn_Click(form.forkBtn, EventArgs.Empty);

        public void BrowseForCloneDirectory()
            => form._browseForCloneToDirbtn_Click(form.browseForCloneToDirbtn, EventArgs.Empty);

        public void CloneSelectedRepository()
            => form._cloneBtn_Click(form.cloneBtn, EventArgs.Empty);

        public string? GetTargetDirectoryWithValidation()
            => form.GetTargetDir();

        public (bool Destination, bool CreateDirectory) ValidatePaths()
        {
            System.ComponentModel.CancelEventArgs destinationEvent = new();
            System.ComponentModel.CancelEventArgs createDirectoryEvent = new();
            form._destinationTB_Validating(form.destinationTB, destinationEvent);
            form._createDirTB_Validating(form.createDirTB, createDirectoryEvent);
            return (destinationEvent.Cancel, createDirectoryEvent.Cancel);
        }
    }

    private sealed record HostedRepositoryRow(
        IHostedRepository? Repository,
        string Name,
        string Owner,
        string IsFork,
        string Forks,
        string IsPrivate)
    {
        public static HostedRepositoryRow Placeholder(string text)
            => new(null, text, string.Empty, string.Empty, string.Empty, string.Empty);

        public static HostedRepositoryRow FromRepository(IHostedRepository repository)
            => new(
                repository,
                repository.Name,
                repository.Owner ?? string.Empty,
                repository.IsAFork ? TranslatedStrings.Yes : TranslatedStrings.No,
                repository.Forks.ToString(),
                repository.IsPrivate ? TranslatedStrings.Yes : TranslatedStrings.No);
    }
}

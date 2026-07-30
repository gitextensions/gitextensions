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
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.RepoHosting;

public partial class ForkAndCloneForm : GitExtensionsForm
{
    private const string UpstreamRemoteName = "upstream";

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

    private readonly IGitUICommands? _commands;
    private readonly IRepositoryHostPlugin? _gitHoster;
    private readonly EventHandler<GitModuleEventArgs>? _gitModuleChanged;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _operations = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly CancellationTokenSequence _myReposSequence = new();
    private readonly CancellationTokenSequence _searchSequence = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();

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
        myReposLV.ItemTemplate = new FuncDataTemplate<HostedRepositoryRow>(
            (row, _) => CreateRepositoryRow(row, isSearchResult: false),
            supportsRecycling: false);
        searchResultsLV.ItemTemplate = new FuncDataTemplate<HostedRepositoryRow>(
            (row, _) => CreateRepositoryRow(row, isSearchResult: true),
            supportsRecycling: false);

        searchBtn.Click += (_, _) => StartSearch(SearchKind.Repository);
        getFromUserBtn.Click += (_, _) => StartSearch(SearchKind.User);
        forkBtn.Click += forkBtn_Click;
        openGitupPageBtn.Click += openGitupPageBtn_Click;
        browseForCloneToDirbtn.Click += browseForCloneToDirbtn_Click;
        cloneBtn.Click += cloneBtn_Click;
        _NO_TRANSLATE_closeBtn.Click += closeBtn_Click;
        myReposLV.SelectionChanged += (_, _) => UpdateCloneInfo();
        searchResultsLV.SelectionChanged += searchResultsLV_SelectionChanged;
        tabControl.SelectionChanged += tabControl_SelectionChanged;
        destinationTB.TextChanged += (_, _) => UpdateCloneInfo(updateCreateDir: false, updateProtocols: false);
        createDirTB.TextChanged += (_, _) => UpdateCloneInfo(updateCreateDir: false, updateProtocols: false);
        addUpstreamRemoteAsCB.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                UpdateCloneInfo(updateCreateDir: false, updateProtocols: false);
            }
        };
        ProtocolDropdownList.SelectionChanged += ProtocolSelectionChanged;
        searchTB.GotFocus += (_, _) => AcceptButton = searchBtn;
        searchTB.LostFocus += (_, _) => AcceptButton = null;

        forkBtn.IsEnabled = false;
        cloneBtn.IsEnabled = false;
        SetProtocolSelectionVisibility(false);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        if (_commands is null || _gitHoster is null)
        {
            return;
        }

        _operations.FileAndForget(() => InitializeAsync(_lifetimeCancellation.Token));
    }

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
        Title = $"{GetGitHoster().Name}: {Title}";
        UpdateCloneInfo();
        StartMyReposLoad();
    }

    private void StartMyReposLoad()
    {
        CancellationToken cancellationToken = _myReposSequence.Next();
        myReposLV.ItemsSource = new[] { HostedRepositoryRow.Placeholder(_strLoading.Text) };
        _operations.FileAndForget(() => LoadMyReposAsync(cancellationToken));
    }

    private async Task LoadMyReposAsync(CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<IHostedRepository> repositories = await Task.Run(
                GetGitHoster().GetMyRepos,
                cancellationToken);
            HostedRepositoryRow[] rows = repositories
                .OrderBy(repository => repository.Name)
                .Select(HostedRepositoryRow.FromRepository)
                .ToArray();

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            myReposLV.ItemsSource = rows;
            UpdateCloneInfo();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                myReposLV.ItemsSource = Array.Empty<HostedRepositoryRow>();
                helpTextLbl.Text = string.Format(_strFailedToGetRepos.Text, GetGitHoster().Name)
                    + Environment.NewLine + Environment.NewLine
                    + "Exception: " + ex.Message
                    + Environment.NewLine + Environment.NewLine
                    + helpTextLbl.Text;
            }
        }
    }

    private void StartSearch(SearchKind searchKind)
    {
        string search = searchTB.Text?.Trim() ?? string.Empty;
        if (search.Length == 0)
        {
            return;
        }

        CancellationToken cancellationToken = _searchSequence.Next();
        searchBtn.IsEnabled = false;
        getFromUserBtn.IsEnabled = false;
        searchResultsLV.SelectedItem = null;
        searchResultsLV.ItemsSource = new[] { HostedRepositoryRow.Placeholder(_strSearching.Text) };
        UpdateCloneInfo();
        _operations.FileAndForget(() => SearchAsync(search, searchKind, cancellationToken));
    }

    private async Task SearchAsync(string search, SearchKind searchKind, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<IHostedRepository> repositories = await Task.Run(
                () => searchKind == SearchKind.Repository
                    ? GetGitHoster().SearchForRepository(search)
                    : GetGitHoster().GetRepositoriesOfUser(search),
                cancellationToken);
            HostedRepositoryRow[] rows = repositories
                .OrderBy(repository => repository.Name)
                .Select(HostedRepositoryRow.FromRepository)
                .ToArray();

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            searchResultsLV.ItemsSource = rows;
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
                string message = searchKind == SearchKind.Repository
                    ? _strSearchFailed.Text
                    : ex.Message.Contains("404", StringComparison.Ordinal)
                        ? _strUserNotFound.Text
                        : _strCouldNotFetchReposOfUser.Text;
                MessageBoxes.Show(
                    this,
                    message + (message == _strUserNotFound.Text ? string.Empty : Environment.NewLine + ex.Message),
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
                searchResultsLV.ItemsSource = Array.Empty<HostedRepositoryRow>();
            }
        }
        finally
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                searchBtn.IsEnabled = true;
                getFromUserBtn.IsEnabled = true;
                UpdateCloneInfo();
            }
        }
    }

    private Control CreateRepositoryRow(HostedRepositoryRow? row, bool isSearchResult)
    {
        Grid grid = new()
        {
            ColumnDefinitions = isSearchResult
                ? new ColumnDefinitions("*,110,41,40")
                : new ColumnDefinitions("*,45,50,45"),
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

    private void searchResultsLV_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IHostedRepository? repository = GetSelectedRepository(searchResultsLV);
        searchResultItemDescription.Text = repository?.Description ?? string.Empty;
        forkBtn.IsEnabled = repository is not null;
        UpdateCloneInfo();
    }

    private void tabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateCloneInfo();
        if (ReferenceEquals(tabControl.SelectedItem, searchReposPage))
        {
            searchTB.Focus();
        }
    }

    private void forkBtn_Click(object? sender, EventArgs e)
    {
        IHostedRepository? repository = GetSelectedRepository(searchResultsLV);
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

        forkBtn.IsEnabled = false;
        _operations.FileAndForget(() => ForkAsync(repository, _lifetimeCancellation.Token));
    }

    private async Task ForkAsync(IHostedRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(repository.Fork, cancellationToken);
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            tabControl.SelectedItem = myReposPage;
            StartMyReposLoad();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
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
                forkBtn.IsEnabled = true;
            }
        }
    }

    private void browseForCloneToDirbtn_Click(object? sender, EventArgs e)
    {
        string? selectedPath = OsShellUtil.PickFolder(this, destinationTB.Text);
        if (selectedPath is not null)
        {
            destinationTB.Text = selectedPath;
        }
    }

    private void openGitupPageBtn_Click(object? sender, EventArgs e)
    {
        IHostedRepository? repository = CurrentySelectedGitRepo;
        if (repository is null)
        {
            return;
        }

        string homepage = repository.Homepage;
        if (!Uri.TryCreate(homepage, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
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

    private void cloneBtn_Click(object? sender, EventArgs e)
    {
        if (CurrentySelectedGitRepo is { } repository)
        {
            Clone(repository);
        }
    }

    private void closeBtn_Click(object? sender, EventArgs e)
        => DialogResult = WinFormsShims.DialogResult.OK;

    private void Clone(IHostedRepository repository)
    {
        string? targetDirectory = GetTargetDir();
        if (targetDirectory is null)
        {
            return;
        }

        IGitUICommands commands = GetCommands();
        ArgumentString command = Commands.Clone(
            repository.CloneUrl,
            targetDirectory,
            commands.Module.GetPathForGitExecution,
            depth: GetDepth());
        using FormRemoteProcess formRemoteProcess = new(commands, command)
        {
            Remote = repository.CloneUrl,
        };
        formRemoteProcess.ShowDialog(this);
        if (formRemoteProcess.ErrorOccurred())
        {
            return;
        }

        GitModule module = new(commands.GetRequiredService<IGitExecutorProvider>(), targetDirectory);
        string upstreamName = addUpstreamRemoteAsCB.Text?.Trim() ?? string.Empty;
        if (upstreamName.Length > 0 && !string.IsNullOrEmpty(repository.ParentUrl))
        {
            string error = module.AddRemote(upstreamName, repository.ParentUrl);
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
            ? GetSelectedRepository(searchResultsLV)
            : GetSelectedRepository(myReposLV);

    private static IHostedRepository? GetSelectedRepository(ListBox listBox)
        => (listBox.SelectedItem as HostedRepositoryRow)?.Repository;

    private void UpdateCloneInfo(bool updateCreateDir = true, bool updateProtocols = true)
    {
        IHostedRepository? repository = CurrentySelectedGitRepo;
        if (repository is null)
        {
            SetProtocolSelectionVisibility(false);
            cloneBtn.IsEnabled = false;
            cloneInfoText.Text = string.Empty;
            if (updateCreateDir)
            {
                createDirTB.Text = string.Empty;
            }

            return;
        }

        IReadOnlyList<GitProtocol> protocols = repository.SupportedCloneProtocols;
        bool hasProtocols = protocols.Count > 0;
        if (updateProtocols)
        {
            ProtocolDropdownList.ItemsSource = protocols;
            ProtocolDropdownList.SelectedItem = protocols.Contains(repository.CloneProtocol)
                ? repository.CloneProtocol
                : protocols.FirstOrDefault();
        }

        SetProtocolSelectionVisibility(hasProtocols);
        if (updateCreateDir)
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

    private void SetCloneInfoText(IHostedRepository repository)
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
            repository.CloneUrl,
            GetTargetDir(showError: false),
            moreInfo);
    }

    private void SetProtocolSelectionVisibility(bool multipleProtocols)
    {
        ProtocolLabel.IsVisible = multipleProtocols;
        ProtocolDropdownList.IsVisible = multipleProtocols;
    }

    private string? GetTargetDir(bool showError = true)
    {
        string destination = destinationTB.Text?.Trim() ?? string.Empty;
        if (destination.Length == 0)
        {
            if (showError)
            {
                MessageBoxes.Show(
                    this,
                    _strCloneFolderCanNotBeEmpty.Text,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }

            return null;
        }

        string directory = createDirTB.Text?.Trim() ?? string.Empty;
        return Path.Combine(destination, directory);
    }

    private int? GetDepth()
        => depthUpDown.Value is > 0 ? (int)depthUpDown.Value.Value : null;

    private void ProtocolSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CurrentySelectedGitRepo is not { } repository
            || ProtocolDropdownList.SelectedItem is not GitProtocol protocol)
        {
            return;
        }

        repository.CloneProtocol = protocol;
        SetCloneInfoText(repository);
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(columnHeaderMyReposName), "Name");
        AddHeaderTranslationItem(translation, nameof(columnHeaderMyReposIsFork), "Is fork");
        AddHeaderTranslationItem(translation, nameof(columnHeaderMyReposForks), "# Forks");
        AddHeaderTranslationItem(translation, nameof(columnHeaderMyReposIsPrivate), "Private");
        AddHeaderTranslationItem(translation, nameof(columnHeaderSearchName), "Name");
        AddHeaderTranslationItem(translation, nameof(columnHeaderSearchOwner), "Owner");
        AddHeaderTranslationItem(translation, nameof(columnHeaderSearchIsFork), "Is fork");
        AddHeaderTranslationItem(translation, nameof(columnHeaderSearchForks), "# Forks");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, columnHeaderMyReposName, nameof(columnHeaderMyReposName), "Name");
        TranslateHeader(translation, columnHeaderMyReposIsFork, nameof(columnHeaderMyReposIsFork), "Is fork");
        TranslateHeader(translation, columnHeaderMyReposForks, nameof(columnHeaderMyReposForks), "# Forks");
        TranslateHeader(translation, columnHeaderMyReposIsPrivate, nameof(columnHeaderMyReposIsPrivate), "Private");
        TranslateHeader(translation, columnHeaderSearchName, nameof(columnHeaderSearchName), "Name");
        TranslateHeader(translation, columnHeaderSearchOwner, nameof(columnHeaderSearchOwner), "Owner");
        TranslateHeader(translation, columnHeaderSearchIsFork, nameof(columnHeaderSearchIsFork), "Is fork");
        TranslateHeader(translation, columnHeaderSearchForks, nameof(columnHeaderSearchForks), "# Forks");
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
        => translation.AddTranslationItem(nameof(ForkAndCloneForm), fieldName, "Text", text);

    private static void TranslateHeader(
        ITranslation translation,
        Border header,
        string fieldName,
        string defaultText)
    {
        string? translated = translation.TranslateItem(
            nameof(ForkAndCloneForm),
            fieldName,
            "Text",
            () => defaultText);
        if (!string.IsNullOrEmpty(translated) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = translated;
        }
    }

    private IGitUICommands GetCommands()
        => _commands ?? throw new InvalidOperationException($"{nameof(ForkAndCloneForm)} was constructed incorrectly.");

    private IRepositoryHostPlugin GetGitHoster()
        => _gitHoster ?? throw new InvalidOperationException($"{nameof(ForkAndCloneForm)} was constructed incorrectly.");

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

        public string CloneInfo => form.cloneInfoText.Text ?? string.Empty;

        public string Description => form.searchResultItemDescription.Text ?? string.Empty;

        public bool CloneEnabled => form.cloneBtn.IsEnabled;

        public IReadOnlyList<string> MyRepositoryNames
            => form.myReposLV.Items.Cast<HostedRepositoryRow>().Select(row => row.Name).ToArray();

        public IReadOnlyList<string> SearchResultNames
            => form.searchResultsLV.Items.Cast<HostedRepositoryRow>().Select(row => row.Name).ToArray();

        public string? TargetDirectory => form.GetTargetDir(showError: false);

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
            => form.SearchAsync(search, byUser ? SearchKind.User : SearchKind.Repository, cancellationToken);
    }

    private enum SearchKind
    {
        Repository,
        User,
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

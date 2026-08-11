using System.ComponentModel;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

internal sealed class MultiRepositoryStatusControl : GitExtensionsControl
{
    private static readonly TimeSpan LocalRefreshInterval = TimeSpan.FromMinutes(1);

    private readonly Button _backButton = new() { AutoSize = true, Text = "返回仓库列表" };
    private readonly Button _fetchAllButton = new() { AutoSize = true, Text = "Fetch 全部" };
    private readonly Button _fetchButton = new() { AutoSize = true, Text = "Fetch 选中" };
    private readonly DataGridView _grid = new();
    private readonly FlowLayoutPanel _headerActions = new();
    private readonly Label _operationLabel = new() { AutoSize = true, Margin = new Padding(12, 7, 3, 3) };
    private readonly Button _openButton = new() { AutoSize = true, Text = "打开" };
    private readonly Button _refreshAllButton = new() { AutoSize = true, Text = "刷新全部" };
    private readonly Button _refreshButton = new() { AutoSize = true, Text = "刷新选中" };
    private readonly Label _titleLabel = new() { AutoSize = true, Text = "仓库状态总览" };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private readonly MultiRepositoryStatusCache _cache = new();
    private readonly MultiRepositoryFetchSchedule _fetchSchedule = new();
    private readonly ISystemIdleTimeProvider _idleTimeProvider;

    private Dictionary<string, MultiRepositoryStatus> _statuses = new(StringComparer.OrdinalIgnoreCase);
    private List<Repository> _repositories = [];
    private IMultiRepositoryStatusProvider? _statusProvider;
    private DateTimeOffset _lastLocalRefreshUtc;
    private bool _initialized;
    private bool _initializationStarted;
    private bool _operationInProgress;
    private bool _schedulerTickInProgress;

    public MultiRepositoryStatusControl()
        : this(new SystemIdleTimeProvider())
    {
    }

    internal MultiRepositoryStatusControl(ISystemIdleTimeProvider idleTimeProvider)
    {
        _idleTimeProvider = idleTimeProvider;
        InitializeUi();
        InitializeComplete();

        _backButton.Click += (_, _) => RepositoriesRequested?.Invoke(this, EventArgs.Empty);
        _openButton.Click += (_, _) => OpenSelectedRepository();
        _refreshButton.Click += (_, _) => this.InvokeAndForget(RefreshSelectedAsync, cancellationToken: _lifetimeCancellation.Token);
        _refreshAllButton.Click += (_, _) => this.InvokeAndForget(() => RefreshAllAsync(reloadRepositories: true), cancellationToken: _lifetimeCancellation.Token);
        _fetchButton.Click += (_, _) => this.InvokeAndForget(FetchSelectedAsync, cancellationToken: _lifetimeCancellation.Token);
        _fetchAllButton.Click += (_, _) => this.InvokeAndForget(() => FetchRepositoriesAsync(_repositories, isAutomatic: false), cancellationToken: _lifetimeCancellation.Token);
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                OpenSelectedRepository();
            }
        };
        _grid.SelectionChanged += (_, _) => UpdateButtonState();
        _timer.Tick += Timer_Tick;
    }

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;
    public event EventHandler? RepositoriesRequested;

    public int HeaderHeight
    {
        get => _headerActions.Height;
        set => _headerActions.Height = value;
    }

    public void ApplyTheme(DashboardTheme theme)
    {
        BackColor = SystemColors.Window;
        ForeColor = theme.PrimaryText;
        _headerActions.BackColor = theme.HeaderBackColor;
        _titleLabel.ForeColor = theme.SecondaryHeadingText;
        _operationLabel.ForeColor = theme.SecondaryText;
        _grid.BackgroundColor = SystemColors.Window;
        _grid.DefaultCellStyle.BackColor = SystemColors.Window;
        _grid.DefaultCellStyle.ForeColor = theme.PrimaryText;
        _grid.DefaultCellStyle.SelectionBackColor = theme.StartBackColor;
        _grid.DefaultCellStyle.SelectionForeColor = theme.PrimaryText;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = theme.SearchBackColor;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = theme.PrimaryText;
        Invalidate(true);
    }

    public void RefreshContent()
    {
        if (!_initialized)
        {
            return;
        }

        this.InvokeAndForget(() => RefreshAllAsync(reloadRepositories: true), cancellationToken: _lifetimeCancellation.Token);
    }

    internal void Start(IServiceProvider serviceProvider)
    {
        if (_initializationStarted || LicenseManager.UsageMode == LicenseUsageMode.Designtime || GitModuleForm.IsUnitTestActive)
        {
            return;
        }

        _initializationStarted = true;
        _statusProvider = new MultiRepositoryStatusProvider(serviceProvider.GetRequiredService<IGitExecutorProvider>());
        _timer.Start();
        ThreadHelper.FileAndForget(InitializeAsync);
    }

    protected override void OnRuntimeLoad()
    {
        base.OnRuntimeLoad();
        Start(ServiceProvider);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
            _titleLabel.Font?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeUi()
    {
        SuspendLayout();

        _titleLabel.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);
        _titleLabel.Margin = new Padding(20, 16, 18, 3);

        _headerActions.Dock = DockStyle.Top;
        _headerActions.Height = 70;
        _headerActions.Padding = new Padding(0, 8, 12, 6);
        _headerActions.WrapContents = true;
        _headerActions.Controls.AddRange([
            _titleLabel,
            _backButton,
            _openButton,
            _refreshButton,
            _refreshAllButton,
            _fetchButton,
            _fetchAllButton,
            _operationLabel]);

        ConfigureGrid();
        Controls.Add(_grid);
        Controls.Add(_headerActions);
        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;

        ResumeLayout(performLayout: true);
    }

    private void ConfigureGrid()
    {
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToOrderColumns = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BorderStyle = BorderStyle.None;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _grid.Dock = DockStyle.Fill;
        _grid.EnableHeadersVisualStyles = false;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        AddColumn("仓库", 180);
        AddColumn("分类", 90);
        AddColumn("分支", 110);
        AddColumn("工作区", 150);
        AddColumn("同步状态", 120);
        AddColumn("上次 Fetch", 115);
        AddColumn("检查时间", 115);
        AddColumn("错误", 220);

        void AddColumn(string header, float fillWeight)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                Name = header.Replace(" ", "", StringComparison.Ordinal),
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = fillWeight
            });
        }
    }

    private async Task InitializeAsync()
    {
        IReadOnlyDictionary<string, MultiRepositoryStatus> cachedStatuses = await Task.Run(_cache.Load, _lifetimeCancellation.Token);
        IList<Repository> repositories = await RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_lifetimeCancellation.Token);

        _repositories = NormalizeRepositories(repositories);
        _statuses = cachedStatuses
            .Where(pair => _repositories.Any(repository => PathsEqual(repository.Path, pair.Key)))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        _initialized = true;
        RenderRows(cached: _statuses.Count != 0);
        await RefreshAllAsync(reloadRepositories: false);
    }

    private async Task RefreshAllAsync(bool reloadRepositories)
    {
        if (_operationInProgress || _statusProvider is null)
        {
            return;
        }

        SetBusy(true, "正在刷新本地状态…");
        try
        {
            if (reloadRepositories)
            {
                IList<Repository> repositories = await RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync();
                _repositories = NormalizeRepositories(repositories);
            }

            IReadOnlyList<MultiRepositoryStatus> results = await GetStatusesAsync(_repositories, _lifetimeCancellation.Token);
            MergeStatuses(results);
            _lastLocalRefreshUtc = DateTimeOffset.UtcNow;
            RenderRows(cached: false);
            SaveCache();
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            SetBusy(false, "");
        }
    }

    private async Task RefreshSelectedAsync()
    {
        Repository? repository = GetSelectedRepository();
        if (repository is null || _operationInProgress || _statusProvider is null)
        {
            return;
        }

        SetBusy(true, $"正在刷新 {GetRepositoryName(repository)}…");
        try
        {
            DateTimeOffset? lastFetchUtc = GetLastFetchUtc(repository.Path);
            MultiRepositoryStatus status = await _statusProvider.GetStatusAsync(repository, lastFetchUtc, _lifetimeCancellation.Token);
            MergeStatuses([status]);
            RenderRows(cached: false);
            SaveCache();
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            SetBusy(false, "");
        }
    }

    private Task FetchSelectedAsync()
    {
        Repository? repository = GetSelectedRepository();
        return repository is null ? Task.CompletedTask : FetchRepositoriesAsync([repository], isAutomatic: false);
    }

    private async Task FetchRepositoriesAsync(IReadOnlyList<Repository> repositories, bool isAutomatic)
    {
        if (_operationInProgress || _statusProvider is null || repositories.Count == 0)
        {
            return;
        }

        SetBusy(true, isAutomatic ? "系统空闲，正在 Fetch…" : "正在 Fetch 全部远端…");
        int concurrency = Math.Clamp(AppSettings.MultiRepositoryStatusFetchConcurrency, 1, 16);
        TimeSpan timeout = TimeSpan.FromSeconds(Math.Clamp(AppSettings.MultiRepositoryStatusFetchTimeoutSeconds, 10, 3600));
        using SemaphoreSlim gate = new(concurrency);

        try
        {
            Task<MultiRepositoryStatus>[] tasks = [.. repositories.Select(async repository =>
            {
                await gate.WaitAsync(_lifetimeCancellation.Token);
                try
                {
                    MultiRepositoryFetchResult fetchResult = await _statusProvider.FetchAllRemotesAsync(repository, timeout, _lifetimeCancellation.Token);
                    DateTimeOffset? lastFetchUtc = fetchResult.FetchedUtc ?? GetLastFetchUtc(repository.Path);
                    MultiRepositoryStatus status = await _statusProvider.GetStatusAsync(repository, lastFetchUtc, _lifetimeCancellation.Token);
                    return status with { Error = fetchResult.Error ?? status.Error };
                }
                finally
                {
                    gate.Release();
                }
            })];

            MultiRepositoryStatus[] results = await Task.WhenAll(tasks);
            MergeStatuses(results);
            _lastLocalRefreshUtc = DateTimeOffset.UtcNow;
            if (!isAutomatic && _idleTimeProvider.GetIdleTime() >= GetConfiguredIdleTime())
            {
                _fetchSchedule.MarkAttempt(DateTimeOffset.UtcNow);
            }

            RenderRows(cached: false);
            SaveCache();
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            SetBusy(false, "");
        }
    }

    private async Task<IReadOnlyList<MultiRepositoryStatus>> GetStatusesAsync(IReadOnlyList<Repository> repositories, CancellationToken cancellationToken)
    {
        if (_statusProvider is null)
        {
            return [];
        }

        int concurrency = Math.Clamp(AppSettings.MultiRepositoryStatusFetchConcurrency, 1, 16);
        using SemaphoreSlim gate = new(concurrency);
        Task<MultiRepositoryStatus>[] tasks = [.. repositories.Select(async repository =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                return await _statusProvider.GetStatusAsync(repository, GetLastFetchUtc(repository.Path), cancellationToken);
            }
            finally
            {
                gate.Release();
            }
        })];

        return await Task.WhenAll(tasks);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_schedulerTickInProgress)
        {
            return;
        }

        _schedulerTickInProgress = true;
        this.InvokeAndForget(async () =>
        {
            try
            {
                await OnTimerTickAsync();
            }
            finally
            {
                _schedulerTickInProgress = false;
            }
        }, cancellationToken: _lifetimeCancellation.Token);
    }

    private async Task OnTimerTickAsync()
    {
        if (!_initialized || _operationInProgress || _lifetimeCancellation.IsCancellationRequested)
        {
            return;
        }

        try
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (Visible && now - _lastLocalRefreshUtc >= LocalRefreshInterval)
            {
                await RefreshAllAsync(reloadRepositories: true);
                return;
            }

            TimeSpan idleTime = _idleTimeProvider.GetIdleTime();
            TimeSpan interval = TimeSpan.FromMinutes(Math.Clamp(AppSettings.MultiRepositoryStatusFetchIntervalMinutes, 1, 24 * 60));
            bool shouldFetch = _fetchSchedule.ShouldFetch(
                now,
                AppSettings.MultiRepositoryStatusAutoFetchEnabled,
                idleTime,
                GetConfiguredIdleTime(),
                interval);
            if (shouldFetch)
            {
                _fetchSchedule.MarkAttempt(now);
                await ReloadRepositoriesAsync();
                await FetchRepositoriesAsync(_repositories, isAutomatic: true);
            }
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
    }

    private async Task ReloadRepositoriesAsync()
    {
        IList<Repository> repositories = await RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync();
        _repositories = NormalizeRepositories(repositories);
    }

    private void MergeStatuses(IEnumerable<MultiRepositoryStatus> statuses)
    {
        HashSet<string> currentPaths = new(_repositories.Select(repository => repository.Path), StringComparer.OrdinalIgnoreCase);
        foreach (string stalePath in _statuses.Keys.Where(path => !currentPaths.Contains(path)).ToList())
        {
            _statuses.Remove(stalePath);
        }

        foreach (MultiRepositoryStatus status in statuses)
        {
            _statuses[status.RepositoryPath] = status;
        }
    }

    private void RenderRows(bool cached)
    {
        string? selectedPath = GetSelectedRepository()?.Path;
        _grid.SuspendLayout();
        try
        {
            _grid.Rows.Clear();
            foreach (Repository repository in _repositories)
            {
                _statuses.TryGetValue(repository.Path, out MultiRepositoryStatus? status);
                int rowIndex = _grid.Rows.Add(
                    GetRepositoryName(repository),
                    repository.Category ?? "",
                    status?.Branch ?? "",
                    FormatWorkingTree(status),
                    FormatSynchronization(status),
                    FormatTimestamp(status?.LastFetchUtc),
                    FormatTimestamp(status?.LastCheckedUtc),
                    status?.Error ?? "");

                DataGridViewRow row = _grid.Rows[rowIndex];
                row.Tag = repository;
                row.Cells[0].ToolTipText = repository.Path;
                if (!string.IsNullOrWhiteSpace(status?.Error))
                {
                    row.Cells[7].Style.ForeColor = Color.Firebrick.AdaptBackColor();
                    row.Cells[7].ToolTipText = status.Error;
                }

                if (selectedPath is not null && PathsEqual(selectedPath, repository.Path))
                {
                    row.Selected = true;
                }
            }
        }
        finally
        {
            _grid.ResumeLayout();
        }

        if (!_operationInProgress)
        {
            _operationLabel.Text = cached && _statuses.Count != 0
                ? "正在显示缓存结果并刷新…"
                : $"{_repositories.Count} 个收藏仓库";
        }

        UpdateButtonState();
    }

    private void SaveCache()
    {
        MultiRepositoryStatus[] statuses = [.. _repositories
            .Select(repository => _statuses.GetValueOrDefault(repository.Path))
            .OfType<MultiRepositoryStatus>()];
        ThreadHelper.FileAndForget(() => Task.Run(() => _cache.Save(statuses), _lifetimeCancellation.Token));
    }

    private void SetBusy(bool busy, string operation)
    {
        _operationInProgress = busy;
        _operationLabel.Text = busy ? operation : $"{_repositories.Count} 个收藏仓库";
        UseWaitCursor = busy;
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool hasSelection = GetSelectedRepository() is not null;
        _openButton.Enabled = !_operationInProgress && hasSelection;
        _refreshButton.Enabled = !_operationInProgress && hasSelection;
        _fetchButton.Enabled = !_operationInProgress && hasSelection;
        _refreshAllButton.Enabled = !_operationInProgress && _repositories.Count != 0;
        _fetchAllButton.Enabled = !_operationInProgress && _repositories.Count != 0;
        _backButton.Enabled = !_operationInProgress;
    }

    private void OpenSelectedRepository()
    {
        Repository? repository = GetSelectedRepository();
        if (repository is null || (!GitModule.IsValidGitWorkingDir(repository.Path) && !GitModule.IsBareRepository(repository.Path)))
        {
            return;
        }

        GitModule module = new(ServiceProvider.GetRequiredService<IGitExecutorProvider>(), repository.Path);
        GitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));
    }

    private Repository? GetSelectedRepository()
        => _grid.SelectedRows.Count == 0 ? null : _grid.SelectedRows[0].Tag as Repository;

    private DateTimeOffset? GetLastFetchUtc(string path)
        => _statuses.TryGetValue(path, out MultiRepositoryStatus? status) ? status.LastFetchUtc : null;

    private static List<Repository> NormalizeRepositories(IEnumerable<Repository> repositories)
        => [.. repositories
            .Where(repository => !string.IsNullOrWhiteSpace(repository.Path))
            .DistinctBy(repository => repository.Path, StringComparer.OrdinalIgnoreCase)];

    private static string GetRepositoryName(Repository repository)
    {
        string path = repository.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.GetFileName(path) is { Length: > 0 } name ? name : path;
    }

    private static string FormatWorkingTree(MultiRepositoryStatus? status)
    {
        if (status is null)
        {
            return "等待检查";
        }

        if (status.IsBare)
        {
            return "裸仓库";
        }

        if (!status.HasWorkingTreeChanges)
        {
            return "干净";
        }

        List<string> parts = [];
        if (status.StagedCount != 0)
        {
            parts.Add($"已暂存 {status.StagedCount}");
        }

        if (status.ModifiedCount != 0)
        {
            parts.Add($"已修改 {status.ModifiedCount}");
        }

        if (status.UntrackedCount != 0)
        {
            parts.Add($"未跟踪 {status.UntrackedCount}");
        }

        return string.Join(" · ", parts);
    }

    private static string FormatSynchronization(MultiRepositoryStatus? status)
    {
        if (status is null)
        {
            return "等待检查";
        }

        if (status.IsDetached)
        {
            return "分离 HEAD";
        }

        if (string.IsNullOrWhiteSpace(status.Upstream))
        {
            return "未设置上游";
        }

        return $"↑{status.Ahead ?? 0} ↓{status.Behind ?? 0}";
    }

    private static string FormatTimestamp(DateTimeOffset? timestamp)
        => timestamp is null || timestamp == default
            ? "从未"
            : timestamp.Value.ToLocalTime().ToString("g");

    private static bool PathsEqual(string left, string right)
        => string.Equals(left.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                         right.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                         StringComparison.OrdinalIgnoreCase);

    private static TimeSpan GetConfiguredIdleTime()
        => TimeSpan.FromMinutes(Math.Clamp(AppSettings.MultiRepositoryStatusIdleMinutes, 1, 24 * 60));
}

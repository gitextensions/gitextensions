using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI.UserControls.RevisionGrid;

public class IndexChangedEventArgs : EventArgs
{
    public IndexChangedEventArgs(bool isIndexChanged)
    {
        IsIndexChanged = isIndexChanged;
    }

    public bool IsIndexChanged { get; }
}

public sealed class IndexWatcher : IDisposable
{
    public event EventHandler<IndexChangedEventArgs>? Changed;

    private readonly IGitUICommandsSource _uICommandsSource;

    private IGitModule Module => _uICommandsSource.UICommands.Module;

    public IndexWatcher(IGitUICommandsSource uiCommandsSource)
    {
        _uICommandsSource = uiCommandsSource;
        _uICommandsSource.UICommandsChanged += OnUICommandsChanged;
        GitIndexWatcher = new FileSystemWatcher();
        RefsWatcher = new FileSystemWatcher();

        GitIndexWatcher.Changed += fileSystemWatcher_Changed;
        RefsWatcher.Changed += fileSystemWatcher_Changed;
    }

    private void OnUICommandsChanged(object? sender, GitUICommandsChangedEventArgs e)
    {
        Clear();
    }

    private void SetFileSystemWatcher()
    {
        if (!Module.IsValidGitWorkingDir())
        {
            GitIndexWatcher.EnableRaisingEvents = false;
            RefsWatcher.EnableRaisingEvents = false;
        }
        else
        {
            try
            {
                _enabled = AppSettings.ShowGitStatusInBrowseToolbar;

                _gitDirPath = Module.WorkingDirGitDir;

                GitIndexWatcher.Path = _gitDirPath;
                GitIndexWatcher.Filter = "index";
                GitIndexWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
                GitIndexWatcher.IncludeSubdirectories = false;
                GitIndexWatcher.EnableRaisingEvents = _enabled;

                RefsWatcher.Path = Path.Join(Module.GitCommonDirectory, "refs");
                RefsWatcher.IncludeSubdirectories = true;
                RefsWatcher.EnableRaisingEvents = _enabled;
            }
            catch
            {
                _enabled = false;
            }
        }
    }

    private bool _indexChanged;

    /// <summary>
    ///  Gets whether the index or any ref has changed since the last <see cref="Reset"/>.
    /// </summary>
    public bool IndexChanged
    {
        get
        {
            if (!_enabled)
            {
                return true;
            }

            if (_gitDirPath != Module.WorkingDirGitDir)
            {
                return true;
            }

            return _indexChanged;
        }
        private set
        {
            _indexChanged = value;
            try
            {
                GitIndexWatcher.EnableRaisingEvents = !IndexChanged
                    && Directory.Exists(GitIndexWatcher.Path);
            }
            catch
            {
                GitIndexWatcher.EnableRaisingEvents = false;
            }

            Changed?.Invoke(this, new IndexChangedEventArgs(IndexChanged));
        }
    }

    private bool _enabled;
    private string? _gitDirPath;
    private static readonly TimeSpan _fileTimeGranularity = TimeSpan.FromMilliseconds(20);
    private readonly Lock _resetLock = new();
    private DateTime _lastResetUtc;
    private FileSystemWatcher GitIndexWatcher { get; }
    private FileSystemWatcher RefsWatcher { get; }

    private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        // The notifications are raised asynchronously on the thread pool and can arrive after Reset()
        // for changes made before it, e.g. by the fetch that triggered the refresh.
        // A no longer existing file, e.g. a *.lock file renamed to the ref, has the time 1601-01-01.
        // Use the time of its directory instead, which is updated by the rename.
        DateTime lastWriteTimeUtc = Path.Exists(e.FullPath)
            ? File.GetLastWriteTimeUtc(e.FullPath)
            : Directory.GetLastWriteTimeUtc(Path.GetDirectoryName(e.FullPath)!);

        // Compare and set atomically with Reset(), else a stale notification can set the state after Reset() cleared it.
        lock (_resetLock)
        {
            if (lastWriteTimeUtc < _lastResetUtc)
            {
                return;
            }

            IndexChanged = true;
        }
    }

    /// <summary>
    /// Reenable indexwatcher (if disabled), reset icon.
    /// </summary>
    public void Reset()
    {
        lock (_resetLock)
        {
            // File times are set with the coarse system clock tick (15.6 ms by default), DateTime.UtcNow is precise.
            // Do not ignore changes made just after the reset.
            _lastResetUtc = DateTime.UtcNow - _fileTimeGranularity;
            RefreshWatcher();
            IndexChanged = false;
        }
    }

    /// <summary>
    /// Disable events, reset icon.
    /// </summary>
    public void Clear()
    {
        GitIndexWatcher.EnableRaisingEvents = false;
        RefsWatcher.EnableRaisingEvents = false;
        IndexChanged = false;
    }

    private void RefreshWatcher()
    {
        if (_gitDirPath != Module.WorkingDirGitDir || _enabled != AppSettings.ShowGitStatusInBrowseToolbar)
        {
            SetFileSystemWatcher();
        }
    }

    public void Dispose()
    {
        _enabled = false;
        GitIndexWatcher.EnableRaisingEvents = false;
        RefsWatcher.EnableRaisingEvents = false;
        GitIndexWatcher.Changed -= fileSystemWatcher_Changed;
        RefsWatcher.Changed -= fileSystemWatcher_Changed;
        GitIndexWatcher.Dispose();
        RefsWatcher.Dispose();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(IndexWatcher indexWatcher)
    {
        public DateTime LastResetUtc => indexWatcher._lastResetUtc;

        public void RaiseChanged(string fullPath)
            => indexWatcher.fileSystemWatcher_Changed(indexWatcher, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(fullPath)!, Path.GetFileName(fullPath)));
    }
}

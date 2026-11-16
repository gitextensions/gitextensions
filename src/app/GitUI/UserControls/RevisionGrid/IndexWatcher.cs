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
    private readonly object _syncRoot = new();
    private int _generation;
    private DateTime _watchedSinceUtc;

    private IGitModule Module => _uICommandsSource.UICommands.Module;

    public IndexWatcher(IGitUICommandsSource uiCommandsSource)
    {
        _uICommandsSource = uiCommandsSource;
        _uICommandsSource.UICommandsChanged += OnUICommandsChanged;
        GitIndexWatcher = new FileSystemWatcher();
        RefsWatcher = new FileSystemWatcher();

        GitIndexWatcher.Changed += fileSystemWatcher_Changed;
        GitIndexWatcher.Renamed += fileSystemWatcher_Renamed;
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
                GitIndexWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
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
    private static readonly DateTime _missingFileTimeUtc = DateTime.FromFileTimeUtc(0);
    private DateTime _lastResetUtc;
    private FileSystemWatcher GitIndexWatcher { get; }
    private FileSystemWatcher RefsWatcher { get; }

    private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        // The notifications are raised asynchronously on the thread pool and can arrive after Reset()
        // for changes made before it, e.g. by the fetch that triggered the refresh.
        // A no longer existing file, e.g. a *.lock file renamed to the ref, has the time 1601-01-01.
        // Use the time of its directory instead, which is updated by the rename.
        // (Check the time read instead of the existence before, else the file can vanish in between.)
        DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(e.FullPath);
        if (lastWriteTimeUtc == _missingFileTimeUtc)
        {
            lastWriteTimeUtc = Directory.GetLastWriteTimeUtc(Path.GetDirectoryName(e.FullPath)!);
        }

        // Compare and set atomically with Reset(), else a stale notification can set the state after Reset() cleared it.
        lock (_syncRoot)
        {
            if (lastWriteTimeUtc < _lastResetUtc)
            {
                return;
            }

            IndexChanged = true;
        }
    }

    private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        // Git writes "index.lock" and renames it over "index", which raises no Changed event for "index".
        // The Filter also matches renames away from "index", which are not index updates.
        // Handle it like a change, so that the same filtering of notifications applies.
        if (e.Name == "index")
        {
            fileSystemWatcher_Changed(sender, e);
        }
    }

    /// <summary>
    /// Reenable indexwatcher (if disabled), reset icon.
    /// </summary>
    public void Reset()
    {
        lock (_syncRoot)
        {
            ++_generation;

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
        lock (_syncRoot)
        {
            ++_generation;
            GitIndexWatcher.EnableRaisingEvents = false;
            RefsWatcher.EnableRaisingEvents = false;
            IndexChanged = false;
        }
    }

    private void RefreshWatcher()
    {
        if (_gitDirPath != Module.WorkingDirGitDir || _enabled != AppSettings.ShowGitStatusInBrowseToolbar)
        {
            SetFileSystemWatcher();
        }

        _watchedSinceUtc = DateTime.UtcNow;
    }

    /// <summary>
    ///  Marks the index as changed if the index or a ref has been modified since the last <see cref="Reset"/>
    ///  in a WSL repo, where the <see cref="FileSystemWatcher"/>s may not report changes
    ///  (https://github.com/microsoft/WSL/issues/4581).
    /// </summary>
    /// <remarks>
    ///  Only file and directory times are compared, no git command is run and the revisions are not reloaded.
    ///  Performs file system I/O, so call it from a background thread.
    /// </remarks>
    public void DetectUnreportedChangesInWsl()
    {
        string gitDirPath;
        DateTime watchedSinceUtc;
        int generation;
        lock (_syncRoot)
        {
            if (!_enabled || _indexChanged || _gitDirPath is null || !PathUtil.IsWslPath(_gitDirPath))
            {
                return;
            }

            gitDirPath = _gitDirPath;
            watchedSinceUtc = _watchedSinceUtc;
            generation = _generation;
        }

        if (!HasChangedSince(gitDirPath, Module.GitCommonDirectory, watchedSinceUtc))
        {
            return;
        }

        lock (_syncRoot)
        {
            // The scan runs concurrently with the UI thread. Discard its result if the watcher has been reset, cleared or disposed meanwhile,
            // otherwise a change which has already been handled by a refresh would mark the index as changed again.
            if (_enabled && generation == _generation)
            {
                IndexChanged = true;
            }
        }
    }

    private static bool HasChangedSince(string gitDirPath, string gitCommonDirPath, DateTime sinceUtc)
    {
        try
        {
            if (File.GetLastWriteTimeUtc(Path.Join(gitDirPath, "index")) > sinceUtc
                || File.GetLastWriteTimeUtc(Path.Join(gitDirPath, "HEAD")) > sinceUtc
                || File.GetLastWriteTimeUtc(Path.Join(gitCommonDirPath, "packed-refs")) > sinceUtc)
            {
                return true;
            }

            // Git updates a loose ref by renaming "<ref>.lock" over it, which updates the time of the containing directory,
            // so the directories need to be checked only, not the probably many ref files.
            string refsPath = Path.Join(gitCommonDirPath, "refs");
            return Directory.GetLastWriteTimeUtc(refsPath) > sinceUtc
                || Directory.EnumerateDirectories(refsPath, "*", SearchOption.AllDirectories)
                    .Any(directory => Directory.GetLastWriteTimeUtc(directory) > sinceUtc);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            _enabled = false;
        }

        GitIndexWatcher.EnableRaisingEvents = false;
        RefsWatcher.EnableRaisingEvents = false;
        GitIndexWatcher.Changed -= fileSystemWatcher_Changed;
        GitIndexWatcher.Renamed -= fileSystemWatcher_Renamed;
        RefsWatcher.Changed -= fileSystemWatcher_Changed;
        GitIndexWatcher.Dispose();
        RefsWatcher.Dispose();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(IndexWatcher indexWatcher)
    {
        public DateTime LastResetUtc => indexWatcher._lastResetUtc;

        public static bool HasChangedSince(string gitDirPath, string gitCommonDirPath, DateTime sinceUtc)
            => IndexWatcher.HasChangedSince(gitDirPath, gitCommonDirPath, sinceUtc);

        public void RaiseChanged(string fullPath)
            => indexWatcher.fileSystemWatcher_Changed(indexWatcher, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(fullPath)!, Path.GetFileName(fullPath)));
    }
}

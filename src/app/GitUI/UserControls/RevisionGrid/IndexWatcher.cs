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
    private bool IndexChanged
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
        set
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
    private FileSystemWatcher GitIndexWatcher { get; }
    private FileSystemWatcher RefsWatcher { get; }

    private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        IndexChanged = true;
    }

    /// <summary>
    /// Reenable indexwatcher (if disabled), reset icon.
    /// </summary>
    public void Reset()
    {
        RefreshWatcher();
        IndexChanged = false;
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

    private DateTime _watchedSinceUtc;

    private void RefreshWatcher()
    {
        if (_gitDirPath != Module.WorkingDirGitDir || _enabled != AppSettings.ShowGitStatusInBrowseToolbar)
        {
            SetFileSystemWatcher();
        }

        _watchedSinceUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the index as changed if the index or a ref has been modified since the last <see cref="Reset"/>
    /// in a WSL repo, where the <see cref="FileSystemWatcher"/>s may not report changes
    /// (https://github.com/microsoft/WSL/issues/4581).
    /// </summary>
    /// <remarks>
    /// Only file and directory times are compared, no git command is run and the revisions are not reloaded.
    /// Performs file system I/O, so call it from a background thread.
    /// </remarks>
    public void DetectUnreportedChanges()
    {
        if (!_enabled || _indexChanged || _gitDirPath is null || !PathUtil.IsWslPath(_gitDirPath))
        {
            return;
        }

        if (HasChangedSince(_gitDirPath, Module.GitCommonDirectory, _watchedSinceUtc))
        {
            IndexChanged = true;
        }
    }

    internal static bool HasChangedSince(string gitDirPath, string gitCommonDirPath, DateTime sinceUtc)
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
        _enabled = false;
        GitIndexWatcher.EnableRaisingEvents = false;
        RefsWatcher.EnableRaisingEvents = false;
        GitIndexWatcher.Changed -= fileSystemWatcher_Changed;
        RefsWatcher.Changed -= fileSystemWatcher_Changed;
        GitIndexWatcher.Dispose();
        RefsWatcher.Dispose();
    }
}

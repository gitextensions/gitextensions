using System.Buffers.Binary;
using System.Security.Cryptography;
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
    private static readonly TimeSpan _captureTimeout = TimeSpan.FromSeconds(10);
    private readonly object _syncRoot = new();
    private int _generation;
    private DateTime _watchedSinceUtc;

    /// <summary>
    ///  The <see cref="GetIndexContentHash"/> of the index at the last <see cref="Reset"/>, <see langword="null"/> if not (yet) known.
    /// </summary>
    private byte[]? _indexContentHashAtReset;

    /// <summary>
    ///  The task reading <see cref="_indexContentHashAtReset"/>, <see langword="null"/> if none.
    /// </summary>
    private Task? _indexContentHashCapture;

    private IGitModule Module => _uICommandsSource.UICommands.Module;

    public IndexWatcher(IGitUICommandsSource uiCommandsSource)
    {
        _uICommandsSource = uiCommandsSource;
        _uICommandsSource.UICommandsChanged += OnUICommandsChanged;
        GitIndexWatcher = new FileSystemWatcher();
        RefsWatcher = new FileSystemWatcher();

        GitIndexWatcher.Changed += gitIndexWatcher_Changed;
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

    private void gitIndexWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        // "git status" refreshes the file system data of the index entries and rewrites the index if that data is stale,
        // e.g. after "git stash". Git Extensions runs the first status update after a refresh, i.e. after Reset(),
        // without "--no-optional-locks" to speed up the subsequent updates, so the refresh would mark the index as changed itself.
        // Ignore rewrites which do not change the entries. Other changes of the index (made concurrently or not) change the entries.
        // The first status may rewrite the index before the baseline has been read, so do not take "not yet read" for "changed".
        // Wait for the baseline; if the capture raced with the rewrite and stored null, re-capture the now-stable index once.
        byte[]? hashAtReset = null;
        Task? capture;
        lock (_syncRoot)
        {
            capture = _indexContentHashCapture;
        }

        for (int captureCount = 0; ; ++captureCount)
        {
            if (capture is null)
            {
                break;
            }

            // The FileSystemWatcher events run on the thread pool, so waiting here cannot deadlock the UI thread
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            capture.Wait(_captureTimeout);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            lock (_syncRoot)
            {
                hashAtReset = _indexContentHashAtReset;
                if (hashAtReset is not null || captureCount >= 1)
                {
                    break;
                }

                // The capture raced with the rewrite and stored null, so re-capture the now-stable index
                CaptureIndexContentHash();
                capture = _indexContentHashCapture;
            }
        }

        if (hashAtReset is not null && ReadIndexContentHash(e.FullPath) is byte[] hash && hash.AsSpan().SequenceEqual(hashAtReset))
        {
            return;
        }

        fileSystemWatcher_Changed(sender, e);
    }

    private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        // Git writes "index.lock" and renames it over "index", which raises no Changed event for "index".
        // The Filter also matches renames away from "index", which are not index updates.
        // Handle it like a change, so that the same filtering of notifications applies.
        if (e.Name == "index")
        {
            gitIndexWatcher_Changed(sender, e);
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
            CaptureIndexContentHash();
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
            _indexContentHashAtReset = null;
            _indexContentHashCapture = null;
            GitIndexWatcher.EnableRaisingEvents = false;
            RefsWatcher.EnableRaisingEvents = false;
            IndexChanged = false;
        }
    }

    /// <summary>
    ///  Reads the <see cref="GetIndexContentHash"/> of the index in the background, if the index is not changed meanwhile.
    /// </summary>
    private void CaptureIndexContentHash()
    {
        _indexContentHashAtReset = null;
        _indexContentHashCapture = null;
        if (!_enabled || _gitDirPath is null)
        {
            return;
        }

        string indexPath = Path.Join(_gitDirPath, "index");
        int generation = _generation;

        // Only the file system data is read synchronously, it identifies the version of the index which is hashed asynchronously.
        // So a change made after the reset but before the hash is read is not taken for the state at the reset, but is still reported.
        IndexFileState? stateAtReset = GetIndexFileState(indexPath);
        if (stateAtReset is null)
        {
            return;
        }

        Task capture = Task.Run(() =>
        {
            byte[]? hash = ReadIndexContentHash(indexPath, stateAtReset);
            lock (_syncRoot)
            {
                if (generation == _generation)
                {
                    _indexContentHashAtReset = hash;
                }
            }
        });
        _indexContentHashCapture = capture;
        capture.FileAndForget();
    }

    private static IndexFileState? GetIndexFileState(string indexPath)
    {
        try
        {
            FileInfo fileInfo = new(indexPath);
            return fileInfo.Exists ? new IndexFileState(fileInfo.LastWriteTimeUtc, fileInfo.Length) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    ///  Reads the index and returns its <see cref="GetIndexContentHash"/>.
    /// </summary>
    /// <param name="indexPath">The path of the index file.</param>
    /// <param name="expectedState">If not <see langword="null"/>, the hash is returned only if this version of the index has been read.</param>
    /// <returns>The hash or <see langword="null"/> if the index cannot be read or parsed.</returns>
    private static byte[]? ReadIndexContentHash(string indexPath, IndexFileState? expectedState = null)
    {
        try
        {
            byte[] index;

            // Git replaces the index by renaming "index.lock" over it, so allow that while reading
            using (FileStream stream = new(indexPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                index = new byte[stream.Length];
                stream.ReadExactly(index);
            }

            if (expectedState is not null && GetIndexFileState(indexPath) != expectedState)
            {
                return null;
            }

            return GetIndexContentHash(index);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    ///  Gets a hash of the entries of the git index (https://git-scm.com/docs/index-format) without the file system data
    ///  (times, device, inode, uid, gid, file size) and without the extensions, which "git status" refreshes.
    /// </summary>
    /// <returns>The hash or <see langword="null"/> if the data is not a complete index of version 2, 3 or 4 with SHA-1 object IDs and a valid checksum, or if it is a split index.</returns>
    private static byte[]? GetIndexContentHash(ReadOnlySpan<byte> index)
    {
        const int headerLength = 12;
        const int statDataLength = 40;
        const int modeOffset = 24;
        const int modeLength = 4;
        const int objectIdLength = 20;
        const int flagsLength = 2;
        const int extendedFlagsLength = 2;
        const ushort extendedFlag = 0x4000;
        const int entryAlignment = 8;
        const int extensionSignatureLength = 4;
        const int extensionHeaderLength = extensionSignatureLength + 4;
        const int checksumLength = 20;

        if (index.Length < headerLength || !index.StartsWith("DIRC"u8))
        {
            return null;
        }

        uint version = BinaryPrimitives.ReadUInt32BigEndian(index[4..]);
        if (version is < 2 or > 4)
        {
            return null;
        }

        uint entryCount = BinaryPrimitives.ReadUInt32BigEndian(index[8..]);

        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(index[..headerLength]);
        ReadOnlySpan<byte> entries = index[headerLength..];
        for (uint i = 0; i < entryCount; ++i)
        {
            int nameOffset = statDataLength + objectIdLength + flagsLength;
            if (entries.Length < nameOffset)
            {
                return null;
            }

            ushort flags = BinaryPrimitives.ReadUInt16BigEndian(entries[(statDataLength + objectIdLength)..]);
            if (version >= 3 && (flags & extendedFlag) != 0)
            {
                if (entries.Length < nameOffset + extendedFlagsLength)
                {
                    return null;
                }

                nameOffset += extendedFlagsLength;
            }

            int entryLength;
            if (version == 4)
            {
                // The path is prefix-compressed: a variable-length number of bytes to strip from the previous path, then the NUL-terminated rest
                int suffixOffset = entries[nameOffset..].IndexOfAnyInRange((byte)0, (byte)0x7F);
                int suffixLength = suffixOffset < 0 ? -1 : entries[(nameOffset + suffixOffset + 1)..].IndexOf((byte)0);
                if (suffixLength < 0)
                {
                    return null;
                }

                entryLength = nameOffset + suffixOffset + 1 + suffixLength + 1;
            }
            else
            {
                // The path is NUL-terminated and padded with NULs to a multiple of 8 bytes
                int nameLength = entries[nameOffset..].IndexOf((byte)0);
                entryLength = (nameOffset + nameLength + entryAlignment) & ~(entryAlignment - 1);
                if (nameLength < 0 || entryLength > entries.Length)
                {
                    return null;
                }
            }

            hash.AppendData(entries.Slice(modeOffset, modeLength));
            hash.AppendData(entries[statDataLength..entryLength]);
            entries = entries[entryLength..];
        }

        // The extensions are not hashed, because "git status" refreshes them (e.g. the untracked cache), but a split index
        // keeps the deleted and replaced entries of the shared index ("sharedindex.*") in the "link" extension.
        // The entries cannot be compared then, so report a change. The trailing checksum must be present, which also detects a truncated index.
        while (entries.Length > checksumLength)
        {
            if (entries.Length < extensionHeaderLength + checksumLength)
            {
                return null;
            }

            uint extensionSize = BinaryPrimitives.ReadUInt32BigEndian(entries[extensionSignatureLength..]);
            if (entries.StartsWith("link"u8) || extensionSize > entries.Length - extensionHeaderLength - checksumLength)
            {
                return null;
            }

            entries = entries[(extensionHeaderLength + (int)extensionSize)..];
        }

        if (entries.Length != checksumLength)
        {
            return null;
        }

        // Validate the trailing SHA-1 checksum, which git uses to detect a corrupt index; a mismatch means the index is invalid
        using IncrementalHash indexChecksum = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        indexChecksum.AppendData(index[..^checksumLength]);
        if (!indexChecksum.GetHashAndReset().AsSpan().SequenceEqual(entries))
        {
            return null;
        }

        return hash.GetHashAndReset();
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
        GitIndexWatcher.Changed -= gitIndexWatcher_Changed;
        GitIndexWatcher.Renamed -= fileSystemWatcher_Renamed;
        RefsWatcher.Changed -= fileSystemWatcher_Changed;
        GitIndexWatcher.Dispose();
        RefsWatcher.Dispose();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(IndexWatcher indexWatcher)
    {
        public DateTime LastResetUtc => indexWatcher._lastResetUtc;

        public bool IndexChanged => indexWatcher.IndexChanged;

        public static byte[]? GetIndexContentHash(byte[] index) => IndexWatcher.GetIndexContentHash(index);

        public static bool HasChangedSince(string gitDirPath, string gitCommonDirPath, DateTime sinceUtc)
            => IndexWatcher.HasChangedSince(gitDirPath, gitCommonDirPath, sinceUtc);

        public void RaiseChanged(string fullPath)
            => indexWatcher.fileSystemWatcher_Changed(indexWatcher, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(fullPath)!, Path.GetFileName(fullPath)));

        public void RaiseIndexChanged()
            => indexWatcher.gitIndexWatcher_Changed(indexWatcher, new FileSystemEventArgs(WatcherChangeTypes.Changed, indexWatcher._gitDirPath!, "index"));

        public bool WaitForIndexContentHashAtReset(TimeSpan timeout)
        {
            IndexWatcher watcher = indexWatcher;
            return SpinWait.SpinUntil(
                () =>
                {
                    lock (watcher._syncRoot)
                    {
                        return watcher._indexContentHashAtReset is not null;
                    }
                },
                timeout);
        }

        public void SimulateCaptureRacedWithRewrite()
        {
            lock (indexWatcher._syncRoot)
            {
                indexWatcher._indexContentHashAtReset = null;
                indexWatcher._indexContentHashCapture = Task.CompletedTask;
            }
        }
    }

    private readonly record struct IndexFileState(DateTime LastWriteTimeUtc, long Length);
}

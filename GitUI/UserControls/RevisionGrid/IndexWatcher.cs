using System;
using System.IO;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid
{
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
        public event EventHandler<IndexChangedEventArgs> Changed;

        private readonly IGitUICommandsSource _uICommandsSource;

        private GitUICommands UICommands => _uICommandsSource.UICommands;

        private GitModule Module => UICommands.Module;

        public IndexWatcher(IGitUICommandsSource uiCommandsSource)
        {
            _uICommandsSource = uiCommandsSource;
            _uICommandsSource.UICommandsChanged += OnUICommandsChanged;
            GitIndexWatcher = new FileSystemWatcher();
            RefsWatcher = new FileSystemWatcher();
            SetFileSystemWatcher();

            IndexChanged = true;
            GitIndexWatcher.Changed += fileSystemWatcher_Changed;
            RefsWatcher.Changed += fileSystemWatcher_Changed;
        }

        private void OnUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
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
                    _enabled = AppSettings.UseFastChecks;

                    _gitDirPath = Module.WorkingDirGitDir;

                    GitIndexWatcher.Path = _gitDirPath;
                    GitIndexWatcher.Filter = "index";
                    GitIndexWatcher.IncludeSubdirectories = false;
                    GitIndexWatcher.EnableRaisingEvents = _enabled;

                    RefsWatcher.Path = Path.Combine(Module.GitCommonDirectory, "refs");
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
            set
            {
                _indexChanged = value;
                GitIndexWatcher.EnableRaisingEvents = !IndexChanged;

                Changed?.Invoke(this, new IndexChangedEventArgs(IndexChanged));
            }
        }

        private bool _enabled;
        private string _gitDirPath;
        private FileSystemWatcher GitIndexWatcher { get; }
        private FileSystemWatcher RefsWatcher { get; }

        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            IndexChanged = true;
        }

        public void Reset()
        {
            IndexChanged = false;
            RefreshWatcher();
        }

        public void Clear()
        {
            IndexChanged = true;
            RefreshWatcher();
        }

        private void RefreshWatcher()
        {
            if (_gitDirPath != Module.WorkingDirGitDir || _enabled != AppSettings.UseFastChecks)
            {
                SetFileSystemWatcher();
            }
        }

        public void Dispose()
        {
            _enabled = false;
            GitIndexWatcher.EnableRaisingEvents = false;
            GitIndexWatcher.Changed -= fileSystemWatcher_Changed;
            RefsWatcher.Changed -= fileSystemWatcher_Changed;
            GitIndexWatcher.Dispose();
            RefsWatcher.Dispose();
        }
    }
}

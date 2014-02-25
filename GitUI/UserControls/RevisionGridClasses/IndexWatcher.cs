using System;
using System.IO;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
{
    [Serializable]
    public delegate void IndexChangedEventHandler(bool indexChanged);

    public sealed class IndexWatcher : IDisposable
    {
        public event IndexChangedEventHandler Changed;

        private readonly IGitUICommandsSource UICommandsSource;

        private GitUICommands UICommands
        {
            get
            {
                return UICommandsSource.UICommands;
            }
        }

        private GitModule Module { get { return UICommands.Module; } }

        public IndexWatcher(IGitUICommandsSource aUICommandsSource)
        {
            UICommandsSource = aUICommandsSource;
            UICommandsSource.GitUICommandsChanged += UICommandsSource_GitUICommandsChanged;
            GitIndexWatcher = new FileSystemWatcher();
            RefsWatcher = new FileSystemWatcher();
            SetFileSystemWatcher();

            IndexChanged = true;
            GitIndexWatcher.Changed += fileSystemWatcher_Changed;
            RefsWatcher.Changed += fileSystemWatcher_Changed;
        }

        void UICommandsSource_GitUICommandsChanged(IGitUICommandsSource sender, GitUICommands oldCommands)
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
                    enabled = GitCommands.AppSettings.UseFastChecks;

                    Path = Module.GetGitDirectory();

                    GitIndexWatcher.Path = Path;
                    GitIndexWatcher.Filter = "index";
                    GitIndexWatcher.IncludeSubdirectories = false;
                    GitIndexWatcher.EnableRaisingEvents = enabled;

                    RefsWatcher.Path = System.IO.Path.Combine(Path, "refs");
                    RefsWatcher.IncludeSubdirectories = true;
                    RefsWatcher.EnableRaisingEvents = enabled;
                }
                catch
                {
                    enabled = false;
                }
            }
        }

        private bool indexChanged;
        public bool IndexChanged
        {
            get
            {
                if (!enabled)
                    return true;

                if (!Module.IsBareRepository())
                    return true;

                return indexChanged;
            }
            set
            {
                indexChanged = value;
                GitIndexWatcher.EnableRaisingEvents = !IndexChanged;

                if (Changed != null)
                    Changed(IndexChanged);
            }
        }

        private bool enabled;
        private string Path;
        private FileSystemWatcher GitIndexWatcher { get; set; }
        private FileSystemWatcher RefsWatcher { get; set; }

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
            if (!Module.IsBareRepository() ||
                enabled != GitCommands.AppSettings.UseFastChecks)
                SetFileSystemWatcher();
        }

        public void Dispose()
        {
            enabled = false;
            GitIndexWatcher.EnableRaisingEvents = false;
            GitIndexWatcher.Changed -= fileSystemWatcher_Changed;
            RefsWatcher.Changed -= fileSystemWatcher_Changed;
            GitIndexWatcher.Dispose();
            RefsWatcher.Dispose();
        }
    }
}

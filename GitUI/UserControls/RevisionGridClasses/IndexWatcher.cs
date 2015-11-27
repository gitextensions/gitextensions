using System;
using System.IO;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses
{
    public class IndexChangedEventArgs : EventArgs
    {
        public IndexChangedEventArgs(bool isIndexChanged)
        {
            IsIndexChanged = isIndexChanged;
        }

        public bool IsIndexChanged { get; private set; }
    }

    public sealed class IndexWatcher : IDisposable
    {
        public event EventHandler<IndexChangedEventArgs> Changed;

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

        void UICommandsSource_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
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

                if (Path != Module.GetGitDirectory())
                    return true;

                return indexChanged;
            }
            set
            {
                indexChanged = value;
                GitIndexWatcher.EnableRaisingEvents = !IndexChanged;

                if (Changed != null)
                    Changed(this, new IndexChangedEventArgs(IndexChanged));
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
            if (Path != Module.GetGitDirectory() ||
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

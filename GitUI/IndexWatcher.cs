using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GitUI
{
    public class IndexWatcher
    {
        public IndexWatcher()
        {
            if (GitIndexWatcher == null)
            {
                GitIndexWatcher = new FileSystemWatcher();
                RefsWatcher = new FileSystemWatcher();
                SetFileSystemWatcher();
            }

            IndexChanged = true;
            GitIndexWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
            RefsWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
        }

        private void SetFileSystemWatcher()
        {
            if (!string.IsNullOrEmpty(GitCommands.Settings.WorkingDirGitDir()))
            {
                try
                {
                    enabled = GitCommands.Settings.UseFastChecks;

                    Path = GitCommands.Settings.WorkingDirGitDir();

                    GitIndexWatcher.Path = GitCommands.Settings.WorkingDirGitDir();
                    GitIndexWatcher.Filter = "index";
                    GitIndexWatcher.IncludeSubdirectories = false;
                    GitIndexWatcher.EnableRaisingEvents = enabled;

                    RefsWatcher.Path = GitCommands.Settings.WorkingDirGitDir() + "\\refs";
                    RefsWatcher.IncludeSubdirectories = true;
                    RefsWatcher.EnableRaisingEvents = enabled;
                }
                catch
                {
                    enabled = false;
                }
            }
        }

        public event EventHandler Changed;

        private void OnChanged()
        {
            // If there are registered clients raise event
            if (Changed != null)
                Changed(this, new EventArgs());
        }

        private bool indexChanged;
        public bool IndexChanged 
        { 
            get
            {
                if (!enabled)
                    return true;

                if (Path != GitCommands.Settings.WorkingDirGitDir())
                    return true;

                return indexChanged;
            }
            set
            {
                indexChanged = value;
            }
        }

        static private bool enabled;
        static private string Path;
        static private FileSystemWatcher GitIndexWatcher { get; set; }
        static private FileSystemWatcher RefsWatcher { get; set; }

        void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            IndexChanged = true;
            OnChanged();
        }

        public void Reset()
        {
            if (Path != GitCommands.Settings.WorkingDirGitDir() ||
                enabled != GitCommands.Settings.UseFastChecks)
                SetFileSystemWatcher();

            IndexChanged = false;
        }

        public void Clear()
        {
            if (Path != GitCommands.Settings.WorkingDirGitDir() ||
                enabled != GitCommands.Settings.UseFastChecks)
                SetFileSystemWatcher();

            IndexChanged = true;
        }
    }
}

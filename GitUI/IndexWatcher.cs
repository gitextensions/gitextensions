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
            if (FileSystemWatcher == null)
            {
                FileSystemWatcher = new FileSystemWatcher();
                SetFileSystemWatcher();
            }

            IndexChanged = true;
            FileSystemWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
        }

        private void SetFileSystemWatcher()
        {
            if (!string.IsNullOrEmpty(GitCommands.Settings.WorkingDirGitDir()))
            {
                enabled = GitCommands.Settings.UseFastChecks;

                FileSystemWatcher.Path = GitCommands.Settings.WorkingDirGitDir();
                FileSystemWatcher.IncludeSubdirectories = true;
                FileSystemWatcher.EnableRaisingEvents = enabled;                    
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

                return indexChanged;
            }
            set
            {
                indexChanged = value;
            }
        }

        static private bool enabled;
        static private FileSystemWatcher FileSystemWatcher { get; set; }

        void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name != "config.lock" && e.Name != "config")
            {
                IndexChanged = true;
                OnChanged();
            }
        }

        public void Reset()
        {
            if (FileSystemWatcher.Path != GitCommands.Settings.WorkingDirGitDir() ||
                enabled != GitCommands.Settings.UseFastChecks)
                SetFileSystemWatcher();

            IndexChanged = false;
        }

        public void Clear()
        {
            if (FileSystemWatcher.Path != GitCommands.Settings.WorkingDirGitDir() ||
                enabled != GitCommands.Settings.UseFastChecks)
                SetFileSystemWatcher();

            IndexChanged = true;
        }
    }
}

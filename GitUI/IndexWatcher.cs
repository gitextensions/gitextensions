using System.IO;
using GitCommands;
using System;

namespace GitUI
{
    [Serializable]
    public delegate void IndexChangedEventHandler(bool indexChanged);

    public class IndexWatcher
    {
        public static event IndexChangedEventHandler Changed;

        public IndexWatcher()
        {
            if (GitIndexWatcher == null)
            {
                GitIndexWatcher = new FileSystemWatcher();
                RefsWatcher = new FileSystemWatcher();
                SetFileSystemWatcher();

                IndexChanged = true;
                GitIndexWatcher.Changed += fileSystemWatcher_Changed;
                RefsWatcher.Changed += fileSystemWatcher_Changed;
            }
        }

        private static void SetFileSystemWatcher()
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

        private static bool indexChanged;
        public static bool IndexChanged 
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
                GitIndexWatcher.EnableRaisingEvents = !IndexChanged;
            }
        }

        static private bool enabled;
        static private string Path;
        static private FileSystemWatcher GitIndexWatcher { get; set; }
        static private FileSystemWatcher RefsWatcher { get; set; }

        private static void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            IndexChanged = true;
            if (Changed != null)
                Changed(IndexChanged);
        }

        public static void Reset()
        {
            IndexChanged = false;
            if (Changed != null)
                Changed(IndexChanged);
            
            if (Path != GitCommands.Settings.WorkingDirGitDir() ||
                enabled != GitCommands.Settings.UseFastChecks)
                SetFileSystemWatcher();
        }

        public static void Clear()
        {
            IndexChanged = true;
            if (Changed != null)
                Changed(IndexChanged);

            if (Path != GitCommands.Settings.WorkingDirGitDir() ||
                enabled != GitCommands.Settings.UseFastChecks)
                SetFileSystemWatcher();
        }
    }
}

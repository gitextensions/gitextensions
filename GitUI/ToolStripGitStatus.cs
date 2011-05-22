using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public partial class ToolStripGitStatus : ToolStripMenuItem
    {
        private static readonly Bitmap ICON_CLEAN = Properties.Resources._81;
        private static readonly Bitmap ICON_DIRTY = Properties.Resources._82;
        private static readonly Bitmap ICON_STAGED = Properties.Resources._83;
        private static readonly Bitmap ICON_MIXED = Properties.Resources._84;

        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int UpdateDelay = 1000;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private GitCommandsInstance gitGetUnstagedCommand = new GitCommandsInstance();
        private readonly SynchronizationContext syncContext;
        private readonly FileSystemWatcher watcher = new FileSystemWatcher();
        private int nextUpdate;

        public ToolStripGitStatus()
        {
            syncContext = SynchronizationContext.Current;
            gitGetUnstagedCommand.Exited += (o, ea) => syncContext.Post(_ => onData(), null);

            InitializeComponent();

            Settings.WorkingDirChanged += Settings_WorkingDirChanged;

            GitUICommands.Instance.PreCheckoutBranch += GitUICommands_PreCheckout;
            GitUICommands.Instance.PreCheckoutRevision += GitUICommands_PreCheckout;
            GitUICommands.Instance.PostCheckoutBranch += GitUICommands_PostCheckout;
            GitUICommands.Instance.PostCheckoutRevision += GitUICommands_PostCheckout;

            // Setup a file watcher to detect changes to our files, or the .git repo files. When they
            // change, we'll update our status.
            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Changed;
            watcher.Deleted += watcher_Changed;
            watcher.Error += watcher_Error;
            watcher.IncludeSubdirectories = true;

            update();
            try
            {
                watcher.Path = Settings.WorkingDir;
                watcher.EnableRaisingEvents = true;
            }
            catch { }
        }


        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            Pause();
        }

        private void GitUICommands_PostCheckout(object sender, GitUIBaseEventArgs e)
        {
            Resume();
        }

        private void Pause()
        {
            timerRefresh.Stop();
        }

        private void Resume()
        {
            timerRefresh.Start();
        }

        void Settings_WorkingDirChanged(string oldDir, string newDir)
        {
            try
            {
                if (Directory.Exists(newDir))
                {
                    watcher.Path = newDir;
                    watcher.EnableRaisingEvents = true;
                }
                else
                {
                    watcher.EnableRaisingEvents = false;
                    Text = "=)";
                }


                nextUpdate = Math.Min(nextUpdate, Environment.TickCount + UpdateDelay);
            }
            catch { }
        }

        // destructor shouldn't be used because it's not predictible when
        // it's going to be called by the GC!
        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            nextUpdate = Math.Min(nextUpdate, Environment.TickCount + UpdateDelay);
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            const string repositoryDirectoryName = ".git";
            var isGitSelfChange = e.FullPath
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(directoryName => directoryName == repositoryDirectoryName);

            if (isGitSelfChange)
                return;

            nextUpdate = Math.Min(nextUpdate, Environment.TickCount + UpdateDelay);
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            update();
        }

        private void update()
        {
            // If the previous status call hasn't exited yet, we'll wait until it is
            // so we don't queue up a bunch of commands.
            if (gitGetUnstagedCommand.IsRunning)
            {
                return;
            }

            if (Environment.TickCount > nextUpdate)
            {
                string command = GitCommandHelpers.GetAllChangedFilesCmd(true, true);
                gitGetUnstagedCommand.CmdStartProcess(Settings.GitCommand, command);

                // Always update every 5 min, even if we don't know anything changed
                nextUpdate = Environment.TickCount + MaxUpdatePeriod;
            }
        }

        private void onData()
        {
            List<GitItemStatus> unstaged = GitCommandHelpers.GetAllChangedFilesFromString
                (
                gitGetUnstagedCommand.Output.ToString()
                );
            List<GitItemStatus> staged = GitCommandHelpers.GetStagedFiles();

            if (staged.Count == 0 && unstaged.Count == 0)
            {
                Image = ICON_CLEAN;
            }
            else
            {

                if (staged.Count == 0)
                {
                    Image = ICON_DIRTY;
                }
                else if (unstaged.Count == 0)
                {
                    Image = ICON_STAGED;
                }
                else
                {
                    Image = ICON_MIXED;
                }
            }

            Text = string.Format("{0} changes", staged.Count + unstaged.Count);
        }
    }
}

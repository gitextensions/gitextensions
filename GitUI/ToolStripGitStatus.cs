using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class ToolStripGitStatus : ToolStripMenuItem
    {
        private static readonly Bitmap ICON_CLEAN = global::GitUI.Properties.Resources._81;
        private static readonly Bitmap ICON_DIRTY = global::GitUI.Properties.Resources._82;
        private static readonly Bitmap ICON_STAGED = global::GitUI.Properties.Resources._83;
        private static readonly Bitmap ICON_MIXED = global::GitUI.Properties.Resources._84;

        // We often change several files at once. Wait a second so they're all changed
        // before we try to get the status
        private const int UPDATE_DELAY = 1000;

        // Update every 5min, just to make sure something didn't slip through the cracks.
        private const int MAX_UPDATE_PERIOD = 5 * 60 * 1000;

        private GitCommands.GitCommands gitGetUnstagedCommand = new GitCommands.GitCommands();
        private readonly SynchronizationContext syncContext;
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private int nextUpdate = 0;

        public ToolStripGitStatus()
        {
            syncContext = SynchronizationContext.Current;
            gitGetUnstagedCommand.Exited += new EventHandler(delegate(object o, EventArgs ea)
                {
                    syncContext.Post(_ => onData(), null);
                });

            InitializeComponent();

            // Setup a file watcher to detect changes to our files, or the .git repo files. When they
            // change, we'll update our status.
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Created += new FileSystemEventHandler(watcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(watcher_Changed);
            watcher.Error += new ErrorEventHandler(watcher_Error);
            watcher.IncludeSubdirectories = true;

            try
            {
                watcher.Path = Settings.WorkingDir; // TODO: Add a settings event to listen for change to working dir
                watcher.EnableRaisingEvents = true;
            }
            catch { }
            update();
        }

        ~ToolStripGitStatus()
        {
            gitGetUnstagedCommand.Kill();
        }

        void watcher_Error(object sender, System.IO.ErrorEventArgs e)
        {
            nextUpdate = Math.Min( nextUpdate, Environment.TickCount + UPDATE_DELAY );
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            nextUpdate = Math.Min(nextUpdate, Environment.TickCount + UPDATE_DELAY);
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            update();
        }

        private void update()
        {
            if (Environment.TickCount > nextUpdate)
            {
                string command = GitCommands.GitCommands.GetAllChangedFilesCmd(true, true);
                gitGetUnstagedCommand.CmdStartProcess(Settings.GitCommand, command);

                // Always update every 5 min, even if we don't know anything changed
                nextUpdate = Environment.TickCount + MAX_UPDATE_PERIOD;
            }
        }

        private void onData()
        {
            List<GitItemStatus> unstaged = GitCommands.GitCommands.GetAllChangedFilesFromString
                (
                gitGetUnstagedCommand.Output.ToString()
                );
            List<GitItemStatus> staged = GitCommands.GitCommands.GetStagedFiles();

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

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
    public sealed partial class ToolStripGitStatus : ToolStripMenuItem
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

        /// <summary>
        /// Paths to be ignored on filesystem changes watching.
        /// </summary>
        private static readonly string[] IgnoredPaths = new[] { @"\.git", @"\.git\index.lock" };

        private GitCommandsInstance gitGetUnstagedCommand = new GitCommandsInstance();
        private readonly SynchronizationContext syncContext;
        private readonly FileSystemWatcher watcher = new FileSystemWatcher();
        private int nextUpdateTime;
        private WorkingStatus currentStatus;
        private bool hasDeferredUpdateRequests;

        public ToolStripGitStatus()
        {
            syncContext = SynchronizationContext.Current;
            gitGetUnstagedCommand.Exited += (o, ea) => syncContext.Post(_ => onData(), null);

            InitializeComponent();

            Settings.WorkingDirChanged += (_, newDir) => TryStartWatchingChanges(newDir);

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

            TryStartWatchingChanges(Settings.WorkingDir);
        }


        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Paused;
        }

        private void GitUICommands_PostCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Started;
        }

        private void TryStartWatchingChanges(string watchingPath)
        {
            // reset status info, it was outdated
            Text = string.Empty;
            Image = null;

            try
            {
                if (!string.IsNullOrEmpty(watchingPath) && Directory.Exists(watchingPath))
                {
                    watcher.Path = watchingPath;
                    CurrentStatus = WorkingStatus.Started;
                }
                else
                {
                    CurrentStatus = WorkingStatus.Stopped;
                }
            }
            catch { }
        }

        // destructor shouldn't be used because it's not predictible when
        // it's going to be called by the GC!
        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            ScheduleNextRegularUpdate();
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var isGitSelfChange = IgnoredPaths.Any(path => e.FullPath.EndsWith(path));
            if (isGitSelfChange)
                return;

            ScheduleNextRegularUpdate();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            // If the previous status call hasn't exited yet, we'll wait until it is
            // so we don't queue up a bunch of commands
            if (gitGetUnstagedCommand.IsRunning)
            {
                hasDeferredUpdateRequests = true; // defer this update request
                return;
            }

            hasDeferredUpdateRequests = false;

            if (Environment.TickCount > nextUpdateTime)
            {
                string command = GitCommandHelpers.GetAllChangedFilesCmd(true, true);
                gitGetUnstagedCommand.CmdStartProcess(Settings.GitCommand, command);

                if (hasDeferredUpdateRequests)
                    // New changes were detected while processing previous changes, schedule deferred update
                    ScheduleDeferredUpdate();
                else
                    // Always update every 5 min, even if we don't know anything changed
                    ScheduleNextJustInCaseUpdate();
            }
        }

        private void onData()
        {
            if (CurrentStatus != WorkingStatus.Started)
                return;

            List<GitItemStatus> allChangedFiles =
                GitCommandHelpers.GetAllChangedFilesFromString(gitGetUnstagedCommand.Output.ToString());

            var stagedCount = allChangedFiles.FindAll(status => status.IsStaged).Count;
            var unstagedCount = allChangedFiles.FindAll(status => !status.IsStaged).Count;

            if (stagedCount == 0 && unstagedCount == 0)
            {
                Image = ICON_CLEAN;
            }
            else
            {

                if (stagedCount == 0)
                {
                    Image = ICON_DIRTY;
                }
                else if (unstagedCount == 0)
                {
                    Image = ICON_STAGED;
                }
                else
                {
                    Image = ICON_MIXED;
                }
            }

            Text = string.Format("{0} changes", allChangedFiles.Count);
        }

        private void ScheduleNextRegularUpdate()
        {
            nextUpdateTime = Math.Min(nextUpdateTime, Environment.TickCount + UpdateDelay);
            hasDeferredUpdateRequests = true;
        }

        private void ScheduleNextJustInCaseUpdate()
        {
            nextUpdateTime = Environment.TickCount + MaxUpdatePeriod;
        }

        private void ScheduleDeferredUpdate()
        {
            nextUpdateTime = Environment.TickCount + UpdateDelay;
        }

        private void ScheduleImmediateUpdate()
        {
            nextUpdateTime = 0;
            Update();
        }

        private WorkingStatus CurrentStatus
        {
            get { return currentStatus; }
            set
            {
                currentStatus = value;
                switch (currentStatus)
                {
                    case WorkingStatus.Stopped:
                        timerRefresh.Stop();
                        watcher.EnableRaisingEvents = false;
                        Visible = false;
                        return;
                    case WorkingStatus.Paused:
                        timerRefresh.Stop();
                        watcher.EnableRaisingEvents = false;
                        return;
                    case WorkingStatus.Started:
                        timerRefresh.Start();
                        watcher.EnableRaisingEvents = true;
                        ScheduleImmediateUpdate();
                        Visible = true;
                        return;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private enum WorkingStatus
        {
            Stopped,
            Paused,
            Started
        }
    }
}

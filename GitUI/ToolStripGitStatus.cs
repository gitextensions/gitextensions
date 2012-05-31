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
        private static readonly Bitmap ICON_CLEAN = Properties.Resources.IconClean;
        private static readonly Bitmap ICON_DIRTY = Properties.Resources.IconDirty;
        private static readonly Bitmap ICON_STAGED = Properties.Resources.IconStaged;
        private static readonly Bitmap ICON_MIXED = Properties.Resources.IconMixed;

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
        private readonly FileSystemWatcher gitDirWatcher = new FileSystemWatcher();
        private string gitPath;
        private int nextUpdateTime;
        private WorkingStatus currentStatus;
        private bool hasDeferredUpdateRequests;

        public string CommitTranslatedString { get; set; }

        public ToolStripGitStatus()
        {
            syncContext = SynchronizationContext.Current;
            gitGetUnstagedCommand.Exited += (o, ea) => syncContext.Post(_ => onData(), null);

            InitializeComponent();
            CommitTranslatedString = "Commit";

            Settings.WorkingDirChanged += (_, newDir, newGitDir) => TryStartWatchingChanges(newDir, newGitDir);

            GitUICommands.Instance.PreCheckoutBranch += GitUICommands_PreCheckout;
            GitUICommands.Instance.PreCheckoutRevision += GitUICommands_PreCheckout;
            GitUICommands.Instance.PostCheckoutBranch += GitUICommands_PostCheckout;
            GitUICommands.Instance.PostCheckoutRevision += GitUICommands_PostCheckout;

            // Setup a file watcher to detect changes to our files. When they
            // change, we'll update our status.
            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Changed;
            watcher.Deleted += watcher_Changed;
            watcher.Error += watcher_Error;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            // Setup a file watcher to detect changes to the .git repo files. When they
            // change, we'll update our status.
            gitDirWatcher.Changed += gitWatcher_Changed;
            gitDirWatcher.Created += gitWatcher_Changed;
            gitDirWatcher.Deleted += gitWatcher_Changed;
            gitDirWatcher.Error += watcher_Error;
            gitDirWatcher.IncludeSubdirectories = true;
            gitDirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            TryStartWatchingChanges(Settings.WorkingDir, Settings.Module.GetGitDirectory());
        }


        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Paused;
        }

        private void GitUICommands_PostCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Started;
        }

        private void TryStartWatchingChanges(string watchingPath, string watchingGitPath)
        {
            // reset status info, it was outdated
            Text = string.Empty;
            Image = null;

            try
            {
                if (!string.IsNullOrEmpty(watchingPath) && Directory.Exists(watchingPath) &&
                    !string.IsNullOrEmpty(watchingGitPath) && Directory.Exists(watchingGitPath))
                {
                    watcher.Path = watchingPath;
                    gitDirWatcher.Path = watchingGitPath;
                    gitPath = Path.GetDirectoryName(watchingGitPath);
                    CurrentStatus = WorkingStatus.Started;
                }
                else
                {
                    CurrentStatus = WorkingStatus.Stopped;
                }
            }
            catch { }
        }

        // destructor shouldn't be used because it's not predictable when
        // it's going to be called by the GC!
        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            ScheduleNextRegularUpdate();
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.StartsWith(gitPath))
            {
                gitWatcher_Changed(sender, e);
                return;
            }

            // new submodule .git file
            if (e.FullPath.EndsWith("\\.git"))
                return;

            // old submodule .git\index.lock file
            if (e.FullPath.EndsWith("\\.git\\index.lock"))
                return;

            ScheduleNextRegularUpdate();
        }

        private void gitWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // git directory changed
            if (e.FullPath.Length == gitPath.Length)
                return;

            if (e.FullPath.EndsWith("\\index.lock"))
                return;

            // new submodule changed
            const string modulePath = "\\modules\\";
            int index = e.FullPath.IndexOf(modulePath, gitPath.Length);
            if (index >= 0 && e.FullPath.IndexOf("\\", index + modulePath.Length) == -1)
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
                try
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
                catch (Exception)
                {
                    CurrentStatus = WorkingStatus.Stopped;
                }
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

            if (allChangedFiles.Count == 0)
                Text = CommitTranslatedString;
            else
                Text = string.Format(CommitTranslatedString + " ({0})", allChangedFiles.Count.ToString());
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
            nextUpdateTime = Environment.TickCount;
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
                        gitDirWatcher.EnableRaisingEvents = false;
                        Visible = false;
                        return;
                    case WorkingStatus.Paused:
                        timerRefresh.Stop();
                        watcher.EnableRaisingEvents = false;
                        gitDirWatcher.EnableRaisingEvents = false;
                        return;
                    case WorkingStatus.Started:
                        timerRefresh.Start();
                        watcher.EnableRaisingEvents = true;
                        gitDirWatcher.EnableRaisingEvents = !gitDirWatcher.Path.StartsWith(watcher.Path);
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

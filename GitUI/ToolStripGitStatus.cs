﻿using System;
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
        private static readonly Bitmap IconClean = Properties.Resources.IconClean;
        private static readonly Bitmap IconDirty = Properties.Resources.IconDirty;
        private static readonly Bitmap IconDirtySubmodules = Properties.Resources.IconDirtySubmodules;
        private static readonly Bitmap IconStaged = Properties.Resources.IconStaged;
        private static readonly Bitmap IconMixed = Properties.Resources.IconMixed;

        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int UpdateDelay = 500;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private bool statusIsUpToDate = true;
        private readonly SynchronizationContext syncContext;
        private readonly FileSystemWatcher workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher gitDirWatcher = new FileSystemWatcher();
        private string gitPath, submodulesPath;
        private int nextUpdateTime;
        private WorkingStatus currentStatus;
        private HashSet<string> ignoredFiles = new HashSet<string>(); 

        public string CommitTranslatedString { get; set; }

        private IGitUICommandsSource _UICommandsSource;
        public IGitUICommandsSource UICommandsSource
        {
            get
            {
                return _UICommandsSource;
            }

            set
            {
                _UICommandsSource = value;
                _UICommandsSource.GitUICommandsChanged += GitUICommandsChanged;
                GitUICommandsChanged(UICommandsSource, null);
            }
        }
        
        public GitUICommands UICommands 
        {
            get
            {
                return UICommandsSource.UICommands;
            }
        }

        public GitModule Module 
        {
            get
            {
                return UICommands.Module;
            }
        }

        public ToolStripGitStatus()
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();
            components.Add(workTreeWatcher);
            components.Add(gitDirWatcher);
            CommitTranslatedString = "Commit";
            ignoredFilesTimer.Interval = MaxUpdatePeriod;
            CurrentStatus = WorkingStatus.Stopped;

            // Setup a file watcher to detect changes to our files. When they
            // change, we'll update our status.
            workTreeWatcher.EnableRaisingEvents = false;
            workTreeWatcher.Changed += WorkTreeChanged;
            workTreeWatcher.Created += WorkTreeChanged;
            workTreeWatcher.Deleted += WorkTreeChanged;
            workTreeWatcher.Renamed += WorkTreeChanged;
            workTreeWatcher.Error += WorkTreeWatcherError;
            workTreeWatcher.IncludeSubdirectories = true;
            workTreeWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            // Setup a file watcher to detect changes to the .git repo files. When they
            // change, we'll update our status.
            gitDirWatcher.EnableRaisingEvents = false;
            gitDirWatcher.Changed += GitDirChanged;
            gitDirWatcher.Created += GitDirChanged;
            gitDirWatcher.Deleted += GitDirChanged;
            gitDirWatcher.Error += WorkTreeWatcherError;
            gitDirWatcher.IncludeSubdirectories = true;
            gitDirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;            
        }

        private void GitUICommandsChanged(IGitUICommandsSource source, GitUICommands oldCommands)
        {
            if (oldCommands != null)
            {
                oldCommands.PreCheckoutBranch -= GitUICommands_PreCheckout;
                oldCommands.PreCheckoutRevision -= GitUICommands_PreCheckout;
                oldCommands.PostCheckoutBranch -= GitUICommands_PostCheckout;
                oldCommands.PostCheckoutRevision -= GitUICommands_PostCheckout;
                oldCommands.PostEditGitIgnore -= GitUICommands_PostEditGitIgnore;
            }

            if (UICommands != null)
            {
                UICommands.PreCheckoutBranch += GitUICommands_PreCheckout;
                UICommands.PreCheckoutRevision += GitUICommands_PreCheckout;
                UICommands.PostCheckoutBranch += GitUICommands_PostCheckout;
                UICommands.PostCheckoutRevision += GitUICommands_PostCheckout;
                UICommands.PostEditGitIgnore += GitUICommands_PostEditGitIgnore;
                
                TryStartWatchingChanges(Module.WorkingDir, Module.GetGitDirectory());
            }
        }

        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Paused;
        }

        private void GitUICommands_PostCheckout(object sender, GitUIPostActionEventArgs e)
        {
            CurrentStatus = WorkingStatus.Started;
        }

        private void GitUICommands_PostEditGitIgnore(object sender, GitUIBaseEventArgs e)
        {
            UpdateIgnoredFiles(true);
        }
        
        private void TryStartWatchingChanges(string workTreePath, string gitDirPath)
        {
            // reset status info, it was outdated
            Text = CommitTranslatedString;
            Image = IconClean;

            try
            {
                if (!string.IsNullOrEmpty(workTreePath) && Directory.Exists(workTreePath) &&
                    !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath))
                {
                    workTreeWatcher.Path = workTreePath;
                    gitDirWatcher.Path = gitDirPath;
                    gitPath = Path.GetDirectoryName(gitDirPath);
                    submodulesPath = Path.Combine(gitPath, "modules");
                    UpdateIgnoredFiles(true);
                    ignoredFilesTimer.Stop();
                    ignoredFilesTimer.Start();
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
        private void WorkTreeWatcherError(object sender, ErrorEventArgs e)
        {
            ScheduleDeferredUpdate();
        }

        private void WorkTreeChanged(object sender, FileSystemEventArgs e)
        {
            var fileName = GitCommandHelpers.FixPath(e.FullPath.Substring(workTreeWatcher.Path.Length));
            if (ignoredFiles.Contains(fileName))
                return;

            if (e.FullPath.StartsWith(gitPath))
            {
                GitDirChanged(sender, e);
                return;
            }

            // new submodule .git file
            if (e.FullPath.EndsWith("\\.git"))
                return;

            // old submodule .git\index.lock file
            if (e.FullPath.EndsWith("\\.git\\index.lock"))
                return;

            ScheduleDeferredUpdate();
        }

        private void GitDirChanged(object sender, FileSystemEventArgs e)
        {
            // git directory changed
            if (e.FullPath.Length == gitPath.Length)
                return;

            if (e.FullPath.EndsWith("\\index.lock"))
                return;

            // submodules directory's subdir changed
            // cut/paste/rename/delete operations are not expected on directories inside nested .git dirs
            if (e.FullPath.StartsWith(submodulesPath) && (Directory.Exists(e.FullPath)))
                return;

            ScheduleDeferredUpdate();
        }

        private HashSet<string> LoadIgnoredFiles()
        { 
            string lsOutput = Module.RunGitCmd("ls-files -o -i --exclude-standard");
            string[] tab = lsOutput.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(tab);
        }

        private void UpdateIgnoredFiles(bool clearImmediately)
        {
            if (clearImmediately)
                ignoredFiles = new HashSet<string>();

            AsyncLoader.DoAsync(
                LoadIgnoredFiles, 
                (ignoredSet) => { ignoredFiles = ignoredSet; },
                (e) => { ignoredFiles = new HashSet<string>(); }
                );   
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (CurrentStatus != WorkingStatus.Started)
                return;

            if (Environment.TickCount >= nextUpdateTime || 
                (Environment.TickCount < 0 && nextUpdateTime > 0))
            {
                // If the previous status call hasn't exited yet, we'll wait until it is
                // so we don't queue up a bunch of commands
                if (Module.IsRunningGitProcess())
                {
                    statusIsUpToDate = false;//tell that computed status isn't up to date
                    return;
                }

                statusIsUpToDate = true;
                AsyncLoader.DoAsync(RunStatusCommand, UpdatedStatusReceived, (e) => { CurrentStatus = WorkingStatus.Stopped; });
                // Always update every 5 min, even if we don't know anything changed
                ScheduleNextJustInCaseUpdate();
            }
        }

        private String RunStatusCommand()
        {
            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, true);
            return Module.RunGitCmd(command);
        }

        private void UpdatedStatusReceived(string updatedStatus)
        {
            if (CurrentStatus != WorkingStatus.Started)
                return;

            if (statusIsUpToDate)
            {
                var allChangedFiles = GitCommandHelpers.GetAllChangedFilesFromString(Module, updatedStatus);
                var stagedCount = allChangedFiles.Count(status => status.IsStaged);
                var unstagedCount = allChangedFiles.Count - stagedCount;
                var unstagedSubmodulesCount = allChangedFiles.Count(status => status.IsSubmodule && !status.IsStaged);

                Image = GetStatusIcon(stagedCount, unstagedCount, unstagedSubmodulesCount);

                if (allChangedFiles.Count == 0)
                    Text = CommitTranslatedString;
                else
                    Text = string.Format(CommitTranslatedString + " ({0})", allChangedFiles.Count.ToString());
            }
            else
                UpdateImmediately();
        }

        private static Image GetStatusIcon(int stagedCount, int unstagedCount, int unstagedSubmodulesCount)
        {
            if (stagedCount == 0 && unstagedCount == 0)
                return IconClean;

            if (stagedCount == 0)
                return unstagedCount != unstagedSubmodulesCount ? IconDirty : IconDirtySubmodules;

            return unstagedCount == 0 ? IconStaged : IconMixed;
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
        }

        private void UpdateImmediately()
        {
            ScheduleImmediateUpdate();
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
                        workTreeWatcher.EnableRaisingEvents = false;
                        gitDirWatcher.EnableRaisingEvents = false;
                        Visible = false;
                        return;
                    case WorkingStatus.Paused:
                        timerRefresh.Stop();
                        workTreeWatcher.EnableRaisingEvents = false;
                        gitDirWatcher.EnableRaisingEvents = false;
                        return;
                    case WorkingStatus.Started:
                        timerRefresh.Start();
                        workTreeWatcher.EnableRaisingEvents = true;
                        gitDirWatcher.EnableRaisingEvents = !gitDirWatcher.Path.StartsWith(workTreeWatcher.Path);
                        UpdateImmediately();
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

        private void ignoredFilesTimer_Tick(object sender, EventArgs e)
        {
            UpdateIgnoredFiles(false);
        }
    }
}

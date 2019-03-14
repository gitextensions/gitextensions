using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Git;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Submodules
{
    public interface ISubmoduleStatusProvider : IDisposable
    {
        event EventHandler StatusUpdating;
        event EventHandler<SubmoduleStatusEventArgs> StatusUpdated;
        void Init();
        bool HasChangedToNone([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles);
        bool HasStatusChanges([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles);
        void UpdateSubmodulesStatus(bool updateStatus, string workingDirectory, string noBranchText);
    }

    public sealed class SubmoduleStatusProvider : ISubmoduleStatusProvider
    {
        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private DateTime _previousSubmoduleUpdateTime;
        private bool _previousSubmoduleHadChanges;

        // Singleton accessor
        public static SubmoduleStatusProvider Default { get; } = new SubmoduleStatusProvider();

        // Invoked when status update is requested (use to clear/lock UI)
        public event EventHandler StatusUpdating;

        // Invoked when status update is complete
        public event EventHandler<SubmoduleStatusEventArgs> StatusUpdated;

        public void Dispose()
        {
            _submodulesStatusSequence.Dispose();
        }

        public void Init()
        {
            // Cancel any previous async activities:
            var cancelToken = _submodulesStatusSequence.Next();
            _previousSubmoduleHadChanges = false;
        }

        public bool HasChangedToNone([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            if (allChangedFiles == null)
            {
                return false;
            }

            bool anyUpdate = allChangedFiles.Any(i => i.IsSubmodule && (i.IsChanged || !i.IsTracked));

            // If status is changed to none, the status must be cleared
            bool result = _previousSubmoduleHadChanges && !anyUpdate;
            _previousSubmoduleHadChanges = anyUpdate;
            return result;
        }

        public bool HasStatusChanges([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            TimeSpan elapsed = DateTime.Now - _previousSubmoduleUpdateTime;

            // Throttle updates triggered from status updates
            if (allChangedFiles == null || elapsed.TotalSeconds <= 15)
            {
                return false;
            }

            // If any submodules are changed, trigger an update
            // TBD: This should check the status against last updated list and only update the required modules
            // (maybe even ignore count)
            return allChangedFiles.Any(i => i.IsSubmodule && (i.IsChanged || !i.IsTracked));
        }

        public void UpdateSubmodulesStatus(bool updateStatus, string workingDirectory, string noBranchText)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // Cancel any previous async activities:
                var cancelToken = _submodulesStatusSequence.Next();

                // If not updating the status, allow a 'quick' update
                _previousSubmoduleUpdateTime = updateStatus ? DateTime.Now : DateTime.MinValue;

                OnStatusUpdating();

                await TaskScheduler.Default;

                // Start gathering new submodule information asynchronously.  This makes a significant difference in UI
                // responsiveness if there are numerous submodules (e.g. > 100).
                // First task: Gather list of submodules on a background thread.

                // Don't access Module directly because it's not thread-safe.  Use a thread-local version:
                var threadModule = new GitModule(workingDirectory);
                var result = new SubmoduleInfoResult
                {
                    Module = threadModule
                };

                // Add all submodules inside the current repository:
                var submodulesTask = GetRepositorySubmodulesStatusAsync(updateStatus, result, threadModule, cancelToken, noBranchText);

                var superTask = GetSuperProjectRepositorySubmodulesStatusAsync(updateStatus, result, threadModule, cancelToken, noBranchText);

                await Task.WhenAll(submodulesTask, superTask);

                OnStatusUpdated(result, cancelToken);

                _previousSubmoduleUpdateTime = updateStatus ? DateTime.Now : DateTime.MinValue;
            }).FileAndForget();
        }

        private void OnStatusUpdating()
        {
            StatusUpdating?.Invoke(this, EventArgs.Empty);
        }

        private void OnStatusUpdated(SubmoduleInfoResult info, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            StatusUpdated?.Invoke(this, new SubmoduleStatusEventArgs(info, token));
        }

        private async Task GetRepositorySubmodulesStatusAsync(bool updateStatus, SubmoduleInfoResult result, IGitModule module, CancellationToken cancelToken, string noBranchText)
        {
            await TaskScheduler.Default;

            foreach (var submodule in module.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName))
            {
                cancelToken.ThrowIfCancellationRequested();
                var name = submodule;
                string path = module.GetSubmoduleFullPath(submodule);
                if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                {
                    name = name + " " + GetModuleBranch(path, noBranchText);
                }

                var smi = new SubmoduleInfo { Text = name, Path = path };
                result.OurSubmodules.Add(smi);
                GetSubmoduleStatus(updateStatus, smi, cancelToken);
            }
        }

        private void GetSubmoduleStatus(bool updateStatus, SubmoduleInfo info, CancellationToken cancelToken)
        {
            if (!updateStatus)
            {
                return;
            }

            cancelToken.ThrowIfCancellationRequested();

            var submodule = new GitModule(info.Path);
            var supermodule = submodule.SuperprojectModule;
            var submoduleName = submodule.GetCurrentSubmoduleLocalPath();

            if (string.IsNullOrEmpty(submoduleName) || supermodule == null)
            {
                return;
            }

            info.Detailed = new AsyncLazy<DetailedSubmoduleInfo>(async () =>
            {
                cancelToken.ThrowIfCancellationRequested();

                var submoduleStatus = await GitCommandHelpers.GetCurrentSubmoduleChangesAsync(supermodule, submoduleName, noLocks: true).ConfigureAwait(false);
                if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
                {
                    submoduleStatus.CheckSubmoduleStatus(submoduleStatus.GetSubmodule(supermodule));
                }

                if (submoduleStatus != null)
                {
                    return new DetailedSubmoduleInfo()
                    {
                        Status = submoduleStatus.Status,
                        IsDirty = submoduleStatus.IsDirty,
                        AddedAndRemovedText = submoduleStatus.AddedAndRemovedString()
                    };
                }

                return null;
            }, ThreadHelper.JoinableTaskFactory);
        }

        private async Task GetSuperProjectRepositorySubmodulesStatusAsync(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText)
        {
            // This function always at least sets result.TopProject

            bool isCurrentTopProject = module.SuperprojectModule == null;
            if (isCurrentTopProject)
            {
                string path = module.WorkingDir;
                string name = Path.GetFileName(Path.GetDirectoryName(module.WorkingDir)) + GetBranchNameSuffix(path, noBranchText);
                result.TopProject = new SubmoduleInfo { Text = name, Path = module.WorkingDir, Bold = true };
                return;
            }

            IGitModule topProject = module.SuperprojectModule.GetTopModule();

            bool isParentTopProject = module.SuperprojectModule.WorkingDir == topProject.WorkingDir;

            // Set result.SuperProject
            SetSuperProjectSubmoduleInfo(updateStatus, result, module, cancelToken, noBranchText, topProject, isParentTopProject);

            // Set result.TopProject
            await SetTopProjectSubmoduleInfoAsync(updateStatus, result, module, cancelToken, noBranchText, topProject, isParentTopProject);

            // Set result.CurrentSubmoduleName and populate result.SuperSubmodules
            await SetSubmoduleDataAsync(updateStatus, result, module, cancelToken, noBranchText, topProject);
        }

        private void SetSuperProjectSubmoduleInfo(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText, IGitModule topProject, bool isParentTopProject)
        {
            string name;
            if (isParentTopProject)
            {
                name = Path.GetFileName(Path.GetDirectoryName(topProject.WorkingDir));
            }
            else
            {
                var localPath = module.SuperprojectModule.WorkingDir.Substring(topProject.WorkingDir.Length);
                localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());
                name = localPath;
            }

            string path = module.SuperprojectModule.WorkingDir;
            name += GetBranchNameSuffix(path, noBranchText);
            result.SuperProject = new SubmoduleInfo { Text = name, Path = module.SuperprojectModule.WorkingDir };
            GetSubmoduleStatus(updateStatus, result.SuperProject, cancelToken);
        }

        private async Task SetTopProjectSubmoduleInfoAsync(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText, IGitModule topProject, bool isParentTopProject)
        {
            await TaskScheduler.Default;

            if (isParentTopProject)
            {
                result.TopProject = result.SuperProject;
            }
            else
            {
                string path = topProject.WorkingDir;
                string name = Path.GetFileName(Path.GetDirectoryName(topProject.WorkingDir)) + GetBranchNameSuffix(path, noBranchText);
                result.TopProject = new SubmoduleInfo { Text = name, Path = topProject.WorkingDir };
                GetSubmoduleStatus(updateStatus, result.TopProject, cancelToken);
            }
        }

        private async Task SetSubmoduleDataAsync(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText, IGitModule topProject)
        {
            await TaskScheduler.Default;

            var submodules = topProject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName).ToArray();
            if (submodules.Any())
            {
                string localPath = module.WorkingDir.Substring(topProject.WorkingDir.Length);
                localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());

                foreach (var submodule in submodules)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    string path = topProject.GetSubmoduleFullPath(submodule);
                    string name = submodule + GetBranchNameSuffix(path, noBranchText);

                    bool bold = false;
                    if (submodule == localPath)
                    {
                        result.CurrentSubmoduleName = module.GetCurrentSubmoduleLocalPath();
                        bold = true;
                    }

                    var smi = new SubmoduleInfo { Text = name, Path = path, Bold = bold };
                    result.SuperSubmodules.Add(smi);
                    GetSubmoduleStatus(updateStatus, smi, cancelToken);
                }
            }
        }

        private string GetBranchNameSuffix(string repositoryPath, string noBranchText)
        {
            if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(repositoryPath))
            {
                return " " + GetModuleBranch(repositoryPath, noBranchText);
            }

            return string.Empty;
        }

        private static string GetModuleBranch(string path, string noBranchText)
        {
            var branch = GitModule.GetSelectedBranchFast(path);
            var text = DetachedHeadParser.IsDetachedHead(branch) ? noBranchText : branch;
            return $"[{text}]";
        }
    }
}
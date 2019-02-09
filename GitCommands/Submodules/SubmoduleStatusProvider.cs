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
        void Init();
        bool HasChangedToNone([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles);
        bool HasStatusChanges([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles);
        void UpdateSubmodulesStatus(bool updateStatus, string workingDirectory, string noBranchText, Action onUpdateBegin, Func<SubmoduleInfoResult, CancellationToken, Task> onUpdateCompleteAsync);
    }

    public sealed class SubmoduleStatusProvider : ISubmoduleStatusProvider
    {
        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private DateTime _previousSubmoduleUpdateTime;
        private bool _previousSubmoduleHadChanges;

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

        public void UpdateSubmodulesStatus(bool updateStatus, string workingDirectory,
            string noBranchText,
            Action onUpdateBegin,
            Func<SubmoduleInfoResult, CancellationToken, Task> onUpdateCompleteAsync)
        {
            // Cancel any previous async activities:
            var cancelToken = _submodulesStatusSequence.Next();

            // If not updating the status, allow a 'quick' update
            _previousSubmoduleUpdateTime = updateStatus ? DateTime.Now : DateTime.MinValue;

            onUpdateBegin();

            // Start gathering new submodule information asynchronously.  This makes a significant difference in UI
            // responsiveness if there are numerous submodules (e.g. > 100).
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // First task: Gather list of submodules on a background thread.

                // Don't access Module directly because it's not thread-safe.  Use a thread-local version:
                var threadModule = new GitModule(workingDirectory);
                var result = new SubmoduleInfoResult();

                // Add all submodules inside the current repository:
                var submodulesTask = GetRepositorySubmodulesStatusAsync(updateStatus, result, threadModule, cancelToken, noBranchText);

                var superTask = GetSuperProjectRepositorySubmodulesStatusAsync(updateStatus, result, threadModule, cancelToken, noBranchText);

                await Task.WhenAll(submodulesTask, superTask);

                await onUpdateCompleteAsync(result, cancelToken);

                _previousSubmoduleUpdateTime = updateStatus ? DateTime.Now : DateTime.MinValue;
            }).FileAndForget();
        }

        [NotNull]
        private static GitModule FindTopProjectModule(GitModule superModule)
        {
            GitModule topSuperModule = superModule;
            do
            {
                if (topSuperModule.SuperprojectModule == null)
                {
                    break;
                }

                topSuperModule = topSuperModule.SuperprojectModule;
            }
            while (topSuperModule != null);

            return topSuperModule;
        }

        private async Task GetRepositorySubmodulesStatusAsync(bool updateStatus, SubmoduleInfoResult result, IGitModule module, CancellationToken cancelToken, string noBranchText)
        {
            List<Task> tasks = new List<Task>();
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
                tasks.Add(GetSubmoduleStatusAsync(updateStatus, smi, cancelToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task GetSubmoduleStatusAsync(bool updateStatus, SubmoduleInfo info, CancellationToken cancelToken)
        {
            if (!updateStatus)
            {
                return;
            }

            await TaskScheduler.Default;
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
                await TaskScheduler.Default;
                cancelToken.ThrowIfCancellationRequested();

                var submoduleStatus = GitCommandHelpers.GetCurrentSubmoduleChanges(supermodule, submoduleName, noLocks: true);
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

            IGitModule topProject = FindTopProjectModule(module.SuperprojectModule);
            bool isParentTopProject = module.SuperprojectModule.WorkingDir == topProject.WorkingDir;

            // Set result.SuperProject
            await SetSuperProjectSubmoduleInfoAsync(updateStatus, result, module, cancelToken, noBranchText, topProject, isParentTopProject);

            // Set result.TopProject
            await SetTopProjectSubmoduleInfoAsync(updateStatus, result, module, cancelToken, noBranchText, topProject, isParentTopProject);

            // Set result.CurrentSubmoduleName and populate result.SuperSubmodules
            await SetSubmoduleDataAsync(updateStatus, result, module, cancelToken, noBranchText, topProject);
        }

        private async Task SetSuperProjectSubmoduleInfoAsync(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText, IGitModule topProject, bool isParentTopProject)
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
            await GetSubmoduleStatusAsync(updateStatus, result.SuperProject, cancelToken);
        }

        private async Task SetTopProjectSubmoduleInfoAsync(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText, IGitModule topProject, bool isParentTopProject)
        {
            if (isParentTopProject)
            {
                result.TopProject = result.SuperProject;
            }
            else
            {
                string path = topProject.WorkingDir;
                string name = Path.GetFileName(Path.GetDirectoryName(topProject.WorkingDir)) + GetBranchNameSuffix(path, noBranchText);
                result.TopProject = new SubmoduleInfo { Text = name, Path = topProject.WorkingDir };
                await GetSubmoduleStatusAsync(updateStatus, result.TopProject, cancelToken);
            }
        }

        private async Task SetSubmoduleDataAsync(bool updateStatus, SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText, IGitModule topProject)
        {
            var submodules = topProject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName).ToArray();
            if (submodules.Any())
            {
                string localPath = module.WorkingDir.Substring(topProject.WorkingDir.Length);
                localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());

                List<Task> subTasks = new List<Task>();

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
                    subTasks.Add(GetSubmoduleStatusAsync(updateStatus, smi, cancelToken));
                }

                await Task.WhenAll(subTasks);
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
using System;
using System.Collections.Generic;
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
        bool HasSubmodulesStatusChanged([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles);
        void UpdateSubmodulesStatus(string workingDirectory, string noBranchText, Action onUpdateBegin, Func<SubmoduleInfoResult, CancellationToken, Task> onUpdateCompleteAsync);
    }

    public sealed class SubmoduleStatusProvider : ISubmoduleStatusProvider
    {
        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private DateTime _previousSubmoduleUpdateTime;

        public void Dispose()
        {
            _submodulesStatusSequence.Dispose();
        }

        public bool HasSubmodulesStatusChanged([CanBeNull] IReadOnlyList<GitItemStatus> allChangedFiles)
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

        public void UpdateSubmodulesStatus(string workingDirectory,
            string noBranchText,
            Action onUpdateBegin,
            Func<SubmoduleInfoResult, CancellationToken, Task> onUpdateCompleteAsync)
        {
            // Cancel any previous async activities:
            var cancelToken = _submodulesStatusSequence.Next();
            _previousSubmoduleUpdateTime = DateTime.Now;

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
                var submodulesTask = GetRepositorySubmodulesStatusAsync(result, threadModule, cancelToken, noBranchText);

                var superTask = GetSuperProjectRepositorySubmodulesStatusAsync(result, threadModule, cancelToken, noBranchText);

                await Task.WhenAll(submodulesTask, superTask);

                await onUpdateCompleteAsync(result, cancelToken);

                _previousSubmoduleUpdateTime = DateTime.Now;
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

        private async Task GetRepositorySubmodulesStatusAsync(SubmoduleInfoResult result, IGitModule module, CancellationToken cancelToken, string noBranchText)
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
                tasks.Add(GetSubmoduleStatusAsync(smi, cancelToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task GetSubmoduleStatusAsync(SubmoduleInfo info, CancellationToken cancelToken)
        {
            if (!AppSettings.ShowSubmoduleStatus)
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

                var submoduleStatus = GitCommandHelpers.GetCurrentSubmoduleChanges(supermodule, submoduleName);
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

        private async Task GetSuperProjectRepositorySubmodulesStatusAsync(SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText)
        {
            if (module.SuperprojectModule == null)
            {
                return;
            }

            string name, path;
            GitModule supersuperproject = FindTopProjectModule(module.SuperprojectModule);
            if (module.SuperprojectModule.WorkingDir != supersuperproject.WorkingDir)
            {
                name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
                path = supersuperproject.WorkingDir;
                if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                {
                    name = name + " " + GetModuleBranch(path, noBranchText);
                }

                result.TopProject = new SubmoduleInfo { Text = name, Path = supersuperproject.WorkingDir };
                await GetSubmoduleStatusAsync(result.TopProject, cancelToken);
            }

            if (module.SuperprojectModule.WorkingDir != supersuperproject.WorkingDir)
            {
                var localPath = module.SuperprojectModule.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());
                name = localPath;
            }
            else
            {
                name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
            }

            path = module.SuperprojectModule.WorkingDir;
            if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
            {
                name = name + " " + GetModuleBranch(path, noBranchText);
            }

            result.SuperProject = new SubmoduleInfo { Text = name, Path = module.SuperprojectModule.WorkingDir };
            await GetSubmoduleStatusAsync(result.SuperProject, cancelToken);

            var submodules = supersuperproject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName).ToArray();
            if (submodules.Any())
            {
                string localPath = module.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());

                List<Task> subTasks = new List<Task>();

                foreach (var submodule in submodules)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    name = submodule;
                    path = supersuperproject.GetSubmoduleFullPath(submodule);
                    if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                    {
                        name = name + " " + GetModuleBranch(path, noBranchText);
                    }

                    bool bold = false;
                    if (submodule == localPath)
                    {
                        result.CurrentSubmoduleName = module.GetCurrentSubmoduleLocalPath();
                        bold = true;
                    }

                    var smi = new SubmoduleInfo { Text = name, Path = path, Bold = bold };
                    result.SuperSubmodules.Add(smi);
                    subTasks.Add(GetSubmoduleStatusAsync(smi, cancelToken));
                }

                await Task.WhenAll(subTasks);
            }
        }

        private static string GetModuleBranch(string path, string noBranchText)
        {
            var branch = GitModule.GetSelectedBranchFast(path);
            var text = DetachedHeadParser.IsDetachedHead(branch) ? noBranchText : branch;
            return $"[{text}]";
        }
    }
}
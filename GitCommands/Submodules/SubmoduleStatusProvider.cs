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

        /// <summary>
        /// Update the submodule structure; find superprojects and submodules
        /// </summary>
        /// <param name="workingDirectory">Current module working directory</param>
        /// <param name="noBranchText">The text where no branch is checked out for the submodule</param>
        /// <param name="updateStatus">Update the detailed submodule status (set when current module is not top project)</param>
        void UpdateSubmodulesStructure(string workingDirectory, string noBranchText, bool updateStatus);

        /// <summary>
        /// Update the submodule status
        /// </summary>
        /// <param name="workingDirectory">Current module working directory</param>
        /// <param name="gitStatus">The Git status for the changes (also other than submodules)</param>
        void UpdateSubmodulesStatus(string workingDirectory, [CanBeNull] IReadOnlyList<GitItemStatus> gitStatus);
    }

    public sealed class SubmoduleStatusProvider : ISubmoduleStatusProvider
    {
        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private DateTime _previousSubmoduleUpdateTime;
        private SubmoduleInfoResult _submoduleInfoResult;
        private readonly Dictionary<string, SubmoduleInfo> _submoduleInfos = new Dictionary<string, SubmoduleInfo>();
        private IReadOnlyList<GitItemStatus> _gitStatusWhileUpdatingStructure;

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
            _submodulesStatusSequence.Next();
        }

        /// <inheritdoc />
        public void UpdateSubmodulesStructure(string workingDirectory, string noBranchText, bool updateStatus)
        {
            _submoduleInfoResult = null;
            _gitStatusWhileUpdatingStructure = null;
            _submoduleInfos.Clear();
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // Cancel any previous async activities:
                var cancelToken = _submodulesStatusSequence.Next();

                // Do not throttle next status update
                _previousSubmoduleUpdateTime = DateTime.MinValue;

                OnStatusUpdating();

                await TaskScheduler.Default;

                // Start gathering new submodule structure asynchronously.
                var currentModule = new GitModule(workingDirectory);
                var result = new SubmoduleInfoResult
                {
                    Module = currentModule
                };

                // Add all submodules inside the current repository:
                GetRepositorySubmodulesStructure(result, noBranchText);
                GetSuperProjectRepositorySubmodulesStructure(currentModule, result, noBranchText);

                // Structure is updated
                OnStatusUpdated(result, cancelToken);

                // Prepare info for status updates (normally triggered by StatusMonitor)
                foreach (var info in result.OurSubmodules)
                {
                    _submoduleInfos[info.Path] = info;
                }

                foreach (var info in result.SuperSubmodules)
                {
                    _submoduleInfos[info.Path] = info;
                }

                // Start update status for the submodules
                if (updateStatus)
                {
                    if (result.SuperProject != null)
                    {
                        // Update from top module (will stop at current)
                        await GetSubmoduleDetailedStatusAsync(currentModule.GetTopModule(), cancelToken);
                    }

                    if (_gitStatusWhileUpdatingStructure != null)
                    {
                        // Current module must be updated separetly (not in _submoduleInfos)
                        await UpdateSubmodulesStatusAsync(currentModule, _gitStatusWhileUpdatingStructure, cancelToken);
                    }

                    OnStatusUpdated(result, cancelToken);
                }

                _submoduleInfoResult = result;
            }).FileAndForget();
        }

        /// <inheritdoc />
        public void UpdateSubmodulesStatus(string workingDirectory, [CanBeNull] IReadOnlyList<GitItemStatus> gitStatus)
        {
            if (_submoduleInfoResult == null)
            {
                _gitStatusWhileUpdatingStructure = gitStatus;
                return;
            }

            // Throttle updates triggered from status updates
            TimeSpan elapsed = DateTime.Now - _previousSubmoduleUpdateTime;
            if (gitStatus == null || elapsed.TotalSeconds <= 15)
            {
                return;
            }

            var cancelToken = _submodulesStatusSequence.Next();
            _gitStatusWhileUpdatingStructure = null;
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                var currentModule = new GitModule(workingDirectory);
                await UpdateSubmodulesStatusAsync(currentModule, gitStatus, cancelToken);

                OnStatusUpdated(_submoduleInfoResult, cancelToken);
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

        private void GetRepositorySubmodulesStructure(SubmoduleInfoResult result, string noBranchText)
        {
            foreach (var submodule in result.Module.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName))
            {
                var name = submodule;
                string path = result.Module.GetSubmoduleFullPath(submodule);
                if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                {
                    name = name + " " + GetModuleBranch(path, noBranchText);
                }

                var smi = new SubmoduleInfo { Text = name, Path = path };
                result.OurSubmodules.Add(smi);
            }
        }

        /// <summary>
        /// This function always at least sets result.TopProject
        /// </summary>
        /// <param name="result">submodule info</param>
        /// <param name="noBranchText">text with no branches</param>
        private void GetSuperProjectRepositorySubmodulesStructure(GitModule currentModule, SubmoduleInfoResult result, string noBranchText)
        {
            bool isCurrentTopProject = currentModule.SuperprojectModule == null;
            if (isCurrentTopProject)
            {
                SetTopProjectSubmoduleInfo(result, noBranchText, result.Module, false, isCurrentTopProject);

                return;
            }

            IGitModule topProject = currentModule.SuperprojectModule.GetTopModule();

            bool isParentTopProject = currentModule.SuperprojectModule.WorkingDir == topProject.WorkingDir;

            // Set result.SuperProject
            SetSuperProjectSubmoduleInfo(currentModule.SuperprojectModule, result, noBranchText, topProject, isParentTopProject);

            // Set result.TopProject
            SetTopProjectSubmoduleInfo(result, noBranchText, topProject, isParentTopProject, isCurrentTopProject);

            // Set result.CurrentSubmoduleName and populate result.SuperSubmodules
            SetSubmoduleData(currentModule, result, noBranchText, topProject);
        }

        private void SetSuperProjectSubmoduleInfo(GitModule superprojectModule, SubmoduleInfoResult result, string noBranchText, IGitModule topProject, bool isParentTopProject)
        {
            string name;
            if (isParentTopProject)
            {
                var localPath = topProject.WorkingDir;
                name = Directory.Exists(localPath) ?
                    Path.GetFileName(Path.GetDirectoryName(localPath)) :
                    localPath;
            }
            else
            {
                var localPath = superprojectModule.WorkingDir.Substring(topProject.WorkingDir.Length);
                name = Path.GetDirectoryName(localPath).ToPosixPath();
             }

            string path = superprojectModule.WorkingDir;
            name += GetBranchNameSuffix(path, noBranchText);
            result.SuperProject = new SubmoduleInfo { Text = name, Path = superprojectModule.WorkingDir };
        }

        private void SetTopProjectSubmoduleInfo(SubmoduleInfoResult result,
            string noBranchText,
            IGitModule topProject,
            bool isParentTopProject,
            bool isCurrentTopProject)
        {
            if (isParentTopProject && !isCurrentTopProject)
            {
                result.TopProject = result.SuperProject;
            }
            else
            {
                string path = topProject.WorkingDir;
                string name = Directory.Exists(path) ?
                   Path.GetFileName(Path.GetDirectoryName(path)) + GetBranchNameSuffix(path, noBranchText) :
                   path;
                result.TopProject = new SubmoduleInfo { Text = name, Path = path, Bold = isCurrentTopProject };
            }
        }

        private void SetSubmoduleData(GitModule currentModule, SubmoduleInfoResult result, string noBranchText, IGitModule topProject)
        {
            var submodules = topProject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName).ToArray();
            if (submodules.Any())
            {
                string localPath = result.Module.WorkingDir.Substring(topProject.WorkingDir.Length);
                localPath = Path.GetDirectoryName(localPath).ToPosixPath();

                foreach (var submodule in submodules)
                {
                    string path = topProject.GetSubmoduleFullPath(submodule);
                    string name = submodule + GetBranchNameSuffix(path, noBranchText);

                    bool bold = false;
                    if (submodule == localPath)
                    {
                        result.CurrentSubmoduleName = currentModule.GetCurrentSubmoduleLocalPath();
                        bold = true;
                    }

                    var smi = new SubmoduleInfo { Text = name, Path = path, Bold = bold };
                    result.SuperSubmodules.Add(smi);
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

        /// <summary>
        /// Update the detailed status from the git status
        /// </summary>
        /// <param name="module">Current module</param>
        /// <param name="gitStatus">git status</param>
        /// <param name="cancelToken">Cancellation token</param>
        /// <returns>The task</returns>
        private async Task UpdateSubmodulesStatusAsync(GitModule module, [CanBeNull] IReadOnlyList<GitItemStatus> gitStatus, CancellationToken cancelToken)
        {
            _previousSubmoduleUpdateTime = DateTime.Now;
            await TaskScheduler.Default;

            var changedSubmodules = gitStatus.Where(i => i.IsSubmodule);
            foreach (var submoduleName in module.GetSubmodulesLocalPaths(false).Where(s => !changedSubmodules.Any(i => i.Name == s)))
            {
                SetSubmoduleEmptyDetailedStatus(module, submoduleName);
            }

            foreach (var submoduleName in changedSubmodules)
            {
                cancelToken.ThrowIfCancellationRequested();

                await GetSubmoduleDetailedStatusAsync(module, submoduleName.Name, cancelToken);
            }
        }

        /// <summary>
        /// Get the detailed submodule status submodules below 'module' (but not this module)
        /// </summary>
        /// <param name="module">Module to compare to</param>
        /// <param name="cancelToken">Cancelation token</param>
        /// <returns>The task</returns>
        private async Task GetSubmoduleDetailedStatusAsync(GitModule module, CancellationToken cancelToken)
        {
            foreach (var name in module.GetSubmodulesLocalPaths(false))
            {
                cancelToken.ThrowIfCancellationRequested();

                await GetSubmoduleDetailedStatusAsync(module, name, cancelToken);
            }
        }

        /// <summary>
        /// Get the detailed submodule status for 'submoduleName' and below
        /// </summary>
        /// <param name="superModule">Module to compare to</param>
        /// <param name="submoduleName">Name of the submodule</param>
        /// <param name="cancelToken">Cancelation token</param>
        /// <returns>the task</returns>
        private async Task GetSubmoduleDetailedStatusAsync(GitModule superModule, string submoduleName, CancellationToken cancelToken)
        {
            if (superModule == null || submoduleName.IsNullOrWhiteSpace())
            {
                return;
            }

            var path = superModule.GetSubmoduleFullPath(submoduleName);
            if (!_submoduleInfos.ContainsKey(path) || _submoduleInfos[path] == null)
            {
                return;
            }

            var info = _submoduleInfos[path];
            cancelToken.ThrowIfCancellationRequested();

            var submoduleStatus = await GitCommandHelpers.GetCurrentSubmoduleChangesAsync(superModule, submoduleName, noLocks: true)
            .ConfigureAwait(false);
            if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
            {
                submoduleStatus.CheckSubmoduleStatus(submoduleStatus.GetSubmodule(superModule));
            }

            info.Detailed = submoduleStatus == null ?
                null :
                new DetailedSubmoduleInfo()
                {
                    Status = submoduleStatus.Status,
                    IsDirty = submoduleStatus.IsDirty,
                    AddedAndRemovedText = submoduleStatus.AddedAndRemovedString()
                };

            // Recursively update submodules
            var module = new GitModule(path);
            await GetSubmoduleDetailedStatusAsync(module, cancelToken);
        }

        /// <summary>
        /// Set empty submodule status for 'submoduleName' and below
        /// </summary>
        /// <param name="superModule">The module to compare to</param>
        /// <param name="submoduleName">Name of the submodule</param>
        private void SetSubmoduleEmptyDetailedStatus(GitModule superModule, string submoduleName)
        {
            if (superModule == null || string.IsNullOrEmpty(submoduleName))
            {
                return;
            }

            string path = superModule.GetSubmoduleFullPath(submoduleName);
            if (!_submoduleInfos.ContainsKey(path) || _submoduleInfos[path] == null)
            {
                return;
            }

            // null is default status
            var info = _submoduleInfos[path];
            info.Detailed = null;

            var module = new GitModule(path);
            foreach (var name in module.GetSubmodulesLocalPaths(false))
            {
                SetSubmoduleEmptyDetailedStatus(module, name);
            }
        }
    }
}
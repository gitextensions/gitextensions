﻿using System;
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
        event EventHandler StatusUpdating;
        event EventHandler<SubmoduleStatusEventArgs> StatusUpdated;
        void Init();

        /// <summary>
        /// Update the submodule structure; find superprojects and submodules
        /// </summary>
        /// <param name="workingDirectory">Current module working directory</param>
        /// <param name="noBranchText">The text where no branch is checked out for the submodule</param>
        /// <param name="updateStatus">Update the detailed submodule status (set when current module is not top project)</param>
        Task UpdateSubmodulesStructureAsync(string workingDirectory, string noBranchText, bool updateStatus);

        /// <summary>
        /// Update the submodule status
        /// </summary>
        /// <param name="workingDirectory">Current module working directory</param>
        /// <param name="gitStatus">The Git status for the changes (also other than submodules)</param>
        /// <param name="forceUpdate">Suppress the usual delay of 15 seconds between consecutive updates</param>
        Task UpdateSubmodulesStatusAsync(string workingDirectory, [CanBeNull] IReadOnlyList<GitItemStatus> gitStatus, bool forceUpdate = false);
    }

    public sealed class SubmoduleStatusProvider : ISubmoduleStatusProvider
    {
        // Throttle updates triggered from status updates
        private const int MinRefreshInterval = 15;

        private readonly CancellationTokenSequence _submodulesStructureSequence = new CancellationTokenSequence();
        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private DateTime _previousSubmoduleUpdateTime;
        private SubmoduleInfoResult _submoduleInfoResult;
        private readonly Dictionary<string, SubmoduleInfo> _submoduleInfos = new Dictionary<string, SubmoduleInfo>();

        // Singleton accessor
        public static SubmoduleStatusProvider Default { get; } = new SubmoduleStatusProvider();

        // Invoked when status update is requested (use to clear/lock UI)
        public event EventHandler StatusUpdating;

        // Invoked when status update is complete
        public event EventHandler<SubmoduleStatusEventArgs> StatusUpdated;

        public void Dispose()
        {
            _submodulesStructureSequence.Dispose();
            _submodulesStatusSequence.Dispose();
        }

        public void Init()
        {
            // Cancel any previous async activities:
            _submodulesStructureSequence.Next();
            _submodulesStatusSequence.Next();
        }

        /// <inheritdoc />
        public async Task UpdateSubmodulesStructureAsync(string workingDirectory, string noBranchText, bool updateStatus)
        {
            _submoduleInfoResult = null;
            _submoduleInfos.Clear();

            // Cancel previous structure and status updates
            var cancelToken = _submodulesStructureSequence.Next();
            _submodulesStatusSequence.Next();

            // Do not throttle next status update
            _previousSubmoduleUpdateTime = DateTime.MinValue;

            OnStatusUpdating();

            await TaskScheduler.Default;

            // Start gathering new submodule structure asynchronously.
            var currentModule = new GitModule(workingDirectory);
            var result = GetSuperProjectRepositorySubmodulesStructure(currentModule, noBranchText);

            // Prepare info for status updates
            _submoduleInfos[result.TopProject.Path] = result.TopProject;
            foreach (var info in result.AllSubmodules)
            {
                _submoduleInfos[info.Path] = info;
            }

            // Structure is updated
            OnStatusUpdated(result, cancelToken);

            if (updateStatus && currentModule.SuperprojectModule != null)
            {
                // For the top module, use git-status information to update the status
                // If a submodule is current, update once from top module to give an overview
                // (The structure below the current module could have been updated from git-status but the current module
                // must be updated from its super project to set the ahead/behind information)
                // Further git-status updates only the current module and below
                var topModule = currentModule.GetTopModule();
                await GetSubmoduleDetailedStatusAsync(topModule, cancelToken);

                // Set status for top module from submodules
                foreach (var name in topModule.GetSubmodulesLocalPaths(false))
                {
                    var path = topModule.GetSubmoduleFullPath(name);

                    if (_submoduleInfos.ContainsKey(path) && _submoduleInfos[path].Detailed != null)
                    {
                        SetModuleAsDirtyUpwards(topModule);
                        break;
                    }
                }

                // Ignore if possible or at least delay the pending git-status trigger
                _previousSubmoduleUpdateTime = DateTime.Now;
                _submodulesStatusSequence.Next();
                OnStatusUpdated(result, cancelToken);
            }

            _submoduleInfoResult = result;
        }

        /// <inheritdoc />
        public async Task UpdateSubmodulesStatusAsync(string workingDirectory, [CanBeNull] IReadOnlyList<GitItemStatus> gitStatus, bool forceUpdate)
        {
            if (gitStatus == null)
            {
                return;
            }

            var cancelToken = _submodulesStatusSequence.Next();
            await TaskScheduler.Default;
            cancelToken.ThrowIfCancellationRequested();

            while (_submoduleInfoResult == null)
            {
                // Wait until the structure is built
                await Task.Delay(TimeSpan.FromMilliseconds(300), cancelToken);
                cancelToken.ThrowIfCancellationRequested();
            }

            TimeSpan remaining = _previousSubmoduleUpdateTime - DateTime.Now.AddSeconds(-MinRefreshInterval);
            if (!forceUpdate && remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, cancelToken);
                cancelToken.ThrowIfCancellationRequested();
            }

            var currentModule = new GitModule(workingDirectory);
            await UpdateSubmodulesStatusAsync(currentModule, gitStatus, cancelToken);

            OnStatusUpdated(_submoduleInfoResult, cancelToken);
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

        /// <summary>
        /// Get the result submodule structure
        /// </summary>
        /// <param name="currentModule">The current module</param>
        /// <param name="noBranchText">text with no branches</param>
        private SubmoduleInfoResult GetSuperProjectRepositorySubmodulesStructure(GitModule currentModule, string noBranchText)
        {
            var result = new SubmoduleInfoResult { Module = currentModule };

            IGitModule topProject = currentModule.GetTopModule();
            bool isCurrentTopProject = currentModule.SuperprojectModule == null;
            SetTopProjectSubmoduleInfo(result, noBranchText, topProject, isCurrentTopProject);

            bool isParentTopProject = currentModule.SuperprojectModule?.WorkingDir == topProject.WorkingDir;
            if (isParentTopProject)
            {
                result.SuperProject = result.TopProject;
            }

            // Set result.CurrentSubmoduleName and populate result.AllSubmodules
            SetSubmoduleData(currentModule, result, noBranchText, topProject);
            return result;
        }

        private void SetTopProjectSubmoduleInfo(SubmoduleInfoResult result,
            string noBranchText,
            IGitModule topProject,
            bool isCurrentTopProject)
        {
            string path = topProject.WorkingDir;
            string name = Directory.Exists(path) ? Path.GetFileName(Path.GetDirectoryName(path)) : path;
            name += GetBranchNameSuffix(path, noBranchText);
            result.TopProject = new SubmoduleInfo { Text = name, Path = path, Bold = isCurrentTopProject };
        }

        private void SetSubmoduleData(GitModule currentModule, SubmoduleInfoResult result, string noBranchText, IGitModule topProject)
        {
            var submodules = topProject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName).ToArray();
            if (!submodules.Any())
            {
                return;
            }

            var superWorkDir = currentModule.SuperprojectModule?.WorkingDir;
            var currentWorkDir = currentModule.WorkingDir;
            var localPath = currentWorkDir.Substring(topProject.WorkingDir.Length);
            if (string.IsNullOrWhiteSpace(localPath))
            {
                localPath = ".";
            }

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
                result.AllSubmodules.Add(smi);
                if (path == superWorkDir)
                {
                    result.SuperProject = smi;
                }

                if (path != currentWorkDir && path.StartsWith(currentWorkDir))
                {
                    result.OurSubmodules.Add(smi);
                }
            }
        }

        private string GetBranchNameSuffix(string repositoryPath, string noBranchText)
        {
            if (AppSettings.ShowRepoCurrentBranch && !GitModule.IsBareRepository(repositoryPath))
            {
                return " " + GetModuleBranch(repositoryPath, noBranchText);
            }

            return string.Empty;
        }

        private static string GetModuleBranch(string path, string noBranchText)
        {
            var branch = GitModule.GetSelectedBranchFast(path);
            var text = DetachedHeadParser.IsDetachedHead(branch) ? noBranchText : branch;
            return $"({text})";
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
            cancelToken.ThrowIfCancellationRequested();
            await TaskScheduler.Default;

            if (!_submoduleInfos.ContainsKey(module.WorkingDir) || _submoduleInfos[module.WorkingDir] == null)
            {
                return;
            }

            // For the current module git-status has information to set and clear the IsDirty flag
            // but cannot evaluate or change the ahead/behind information
            // The top module can only be dirty, but a submodule will only update the tree below current module
            if (gitStatus != null && gitStatus.Count > 0)
            {
                // If changes this and all super projects are at least dirty
                // (changed commit can be missed, but top module can only be dirty)
                SetModuleAsDirtyUpwards(module);
            }
            else if (_submoduleInfos[module.WorkingDir].Detailed != null)
            {
                // No Git changes for this module, clear dirty status (but unknown for super projects)
                if (_submoduleInfos[module.WorkingDir].Detailed.Status == SubmoduleStatus.Unknown)
                {
                    _submoduleInfos[module.WorkingDir].Detailed = null;
                }
                else
                {
                    _submoduleInfos[module.WorkingDir].Detailed.IsDirty = false;
                }
            }

            // Recursive update submodules,
            // git-status can set IsDirty if Unknown remains (but not ahead/behind status)
            var changedSubmodules = gitStatus?.Where(i => i.IsSubmodule) ?? new List<GitItemStatus>();
            var unchangedSubmoduleNames = module
                .GetSubmodulesLocalPaths(false)
                .Where(s => changedSubmodules.All(i => i.Name != s));
            foreach (var submoduleName in unchangedSubmoduleNames)
            {
                SetSubmoduleEmptyDetailedStatus(module, submoduleName);
            }

            foreach (var status in changedSubmodules)
            {
                if (status.IsDirty && !status.IsChanged)
                {
                    // Submodule is only dirty, no further Git command required for this submodule
                    var path = module.GetSubmoduleFullPath(status.Name);
                    SetModuleAsDirty(path, true);

                    var subModule = new GitModule(path);
                    await GetSubmoduleDetailedStatusAsync(subModule, cancelToken);
                }
                else
                {
                    await GetSubmoduleDetailedStatusAsync(module, status.Name, cancelToken);
                }
            }
        }

        /// <summary>
        /// Set the module (normally top module) as dirty (if changes in module or any submodule)
        /// If status is already set, use that (so no change from changed commits to dirty)
        /// </summary>
        /// <param name="path">path to the module</param>
        private void SetModuleAsDirty(string path, bool force = false)
        {
            if (!_submoduleInfos.ContainsKey(path) || _submoduleInfos[path] == null)
            {
                return;
            }

            if (force || _submoduleInfos[path].Detailed == null)
            {
                _submoduleInfos[path].Detailed = new DetailedSubmoduleInfo
                {
                    Status = SubmoduleStatus.Unknown,
                    IsDirty = true,
                    AddedAndRemovedText = ""
                };
            }
            else
            {
                _submoduleInfos[path].Detailed.IsDirty = true;
            }
        }

        /// <summary>
        /// Set the status to 'dirty' recursively to super projects
        /// </summary>
        /// <param name="module">module</param>
        private void SetModuleAsDirtyUpwards(GitModule module)
        {
            while (module != null)
            {
                SetModuleAsDirty(module.WorkingDir);

                module = module.SuperprojectModule;
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
            if (!_submoduleInfos.ContainsKey(module.WorkingDir) || _submoduleInfos[module.WorkingDir] == null)
            {
                return;
            }

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
            if (superModule == null || string.IsNullOrWhiteSpace(submoduleName))
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

            var submoduleStatus = await SubmoduleHelpers.GetCurrentSubmoduleChangesAsync(superModule, submoduleName, noLocks: true)
            .ConfigureAwait(false);
            if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
            {
                submoduleStatus.CheckSubmoduleStatus(submoduleStatus.GetSubmodule(superModule));
            }

            // If no changes, set info.Detailed set to null
            info.Detailed = submoduleStatus == null ?
                null :
                new DetailedSubmoduleInfo
                {
                    Status = submoduleStatus.Status,
                    IsDirty = submoduleStatus.IsDirty,
                    AddedAndRemovedText = submoduleStatus.AddedAndRemovedString()
                };

            // Recursively update submodules
            var module = new GitModule(path);
            if (submoduleStatus != null && submoduleStatus.IsDirty)
            {
                await GetSubmoduleDetailedStatusAsync(module, cancelToken);
                return;
            }

            // no changes to submodules
            foreach (var name in module.GetSubmodulesLocalPaths(false))
            {
                SetSubmoduleEmptyDetailedStatus(module, name);
            }
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

            if (_submoduleInfos[path].Detailed == null)
            {
                // If no info, submodules are already the default
                return;
            }

            _submoduleInfos[path].Detailed = null;
            var module = new GitModule(path);
            foreach (var name in module.GetSubmodulesLocalPaths(false))
            {
                SetSubmoduleEmptyDetailedStatus(module, name);
            }
        }
    }
}

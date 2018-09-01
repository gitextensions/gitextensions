using System;
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
    public sealed class SubmoduleStatusProvider : IDisposable
    {
        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private DateTime _previousSubmoduleUpdateTime;

        public bool ShouldUpdateSubmodules()
        {
            TimeSpan elapsed = DateTime.Now - _previousSubmoduleUpdateTime;
            return elapsed.TotalSeconds > 15;
        }

        public void Dispose()
        {
            _submodulesStatusSequence.Dispose();
        }

        public async Task GetSubmoduleStatusAsync(SubmoduleInfo info, CancellationToken cancelToken)
        {
            await TaskScheduler.Default;
            cancelToken.ThrowIfCancellationRequested();

            var submodule = new GitModule(info.Path);
            var supermodule = submodule.SuperprojectModule;
            var submoduleName = submodule.GetCurrentSubmoduleLocalPath();

            info.Status = null;

            if (string.IsNullOrEmpty(submoduleName) || supermodule == null)
            {
                return;
            }

            var submoduleStatus = GitCommandHelpers.GetCurrentSubmoduleChanges(supermodule, submoduleName);
            if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
            {
                submoduleStatus.CheckSubmoduleStatus(submoduleStatus.GetSubmodule(supermodule));
            }

            if (submoduleStatus != null)
            {
                info.Status = submoduleStatus.Status;
                info.IsDirty = submoduleStatus.IsDirty;
                info.Text += submoduleStatus.AddedAndRemovedString();
            }
        }

        public void UpdateSubmodulesList(string workingDirectory, string noBranchText, Action onUpdateBegin, Func<SubmoduleInfoResult, CancellationToken, Task> onUpdateCompleteAsync)
        {
            if (!ShouldUpdateSubmodules())
            {
                return;
            }

            _previousSubmoduleUpdateTime = DateTime.Now;

            // Cancel any previous async activities:
            var cancelToken = _submodulesStatusSequence.Next();

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
                GetRepositorySubmodulesStatus(result, threadModule, cancelToken, noBranchText);

                GetSuperProjectRepositorySubmodulesStatus(result, threadModule, cancelToken, noBranchText);

                // populate toolbar
                await onUpdateCompleteAsync(result, cancelToken);

                _previousSubmoduleUpdateTime = DateTime.Now;
            });
        }

        private void GetRepositorySubmodulesStatus(SubmoduleInfoResult result, IGitModule module, CancellationToken cancelToken, string noBranchText)
        {
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
                GetSubmoduleStatusAsync(smi, cancelToken).FileAndForget();
            }
        }

        private void GetSuperProjectRepositorySubmodulesStatus(SubmoduleInfoResult result, GitModule module, CancellationToken cancelToken, string noBranchText)
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
                GetSubmoduleStatusAsync(result.TopProject, cancelToken).FileAndForget();
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

            result.Superproject = new SubmoduleInfo { Text = name, Path = module.SuperprojectModule.WorkingDir };
            GetSubmoduleStatusAsync(result.Superproject, cancelToken).FileAndForget();

            var submodules = supersuperproject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName).ToArray();
            if (submodules.Any())
            {
                string localPath = module.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());

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
                    GetSubmoduleStatusAsync(smi, cancelToken).FileAndForget();
                }
            }
        }

        [NotNull]
        public GitModule FindTopProjectModule(GitModule superModule)
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

        private string GetModuleBranch(string path, string noBranchText)
        {
            var branch = GitModule.GetSelectedBranchFast(path);
            var text = DetachedHeadParser.IsDetachedHead(branch) ? noBranchText : branch;
            return $"[{text}]";
        }
    }
}
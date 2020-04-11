using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.UserRepositoryHistory;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public interface IFormBrowseController
    {
        void AddRecentRepositories([NotNull] ToolStripDropDownItem menuItemContainer,
                                   [NotNull] Repository repo,
                                   [NotNull] string caption,
                                   [NotNull] Action<object, GitModuleEventArgs> setGitModule);
        Task<GpgInfo> LoadGpgInfoAsync(GitRevision revision);
    }

    public class FormBrowseController : IFormBrowseController
    {
        private readonly IGitGpgController _gitGpgController;
        private readonly IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
        private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;

        public FormBrowseController(IGitGpgController gitGpgController,
                                    IRepositoryCurrentBranchNameProvider repositoryCurrentBranchNameProvider,
                                    IInvalidRepositoryRemover invalidRepositoryRemover)
        {
            _gitGpgController = gitGpgController;
            _repositoryCurrentBranchNameProvider = repositoryCurrentBranchNameProvider;
            _invalidRepositoryRemover = invalidRepositoryRemover;
        }

        public void AddRecentRepositories([NotNull] ToolStripDropDownItem menuItemContainer,
                                          [NotNull] Repository repo,
                                          [NotNull] string caption,
                                          [NotNull] Action<object, GitModuleEventArgs> setGitModule)
        {
            string branchName = _repositoryCurrentBranchNameProvider.GetCurrentBranchName(repo.Path);
            var item = new ToolStripMenuItem(caption)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ShortcutKeyDisplayString = branchName
            };

            menuItemContainer.DropDownItems.Add(item);

            item.Click += (obj, args) =>
            {
                OpenRepo(repo.Path, setGitModule);
            };

            if (repo.Path != caption)
            {
                item.ToolTipText = repo.Path;
            }
        }

        [ItemCanBeNull]
        public async Task<GpgInfo> LoadGpgInfoAsync(GitRevision revision)
        {
            if (!AppSettings.ShowGpgInformation.ValueOrDefault || revision?.ObjectId == null)
            {
                return null;
            }

            var getCommitSignature = _gitGpgController.GetRevisionCommitSignatureStatusAsync(revision);
            var getTagSignature = _gitGpgController.GetRevisionTagSignatureStatusAsync(revision);
            await Task.WhenAll(getCommitSignature, getTagSignature);

            var commitStatus = await getCommitSignature;
            var tagStatus = await getTagSignature;

            // Absence of Commit sign and Tag sign
            if (commitStatus == CommitStatus.NoSignature && tagStatus == TagStatus.NoTag)
            {
                return null;
            }

            return new GpgInfo(commitStatus,
                               _gitGpgController.GetCommitVerificationMessage(revision),
                               tagStatus,
                               _gitGpgController.GetTagVerifyMessage(revision));
        }

        private void ChangeWorkingDir(string path, Action<object, GitModuleEventArgs> setGitModule)
        {
            var module = new GitModule(path);
            if (module.IsValidGitWorkingDir())
            {
                setGitModule(this, new GitModuleEventArgs(module));
                return;
            }

            _invalidRepositoryRemover.ShowDeleteInvalidRepositoryDialog(path);
        }

        private void OpenRepo(string repoPath, Action<object, GitModuleEventArgs> setGitModule)
        {
            if (Control.ModifierKeys != Keys.Control)
            {
                ChangeWorkingDir(repoPath, setGitModule);
                return;
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = AppSettings.GetGitExtensionsFullPath(),
                    Arguments = "browse",
                    WorkingDirectory = repoPath,
                    UseShellExecute = false
                }
            };
            process.Start();
        }
    }
}

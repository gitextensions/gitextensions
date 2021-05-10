using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IFormBrowseController
    {
        void AddRecentRepositories(ToolStripDropDownItem menuItemContainer,
                                   Repository repo,
                                   string? caption,
                                   Action<object, GitModuleEventArgs> setGitModule);

        Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision);
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

        public void AddRecentRepositories(ToolStripDropDownItem menuItemContainer,
                                          Repository repo,
                                          string? caption,
                                          Action<object, GitModuleEventArgs> setGitModule)
        {
            string branchName = _repositoryCurrentBranchNameProvider.GetCurrentBranchName(repo.Path);
            ToolStripMenuItem item = new(caption)
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

        public async Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision)
        {
            if (!AppSettings.ShowGpgInformation.Value || revision?.ObjectId is null)
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
            GitModule module = new(path);
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

            GitUICommands.LaunchBrowse(repoPath);
        }
    }
}

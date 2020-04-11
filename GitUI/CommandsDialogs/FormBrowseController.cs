using System;
using System.Diagnostics;
using System.Linq;
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

        public FormBrowseController(IGitGpgController gitGpgController, IRepositoryCurrentBranchNameProvider repositoryCurrentBranchNameProvider)
        {
            _gitGpgController = gitGpgController;
            _repositoryCurrentBranchNameProvider = repositoryCurrentBranchNameProvider;
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

            int invalidPathCount = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync())
                                                                   .Count(repo => !GitModule.IsValidGitWorkingDir(repo.Path));
            string commandButtonCaptions = Strings.RemoveSelectedInvalidRepository;
            if (invalidPathCount > 1)
            {
                commandButtonCaptions =
                    string.Format("{0}|{1}", commandButtonCaptions, string.Format(Strings.RemoveAllInvalidRepositories, invalidPathCount));
            }

            int dialogResult = PSTaskDialog.cTaskDialog.ShowCommandBox(
                Title: Strings.Open,
                MainInstruction: Strings.DirectoryInvalidRepository,
                Content: "",
                CommandButtons: commandButtonCaptions,
                ShowCancelButton: true);

            if (dialogResult < 0)
            {
                /* Cancel */
                return;
            }
            else if (PSTaskDialog.cTaskDialog.CommandButtonResult == 0)
            {
                /* Remove selected invalid repo */
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(path));
            }
            else if (PSTaskDialog.cTaskDialog.CommandButtonResult == 1)
            {
                /* Remove all invalid repos */
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(repoPath => GitModule.IsValidGitWorkingDir(repoPath)));
            }
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

    public class GpgInfo
    {
        public GpgInfo(CommitStatus commitStatus, string commitVerificationMessage, TagStatus tagStatus, string tagVerificationMessage)
        {
            CommitStatus = commitStatus;
            CommitVerificationMessage = commitVerificationMessage;
            TagStatus = tagStatus;
            TagVerificationMessage = tagVerificationMessage;
        }

        public CommitStatus CommitStatus { get; }
        public string CommitVerificationMessage { get; }
        public TagStatus TagStatus { get; }
        public string TagVerificationMessage { get; }
    }
}

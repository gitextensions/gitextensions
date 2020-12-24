using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public interface IFormBrowseController
    {
        void AddRecentRepositories([NotNull] ToolStripDropDownItem menuItemContainer,
            [NotNull] Repository repo,
            [NotNull] string caption,
            RepoType type,
            [NotNull] Action<object, GitModuleEventArgs> setGitModule);
        Task<GpgInfo> LoadGpgInfoAsync(GitRevision revision);
    }

    public class FormBrowseController : IFormBrowseController
    {
        private readonly IGitGpgController _gitGpgController;
        private readonly IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
        private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;

        private static readonly Dictionary<RepoType, Bitmap> _repoTypeBitmaps = new Dictionary<RepoType, Bitmap>()
        {
            { RepoType.Current, Properties.Images.DashboardFolderGit },
            { RepoType.Invalid, Properties.Images.DashboardFolderError },
            { RepoType.Superproject, Properties.Images.NavigateUp },
            { RepoType.Submodule, Properties.Images.FolderSubmodule },
            { RepoType.SubmoduleRevisionUpDirty, Properties.Images.SubmoduleRevisionUpDirty },
            { RepoType.SubmoduleRevisionUp, Properties.Images.SubmoduleRevisionUp },
            { RepoType.SubmoduleRevisionDownDirty, Properties.Images.SubmoduleRevisionDownDirty },
            { RepoType.SubmoduleRevisionDown, Properties.Images.SubmoduleRevisionDown },
            { RepoType.SubmoduleRevisionSemiUpDirty, Properties.Images.SubmoduleRevisionSemiUpDirty },
            { RepoType.SubmoduleRevisionSemiUp, Properties.Images.SubmoduleRevisionSemiUp },
            { RepoType.SubmoduleRevisionSemiDownDirty, Properties.Images.SubmoduleRevisionSemiDownDirty },
            { RepoType.SubmoduleRevisionSemiDown, Properties.Images.SubmoduleRevisionSemiDown },
            { RepoType.SubmoduleDirty, Properties.Images.SubmoduleDirty },
            { RepoType.SubmoduleModified, Properties.Images.FileStatusModified },
            { RepoType.Worktree, Properties.Images.WorkTree },
            { RepoType.WorktreeDeleted, Properties.Images.WorkTreeDeleted }
        };

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
            RepoType type,
            [NotNull] Action<object, GitModuleEventArgs> setGitModule)
        {
            string branchName = _repositoryCurrentBranchNameProvider.GetCurrentBranchName(repo.Path);
            _repoTypeBitmaps.TryGetValue(type, out Bitmap repoBitmap);
            var item = new ToolStripMenuItem(caption)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ShortcutKeyDisplayString = branchName,
                Image = repoBitmap
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

            GitUICommands.LaunchBrowse(repoPath);
        }
    }
}

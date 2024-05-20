using GitCommands;
using GitCommands.Gpg;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.CommandsDialogs
{
    public interface IFormBrowseController
    {
        Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision);
    }

    public class FormBrowseController : IFormBrowseController
    {
        private readonly IGitGpgController _gitGpgController;

        public FormBrowseController(IGitGpgController gitGpgController)
        {
            _gitGpgController = gitGpgController;
        }

        public async Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision)
        {
            if (!AppSettings.ShowGpgInformation.Value || revision?.ObjectId is null)
            {
                return null;
            }

            await TaskScheduler.Default;
            Task<CommitStatus> getCommitSignature = _gitGpgController.GetRevisionCommitSignatureStatusAsync(revision);
            Task<TagStatus> getTagSignature = _gitGpgController.GetRevisionTagSignatureStatusAsync(revision);
            await Task.WhenAll(getCommitSignature, getTagSignature);

            CommitStatus commitStatus = getCommitSignature.CompletedResult();
            TagStatus tagStatus = getTagSignature.CompletedResult();

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
    }
}

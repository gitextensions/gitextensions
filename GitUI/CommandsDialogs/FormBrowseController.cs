using System.Threading.Tasks;
using GitCommands;
using GitCommands.Gpg;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public interface IFormBrowseController
    {
        Task<GpgInfo> LoadGpgInfoAsync(GitRevision revision);
    }

    public class FormBrowseController : IFormBrowseController
    {
        private readonly IGitGpgController _gitGpgController;

        public FormBrowseController(IGitGpgController gitGpgController)
        {
            _gitGpgController = gitGpgController;
        }

        [ItemCanBeNull]
        public async Task<GpgInfo> LoadGpgInfoAsync(GitRevision revision)
        {
            if (!AppSettings.ShowGpgInformation.ValueOrDefault || string.IsNullOrWhiteSpace(revision?.Guid))
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

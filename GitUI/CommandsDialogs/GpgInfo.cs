using GitCommands.Gpg;

namespace GitUI.CommandsDialogs
{
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

using GitExtensions.Extensibility.Git;

namespace GitUI.GitComments
{
    public class GitDefaultCommentStrategy : ICommentStrategy
    {
        public const string DefaultComment = "#";
        public int Id => 3;

        public string Name => "Git Config";

        public string? GetComment(IGitModule gitModule)
        {
            if (gitModule == null)
            {
                throw new ArgumentNullException(nameof(gitModule));
            }

            var commentString = gitModule.GetCommentString();
            return commentString ?? DefaultComment;
        }

        public string Description => "The behaviour of GitExtension reading the comment from git config, falling back to a hardcoded '#'";
    }
}

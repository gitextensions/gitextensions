using GitExtensions.Extensibility.Git;

namespace GitUI.GitComments
{
    public class DefaultCommentStrategy : ICommentStrategy
    {
        public int Id => 1;

        public string Name => "Default";

        public string? GetComment(IGitModule gitModule) => "#";

        public string Description => "The default behaviour of GitExtension with a hardcoded comment '#'";
    }
}

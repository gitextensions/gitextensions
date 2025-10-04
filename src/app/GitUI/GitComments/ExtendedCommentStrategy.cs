using GitExtensions.Extensibility.Git;

namespace GitUI.GitComments
{
    public class ExtendedCommentStrategy : ICommentStrategy
    {
        public int Id => 2;

        public string Name => "Default comment with space";

        public string? GetComment(IGitModule gitModule) => "# ";

        public string Description => "The behaviour of GitExtension with an extended hardcoded comment '# '";
    }
}

using GitExtensions.Extensibility.Git;

namespace GitUI.GitComments
{
    public interface ICommentStrategy
    {
        int Id { get; }

        string Name { get; }

        string? GetComment(IGitModule gitModule);

        string Description { get; }
    }
}

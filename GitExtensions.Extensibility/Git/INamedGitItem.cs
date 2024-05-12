namespace GitExtensions.Extensibility.Git;

public interface INamedGitItem : IGitItem
{
    string Name { get; }
}

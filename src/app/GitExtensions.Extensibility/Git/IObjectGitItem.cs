namespace GitExtensions.Extensibility.Git;

public interface IObjectGitItem : INamedGitItem
{
    GitObjectType ObjectType { get; }
}

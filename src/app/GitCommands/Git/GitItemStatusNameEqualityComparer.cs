using GitExtensions.Extensibility.Git;

namespace GitCommands.Git;

/// <summary>
/// Compares the file names.
/// </summary>
public class GitItemStatusNameEqualityComparer : EqualityComparer<GitItemStatus?>
{
    public override bool Equals(GitItemStatus? x, GitItemStatus? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Name == y.Name
            || (!string.IsNullOrWhiteSpace(x.OldName) && x.OldName == y.Name)
            || (!string.IsNullOrWhiteSpace(y.OldName) && x.Name == y.OldName)
            || (!string.IsNullOrWhiteSpace(x.OldName) && !string.IsNullOrWhiteSpace(y.OldName) && x.OldName == y.OldName);
    }

    public override int GetHashCode(GitItemStatus? obj)
    {
        // as renamed is an "or", hash cannot be used to compare
        return 0;
    }
}

using GitExtensions.Extensibility.Git;
#nullable enable
namespace GitUI.LeftPanel;

internal class BranchIdentifier(ObjectId objectId, string name)
{
    public ObjectId? ObjectId { get; set; } = objectId;

    public string Name { get; set; } = name;

    public override bool Equals(object? obj)
    {
        if (obj is not BranchIdentifier other)
        {
            return false;
        }

        return ObjectId == other.ObjectId && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, Name);
    }

    public static bool IsValid(BranchIdentifier branch)
    {
        return branch is not null && branch.ObjectId != default && !string.IsNullOrEmpty(branch.Name);
    }
}

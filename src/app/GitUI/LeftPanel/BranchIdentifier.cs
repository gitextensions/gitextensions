using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal class BranchIdentifier(ObjectId objectId, string name)
{
    public ObjectId ObjectId { get; } = objectId;

    public string Name { get; set; } = name;

    public override bool Equals(object obj)
    {
        if (obj is not BranchIdentifier other)
        {
            return false;
        }

        return ObjectId == other.ObjectId || Name == other.Name;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public static bool IsValid(BranchIdentifier branch)
    {
        return branch != null && branch.ObjectId != default && !string.IsNullOrEmpty(branch.Name);
    }
}

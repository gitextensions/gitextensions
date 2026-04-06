using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;

namespace GitHubActionsIntegrationTests;

internal class BuildInfoEqualityComparer : IEqualityComparer<BuildInfo>
{
    public bool Equals(BuildInfo? x, BuildInfo? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Id == y.Id
               && x.Status == y.Status
               && x.CommitHashList.SequenceEqual(y.CommitHashList)
               && x.Url == y.Url
               && x.ShowInBuildReportTab == y.ShowInBuildReportTab;
    }

    public int GetHashCode(BuildInfo obj)
    {
        HashCode hashCode = new();
        hashCode.Add(obj.Id);
        hashCode.Add((int)obj.Status);
        foreach (ObjectId commitHash in obj.CommitHashList)
        {
            hashCode.Add(commitHash);
        }

        hashCode.Add(obj.Url);
        hashCode.Add(obj.ShowInBuildReportTab);
        return hashCode.ToHashCode();
    }
}

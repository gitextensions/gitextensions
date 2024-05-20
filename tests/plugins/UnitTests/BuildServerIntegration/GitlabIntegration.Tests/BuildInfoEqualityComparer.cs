using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;

namespace GitlabIntegrationTests
{
    internal class BuildInfoEqualityComparer : IEqualityComparer<BuildInfo>
    {
        public bool Equals(BuildInfo x, BuildInfo y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Id == y.Id
                   && x.StartDate.Equals(y.StartDate)
                   && x.Duration == y.Duration
                   && x.Status == y.Status
                   && x.Description == y.Description
                   && x.CommitHashList.SequenceEqual(y.CommitHashList)
                   && x.Url == y.Url
                   && x.ShowInBuildReportTab == y.ShowInBuildReportTab
                   && x.Tooltip == y.Tooltip
                   && x.PullRequestUrl == y.PullRequestUrl;
        }

        public int GetHashCode(BuildInfo obj)
        {
            HashCode hashCode = new();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.StartDate);
            hashCode.Add(obj.Duration);
            hashCode.Add((int)obj.Status);
            hashCode.Add(obj.Description);
            foreach (ObjectId commitHash in obj.CommitHashList)
            {
                hashCode.Add(commitHash);
            }

            hashCode.Add(obj.Url);
            hashCode.Add(obj.ShowInBuildReportTab);
            hashCode.Add(obj.Tooltip);
            hashCode.Add(obj.PullRequestUrl);
            return hashCode.ToHashCode();
        }
    }
}

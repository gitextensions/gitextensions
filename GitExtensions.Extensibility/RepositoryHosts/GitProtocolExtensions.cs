using System;

namespace GitExtensions.Extensibility.RepositoryHosts
{
    public static class GitProtocolExtensions
    {
        public static bool IsUrlUsingHttp(this string url)
        {
            return url.StartsWith($"https://", StringComparison.CurrentCultureIgnoreCase) ||
                url.StartsWith($"http://", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}

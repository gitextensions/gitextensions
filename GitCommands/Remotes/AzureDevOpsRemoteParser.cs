namespace GitCommands.Remotes
{
    public sealed class AzureDevOpsRemoteParser : RemoteParser
    {
        private static readonly string VstsHttpsRemoteRegex = @"^https:\/\/(?<owner>[^.]*)\.visualstudio\.com\/(?<project>[^\/]*)\/_git\/(?<repo>.*)$";
        private static readonly string VstsSshRemoteRegex = @"^[^@]*@vs-ssh\.visualstudio\.com:v\d\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/(?<repo>.*)$";
        private static readonly string AzureDevopsHttpsRemoteRegex = @"^https:\/\/[^@]*@dev\.azure\.com\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/_git\/(?<repo>.*)$";
        private static readonly string AzureDevopsSshRemoteRegex = @"^git@ssh\.dev\.azure\.com:v\d\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/(?<repo>.*)$";
        private static readonly string[] AzureDevopsRegexes = { AzureDevopsHttpsRemoteRegex, AzureDevopsSshRemoteRegex, VstsHttpsRemoteRegex, VstsSshRemoteRegex };

        public bool IsValidRemoteUrl(string remoteUrl)
        {
            return TryExtractAzureDevopsDataFromRemoteUrl(remoteUrl, out _, out _, out _);
        }

        public bool TryExtractAzureDevopsDataFromRemoteUrl(string remoteUrl, out string owner, out string project, out string repository)
        {
            owner = null;
            project = null;
            repository = null;

            var m = MatchRegExes(remoteUrl, AzureDevopsRegexes);

            if (m == null || !m.Success)
            {
                return false;
            }

            owner = m.Groups["owner"].Value;
            project = m.Groups["project"].Value;
            repository = m.Groups["repo"].Value;
            return true;
        }
    }
}
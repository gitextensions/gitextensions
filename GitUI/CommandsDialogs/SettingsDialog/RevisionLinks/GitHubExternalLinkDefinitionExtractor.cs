using System.Collections.Generic;
using System.Drawing;
using GitCommands.ExternalLinks;
using GitCommands.Remotes;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public sealed class GitHubExternalLinkDefinitionExtractor : ExternalLinkDefinitionExtractor
    {
        public override string ServiceName => "GitHub";
        public override Image Icon => Images.GitHub;
        private readonly GitHubRemoteParser _remoteParser = new GitHubRemoteParser();

        public override bool IsValidRemoteUrl(string remoteUrl)
        {
            return _remoteParser.IsValidRemoteUrl(remoteUrl);
        }

        public override IList<ExternalLinkDefinition> GetDefinitions(string remoteUrl)
        {
            var externalLinkDefinitions = new List<ExternalLinkDefinition>();
            string organizationName = null;
            string repoName = null;

            if (!string.IsNullOrWhiteSpace(remoteUrl))
            {
                _remoteParser.TryExtractGitHubDataFromRemoteUrl(remoteUrl, out organizationName, out repoName);
            }

            organizationName = organizationName ?? "ORGANIZATION_NAME";
            repoName = repoName ?? "REPO_NAME";

            var gitHubUrl = $"https://github.com/{organizationName}/{repoName}";
            var definition = new ExternalLinkDefinition
            {
                Name = string.Format(CodeLink.Text, ServiceName),
                Enabled = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
                SearchPattern = ".*",
                LinkFormats =
                {
                    new ExternalLinkFormat { Caption = string.Format(ViewCommitLink.Text, ServiceName), Format = gitHubUrl + "/commit/%COMMIT_HASH%" },
                    new ExternalLinkFormat { Caption = string.Format(ViewProjectLink.Text, ServiceName), Format = gitHubUrl }
                }
            };
            externalLinkDefinitions.Add(definition);

            externalLinkDefinitions.Add(new ExternalLinkDefinition
            {
                Name = string.Format(IssuesLink.Text, ServiceName),
                Enabled = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message, ExternalLinkDefinition.RevisionPart.LocalBranches },
                SearchPattern = @"(?i)(?<!pull request |pr[ _]?)(#|(((feat(ure)?)|fix)[/_-]))\d+",
                NestedSearchPattern = @"\d+",
                LinkFormats = { new ExternalLinkFormat { Caption = "#{0}", Format = gitHubUrl + "/issues/{0}" } }
            });

            externalLinkDefinitions.Add(new ExternalLinkDefinition
            {
                Name = string.Format(PullRequestsLink.Text, ServiceName),
                Enabled = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message, ExternalLinkDefinition.RevisionPart.LocalBranches, ExternalLinkDefinition.RevisionPart.RemoteBranches },
                SearchPattern = @"(?i)(pull request |pr[ _]?)#?\d+",
                NestedSearchPattern = @"\d+",
                LinkFormats = { new ExternalLinkFormat { Caption = "PR #{0}", Format = gitHubUrl + "/pull/{0}" } }
            });

            return externalLinkDefinitions;
        }
    }
}
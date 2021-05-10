using System.Collections.Generic;
using System.Drawing;
using GitCommands.ExternalLinks;
using GitCommands.Remotes;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public sealed class AzureDevopsExternalLinkDefinitionExtractor : ExternalLinkDefinitionExtractor
    {
        public override string ServiceName => "Azure DevOps";
        public override Image Icon => Images.VisualStudioTeamServices;
        private readonly AzureDevOpsRemoteParser _azureDevOpsRemoteParser = new();

        public override bool IsValidRemoteUrl(string remoteUrl)
        {
            return _azureDevOpsRemoteParser.IsValidRemoteUrl(remoteUrl);
        }

        public override IList<ExternalLinkDefinition> GetDefinitions(string remoteUrl)
        {
            List<ExternalLinkDefinition> externalLinkDefinitions = new();
            string? accountName = null;
            string? repoName = null;

            if (!string.IsNullOrWhiteSpace(remoteUrl))
            {
                _azureDevOpsRemoteParser.TryExtractAzureDevopsDataFromRemoteUrl(remoteUrl, out accountName, out _, out repoName);
            }

            accountName ??= "ACCOUNT_NAME";
            repoName ??= "REPO_NAME";

            var azureDevopsUrl = $"https://dev.azure.com/{accountName}";
            ExternalLinkDefinition definition = new()
            {
                Name = string.Format(CodeLink.Text, ServiceName),
                Enabled = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
                SearchPattern = @".*",
                LinkFormats =
                {
                    new ExternalLinkFormat { Caption = string.Format(ViewCommitLink.Text, ServiceName), Format = $"{azureDevopsUrl}/_git/{repoName}/commit/%COMMIT_HASH%" },
                    new ExternalLinkFormat { Caption = string.Format(ViewProjectLink.Text, ServiceName), Format = $"{azureDevopsUrl}/{repoName}" }
                }
            };
            externalLinkDefinitions.Add(definition);

            externalLinkDefinitions.Add(new ExternalLinkDefinition
            {
                Name = string.Format(IssuesLink.Text, ServiceName),
                Enabled = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
                SearchPattern = @"#(\d+)",
                LinkFormats = { new ExternalLinkFormat { Caption = "#{0}", Format = $"{azureDevopsUrl}/{repoName}/_workitems/edit/{{0}}" } }
            });

            return externalLinkDefinitions;
        }
    }
}

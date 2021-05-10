using System;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public sealed class CloudProviderExternalLinkDefinitionExtractorFactory : ICloudProviderExternalLinkDefinitionExtractorFactory
    {
        public ICloudProviderExternalLinkDefinitionExtractor? Get(CloudProviderKind cloudProviderKind)
        {
            return cloudProviderKind switch
            {
                CloudProviderKind.GitHub => new GitHubExternalLinkDefinitionExtractor(),
                CloudProviderKind.AzureDevOps => new AzureDevopsExternalLinkDefinitionExtractor(),
                _ => null
            };
        }

        public IEnumerable<ICloudProviderExternalLinkDefinitionExtractor> GetAllExtractor()
        {
            var cloudProviderKinds = Enum.GetValues(typeof(CloudProviderKind)).OfType<CloudProviderKind>();
            CloudProviderExternalLinkDefinitionExtractorFactory cloudProviderExternalLinkDefinitionExtractorFactory = new();
            return cloudProviderKinds.Select(c => cloudProviderExternalLinkDefinitionExtractorFactory.Get(c))
                .WhereNotNull();
        }
    }
}

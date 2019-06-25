using System;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public sealed class CloudProviderExternalLinkDefinitionExtractorFactory : ICloudProviderExternalLinkDefinitionExtractorFactory
    {
        public ICloudProviderExternalLinkDefinitionExtractor Get(CloudProviderKind cloudProviderKind)
        {
            switch (cloudProviderKind)
            {
                case CloudProviderKind.GitHub:
                    return new GitHubExternalLinkDefinitionExtractor();
                case CloudProviderKind.AzureDevOps:
                    return new AzureDevopsExternalLinkDefinitionExtractor();
            }

            return null;
        }

        public IEnumerable<ICloudProviderExternalLinkDefinitionExtractor> GetAllExtractor()
        {
            var cloudProviderKinds = Enum.GetValues(typeof(CloudProviderKind)).OfType<CloudProviderKind>();
            var cloudProviderExternalLinkDefinitionExtractorFactory = new CloudProviderExternalLinkDefinitionExtractorFactory();
            return cloudProviderKinds.Select(c => cloudProviderExternalLinkDefinitionExtractorFactory.Get(c))
                .Where(e => e != null);
        }
    }
}

using System.ComponentModel.Composition;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.GitHubActionsIntegration;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class)]
public class GitHubActionsIntegrationMetadataAttribute : BuildServerAdapterMetadataAttribute
{
    public GitHubActionsIntegrationMetadataAttribute(string buildServerType)
        : base(buildServerType)
    {
    }
}

using System.ComponentModel.Composition;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.UITests.CommandsDialogs
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class MockGenericBuildServerIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public MockGenericBuildServerIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }
    }
}

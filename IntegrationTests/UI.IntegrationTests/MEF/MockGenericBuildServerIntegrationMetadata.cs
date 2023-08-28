using System.ComponentModel.Composition;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace UITests.CommandsDialogs.SettingsDialog.Pages
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

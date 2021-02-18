using System;
using System.ComponentModel.Composition;

namespace GitExtensions.Plugins.BuildServer.Core
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildServerSettingsUserControlMetadata : BuildServerAdapterMetadataAttribute
    {
        public BuildServerSettingsUserControlMetadata(string buildServerType)
            : base(buildServerType)
        {
        }
    }
}
using System;
using System.ComponentModel.Composition;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.BuildServer.Core
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildServerAdapterMetadataAttribute : ExportAttribute
    {
        public BuildServerAdapterMetadataAttribute(string buildServerType)
            : base(typeof(IBuildServerTypeMetadata))
        {
            if (string.IsNullOrEmpty(buildServerType))
            {
                throw new ArgumentException();
            }

            BuildServerType = buildServerType;
        }

        public string BuildServerType { get; }

        public virtual string? CanBeLoaded => null;
    }
}

using System;
using System.ComponentModel.Composition;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
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

        public virtual string CanBeLoaded => null;
    }
}
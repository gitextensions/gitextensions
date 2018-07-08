using System;
using System.ComponentModel.Composition;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces.BuildServerIntegration
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

        [CanBeNull]
        public virtual string CanBeLoaded => null;
    }
}
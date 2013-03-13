using System;
using System.ComponentModel.Composition;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BuildServerSettingsUserControlMetadata : ExportAttribute
    {
        public BuildServerSettingsUserControlMetadata(string buildServerType)
            : base(typeof(IBuildServerTypeMetadata))
        {
            if (string.IsNullOrEmpty(buildServerType))
                throw new ArgumentException();

            BuildServerType = buildServerType;
        }

        public string BuildServerType { get; private set; }
    }
}
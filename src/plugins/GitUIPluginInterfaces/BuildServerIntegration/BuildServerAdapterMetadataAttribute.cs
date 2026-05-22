using System.ComponentModel.Composition;

namespace GitUIPluginInterfaces.BuildServerIntegration;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class)]
public class BuildServerAdapterMetadataAttribute : ExportAttribute
{
    public BuildServerAdapterMetadataAttribute(string buildServerType)
        : base(typeof(IBuildServerTypeMetadata))
    {
        ArgumentException.ThrowIfNullOrEmpty(buildServerType);

        BuildServerType = buildServerType;
    }

    public string BuildServerType { get; }

    public virtual string? CanBeLoaded => null;
}

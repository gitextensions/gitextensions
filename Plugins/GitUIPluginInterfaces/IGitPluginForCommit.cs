using GitExtensions.Extensibility.Plugins;

namespace GitUIPluginInterfaces;

/// <summary>
///  Plugin that will be loaded when FormCommit is opened
/// </summary>
public interface IGitPluginForCommit : IGitPluginForRepository
{
}

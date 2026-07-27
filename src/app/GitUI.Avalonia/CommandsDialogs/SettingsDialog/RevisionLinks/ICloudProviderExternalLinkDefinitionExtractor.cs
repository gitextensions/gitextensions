using Avalonia.Media;
using GitCommands.ExternalLinks;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks;

public interface ICloudProviderExternalLinkDefinitionExtractor
{
    string ServiceName { get; }

    IImage Icon { get; }

    bool IsValidRemoteUrl(string remoteUrl);

    IList<ExternalLinkDefinition> GetDefinitions(string remoteUrl);
}

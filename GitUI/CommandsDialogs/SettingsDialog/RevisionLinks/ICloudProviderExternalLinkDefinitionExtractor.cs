using System.Collections.Generic;
using System.Drawing;
using GitCommands.ExternalLinks;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public interface ICloudProviderExternalLinkDefinitionExtractor
    {
        string ServiceName { get; }
        Image Icon { get; }
        bool IsValidRemoteUrl(string remoteUrl);
        IList<ExternalLinkDefinition> GetDefinitions(string remoteUrl);
    }
}
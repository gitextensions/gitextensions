namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public interface ICloudProviderExternalLinkDefinitionExtractorFactory
    {
        ICloudProviderExternalLinkDefinitionExtractor Get(CloudProviderKind cloudProviderKind);
    }
}

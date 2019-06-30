using System.Collections.Generic;
using System.Drawing;
using GitCommands.ExternalLinks;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public abstract class ExternalLinkDefinitionExtractor : ICloudProviderExternalLinkDefinitionExtractor
    {
        private protected static readonly TranslationString CodeLink = new TranslationString("{0} - Code");
        private protected static readonly TranslationString IssuesLink = new TranslationString("{0} - Issues");
        private protected static readonly TranslationString PullRequestsLink = new TranslationString("{0} - Pull Requests");
        private protected static readonly TranslationString ViewCommitLink = new TranslationString("View commit in {0}");
        private protected static readonly TranslationString ViewProjectLink = new TranslationString("View project in {0}");

        public abstract string ServiceName { get; }
        public abstract Image Icon { get; }

        public abstract bool IsValidRemoteUrl(string remoteUrl);
        public abstract IList<ExternalLinkDefinition> GetDefinitions(string remoteUrl);
    }
}

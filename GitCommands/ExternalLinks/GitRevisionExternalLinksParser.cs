using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitCommands.ExternalLinks
{
    public interface IGitRevisionExternalLinksParser
    {
        IEnumerable<ExternalLink> Parse(GitRevision revision, DistributedSettings settings);
    }

    public sealed class GitRevisionExternalLinksParser : IGitRevisionExternalLinksParser
    {
        private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;
        private readonly IExternalLinkRevisionParser _externalLinkRevisionParser;

        public GitRevisionExternalLinksParser(IConfiguredLinkDefinitionsProvider effectiveLinkDefinitionsProvider, IExternalLinkRevisionParser externalLinkRevisionParser)
        {
            _effectiveLinkDefinitionsProvider = effectiveLinkDefinitionsProvider;
            _externalLinkRevisionParser = externalLinkRevisionParser;
        }

        public IEnumerable<ExternalLink> Parse(GitRevision revision, DistributedSettings settings)
        {
            var definitions = _effectiveLinkDefinitionsProvider.Get(settings);
            return definitions.Where(definition => definition.Enabled)
                              .SelectMany(definition => _externalLinkRevisionParser.Parse(revision, definition));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using GitCommands.Settings;

namespace GitCommands.ExternalLinks
{
    public interface IGitRevisionExternalLinksParser
    {
        IEnumerable<ExternalLink> Parse(GitRevision revision, RepoDistSettings settings);
    }

    public sealed class GitRevisionExternalLinksParser : IGitRevisionExternalLinksParser
    {
        private readonly IConfiguredLinkDefinitionsProvider _effectiveLinkDefinitionsProvider;


        public GitRevisionExternalLinksParser(IConfiguredLinkDefinitionsProvider effectiveLinkDefinitionsProvider)
        {
            _effectiveLinkDefinitionsProvider = effectiveLinkDefinitionsProvider;
        }


        public IEnumerable<ExternalLink> Parse(GitRevision revision, RepoDistSettings settings)
        {
            var definitions = _effectiveLinkDefinitionsProvider.Get(settings);
            return definitions.Where(definition => definition.Enabled)
                              .SelectMany(linkDef => linkDef.Parse(revision));
        }
    }
}
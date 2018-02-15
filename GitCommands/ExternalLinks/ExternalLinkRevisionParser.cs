using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitCommands.Remote;

namespace GitCommands.ExternalLinks
{
    public interface IExternalLinkRevisionParser
    {
        IEnumerable<ExternalLink> Parse(GitRevision revision, ExternalLinkDefinition definition);
    }

    public sealed class ExternalLinkRevisionParser : IExternalLinkRevisionParser
    {
        private readonly IGitRemoteManager _gitRemoteManager;


        public ExternalLinkRevisionParser(IGitRemoteManager gitRemoteManager)
        {
            _gitRemoteManager = gitRemoteManager;
        }


        public IEnumerable<ExternalLink> Parse(GitRevision revision, ExternalLinkDefinition definition)
        {
            var remoteMatches = ParseRemotes(definition);
            return remoteMatches.SelectMany(remoteMatch => ParseRevision(revision, definition, remoteMatch));
        }


        private static IEnumerable<GitRemote> GetMatchingRemotes(ExternalLinkDefinition definition, IEnumerable<GitRemote> remotes)
        {
            if (definition.UseRemotesPattern.IsNullOrWhiteSpace() || definition.UseRemotesRegex.Value == null)
            {
                return remotes;
            }

            IEnumerable<GitRemote> matchingRemotes = remotes.Where(r => definition.UseRemotesRegex.Value.IsMatch(r.Name))
                                                            .OrderBy(r => definition.UseRemotesPattern.IndexOf(r.Name, StringComparison.OrdinalIgnoreCase));
            if (definition.UseOnlyFirstRemote)
            {
                matchingRemotes = matchingRemotes.Take(1);
            }

            return matchingRemotes;
        }

        private IEnumerable<Match> ParseRemotes(ExternalLinkDefinition definition)
        {
            IList<Match> allMatches = new List<Match>();

            if (definition.RemoteSearchPattern.IsNullOrWhiteSpace() || definition.RemoteSearchPatternRegex.Value == null)
            {
                allMatches.Add(null);
                return allMatches;
            }

            IList<string> remoteUrls = new List<string>();

            var remotes = _gitRemoteManager.LoadRemotes(false);
            var matchingRemotes = GetMatchingRemotes(definition, remotes);

            foreach (var remote in matchingRemotes)
            {
                if (definition.RemoteSearchInParts.Contains(ExternalLinkDefinition.RemotePart.URL))
                {
                    if (remote.Url.IsNotNullOrWhitespace())
                    {
                        remoteUrls.Add(remote.Url.ToLower());
                    }
                }
                if (definition.RemoteSearchInParts.Contains(ExternalLinkDefinition.RemotePart.PushURL))
                {
                    if (remote.PushUrl.IsNotNullOrWhitespace())
                    {
                        remoteUrls.Add(remote.PushUrl.ToLower());
                    }
                }
            }

            foreach (var url in remoteUrls.Distinct())
            {
                var matches = definition.RemoteSearchPatternRegex.Value.Matches(url);
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    if (match.Success)
                    {
                        allMatches.Add(match);
                    }
                }
            }

            return allMatches;
        }

        private IEnumerable<ExternalLink> ParseRevision(GitRevision revision, ExternalLinkDefinition definition, Match remoteMatch)
        {
            var links = new List<IEnumerable<ExternalLink>>();

            if (definition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.LocalBranches))
            {
                foreach (var head in revision.Refs.Where(b => !b.IsRemote))
                {
                    links.Add(ParseRevisionPart(revision, definition, remoteMatch, head.LocalName));
                }
            }

            if (definition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.RemoteBranches))
            {
                foreach (var head in revision.Refs.Where(b => b.IsRemote))
                {
                    links.Add(ParseRevisionPart(revision, definition, remoteMatch, head.LocalName));
                }
            }

            if (definition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.Message))
            {
                links.Add(ParseRevisionPart(revision, definition, remoteMatch, revision.Body));
            }

            return links.SelectMany(list => list);
        }

        private IEnumerable<ExternalLink> ParseRevisionPart(GitRevision revision, ExternalLinkDefinition definition, Match remoteMatch, string part)
        {
            if (definition.SearchPattern.IsNullOrEmpty() || definition.SearchPatternRegex.Value == null || part == null)
                yield break;

            IList<Match> allMatches = new List<Match>();

            MatchCollection matches = definition.SearchPatternRegex.Value.Matches(part);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Success)
                {
                    if (definition.NestedSearchPattern.IsNullOrEmpty())
                    {
                        allMatches.Add(match);
                    }
                    else if (definition.NestedSearchPatternRegex.Value != null && match.Value != null)
                    {
                        MatchCollection nestedMatches = definition.NestedSearchPatternRegex.Value.Matches(match.Value);

                        for (var n = 0; n < nestedMatches.Count; n++)
                        {
                            allMatches.Add(nestedMatches[n]);
                        }
                    }
                }
            }

            foreach (var match in allMatches.Where(m => m.Success))
            {
                foreach (var format in definition.LinkFormats)
                {
                    yield return format.Apply(remoteMatch, match, revision);
                }
            }
        }

    }
}
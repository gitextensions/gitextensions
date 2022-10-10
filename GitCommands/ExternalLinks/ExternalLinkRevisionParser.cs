﻿using System.Text.RegularExpressions;
using GitCommands.Remotes;
using GitUIPluginInterfaces;

namespace GitCommands.ExternalLinks
{
    public interface IExternalLinkRevisionParser
    {
        IEnumerable<ExternalLink> Parse(GitRevision revision, ExternalLinkDefinition definition);
    }

    public sealed class ExternalLinkRevisionParser : IExternalLinkRevisionParser
    {
        private readonly IConfigFileRemoteSettingsManager _remotesManager;

        public ExternalLinkRevisionParser(IConfigFileRemoteSettingsManager remotesManager)
        {
            _remotesManager = remotesManager;
        }

        public IEnumerable<ExternalLink> Parse(GitRevision revision, ExternalLinkDefinition definition)
        {
            var remoteMatches = ParseRemotes(definition);
            return remoteMatches.SelectMany(remoteMatch => ParseRevision(revision, definition, remoteMatch));
        }

        private static IEnumerable<ConfigFileRemote> GetMatchingRemotes(ExternalLinkDefinition definition, IEnumerable<ConfigFileRemote> remotes)
        {
            if (string.IsNullOrWhiteSpace(definition.UseRemotesPattern) || definition.UseRemotesRegex?.Value is null)
            {
                return remotes;
            }

            IEnumerable<ConfigFileRemote> matchingRemotes = remotes.Where(r => definition.UseRemotesRegex.Value.IsMatch(r.Name))
                                                            .OrderBy(r => definition.UseRemotesPattern.IndexOf(r.Name, StringComparison.OrdinalIgnoreCase));
            if (definition.UseOnlyFirstRemote)
            {
                matchingRemotes = matchingRemotes.Take(1);
            }

            return matchingRemotes;
        }

        private IEnumerable<Match?> ParseRemotes(ExternalLinkDefinition definition)
        {
            List<Match?> allMatches = new();

            if (string.IsNullOrWhiteSpace(definition.RemoteSearchPattern) || definition.RemoteSearchPatternRegex?.Value is null)
            {
                allMatches.Add(null);
                return allMatches;
            }

            List<string> remoteUrls = new();

            var remotes = _remotesManager.LoadRemotes(false);
            var matchingRemotes = GetMatchingRemotes(definition, remotes);

            foreach (var remote in matchingRemotes)
            {
                if (definition.RemoteSearchInParts.Contains(ExternalLinkDefinition.RemotePart.URL))
                {
                    if (!string.IsNullOrWhiteSpace(remote.Url))
                    {
                        remoteUrls.Add(remote.Url);
                    }
                }

                if (definition.RemoteSearchInParts.Contains(ExternalLinkDefinition.RemotePart.PushURL))
                {
                    if (!string.IsNullOrWhiteSpace(remote.PushUrl))
                    {
                        remoteUrls.Add(remote.PushUrl);
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

        private static IEnumerable<ExternalLink> ParseRevision(GitRevision revision, ExternalLinkDefinition definition, Match? remoteMatch)
        {
            List<IEnumerable<ExternalLink>> links = new();

            if (definition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.LocalBranches))
            {
                links.AddRange(
                    revision.Refs
                        .Where(b => !b.IsRemote)
                        .Select(head => ParseRevisionPart(revision, definition, remoteMatch, head.LocalName)));
            }

            if (definition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.RemoteBranches))
            {
                links.AddRange(
                    revision.Refs
                        .Where(b => b.IsRemote)
                        .Select(head => ParseRevisionPart(revision, definition, remoteMatch, head.LocalName)));
            }

            if (definition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.Message))
            {
                links.Add(ParseRevisionPart(revision, definition, remoteMatch, revision.Body));
            }

            return links.SelectMany(list => list);
        }

        private static IEnumerable<ExternalLink> ParseRevisionPart(GitRevision revision, ExternalLinkDefinition definition, Match? remoteMatch, string? part)
        {
            if (string.IsNullOrEmpty(definition.SearchPattern) || definition.SearchPatternRegex?.Value is null || part is null)
            {
                yield break;
            }

            List<Match> allMatches = new();

            MatchCollection matches = definition.SearchPatternRegex.Value.Matches(part);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Success)
                {
                    if (string.IsNullOrEmpty(definition.NestedSearchPattern))
                    {
                        allMatches.Add(match);
                    }
                    else if (definition.NestedSearchPatternRegex?.Value is not null)
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

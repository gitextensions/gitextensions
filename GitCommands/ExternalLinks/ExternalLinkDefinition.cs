using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using GitCommands.Core;
using GitCommands.Remote;

namespace GitCommands.ExternalLinks
{
    [XmlType("GitExtLinkDef")]
    public class ExternalLinkDefinition : SimpleStructured
    {
        //revision's parts that can be searched for candidates for a link
        public enum RevisionPart
        {
            Message,
            LocalBranches,
            RemoteBranches
        }

        public enum RemotePart
        {
            URL,
            PushURL
        }

        private string _searchPattern;
        private string _nestedSearchPattern;
        private string _remoteSearchPattern;
        private string _useRemotesPattern;


        /// <summary>
        /// Non-local link def can be locally disabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// List of formats to be applied for each revision part matched by SearchPattern
        /// </summary>
        public BindingList<ExternalLinkFormat> LinkFormats = new BindingList<ExternalLinkFormat>();

        /// <summary>Short name for this link def</summary>
        public string Name { get; set; }

        /// <summary>
        /// RegEx for revision parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string NestedSearchPattern
        {
            get
            {
                return _nestedSearchPattern;
            }
            set
            {
                _nestedSearchPattern = value;
                NestedSearchPatternRegex = new Lazy<Regex>(() =>
                {
                    try
                    {
                        return new Regex(NestedSearchPattern, RegexOptions.Compiled);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Print(e.ToStringWithData());
                        return null;
                    }
                }
                );


            }
        }

        public HashSet<RemotePart> RemoteSearchInParts = new HashSet<RemotePart>();

        /// <summary>
        /// RegEx for remote parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string RemoteSearchPattern
        {
            get
            {
                return _remoteSearchPattern;
            }
            set
            {
                _remoteSearchPattern = value;
                RemoteSearchPatternRegex = new Lazy<Regex>(() =>
                {
                    try
                    {
                        return new Regex(RemoteSearchPattern, RegexOptions.Compiled);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Print(e.ToStringWithData());
                        return null;
                    }
                }
                );
            }
        }

        public HashSet<RevisionPart> SearchInParts = new HashSet<RevisionPart>();

        /// <summary>
        /// RegEx for revision parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string SearchPattern
        {
            get
            {
                return _searchPattern;
            }
            set
            {
                _searchPattern = value;
                SearchPatternRegex = new Lazy<Regex>(() =>
                    {
                        try
                        {
                            return new Regex(SearchPattern, RegexOptions.Compiled);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.Print(e.ToStringWithData());
                            return null;
                        }
                    }
                        );
            }
        }

        /// <summary>
        /// RegEx for remotes that have to be use to search in
        /// empty string stands for an unconditionally use of the all remotes
        /// </summary>
        public string UseRemotesPattern
        {
            get
            {
                return _useRemotesPattern;
            }
            set
            {
                _useRemotesPattern = value;
                UseRemotesRegex = new Lazy<Regex>(() =>
                    {
                        try
                        {
                            return new Regex(UseRemotesPattern, RegexOptions.Compiled);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.Print(e.ToStringWithData());
                            return null;
                        }
                    }
                );
            }
        }

        /// <summary>Indicates if only the first among the matching remotes should be used</summary>
        public bool UseOnlyFirstRemote { get; set; }


        /// <summary>Compiled SearchPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> SearchPatternRegex { get; private set; }

        /// <summary>Compiled SearchPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> NestedSearchPatternRegex { get; private set; }

        /// <summary>Compiled RemoteSearchPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> RemoteSearchPatternRegex { get; private set; }

        /// <summary>Compiled UseRemotesPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> UseRemotesRegex { get; private set; }


        public IEnumerable<ExternalLink> Parse(GitRevision revision)
        {
            GitRemoteManager remoteManager = new GitRemoteManager(revision.Module);
            return Parse(revision, remoteManager);
        }

        internal IEnumerable<ExternalLink> Parse(GitRevision revision, IGitRemoteManager remoteManager)
        {
            IEnumerable<Match> remoteMatches = ParseRemotes(remoteManager);

            return remoteMatches.SelectMany(remoteMatch => ParseRevision(remoteMatch, revision));
        }

        public IEnumerable<ExternalLink> ParseRevision(Match remoteMatch, GitRevision revision)
        {
            List<IEnumerable<ExternalLink>> links = new List<IEnumerable<ExternalLink>>();

            if (SearchInParts.Contains(RevisionPart.LocalBranches))
            {
                foreach (var head in revision.Refs.Where(b => !b.IsRemote))
                {
                    links.Add(ParseRevisionPart(remoteMatch, head.LocalName, revision));
                }
            }

            if (SearchInParts.Contains(RevisionPart.RemoteBranches))
            {
                foreach (var head in revision.Refs.Where(b => b.IsRemote))
                {
                    links.Add(ParseRevisionPart(remoteMatch, head.LocalName, revision));
                }
            }

            if (SearchInParts.Contains(RevisionPart.Message))
            {
                links.Add(ParseRevisionPart(remoteMatch, revision.Body, revision));
            }

            return links.SelectMany(list => list);
        }

        private IEnumerable<Match> ParseRemotes(IGitRemoteManager remoteManager)
        {
            IList<Match> allMatches = new List<Match>();

            if (RemoteSearchPattern.IsNullOrWhiteSpace() || RemoteSearchPatternRegex.Value == null)
            {
                allMatches.Add(null);
            }
            else
            {
                IList<string> remoteUrls = new List<string>();

                var remotes = remoteManager.LoadRemotes(false);
                IEnumerable<GitRemote> matchingRemotes = GetMatchingRemotes(remotes);

                foreach (GitRemote remote in matchingRemotes)
                {
                    if (RemoteSearchInParts.Contains(RemotePart.URL))
                    {
                        if (remote.Url.IsNotNullOrWhitespace())
                        {
                            remoteUrls.Add(remote.Url.ToLower());
                        }
                    }
                    if (RemoteSearchInParts.Contains(RemotePart.PushURL))
                    {
                        if (remote.PushUrl.IsNotNullOrWhitespace())
                        {
                            remoteUrls.Add(remote.PushUrl.ToLower());
                        }
                    }
                }

                foreach (string url in remoteUrls.Distinct())
                {
                    MatchCollection matches = RemoteSearchPatternRegex.Value.Matches(url);
                    for (var i = 0; i < matches.Count; i++)
                    {
                        Match match = matches[i];
                        if (match.Success)
                        {
                            allMatches.Add(match);
                        }
                    }
                }
            }

            return allMatches;
        }

        private IEnumerable<GitRemote> GetMatchingRemotes(IEnumerable<GitRemote> remotes)
        {
            if (UseRemotesPattern.IsNullOrWhiteSpace() || UseRemotesRegex.Value == null)
            {
                return remotes;
            }

            IEnumerable<GitRemote> matchingRemotes = remotes.Where(r => UseRemotesRegex.Value.IsMatch(r.Name));
            matchingRemotes = OrderByPositionInUseRemotePattern(matchingRemotes);
            if (UseOnlyFirstRemote)
            {
                matchingRemotes = matchingRemotes.Take(1);
            }

            return matchingRemotes;
        }

        private IEnumerable<GitRemote> OrderByPositionInUseRemotePattern(IEnumerable<GitRemote> remotes)
        {
            return remotes.OrderBy(r => UseRemotesPattern.IndexOf(r.Name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ExternalLink> ParseRevisionPart(Match remoteMatch, string part, GitRevision revision)
        {
            if (SearchPattern.IsNullOrEmpty() || SearchPatternRegex.Value == null || part == null)
                yield break;

            IList<Match> allMatches = new List<Match>();

            MatchCollection matches = SearchPatternRegex.Value.Matches(part);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Success)
                {
                    if (NestedSearchPattern.IsNullOrEmpty())
                    {
                        allMatches.Add(match);
                    }
                    else if (NestedSearchPatternRegex.Value != null && match.Value != null)
                    {
                        MatchCollection nestedMatches = NestedSearchPatternRegex.Value.Matches(match.Value);

                        for (var n = 0; n < nestedMatches.Count; n++)
                        {
                            allMatches.Add(nestedMatches[n]);
                        }
                    }
                }
            }

            foreach (var match in allMatches.Where(m => m.Success))
            {
                foreach (var format in LinkFormats)
                {
                    yield return format.Apply(remoteMatch, match, revision);
                }
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void RemoveEmptyFormats()
        {
            var toRemove = LinkFormats.Where(f => f.Caption.IsNullOrWhiteSpace() && f.Format.IsNullOrWhiteSpace()).ToArray();
            toRemove.ForEach(f => LinkFormats.Remove(f));
        }

        protected internal override IEnumerable<object> InlinedStructure()
        {
            yield return Name;
            yield return SearchPattern;
            yield return SearchInParts;
            yield return NestedSearchPattern;
            yield return Enabled;
            yield return LinkFormats;
        }
    }
}

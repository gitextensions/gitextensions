using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using GitCommands.Core;
using GitCommands.Remote;

namespace GitCommands.GitExtLinks
{
    public class GitExtLinkDef : SimpleStructured
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

        /// <summary>Short name for this link def</summary>
        public string Name { get; set; }
        /// <summary></summary>
        public HashSet<RevisionPart> SearchInParts = new HashSet<RevisionPart>();

        private string _SearchPattern;
        /// <summary>
        /// RegEx for revision parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string SearchPattern
        {
            get
            {
                return _SearchPattern;
            }
            set
            {
                _SearchPattern = value;
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
        /// <summary>Compiled SearchPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> SearchPatternRegex { get; private set; }
        private string _NestedSearchPattern;
        /// <summary>
        /// RegEx for revision parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string NestedSearchPattern
        {
            get
            {
                return _NestedSearchPattern;
            }
            set
            {
                _NestedSearchPattern = value;
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
        /// <summary>Compiled SearchPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> NestedSearchPatternRegex { get; private set; }
        /// <summary>
        /// Non-local link def can be locally disabled
        /// </summary>
        public bool Enabled { get; set; }

        public HashSet<RemotePart> RemoteSearchInParts = new HashSet<RemotePart>();

        /// <summary>Compiled RemoteSearchPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> RemoteSearchPatternRegex { get; private set; }
        private string _RemoteSearchPattern;
        /// <summary>
        /// RegEx for remote parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string RemoteSearchPattern
        {
            get
            {
                return _RemoteSearchPattern;
            }
            set
            {
                _RemoteSearchPattern = value;
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

        /// <summary>Compiled UseRemotesPattern</summary>
        [XmlIgnore]
        public Lazy<Regex> UseRemotesRegex { get; private set; }
        private string _UseRemotesPattern;
        /// <summary>
        /// RegEx for remotes that have to be use to search in
        /// empty string stands for an unconditionally use of the all remotes
        /// </summary>
        public string UseRemotesPattern
        {
            get
            {
                return _UseRemotesPattern;
            }
            set
            {
                _UseRemotesPattern = value;
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

        /// <summary>
        /// List of formats to be applied for each revision part matched by SearchPattern
        /// </summary>
        public BindingList<GitExtLinkFormat> LinkFormats = new BindingList<GitExtLinkFormat>();

        public GitExtLinkDef()
        {
        }

        public IEnumerable<GitExtLink> Parse(GitRevision revision)
        {
            GitRemoteManager remoteManager = new GitRemoteManager(revision.Module);
            return Parse(revision, remoteManager);
        }

        internal IEnumerable<GitExtLink> Parse(GitRevision revision, IGitRemoteManager remoteManager)
        {
            IEnumerable<Match> remoteMatches = ParseRemotes(remoteManager);

            return remoteMatches.SelectMany(remoteMatch => ParseRevision(remoteMatch, revision));
        }

        public IEnumerable<GitExtLink> ParseRevision(Match remoteMatch, GitRevision revision)
        {
            List<IEnumerable<GitExtLink>> links = new List<IEnumerable<GitExtLink>>();

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

        public IEnumerable<GitExtLink> ParseRevisionPart(Match remoteMatch, string part, GitRevision revision)
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
                    yield return format.ToGitExtLink(remoteMatch, match, revision);
                }
            }
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

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void RemoveEmptyFormats()
        {
            var toRemove = LinkFormats.Where(f => f.Caption.IsNullOrWhiteSpace() && f.Format.IsNullOrWhiteSpace()).ToArray();
            toRemove.ForEach(f => LinkFormats.Remove(f));
        }

    }

    public class GitExtLinkFormat : SimpleStructured
    {
        public string Caption { get; set; }
        public string Format { get; set; }
        [XmlIgnore]
        public bool IsValid { get; private set; }

        public GitExtLink ToGitExtLink(Match remoteMatch, Match revisionMatch, GitRevision revision)
        {
            GitExtLink link = new GitExtLink();

            var groups = new List<string>();
            AddGroupsFromMatches(remoteMatch, groups);
            AddGroupsFromMatches(revisionMatch, groups);
            string[] groupsArray = groups.ToArray();

            try
            {
                link.Caption = string.Format(Caption, groupsArray);
                link.URI = Format.Replace("%COMMIT_HASH%", revision.Guid);
                link.URI = string.Format(link.URI, groupsArray);
                IsValid = true;
            }
            catch (Exception e)
            {
                link.URI = e.Message + ": " + Format + " " + groupsArray;
                IsValid = false;
            }

            return link;
        }

        private void AddGroupsFromMatches(Match match, List<string> groups)
        {
            if (match != null)
            {
                for (int i = match.Groups.Count > 1 ? 1 : 0; i < match.Groups.Count; i++)
                {
                    groups.Add(match.Groups[i].Value);
                }
            }
        }

        protected internal override IEnumerable<object> InlinedStructure()
        {
            yield return Caption;
            yield return Format;
        }
    }
}

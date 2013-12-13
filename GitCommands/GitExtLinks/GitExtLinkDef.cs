using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using GitCommands.Config;
using GitCommands.Core;

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
                        catch(Exception e)
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
        /// <summary>
        /// List of formats to be applied for each revision part matched by SearchPattern
        /// </summary>
        public BindingList<GitExtLinkFormat> LinkFormats = new BindingList<GitExtLinkFormat>();

        public GitExtLinkDef()
        {
        }

        public IEnumerable<GitExtLink> Parse(GitRevision revision)
        { 
            List<IEnumerable<GitExtLink>> links = new List<IEnumerable<GitExtLink>>();

            if (SearchInParts.Contains(RevisionPart.LocalBranches))
                foreach (var head in revision.Refs.Where(b => !b.IsRemote))
                    links.Add(ParsePart(head.LocalName, revision));

            if (SearchInParts.Contains(RevisionPart.RemoteBranches))
                foreach (var head in revision.Refs.Where(b => b.IsRemote))
                    links.Add(ParsePart(head.LocalName, revision));

            if (SearchInParts.Contains(RevisionPart.Message))
                links.Add(ParsePart(revision.Body, revision));

            return links.Unwrap();
        }

        public IEnumerable<GitExtLink> ParsePart(string part, GitRevision revision)
        {
            if (SearchPattern.IsNullOrEmpty() || SearchPatternRegex.Value == null)
                yield break;

            IList<Match> allMatches = new List<Match>();

            MatchCollection matches = SearchPatternRegex.Value.Matches(part);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (NestedSearchPattern.IsNullOrEmpty())
                {
                    allMatches.Add(match);
                }
                else if (NestedSearchPatternRegex.Value != null)
                {
                    MatchCollection nestedMatches = NestedSearchPatternRegex.Value.Matches(match.Value);

                    for (var n = 0; n < nestedMatches.Count; n++)
                    {
                        allMatches.Add(nestedMatches[n]);
                    }
                }
            }
            
            foreach (var match in allMatches.Where(m => m.Success))
            {
                foreach (var format in LinkFormats)
                {
                    yield return format.ToGitExtLink(match, revision);
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

        public GitExtLink ToGitExtLink(Match match, GitRevision revision)
        {
            GitExtLink link = new GitExtLink();

            try
            {
                var groups = new List<string>();
                for (int i = match.Groups.Count > 1 ? 1 : 0; i < match.Groups.Count; i++)
                {
                    groups.Add(match.Groups[i].Value);
                }

                link.Caption = string.Format(Caption, groups.ToArray());
                link.URI = Format.Replace("%COMMIT_HASH%", revision.Guid);
                link.URI = string.Format(link.URI, groups.ToArray());
                IsValid = true;
            }
            catch (Exception e)
            {
                link.URI = e.Message + ": " + Format + " " + match.Value;
                IsValid = false;
            }

            return link;
        }

        protected internal override IEnumerable<object> InlinedStructure()
        {
            yield return Caption;
            yield return Format;
        }
    }
}

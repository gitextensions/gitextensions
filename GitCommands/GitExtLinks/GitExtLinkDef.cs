using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            Hash,
            LocalBranches,
            RemoteBranches
        }

        public const string SearchPatternKey = "SearchPattern";
        public const string NestedSearchPatternKey = "NestedSearchPattern";
        public const string LinkCaptionKey = "LinkCaption";
        public const string LinkFormatKey = "LinkFormat";
        public const string LinkFormatCountKey = "LinkFormatCount";
        public const string GitExtLinkDefKey = "GitExtLinkDef";
        public const string DisabledKey = "Disabled";
        public const string RevisionPartsKey = "RevisionParts";
        
        /// <summary>Short name for this link def</summary>
        public string Name { get; set; }
        /// <summary></summary>
        public ISet<RevisionPart> SearchInParts = new HashSet<RevisionPart>();

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
                SearchPatternRegex = new Lazy<Regex>(() => new Regex(SearchPattern, RegexOptions.Compiled));
            }
        }
        /// <summary>Compiled SearchPattern</summary>
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
                NestedSearchPatternRegex = new Lazy<Regex>(() => new Regex(NestedSearchPattern, RegexOptions.Compiled));
            }
        }
        /// <summary>Compiled SearchPattern</summary>
        public Lazy<Regex> NestedSearchPatternRegex { get; private set; }
        /// <summary>
        /// Non-local link def can be locally disabled
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// Scope of this link def
        /// true - def is stored in repository local config file
        /// false - def is stored in <repo root>/.gitextensions config file
        /// </summary>
        public bool Local { get; set; }
        /// <summary>
        /// List of formats to be applied for each revision part matched by SearchPattern
        /// </summary>
        public IList<GitExtLinkFormat> LinkFormats = new List<GitExtLinkFormat>();

        public GitExtLinkDef()
        {
        }

        private static string FormatKeyFor(int i)
        {
            return LinkFormatKey + i;
        }

        private static string CaptionKeyFor(int i)
        {
            return LinkCaptionKey + i;
        }

        public static GitExtLinkDef FromConfigSection(ConfigSection section, bool local)
        {
            GitExtLinkDef linkDef = new GitExtLinkDef();
            linkDef.Name = section.SubSection;
            linkDef.SearchPattern = section.GetValue(SearchPatternKey);
            linkDef.NestedSearchPattern = section.GetValue(NestedSearchPatternKey);
            linkDef.Disabled = section.GetValueAsBool(DisabledKey, false);
            linkDef.Local = local;
            linkDef.SearchInParts.Clear();
            var partsStr = section.GetValue(RevisionPartsKey);
            foreach (var s in partsStr.Split(' '))
            { 
                RevisionPart part;
                if (Enum.TryParse(s, true, out part))
                    linkDef.SearchInParts.Add(part);
            }

            int linkCount;
            string linkCountStr = section.GetValue(LinkFormatCountKey);
            if (Int32.TryParse(linkCountStr, out linkCount))
            {
                for (int i = 0; i < linkCount; i++)
                {
                    GitExtLinkFormat linkFormat = new GitExtLinkFormat();
                    string formatKey = FormatKeyFor(i);
                    string captionKey = CaptionKeyFor(i);
                    linkFormat.Format = section.GetValue(formatKey);
                    linkFormat.Caption = section.GetValue(captionKey);
                    linkDef.LinkFormats.Add(linkFormat);
                }                
            }

            return linkDef;
        }

        public ConfigSection ToConfigSection(bool local)
        {
            ConfigSection section = new ConfigSection(GitExtLinkDefKey, true);
            section.SubSection = Name;
            section.SetValueAsBool(DisabledKey, Disabled);
            //non local def can be disabled locally
            //don't store additional data
            if (!local || !Disabled)
            {
                section.SetValue(SearchPatternKey, SearchPattern);
                section.SetValue(NestedSearchPatternKey, NestedSearchPattern);
                var partsStr = SearchInParts.Select(part => part.ToString()).Join(" ");
                section.SetValue(RevisionPartsKey, partsStr);

                int i = 0;
                foreach (var linkFormat in LinkFormats)
                {
                    section.AddValue(FormatKeyFor(i), linkFormat.Format);
                    section.AddValue(CaptionKeyFor(i), linkFormat.Caption);
                    i++;
                }
                section.AddValue(LinkFormatCountKey, i.ToString());
            }
            return section;
        }

        public IEnumerable<GitExtLink> Parse(GitRevision revision)
        { 
            List<IEnumerable<GitExtLink>> links = new List<IEnumerable<GitExtLink>>();

            if (SearchInParts.Contains(RevisionPart.LocalBranches))
                foreach (var head in revision.Heads.Where(b => !b.IsRemote))
                    links.Add(ParsePart(head.LocalName));

            if (SearchInParts.Contains(RevisionPart.RemoteBranches))
                foreach (var head in revision.Heads.Where(b => b.IsRemote))
                    links.Add(ParsePart(head.LocalName));

            if (SearchInParts.Contains(RevisionPart.Message))
                links.Add(ParsePart(revision.Message));

            if (SearchInParts.Contains(RevisionPart.Hash))
                links.Add(ParsePart(revision.Guid));

            return links.Unwrap();
        }

        public IEnumerable<GitExtLink> ParsePart(string part)
        {
            if (SearchPattern.IsNullOrEmpty())
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
                else
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
                    yield return format.ToGitExtLink(match);
                }
            }
        }

        protected internal override IEnumerable<object> InlinedStructure()
        {
            yield return Name;
            yield return SearchPattern;
            yield return SearchInParts;
            yield return NestedSearchPattern;
            yield return Disabled;
            yield return Local;
            yield return LinkFormats;
        }
    }

    public class GitExtLinkFormat : SimpleStructured
    {
        public string Caption { get; set; }
        public string Format { get; set; }
        public bool IsValid { get; private set; }

        public GitExtLink ToGitExtLink(Match match)
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
                link.URI = string.Format(Format, groups.ToArray());
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

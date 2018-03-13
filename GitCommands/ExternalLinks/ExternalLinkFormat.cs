using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using GitCommands.Core;

namespace GitCommands.ExternalLinks
{
    [XmlType("GitExtLinkFormat")]
    public class ExternalLinkFormat : SimpleStructured
    {
        public string Caption { get; set; }
        public string Format { get; set; }
        [XmlIgnore]
        public bool IsValid { get; private set; }

        public ExternalLink Apply(Match remoteMatch, Match revisionMatch, GitRevision revision)
        {
            ExternalLink link = new ExternalLink();

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

        private static void AddGroupsFromMatches(Match match, List<string> groups)
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
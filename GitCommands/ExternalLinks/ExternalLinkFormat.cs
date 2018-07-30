using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace GitCommands.ExternalLinks
{
    [XmlType("GitExtLinkFormat")]
    public class ExternalLinkFormat
    {
        public string Caption { get; set; }
        public string Format { get; set; }
        [XmlIgnore]
        public bool IsValid { get; private set; }

        public ExternalLink Apply(Match remoteMatch, Match revisionMatch, GitRevision revision)
        {
            var groups = new List<string>();
            AddGroupsFromMatches(remoteMatch);
            AddGroupsFromMatches(revisionMatch);
            var groupsArray = groups.ToArray<object>();

            string caption = null;
            string uri;
            try
            {
                caption = string.Format(Caption, groupsArray);
                uri = Format.Replace("%COMMIT_HASH%", revision.Guid);
                uri = string.Format(uri, groupsArray);
                IsValid = true;
            }
            catch (Exception e)
            {
                uri = e.Message + ": " + Format + " " + groupsArray;
                IsValid = false;
            }

            return new ExternalLink(caption, uri);

            void AddGroupsFromMatches(Match match)
            {
                if (match != null)
                {
                    for (int i = match.Groups.Count > 1 ? 1 : 0; i < match.Groups.Count; i++)
                    {
                        groups.Add(match.Groups[i].Value);
                    }
                }
            }
        }
    }
}
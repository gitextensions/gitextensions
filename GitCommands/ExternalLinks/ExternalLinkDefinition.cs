﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using GitCommands.Core;

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
            get => _nestedSearchPattern;
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
                });
            }
        }

        public HashSet<RemotePart> RemoteSearchInParts = new HashSet<RemotePart>();

        /// <summary>
        /// RegEx for remote parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string RemoteSearchPattern
        {
            get => _remoteSearchPattern;
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
                });
            }
        }

        public HashSet<RevisionPart> SearchInParts = new HashSet<RevisionPart>();

        /// <summary>
        /// RegEx for revision parts that have to be transformed into links
        /// empty string stands for unconditionally always added link
        /// </summary>
        public string SearchPattern
        {
            get => _searchPattern;
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
                });
            }
        }

        /// <summary>
        /// RegEx for remotes that have to be use to search in
        /// empty string stands for an unconditionally use of the all remotes
        /// </summary>
        public string UseRemotesPattern
        {
            get => _useRemotesPattern;
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
                });
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

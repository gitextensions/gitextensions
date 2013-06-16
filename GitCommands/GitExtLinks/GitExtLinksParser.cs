﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using GitCommands.Config;
using GitCommands.Settings;


namespace GitCommands.GitExtLinks
{
    public class GitExtLinksParser
    {
        private readonly RepoDistSettings Settings;

        public List<GitExtLinkDef> LinkDefs;
        
        private readonly GitExtLinksParser LowerPriority;

        public GitExtLinksParser(RepoDistSettings aSettings)            
        {
            Settings = new RepoDistSettings(null, aSettings.SettingsCache);
            LoadFromSettings();

            if (aSettings.LowerPriority == null)
                LowerPriority = null;
            else
                LowerPriority = new GitExtLinksParser(aSettings.LowerPriority);
        }

        public List<GitExtLinkDef> EffectiveLinkDefs
        {
            get 
            {
                HashSet<GitExtLinkDef> result = new HashSet<GitExtLinkDef>();

                foreach (var def in LinkDefs)
                    result.Add(def);

                if (LowerPriority != null)
                {
                    foreach (var def in LowerPriority.EffectiveLinkDefs)
                        result.Add(def);
                }

                return result.ToList();
            }
        }

        public bool ContainsLinkDef(string name)
        {
            return LinkDefs.Any(linkDef => linkDef.Name.Equals(name));
        }

        public void AddLinkDef(GitExtLinkDef linkDef)
        {
            if (LowerPriority == null
                || LowerPriority.LowerPriority == null
                || ContainsLinkDef(linkDef.Name)
                || LowerPriority.ContainsLinkDef(linkDef.Name))
            {
                LinkDefs.Add(linkDef);
            }
            else
            {
                LowerPriority.LowerPriority.AddLinkDef(linkDef);
            }
        }

        public void RemoveLinkDef(GitExtLinkDef linkDef)
        {
            if (!LinkDefs.Remove(linkDef) && LowerPriority != null)
                LowerPriority.RemoveLinkDef(linkDef);
        }

        public IEnumerable<GitExtLink> Parse(GitRevision revision)
        {
            return EffectiveLinkDefs.
                Where(linkDef => linkDef.Enabled).
                Select(linkDef => linkDef.Parse(revision)).
                Unwrap();
        }

        public void LoadFromSettings()
        {
            if (LowerPriority != null)
                LowerPriority.LoadFromSettings();

            LinkDefs = null;

            try
            {
                var serializer = new XmlSerializer(typeof(List<GitExtLinkDef>));
                StringReader stringReader = null;
                try
                {
                    var xml = Settings.GetString("RevisionLinkDefs", null);
                    stringReader = new StringReader(xml);
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        stringReader = null;
                        LinkDefs = serializer.Deserialize(xmlReader) as List<GitExtLinkDef>;
                    }
                }
                finally
                {
                    if (stringReader != null)
                        stringReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LinkDefs = null;
            }

            if (LinkDefs == null)
                LinkDefs = new List<GitExtLinkDef>();
        }

        public void SaveToSettings()
        {
            if (LowerPriority != null)
                LowerPriority.SaveToSettings();

            string xml;
            try
            {
                if (LinkDefs.Count == 0)
                {
                    xml = null;
                }
                else
                {
                    LinkDefs.ForEach(linkDef => linkDef.RemoveEmptyFormats());

                    var sw = new StringWriter();
                    var serializer = new XmlSerializer(typeof(List<GitExtLinkDef>));
                    serializer.Serialize(sw, LinkDefs);
                    xml = sw.ToString();                    
                }

                Settings.SetString("RevisionLinkDefs", xml);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }            
        }
    }
}

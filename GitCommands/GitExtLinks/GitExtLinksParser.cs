using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands.Config;

namespace GitCommands.GitExtLinks
{
    public class GitExtLinksParser
    {
        private readonly GitModule Module;

        public IList<GitExtLinkDef> LinkDefs = new List<GitExtLinkDef>();

        public GitExtLinksParser(GitModule aModule)            
        {
            Module = aModule;
            LoadFromConfig();
        }

        public IEnumerable<GitExtLink> Parse(GitRevision revision)
        {
            return LinkDefs.Select(linkDef => linkDef.Parse(revision)).Unwrap();
        }

        public void LoadFromConfig()
        {
            LinkDefs.Clear();
            //fill with repo scope defs             
            ConfigFile config = Module.GetGitExtensionsConfig();
            var repoSections = config.GetConfigSections(GitExtLinkDef.GitExtLinkDefKey);
            LinkDefs.AddAll(repoSections.Select(section => GitExtLinkDef.FromConfigSection(section, false)));
            Dictionary<string, GitExtLinkDef> repoDefs = LinkDefs.ToDictionary(link => link.Name);
            
            //add or override local defs
            config = Module.GetLocalConfig();
            var localSections = config.GetConfigSections(GitExtLinkDef.GitExtLinkDefKey);
            var localDefs = localSections.Select(section => GitExtLinkDef.FromConfigSection(section, true));
            foreach (var localDef in localDefs)
            { 
                GitExtLinkDef repoDef;
                if (repoDefs.TryGetValue(localDef.Name, out repoDef))
                    repoDef.Disabled = localDef.Disabled;
                else
                    LinkDefs.Add(localDef);
            }
        }

        public void SaveToConfig()
        {
            ConfigFile config = Module.GetGitExtensionsConfig();
            config.RemoveConfigSections(GitExtLinkDef.GitExtLinkDefKey);

            foreach (GitExtLinkDef linkDef in LinkDefs.Where(link => !link.Local))
                config.AddConfigSection(linkDef.ToConfigSection(false));

            config.Save();

            config = Module.GetLocalConfig();
            config.RemoveConfigSections(GitExtLinkDef.GitExtLinkDefKey);

            foreach (GitExtLinkDef linkDef in LinkDefs.Where(link => link.Local))
                config.AddConfigSection(linkDef.ToConfigSection(true));

            //store in local config fact of disabling link def
            foreach (GitExtLinkDef linkDef in LinkDefs.Where(link => !link.Local && link.Disabled))
                config.AddConfigSection(linkDef.ToConfigSection(true));

            config.Save();
        }

    }
}

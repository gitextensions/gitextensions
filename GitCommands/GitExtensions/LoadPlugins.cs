using System;
using System.Collections.Generic;
using System.Text;
using AutoCompileSubmodules;
using GitUI;

namespace GitExtensions
{
    static class PluginLoader
    {
        public static void Load()
        {
            AutoCompileSubModules submodule = new AutoCompileSubModules();
            submodule.Register(GitUICommands.Instance, new GitPluginSettingsContainer(submodule.Description));
        }
    }
}

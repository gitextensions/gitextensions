using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI;

namespace Gerrit
{
    public class FormGerritBase : GitExtensionsForm
    {
        private const string PuttyText = "PuTTY";

        protected GerritSettings Settings { get; private set; }

        protected FormGerritBase()
        {
        }

        protected void StartAgent(IWin32Window owner, string remote)
        {
            if (GitCommandHelpers.Plink())
            {
                if (!File.Exists(GitCommands.Settings.Pageant))
                    MessageBox.Show(owner, "Cannot load SSH key. PuTTY is not configured properly.", PuttyText);
                else
                    GitCommands.Settings.Module.StartPageantForRemote(remote);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode)
                return;

            Settings = GerritSettings.Load();

            if (Settings == null)
            {
                MessageBox.Show(this, "There was a problem loading the .gitreview file. Please review your settings.");
                Dispose();
                return;
            }

            base.OnLoad(e);
        }

        protected class GerritSettings
        {
            public string Host { get; private set; }
            public int Port { get; private set; }
            public string Project { get; private set; }
            public string DefaultBranch { get; private set; }
            public string DefaultRemote { get; private set; }
            public bool DefaultRebase { get; private set; }

            private GerritSettings()
            {
                Port = 29418;
                DefaultBranch = "master";
                DefaultRemote = "gerrit";
                DefaultRebase = true;
            }

            private bool IsValid()
            {
                if (
                    string.IsNullOrEmpty(Host) &&
                    string.IsNullOrEmpty(Project)
                )
                    return false;

                var remotes = GitCommands.Settings.Module.GetRemotes();

                return remotes.Contains(DefaultRemote);
            }

            public static GerritSettings Load()
            {
                string path = GitCommands.Settings.WorkingDir + ".gitreview";

                if (!File.Exists(path))
                    return null;

                bool inHeader = false;
                var result = new GerritSettings();

                foreach (string line in File.ReadAllLines(path))
                {
                    string trimmed = line.Trim();

                    // Skip empty lines and comments.

                    if (trimmed.Length == 0 || trimmed[0] == '#')
                        continue;

                    // Look for the section header.

                    if (trimmed[0] == '[')
                    {
                        inHeader = trimmed.Trim(new[] { '[', ']' }).Equals("gerrit", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (inHeader)
                    {
                        // Split on the = and trim the parts.

                        string[] parts = trimmed.Split(new[] { '=' }, 2).Select(p => p.Trim()).ToArray();

                        // Ignore invalid lines.

                        if (parts.Length != 2 || parts[1].Length == 0)
                            continue;

                        // Get the parts of the config file.

                        switch (parts[0].ToLowerInvariant())
                        {
                            case "host": result.Host = parts[1]; break;
                            case "project": result.Project = parts[1]; break;
                            case "defaultbranch": result.DefaultBranch = parts[1]; break;
                            case "defaultremote": result.DefaultRemote = parts[1]; break;
                            case "defaultrebase": result.DefaultRebase = !parts[1].Equals("0"); break;

                            case "port":
                                int value;
                                if (!int.TryParse(parts[1], out value))
                                    return null;
                                result.Port = value;
                                break;
                        }
                    }
                }

                if (!result.IsValid())
                    return null;

                return result;
            }
        }
    }
}

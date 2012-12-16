using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace Gerrit
{
    public class FormGerritBase : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _cannotLoadSshKey = new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");

        private readonly TranslationString _settingsError = new TranslationString("Error loading .gitreview file.");
        #endregion

        private const string PuttyText = "PuTTY";

        protected GerritSettings Settings { get; private set; }
        protected readonly IGitUICommands UICommands;
        protected IGitModule Module { get { return UICommands.GitModule; } }

        private FormGerritBase()
            : this(null)
        { }

        protected FormGerritBase(IGitUICommands agitUiCommands)
            : base(true)
        {
            UICommands = agitUiCommands;
        }

        protected void StartAgent(IWin32Window owner, string remote)
        {
            if (GitCommandHelpers.Plink())
            {
                if (!File.Exists(GitCommands.Settings.Pageant))
                    MessageBox.Show(owner, _cannotLoadSshKey.Text, PuttyText);
                else
                    Module.StartPageantForRemote(remote);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode)
                return;

            try
            {
                Settings = GerritSettings.Load(Module);
            }
            catch (GerritSettingsException ex)
            {
                MessageBox.Show(this, ex.Message, _settingsError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Dispose();
                return;
            }

            base.OnLoad(e);
        }

        protected class GerritSettings
        {
            #region Translation
            private static readonly TranslationString _settingsErrorFileNotFound = new TranslationString("Cannot find the \".gitreview\" file in the working directory.");
            private static readonly TranslationString _settingsErrorPortNotNumeric = new TranslationString("The \"port\" specified in the .gitreview file may only contain digits.");
            private static readonly TranslationString _settingsErrorHostNotEntered = new TranslationString("The \"host\" setting in the .gitreview file is mandatory.");
            private static readonly TranslationString _settingsErrorProjectNotEntered = new TranslationString("The \"project\" setting in the .gitreview file is mandatory.");
            private static readonly TranslationString _settingsErrorDefaultRemoteNotPresent = new TranslationString("The \"defaultremote\" setting in the .gitreview file does not refer to a configured remote. Either create this remote or change the setting in the .gitreview file.");
            #endregion

            public string Host { get; private set; }
            public int Port { get; private set; }
            public string Project { get; private set; }
            public string DefaultBranch { get; private set; }
            public string DefaultRemote { get; private set; }
            public bool DefaultRebase { get; private set; }
            private readonly IGitModule Module;

            private GerritSettings(IGitModule aModule)
            {
                Module = aModule;
                Port = 29418;
                DefaultBranch = "master";
                DefaultRemote = "gerrit";
                DefaultRebase = true;
            }

            private void Validate()
            {
                if (string.IsNullOrEmpty(Host))
                    throw new GerritSettingsException(_settingsErrorHostNotEntered.Text);
                if (string.IsNullOrEmpty(Project))
                    throw new GerritSettingsException(_settingsErrorProjectNotEntered.Text);

                var remotes = Module.GetRemotes(true);

                if (!remotes.Contains(DefaultRemote))
                    throw new GerritSettingsException(_settingsErrorDefaultRemoteNotPresent.Text);
            }

            public static GerritSettings Load(IGitModule aModule)
            {
                string path = aModule.GitWorkingDir + ".gitreview";

                if (!File.Exists(path))
                    throw new GerritSettingsException(_settingsErrorFileNotFound.Text);

                bool inHeader = false;
                var result = new GerritSettings(aModule);

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
                                    throw new GerritSettingsException(_settingsErrorPortNotNumeric.Text);
                                result.Port = value;
                                break;
                        }
                    }
                }

                result.Validate();

                return result;
            }
        }
    }
}

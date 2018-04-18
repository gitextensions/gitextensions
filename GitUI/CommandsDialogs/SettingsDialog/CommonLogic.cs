using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using Microsoft.Win32;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class CommonLogic : Translate
    {
        private readonly TranslationString _cantReadRegistry =
            new TranslationString("GitExtensions has insufficient permissions to check the registry.");

        private readonly TranslationString _selectFile =
            new TranslationString("Select file");

        public readonly RepoDistSettingsSet RepoDistSettingsSet;
        public readonly ConfigFileSettingsSet ConfigFileSettingsSet;
        public readonly GitModule Module;

        public CommonLogic(GitModule module)
        {
            Module = module;

            if (module != null)
            {
                var repoDistGlobalSettings = RepoDistSettings.CreateGlobal(false);
                var repoDistPulledSettings = RepoDistSettings.CreateDistributed(Module, false);
                var repoDistLocalSettings = RepoDistSettings.CreateLocal(Module, false);
                var repoDistEffectiveSettings = new RepoDistSettings(
                    new RepoDistSettings(repoDistGlobalSettings, repoDistPulledSettings.SettingsCache),
                    repoDistLocalSettings.SettingsCache);

                var configFileGlobalSettings = ConfigFileSettings.CreateGlobal(false);
                var configFileLocalSettings = ConfigFileSettings.CreateLocal(Module, false);
                var configFileEffectiveSettings = new ConfigFileSettings(configFileGlobalSettings, configFileLocalSettings.SettingsCache);

                RepoDistSettingsSet = new RepoDistSettingsSet(
                    repoDistEffectiveSettings,
                    repoDistLocalSettings,
                    repoDistPulledSettings,
                    repoDistGlobalSettings);

                ConfigFileSettingsSet = new ConfigFileSettingsSet(
                    configFileEffectiveSettings,
                    configFileLocalSettings,
                    configFileGlobalSettings);
            }
        }

        public const string GitExtensionsShellEx32Name = "GitExtensionsShellEx32.dll";
        public const string GitExtensionsShellEx64Name = "GitExtensionsShellEx64.dll";

        public string GetGlobalMergeTool()
        {
            return ConfigFileSettingsSet.GlobalSettings.GetValue("merge.tool");
        }

        public void SetGlobalMergeTool(string value)
        {
            ConfigFileSettingsSet.GlobalSettings.SetValue("merge.tool", value);
        }

        public bool IsMergeTool(string toolName)
        {
            return GetGlobalMergeTool().Equals(toolName,
                StringComparison.CurrentCultureIgnoreCase);
        }

        public string GetRegistryValue(RegistryKey root, string subkey, string key)
        {
            string value = null;
            try
            {
                RegistryKey registryKey = root.OpenSubKey(subkey, false);
                if (registryKey != null)
                {
                    using (registryKey)
                    {
                        value = registryKey.GetValue(key) as string;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(_cantReadRegistry.Text);
            }

            return value ?? string.Empty;
        }

        public string GetGlobalEditor()
        {
            string editor = Environment.GetEnvironmentVariable("GIT_EDITOR");
            if (!string.IsNullOrEmpty(editor))
            {
                return editor;
            }

            editor = ConfigFileSettingsSet.GlobalSettings.GetValue("core.editor");
            if (!string.IsNullOrEmpty(editor))
            {
                return editor;
            }

            editor = Environment.GetEnvironmentVariable("VISUAL");
            if (!string.IsNullOrEmpty(editor))
            {
                return editor;
            }

            return Environment.GetEnvironmentVariable("EDITOR");
        }

        public string SelectFile(string initialDirectory, string filter, string prev)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = filter,
                InitialDirectory = initialDirectory,
                Title = _selectFile.Text
            })
            {
                return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : prev;
            }
        }

        public void EncodingToCombo(Encoding encoding, ComboBox combo)
        {
            combo.Text = encoding?.EncodingName ?? "";
        }

        public Encoding ComboToEncoding(ComboBox combo)
        {
            return combo.SelectedItem as Encoding;
        }

        public void FillEncodings(ComboBox combo)
        {
            combo.Items.AddRange(AppSettings.AvailableEncodings.Values.ToArray());
            combo.DisplayMember = nameof(Encoding.EncodingName);
        }
    }
}

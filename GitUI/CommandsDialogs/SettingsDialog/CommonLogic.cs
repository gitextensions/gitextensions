using System.Text;
using GitCommands;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.Win32;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public sealed class CommonLogic : Translate
    {
        private static readonly TranslationString _cantReadRegistry =
            new("Git Extensions has insufficient permissions to check the registry.");

        private readonly TranslationString _selectFile =
            new("Select file");

        public readonly RepoDistSettingsSet RepoDistSettingsSet;
        public readonly ConfigFileSettingsSet ConfigFileSettingsSet;
        public readonly GitModule Module;

        private CommonLogic()
        {
            // For translation only
            Module = null!;
            RepoDistSettingsSet = null!;
            ConfigFileSettingsSet = null!;
        }

        public CommonLogic(GitModule module)
        {
            Requires.NotNull(module, nameof(module));

            Module = module;

            var repoDistGlobalSettings = RepoDistSettings.CreateGlobal(false);
            var repoDistPulledSettings = RepoDistSettings.CreateDistributed(module, false);
            var repoDistLocalSettings = RepoDistSettings.CreateLocal(module, false);
            RepoDistSettings repoDistEffectiveSettings = new(
                new RepoDistSettings(repoDistGlobalSettings, repoDistPulledSettings.SettingsCache, SettingLevel.Distributed),
                repoDistLocalSettings.SettingsCache,
                SettingLevel.Effective);

            var configFileGlobalSettings = ConfigFileSettings.CreateGlobal(false);
            var configFileLocalSettings = ConfigFileSettings.CreateLocal(module, false);
            ConfigFileSettings configFileEffectiveSettings = new(
                configFileGlobalSettings, configFileLocalSettings.SettingsCache, SettingLevel.Effective);

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

        /// <summary>
        /// Reads the registry key.
        /// </summary>
        /// <param name="root">Registry root</param>
        /// <param name="subkey">Registry subkey</param>
        /// <param name="key">Registry key, specify <see langword="null"/> to read default key</param>
        /// <returns>registry value or empty string in case of error</returns>
        public static string GetRegistryValue(RegistryKey root, string subkey, string? key = null)
        {
            string? value = null;
            try
            {
                var registryKey = root.OpenSubKey(subkey, writable: false);

                if (registryKey is not null)
                {
                    using (registryKey)
                    {
                        value = registryKey.GetValue(key) as string;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(_cantReadRegistry.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return value ?? "";
        }

        public string? GetGlobalEditor()
        {
            return GetEditorOptions().FirstOrDefault(o => !string.IsNullOrEmpty(o));

            IEnumerable<string> GetEditorOptions()
            {
                yield return Environment.GetEnvironmentVariable("GIT_EDITOR");
                yield return ConfigFileSettingsSet.GlobalSettings.GetValue("core.editor");
                yield return Environment.GetEnvironmentVariable("VISUAL");
                yield return Environment.GetEnvironmentVariable("EDITOR");
            }
        }

        public string SelectFile(string initialDirectory, string filter, string prev)
        {
            using System.Windows.Forms.OpenFileDialog dialog = new()
            {
                Filter = filter,
                InitialDirectory = initialDirectory,
                Title = _selectFile.Text
            };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : prev;
        }

        public void FillEncodings(ComboBox combo)
        {
            combo.Items.AddRange(AppSettings.AvailableEncodings.Values.ToArray<object>());
            combo.DisplayMember = nameof(Encoding.EncodingName);
        }
    }
}

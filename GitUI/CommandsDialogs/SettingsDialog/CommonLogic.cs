using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
            new TranslationString("Git Extensions has insufficient permissions to check the registry.");

        private readonly TranslationString _selectFile =
            new TranslationString("Select file");

        private RepoDistSettings _effectiveSettings;

        public readonly RepoDistSettingsSet RepoDistSettingsSet;
        public readonly ConfigFileSettingsSet ConfigFileSettingsSet;
        public readonly GitModule Module;

        private CommonLogic()
        {
            // For translation only
            Module = null!;
            RepoDistSettingsSet = null!;
            ConfigFileSettingsSet = null!;
            _effectiveSettings = null!;
        }

        public CommonLogic(GitModule module)
        {
            Requires.NotNull(module, nameof(module));

            Module = module;

            var repoDistGlobalSettings = RepoDistSettings.CreateGlobal(false);
            var repoDistPulledSettings = RepoDistSettings.CreateDistributed(module, false);
            var repoDistLocalSettings = RepoDistSettings.CreateLocal(module, false);
            var repoDistEffectiveSettings = new RepoDistSettings(
                new RepoDistSettings(repoDistGlobalSettings, repoDistPulledSettings.SettingsCache, SettingLevel.Distributed),
                repoDistLocalSettings.SettingsCache,
                SettingLevel.Effective);

            var configFileGlobalSettings = ConfigFileSettings.CreateGlobal(false);
            var configFileLocalSettings = ConfigFileSettings.CreateLocal(module, false);
            var configFileEffectiveSettings = new ConfigFileSettings(
                configFileGlobalSettings, configFileLocalSettings.SettingsCache, SettingLevel.Effective);

            RepoDistSettingsSet = new RepoDistSettingsSet(
                repoDistEffectiveSettings,
                repoDistLocalSettings,
                repoDistPulledSettings,
                repoDistGlobalSettings);

            _effectiveSettings = repoDistEffectiveSettings;

            ConfigFileSettingsSet = new ConfigFileSettingsSet(
                configFileEffectiveSettings,
                configFileLocalSettings,
                configFileGlobalSettings);
        }

        public const string GitExtensionsShellEx32Name = "GitExtensionsShellEx32.dll";
        public const string GitExtensionsShellEx64Name = "GitExtensionsShellEx64.dll";

        public static string GetRegistryValue(RegistryKey root, string subkey, string? key)
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
            using var dialog = new System.Windows.Forms.OpenFileDialog
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

        public void RepoDistSettingsSave()
        {
            _effectiveSettings.Save();
        }
    }
}

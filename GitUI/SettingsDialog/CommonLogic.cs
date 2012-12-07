using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Forms;
using ResourceManager.Translation;
using System.IO;
using GitCommands.Config;

namespace GitUI.SettingsDialog
{
    public class CommonLogic
    {
        private static readonly TranslationString _cantReadRegistry =
            new TranslationString("GitExtensions has insufficient permissions to check the registry.");

        private static readonly TranslationString _cantReadRegistryAddEntryManually =
            new TranslationString("GitExtensions has insufficient permissions to modify the registry." +
                                Environment.NewLine + "Please add this key to the registry manually." +
                                Environment.NewLine + "Path:  {0}\\{1}" + Environment.NewLine +
                                "Value:  {2} = {3}");

        private static readonly TranslationString _selectFile =
            new TranslationString("Select file");

        private GitModule _gitModule;
        public CommonLogic(GitModule gitModule)
        {
            _gitModule = gitModule;
        }

        /// <summary>
        /// remove later
        /// </summary>
        private GitModule Module { get { return _gitModule; } }

        public const string GitExtensionsShellExName = "GitExtensionsShellEx32.dll";

        public string GetGlobalMergeTool()
        {
            return Module.GetGlobalSetting("merge.tool");
        }

        public void SetGlobalMergeTool(string value)
        {
            Module.SetGlobalSetting("merge.tool", value);
        }

        public bool IsMergeTool(string toolName)
        {
            return GetGlobalMergeTool().Equals(toolName,
                StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetRegistryValue(RegistryKey root, string subkey, string key)
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

        public void SetRegistryValue(RegistryKey root, string subkey, string key, string value)
        {
            try
            {
                value = value.Replace("\\", "\\\\");
                string reg = "Windows Registry Editor Version 5.00" + Environment.NewLine + Environment.NewLine + "[" + root +
                             "\\" + subkey + "]" + Environment.NewLine + "\"" + key + "\"=\"" + value + "\"";

                TextWriter tw = new StreamWriter(Path.GetTempPath() + "GitExtensions.reg", false);
                tw.Write(reg);
                tw.Close();
                Module.RunCmd("regedit", "\"" + Path.GetTempPath() + "GitExtensions.reg" + "\"");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(String.Format(_cantReadRegistryAddEntryManually.Text, root, subkey, key, value));
            }
        }

        public string GetGlobalEditor()
        {
            string editor = Environment.GetEnvironmentVariable("GIT_EDITOR");
            if (!string.IsNullOrEmpty(editor))
                return editor;
            editor = Module.GetGlobalPathSetting("core.editor");
            if (!string.IsNullOrEmpty(editor))
                return editor;
            editor = Environment.GetEnvironmentVariable("VISUAL");
            if (!string.IsNullOrEmpty(editor))
                return editor;
            return Environment.GetEnvironmentVariable("EDITOR");
        }

        public static string SelectFile(string initialDirectory, string filter, string prev)
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
            if (encoding == null)
                combo.Text = "";
            else
                combo.Text = encoding.EncodingName;
        }

        public Encoding ComboToEncoding(ComboBox combo)
        {
            return combo.SelectedItem as Encoding;
        }

        public void FillEncodings(ComboBox combo)
        {
            combo.Items.AddRange(Settings.availableEncodings.Values.ToArray());
            combo.DisplayMember = "EncodingName";
        }

        public static void SetCheckboxFromString(CheckBox checkBox, string str)
        {
            str = str.Trim().ToLower();

            switch (str)
            {
                case "true":
                    {
                        checkBox.CheckState = CheckState.Checked;
                        return;
                    }
                case "false":
                    {
                        checkBox.CheckState = CheckState.Unchecked;
                        return;
                    }
                default:
                    checkBox.CheckState = CheckState.Indeterminate;
                    return;
            }
        }

        public static void SetEncoding(Encoding e, ConfigFile configFile, string name)
        {
            string value = e == null ? "" : e.HeaderName;
            configFile.SetValue(name, value);
        }
    }
}

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

        [Browsable(false)]
        public GitModule Module { get { return null; /* TODO: see GitModuleForm */ } }

        public const string GitExtensionsShellExName = "GitExtensionsShellEx32.dll";

        public string GetMergeTool()
        {
            return Module.GetGlobalSetting("merge.tool");
        }

        public bool IsMergeTool(string toolName)
        {
            return GetMergeTool().Equals(toolName,
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
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GitCommands;

namespace GitUI.Script
{
    public static class ScriptManager
    {
        private static BindingList<ScriptInfo> Scripts { get; set; }

        public static BindingList<ScriptInfo> GetScripts()
        {
            if (Scripts == null)
            {
                DeserializeFromXml(AppSettings.OwnScripts);
                if (Scripts != null)
                {
                    FixAmbiguousHotkeyCommandIdentifiers(Scripts);
                }
            }

            return Scripts;
        }

        private static void FixAmbiguousHotkeyCommandIdentifiers(BindingList<ScriptInfo> scripts)
        {
            ISet<int> ids = new HashSet<int>();
            foreach (ScriptInfo si in scripts)
            {
                if (ids.Contains(si.HotkeyCommandIdentifier))
                {
                    si.HotkeyCommandIdentifier = NextHotkeyCommandIdentifier();
                }

                ids.Add(si.HotkeyCommandIdentifier);
            }
        }

        public static ScriptInfo GetScript(string key)
        {
            foreach (ScriptInfo script in GetScripts())
            {
                if (script.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    return script;
                }
            }

            return null;
        }

        public static void RunEventScripts(GitModuleForm form, ScriptEvent scriptEvent)
        {
            foreach (var scriptInfo in GetScripts().Where(scriptInfo => scriptInfo.Enabled && scriptInfo.OnEvent == scriptEvent))
            {
                ScriptRunner.RunScript(form, form.Module, scriptInfo.Name, null);
            }
        }

        public static string SerializeIntoXml()
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(BindingList<ScriptInfo>));
                serializer.Serialize(sw, Scripts);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static void DeserializeFromXml(string xml)
        {
            // When there is nothing to deserialize, add default scripts
            if (string.IsNullOrEmpty(xml))
            {
                Scripts = new BindingList<ScriptInfo>();
                AddDefaultScripts();
                return;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(BindingList<ScriptInfo>));
                using (var stringReader = new StringReader(xml))
                {
                    var xmlReader = new XmlTextReader(stringReader);
                    Scripts = serializer.Deserialize(xmlReader) as BindingList<ScriptInfo>;
                }
            }
            catch (Exception ex)
            {
                Scripts = new BindingList<ScriptInfo>();
                DeserializeFromOldFormat(xml);

                Trace.WriteLine(ex.Message);
            }
        }

        private static void AddDefaultScripts()
        {
            Scripts.Add(new ScriptInfo
            {
                HotkeyCommandIdentifier = 9000,
                Name = "Fetch changes after commit",
                Command = "git",
                Arguments = "fetch",
                RunInBackground = false,
                AskConfirmation = true,
                OnEvent = ScriptEvent.AfterCommit,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            });

            Scripts.Add(new ScriptInfo
            {
                HotkeyCommandIdentifier = 9001,
                Name = "Update submodules after pull",
                Command = "git",
                Arguments = "submodule update --init --recursive",
                RunInBackground = false,
                AskConfirmation = true,
                OnEvent = ScriptEvent.AfterPull,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            });

            Scripts.Add(new ScriptInfo
            {
                HotkeyCommandIdentifier = 9002,
                Name = "Example",
                Command = "c:\\windows\\system32\\calc.exe",
                Arguments = "",
                RunInBackground = false,
                AskConfirmation = false,
                OnEvent = ScriptEvent.ShowInUserMenuBar,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            });

            Scripts.Add(new ScriptInfo
            {
                HotkeyCommandIdentifier = 9003,
                Name = "Open on GitHub",
                Command = "{openurl}",
                Arguments = "https://github.com{cDefaultRemotePathFromUrl}/commit/{sHash}",
                RunInBackground = false,
                AskConfirmation = false,
                OnEvent = 0,
                AddToRevisionGridContextMenu = true,
                Enabled = false
            });

            Scripts.Add(new ScriptInfo
            {
                HotkeyCommandIdentifier = 9004,
                Name = "Fetch All Submodules",
                Command = "git",
                Arguments = "submodule foreach --recursive git fetch --all",
                RunInBackground = false,
                AskConfirmation = false,
                OnEvent = 0,
                AddToRevisionGridContextMenu = true,
                Enabled = false
            });
        }

        internal static int NextHotkeyCommandIdentifier()
        {
            return GetScripts().Select(s => s.HotkeyCommandIdentifier).Max() + 1;
        }

        private static void DeserializeFromOldFormat(string inputString)
        {
            const string paramSeparator = "<_PARAM_SEPARATOR_>";
            const string scriptSeparator = "<_SCRIPT_SEPARATOR_>";

            if (inputString.Contains(paramSeparator) || inputString.Contains(scriptSeparator))
            {
                Scripts = new BindingList<ScriptInfo>();

                string[] scripts = inputString.Split(new[] { scriptSeparator }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < scripts.Length; i++)
                {
                    string[] parameters = scripts[i].Split(new[] { paramSeparator }, StringSplitOptions.None);

                    Scripts.Add(new ScriptInfo
                    {
                        Name = parameters[0],
                        Command = parameters[1],
                        Arguments = parameters[2],
                        AddToRevisionGridContextMenu = parameters[3] == "yes",
                        Enabled = true,
                        RunInBackground = false
                    });
                }
            }
        }
    }
}

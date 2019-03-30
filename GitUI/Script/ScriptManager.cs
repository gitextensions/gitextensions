using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GitCommands;
using GitUI.Browsing.Dialogs;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.Script
{
    internal sealed class ScriptManager : IScriptManager
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(BindingList<ScriptInfo>));
        private static BindingList<ScriptInfo> _scripts;

        [NotNull]
        public BindingList<ScriptInfo> GetScripts()
        {
            if (_scripts == null)
            {
                _scripts = DeserializeFromXml(AppSettings.OwnScripts);
                FixAmbiguousHotkeyCommandIdentifiers();
            }

            return _scripts;

            void FixAmbiguousHotkeyCommandIdentifiers()
            {
                var ids = new HashSet<int>();

                foreach (var script in _scripts)
                {
                    if (!ids.Add(script.HotkeyCommandIdentifier))
                    {
                        script.HotkeyCommandIdentifier = NextHotKeyCommandIdentifier();
                    }
                }
            }
        }

        [CanBeNull]
        public ScriptInfo GetScript(string key)
        {
            foreach (var script in GetScripts())
            {
                if (script.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    return script;
                }
            }

            return null;
        }

        public void RunEventScripts(GitModuleForm form, ScriptEvent scriptEvent)
        {
            foreach (var script in GetScripts().Where(scriptInfo => scriptInfo.Enabled && scriptInfo.OnEvent == scriptEvent))
            {
                var gitUIEventArgs = new GitUIEventArgs(form, form.UICommands);
                var simpleDialog = new SimpleDialog(form);
                var scriptOptionsParser = new ScriptOptionsParser(simpleDialog);
                var scriptRunner = new ScriptRunner(form.Module, gitUIEventArgs, scriptOptionsParser, simpleDialog, new ScriptManager());

                scriptRunner.RunScript(script.Name);
            }
        }

        [CanBeNull]
        public string SerializeIntoXml()
        {
            try
            {
                var sw = new StringWriter();
                _serializer.Serialize(sw, _scripts);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        [NotNull]
        private static BindingList<ScriptInfo> DeserializeFromXml([CanBeNull] string xml)
        {
            // When there is nothing to deserialize, add default scripts
            if (string.IsNullOrEmpty(xml))
            {
                return GetDefaultScripts();
            }

            try
            {
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return (BindingList<ScriptInfo>)_serializer.Deserialize(xmlReader);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return DeserializeFromOldFormat(xml);
            }

            BindingList<ScriptInfo> GetDefaultScripts() => new BindingList<ScriptInfo>
            {
                new ScriptInfo
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
                },
                new ScriptInfo
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
                },
                new ScriptInfo
                {
                    HotkeyCommandIdentifier = 9002,
                    Name = "Example",
                    Command = @"c:\windows\system32\calc.exe",
                    Arguments = "",
                    RunInBackground = false,
                    AskConfirmation = false,
                    OnEvent = ScriptEvent.ShowInUserMenuBar,
                    AddToRevisionGridContextMenu = false,
                    Enabled = false
                },
                new ScriptInfo
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
                },
                new ScriptInfo
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
                }
            };

            BindingList<ScriptInfo> DeserializeFromOldFormat(string inputString)
            {
                const string paramSeparator = "<_PARAM_SEPARATOR_>";
                const string scriptSeparator = "<_SCRIPT_SEPARATOR_>";

                var scripts = new BindingList<ScriptInfo>();

                if (inputString.Contains(paramSeparator) || inputString.Contains(scriptSeparator))
                {
                    foreach (var script in inputString.Split(new[] { scriptSeparator }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parameters = script.Split(new[] { paramSeparator }, StringSplitOptions.None);

                        scripts.Add(new ScriptInfo
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

                return scripts;
            }
        }

        public int NextHotKeyCommandIdentifier()
        {
            return GetScripts().Select(s => s.HotkeyCommandIdentifier).Max() + 1;
        }
    }
}

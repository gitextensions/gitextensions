﻿using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using GitCommands;

namespace GitUI.Script
{
    public static class ScriptManager
    {
        internal const int MinimumUserScriptID = 9000;
        private static readonly XmlSerializer _serializer = new(typeof(BindingList<ScriptInfo>));
        private static BindingList<ScriptInfo>? _scripts;

        public static BindingList<ScriptInfo> GetScripts()
        {
            if (_scripts is null)
            {
                _scripts = DeserializeFromXml(AppSettings.OwnScripts);
                FixAmbiguousHotkeyCommandIdentifiers();
            }

            return _scripts;

            static void FixAmbiguousHotkeyCommandIdentifiers()
            {
                HashSet<int> ids = new();

                foreach (var script in _scripts!)
                {
                    if (!ids.Add(script.HotkeyCommandIdentifier))
                    {
                        script.HotkeyCommandIdentifier = NextHotkeyCommandIdentifier();
                    }
                }
            }
        }

        public static ScriptInfo? GetScript(string key)
        {
            foreach (var script in GetScripts())
            {
                if (StringComparer.CurrentCultureIgnoreCase.Equals(script.Name, key))
                {
                    return script;
                }
            }

            return null;
        }

        public static bool RunEventScripts(GitModuleForm form, ScriptEvent scriptEvent)
        {
            foreach (var script in GetScripts().Where(scriptInfo => scriptInfo.Enabled && scriptInfo.OnEvent == scriptEvent))
            {
                var result = ScriptRunner.RunScript(form, form.Module, script.Name, form.UICommands, revisionGrid: null);
                if (!result.Executed)
                {
                    return false;
                }
            }

            return true;
        }

        public static string? SerializeIntoXml()
        {
            try
            {
                XmlWriterSettings xmlWriterSettings = new()
                {
                    Indent = true
                };
                using StringWriter sw = new();
                using XmlWriter xmlWriter = XmlWriter.Create(sw, xmlWriterSettings);

                _serializer.Serialize(xmlWriter, _scripts);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static BindingList<ScriptInfo> DeserializeFromXml(string? xml)
        {
            // When there is nothing to deserialize, add default scripts
            if (string.IsNullOrEmpty(xml))
            {
                return GetDefaultScripts();
            }

            try
            {
                using StringReader stringReader = new(xml);
                using XmlTextReader xmlReader = new(stringReader);
                return (BindingList<ScriptInfo>)_serializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return DeserializeFromOldFormat(xml);
            }

            BindingList<ScriptInfo> GetDefaultScripts() => new()
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

                BindingList<ScriptInfo> scripts = new();

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

        internal static int NextHotkeyCommandIdentifier()
        {
            return GetScripts().Select(s => s.HotkeyCommandIdentifier).Max() + 1;
        }

        internal struct TestAccessor
        {
            public static void Reset() => _scripts = null;
        }
    }
}

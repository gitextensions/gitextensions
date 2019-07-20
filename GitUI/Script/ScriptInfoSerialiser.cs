using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace GitUI.Script
{
    public sealed class ScriptInfoSerialiser
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<ScriptInfo>));

        public IList<ScriptInfo> Deserialise(string xml)
        {
            // When there is nothing to deserialize, add default scripts
            if (string.IsNullOrWhiteSpace(xml))
            {
                return GetDefaultScripts();
            }

            try
            {
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return (IList<ScriptInfo>)_serializer.Deserialize(xmlReader);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return DeserializeFromOldFormat(xml);
            }
        }

        [CanBeNull]
        public string Serialise(IList<ScriptInfo> scripts)
        {
            if (scripts == null)
            {
                throw new ArgumentNullException(nameof(scripts));
            }

            try
            {
                var list = scripts is List<ScriptInfo> ? (List<ScriptInfo>)scripts : scripts.ToList();

                using (var sw = new StringWriter())
                {
                    _serializer.Serialize(sw, list);
                    return sw.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        private IList<ScriptInfo> GetDefaultScripts() => new Collection<ScriptInfo>
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

        private Collection<ScriptInfo> DeserializeFromOldFormat(string inputString)
        {
            const string paramSeparator = "<_PARAM_SEPARATOR_>";
            const string scriptSeparator = "<_SCRIPT_SEPARATOR_>";

            var scripts = new BindingList<ScriptInfo>();

            if (!inputString.Contains(paramSeparator) && !inputString.Contains(scriptSeparator))
            {
                return scripts;
            }

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

            return scripts;
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly ScriptInfoSerialiser _scriptInfoSerialiser;

            public TestAccessor(ScriptInfoSerialiser scriptInfoSerialiser)
            {
                _scriptInfoSerialiser = scriptInfoSerialiser;
            }

            public IList<ScriptInfo> GetDefaultScripts() => _scriptInfoSerialiser.GetDefaultScripts();
        }
    }
}

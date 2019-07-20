using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.Script
{
    internal interface IScriptManager
    {
        IList<ScriptInfo> GetScripts();

        ScriptInfo GetScript(string key);

        int NextHotKeyCommandIdentifier();
    }

    internal sealed class ScriptManager : IScriptManager
    {
        private static IList<ScriptInfo> _scripts;
        private readonly ScriptInfoSerialiser _scriptInfoSerialiser;

        public ScriptManager(ScriptInfoSerialiser scriptInfoSerialiser = null)
        {
            _scriptInfoSerialiser = scriptInfoSerialiser ?? new ScriptInfoSerialiser();
        }

        [NotNull]
        public IList<ScriptInfo> GetScripts()
        {
            if (_scripts == null)
            {
                _scripts = _scriptInfoSerialiser.Deserialise(AppSettings.OwnScripts);
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

        public int NextHotKeyCommandIdentifier()
        {
            return GetScripts().Select(s => s.HotkeyCommandIdentifier).Max() + 1;
        }
    }
}

using GitCommands;
using GitCommands.Settings;
using GitExtUtils;
using GitUI.NBugReports;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.ScriptsEngine
{
    internal sealed partial class DistributedScriptsManager : IScriptsManager, IScriptsRunner
    {
        private readonly IUserScriptsStorage _scriptsStorage;
        private DistributedSettings _cachedSettings;
        private DistributedScriptsManager? _lowerPriority;
        private List<ScriptInfo>? _scripts;

        public DistributedScriptsManager(IUserScriptsStorage scriptsStorage)
        {
            _scriptsStorage = scriptsStorage;
        }

        /// <summary>
        ///  Gets the minimum script ID for each settings level.
        /// </summary>
        private int MinimumUserScriptID => _cachedSettings.SettingLevel switch
        {
            SettingLevel.Global => 9_000,
            SettingLevel.Distributed => 10_000,
            _ => 11_000
        };

        public void Add(ScriptInfo script)
        {
            if (_lowerPriority is null || _lowerPriority.Contains(script.HotkeyCommandIdentifier))
            {
                if (!Contains(script.HotkeyCommandIdentifier))
                {
                    script.HotkeyCommandIdentifier = Math.Max(MinimumUserScriptID, _scripts.Max(s => s.HotkeyCommandIdentifier)) + 1;
                    _scripts.Add(script);
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority.Add(script);
            }
        }

        /// <summary>
        ///  Checks if a script definition with the supplied ID exists in any level of available settings.
        /// </summary>
        /// <param name="scriptId">The script with ID to find.</param>
        /// <returns><see langword="true"/> if a script definition already exists; otherwise <see langword="false"/>.</returns>
        private bool Contains(int scriptId)
        {
            return _scripts.Any(scriptInfo => scriptInfo.HotkeyCommandIdentifier == scriptId) ||
                (_lowerPriority?.Contains(scriptId) ?? false);
        }

        public ScriptInfo? GetScript(int scriptId)
        {
            foreach (ScriptInfo script in GetScriptsInternal())
            {
                if (script.HotkeyCommandIdentifier == scriptId)
                {
                    return script;
                }
            }

            return null;
        }

        public IReadOnlyList<ScriptInfo> GetScripts()
        {
            List<ScriptInfo> scripts = GetScriptsInternal().ToList();

            // Ensure script IDs are unique; update if necessary.
            // NB: this loop is in O(n^2), but we don't expect more than few dozens of scripts, so it should be acceptable.
            HashSet<int> ids = new();
            foreach (ScriptInfo script in scripts)
            {
                if (!ids.Add(script.HotkeyCommandIdentifier))
                {
                    script.HotkeyCommandIdentifier = scripts.Select(s => s.HotkeyCommandIdentifier).Max() + 1;
                }
            }

            return scripts;
        }

        private IEnumerable<ScriptInfo> GetScriptsInternal()
        {
            return _scripts.Union(_lowerPriority?.GetScriptsInternal() ?? Enumerable.Empty<ScriptInfo>());
        }

        public void Initialize(DistributedSettings settings)
        {
            _cachedSettings = new DistributedSettings(null, settings.SettingsCache, settings.SettingLevel);
            _scripts = new List<ScriptInfo>(_scriptsStorage.Load(_cachedSettings));

            if (settings.LowerPriority is not null)
            {
                _lowerPriority = new DistributedScriptsManager(_scriptsStorage);
                _lowerPriority.Initialize(settings.LowerPriority);
            }
        }

        public void Remove(ScriptInfo script)
        {
            if (_lowerPriority is null || _lowerPriority.Contains(script.HotkeyCommandIdentifier))
            {
                for (int index = 0; index < _scripts.Count; index++)
                {
                    if (_scripts[index].HotkeyCommandIdentifier == script.HotkeyCommandIdentifier)
                    {
                        _scripts.RemoveAt(index);
                    }
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority?.Remove(script);
            }
        }

        public bool RunEventScripts<THostForm>(ScriptEvent scriptEvent, THostForm form)
            where THostForm : IGitModuleForm, IWin32Window
        {
            foreach (ScriptInfo script in GetScripts().Where(scriptInfo => scriptInfo.Enabled && scriptInfo.OnEvent == scriptEvent))
            {
                CommandStatus result = ScriptRunner.RunScript(script, form, revisionGrid: null);
                if (!result.Executed)
                {
                    return false;
                }
            }

            return true;
        }

        public CommandStatus RunScript<THostForm>(int scriptId, THostForm form, RevisionGridControl? revisionGrid = null)
            where THostForm : IGitModuleForm, IWin32Window
        {
            ScriptInfo? scriptInfo = GetScript(scriptId);
            if (scriptInfo is null)
            {
                throw new UserExternalOperationException($"{TranslatedStrings.ScriptErrorCantFind}: '{scriptId}'",
                    new ExternalOperationException(workingDirectory: form.UICommands.GitModule.WorkingDir));
            }

            return ScriptRunner.RunScript(scriptInfo, form, revisionGrid);
        }

        public void Save()
        {
            _lowerPriority?.Save();
            _scriptsStorage.Save(_cachedSettings, _scripts);
        }

        public void Update(ScriptInfo script)
        {
            if (_lowerPriority is null || !_lowerPriority.Contains(script.HotkeyCommandIdentifier))
            {
                for (int index = 0; index < _scripts.Count; index++)
                {
                    if (_scripts[index].HotkeyCommandIdentifier == script.HotkeyCommandIdentifier)
                    {
                        _scripts[index] = script;
                    }
                }

                // TODO: else notify the user?
            }
            else
            {
                _lowerPriority.Update(script);
            }
        }
    }
}

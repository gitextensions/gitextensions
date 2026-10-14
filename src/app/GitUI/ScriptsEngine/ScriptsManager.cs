using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using GitCommands;
using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitUI.ScriptsEngine;

internal sealed partial class ScriptsManager : IScriptsManager, IScriptsRunner
{
    internal const int MinimumUserScriptID = 9000;

    // ScriptEvent once had this value, which put the script on the Scripts toolbar.
    private const string LegacyShowInUserMenuBar = "ShowInUserMenuBar";

    private static readonly XmlSerializer _serializer = new(typeof(BindingList<ScriptInfo>));
    private BindingList<ScriptInfo>? _scripts;
    private List<ScriptInfo> _scriptsFormerlyShownInUserMenuBar = [];

    public BindingList<ScriptInfo> GetScripts()
    {
        if (_scripts is null)
        {
            _scripts = DeserializeFromXml(AppSettings.OwnScripts, out List<int> userMenuBarScriptIndices);
            FixAmbiguousHotkeyCommandIdentifiers(_scripts);

            // Kept as instances rather than identifiers, which the fix-up above may have changed.
            _scriptsFormerlyShownInUserMenuBar = [.. userMenuBarScriptIndices.Select(index => _scripts[index])];
        }

        return _scripts;

        static void FixAmbiguousHotkeyCommandIdentifiers(BindingList<ScriptInfo> loadedScripts)
        {
            HashSet<int> ids = [];

            foreach (ScriptInfo script in loadedScripts)
            {
                if (!ids.Add(script.HotkeyCommandIdentifier))
                {
                    script.HotkeyCommandIdentifier = NextHotkeyCommandIdentifier(loadedScripts);
                }
            }
        }
    }

    public IReadOnlyList<ScriptInfo> TakeScriptsFormerlyShownInUserMenuBar()
    {
        GetScripts();

        List<ScriptInfo> scripts = _scriptsFormerlyShownInUserMenuBar;
        _scriptsFormerlyShownInUserMenuBar = [];
        return scripts;
    }

    public ScriptInfo? GetScript(int scriptId)
    {
        foreach (ScriptInfo script in GetScripts())
        {
            if (script.HotkeyCommandIdentifier == scriptId)
            {
                return script;
            }
        }

        return null;
    }

    public bool RunEventScripts<THostForm>(ScriptEvent scriptEvent, THostForm form)
        where THostForm : IGitModuleForm, IScriptOptionsForm, IWin32Window
    {
        foreach (ScriptInfo script in GetScripts().Where(scriptInfo => scriptInfo.Enabled && scriptInfo.OnEvent == scriptEvent))
        {
            bool executed = ScriptRunner.RunScript(script, owner: form, form.UICommands, form.GetScriptOptionsProvider());
            if (!executed)
            {
                return false;
            }
        }

        return true;
    }

    public bool RunScript(ScriptInfo scriptInfo, IWin32Window owner, IGitUICommands commands, IScriptOptionsProvider scriptOptionsProvider)
    {
        return ScriptRunner.RunScript(scriptInfo, owner, commands, scriptOptionsProvider);
    }

    public string SerializeIntoXml()
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
            return null!;
        }
    }

    // userMenuBarScriptIndices receives the position, in the returned list, of each script saved with
    // the former ShowInUserMenuBar event (see ReadLegacyUserMenuBarEvent).
    internal static BindingList<ScriptInfo> DeserializeFromXml(string? xml, out List<int> userMenuBarScriptIndices)
    {
        userMenuBarScriptIndices = [];

        // When there is nothing to deserialize, add default scripts
        if (string.IsNullOrEmpty(xml))
        {
            return GetDefaultScripts();
        }

        try
        {
            using StringReader stringReader = new(ReadLegacyUserMenuBarEvent(xml, userMenuBarScriptIndices));
            using XmlTextReader xmlReader = new(stringReader);
            return (BindingList<ScriptInfo>)_serializer.Deserialize(xmlReader)!;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
            userMenuBarScriptIndices.Clear();
            return DeserializeFromOldFormat(xml);
        }

        BindingList<ScriptInfo> GetDefaultScripts() =>
        [
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
                HotkeyCommandIdentifier = 9005,
                Icon = "EditFile",
                Name = "&Open in VS Code",
                Command = "bash",
                Arguments = "-c '"
                    + @"if [ -z ""{SelectedRelativePaths}"" ]; then code .; "
                    + @"elif [ -d ""{SelectedRelativePaths}"" ]; then code {{SelectedRelativePaths}}; "
                    + @"elif [ ! -f ""{SelectedRelativePaths}"" ]; then code . --goto {{SelectedRelativePaths}}; "
                    + @"else code . --goto {{SelectedRelativePaths}}:{LineNumber}:{ColumnNumber}; "
                    + @"fi'",
                RunInBackground = true,
                AskConfirmation = false,
                OnEvent = ScriptEvent.None,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            },
            new ScriptInfo
            {
                HotkeyCommandIdentifier = 9002,
                Name = "&Example",
                Command = @"c:\windows\system32\calc.exe",
                Arguments = "",
                RunInBackground = false,
                AskConfirmation = false,
                OnEvent = ScriptEvent.None,
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
            },
            new ScriptInfo
            {
                HotkeyCommandIdentifier = 9006,
                Name = "Convert workspace file to LF",
                Command = "bash.exe",
                Arguments = "-c 'dos2unix.exe {{SelectedRelativePaths}}'",
                RunInBackground = true,
                AskConfirmation = false,
                OnEvent = 0,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            },
            new ScriptInfo
            {
                HotkeyCommandIdentifier = 9007,
                Name = "Convert workspace file to CRLF",
                Command = "bash.exe",
                Arguments = "-c 'unix2dos.exe {{SelectedRelativePaths}}'",
                RunInBackground = true,
                AskConfirmation = false,
                OnEvent = 0,
                AddToRevisionGridContextMenu = false,
                Enabled = false
            }
        ];

        BindingList<ScriptInfo> DeserializeFromOldFormat(string inputString)
        {
            const string paramSeparator = "<_PARAM_SEPARATOR_>";
            const string scriptSeparator = "<_SCRIPT_SEPARATOR_>";

            BindingList<ScriptInfo> scripts = [];

            if (inputString.Contains(paramSeparator) || inputString.Contains(scriptSeparator))
            {
                foreach (string script in inputString.Split([scriptSeparator], StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parameters = script.Split([paramSeparator], StringSplitOptions.None);

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

    // ShowInUserMenuBar, which put a script on the Scripts toolbar, is no longer a ScriptEvent: the
    // toolbar layout says where a script goes, as it does for any action. The serializer would reject
    // the whole list over the one value it no longer knows, so a script saved with it is read as
    // having no event, and its position among the scripts is added to scriptIndices so that its
    // place on the toolbar can be carried over into the layout.
    private static string ReadLegacyUserMenuBarEvent(string xml, List<int> scriptIndices)
    {
        if (!xml.Contains(LegacyShowInUserMenuBar, StringComparison.Ordinal))
        {
            return xml;
        }

        XDocument document = XDocument.Parse(xml);
        List<XElement> scripts = document.Root?.Elements(nameof(ScriptInfo)).ToList() ?? [];

        for (int index = 0; index < scripts.Count; index++)
        {
            XElement? onEvent = scripts[index].Element(nameof(ScriptInfo.OnEvent));
            if (onEvent?.Value == LegacyShowInUserMenuBar)
            {
                onEvent.Value = nameof(ScriptEvent.None);
                scriptIndices.Add(index);
            }
        }

        return document.ToString();
    }

    internal static int NextHotkeyCommandIdentifier(BindingList<ScriptInfo> scripts)
    {
        return scripts.Select(s => s.HotkeyCommandIdentifier).Max() + 1;
    }
}

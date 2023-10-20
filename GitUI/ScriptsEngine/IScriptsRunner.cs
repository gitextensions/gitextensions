using GitCommands;
using ResourceManager;

namespace GitUI.ScriptsEngine
{
    public interface IScriptsRunner
    {
        bool RunEventScripts(ScriptEvent scriptEvent, IScriptHostControl scriptHostControl);

        bool RunScript(int scriptId, IScriptHostControl scriptHostControl);
    }
}

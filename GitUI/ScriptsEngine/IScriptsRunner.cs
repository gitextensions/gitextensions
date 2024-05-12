using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitUI.ScriptsEngine
{
    public interface IScriptsRunner
    {
        bool RunEventScripts<THostForm>(ScriptEvent scriptEvent, THostForm form)
            where THostForm : IGitModuleForm, IWin32Window;

        bool RunScript(ScriptInfo scriptInfo, IWin32Window owner, IGitUICommands commands, IScriptOptionsProvider? scriptOptionsProvider = null);
    }
}

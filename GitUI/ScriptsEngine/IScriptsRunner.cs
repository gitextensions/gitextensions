using GitCommands;
using ResourceManager;

namespace GitUI.ScriptsEngine
{
    public interface IScriptsRunner
    {
        bool RunEventScripts<THostForm>(ScriptEvent scriptEvent, THostForm form)
            where THostForm : IGitModuleForm, IWin32Window;

        bool RunScript<THostForm>(int scriptId, THostForm form, RevisionGridControl? revisionGrid = null)
            where THostForm : IGitModuleForm, IWin32Window;
    }
}

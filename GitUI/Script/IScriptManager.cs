using System.ComponentModel;

namespace GitUI.Script
{
    internal interface IScriptManager
    {
        BindingList<ScriptInfo> GetScripts();

        ScriptInfo GetScript(string key);

        void RunEventScripts(GitModuleForm form, ScriptEvent scriptEvent);

        string SerializeIntoXml();

        int NextHotKeyCommandIdentifier();
    }
}
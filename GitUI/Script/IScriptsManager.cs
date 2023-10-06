using System.ComponentModel;

namespace GitUI.Script
{
    public interface IScriptsManager
    {
        ScriptInfo? GetScript(int scriptId);

        BindingList<ScriptInfo> GetScripts();

        string SerializeIntoXml();
    }
}

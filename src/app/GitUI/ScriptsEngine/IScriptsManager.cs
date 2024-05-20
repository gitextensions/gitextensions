using System.ComponentModel;

namespace GitUI.ScriptsEngine
{
    public interface IScriptsManager
    {
        ScriptInfo? GetScript(int scriptId);

        BindingList<ScriptInfo> GetScripts();

        string SerializeIntoXml();
    }
}

using System.ComponentModel;

namespace GitUI.ScriptsEngine;

public interface IScriptsManager
{
    ScriptInfo? GetScript(int scriptId);

    BindingList<ScriptInfo> GetScripts();

    string SerializeIntoXml();

    /// <summary>
    ///  Returns, once, the scripts saved with the former <c>ShowInUserMenuBar</c> event, which put
    ///  a script on the Scripts toolbar. They are loaded with no event instead: the toolbar layout
    ///  places scripts now, and these are for it to take over.
    /// </summary>
    IReadOnlyList<ScriptInfo> TakeScriptsFormerlyShownInUserMenuBar();
}

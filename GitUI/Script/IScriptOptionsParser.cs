using GitUIPluginInterfaces;

namespace GitUI.Script
{
    internal interface IScriptOptionsParser
    {
        (string argument, bool abort) Parse(string argument, IGitModule module, RevisionGridControl revisionGrid);
    }
}

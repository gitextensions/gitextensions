using GitCommands;

namespace GitUI.Script
{
    internal interface IScriptRunner
    {
        /// <summary>
        /// Tries to run scripts identified by a <paramref name="command"/>
        /// </summary>
        CommandStatus ExecuteScriptCommand(int command);

        CommandStatus RunScript(string scriptKey);
    }
}

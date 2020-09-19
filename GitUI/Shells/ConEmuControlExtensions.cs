#nullable enable

using System.Threading.Tasks;
using ConEmu.WinForms;

namespace GitUI.Shells
{
    public static class ConEmuControlExtensions
    {
        private const string TerminalCloseMacro = "Close";
        private const int TerminalCloseMacroFirstParameter = 0;
        private const int TerminalCloseMacroSecondParameter = 1;

        public static Task CloseTerminal(this ConEmuControl terminal)
        {
            if (terminal is null || terminal.RunningSession is null)
            {
                return Task.CompletedTask;
            }

            return terminal.RunningSession
                .BeginGuiMacro(TerminalCloseMacro)
                .WithParam(TerminalCloseMacroFirstParameter)
                .WithParam(TerminalCloseMacroSecondParameter)
                .ExecuteAsync();
        }
    }
}

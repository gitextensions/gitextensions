using System.Collections.Generic;
using System.Linq;
using ConEmu.WinForms;

namespace GitUI.Shells
{
    public class ShellProvider
    {
        private static IShellDescriptor DefaultShell = new BashShell();
        private static readonly IShellDescriptor[] Shells = { DefaultShell, new CmdShell(), new PwshShell(), new PowerShellShell() };

        public IReadOnlyList<IShellDescriptor> GetShells() => Shells;

        public IShellDescriptor GetShell(string? name) => Shells.FirstOrDefault(s => s.Name == name) ?? DefaultShell;

        public string? GetShellCommandLine(string? shellType)
        {
            var shell = GetShell(shellType);

            if (!shell.HasExecutable)
            {
                return ConEmuConstants.DefaultConsoleCommandLine;
            }

            return shell.ExecutableCommandLine;
        }
    }
}

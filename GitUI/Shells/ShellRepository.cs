#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using Newtonsoft.Json;

namespace GitUI.Shells
{
    internal interface IShellRepository
    {
        void Add(Shell shell);

        void Remove(Shell shell);

        void Save();

        IEnumerable<Shell> Query();
    }

    internal sealed class ShellRepository : IShellRepository
    {
        private const string BashShellId = "8124BE00-46EC-44E9-BB7A-B159A1F6AC3D";
        private const string BashShellName = "bash";
        private const string BashShellIcon = nameof(Properties.Images.GitForWindows);
        private const string BashShellPath = @"C:\Program Files\Git\bin\bash.exe";
        private const string BashShellArguments = "--login -i";

        private const string CmdShellId = "90F358CA-3FD2-4588-AD21-8F1CFA8E2B14";
        private const string CmdShellName = "cmd";
        private const string CmdShellIcon = nameof(Properties.Images.cmd);
        private const string CmdShellPath = @"C:\WINDOWS\system32\cmd.exe";
        private const string CmdShellArguments = "";

        private const string PoweShellShellId = "1D3FF675-FFA5-45DC-8947-5B370EA661F6";
        private const string PoweShellShellName = "powershell";
        private const string PoweShellShellIcon = nameof(Properties.Images.powershell);
        private const string PoweShellShellPath = @"C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe";
        private const string PoweShellShellArguments = "";

        private const string PwshShellId = "E14E9C8D-C9C7-43E1-AB60-72096FE82567";
        private const string PwshShellName = "pwsh";
        private const string PwshShellIcon = nameof(Properties.Images.pwsh);
        private const string PwshShellPath = "";
        private const string PwshShellArguments = "";

        private static List<Shell> _shells = new List<Shell>();

        public ShellRepository()
        {
            if (AppSettings.Shells.Value == AppSettings.Shells.Default)
            {
                _shells = new List<Shell>(PredefinedShells());
            }
            else
            {
                _shells = JsonConvert
                    .DeserializeObject<List<Shell>>(AppSettings.Shells.Value);
            }
        }

        public void Add(Shell shell)
        {
            _shells.Add(shell);
        }

        public void Remove(Shell shell)
        {
            _shells.Remove(shell);
        }

        public void Save()
        {
            AppSettings.Shells.Value = JsonConvert
                .SerializeObject(_shells);
        }

        public IEnumerable<Shell> Query()
        {
            return _shells
                .AsEnumerable();
        }

        private static IEnumerable<Shell> PredefinedShells()
        {
            var oldShellName = AppSettings.GetString("ConEmuTerminal", BashShellName);

            yield return new Shell
            {
                Id = Guid.Parse(BashShellId),
                Name = BashShellName,
                Icon = BashShellIcon,
                Command = BashShellPath,
                Arguments = BashShellArguments,
                Default = oldShellName is BashShellName,
                Enabled = true
            };

            yield return new Shell
            {
                Id = Guid.Parse(CmdShellId),
                Name = CmdShellName,
                Icon = CmdShellIcon,
                Command = CmdShellPath,
                Arguments = CmdShellArguments,
                Default = oldShellName is CmdShellName,
                Enabled = true
            };

            yield return new Shell
            {
                Id = Guid.Parse(PoweShellShellId),
                Name = PoweShellShellName,
                Icon = PoweShellShellIcon,
                Command = PoweShellShellPath,
                Arguments = PoweShellShellArguments,
                Default = oldShellName is PoweShellShellName,
                Enabled = true
            };

            yield return new Shell
            {
                Id = Guid.Parse(PwshShellId),
                Name = PwshShellName,
                Icon = PwshShellIcon,
                Command = PwshShellPath,
                Arguments = PwshShellArguments,
                Default = oldShellName is PwshShellName,
                Enabled = true
            };
        }
    }
}

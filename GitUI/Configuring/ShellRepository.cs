using System.Collections.Generic;
using System.Linq;
using GitCommands;
using Newtonsoft.Json;

namespace GitUI.Configuring
{
    internal interface IShellRepository
    {
        void Save();

        IEnumerable<Shell> Query();

        Shell FindByName(string name);

        Shell FindByDefault();
    }

    internal sealed class ShellRepository : IShellRepository
    {
        private const string BashShellName = "bash";
        private const string BashShellPath = @"C:\Program Files\Git\bin\bash.exe";
        private const string BashShellArguments = "--login -i";

        private const string CmdShellName = "cmd";
        private const string CmdShellPath = @"C:\WINDOWS\system32\cmd.exe";
        private const string CmdShellArguments = "";

        private const string PoweShellShellName = "powershell";
        private const string PoweShellShellPath = "";
        private const string PoweShellShellArguments = "";

        private const string PwshShellName = "pwsh";
        private const string PwshShellPath = @"C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe";
        private const string PwshShellArguments = "";

        private static List<Shell> _shells;

        public void Save()
        {
            if (_shells == null)
            {
                return;
            }

            AppSettings.Shells.Value = JsonConvert
                .SerializeObject(_shells);
        }

        public IEnumerable<Shell> Query()
        {
            if (_shells == null)
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

            return _shells
                .AsEnumerable();
        }

        public Shell FindByName(string name)
        {
            return Query()
                .FirstOrDefault(x => x.Name == name);
        }

        public Shell FindByDefault()
        {
            return Query()
                .FirstOrDefault(x => x.Default);
        }

        private static IEnumerable<Shell> PredefinedShells()
        {
            yield return new Shell
            {
                Name = BashShellName,
                Path = BashShellPath,
                Arguments = BashShellArguments,
                Enabled = true,
                Default = true
            };

            yield return new Shell
            {
                Name = CmdShellName,
                Path = CmdShellPath,
                Arguments = CmdShellArguments,
                Enabled = true,
                Default = false
            };

            yield return new Shell
            {
                Name = PoweShellShellName,
                Path = PoweShellShellPath,
                Arguments = PoweShellShellArguments,
                Enabled = true,
                Default = false
            };

            yield return new Shell
            {
                Name = PwshShellName,
                Path = PwshShellPath,
                Arguments = PwshShellArguments,
                Enabled = true,
                Default = false
            };
        }
    }
}

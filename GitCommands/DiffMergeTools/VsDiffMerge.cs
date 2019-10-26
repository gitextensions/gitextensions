using System;
using System.Collections.Generic;
using System.IO;
using GitCommands.Utils;
using Microsoft.Win32;

namespace GitCommands.DiffMergeTools
{
    internal class VsDiffMerge : DiffMergeTool
    {
        private const string ExeName = "vsdiffmerge.exe";

        /// <inheritdoc />
        public override string MergeCommand => "/m \"$REMOTE\" \"$LOCAL\" \"$BASE\" \"$MERGED\"";

        /// <inheritdoc />
        public override string ExeFileName => ExeName;

        /// <inheritdoc />
        public override string Name => "vsdiffmerge";

        /// <inheritdoc />
        public override IEnumerable<string> SearchPaths => new[]
        {
            GetVsDiffMergePath()
        };

        private static string GetVsDiffMergePath()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return ExeName;
            }

            // For 2017 (15.0) and later, VsDiffMerge is not installed by default but often included
            // C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer
            var vsVersions = new[] { "14.0", "12.0", "11.0" };

            foreach (var version in vsVersions)
            {
                string registryKeyString = $@"SOFTWARE{(Environment.Is64BitProcess ? @"\Wow6432Node\" : "\\")}Microsoft\VisualStudio\{version}";
                using (RegistryKey localMachineKey = Registry.LocalMachine.OpenSubKey(registryKeyString))
                {
                    var path = localMachineKey?.GetValue("InstallDir") as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        return Path.Combine(path, ExeName);
                    }
                }
            }

            return ExeName;
        }
    }
}

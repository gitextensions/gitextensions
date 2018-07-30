using System;
using System.Collections.Generic;
using System.IO;
using GitCommands.Utils;

namespace GitCommands
{
    public class PathEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string path1, string path2)
        {
            path1 = Path.GetFullPath(path1).TrimEnd('\\');
            path2 = Path.GetFullPath(path2).TrimEnd('\\');
            var comparison = !EnvUtils.RunningOnWindows()
                ? StringComparison.InvariantCulture
                : StringComparison.InvariantCultureIgnoreCase;

            return string.Compare(path1, path2, comparison) == 0;
        }

        public int GetHashCode(string path)
        {
            path = Path.GetFullPath(path).TrimEnd('\\');
            if (EnvUtils.RunningOnWindows())
            {
                path = path.ToLower();
            }

            return path.GetHashCode();
        }
    }
}
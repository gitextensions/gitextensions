using System;
using System.Collections.Generic;
using System.IO;
using GitCommands.Utils;

namespace GitCommands
{
    public static class PathUtil
    {
        /// <summary>
        /// Code guideline: "A directory path should always end with / or \.
        /// Better use Path.Combine instead of Setting.PathSeparator"
        /// 
        /// This method can be used to add (or keep) a trailing path separator character to a directory path.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string EnsureTrailingPathSeparator(string dirPath)
        {
            if (!dirPath.IsNullOrEmpty() && !dirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                dirPath += Path.DirectorySeparatorChar;
            }

            return dirPath;
        }

        public static bool Equal(string path1, string path2)
        {
            path1 = Path.GetFullPath(path1).TrimEnd('\\');
            path2 = Path.GetFullPath(path2).TrimEnd('\\');
            StringComparison comprasion = EnvUtils.RunningOnUnix()
                                              ? StringComparison.InvariantCulture
                                              : StringComparison.InvariantCultureIgnoreCase;

            return String.Compare(path1, path2, comprasion) == 0;
        }

        private class PathEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string path1, string path2)
            {
                return Equal(path1, path2);
            }

            public int GetHashCode(string path)
            {
                path = Path.GetFullPath(path).TrimEnd('\\');
                if (!EnvUtils.RunningOnUnix())
                    path = path.ToLower();
                return path.GetHashCode();
            }
        }

        public static IEqualityComparer<string> CreatePathEqualityComparer()
        {
            return new PathEqualityComparer();
        }
    }
}

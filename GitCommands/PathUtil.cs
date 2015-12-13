using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GitCommands.Utils;

namespace GitCommands
{
    public static class PathUtil
    {
        /// <summary>Replaces native path separator with posix path separator.</summary>
        public static string ToPosixPath(this string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, AppSettings.PosixPathSeparator);
        }

        /// <summary>Replaces '\' with '/'.</summary>
        public static string ToNativePath(this string path)
        {
            return path.Replace(AppSettings.PosixPathSeparator, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Code guideline: "A directory path should always end with / or \.
        /// Better use Path.Combine instead of Setting.PathSeparator"
        ///
        /// This method can be used to add (or keep) a trailing path separator character to a directory path.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string EnsureTrailingPathSeparator(this string dirPath)
        {
            if (!dirPath.IsNullOrEmpty() &&
                dirPath[dirPath.Length - 1] != Path.DirectorySeparatorChar &&
                dirPath[dirPath.Length - 1] != AppSettings.PosixPathSeparator)
            {
                dirPath += Path.DirectorySeparatorChar;
            }

            return dirPath;
        }

        public static bool IsLocalFile(string fileName)
        {
            Regex regex = new Regex(@"^(\w+):\/\/([\S]+)");
            if (regex.IsMatch(fileName))
            {
                return false;
            }
            return true;
        }

        public static string GetFileName(string fileName)
        {
            var pathSeparators = new[] { Path.DirectorySeparatorChar, AppSettings.PosixPathSeparator };
            var pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
                fileName = fileName.Substring(pos + 1);
            return fileName;
        }

        public static string GetDirectoryName(string fileName)
        {
            var pathSeparators = new[] { Path.DirectorySeparatorChar, AppSettings.PosixPathSeparator };
            var pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
                fileName = fileName.Substring(0, pos);
            if (fileName.Length == 2 && char.IsLetter(fileName[0]) && fileName[1] == Path.VolumeSeparatorChar)
                return "";
            return fileName;
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

        public static string GetRepositoryName(string repositoryUrl)
        {
            string name = "";

            if (repositoryUrl != null)
            {
                const string standardRepositorySuffix = ".git";
                string path = repositoryUrl.TrimEnd(new[] { '\\', '/' });

                if (path.EndsWith(standardRepositorySuffix))
                    path = path.Substring(0, path.Length - standardRepositorySuffix.Length);

                if (path.Contains("\\") || path.Contains("/"))
                    name = path.Substring(path.LastIndexOfAny(new[] { '\\', '/' }) + 1);
            }

            return name;
        }
    }
}

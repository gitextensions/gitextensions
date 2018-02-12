using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GitCommands
{
    public static class PathUtil
    {
        private static readonly IEnvironmentAbstraction EnvironmentAbstraction = new EnvironmentAbstraction();
        private static readonly IEnvironmentPathsProvider EnvironmentPathsProvider = new EnvironmentPathsProvider(EnvironmentAbstraction);


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
            {
                if (pos == 0 && fileName[0] == AppSettings.PosixPathSeparator)
                {
                    return fileName.Length == 1 ? string.Empty : AppSettings.PosixPathSeparator.ToString();
                }
                else
                {
                    fileName = fileName.Substring(0, pos);
                }
            }
            if (fileName.Length == 2 && char.IsLetter(fileName[0]) && fileName[1] == Path.VolumeSeparatorChar)
                return "";
            return fileName;
        }

        public static bool TryConvertWindowsPathToPosix(string path, out string posixPath)
        {
            posixPath = null;
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                return false;
            posixPath = "/" + directoryInfo.FullName.ToPosixPath().Remove(1, 1);
            return true;
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

                
        public static bool TryFindFullPath(string fileName, out string fullPath)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    fullPath = Path.GetFullPath(fileName);
                    return true;
                }

                foreach (var path in EnvironmentPathsProvider.GetEnvironmentValidPaths())
                {
                    fullPath = Path.Combine(path, fileName);
                    if (File.Exists(fullPath))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // do nothing 
            }
            fullPath = null;
            return false;
        }

        public static bool TryFindShellPath(string shell, out string shellPath)
        {
            try
            {
                shellPath = Path.Combine(EnvironmentAbstraction.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", shell);
                if (File.Exists(shellPath))
                {
                    return true;
                }
                shellPath = Path.Combine(AppSettings.GitBinDir, shell);
                if (File.Exists(shellPath))
                {
                    return true;
                }
                if (TryFindFullPath(shell, out shellPath))
                {
                    return true;
                }
            }
            catch
            {
                // do nothing 
            }
            shellPath = null;
            return false;
        }

    }
}

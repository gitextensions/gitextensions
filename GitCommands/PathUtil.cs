﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class PathUtil
    {
        private static readonly IEnvironmentAbstraction EnvironmentAbstraction = new EnvironmentAbstraction();
        private static readonly IEnvironmentPathsProvider EnvironmentPathsProvider = new EnvironmentPathsProvider(EnvironmentAbstraction);

        /// <summary>Replaces native path separator with posix path separator.</summary>
        [NotNull]
        public static string ToPosixPath([NotNull] this string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, AppSettings.PosixPathSeparator);
        }

        /// <summary>Replaces '\' with '/'.</summary>
        [NotNull]
        public static string ToNativePath([NotNull] this string path)
        {
            return path.Replace(AppSettings.PosixPathSeparator, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Code guideline: "A directory path should always end with / or \.
        /// Better use Path.Combine instead of Setting.PathSeparator"
        ///
        /// This method can be used to add (or keep) a trailing path separator character to a directory path.
        /// </summary>
        [ContractAnnotation("dirPath:null=>null")]
        [ContractAnnotation("dirPath:notnull=>notnull")]
        public static string EnsureTrailingPathSeparator([CanBeNull] this string dirPath)
        {
            if (!dirPath.IsNullOrEmpty() &&
                dirPath[dirPath.Length - 1] != Path.DirectorySeparatorChar &&
                dirPath[dirPath.Length - 1] != AppSettings.PosixPathSeparator)
            {
                dirPath += Path.DirectorySeparatorChar;
            }

            return dirPath;
        }

        public static bool IsLocalFile([NotNull] string fileName)
        {
            return !Regex.IsMatch(fileName, @"^(\w+):\/\/([\S]+)");
        }

        /// <summary>
        /// A naive way to check whethere the given path is a URL by checking
        /// whether it starts with either 'http', 'ssh' or 'git'.
        /// </summary>
        /// <param name="path">A path to check.</param>
        /// <returns><see langword="true"/> if the given path starts with 'http', 'ssh' or 'git'; otherwise <see langword="false"/>.</returns>
        [ContractAnnotation("path:null=>false")]
        [Pure]
        public static bool IsUrl(string path)
        {
            return !string.IsNullOrEmpty(path) &&
                   (path.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ||
                    path.StartsWith("git", StringComparison.CurrentCultureIgnoreCase) ||
                    path.StartsWith("ssh", StringComparison.CurrentCultureIgnoreCase));
        }

        [NotNull]
        public static string GetFileName([NotNull] string fileName)
        {
            var pathSeparators = new[] { Path.DirectorySeparatorChar, AppSettings.PosixPathSeparator };
            var pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
            {
                fileName = fileName.Substring(pos + 1);
            }

            return fileName;
        }

        [NotNull]
        public static string GetDirectoryName([NotNull] string fileName)
        {
            var pathSeparators = new[] { Path.DirectorySeparatorChar, AppSettings.PosixPathSeparator };
            var pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
            {
                if (pos == 0 && fileName[0] == AppSettings.PosixPathSeparator)
                {
                    return fileName.Length == 1 ? string.Empty : AppSettings.PosixPathSeparator.ToString();
                }

                fileName = fileName.Substring(0, pos);
            }

            if (fileName.Length == 2 && char.IsLetter(fileName[0]) && fileName[1] == Path.VolumeSeparatorChar)
            {
                return "";
            }

            return fileName;
        }

        [ContractAnnotation("=>false,posixPath:null")]
        [ContractAnnotation("=>true,posixPath:notnull")]
        public static bool TryConvertWindowsPathToPosix([NotNull] string path, out string posixPath)
        {
            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
            {
                posixPath = null;
                return false;
            }

            posixPath = "/" + directoryInfo.FullName.ToPosixPath().Remove(1, 1);
            return true;
        }

        [NotNull]
        public static string GetRepositoryName([CanBeNull] string repositoryUrl)
        {
            string name = "";

            if (repositoryUrl != null)
            {
                const string standardRepositorySuffix = ".git";
                string path = repositoryUrl.TrimEnd('\\', '/');

                if (path.EndsWith(standardRepositorySuffix))
                {
                    path = path.Substring(0, path.Length - standardRepositorySuffix.Length);
                }

                if (path.Contains("\\") || path.Contains("/"))
                {
                    name = path.Substring(path.LastIndexOfAny(new[] { '\\', '/' }) + 1);
                }
            }

            return name;
        }

        [ContractAnnotation("=>false,fullPath:null")]
        [ContractAnnotation("=>true,fullPath:notnull")]
        public static bool TryFindFullPath([NotNull] string fileName, out string fullPath)
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

        [ContractAnnotation("=>false,shellPath:null")]
        [ContractAnnotation("=>true,shellPath:notnull")]
        public static bool TryFindShellPath([NotNull] string shell, out string shellPath)
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

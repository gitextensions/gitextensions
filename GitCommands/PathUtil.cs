using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GitCommands.Utils;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class PathUtil
    {
        private static readonly IEnvironmentAbstraction EnvironmentAbstraction = new EnvironmentAbstraction();
        private static readonly IEnvironmentPathsProvider EnvironmentPathsProvider = new EnvironmentPathsProvider(EnvironmentAbstraction);

        public static readonly char PosixDirectorySeparatorChar = '/';
        public static readonly char NativeDirectorySeparatorChar = Path.DirectorySeparatorChar;

        /// <summary>Replaces native path separator with posix path separator.</summary>
        [NotNull]
        public static string ToPosixPath([NotNull] this string path)
        {
            return path.Replace(NativeDirectorySeparatorChar, PosixDirectorySeparatorChar);
        }

        /// <summary>Replaces '\' with '/'.</summary>
        [NotNull]
        public static string ToNativePath([NotNull] this string path)
        {
            return path.Replace(PosixDirectorySeparatorChar, NativeDirectorySeparatorChar);
        }

        /// <summary>
        /// Removes any trailing path separator character from the end of <paramref name="dirPath"/>.
        /// </summary>
        [ContractAnnotation("dirPath:null=>null")]
        [ContractAnnotation("dirPath:notnull=>notnull")]
        public static string RemoveTrailingPathSeparator([CanBeNull] this string dirPath)
        {
            if (dirPath?.Length > 0 &&
                (dirPath[dirPath.Length - 1] == NativeDirectorySeparatorChar ||
                 dirPath[dirPath.Length - 1] == PosixDirectorySeparatorChar))
            {
                return dirPath.Substring(0, dirPath.Length - 1);
            }

            return dirPath;
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
            if (!string.IsNullOrEmpty(dirPath) &&
                dirPath[dirPath.Length - 1] != NativeDirectorySeparatorChar &&
                dirPath[dirPath.Length - 1] != PosixDirectorySeparatorChar)
            {
                dirPath += NativeDirectorySeparatorChar;
            }

            return dirPath;
        }

        public static bool IsLocalFile([NotNull] string fileName)
        {
            return !Regex.IsMatch(fileName, @"^(\w+):\/\/([\S]+)");
        }

        /// <summary>
        /// A naive way to check whether the given path is a URL by checking
        /// whether it starts with either 'http', 'ssh' or 'git'.
        /// </summary>
        /// <param name="path">A path to check.</param>
        /// <returns><see langword="true"/> if the given path starts with 'http', 'ssh' or 'git'; otherwise <see langword="false"/>.</returns>
        [ContractAnnotation("path:null=>false")]
        [Pure]
        public static bool IsUrl(string path)
        {
            return !string.IsNullOrEmpty(path)
                && (path.StartsWith("http:", StringComparison.CurrentCultureIgnoreCase)
                 || path.StartsWith("https:", StringComparison.CurrentCultureIgnoreCase)
                 || path.StartsWith("git:", StringComparison.CurrentCultureIgnoreCase)
                 || path.StartsWith("ssh:", StringComparison.CurrentCultureIgnoreCase)
                 || path.StartsWith("file:", StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool CanBeGitURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return IsUrl(url)
                   || url.EndsWith(".git", StringComparison.CurrentCultureIgnoreCase)
                   || GitModule.IsValidGitWorkingDir(url);
        }

        [NotNull]
        public static string GetFileName([NotNull] string fileName)
        {
            var pathSeparators = new[] { NativeDirectorySeparatorChar, PosixDirectorySeparatorChar };
            var pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
            {
                fileName = fileName.Substring(pos + 1);
            }

            return fileName;
        }

        [NotNull]
        public static string NormalizePath([NotNull] this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            try
            {
                return Path.GetFullPath(Resolve(path));
            }
            catch (UriFormatException)
            {
                return string.Empty;
            }
        }

        [NotNull]
        public static string Resolve([NotNull] string path, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return IsWslPath(path) ? ResolveWsl(path, relativePath) : ResolveRelativePath(path, relativePath);
        }

        /// <summary>
        /// Special handling of on purpose invalid WSL machine name in Windows 10
        /// </summary>
        [NotNull]
        internal static string ResolveWsl([NotNull] string path, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(path) || !IsWslPath(path))
            {
                throw new ArgumentException(nameof(path));
            }

            // Temporarily replace machine name with a valid name (remove $ sign from \\wsl$\)
            path = path.Remove(5, 1);

            path = ResolveRelativePath(path, relativePath);

            // Revert temporary replacement of WSL machine name (add $ sign back)
            return path.Insert(5, "$");
        }

        [NotNull]
        private static string ResolveRelativePath([NotNull] string path, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            Uri tempPath = new Uri(path);
            if (!string.IsNullOrEmpty(relativePath))
            {
                tempPath = new Uri(tempPath, relativePath);
            }

            return tempPath.LocalPath;
        }

        internal static bool IsWslPath([NotNull] string path)
        {
            return path.ToLower().StartsWith(@"\\wsl$\");
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

        [NotNull]
        public static string GetDisplayPath([NotNull] string path)
        {
            // TODO verify whether the user profile contains forwards/backwards slashes on other platforms
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var comparison = EnvUtils.RunningOnWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (path.StartsWith(userProfile, comparison))
            {
                var length = path.Length - userProfile.Length;
                if (path.EndsWith("/") || path.EndsWith("\\"))
                {
                    length--;
                }

                return $"~{path.Substring(userProfile.Length, length)}";
            }

            return path;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<string> FindAncestors([NotNull] string path)
        {
            path = path.RemoveTrailingPathSeparator();

            if (string.IsNullOrWhiteSpace(path))
            {
                yield break;
            }

            while (true)
            {
                path = Path.GetDirectoryName(path);

                if (string.IsNullOrEmpty(path))
                {
                    yield break;
                }

                yield return path.EnsureTrailingPathSeparator();
            }
        }

        public static string FindInFolders([NotNull] this string fileName, [NotNull] IEnumerable<string> folders)
        {
            foreach (string location in folders)
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    continue;
                }

                string fullName;
                if (Path.IsPathRooted(location))
                {
                    fullName = FindFile(location, fileName);
                    if (fullName != null)
                    {
                        return fullName;
                    }

                    continue;
                }

                fullName = FindFileInEnvVarFolder("ProgramFiles", location, fileName);
                if (fullName != null)
                {
                    return fullName;
                }

                fullName = FindFileInEnvVarFolder("ProgramW6432", location, fileName);
                if (fullName != null)
                {
                    return fullName;
                }

                if (IntPtr.Size == 8 || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                {
                    fullName = FindFileInEnvVarFolder("ProgramFiles(x86)", location, fileName);
                    if (fullName != null)
                    {
                        return fullName;
                    }
                }
            }

            return string.Empty;

            string FindFileInEnvVarFolder(string environmentVariable, string location, string fileName1)
            {
                var envVarFolder = Environment.GetEnvironmentVariable(environmentVariable);
                if (string.IsNullOrEmpty(envVarFolder))
                {
                    return null;
                }

                var path = Path.Combine(envVarFolder, location);
                if (!Directory.Exists(path))
                {
                    return null;
                }

                return FindFile(path, fileName1);
            }

            string FindFile(string location, string fileName1)
            {
                string fullName = Path.Combine(location, fileName1);
                if (File.Exists(fullName))
                {
                    return fullName;
                }

                return null;
            }
        }

        /// <summary>
        ///  Deletes the requested folder recursively.
        /// </summary>
        /// <returns>
        ///  <see langword="true" /> if the folder is absent or successfully removed; otherwise <see langword="false" />.
        /// </returns>
        [ContractAnnotation("=>false,errorMessage:notnull")]
        [ContractAnnotation("=>true,errorMessage:null")]
        public static bool TryDeleteDirectory([NotNull] this string path, out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return true;
            }

            try
            {
                Directory.Delete(path, recursive: true);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            return true;
        }
    }
}

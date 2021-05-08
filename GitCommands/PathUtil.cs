using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;
using GitCommands.Utils;
using GitExtUtils;

namespace GitCommands
{
    public static class PathUtil
    {
        private static readonly IEnvironmentAbstraction EnvironmentAbstraction = new EnvironmentAbstraction();
        private static readonly IEnvironmentPathsProvider EnvironmentPathsProvider = new EnvironmentPathsProvider(EnvironmentAbstraction);

        // URL regex obtained from https://www.regextester.com/53716 with some modifications
        private static readonly Regex UrlRegex = new Regex(
            @"(?:(?:https?|ftp|file|ssh|git):\/\/|www\.)(?:[-A-Z0-9+&@#\/%=~_|$?!:,.])*(?:[A-Z0-9+&@#\/%=~_|$])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly char PosixDirectorySeparatorChar = '/';
        public static readonly char NativeDirectorySeparatorChar = Path.DirectorySeparatorChar;

        /// <summary>Replaces native path separator with posix path separator.</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToPosixPath(this string? path)
        {
            return path?.Replace(NativeDirectorySeparatorChar, PosixDirectorySeparatorChar);
        }

        /// <summary>Replaces '\' with '/'.</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToNativePath(this string? path)
        {
            return path?.Replace(PosixDirectorySeparatorChar, NativeDirectorySeparatorChar);
        }

        /// <summary>
        /// Removes any trailing path separator character from the end of <paramref name="dirPath"/>.
        /// </summary>
        [return: NotNullIfNotNull("dirPath")]
        public static string? RemoveTrailingPathSeparator(this string? dirPath)
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
        [return: NotNullIfNotNull("dirPath")]
        public static string? EnsureTrailingPathSeparator(this string? dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath) &&
                dirPath[dirPath.Length - 1] != NativeDirectorySeparatorChar &&
                dirPath[dirPath.Length - 1] != PosixDirectorySeparatorChar)
            {
                dirPath += NativeDirectorySeparatorChar;
            }

            return dirPath;
        }

        public static bool IsLocalFile(string fileName)
        {
            return !Regex.IsMatch(fileName, @"^(\w+):\/\/([\S]+)");
        }

        /// <summary>
        /// A slightly less naive way to check whether the given path is a URL by checking
        /// whether it matches a regular expression defined in <see cref="UrlRegex"/>.
        /// </summary>
        /// <remarks>
        /// Certain strings such as "http://" or "ssh:" are not considered valid URLs
        /// as they don't have a host, port or path.
        /// </remarks>
        /// <param name="path">A path to check.</param>
        /// <returns><see langword="true"/> if the given path starts with 'http', 'ssh' or 'git'; otherwise <see langword="false"/>.</returns>
        [Pure]
        public static bool IsUrl(string? path)
        {
            return !string.IsNullOrEmpty(path)
                && UrlRegex.IsMatch(path);
        }

        public static bool CanBeGitURL(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return IsUrl(url)
                   || url.EndsWith(".git", StringComparison.CurrentCultureIgnoreCase)
                   || GitModule.IsValidGitWorkingDir(url);
        }

        public static string GetFileName(string fileName)
        {
            var pathSeparators = new[] { NativeDirectorySeparatorChar, PosixDirectorySeparatorChar };
            var pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
            {
                fileName = fileName.Substring(pos + 1);
            }

            return fileName;
        }

        public static string NormalizePath(this string path)
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

        /// <summary>
        /// Wrapper for Path.Combine.
        /// </summary>
        /// <remark>
        /// Similar to the .NET Core 2.1 variant, except that null is returned if Windows
        /// invalid characters (that may be accepted in Git or other filesystems)
        /// are in the paths instead of a possible path (the OS or file system will throw
        /// if the paths are invalid).
        /// </remark>
        /// <param name="path1">initial part.</param>
        /// <param name="path2">second part.</param>
        /// <returns>path if it can be combined, null otherwise.</returns>
        public static string? Combine(string path1, string path2)
        {
            try
            {
                return Path.Combine(path1, path2);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Wrapper for Path.GetExtension.
        /// </summary>
        /// <remark>
        /// <see cref="Combine"/> for motivation.
        /// </remark>
        /// <param name="path">path to check.</param>
        /// <returns>path if it can be combined, empty otherwise.</returns>
        public static string GetExtension(string? path)
        {
            if (path is null)
            {
                return string.Empty;
            }

            try
            {
                return Path.GetExtension(path);
            }
            catch (ArgumentException)
            {
                // This could return part after a '.' using string commands,
                // but wait for .NET Core with this edge case
                return string.Empty;
            }
        }

        public static string Resolve(string path, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return IsWslPath(path) ? ResolveWsl(path, relativePath) : ResolveRelativePath(path, relativePath);
        }

        /// <summary>
        /// Special handling of on purpose invalid WSL machine name in Windows 10.
        /// </summary>
        internal static string ResolveWsl(string path, string relativePath = "")
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

        private static string ResolveRelativePath(string path, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            Uri tempPath = new(path);
            if (!string.IsNullOrEmpty(relativePath))
            {
                tempPath = new Uri(tempPath, relativePath);
            }

            return tempPath.LocalPath;
        }

        internal static bool IsWslPath(string path)
        {
            return path.ToLower().StartsWith(@"\\wsl$\");
        }

        public static string GetRepositoryName(string? repositoryUrl)
        {
            string name = "";

            if (repositoryUrl is not null)
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

        public static bool TryFindFullPath(string fileName, [NotNullWhen(returnValue: true)] out string? fullPath)
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
                    fullPath = Combine(path, fileName);
                    if (File.Exists(fullPath))
                    {
                        // TODO remove suppression when targeting .NET 5
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
                        return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
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

        public static bool TryFindShellPath(string shell, [NotNullWhen(returnValue: true)] out string? shellPath)
        {
            try
            {
                shellPath = Path.Combine(EnvironmentAbstraction.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", shell);
                if (File.Exists(shellPath))
                {
                    return true;
                }

                shellPath = Combine(AppSettings.GitBinDir, shell);
                if (File.Exists(shellPath))
                {
                    // TODO remove suppression when targeting .NET 5
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
                    return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
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

        public static string GetDisplayPath(string path)
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

        public static IEnumerable<string> FindAncestors(string path)
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

        public static string FindInFolders(this string fileName, IEnumerable<string?> folders)
        {
            foreach (string? location in folders)
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    continue;
                }

                string? fullName;
                if (Path.IsPathRooted(location))
                {
                    fullName = FindFile(location, fileName);
                    if (fullName is not null)
                    {
                        return fullName;
                    }

                    continue;
                }

                fullName = FindFileInEnvVarFolder("ProgramFiles", location, fileName);
                if (fullName is not null)
                {
                    return fullName;
                }

                fullName = FindFileInEnvVarFolder("ProgramW6432", location, fileName);
                if (fullName is not null)
                {
                    return fullName;
                }

                if (IntPtr.Size == 8 || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                {
                    fullName = FindFileInEnvVarFolder("ProgramFiles(x86)", location, fileName);
                    if (fullName is not null)
                    {
                        return fullName;
                    }
                }
            }

            return string.Empty;

            string? FindFileInEnvVarFolder(string environmentVariable, string location, string fileName1)
            {
                var envVarFolder = Environment.GetEnvironmentVariable(environmentVariable);
                if (string.IsNullOrEmpty(envVarFolder))
                {
                    return null;
                }

                var path = Combine(envVarFolder, location);
                if (!Directory.Exists(path))
                {
                    return null;
                }

                return FindFile(path!, fileName1);
            }

            static string? FindFile(string location, string fileName1)
            {
                string? fullName = Combine(location, fileName1);
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
        public static bool TryDeleteDirectory(this string? path, [NotNullWhen(returnValue: false)] out string? errorMessage)
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

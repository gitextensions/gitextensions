using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GitCommands.Utils;

namespace GitCommands
{
    public static class PathUtil
    {
        private static readonly IEnvironmentAbstraction EnvironmentAbstraction = new EnvironmentAbstraction();
        private static readonly IEnvironmentPathsProvider EnvironmentPathsProvider = new EnvironmentPathsProvider(EnvironmentAbstraction);

        // Windows build 21354 supports wsl.localhost too, not supported for WSL Git
        private const string WslPrefix = @"\\wsl$\";

        public static readonly char PosixDirectorySeparatorChar = '/';
        public static readonly char NativeDirectorySeparatorChar = Path.DirectorySeparatorChar;

        /// <summary>Replaces native path separator with posix path separator (/).</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToPosixPath(this string? path)
        {
            return path?.Replace(NativeDirectorySeparatorChar, PosixDirectorySeparatorChar);
        }

        /// <summary>Replaces '/' with native path separator.</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToNativePath(this string? path)
        {
            return path?.Replace(PosixDirectorySeparatorChar, NativeDirectorySeparatorChar);
        }

        /// <summary>Replaces native path separator with posix path separator (/) and drive letter X: with /mnt/x for use in WSL.</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToWslPath(this string? path)
        {
            return path?.ToMountPath("/mnt/");
        }

        /// <summary>Replaces native path separator with posix path separator (/) and drive letter X: with /cygdrive/x.</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToCygwinPath(this string? path)
        {
            return path?.ToMountPath("/cygdrive/");
        }

        /// <summary>Replaces native path separator with posix path separator (/) and drive letter X: with /prefix/x.</summary>
        [return: NotNullIfNotNull("path")]
        public static string? ToMountPath(this string? path, string prefix)
        {
            if (path is null)
            {
                return null;
            }

            path = path.ToPosixPath();
            if (path.Length >= 2 && path[1] == ':')
            {
                char drive = char.ToLowerInvariant(path[0]);
                if (drive is (>= 'a' and <= 'z'))
                {
                    return $"{prefix}{drive}{path[2..]}";
                }
            }

            return path;
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

        public static bool CanBeGitURL(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return Uri.IsWellFormedUriString(url, UriKind.Absolute)
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

        public static string Resolve(string path, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return IsWslPrefixPath(path) ? ResolveWsl(path, relativePath) : ResolveRelativePath(path, relativePath);
        }

        /// <summary>
        /// Special handling of on purpose invalid WSL machine name in Windows 10.
        /// </summary>
        internal static string ResolveWsl(string path, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(path) || !IsWslPrefixPath(path))
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
#pragma warning disable SYSLIB0013 // 'Uri.EscapeUriString(string)' is obsolete
                tempPath = new Uri(tempPath, Uri.EscapeUriString(relativePath));
#pragma warning restore SYSLIB0013 // 'Uri.EscapeUriString(string)' is obsolete
                return Uri.UnescapeDataString(tempPath.LocalPath);
            }

            return tempPath.LocalPath;
        }

        /// <summary>
        /// Check if the path is any known path for WSL that may require special handling
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>true if a path is a known WSL path</returns>
        public static bool IsWslPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && (IsWslPrefixPath(path)
                    || path.ToLower().StartsWith(@"\\wsl.localhost\"));
        }

        /// <summary>
        /// Check if the path is has handled specially for WSL
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>true if the path is a WSL path with internal handling</returns>
        private static bool IsWslPrefixPath(string path)
        {
            return path.ToLower().StartsWith(WslPrefix);
        }

        /// <summary>
        /// Get the name of the distribution (like "Ubuntu-20.04") for WSL2 paths.
        /// </summary>
        /// <param name="path">The directory path.</param>
        /// <returns>The name of the distro or empty for non WSL2 paths</returns>
        public static string GetWslDistro(string? path)
        {
            int distroLen = GetWslDistroLength(path);
            return distroLen <= 0 ? "" : path.Substring(WslPrefix.Length, distroLen);

            static int GetWslDistroLength(string? path)
            {
                if (string.IsNullOrWhiteSpace(path) || !IsWslPrefixPath(path))
                {
                    return -1;
                }

                return path.IndexOfAny(new[] { '\\', '/' }, WslPrefix.Length) - WslPrefix.Length;
            }
        }

        /// <summary>
        /// Convert a path for the Git executable.
        /// For Windows Git (native for the app) the path is unchanged, for WSL Git the path may be converted.
        /// </summary>
        /// <param name="path">The path as seen by the Git Extensions Windows (native) application.</param>
        /// <param name="wslDistro">The name of the distro or empty for non WSL paths.</param>
        /// <returns>The Posix path if Windows Git (not a WSL distro), WSL path for WSL Git.</returns>
        public static string GetGitExecPath(string? path, string? wslDistro)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path ?? "";
            }

            if (string.IsNullOrEmpty(wslDistro))
            {
                return path.ToPosixPath();
            }

            string pathDistro = GetWslDistro(path);
            if (!string.IsNullOrEmpty(pathDistro))
            {
                // Unexpected but not handled as an error
                if (pathDistro != wslDistro)
                {
                    return path.ToPosixPath();
                }

                return path[(WslPrefix.Length + pathDistro.Length)..].ToPosixPath();
            }

            if (path.Length > 2 && char.IsLetter(path[0]) && path[1] == ':')
            {
                return $"/mnt/{char.ToLower(path[0])}{path[2..].ToPosixPath()}";
            }

            return path.ToPosixPath();
        }

        /// <summary>
        /// Convert a path to Windows format, native to the application.
        /// If the app is supported on other OSes, the method should be renamed.
        /// </summary>
        /// <param name="path">The path as seen by the Git executable, possibly WSL Git.</param>
        /// <param name="wslDistro">The name of the distro or empty for non WSL paths.</param>
        /// <returns>The path in Windows format with native file separators.</returns>
        public static string GetWindowsPath(string? path, string? wslDistro)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(wslDistro) || path.StartsWith($"{WslPrefix}"))
            {
                return path?.ToNativePath() ?? "";
            }

            if (path.StartsWith("/mnt/") && path.Length > 7)
            {
                return $"{char.ToUpper(path[5])}:{path[6..]}".ToNativePath();
            }

            if (path[0] is '\\' or '/')
            {
                return $@"{WslPrefix}{wslDistro}{path}".ToNativePath();
            }

            return $@"{WslPrefix}{wslDistro}\{path}".ToNativePath();
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

        public static bool TryFindShellPath(string shell, [NotNullWhen(returnValue: true)] out string? shellPath)
        {
            try
            {
                shellPath = Path.Combine(EnvironmentAbstraction.GetEnvironmentVariable("ProgramW6432"), "Git", shell);
                if (File.Exists(shellPath))
                {
                    return true;
                }

                shellPath = Path.Combine(EnvironmentAbstraction.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", shell);
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

                var path = Path.Combine(envVarFolder, location);
                if (!Directory.Exists(path))
                {
                    return null;
                }

                return FindFile(path!, fileName1);
            }

            static string? FindFile(string location, string fileName1)
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

        internal readonly struct TestAccessor
        {
            public static bool IsWslPrefixPath(string path) => PathUtil.IsWslPrefixPath(path);
        }
    }
}

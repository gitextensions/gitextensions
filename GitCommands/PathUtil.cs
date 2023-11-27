using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GitCommands.Utils;

namespace GitCommands
{
    public static partial class PathUtil
    {
        private static readonly IEnvironmentAbstraction EnvironmentAbstraction = new EnvironmentAbstraction();
        private static readonly IEnvironmentPathsProvider EnvironmentPathsProvider = new EnvironmentPathsProvider(EnvironmentAbstraction);

        // Windows build 21354 supports wsl.localhost too, not supported for WSL Git
        private const string WslPrefix = @"\\wsl$\";
        private const string WslLocalhostPrefix = @"\\wsl.localhost\";

        public static readonly char PosixDirectorySeparatorChar = '/';
        public static readonly char NativeDirectorySeparatorChar = Path.DirectorySeparatorChar;

        [GeneratedRegex(@"^(\w+):\/\/([\S]+)")]
        private static partial Regex DriveLetterRegex();

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
                (dirPath[^1] == NativeDirectorySeparatorChar ||
                 dirPath[^1] == PosixDirectorySeparatorChar))
            {
                return dirPath[..^1];
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
                dirPath[^1] != NativeDirectorySeparatorChar &&
                dirPath[^1] != PosixDirectorySeparatorChar)
            {
                dirPath += NativeDirectorySeparatorChar;
            }

            return dirPath;
        }

        public static bool IsLocalFile(string fileName)
        {
            return !DriveLetterRegex().IsMatch(fileName);
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
            char[] pathSeparators = new[] { NativeDirectorySeparatorChar, PosixDirectorySeparatorChar };
            int pos = fileName.LastIndexOfAny(pathSeparators);
            if (pos != -1)
            {
                fileName = fileName[(pos + 1)..];
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
        ///  Replaces <c>\\wsl.localhost\</c> with <c>\\wsl$\</c>, if found. Else returns the <paramref name="path"/> untouched.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The WSL normalized path.</returns>
        public static string NormalizeWslPath(this string path)
        {
            // NOTE: path is expected to be already normalized!

            if (!IsWslLocalhostPrefixPath(path))
            {
                return path;
            }

            return path.Replace(WslLocalhostPrefix, WslPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// WSL links are not fully supported by the Windows or .NET
        /// This is a workaround to handle symbolic links to .git directories in WSL,
        /// other use cases are not considered.
        /// Note especially that this method is not checking if the link points to a file or directory.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><see langword="true"/> if the path seem to be a link; otherwise, <see langword="false"/>.</returns>
        public static bool IsWslLink(string path)
        {
            // FileAttributes.ReparsePoint is a 'reasonable' indicator for a WSL link.
            // File.Exists() (without trailing separator) is true but
            // Directory.Exists() is false for links to both files and directories.
            // To access these paths, native WSL utilities are required.
            return IsWslPrefixPath(path)
                 && File.Exists(RemoveTrailingPathSeparator(path))
                 && File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
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
        /// <returns><see langword="true"/> if a path is a known WSL path; otherwise, <see langword="false"/>.</returns>
        public static bool IsWslPath(string? path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && (IsWslPrefixPath(path) || IsWslLocalhostPrefixPath(path));
        }

        /// <summary>
        ///  Check if the path starts with '\\wsl.localhost\'.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns><see langword="true"/> if the path starts with '\\wsl.localhost\'; otherwise, <see langword="false"/>.</returns>
        private static bool IsWslLocalhostPrefixPath(string path) => path.StartsWith(WslLocalhostPrefix, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Check if the path is has handled specially for WSL
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns><see langword="true"/> if the path is a WSL path with internal handling; otherwise, <see langword="false"/>.</returns>
        private static bool IsWslPrefixPath(string path) => path.StartsWith(WslPrefix, StringComparison.OrdinalIgnoreCase);

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
        public static string GetPathForGitExecution(string? path, string? wslDistro)
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
                string path = (Uri.IsWellFormedUriString(repositoryUrl, UriKind.Absolute) || repositoryUrl.StartsWith("git@", StringComparison.OrdinalIgnoreCase)
                    ? System.Net.WebUtility.UrlDecode(repositoryUrl)
                    : repositoryUrl)
                    .TrimEnd('\\', '/');

                if (path.EndsWith(standardRepositorySuffix))
                {
                    path = path[..^standardRepositorySuffix.Length];
                }

                if (path.Contains("\\") || path.Contains("/"))
                {
                    name = path[(path.LastIndexOfAny(new[] { '\\', '/' }) + 1)..];
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

                foreach (string path in EnvironmentPathsProvider.GetEnvironmentValidPaths())
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

                shellPath = Path.Combine(AppSettings.LinuxToolsDir, shell);
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
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            StringComparison comparison = EnvUtils.RunningOnWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (path.StartsWith(userProfile, comparison))
            {
                int length = path.Length - userProfile.Length;
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
                string envVarFolder = Environment.GetEnvironmentVariable(environmentVariable);
                if (string.IsNullOrEmpty(envVarFolder))
                {
                    return null;
                }

                string path = Path.Combine(envVarFolder, location);
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
            public static bool IsWslLocalhostPrefixPath(string path) => PathUtil.IsWslLocalhostPrefixPath(path);
            public static bool IsWslPrefixPath(string path) => PathUtil.IsWslPrefixPath(path);
        }
    }
}

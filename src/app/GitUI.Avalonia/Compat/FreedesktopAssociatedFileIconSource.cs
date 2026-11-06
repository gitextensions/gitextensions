using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace GitUI.Compat;

internal sealed class FreedesktopAssociatedFileIconSource : IAssociatedFileIconSource
{
    private readonly IReadOnlyList<string> _dataDirectories;
    private readonly IReadOnlyList<string> _iconDirectories;

    public FreedesktopAssociatedFileIconSource()
        : this(GetDataDirectories(), GetIconDirectories())
    {
    }

    internal FreedesktopAssociatedFileIconSource(
        IReadOnlyList<string> dataDirectories,
        IReadOnlyList<string> iconDirectories)
    {
        _dataDirectories = dataDirectories;
        _iconDirectories = iconDirectories;
    }

    public IImage? Get(string workingDirectory, string relativeFilePath)
    {
        string? mimeType = GetMimeType(Path.GetFileName(relativeFilePath));
        if (mimeType is null)
        {
            return null;
        }

        foreach (string iconName in GetIconNames(mimeType))
        {
            string? path = FindPng(iconName);
            if (path is null)
            {
                continue;
            }

            try
            {
                return new Bitmap(path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
            }
        }

        return null;
    }

    internal string? GetMimeType(string fileName)
    {
        GlobMatch? best = null;
        for (int directoryIndex = 0; directoryIndex < _dataDirectories.Count; directoryIndex++)
        {
            string path = Path.Combine(_dataDirectories[directoryIndex], "mime", "globs2");
            foreach (string line in ReadLines(path))
            {
                string[] parts = line.Split(':', count: 4);
                if (parts.Length < 3
                    || !int.TryParse(parts[0], out int weight)
                    || !IsExtensionGlobMatch(parts[2], fileName, parts.Length == 4 ? parts[3] : null))
                {
                    continue;
                }

                GlobMatch match = new(parts[1], weight, parts[2].Length, directoryIndex);
                if (best is null || match.CompareTo(best.Value) > 0)
                {
                    best = match;
                }
            }
        }

        return best?.MimeType;
    }

    internal IReadOnlyList<string> GetIconNames(string mimeType)
    {
        List<string> names = [];
        AddMappedName("icons");
        names.Add(mimeType.Replace('/', '-'));
        AddMappedName("generic-icons");
        names.Add(mimeType[..mimeType.IndexOf('/')] + "-x-generic");
        return names.Distinct(StringComparer.Ordinal).ToArray();

        void AddMappedName(string fileName)
        {
            foreach (string dataDirectory in _dataDirectories)
            {
                foreach (string line in ReadLines(Path.Combine(dataDirectory, "mime", fileName)))
                {
                    int separator = line.IndexOf(':');
                    if (separator > 0 && line.AsSpan(0, separator).SequenceEqual(mimeType))
                    {
                        names.Add(line[(separator + 1)..]);
                        return;
                    }
                }
            }
        }
    }

    internal string? FindPng(string iconName)
    {
        IconMatch? best = null;
        for (int rootIndex = 0; rootIndex < _iconDirectories.Count; rootIndex++)
        {
            string root = _iconDirectories[rootIndex];
            if (!Directory.Exists(root))
            {
                continue;
            }

            IEnumerable<string> candidates;
            try
            {
                candidates = Directory.EnumerateFiles(root, iconName + ".png", SearchOption.AllDirectories);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            try
            {
                foreach (string candidate in candidates)
                {
                    IconMatch match = new(candidate, GetSizeScore(candidate), rootIndex);
                    if (best is null || match.CompareTo(best.Value) > 0)
                    {
                        best = match;
                    }
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
            }
        }

        return best?.Path;
    }

    private static bool IsExtensionGlobMatch(string glob, string fileName, string? flags)
    {
        if (!glob.StartsWith("*.", StringComparison.Ordinal) || glob.AsSpan(2).Contains('*') || glob.AsSpan(2).Contains('?'))
        {
            return false;
        }

        StringComparison comparison = flags?.Split(',').Contains("cs", StringComparer.Ordinal) == true
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        return fileName.EndsWith(glob[1..], comparison);
    }

    private static int GetSizeScore(string path)
    {
        string normalized = path.Replace('\\', '/');
        return normalized.Contains("/16x16/", StringComparison.OrdinalIgnoreCase) ? 100
            : normalized.Contains("/22x22/", StringComparison.OrdinalIgnoreCase) ? 90
            : normalized.Contains("/24x24/", StringComparison.OrdinalIgnoreCase) ? 80
            : normalized.Contains("/32x32/", StringComparison.OrdinalIgnoreCase) ? 70
            : normalized.Contains("/scalable/", StringComparison.OrdinalIgnoreCase) ? 10
            : 50;
    }

    private static IEnumerable<string> ReadLines(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            return File.ReadLines(path).Where(line => line.Length > 0 && line[0] != '#').ToArray();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static IReadOnlyList<string> GetDataDirectories()
    {
        List<string> directories = [];
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            ?? Path.Combine(home, ".local", "share");
        directories.Add(dataHome);
        string dataDirectories = Environment.GetEnvironmentVariable("XDG_DATA_DIRS")
            ?? "/usr/local/share:/usr/share";
        directories.AddRange(dataDirectories.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));
        if (File.Exists("/.flatpak-info"))
        {
            directories.Add("/run/host/usr/local/share");
            directories.Add("/run/host/usr/share");
        }

        return directories.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> GetIconDirectories()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        List<string> directories = [Path.Combine(home, ".icons")];
        foreach (string dataDirectory in GetDataDirectories())
        {
            directories.Add(Path.Combine(dataDirectory, "icons"));
            directories.Add(Path.Combine(dataDirectory, "pixmaps"));
        }

        return directories.Distinct(StringComparer.Ordinal).ToArray();
    }

    private readonly record struct GlobMatch(string MimeType, int Weight, int PatternLength, int DirectoryIndex)
    {
        public int CompareTo(GlobMatch other)
        {
            int weight = Weight.CompareTo(other.Weight);
            if (weight != 0)
            {
                return weight;
            }

            int length = PatternLength.CompareTo(other.PatternLength);
            return length != 0 ? length : other.DirectoryIndex.CompareTo(DirectoryIndex);
        }
    }

    private readonly record struct IconMatch(string Path, int SizeScore, int RootIndex)
    {
        public int CompareTo(IconMatch other)
        {
            int size = SizeScore.CompareTo(other.SizeScore);
            return size != 0 ? size : other.RootIndex.CompareTo(RootIndex);
        }
    }
}

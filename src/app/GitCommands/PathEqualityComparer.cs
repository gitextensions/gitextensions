namespace GitCommands;

public class PathEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string? path1, string? path2)
    {
        if (path1 is null || path2 is null)
        {
            return path1 is null && path2 is null;
        }

        path1 = Path.GetFullPath(path1).TrimEnd('\\');
        path2 = Path.GetFullPath(path2).TrimEnd('\\');
        StringComparison comparison = !OperatingSystem.IsWindows()
            ? StringComparison.InvariantCulture
            : StringComparison.InvariantCultureIgnoreCase;

        return string.Compare(path1, path2, comparison) == 0;
    }

    public int GetHashCode(string path)
    {
        path = Path.GetFullPath(path).TrimEnd('\\');
        if (OperatingSystem.IsWindows())
        {
            path = path.ToLower();
        }

        return path.GetHashCode();
    }
}

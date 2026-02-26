using System.Text.Json;

namespace GitUI.LeftPanel;

/// <summary>
/// Manages persistence of branches protected from accidental deletion.
/// The list is stored in a JSON file inside the repository's .git directory:
/// <c>.git/gitextensions_protected_branches.json</c>
/// </summary>
internal static class ProtectedBranchStore
{
    private const string FileName = "gitextensions_protected_branches.json";

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private static string GetFilePath(string gitDir) => Path.Combine(gitDir, FileName);

    public static bool IsProtected(string gitDir, string branchFullPath)
    {
        return ReadAll(GetFilePath(gitDir)).Contains(branchFullPath);
    }

    public static void Protect(string gitDir, string branchFullPath)
    {
        string filePath = GetFilePath(gitDir);
        HashSet<string> branches = ReadAll(filePath);
        if (branches.Add(branchFullPath))
        {
            WriteAll(filePath, branches);
        }
    }

    public static void Unprotect(string gitDir, string branchFullPath)
    {
        string filePath = GetFilePath(gitDir);
        HashSet<string> branches = ReadAll(filePath);
        if (branches.Remove(branchFullPath))
        {
            WriteAll(filePath, branches);
        }
    }

    private static HashSet<string> ReadAll(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        try
        {
            string json = File.ReadAllText(filePath);
            string[]? entries = JsonSerializer.Deserialize<string[]>(json);
            return entries is null ? [] : [.. entries];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static void WriteAll(string filePath, HashSet<string> branches)
    {
        string json = JsonSerializer.Serialize(branches.OrderBy(b => b, StringComparer.Ordinal).ToArray(), _jsonOptions);
        File.WriteAllText(filePath, json);
    }
}

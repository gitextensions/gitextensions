using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GitExtensions.ParityFix;

// parity-scaffolding: Proves hand-placed comments against HEAD independently of inventory and automatic placement.
internal static class WorktreeProof
{
    public static async Task<ProofReport> ProveAsync(string workingDirectory)
    {
        string repository = Path.GetFullPath((await GitAsync(workingDirectory, "rev-parse", "--show-toplevel")).Trim());
        string head = (await GitAsync(repository, "rev-parse", "HEAD")).Trim();
        string[] raw = (await GitAsync(repository, "diff", "--raw", "-z", "--no-renames", "--ignore-submodules=none", head, "--"))
            .Split('\0', StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, bool> unchangedModes = new(StringComparer.Ordinal);
        for (int index = 0; index < raw.Length; index += 2)
        {
            string[] fields = raw[index].Split(' ');
            unchangedModes.Add(raw[index + 1], fields[0] is ":100644" or ":100755" && fields[0][1..] == fields[1]);
        }

        string[] paths = (await GitAsync(repository, "diff", "--name-only", "-z", "--no-renames", "--ignore-submodules=none", head, "--")
            + await GitAsync(repository, "ls-files", "--others", "--exclude-standard", "-z", "--"))
            .Split('\0', StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        List<string> exemptPaths = [];
        List<FileProof> files = [];
        foreach (string path in paths)
        {
            if (path is "eng/avalonia/portmap.json" or "eng/avalonia/parity-ledger.json")
            {
                exemptPaths.Add(path);
                continue;
            }

            string current = Path.Combine(repository, path);
            if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || !File.Exists(current)
                || !unchangedModes.GetValueOrDefault(path))
            {
                files.Add(new(path, false, "Only existing regular C# files with unchanged file modes can be proved; additions, deletions and other formats fail."));
                continue;
            }

            string original;
            try
            {
                original = await GitAsync(repository, "show", $"{head}:{path}");
            }
            catch (InvalidOperationException)
            {
                files.Add(new(path, false, "The file does not exist in HEAD."));
                continue;
            }

            bool passed = AreCommentOnlyEquivalent(original, await File.ReadAllTextAsync(current));
            files.Add(new(path, passed, passed ? null : "Syntax, directives or disabled text differ from HEAD."));
        }

        return new(head, files, exemptPaths);
    }

    internal static bool AreCommentOnlyEquivalent(string original, string twin)
    {
        SyntaxNode before = Parse(original);
        SyntaxNode after = Parse(twin);
        return !before.ContainsDiagnostics && !after.ContainsDiagnostics
            && SyntaxFactory.AreEquivalent(before, after)
            && Directives(before).SequenceEqual(Directives(after))
            && DisabledText(before).SequenceEqual(DisabledText(after));
    }

    internal static async Task<string> GitAsync(string directory, params string[] arguments)
    {
        ProcessStartInfo start = new("git")
        {
            WorkingDirectory = directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("Could not start git.");
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        string text = await output;
        string diagnostic = await error;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"git {arguments[0]} failed: {diagnostic.Trim()}");
        }

        return text;
    }

    private static SyntaxNode Parse(string text) => CSharpSyntaxTree.ParseText(
        text.Replace("\r\n", "\n", StringComparison.Ordinal), new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();

    private static IEnumerable<string> Directives(SyntaxNode root) => root.DescendantTrivia()
        .Where(trivia => trivia.GetStructure() is DirectiveTriviaSyntax).Select(trivia => trivia.ToFullString());

    private static IEnumerable<string> DisabledText(SyntaxNode root) => root.DescendantTrivia()
        .Where(trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia)).Select(trivia => trivia.ToFullString());
}

// parity-scaffolding: Retains the HEAD identity and every failed file without treating empty coverage as product parity.
internal sealed record ProofReport(string Head, IReadOnlyList<FileProof> Files, IReadOnlyList<string> ExemptPaths)
{
    public bool Passed => Files.All(file => file.Passed);
}

// parity-scaffolding: A non-C# or newly added path cannot silently disappear from the proof result.
internal sealed record FileProof(string Path, bool Passed, string? Error);

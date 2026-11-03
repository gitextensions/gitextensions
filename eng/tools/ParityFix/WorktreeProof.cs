using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GitExtensions.ParityFix;

// parity-scaffolding: Proves hand-placed comments against HEAD independently of inventory and automatic placement.
internal static class WorktreeProof
{
    public static async Task<ProofReport> ProveAsync(string workingDirectory, string twinRoot)
    {
        string repository = Path.GetFullPath((await GitAsync(workingDirectory, "rev-parse", "--show-toplevel")).Trim());
        string head = (await GitAsync(repository, "rev-parse", "HEAD")).Trim();
        string root = Path.GetFullPath(twinRoot, repository);
        if (!FixRunner.IsWithin(repository, root) || root == repository || !Directory.Exists(root))
        {
            throw new ArgumentException("The twin root must be a directory inside the repository.", nameof(twinRoot));
        }

        string relative = Path.GetRelativePath(repository, root).Replace('\\', '/');
        string[] paths = (await GitAsync(repository, "diff", "--name-only", "-z", "--no-renames", head, "--", relative)
            + await GitAsync(repository, "ls-files", "--others", "--exclude-standard", "-z", "--", relative))
            .Split('\0', StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        List<FileProof> files = [];
        foreach (string path in paths)
        {
            string current = Path.Combine(repository, path);
            if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || !File.Exists(current))
            {
                files.Add(new(path, false, "Only existing C# twin files can be proved; additions, deletions and other formats require review."));
                continue;
            }

            string original;
            try
            {
                original = await GitAsync(repository, "show", $"{head}:{path}");
            }
            catch (InvalidOperationException)
            {
                files.Add(new(path, false, "The twin file does not exist in HEAD."));
                continue;
            }

            bool passed = AreCommentOnlyEquivalent(original, await File.ReadAllTextAsync(current));
            files.Add(new(path, passed, passed ? null : "Syntax or directives differ from HEAD."));
        }

        return new(head, files);
    }

    internal static bool AreCommentOnlyEquivalent(string original, string twin)
    {
        SyntaxNode before = Parse(original);
        SyntaxNode after = Parse(twin);
        return !before.ContainsDiagnostics && !after.ContainsDiagnostics
            && SyntaxFactory.AreEquivalent(before, after)
            && Directives(before).SequenceEqual(Directives(after));
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
}

// parity-scaffolding: Retains the HEAD identity and every failed file without treating empty coverage as product parity.
internal sealed record ProofReport(string Head, IReadOnlyList<FileProof> Files)
{
    public bool Passed => Files.All(file => file.Passed);
}

// parity-scaffolding: A non-C# or newly added twin cannot silently disappear from the proof result.
internal sealed record FileProof(string Path, bool Passed, string? Error);

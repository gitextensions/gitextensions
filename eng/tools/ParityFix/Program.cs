using System.Text.Json;
using GitExtensions.ParityFix;

// parity-scaffolding: Explicit apply is separate from the default reviewable mechanical-fix preview.
try
{
    bool prove = args.FirstOrDefault() == "prove";
    HashSet<string> switches = prove ? [] : ["--apply", "--allow-initializers"];
    HashSet<string> values = prove ? ["--report"] : ["--inventory", "--ownership", "--owner", "--report", "--twin-root"];
    Dictionary<string, string> options = new(StringComparer.Ordinal);
    for (int index = prove ? 1 : 0; index < args.Length; index++)
    {
        string key = args[index];
        if (!switches.Contains(key) && !values.Contains(key))
        {
            throw new ArgumentException($"Unknown argument: {key}");
        }

        if (!options.TryAdd(key, switches.Contains(key) ? "true" : ++index < args.Length
                ? args[index] : throw new ArgumentException($"Missing value for {key}")))
        {
            throw new ArgumentException($"Duplicate argument: {key}");
        }
    }

    string Required(string key) => options.GetValueOrDefault(key) ?? throw new ArgumentException($"Required: {key}");

    if (prove)
    {
        ProofReport proof = await WorktreeProof.ProveAsync(Directory.GetCurrentDirectory());
        if (options.TryGetValue("--report", out string? output))
        {
            string path = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(proof, new JsonSerializerOptions { WriteIndented = true }));
        }

        Console.WriteLine($"HEAD {proof.Head}: {proof.Files.Count(file => file.Passed)} of {proof.Files.Count} changed repository files proved equivalent with unchanged directives and disabled text; {proof.ExemptPaths.Count} metadata paths exempt.");
        foreach (FileProof file in proof.Files.Where(file => !file.Passed))
        {
            Console.Error.WriteLine($"{file.Path}: {file.Error}");
        }

        return proof.Passed ? 0 : 1;
    }

    using JsonDocument ownership = JsonDocument.Parse(File.ReadAllText(Required("--ownership")));
    string owner = Required("--owner");
    HashSet<string> selected = ownership.RootElement.GetProperty("types").EnumerateArray()
        .Where(type => type.GetProperty("owner").GetString() == owner)
        .Select(type => type.GetProperty("typeName").GetString()!).ToHashSet(StringComparer.Ordinal);
    string reportPath = Path.GetFullPath(Required("--report"));
    Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
    FixRun run = FixRunner.Run(Required("--inventory"), selected,
        options.GetValueOrDefault("--twin-root") ?? "src/app/GitUI.Avalonia",
        options.ContainsKey("--apply"), options.ContainsKey("--allow-initializers"));
    File.WriteAllText(reportPath, JsonSerializer.Serialize(new
    {
        owner,
        applied = options.ContainsKey("--apply"),
        run.SelectedFindings,
        run.CommentFindings,
        unplacedComments = run.CommentFindings - run.Files.Sum(file => file.CommentsRestored),
        orderFindingsBefore = run.SelectedFindings - run.CommentFindings,
        run.UnresolvedOperations,
        commentsRestored = run.Files.Sum(file => file.CommentsRestored),
        reorderedFiles = run.Files.Count(file => file.OrderChanged),
        files = run.Files.Select(file => new { file.Path, file.CommentsRestored, file.OrderChanged, file.InitializedFields, file.Unresolved, file.ProofPassed })
    }, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"{run.SelectedFindings} selected findings; {run.Files.Sum(file => file.CommentsRestored)} comments restored, {run.CommentFindings - run.Files.Sum(file => file.CommentsRestored)} unplaced; {run.Files.Count(file => file.OrderChanged)} files reordered. Report: {reportPath}");
    Console.WriteLine("Run a fresh reviewed inventory to measure actual finding closure; unmatched partials and anchors remain open.");
    return run.Files.All(file => file.ProofPassed) ? 0 : 1;
}
catch (Exception exception) when (exception is ArgumentException or IOException or JsonException or InvalidOperationException)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

using System.Text.Json;
using GitExtensions.ParityFix;

// parity-scaffolding: Explicit apply is separate from the default reviewable mechanical-fix preview.
try
{
    HashSet<string> switches = ["--apply", "--allow-initializers"];
    HashSet<string> values = ["--inventory", "--ownership", "--owner", "--report", "--twin-root"];
    Dictionary<string, string> options = new(StringComparer.Ordinal);
    for (int index = 0; index < args.Length; index++)
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

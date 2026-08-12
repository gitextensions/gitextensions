using WinFormsInputParityToAvalonia;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.Error.WriteLine("""
        Generates the Avalonia input metadata projection from matching WinForms Designer files.

        Usage:
          WinFormsInputParityToAvalonia <WinFormsRoot> <AvaloniaRoot> [<WinFormsRoot> <AvaloniaRoot> ...] -o <WinFormsInputMetadata.g.cs>

        Output includes TabIndex, explicit TabStop, and explicit AccessibleName values for
        controls whose original field and same-named AXAML control both exist.
        """);
    return 1;
}

int sourceArgumentCount = args.Length - 2;
if (args.Length < 4 || sourceArgumentCount % 2 != 0 || args[^2] != "-o")
{
    Console.Error.WriteLine("Expected one or more <WinFormsRoot> <AvaloniaRoot> pairs followed by -o <output>.");
    return 1;
}

try
{
    (string WinFormsRoot, string AvaloniaRoot)[] sourceRoots = Enumerable.Range(0, sourceArgumentCount / 2)
        .Select(index => (args[index * 2], args[(index * 2) + 1]))
        .ToArray();
    string generatedSource = InputMetadataGenerator.Generate(sourceRoots);
    string outputPath = args[^1];
    string? existingSource = File.Exists(outputPath) ? File.ReadAllText(outputPath) : null;
    if (!string.Equals(existingSource?.ReplaceLineEndings("\n"), generatedSource, StringComparison.Ordinal))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, generatedSource);
        Console.Error.WriteLine($"Generated {outputPath}");
    }
    else
    {
        Console.Error.WriteLine($"Already current: {outputPath}");
    }
}
catch (InvalidDataException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

return 0;

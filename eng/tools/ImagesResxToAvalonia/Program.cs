using ImagesResxToAvalonia;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.Error.WriteLine("""
        Generates GitUI.Properties.Images for the Avalonia port from Images.resx.

        Usage:
          ImagesResxToAvalonia <Images.resx> -o <Images.g.cs>

        Only file-backed bitmap entries are supported. Existing Resources and setup logo
        PNGs are mapped to their packaged Avalonia resource paths. Output is deterministic.
        """);
    return 1;
}

string inputPath = args[0];
string? outputPath = null;

for (int i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-o" when i + 1 < args.Length:
            outputPath = args[++i];
            break;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            return 1;
    }
}

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"File not found: {inputPath}");
    return 1;
}

if (outputPath is null)
{
    Console.Error.WriteLine("Missing required -o <Images.g.cs> argument.");
    return 1;
}

try
{
    string generatedSource = ImagesGenerator.Generate(inputPath);
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

using WinFormsInputParityToAvalonia;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.Error.WriteLine("""
        Generates the Avalonia input metadata projection from matching WinForms Designer files.

        Usage:
          WinFormsInputParityToAvalonia <GitUI> <GitUI.Avalonia> -o <WinFormsInputMetadata.g.cs>

        Output includes TabIndex, explicit TabStop, and explicit AccessibleName values for
        controls whose original field and same-named AXAML control both exist.
        """);
    return 1;
}

if (args.Length != 4 || args[2] != "-o")
{
    Console.Error.WriteLine("Expected <GitUI> <GitUI.Avalonia> -o <output>.");
    return 1;
}

try
{
    string generatedSource = InputMetadataGenerator.Generate(args[0], args[1]);
    string outputPath = args[3];
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

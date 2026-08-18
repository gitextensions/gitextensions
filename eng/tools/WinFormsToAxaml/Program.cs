using WinFormsToAxaml;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.Error.WriteLine("""
        Scaffolds an Avalonia .axaml layout from a Windows Forms .Designer.cs file.

        Usage:
          WinFormsToAxaml <Form.Designer.cs> [-o <output.axaml>] [--usercontrol]

        Options:
          -o <path>      Write the scaffold to a file instead of standard output.
          --usercontrol  Emit a <UserControl> root instead of a <Window>.

        The output is a deterministic scaffold, not a finished layout: field names are
        preserved as x:Name (translation keys depend on them), known controls, properties,
        and events are mapped, and everything else is kept as a TODO:WinForms comment.
        Re-run on an upstream-changed Designer file and diff the scaffolds to see what the
        upstream change means for the ported twin.
        """);
    return 1;
}

string inputPath = args[0];
string? outputPath = null;
bool asUserControl = false;

for (int i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-o" when i + 1 < args.Length:
            outputPath = args[++i];
            break;
        case "--usercontrol":
            asUserControl = true;
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

DesignerForm form;
try
{
    form = DesignerParser.Parse(File.ReadAllText(inputPath));
}
catch (InvalidDataException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

// A Designer file of a UserControl has no form-level dialog properties.
if (!asUserControl
    && !form.Root.Properties.ContainsKey("Text")
    && !form.Root.Properties.ContainsKey("ClientSize")
    && !form.Root.Properties.ContainsKey("AcceptButton"))
{
    asUserControl = true;
}

// The source path in the header is repo-relative so output does not depend on the machine.
string sourceDescription = inputPath.Replace('\\', '/');
int srcIndex = sourceDescription.IndexOf("src/", StringComparison.Ordinal);
if (srcIndex >= 0)
{
    sourceDescription = sourceDescription[srcIndex..];
}

string axaml = new AxamlEmitter(form, sourceDescription, asUserControl).Emit();

if (outputPath is null)
{
    Console.Out.Write(axaml);
}
else
{
    File.WriteAllText(outputPath, axaml);
    Console.Error.WriteLine($"Scaffold written to {outputPath}");
}

return 0;

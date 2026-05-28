#!/usr/bin/env dotnet-run
#:property TargetFramework=net10.0
#:property UseWindowsForms=false
#:property EnableStyleCopAnalyzers=false
#:property EnableVisualStudioThreading=false
#:property GenerateDocumentationFile=false

using System.Text.RegularExpressions;

// Parse command-line arguments
string? version = null;
string? textVersion = null;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-v" or "--version" when i + 1 < args.Length:
            version = args[++i];
            break;
        case "-t" or "--text" when i + 1 < args.Length:
            textVersion = args[++i];
            break;
    }
}

TimeSpan regexTimeout = TimeSpan.FromSeconds(1);

if (version is null || !Regex.IsMatch(version, @"^\d+\.\d+", RegexOptions.None, regexTimeout))
{
    Console.Error.WriteLine("Usage: dotnet run set_version_to.cs -- -v <version> [-t <text-version>]");
    return 1;
}

textVersion ??= version;

// Split version into up to 4 numeric components
string[] versionParts = version.Split('.');
string[] verData = ["0", "0", "0", "0"];

for (int i = 0; i < Math.Min(versionParts.Length, 4); i++)
{
    Match m = Regex.Match(versionParts[i], @"^(\d+)", RegexOptions.None, regexTimeout);
    if (m.Success)
    {
        verData[i] = m.Groups[1].Value;
    }
}

string numericVersion = string.Join('.', verData);
string commaVersion = string.Join(',', verData);
string dottedVersion = string.Join('.', versionParts);

// Paths are relative to the working directory (eng/)
string repoRoot = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, ".."));

// Update C# assembly info files
List<string> assemblyInfoFiles =
[
    Path.Combine(repoRoot, "CommonAssemblyInfo.cs"),
    Path.Combine(repoRoot, "CommonAssemblyInfoExternals.cs"),
];

string externalsDir = Path.Combine(repoRoot, "externals");
if (Directory.Exists(externalsDir))
{
    assemblyInfoFiles.AddRange(
        Directory.EnumerateFiles(externalsDir, "AssemblyInfo.cs", SearchOption.AllDirectories));
}

foreach (string file in assemblyInfoFiles)
{
    Console.WriteLine(file);
    string[] lines = File.ReadAllLines(file);

    for (int i = 0; i < lines.Length; i++)
    {
        if (!lines[i].Contains("[assembly: Assembly"))
        {
            continue;
        }

        if (lines[i].Contains("AssemblyVersion(") || lines[i].Contains("AssemblyFileVersion("))
        {
            string[] parts = lines[i].Split('"');
            parts[1] = numericVersion;
            lines[i] = string.Join('"', parts);
        }
        else if (lines[i].Contains("AssemblyInformationalVersion("))
        {
            string[] parts = lines[i].Split('"');
            parts[1] = textVersion;
            lines[i] = string.Join('"', parts);
        }
    }

    File.WriteAllText(file, string.Join('\n', lines) + '\n', DetectEncoding(file));
}

// Update native resource files
UpdateResourceFile(Path.Combine(repoRoot, "src", "native", "GitExtensionsShellEx", "GitExtensionsShellEx.rc"));
UpdateResourceFile(Path.Combine(repoRoot, "src", "native", "GitExtSshAskPass", "SshAskPass.rc2"));

return 0;

static System.Text.Encoding DetectEncoding(string filePath)
{
    byte[] header = new byte[3];
    using (FileStream fs = File.OpenRead(filePath))
    {
        _ = fs.Read(header, 0, 3);
    }

    bool hasBom = header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF;
    return new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: hasBom);
}

void UpdateResourceFile(string filePath)
{
    Console.WriteLine(filePath);
    string[] lines = File.ReadAllLines(filePath);

    for (int i = 0; i < lines.Length; i++)
    {
        if (lines[i].Contains("FILEVERSION"))
        {
            string[] parts = lines[i].Split(' ');
            parts[2] = commaVersion;
            lines[i] = string.Join(' ', parts);
        }
        else if (lines[i].Contains("PRODUCTVERSION"))
        {
            string[] parts = lines[i].Split(' ');
            parts[2] = commaVersion;
            lines[i] = string.Join(' ', parts);
        }
        else if (lines[i].Contains("\"FileVersion\""))
        {
            string[] parts = lines[i].Split(", ", 2);
            parts[1] = $"\"{dottedVersion}\"";
            lines[i] = string.Join(", ", parts);
        }
        else if (lines[i].Contains("\"ProductVersion\""))
        {
            string[] parts = lines[i].Split(", ", 2);
            parts[1] = $"\"{textVersion}\"";
            lines[i] = string.Join(", ", parts);
        }
    }

    File.WriteAllText(filePath, string.Join('\n', lines) + '\n', DetectEncoding(filePath));
}

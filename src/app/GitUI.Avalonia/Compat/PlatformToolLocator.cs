using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitCommands.DiffMergeTools;
using GitExtensions.Extensibility.Configurations;

namespace GitUI.Compat;

/// <summary>
///  An executable and its literal prefix arguments in the host's command namespace.
/// </summary>
public sealed record LocatedTool(string Executable, IReadOnlyList<string> PrefixArguments);

/// <summary>
///  Supplements the unchanged shared search with known portable tool locations.
/// </summary>
public sealed class PlatformToolLocator
{
    private static PlatformToolLocator _current = new();
    private static readonly Dictionary<string, string[]> FlatpakIds = new(StringComparer.Ordinal)
    {
        ["code"] = ["com.visualstudio.code", "com.vscodium.codium"],
        ["meld"] = ["org.gnome.meld"],
        ["kdiff3"] = ["org.kde.kdiff3"],
        ["subl"] = ["com.sublimehq.SublimeText", "com.sublimetext.three"],
        ["smerge"] = ["com.sublimemerge.App"],
        ["zed"] = ["dev.zed.Zed"],
        ["konsole"] = ["org.kde.konsole"],
    };

    private static readonly Dictionary<string, string[]> MacExecutables = new(StringComparer.Ordinal)
    {
        ["meld"] = ["Meld.app/Contents/MacOS/Meld"],
        ["code"] = ["Visual Studio Code.app/Contents/Resources/app/bin/code", "VSCodium.app/Contents/Resources/app/bin/codium"],
        ["kdiff3"] = ["kdiff3.app/Contents/MacOS/kdiff3", "KDiff3.app/Contents/MacOS/kdiff3"],
        ["subl"] = ["Sublime Text.app/Contents/SharedSupport/bin/subl"],
        ["bcompare"] = ["Beyond Compare.app/Contents/MacOS/bcomp"],
        ["compare"] = ["Araxis Merge.app/Contents/Utilities/compare"],
        ["smerge"] = ["Sublime Merge.app/Contents/SharedSupport/bin/smerge"],
        ["zed"] = ["Zed.app/Contents/MacOS/cli"],
        ["p4merge"] = ["p4merge.app/Contents/Resources/launchp4merge", "p4merge.app/Contents/MacOS/p4merge"],
        ["diffmerge"] = ["DiffMerge.app/Contents/MacOS/DiffMerge"],
    };

    private readonly TerminalPlatform _platform;
    private readonly Func<string, bool> _fileExists;
    private readonly Func<string, string?> _findOnPath;
    private readonly Func<string, IReadOnlyCollection<string>> _listFlatpakApps;
    private readonly IReadOnlyList<string> _applicationDirectories;
    private readonly IReadOnlyList<string> _brewDirectories;
    private readonly bool _isFlatpak;

    /// <summary>
    ///  Creates a locator for the current desktop, preserving the original shared search as the first step.
    /// </summary>
    public PlatformToolLocator()
        : this(OperatingSystem.IsWindows() ? TerminalPlatform.Windows : OperatingSystem.IsMacOS() ? TerminalPlatform.MacOS : TerminalPlatform.Linux)
    {
    }

    internal PlatformToolLocator(TerminalPlatform platform)
        : this(platform, File.Exists, FindOnPath, ListFlatpakApps,
            ["/Applications", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications")],
            ["/opt/homebrew/bin", "/usr/local/bin"], FlatpakEnvironment.IsFlatpak())
    {
    }

    internal static PlatformToolLocator Current => _current;

    internal bool CanDiscover => !_isFlatpak;

    internal PlatformToolLocator(
        TerminalPlatform platform,
        Func<string, bool> fileExists,
        Func<string, string?> findOnPath,
        Func<string, IReadOnlyCollection<string>> listFlatpakApps,
        IReadOnlyList<string> applicationDirectories,
        IReadOnlyList<string> brewDirectories,
        bool isFlatpak = false)
    {
        _platform = platform;
        _fileExists = fileExists;
        _findOnPath = findOnPath;
        _listFlatpakApps = listFlatpakApps;
        _applicationDirectories = applicationDirectories;
        _brewDirectories = brewDirectories;
        _isFlatpak = isFlatpak;
    }

    public LocatedTool? Find(string executable)
    {
        if (_platform == TerminalPlatform.Windows || !IsKnownExecutable(executable))
        {
            return null;
        }

        string name = GetUnixName(executable);
        if (_platform == TerminalPlatform.MacOS)
        {
            foreach (string root in _applicationDirectories)
            {
                foreach (string relative in MacExecutables.GetValueOrDefault(name) ?? [])
                {
                    string candidate = Path.Combine(root, relative);
                    if (_fileExists(candidate))
                    {
                        return new LocatedTool(candidate, []);
                    }
                }
            }

            foreach (string root in _brewDirectories)
            {
                string candidate = Path.Combine(root, name);
                if (_fileExists(candidate))
                {
                    return new LocatedTool(candidate, []);
                }
            }
        }

        if (_isFlatpak)
        {
            return null;
        }

        string? path = _findOnPath(name);
        if (!string.IsNullOrEmpty(path))
        {
            return new LocatedTool(path, []);
        }

        if (_platform == TerminalPlatform.Linux && FlatpakIds.TryGetValue(name, out string[]? ids)
            && _findOnPath("flatpak") is string flatpak)
        {
            IReadOnlyCollection<string> installed = _listFlatpakApps(flatpak);
            string? id = ids.FirstOrDefault(candidate => installed.Contains(candidate, StringComparer.Ordinal));
            if (id is not null)
            {
                return new LocatedTool(flatpak, ["run", id]);
            }
        }

        return null;
    }

    internal DiffMergeToolConfiguration Adapt(DiffMergeToolConfiguration original)
    {
        if (!string.Equals(original.Path, original.ExeFileName, StringComparison.Ordinal))
        {
            return original;
        }

        LocatedTool? location = Find(original.ExeFileName);
        if (location is null)
        {
            return original;
        }

        return WithLocation(original, location);
    }

    private static DiffMergeToolConfiguration WithLocation(DiffMergeToolConfiguration original, LocatedTool location)
    {
        bool forwardFiles = IsFlatpakApp(location);
        IReadOnlyList<string> arguments = forwardFiles
            ? ["run", "--file-forwarding", location.PrefixArguments[1]]
            : location.PrefixArguments;
        string prefix = arguments.Count == 0
            ? string.Empty
            : string.Join(" ", arguments.Select(argument => "\"" + argument + "\"")) + " ";
        return new DiffMergeToolConfiguration(original.ExeFileName, location.Executable,
            original.DiffCommand.Length == 0 ? null : prefix + (forwardFiles ? ForwardFiles(original.DiffCommand) : original.DiffCommand),
            original.MergeCommand.Length == 0 ? null : prefix + (forwardFiles ? ForwardFiles(original.MergeCommand) : original.MergeCommand));
    }

    private static bool IsFlatpakApp(LocatedTool location)
        => location.PrefixArguments.Count == 2 && location.PrefixArguments[0] == "run";

    private static string ForwardFiles(string command)
    {
        // Flatpak forwards whole file arguments; Sublime Merge also accepts its documented separate output option.
        command = command.Replace("-o=\"$MERGED\"", "-o \"$MERGED\"", StringComparison.Ordinal);
        foreach (string variable in new[] { "LOCAL", "BASE", "REMOTE", "MERGED", "BACKUP" })
        {
            command = command.Replace($"\"${variable}\"", $"@@ \"${variable}\" @@", StringComparison.Ordinal);
        }

        return command;
    }

    internal static string GetEditorCommand(LocatedTool location, string parameters)
    {
        if (IsFlatpakApp(location))
        {
            string script = $"exec {QuoteShell(location.Executable.ToPosixPath())} run --file-forwarding {QuoteShell(location.PrefixArguments[1])} {parameters} @@ \"$@\" @@";
            return $"sh -c {QuoteShell(script)} gitextensions-editor";
        }

        string prefix = location.PrefixArguments.Count == 0 ? string.Empty : string.Join(" ", location.PrefixArguments) + " ";
        return $"{location.Executable.Quote()} {prefix}{parameters}";
    }

    internal static string QuoteShell(string text)
        => "'" + text.Replace("'", "'\\''", StringComparison.Ordinal) + "'";

    internal string GetToolPath(DiffMergeToolConfigurationManager shared, string? name, DiffMergeToolType type, IConfigValueStore? settings)
    {
        string original = shared.GetToolPath(name, type);
        if (string.IsNullOrWhiteSpace(name) || HasConfiguredPath(settings, name, type))
        {
            return original;
        }

        DiffMergeToolConfiguration config = shared.LoadDiffMergeToolConfig(name, null);
        return original == config.ExeFileName ? Adapt(config).Path : original;
    }

    internal string GetToolCommand(DiffMergeToolConfigurationManager shared, string? name, DiffMergeToolType type, IConfigValueStore? settings)
    {
        string original = shared.GetToolCommand(name, type);
        if (string.IsNullOrWhiteSpace(name) || HasConfiguredPath(settings, name, type) || HasSetting(settings, name, type, "cmd"))
        {
            return original;
        }

        DiffMergeToolConfiguration config = shared.LoadDiffMergeToolConfig(name, null);
        string originalDefault = type == DiffMergeToolType.Diff ? config.FullDiffCommand : config.FullMergeCommand;
        if (original != originalDefault)
        {
            return original;
        }

        DiffMergeToolConfiguration adapted = Adapt(config);
        return type == DiffMergeToolType.Diff ? adapted.FullDiffCommand : adapted.FullMergeCommand;
    }

    internal DiffMergeToolConfiguration LoadDiffMergeToolConfig(DiffMergeToolConfigurationManager shared, string name, string? path, IConfigValueStore? settings)
    {
        DiffMergeToolConfiguration original = shared.LoadDiffMergeToolConfig(name, path);
        if (!_isFlatpak && _platform == TerminalPlatform.Linux && Path.GetFileName(original.Path) == "flatpak"
            && _fileExists(original.Path) && FlatpakIds.TryGetValue(GetUnixName(original.ExeFileName), out string[]? ids))
        {
            IReadOnlyCollection<string> installed = _listFlatpakApps(original.Path);
            string? id = ids.FirstOrDefault(candidate => installed.Contains(candidate, StringComparer.Ordinal));
            if (id is not null)
            {
                return WithLocation(original, new LocatedTool(original.Path, ["run", id]));
            }
        }

        return !string.IsNullOrWhiteSpace(path) || HasConfiguredPath(settings, name, DiffMergeToolType.Merge)
            ? original
            : Adapt(original);
    }

    private static bool HasConfiguredPath(IConfigValueStore? settings, string name, DiffMergeToolType type)
        => HasSetting(settings, name, type, "path") || HasSetting(settings, name, DiffMergeToolType.Merge, "path");

    internal static string? GetArguments(string? executable, string? command)
    {
        if (string.IsNullOrWhiteSpace(executable) || string.IsNullOrWhiteSpace(command))
        {
            return command;
        }

        string text = command.TrimStart();
        foreach (string prefix in new[] { "\"" + executable + "\"", executable })
        {
            if (text.StartsWith(prefix, StringComparison.Ordinal)
                && (text.Length == prefix.Length || char.IsWhiteSpace(text[prefix.Length])))
            {
                return text[prefix.Length..].TrimStart();
            }
        }

        return command;
    }

    private static bool HasSetting(IConfigValueStore? settings, string name, DiffMergeToolType type, string suffix)
        => !string.IsNullOrWhiteSpace(settings?.GetValue($"{(type == DiffMergeToolType.Diff ? "difftool" : "mergetool")}.{name}.{suffix}"));

    internal string GetFallbackExecutableName(string executable)
        => _platform != TerminalPlatform.Windows ? GetUnixName(executable)
            : Path.GetExtension(executable) == ".exe" ? Path.GetFileNameWithoutExtension(executable) : executable;

    private static string? FindOnPath(string name)
    {
        string? searchPath = Environment.GetEnvironmentVariable("PATH");
        if (searchPath is null)
        {
            return null;
        }

        foreach (string directory in searchPath.Split(Path.PathSeparator))
        {
            string candidate = Path.Combine(directory, name);
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }

    private static IReadOnlyCollection<string> ListFlatpakApps(string flatpak)
    {
        if (Design.IsDesignMode || !File.Exists(flatpak))
        {
            return [];
        }

        ProcessStartInfo request = new(flatpak);
        request.ArgumentList.Add("list");
        request.ArgumentList.Add("--app");
        request.ArgumentList.Add("--columns=application");
        (int exitCode, string output, _) = FlatpakHost.Query(request);
        return exitCode == 0 ? output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries) : [];
    }

    private static bool IsKnownExecutable(string executable)
    {
        string name = GetUnixName(executable);
        string[] additional = ["vi", "notepad", "notepad++", "subl", "code", "zed", "gource", "xdg-terminal-exec", "x-terminal-emulator", "gnome-terminal", "konsole", "xterm"];
        if (additional.Contains(name, StringComparer.Ordinal))
        {
            return true;
        }

        DiffMergeToolConfigurationManager shared = new(() => null);
        return RegisteredDiffMergeTools.All(DiffMergeToolType.Diff).Concat(RegisteredDiffMergeTools.All(DiffMergeToolType.Merge))
            .Distinct(StringComparer.Ordinal)
            .Any(tool => GetUnixName(shared.LoadDiffMergeToolConfig(tool, null).ExeFileName) == name);
    }

    private static string GetUnixName(string executable)
        => executable.ToLowerInvariant() switch
        {
            "code.exe" => "code",
            "meld.exe" => "meld",
            "kdiff3.exe" => "kdiff3",
            "bcomp.exe" => "bcompare",
            "p4merge.exe" => "p4merge",
            "compare.exe" => "compare",
            "sgdm.exe" => "diffmerge",
            "smerge.exe" => "smerge",
            "zed.exe" => "zed",
            "sublime_text.exe" => "subl",
            _ => executable,
        };

    // parity-scaffolding: substitutes only desktop/filesystem/process answers in locator integration tests.
    internal static class TestAccessor
    {
        internal static PlatformToolLocator Current
        {
            get => _current;
            set => _current = value;
        }
    }
}

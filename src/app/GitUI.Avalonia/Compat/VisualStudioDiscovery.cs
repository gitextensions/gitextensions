using GitCommands;
using GitExtensions.Extensibility;

namespace GitUI.Compat;

// parity-scaffolding: substitute external I/O while exercising the original static initialization path.
internal static class VisualStudioDiscovery
{
    // parity-scaffolding: replace only external installer-location and process I/O in discovery tests.
    internal static Func<string> VsWherePath = () => $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Microsoft Visual Studio\Installer\vswhere.exe";

    // parity-scaffolding: retain the real Executable by default, with an external process substitute for tests.
    internal static Func<string, IExecutable> CreateExecutable = path => new Executable(path);
}

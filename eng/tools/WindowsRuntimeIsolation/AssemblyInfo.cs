using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

// parity-scaffolding: exposes the real runtime-isolation boundary to its tool contracts.
[assembly: InternalsVisibleTo("WindowsRuntimeIsolation.Tests")]

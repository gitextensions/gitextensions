using System.Reflection;
using System.Runtime.InteropServices;

// Variant of CommonAssemblyInfo.cs for the cross-platform Avalonia port assemblies:
// identical metadata without the SupportedOSPlatform("windows7.0") attribute, which would
// mark these assemblies Windows-only (and make NUnit skip every test on other platforms).

[assembly: AssemblyTitle("Git Extensions")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Git Extensions")]
[assembly: AssemblyProduct("Git Extensions")]
[assembly: AssemblyCopyright("Copyright © 2008-2026 Git Extensions Team")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("33.33.33")]
[assembly: AssemblyFileVersion("33.33.33")]
[assembly: AssemblyInformationalVersion("33.33.33")]

// Disable CLS compliance. See https://github.com/gitextensions/gitextensions/issues/4710
[assembly: CLSCompliant(isCompliant: false)]

[assembly: ComVisible(false)]

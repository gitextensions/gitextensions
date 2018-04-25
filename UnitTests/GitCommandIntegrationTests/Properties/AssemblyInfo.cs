using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ApprovalTests.Reporters;

[assembly: AssemblyTitle("GitCommandIntegrationTests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("GitCommandIntegrationTests")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("62c7a5d4-dd49-413d-81d5-01b9a7f03287")]

// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: UseReporter(typeof(DiffReporter))]

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------



using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NBug")]
[assembly: AssemblyDescription("NBug bug reporting library created by Teoman Soygul.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6c55f06b-1a75-4e10-ad9a-168604ee2d91")]

// Set up for unit testing
[assembly:
	InternalsVisibleTo(
		"NBug.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100298c7bc6603288f349c24fe85cebfba2efbc6a88af29805efc852b93a86ba3c2e0a6b9979f49803b579387fcff27b2e97526b8001b3a68e0785da60352f9918a433cc4e7c3b143341289b10c0fd250d8aeddd5edecb049072d5a78c2eecb42fc5e3e5c32161cfc445bdc8dc366b0e52389cc50b07168b45d411c56e541b5bdae"
		)]
[assembly:
	InternalsVisibleTo(
		"NBug.Configurator, PublicKey=0024000004800000940000000602000000240000525341310004000001000100298c7bc6603288f349c24fe85cebfba2efbc6a88af29805efc852b93a86ba3c2e0a6b9979f49803b579387fcff27b2e97526b8001b3a68e0785da60352f9918a433cc4e7c3b143341289b10c0fd250d8aeddd5edecb049072d5a78c2eecb42fc5e3e5c32161cfc445bdc8dc366b0e52389cc50b07168b45d411c56e541b5bdae"
		)]

// Version information for an assembly consists of the following four values:
//      Major Version
//      Minor Version
//      Build Number
//      Revision
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
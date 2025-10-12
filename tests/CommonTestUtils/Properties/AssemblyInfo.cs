﻿using System.Reflection;
using System.Runtime.CompilerServices;
using CommonTestUtils;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Common Test Utils")]

[assembly: TestAppSettings]
[assembly: NUnit.Framework.Category("UnitTests")]

[assembly: InternalsVisibleTo("CommonTestUtils.Tests")]

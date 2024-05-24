﻿using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyDescription("GitExtensions UI")]

[assembly: InternalsVisibleTo("GitUI.Tests")]
[assembly: InternalsVisibleTo("UI.IntegrationTests")]
[assembly: InternalsVisibleTo("TranslationApp")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // required for NSubstitute for mocking internal members

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copied from https://github.com/dotnet/roslyn/blob/315c2e149b/src/Workspaces/CoreTestUtilities/MEF/ExportProviderCache.cs with some tweaks

using Microsoft.VisualStudio.Composition;

namespace CommonTestUtils.MEF
{
    public static partial class ExportProviderCache
    {
        private sealed class Scope
        {
            public readonly string Name;
            public ExportProvider? CurrentExportProvider;
            public ComposableCatalog? ExpectedCatalog;
            public ExportProvider? ExpectedProviderForCatalog;

            public Scope(string name)
            {
                Name = name;
            }

            public void Clear()
            {
                CurrentExportProvider = null;
                ExpectedCatalog = null;
                ExpectedProviderForCatalog = null;
            }
        }
    }
}

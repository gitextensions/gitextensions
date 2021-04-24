// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copied from https://github.com/dotnet/roslyn/blob/315c2e149b/src/Workspaces/CoreTestUtilities/MEF/ExportProviderCache.cs with some tweaks

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.VisualStudio.Composition;

namespace CommonTestUtils.MEF
{
    public static partial class ExportProviderCache
    {
        private sealed class SingleExportProviderFactory : IExportProviderFactory
        {
            private readonly Scope _scope;
            private readonly ComposableCatalog _catalog;
            private readonly CompositionConfiguration _configuration;
            private readonly IExportProviderFactory _exportProviderFactory;

            public SingleExportProviderFactory(Scope scope, ComposableCatalog catalog, CompositionConfiguration configuration, IExportProviderFactory exportProviderFactory)
            {
                _scope = scope;
                _catalog = catalog;
                _configuration = configuration;
                _exportProviderFactory = exportProviderFactory;
            }

            public ExportProvider GetOrCreateExportProvider()
            {
                var expectedCatalog = Interlocked.CompareExchange(ref _scope.ExpectedCatalog, _catalog, null) ?? _catalog;
                RequireForSingleExportProvider(expectedCatalog == _catalog);

                var expected = _scope.ExpectedProviderForCatalog;
                if (expected == null)
                {
                    foreach (var errorCollection in _configuration.CompositionErrors)
                    {
                        foreach (var error in errorCollection)
                        {
                            foreach (var part in error.Parts)
                            {
                                foreach (var (importBinding, exportBindings) in part.SatisfyingExports)
                                {
                                    if (exportBindings.Count <= 1)
                                    {
                                        // Ignore composition errors for missing parts
                                        continue;
                                    }

                                    if (importBinding.ImportDefinition.Cardinality != ImportCardinality.ZeroOrMore)
                                    {
                                        // This failure occurs when a binding fails because multiple exports were
                                        // provided but only a single one (at most) is expected. This typically occurs
                                        // when a test ExportProvider is created with a mock implementation without
                                        // first removing a value provided by default.
                                        throw new InvalidOperationException(
                                            "Failed to construct the MEF catalog for testing. Multiple exports were found for a part for which only one export is expected:" + Environment.NewLine
                                            + error.Message);
                                    }
                                }
                            }
                        }
                    }

                    expected = _exportProviderFactory.CreateExportProvider();
                    expected = Interlocked.CompareExchange(ref _scope.ExpectedProviderForCatalog, expected, null) ?? expected;
                    Interlocked.CompareExchange(ref _scope.CurrentExportProvider, expected, null);
                }

                var exportProvider = _scope.CurrentExportProvider;
                RequireForSingleExportProvider(exportProvider == expected);

                return exportProvider;
            }

            ExportProvider IExportProviderFactory.CreateExportProvider()
            {
                // Currently this implementation deviates from the typical behavior of IExportProviderFactory. For the
                // duration of a single test, an instance of SingleExportProviderFactory will continue returning the
                // same ExportProvider instance each time this method is called.
                //
                // It may be clearer to refactor the implementation to only allow one call to CreateExportProvider in
                // the context of a single test. https://github.com/dotnet/roslyn/issues/25863
                return GetOrCreateExportProvider();
            }

            private void RequireForSingleExportProvider([DoesNotReturnIf(false)] bool condition)
            {
                if (!condition)
                {
                    // The ExportProvider provides services that act as singleton instances in the context of an
                    // application (this include cases of multiple exports, where the 'singleton' is the list of all
                    // exports matching the contract). When reasoning about the behavior of test code, it is valuable to
                    // know service instances will be used in a consistent manner throughout the execution of a test,
                    // regardless of whether they are passed as arguments or obtained through requests to the
                    // ExportProvider.
                    //
                    // Restricting a test to a single ExportProvider guarantees that objects that *look* like singletons
                    // will *behave* like singletons for the duration of the test. Each test is expected to create and
                    // use its ExportProvider in a consistent manner.
                    //
                    // A test that validates remote services is allowed to create a couple of ExportProviders:
                    // one for local workspace and the other for the remote one.
                    //
                    // When this exception is thrown by a test, it typically means one of the following occurred:
                    //
                    // * A test failed to pass an ExportProvider via an optional argument to a method, resulting in the
                    //   method attempting to create a default ExportProvider which did not match the one assigned to
                    //   the test.
                    // * A test attempted to perform multiple test sequences in the context of a single test method,
                    //   rather than break up the test into distinct tests for each case.
                    // * A test referenced different predefined ExportProvider instances within the context of a test.
                    //   Each test is expected to use the same ExportProvider throughout the test.
                    throw new InvalidOperationException($"Only one {_scope.Name} {nameof(ExportProvider)} can be created in the context of a single test.");
                }
            }
        }
    }
}

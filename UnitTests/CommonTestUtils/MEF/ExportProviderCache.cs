// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copied from https://github.com/dotnet/roslyn/blob/315c2e149b/src/Workspaces/CoreTestUtilities/MEF/ExportProviderCache.cs with some tweaks

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Composition;

namespace CommonTestUtils.MEF
{
    public static partial class ExportProviderCache
    {
        private static readonly PartDiscovery _partDiscovery = CreatePartDiscovery(Resolver.DefaultInstance);
        private static readonly Scope _localCompositionScope = new("local");

        internal static ExportProvider? LocalExportProviderForCleanup => _localCompositionScope.CurrentExportProvider;

        /// <summary>
        /// Use to create <see cref="IExportProviderFactory"/> for default instances of <see cref="MefHostServices"/>.
        /// </summary>
        public static IExportProviderFactory GetOrCreateExportProviderFactory(IEnumerable<Assembly> assemblies)
        {
            return CreateExportProviderFactory(CreateAssemblyCatalog(assemblies), isRemoteHostComposition: false);
        }

        public static ComposableCatalog CreateAssemblyCatalog(IEnumerable<Assembly> assemblies, Resolver? resolver = null)
        {
            var discovery = resolver == null ? _partDiscovery : CreatePartDiscovery(resolver);

            // If we run CreatePartsAsync on the test thread we may deadlock since it'll schedule stuff back
            // on the thread.
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            var parts = Task.Run(async () => await discovery.CreatePartsAsync(assemblies).ConfigureAwait(false)).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            return ComposableCatalog.Create(resolver ?? Resolver.DefaultInstance).AddParts(parts);
        }

        public static ComposableCatalog CreateTypeCatalog(IEnumerable<Type> types, Resolver? resolver = null)
        {
            var discovery = resolver == null ? _partDiscovery : CreatePartDiscovery(resolver);

            // If we run CreatePartsAsync on the test thread we may deadlock since it'll schedule stuff back
            // on the thread.
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            var parts = Task.Run(async () => await discovery.CreatePartsAsync(types).ConfigureAwait(false)).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            return ComposableCatalog.Create(resolver ?? Resolver.DefaultInstance).AddParts(parts);
        }

        public static Resolver CreateResolver()
        {
            // simple assembly loader is stateless, so okay to share
            return new Resolver(SimpleAssemblyLoader.Instance);
        }

        public static PartDiscovery CreatePartDiscovery(Resolver resolver)
            => PartDiscovery.Combine(new AttributedPartDiscoveryV1(resolver), new AttributedPartDiscovery(resolver, isNonPublicSupported: true));

        public static ComposableCatalog WithParts(this ComposableCatalog catalog, IEnumerable<Type> types)
            => catalog.AddParts(CreateTypeCatalog(types).DiscoveredParts);

        /// <summary>
        /// Creates a <see cref="ComposableCatalog"/> derived from <paramref name="catalog"/>, but with all exported
        /// parts assignable to any type in <paramref name="types"/> removed from the catalog.
        /// </summary>
        public static ComposableCatalog WithoutPartsOfTypes(this ComposableCatalog catalog, IEnumerable<Type> types)
        {
            var parts = catalog.Parts.Where(composablePartDefinition => !IsExcludedPart(composablePartDefinition));
            return ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);

            bool IsExcludedPart(ComposablePartDefinition part)
            {
                return types.Any(excludedType => excludedType.IsAssignableFrom(part.Type));
            }
        }

        public static IExportProviderFactory CreateExportProviderFactory(ComposableCatalog catalog, bool isRemoteHostComposition)
        {
            var scope = _localCompositionScope;
            var configuration = CompositionConfiguration.Create(catalog.WithCompositionService());
            var runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
            var exportProviderFactory = runtimeComposition.CreateExportProviderFactory();

            return new SingleExportProviderFactory(scope, catalog, configuration, exportProviderFactory);
        }
    }
}

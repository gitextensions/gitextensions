﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copied from https://github.com/dotnet/roslyn/blob/315c2e149b/src/Workspaces/CoreTestUtilities/MEF/TestComposition.cs with some tweaks

using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.Composition;

namespace CommonTestUtils.MEF
{
    /// <summary>
    /// Represents a MEF composition used for testing.
    /// </summary>
    public sealed partial class TestComposition
    {
        public static readonly TestComposition Empty = new(ImmutableHashSet<Assembly>.Empty, ImmutableHashSet<Type>.Empty, ImmutableHashSet<Type>.Empty);
        private static readonly Dictionary<CacheKey, IExportProviderFactory> _factoryCache = new();

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly ImmutableArray<Assembly> _assemblies;
            private readonly ImmutableArray<Type> _parts;
            private readonly ImmutableArray<Type> _excludedPartTypes;

            public CacheKey(ImmutableHashSet<Assembly> assemblies, ImmutableHashSet<Type> parts, ImmutableHashSet<Type> excludedPartTypes)
            {
                _assemblies = assemblies.OrderBy(a => a.FullName).ToImmutableArray();
                _parts = parts.OrderBy(a => a.FullName).ToImmutableArray();
                _excludedPartTypes = excludedPartTypes.OrderBy(a => a.FullName).ToImmutableArray();
            }

            public override bool Equals(object? obj)
                => obj is CacheKey key && Equals(key);

            public bool Equals(CacheKey other)
                => _parts.SequenceEqual(other._parts) &&
                   _excludedPartTypes.SequenceEqual(other._excludedPartTypes) &&
                   _assemblies.SequenceEqual(other._assemblies);

            public override int GetHashCode()
                => Hash.Combine(Hash.Combine(Hash.CombineValues(_assemblies), Hash.CombineValues(_parts)), Hash.CombineValues(_excludedPartTypes));

            public static bool operator ==(CacheKey left, CacheKey right)
                => left.Equals(right);

            public static bool operator !=(CacheKey left, CacheKey right)
                => !(left == right);
        }

        /// <summary>
        /// Assemblies to include in the composition.
        /// </summary>
        public readonly ImmutableHashSet<Assembly> Assemblies;

        /// <summary>
        /// Types to exclude from the composition.
        /// All subtypes of types specified in <see cref="ExcludedPartTypes"/> and defined in <see cref="Assemblies"/> are excluded before <see cref="Parts"/> are added.
        /// </summary>
        public readonly ImmutableHashSet<Type> ExcludedPartTypes;

        /// <summary>
        /// Additional part types to add to the composition.
        /// </summary>
        public readonly ImmutableHashSet<Type> Parts;

        private readonly Lazy<IExportProviderFactory> _exportProviderFactory;

        private TestComposition(ImmutableHashSet<Assembly> assemblies, ImmutableHashSet<Type> parts, ImmutableHashSet<Type> excludedPartTypes)
        {
            Assemblies = assemblies;
            Parts = parts;
            ExcludedPartTypes = excludedPartTypes;

            _exportProviderFactory = new Lazy<IExportProviderFactory>(GetOrCreateFactory);
        }

        /// <summary>
        /// VS MEF <see cref="ExportProvider"/>.
        /// </summary>
        public IExportProviderFactory ExportProviderFactory => _exportProviderFactory.Value;

        private IExportProviderFactory GetOrCreateFactory()
        {
            CacheKey key = new(Assemblies, Parts, ExcludedPartTypes);

            lock (_factoryCache)
            {
                if (_factoryCache.TryGetValue(key, out var existing))
                {
                    return existing;
                }
            }

            var newFactory = ExportProviderCache.CreateExportProviderFactory(GetCatalog(), isRemoteHostComposition: false);

            lock (_factoryCache)
            {
                if (_factoryCache.TryGetValue(key, out var existing))
                {
                    return existing;
                }

                _factoryCache.Add(key, newFactory);
            }

            return newFactory;
        }

        private ComposableCatalog GetCatalog()
            => ExportProviderCache.CreateAssemblyCatalog(Assemblies, ExportProviderCache.CreateResolver()).WithoutPartsOfTypes(ExcludedPartTypes).WithParts(Parts);

        public CompositionConfiguration GetCompositionConfiguration()
            => CompositionConfiguration.Create(GetCatalog());

        public TestComposition Add(TestComposition composition)
            => AddAssemblies(composition.Assemblies).AddParts(composition.Parts).AddExcludedPartTypes(composition.ExcludedPartTypes);

        public TestComposition AddAssemblies(params Assembly[]? assemblies)
            => AddAssemblies((IEnumerable<Assembly>?)assemblies);

        public TestComposition AddAssemblies(IEnumerable<Assembly>? assemblies)
            => WithAssemblies(Assemblies.Union(assemblies ?? Array.Empty<Assembly>()));

        public TestComposition AddParts(IEnumerable<Type>? types)
            => WithParts(Parts.Union(types ?? Array.Empty<Type>()));

        public TestComposition AddParts(params Type[]? types)
            => AddParts((IEnumerable<Type>?)types);

        public TestComposition AddExcludedPartTypes(IEnumerable<Type>? types)
            => WithExcludedPartTypes(ExcludedPartTypes.Union(types ?? Array.Empty<Type>()));

        public TestComposition AddExcludedPartTypes(params Type[]? types)
            => AddExcludedPartTypes((IEnumerable<Type>?)types);

        public TestComposition Remove(TestComposition composition)
            => RemoveAssemblies(composition.Assemblies).RemoveParts(composition.Parts).RemoveExcludedPartTypes(composition.ExcludedPartTypes);

        public TestComposition RemoveAssemblies(params Assembly[]? assemblies)
            => RemoveAssemblies((IEnumerable<Assembly>?)assemblies);

        public TestComposition RemoveAssemblies(IEnumerable<Assembly>? assemblies)
            => WithAssemblies(Assemblies.Except(assemblies ?? Array.Empty<Assembly>()));

        public TestComposition RemoveParts(IEnumerable<Type>? types)
            => WithParts(Parts.Except(types ?? Array.Empty<Type>()));

        public TestComposition RemoveParts(params Type[]? types)
            => RemoveParts((IEnumerable<Type>?)types);

        public TestComposition RemoveExcludedPartTypes(IEnumerable<Type>? types)
            => WithExcludedPartTypes(ExcludedPartTypes.Except(types ?? Array.Empty<Type>()));

        public TestComposition RemoveExcludedPartTypes(params Type[]? types)
            => RemoveExcludedPartTypes((IEnumerable<Type>?)types);

        public TestComposition WithAssemblies(ImmutableHashSet<Assembly> assemblies)
        {
            if (assemblies == Assemblies)
            {
                return this;
            }

            var testAssembly = assemblies.FirstOrDefault(IsTestAssembly);
            if (testAssembly == null)
            {
                throw new NullReferenceException($"Test assemblies are not allowed in test composition: {testAssembly}. Specify explicit test parts instead.");
            }

            return new TestComposition(assemblies, Parts, ExcludedPartTypes);

            static bool IsTestAssembly(Assembly assembly)
            {
                var name = assembly.GetName().Name!;
                return
                    name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(".UnitTests", StringComparison.OrdinalIgnoreCase) ||
                    name.IndexOf("Test.Utilities", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        public TestComposition WithParts(ImmutableHashSet<Type> parts)
            => parts == Parts ? this : new TestComposition(Assemblies, parts, ExcludedPartTypes);

        public TestComposition WithExcludedPartTypes(ImmutableHashSet<Type> excludedPartTypes)
            => excludedPartTypes == ExcludedPartTypes ? this : new TestComposition(Assemblies, Parts, excludedPartTypes);

        /// <summary>
        /// Use for VS MEF composition troubleshooting.
        /// </summary>
        /// <returns>All composition error messages.</returns>
        internal string GetCompositionErrorLog()
        {
            var configuration = CompositionConfiguration.Create(GetCatalog());

            StringBuilder sb = new();
            foreach (var errorGroup in configuration.CompositionErrors)
            {
                foreach (var error in errorGroup)
                {
                    sb.Append(error.Message);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}

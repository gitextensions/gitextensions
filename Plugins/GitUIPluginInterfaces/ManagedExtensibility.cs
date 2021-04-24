using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GitUI;
using Microsoft.VisualStudio.Composition;

namespace GitUIPluginInterfaces
{
    public static class ManagedExtensibility
    {
        private static ComposableCatalog? _aggregateCatalog;
        private static Lazy<ExportProvider>? _exportProvider;

        /// <summary>
        /// Gets a root path where user installed plugins are located.
        /// </summary>
        public static string? UserPluginsPath { get; private set; }

        /// <summary>
        /// Sets a root path to a folder where user plugins are located.
        /// </summary>
        /// <param name="userPluginsPath">A root path to a folder where user plugins are located.</param>
        public static void SetUserPluginsPath(string userPluginsPath)
        {
            if (UserPluginsPath is not null)
            {
                throw new InvalidOperationException("The user plugins path has already been initialized.");
            }

            UserPluginsPath = userPluginsPath;
        }

        private static Lazy<ExportProvider> GetOrCreateLazyExportProvider(string? applicationDataFolder)
        {
            Lazy<ExportProvider> lazyExportProvider = Volatile.Read(ref _exportProvider);
            if (lazyExportProvider is null)
            {
                string capturedApplicationDataFolder = applicationDataFolder;
                Lazy<ExportProvider> newLazyExportProvider = new(() => CreateExportProvider(capturedApplicationDataFolder), LazyThreadSafetyMode.ExecutionAndPublication);
                lazyExportProvider = Interlocked.CompareExchange(ref _exportProvider, newLazyExportProvider, null) ?? newLazyExportProvider;
            }

            return lazyExportProvider;
        }

        private static ExportProvider CreateExportProvider(string? applicationDataFolder)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string defaultPluginsPath = Path.Combine(new FileInfo(Application.ExecutablePath).Directory.FullName, "Plugins");
            string? userPluginsPath = UserPluginsPath;

            IEnumerable<FileInfo> pluginFiles = PluginsPathScanner.GetFiles(defaultPluginsPath, userPluginsPath);
            string cacheFile = Path.Combine(applicationDataFolder ?? "ignored", "Plugins", "composition.cache");
            IExportProviderFactory exportProviderFactory;
            if (applicationDataFolder is not null && File.Exists(cacheFile))
            {
                using FileStream cacheStream = File.OpenRead(cacheFile);
                exportProviderFactory = ThreadHelper.JoinableTaskFactory.Run(() => new CachedComposition().LoadExportProviderFactoryAsync(cacheStream, Resolver.DefaultInstance));
            }
            else
            {
                Assembly[] assemblies = pluginFiles.Select(assemblyFile => TryLoadAssembly(assemblyFile)).WhereNotNull().ToArray();

                PartDiscovery? discovery = PartDiscovery.Combine(
                    new AttributedPartDiscoveryV1(Resolver.DefaultInstance),
                    new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true));
                DiscoveredParts? parts = ThreadHelper.JoinableTaskFactory.Run(() => discovery.CreatePartsAsync(assemblies));
                ComposableCatalog catalog = ComposableCatalog.Create(Resolver.DefaultInstance)
                    .AddCatalog(_aggregateCatalog)
                    .AddParts(parts);

                CompositionConfiguration configuration = CompositionConfiguration.Create(catalog.WithCompositionService());
                RuntimeComposition runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
                if (applicationDataFolder is not null)
                {
#if false // Composition caching currently disabled
                    Directory.CreateDirectory(Path.Combine(applicationDataFolder, "Plugins"));
                    using var cacheStream = File.OpenWrite(cacheFile);
                    ThreadHelper.JoinableTaskFactory.Run(() => new CachedComposition().SaveAsync(runtimeComposition, cacheStream));
#endif
                }

                exportProviderFactory = runtimeComposition.CreateExportProviderFactory();
            }

            return exportProviderFactory.CreateExportProvider();
        }

        private static Assembly? TryLoadAssembly(FileInfo file)
        {
            try
            {
                return Assembly.LoadFile(file.FullName);
            }
            catch
            {
                return null;
            }
        }

        public static void Initialise(IEnumerable<Assembly> assemblies)
        {
            PartDiscovery? discovery = PartDiscovery.Combine(
              new AttributedPartDiscoveryV1(Resolver.DefaultInstance),
              new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true));
            DiscoveredParts? parts = ThreadHelper.JoinableTaskFactory.Run(() => discovery.CreatePartsAsync(assemblies));

            ComposableCatalog? catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);

            _aggregateCatalog = catalog;
        }

        public static Lazy<T> GetExport<T>()
        {
            return GetOrCreateLazyExportProvider(null).Value.GetExport<T>();
        }

        public static IEnumerable<Lazy<T>> GetExports<T>()
        {
            return GetOrCreateLazyExportProvider(null).Value.GetExports<T>();
        }

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>()
        {
            return GetOrCreateLazyExportProvider(null).Value.GetExports<T, TMetadataView>();
        }

        public static void SetTestExportProvider(ExportProvider exportProvider)
        {
            _exportProvider = new Lazy<ExportProvider>(() => exportProvider);
        }
    }
}

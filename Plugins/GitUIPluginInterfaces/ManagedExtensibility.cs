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
        private static Lazy<ExportProvider> _exportProvider;

        private static Lazy<ExportProvider> GetOrCreateLazyExportProvider(string applicationDataFolder)
        {
            var lazyExportProvider = Volatile.Read(ref _exportProvider);
            if (lazyExportProvider is null)
            {
                var capturedApplicationDataFolder = applicationDataFolder;
                var newLazyExportProvider = new Lazy<ExportProvider>(() => CreateExportProvider(capturedApplicationDataFolder), LazyThreadSafetyMode.ExecutionAndPublication);
                lazyExportProvider = Interlocked.CompareExchange(ref _exportProvider, newLazyExportProvider, null) ?? newLazyExportProvider;
            }

            return lazyExportProvider;
        }

        private static ExportProvider CreateExportProvider(string applicationDataFolder)
        {
            var stopwatch = Stopwatch.StartNew();

            var file = new FileInfo(Application.ExecutablePath);

            FileInfo[] plugins =
                Directory.Exists(Path.Combine(file.Directory.FullName, "Plugins"))
                ? new DirectoryInfo(Path.Combine(file.Directory.FullName, "Plugins")).GetFiles("*.dll")
                : new FileInfo[] { };

            var pluginFiles = plugins;

            var cacheFile = Path.Combine(applicationDataFolder ?? "ignored", "Plugins", "composition.cache");
            IExportProviderFactory exportProviderFactory;
            if (applicationDataFolder != null && File.Exists(cacheFile))
            {
                using (var cacheStream = File.OpenRead(cacheFile))
                {
                    exportProviderFactory = ThreadHelper.JoinableTaskFactory.Run(() => new CachedComposition().LoadExportProviderFactoryAsync(cacheStream, Resolver.DefaultInstance));
                }
            }
            else
            {
                var assemblies = pluginFiles.Select(assemblyFile => TryLoadAssembly(assemblyFile)).Where(assembly => assembly != null).ToArray();

                var discovery = PartDiscovery.Combine(
                    new AttributedPartDiscoveryV1(Resolver.DefaultInstance),
                    new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true));
                var parts = ThreadHelper.JoinableTaskFactory.Run(() => discovery.CreatePartsAsync(assemblies));
                var catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);

                var configuration = CompositionConfiguration.Create(catalog.WithCompositionService());
                var runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
                if (applicationDataFolder != null)
                {
#if false // Composition caching currently disabled
                    Directory.CreateDirectory(Path.Combine(applicationDataFolder, "Plugins"));
                    using (var cacheStream = File.OpenWrite(cacheFile))
                    {
                        ThreadHelper.JoinableTaskFactory.Run(() => new CachedComposition().SaveAsync(runtimeComposition, cacheStream));
                    }
#endif
                }

                exportProviderFactory = runtimeComposition.CreateExportProviderFactory();
            }

            return exportProviderFactory.CreateExportProvider();
        }

        private static Assembly TryLoadAssembly(FileInfo file)
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                if (assemblyName == null)
                {
                    return null;
                }

                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }

        public static void SetApplicationDataFolder(string applicationDataFolder)
        {
            GetOrCreateLazyExportProvider(applicationDataFolder);
        }

        public static IEnumerable<Lazy<T>> GetExports<T>()
        {
            return GetOrCreateLazyExportProvider(null).Value.GetExports<T>();
        }

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>()
        {
            return GetOrCreateLazyExportProvider(null).Value.GetExports<T, TMetadataView>();
        }
    }
}
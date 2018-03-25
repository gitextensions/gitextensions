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
        private static readonly Lazy<ExportProvider> _exportProvider = new Lazy<ExportProvider>(CreateExportProvider, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ExportProvider CreateExportProvider()
        {
            var stopwatch = Stopwatch.StartNew();

            var file = new FileInfo(Application.ExecutablePath);

            FileInfo[] plugins =
                Directory.Exists(Path.Combine(file.Directory.FullName, "Plugins"))
                ? new DirectoryInfo(Path.Combine(file.Directory.FullName, "Plugins")).GetFiles("*.dll")
                : new FileInfo[] { };

            var pluginFiles = plugins;

            var cacheFile = @"Plugins\GitUIPluginInterfaces\composition.cache";
            IExportProviderFactory exportProviderFactory;
            if (File.Exists(cacheFile))
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
                using (var cacheStream = File.OpenWrite(cacheFile))
                {
                    ThreadHelper.JoinableTaskFactory.Run(() => new CachedComposition().SaveAsync(runtimeComposition, cacheStream));
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

        public static IEnumerable<Lazy<T>> GetExports<T>()
        {
            return _exportProvider.Value.GetExports<T>();
        }

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>()
        {
            return _exportProvider.Value.GetExports<T, TMetadataView>();
        }
    }
}
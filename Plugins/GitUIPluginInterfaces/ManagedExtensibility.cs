using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public static class ManagedExtensibility
    {
        private static List<CompositionContainer> _compositionContainers;

        private static readonly object compositionContainerSyncObj = new object();

        private static readonly HashSet<string> _loggedExecptionMessages = new HashSet<string>();

        /// <summary>
        /// The MEF container.
        /// </summary>
        private static List<CompositionContainer> GetCompositionContainers()
        {
            lock (compositionContainerSyncObj)
            {
                if (_compositionContainers == null)
                {
                    _compositionContainers = new List<CompositionContainer>();
                    var pluginsDir = new DirectoryInfo(Directory.GetParent(Application.ExecutablePath).FullName + Path.DirectorySeparatorChar + "Plugins");
                    if (pluginsDir.Exists)
                    {
                        foreach (var dll in pluginsDir.EnumerateFiles("*.dll"))
                        {
                            _compositionContainers.Add(new CompositionContainer(new DirectoryCatalog(dll.DirectoryName, dll.Name)));
                        }
                    }
                }

                return _compositionContainers;
            }
        }

        public static IEnumerable<Lazy<T>> GetExports<T>()
        {
            var ret = new List<Lazy<T>>();
            foreach (var container in GetCompositionContainers())
            {
                try
                {
                    var exps = container.GetExports<T>();
                    ret.AddRange(exps);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Trace.TraceError("GetExports() failed {0}", string.Join(Environment.NewLine, ex.LoaderExceptions.Select(r => r.ToString())));
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed to get exports, {0}", ex);
                }
            }

            return ret;
        }

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>()
        {
            var ret = new List<Lazy<T, TMetadataView>>();
            foreach (var container in GetCompositionContainers())
            {
                try
                {
                    var exps = container.GetExports<T, TMetadataView>();
                    ret.AddRange(exps);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var exceptions = ex.LoaderExceptions.Where(ShouldLogException).ToList();
                    if (exceptions.Count != 0)
                    {
                        Trace.TraceError("Failed to load type when getting exports: {0}", string.Join(Environment.NewLine, exceptions.Select(r => r.ToString())));
                    }
                }
                catch (Exception ex)
                {
                    if (ShouldLogException(ex))
                    {
                        Trace.TraceError("Failed to get exports: {0}", ex);
                    }
                }
            }

            return ret;

            bool ShouldLogException(Exception e) => _loggedExecptionMessages.Add(e.Message);
        }
    }
}
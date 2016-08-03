﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class ManagedExtensibility
    {
        private static List<CompositionContainer> _compositionContainers;

        private static readonly object compositionContainerSyncObj = new object();

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

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>()
        {
            var ret = new List<Lazy<T, TMetadataView>>();
            foreach(var container in GetCompositionContainers())
            {
                try
                {
                    var exps = container.GetExports<T, TMetadataView>();
                    ret.AddRange(exps);
                }
                catch (System.Reflection.ReflectionTypeLoadException ex)
                {
                    Trace.TraceError("GetExports() failed {0}", string.Join(Environment.NewLine, ex.LoaderExceptions.Select(r => r.ToString())));
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed to get exports, {0}", ex.ToString());
                }
            }
            return ret;
        }
    }
}
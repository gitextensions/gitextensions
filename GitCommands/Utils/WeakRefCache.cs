using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GitCommands.Utils
{
    public class WeakRefCache
    {
        private Dictionary<string, WeakReference> weakMap = new Dictionary<string, WeakReference>();
        private readonly Timer _clearTimer = new Timer(60 * 1000);

        public static readonly WeakRefCache Default = new WeakRefCache();

        public WeakRefCache()
        {
            _clearTimer.Elapsed += OnClearTimer;
            _clearTimer.Start();
        }

        /// <summary>
        /// TODO add expiration time (MemoryCache) after change to .net 4 full profile
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectUniqueKey"></param>
        /// <param name="provideObject"></param>
        /// <returns></returns>
        public T Get<T>(string objectUniqueKey, Lazy<T> provideObject)
        {
            object cached = null;

            lock (weakMap)
            {
                WeakReference wref;
                if (weakMap.TryGetValue(objectUniqueKey, out wref))
                {
                    cached = wref.Target;
                }

                if (cached == null)
                {
                    cached = provideObject.Value;
                    weakMap[objectUniqueKey] = new WeakReference(cached);
                }
                else
                {
                    if (!(cached is T))
                    {
                        throw new InvalidCastException("Incompatible class for object: " + objectUniqueKey + ". Expected: " + typeof(T).FullName + ", found: " + cached.GetType().FullName);
                    }
                }
            }

            return (T)cached;
        }

        private void OnClearTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            lock (weakMap)
            {
                var toRemove = weakMap.Where(p => !p.Value.IsAlive).Select(p => p.Key).ToArray();
                foreach (var key in toRemove)
                    weakMap.Remove(key);
            }
        }
    }
}

﻿using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GitCommands.Utils
{
    public class WeakRefCache : IDisposable
    {
        private readonly Dictionary<string, WeakReference> _weakMap = new();
        private readonly Timer _clearTimer = new(60 * 1000);

        public static readonly WeakRefCache Default = new();

        public WeakRefCache()
        {
            _clearTimer.Elapsed += OnClearTimer;
            _clearTimer.Start();
        }

        // TODO add expiration time (MemoryCache) after change to .net 4 full profile

        public T Get<T>(string objectUniqueKey, Lazy<T> provideObject)
        {
            object? cached = null;

            lock (_weakMap)
            {
                if (_weakMap.TryGetValue(objectUniqueKey, out var weakReference))
                {
                    cached = weakReference.Target;
                }

                if (cached is null)
                {
                    cached = provideObject.Value;
                    _weakMap[objectUniqueKey] = new WeakReference(cached);
                }
                else
                {
                    if (cached is not T)
                    {
                        throw new InvalidCastException("Incompatible class for object: " + objectUniqueKey + ". Expected: " + typeof(T).FullName + ", found: " + cached.GetType().FullName);
                    }
                }
            }

            Debug.Assert(cached is not null, "cached is not null -- if this is violated, the annotations on SettingsContainer<,>.ctor cache are wrong");

            return (T)cached!;
        }

        private void OnClearTimer(object source, ElapsedEventArgs e)
        {
            lock (_weakMap)
            {
                var toRemove = _weakMap.Where(p => !p.Value.IsAlive).Select(p => p.Key).ToArray();
                foreach (var key in toRemove)
                {
                    _weakMap.Remove(key);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _clearTimer.Dispose();
            }
        }
    }
}

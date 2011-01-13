using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GithubSharp.Core.Services;

namespace Github
{
    public class InMemLogger : ILogProvider
    {
        private StringBuilder _out = new StringBuilder();
        public bool DebugMode { get; set; }

        public bool HandleAndReturnIfToThrowError(Exception error)
        {
            return true;
        }

        public void LogMessage(string message, params object[] arguments)
        {
            _out.AppendFormat(message, arguments);
        }

        public void Clear()
        {
            _out.Length = 0;
        }

        public override string ToString()
        {
            return _out.ToString();
        }
    }

    public class BasicCacher : ICacheProvider
    {
        private static Dictionary<string, object> _cache;
        private static object _syncObj = new object();

        private const string CachePrefix = "BC";

        static BasicCacher()
        {
            lock (_syncObj)
                _cache = new Dictionary<string, object>();
        }

        public T Get<T>(string Name) where T : class
        {
            return Get<T>(Name, DefaultDuractionInMinutes);
        }

        public T Get<T>(string Name, int CacheDurationInMinutes) where T : class
        {
            lock (_syncObj)
            {
                if (!_cache.ContainsKey(CachePrefix + Name)) return null;
                var cached = _cache[CachePrefix + Name] as CachedObject<T>;
                if (cached == null) return null;

                if (cached.When.AddMinutes(CacheDurationInMinutes) < DateTime.Now)
                    return null;

                return cached.Cached;
            }
        }

        public bool IsCached<T>(string Name) where T : class
        {
            lock (_syncObj)
                return _cache.ContainsKey(CachePrefix + Name);
        }

        public void Set<T>(T ObjectToCache, string Name) where T : class
        {
            var cacheObj = new CachedObject<T>();
            cacheObj.Cached = ObjectToCache;
            cacheObj.When = DateTime.Now;

            lock (_syncObj)
                _cache[CachePrefix + Name] = cacheObj;
        }

        public void Delete(string Name)
        {
            lock (_syncObj)
                _cache.Remove(CachePrefix + Name);
        }

        public void DeleteWhereStartingWith(string Name)
        {
            var enumerator = _cache.GetEnumerator();

            lock (_syncObj)
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key.StartsWith(CachePrefix + Name))
                        _cache.Remove(enumerator.Current.Key);
                }
            }
        }

        public void Clear()
        {
            lock (_syncObj)
                _cache.Clear();
        }

        public void DeleteAll<T>() where T : class
        {
            lock (_syncObj)
                _cache.Clear();
        }

        public int DefaultDuractionInMinutes
        {
            get { return 20; }
        }
    }
}

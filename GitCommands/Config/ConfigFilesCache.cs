using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitCommands.Config
{
    public sealed class ConfigFilesCache
    {
        private static Lazy<ConfigFilesCache> _globalInst;
        private readonly IDictionary<string, CachedConfigFile> _cache = new Dictionary<string, CachedConfigFile>();

        static ConfigFilesCache()
        {
            _globalInst = new Lazy<ConfigFilesCache>(() => new ConfigFilesCache());
        }

        public static ConfigFilesCache Global { get { return _globalInst.Value; } }

        public CachedConfigFile GetConfigFile(string fileName, bool aLocal)
        { 
            CachedConfigFile result;

            lock (_cache)
            {
                if (!_cache.TryGetValue(fileName, out result))
                {
                    result = new CachedConfigFile(this, fileName, aLocal);
                    _cache[fileName] = result;
                }
            }

            return result;
        }

        internal void RemoveFromCache(CachedConfigFile cached)
        {
            lock (_cache)
            {
                CachedConfigFile dict;
                if (_cache.TryGetValue(cached.FileName, out dict))
                {
                    if (dict == cached)
                        _cache.Remove(cached.FileName);
                }
            }        
        }

    }

    public sealed class CachedConfigFile : IDisposable
    {
        private readonly ConfigFilesCache _cache;
        private readonly string _fileName;
        private readonly bool _local;
        private Lazy<ConfigFile> _configFile;
        private bool _disposed = false;
        private readonly FileSystemWatcher _fileWatcher = new FileSystemWatcher();


        public CachedConfigFile(ConfigFilesCache cache, string fileName, bool aLocal)
        {
            _cache = cache;
            _fileName = fileName;
            _local = aLocal;            
            _fileWatcher.Path = Path.GetDirectoryName(fileName);
            _fileWatcher.Filter = Path.GetFileName(fileName);
            _fileWatcher.IncludeSubdirectories = false;
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Changed += new FileSystemEventHandler(_fileWatcher_Changed);
            Clear();            
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            _fileWatcher.EnableRaisingEvents = false;
            _configFile = new Lazy<ConfigFile>(() =>
                {
                    _fileWatcher.EnableRaisingEvents = true;
                    return new ConfigFile(_fileName, _local);
                });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cache.RemoveFromCache(this);
                _fileWatcher.Dispose();
            }
        }

        public string FileName { get { return _fileName; } }

        public ConfigFile ConfigFile
        {
            get
            {
                lock (_configFile)
                    return _configFile.Value;
            }
        }
    }
}

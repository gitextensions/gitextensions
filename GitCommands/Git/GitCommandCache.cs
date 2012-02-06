using System;
using System.Collections.Generic;
using System.Text;

namespace GitCommands
{
    /// <summary>
    /// Some commands are suitable for caching. All commands used to get info
    /// for a specific sha1 or gets a diff beteen two sha1's are suitable.
    /// WARNING: Never cache commands that return the diff between a sha1 and the head!
    /// </summary>
    public static class GitCommandCache
    {
        private struct CacheItem
        {
            public CacheItem(byte[] output, byte[] error)
            {
                this.Output = output;
                this.Error = error;
            }

            public byte[] Output;

            public byte[] Error;
        }

        //Cache limit
        const int cacheLimit = 40;

        public delegate void CachedCommandsChangedHandler();

        /// <summary>
        /// Simple dictionary to store cmd/output pairs
        /// </summary>
        private static Dictionary<string, CacheItem> commandCache = new Dictionary<string, CacheItem>(cacheLimit);
        
        /// <summary>
        /// Queue used to limit commandCache. The oldest item is removed
        /// when the limit is reached.
        /// </summary>
        private static Queue<string> queue = new Queue<string>(cacheLimit);

        private static CachedCommandsChangedHandler _CachedCommandsChanged;


        public static string[] CachedCommands()
        {
            lock (queue)
            {
                return queue.ToArray();
            }
        }

        public static bool TryGet(string cmd, Encoding encoding, out string cmdOutput)
        {
            //Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
            {
                cmdOutput = null;
                return false;
            }
            
            byte[] output, error;
            if (!TryGet(cmd, out output, out error))
            {
                cmdOutput = null;
                return false;
            }

            cmdOutput = EncodingHelper.GetString(output, error, encoding);
            return true;
             
        }

        public static bool TryGet(string cmd, out byte[] output, out byte[] error)
        {
            CacheItem item = new CacheItem();
            bool res;
            lock (queue)
                //Never cache empty commands
                res = !string.IsNullOrEmpty(cmd) && commandCache.TryGetValue(cmd, out item);

           output = item.Output;
           error = item.Error;
           return res;
        }

        public static void Add(string cmd, byte[] output, byte[] error)
        {
            //Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
                return;

            lock (queue)
            {
                commandCache[cmd] = new CacheItem(output, error);
                queue.Enqueue(cmd);

                //Limit cache to X commands
                if (queue.Count >= cacheLimit)
                    commandCache.Remove(queue.Dequeue());
            }
            FireCachedCommandsChanged();
        }

        public static event CachedCommandsChangedHandler CachedCommandsChanged
        {
            add
            {
                lock (queue)
                {
                    _CachedCommandsChanged += value;
                }
            }
            remove
            {
                lock (queue)
                {
                    _CachedCommandsChanged -= value;
                }
            }
        }

        private static void FireCachedCommandsChanged()
        {
            CachedCommandsChangedHandler handler;
            lock (queue)
            {
                handler = _CachedCommandsChanged;
            }
            if (handler != null)
            {
                handler();
            }
        }

    }
}

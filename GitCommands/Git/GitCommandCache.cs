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
        //Cache limit
        const int cacheLimit = 40;

        public delegate void CachedCommandsChangedHandler();

        /// <summary>
        /// Simple dictionary to store cmd/output pairs
        /// </summary>
        private static Dictionary<string, string> commandCache = new Dictionary<string, string>(cacheLimit);
        
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

        public static bool TryGet(string cmd, out string cmdOutput)
        {
            //Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
            {
                cmdOutput = null;
                return false;
            }
            lock (queue)
                return commandCache.TryGetValue(cmd, out cmdOutput);
        }

        public static void Add(string cmd, string cmdOutput)
        {
            //Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
                return;

            lock (queue)
            {
                commandCache[cmd] = cmdOutput;
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

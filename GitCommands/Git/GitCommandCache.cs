using System;
using System.Collections.Generic;

namespace GitCommands
{
    /// <summary>
    /// Some commands are suitable for caching. All commands used to get info
    /// for a specific sha1 or gets a diff between two sha1's are suitable.
    /// WARNING: Never cache commands that return the diff between a sha1 and the head!
    /// </summary>
    public static class GitCommandCache
    {
        private struct CacheItem
        {
            public CacheItem(byte[] output, byte[] error)
            {
                Output = output;
                Error = error;
            }

            public readonly byte[] Output;

            public readonly byte[] Error;
        }

        // Cache limit
        private const int CacheLimit = 40;

        public static event EventHandler CachedCommandsChanged = (sender, args) => { };

        /// <summary>
        /// Simple dictionary to store cmd/output pairs
        /// </summary>
        private static readonly Dictionary<string, CacheItem> CommandCache = new Dictionary<string, CacheItem>(CacheLimit);

        /// <summary>
        /// Queue used to limit commandCache. The oldest item is removed
        /// when the limit is reached.
        /// </summary>
        private static readonly Queue<string> Queue = new Queue<string>(CacheLimit);

        public static string[] CachedCommands()
        {
            lock (Queue)
            {
                return Queue.ToArray();
            }
        }

        public static bool TryGet(string cmd, out byte[] output, out byte[] error)
        {
            // Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
            {
                output = null;
                error = null;
                return false;
            }

            CacheItem item;
            lock (Queue)
            {
                if (!CommandCache.TryGetValue(cmd, out item))
                {
                    output = null;
                    error = null;
                    return false;
                }
            }

            output = item.Output;
            error = item.Error;
            return true;
        }

        public static void Add(string cmd, byte[] output, byte[] error)
        {
            // Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            lock (Queue)
            {
                CommandCache[cmd] = new CacheItem(output, error);
                Queue.Enqueue(cmd);

                // Limit cache to X commands
                if (Queue.Count >= CacheLimit)
                {
                    CommandCache.Remove(Queue.Dequeue());
                }
            }

            CachedCommandsChanged(typeof(GitCommandCache), EventArgs.Empty);
        }

        public static void CleanCache()
        {
            lock (Queue)
            {
                Queue?.Clear();
            }

            CachedCommandsChanged(typeof(GitCommandCache), EventArgs.Empty);
        }
    }
}

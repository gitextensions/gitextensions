using System;
using System.Collections.Generic;
using GitExtUtils;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <summary>
    /// Caches a set of command output/error bytes, for the purpose of avoiding repeated
    /// process invocations when the results are known to be identical across operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A bounded number of commands are cached. When that number is exceeded, least
    /// recently used commands are removed.
    /// </para>
    /// <para>
    /// Commands that don't change over time may be cached, e.g.
    /// <list type="bullet">
    ///   <item>Commit data queried by commit ID, or</item>
    ///   <item>Diffs between commit IDs.</item>
    /// </list>
    /// Commands that deal with changeable data should never be cached, e.g.
    /// <list type="bullet">
    ///   <item>Refs, because they are moveable, or</item>
    ///   <item>Commit notes, because they can change.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class CommandCache
    {
        /// <summary>
        /// Raised whenever the contents of the cache is changed.
        /// </summary>
        public event EventHandler Changed;

        private readonly MruCache<string, (byte[] output, byte[] error)> _cache;

        /// <summary>
        /// Initialises a new instance of <see cref="CommandCache"/> with specified <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">The maximum number of commands to cache.</param>
        public CommandCache(int capacity = 40)
        {
            _cache = new MruCache<string, (byte[] output, byte[] error)>(capacity: capacity);
        }

        /// <summary>
        /// Gets the list of commands stored within the cache.
        /// </summary>
        public IReadOnlyList<string> GetCachedCommands()
        {
            lock (_cache)
            {
                return _cache.Keys;
            }
        }

        /// <summary>
        /// Looks up a command's output in the cache.
        /// </summary>
        /// <param name="cmd">The command to look for.</param>
        /// <param name="output">Stored output bytes of the command, if found.</param>
        /// <param name="error">Stored error bytes of the command, if found.</param>
        /// <returns><c>true</c> if the command was found, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>false,output:null,error:null")]
        [ContractAnnotation("=>true,output:notnull,error:notnull")]
        public bool TryGet([NotNull] string cmd, out byte[] output, out byte[] error)
        {
            // Never cache empty commands
            if (!string.IsNullOrEmpty(cmd))
            {
                lock (_cache)
                {
                    if (_cache.TryGetValue(cmd, out var item))
                    {
                        (output, error) = item;
                        return true;
                    }
                }
            }

            output = null;
            error = null;
            return false;
        }

        /// <summary>
        /// Adds output and error bytes for a command.
        /// </summary>
        /// <param name="cmd">The command to add to the cache.</param>
        /// <param name="output">Output bytes of the command.</param>
        /// <param name="error">Error bytes of the command.</param>
        public void Add([NotNull] string cmd, byte[] output, byte[] error)
        {
            // Never cache empty commands
            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            lock (_cache)
            {
                _cache.Add(cmd, (output, error));
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}

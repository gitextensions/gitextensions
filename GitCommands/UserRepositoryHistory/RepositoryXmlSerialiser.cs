using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using GitCommands.UserRepositoryHistory.Legacy;

namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Serialises or deserialises user's git repositories into XML.
    /// </summary>
    public sealed class RepositoryXmlSerialiser : IRepositorySerialiser<Repository>
    {
        /// <summary>
        /// Restores a list of user's git repositories from the supplied string.
        /// </summary>
        /// <param name="serialised">A serialised list of user's git repositories.</param>
        /// <returns>A list of user's git repositories, if successfully deserialised; otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="serialised"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        public IReadOnlyList<Repository>? Deserialize(string serialised)
        {
            if (string.IsNullOrEmpty(serialised))
            {
                throw new ArgumentException(nameof(serialised));
            }

            try
            {
                XmlSerializer serializer = new(typeof(RepositoryHistorySurrogate));
                using TextReader reader = new StringReader(serialised);
                if (serializer.Deserialize(reader) is RepositoryHistorySurrogate obj)
                {
                    return obj.Repositories;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Serialises the given list of user's git repositories.
        /// </summary>
        /// <param name="repositories">A list of user's git repositories.</param>
        /// <returns>A serialised list of user's git repositories, if successful; otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repositories"/> is <see langword="null"/>.</exception>
        public string? Serialize(IEnumerable<Repository> repositories)
        {
            if (repositories is null)
            {
                throw new ArgumentNullException(nameof(repositories));
            }

            try
            {
                RepositoryHistorySurrogate surrogate = new(repositories);
                using StringWriter sw = new();
                XmlSerializer serializer = new(typeof(RepositoryHistorySurrogate));
                XmlSerializerNamespaces ns = new();
                ns.Add(string.Empty, string.Empty);
                serializer.Serialize(sw, surrogate, ns);
                return sw.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }
    }
}

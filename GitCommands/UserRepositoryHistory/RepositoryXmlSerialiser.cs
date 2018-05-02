using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Provides the ability to serialise and deserialise collections of user's git repositories.
    /// </summary>
    public interface IRepositorySerialiser
    {
        /// <summary>
        /// Restores a list of user's git repositories from the supplied string.
        /// </summary>
        /// <param name="serialised">A serialised list of user's git repositories.</param>
        /// <returns>A list of user's git repositories.</returns>
        IReadOnlyList<Repository> Deserialize(string serialised);

        /// <summary>
        /// Serialises the given list of user's git repositories.
        /// </summary>
        /// <param name="repositories">A list of user's git repositories.</param>
        /// <returns>A serialised list of user's git repositories.</returns>
        string Serialize(IEnumerable<Repository> repositories);
    }

    /// <summary>
    /// Serialises or deserialises user's git repositories into XML.
    /// </summary>
    public sealed class RepositoryXmlSerialiser : IRepositorySerialiser
    {
        /// <summary>
        /// Restores a list of user's git repositories from the supplied string.
        /// </summary>
        /// <param name="serialised">A serialised list of user's git repositories.</param>
        /// <returns>A list of user's git repositories, if successfully deserialised; otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="serialised"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("serialised:null=>halt")]
        public IReadOnlyList<Repository> Deserialize([NotNull]string serialised)
        {
            if (string.IsNullOrEmpty(serialised))
            {
                throw new ArgumentException(nameof(serialised));
            }

            try
            {
                var serializer = new XmlSerializer(typeof(RepositoryHistorySurrogate));
                using (TextReader reader = new StringReader(serialised))
                {
                    if (serializer.Deserialize(reader) is RepositoryHistorySurrogate obj)
                    {
                        return obj.Repositories;
                    }
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
        [ContractAnnotation("repositories:null=>halt")]
        public string Serialize([NotNull]IEnumerable<Repository> repositories)
        {
            if (repositories == null)
            {
                throw new ArgumentNullException(nameof(repositories));
            }

            try
            {
                var surrogate = new RepositoryHistorySurrogate(repositories);
                using (var sw = new StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(RepositoryHistorySurrogate));
                    var ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    serializer.Serialize(sw, surrogate, ns);
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// The surrogate is necessary to provide backwards compatibility.
        /// The original implementation persisted user's git repositories under "RepositoryHistory" root node.
        /// </summary>
        [XmlRoot("RepositoryHistory")]
        public class RepositoryHistorySurrogate
        {
            public RepositoryHistorySurrogate(IEnumerable<Repository> repositories)
            {
                Repositories.AddRange(repositories);
            }

            public RepositoryHistorySurrogate()
            {
            }

            public List<Repository> Repositories { get; } = new List<Repository>();
        }
    }
}
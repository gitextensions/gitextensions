using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace GitCommands.UserRepositoryHistory.Legacy
{
    /// <summary>
    /// Provides the ability to deserialise collections of categorised user's git repositories.
    /// This functionality is provided for backwards compatibility ONLY and is purposed for
    /// migration of categorised user's git repositories from version prior to v3.
    /// </summary>
    internal sealed class RepositoryCategoryXmlSerialiser : IRepositorySerialiser<RepositoryCategory>
    {
        /// <summary>
        /// Restores a list of categorised user's git repositories from the supplied string.
        /// </summary>
        /// <param name="serialised">A serialised list of categorised user's git repositories.</param>
        /// <returns>A list of categorised user's git repositories.</returns>
        /// <exception cref="ArgumentException"><paramref name="serialised"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [CanBeNull]
        public IReadOnlyList<RepositoryCategory> Deserialize([NotNull] string serialised)
        {
            if (string.IsNullOrEmpty(serialised))
            {
                throw new ArgumentException(nameof(serialised));
            }

            try
            {
                var serializer = new XmlSerializer(typeof(List<RepositoryCategory>));
                using (TextReader reader = new StringReader(serialised))
                {
                    if (serializer.Deserialize(reader) is List<RepositoryCategory> obj)
                    {
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        string IRepositorySerialiser<RepositoryCategory>.Serialize(IEnumerable<RepositoryCategory> repositories)
        {
            throw new NotSupportedException("Serialisation is not supported, backwards compatibility only");
        }
    }
}
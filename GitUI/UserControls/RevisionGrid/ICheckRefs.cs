using System;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    public interface ICheckRefs
    {
        /// <summary>
        /// Checks whether the given hash is present in a collection.
        /// </summary>
        /// <param name="objectId">The hash to find.</param>
        /// <returns><see langword="true"/>, if the given hash if found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objectId"/> is <see langword="null"/>.</exception>
        bool Contains(ObjectId objectId);
    }
}

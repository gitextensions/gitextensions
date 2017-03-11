using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitItem
    {
        string Guid { get; }
        string Name { get; }

        IEnumerable<IGitItem> SubItems { get; }
    }
}
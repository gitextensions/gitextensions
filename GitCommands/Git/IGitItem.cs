using System.Collections.Generic;

namespace GitCommands
{
    public interface IGitItem
    {
        string Guid { get; }
        string Name { get; }

        IEnumerable<IGitItem> SubItems { get; }
    }
}
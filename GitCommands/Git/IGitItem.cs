using System;
using System.Collections.Generic;

namespace GitCommands
{
    public interface IGitItem
    {
        string Guid { get; set; }
        string Name { get; set; }

        List<IGitItem> SubItems
        {
            get;
        }
    }
}

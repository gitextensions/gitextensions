using System;
using System.Threading;

namespace GitCommands.Submodules
{
    public class SubmoduleStatusEventArgs : EventArgs
    {
        public SubmoduleInfoResult Info { get; }

        /// <summary>
        /// First update of the submodule structure. Status of the submodule will be updated asynchronously.
        /// </summary>
        public bool StructureUpdated { get; }

        public CancellationToken Token { get; }

        public SubmoduleStatusEventArgs(SubmoduleInfoResult info, bool structureUpdated, CancellationToken token)
        {
            Info = info;
            Token = token;
            StructureUpdated = structureUpdated;
        }
    }
}

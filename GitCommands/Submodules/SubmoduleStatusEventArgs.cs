using System;
using System.Threading;
using JetBrains.Annotations;

namespace GitCommands.Submodules
{
    public class SubmoduleStatusEventArgs : EventArgs
    {
        [NotNull]
        public SubmoduleInfoResult Info { get; }

        [NotNull]
        public CancellationToken Token { get; }

        public SubmoduleStatusEventArgs(SubmoduleInfoResult info, CancellationToken token)
        {
            Info = info;
            Token = token;
        }
    }
}

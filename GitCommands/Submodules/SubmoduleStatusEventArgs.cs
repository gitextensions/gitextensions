using System;
using System.Threading;

namespace GitCommands.Submodules
{
    public class SubmoduleStatusEventArgs : EventArgs
    {
        public SubmoduleInfoResult Info { get; }

        public CancellationToken Token { get; }

        public SubmoduleStatusEventArgs(SubmoduleInfoResult info, CancellationToken token)
        {
            Info = info;
            Token = token;
        }
    }
}

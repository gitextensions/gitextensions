using System;
using GitCommands;

namespace GitUI
{
    public class RefFilterOptionsEventArgs : EventArgs
    {
        public RefFilterOptionsEventArgs(RefFilterOptions refFilterOptions)
        {
            RefFilterOptions = refFilterOptions;
        }

        public RefFilterOptions RefFilterOptions { get; }
    }
}

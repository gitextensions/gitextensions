using System;

namespace GitExtensions.Plugins.DeleteUnusedBranches
{
    public class CheckBoxHeaderCellEventArgs : EventArgs
    {
        public CheckBoxHeaderCellEventArgs(bool checkedValue)
        {
            Checked = checkedValue;
        }

        public bool Checked { get; }
    }
}

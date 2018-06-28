using System;

namespace DeleteUnusedBranches
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

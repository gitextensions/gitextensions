using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public static class Abort
    {
        public static bool AbortCurrentAction()
        {
            if (MessageBox.Show("You can abort the current operation by resetting changes." + Environment.NewLine + "All changes since the last commit will be deleted." + Environment.NewLine + Environment.NewLine + "Do you want to reset changes?", "Abort", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (MessageBox.Show("Are you sure you want to DELETE all changes?" + Environment.NewLine + Environment.NewLine + "This action cannot be made undone.", "WARNING!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitCommands.GitCommands.ResetHard("");
                    return true;
                }
            }
            return false;
        }
    }
}

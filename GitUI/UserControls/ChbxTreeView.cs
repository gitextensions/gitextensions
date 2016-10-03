using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    /// <summary>
    ///  This is a regular Windows Forms TreeView with a checkbox double click bug fixed/workedaround.
    ///  The bug causes the AfterCheck event not to fire after double click, and the checkbox to be in a wrong state
    ///  It has been recognized by MS: 
    ///  https://connect.microsoft.com/VisualStudio/feedback/details/374516/treeview-control-does-not-fire-events-reliably-when-double-clicking-on-checkbox
    ///  but not fixed.
    ///  The workaround is to disable double click when it happens on a checkbox.
    /// </summary>
    class ChbxTreeView : TreeView
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x203) // identified double click
            {
                var localPos = PointToClient(Cursor.Position);
                var hitTestInfo = HitTest(localPos);
                if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                    m.Result = IntPtr.Zero;
                else
                    base.WndProc(ref m);
            }
            else base.WndProc(ref m);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.BrowseRepo
{
    /// <summary>
    /// makes sure that disabled items do not get a mouse over effect
    /// </summary>
    class CustomMenuRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Enabled)
            {

            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }
}

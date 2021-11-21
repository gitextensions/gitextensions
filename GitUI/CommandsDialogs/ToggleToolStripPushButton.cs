using System;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public class ToggleToolStripPushButton : ToolStripButton
    {
        private bool _toggled;

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            _toggled = Checked;
        }

        public override bool Selected => base.Selected || _toggled;
    }
}

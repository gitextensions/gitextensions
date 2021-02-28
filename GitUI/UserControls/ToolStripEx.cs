using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// This class adds on to the functionality provided in System.Windows.Forms.ToolStrip.
    /// </summary>
    public class ToolStripEx : ToolStrip, IToolStripEx
    {
        /// <summary>
        /// Gets or sets whether the ToolStripEx honors item clicks when its containing form does
        /// not have input focus.
        /// </summary>
        /// <remarks>
        /// Default value is false, which is the same behavior provided by the base ToolStrip class.
        /// </remarks>
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ClickThrough { get; set; }

        /// <inheritdoc/>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool DrawBorder { get; set; } = true;

        public ToolStripEx()
        {
            this.UseCustomRenderer();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (ClickThrough &&
                m.Msg == NativeMethods.WM_MOUSEACTIVATE &&
                m.Result == (IntPtr)NativeMethods.MA_ACTIVATEANDEAT)
            {
                m.Result = (IntPtr)NativeMethods.MA_ACTIVATE;
            }
        }
    }
}

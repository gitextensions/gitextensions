using System;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// This class adds on to the functionality provided in System.Windows.Forms.ToolStrip.
    /// </summary>
    public class ToolStripEx : ToolStrip
    {
        /// <summary>
        /// Gets or sets whether the ToolStripEx honors item clicks when its containing form does
        /// not have input focus.
        /// </summary>
        /// <remarks>
        /// Default value is false, which is the same behavior provided by the base ToolStrip class.
        /// </remarks>
        public bool ClickThrough { get; set; } = true;

        public ToolStripEx()
        {
            Renderer = new BorderlessToolStripRenderer();
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

        private sealed class BorderlessToolStripRenderer : ToolStripSystemRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip.GetType() == typeof(ToolStripEx))
                {
                    // skip border
                }
                else
                {
                    // render border
                    base.OnRenderToolStripBorder(e);
                }
            }
        }
    }
}

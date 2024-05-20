using System.ComponentModel;
using System.Reflection;

namespace GitUI
{
    /// <summary>
    /// This class adds on to the functionality provided in System.Windows.Forms.ToolStrip.
    /// </summary>
    public class ToolStripEx : ToolStrip, IToolStripEx
    {
        private readonly ToolStripButton _gripButton;

        public ToolStripEx()
        {
            Renderer = new ToolStripExSystemRenderer();

            PropertyInfo propGrip = GetType().GetProperty("Grip", BindingFlags.Instance | BindingFlags.NonPublic);
            _gripButton = propGrip.GetValue(this) as ToolStripButton;
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e)
        {
            base.OnItemAdded(e);

            if (e.Item is ToolStripDropDownItem item)
            {
                item.DropDownOpening += SplitButton_DropDownOpening;
                item.DropDownClosed += SplitButton_DropDownClosed;
            }
        }

        protected override void OnItemRemoved(ToolStripItemEventArgs e)
        {
            if (e.Item is ToolStripDropDownItem item)
            {
                item.DropDownOpening -= SplitButton_DropDownOpening;
                item.DropDownClosed -= SplitButton_DropDownClosed;
            }
        }

        private static void SplitButton_DropDownOpening(object? sender, EventArgs e)
        {
            if (sender is ToolStripDropDownItem { Owner: Control control })
            {
                // Suspends the control's rendering process.
                NativeMethods.SendMessageW(control.Handle, NativeMethods.WM_SETREDRAW, NativeMethods.FALSE, IntPtr.Zero);
            }
        }

        private static void SplitButton_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is ToolStripDropDownItem { Owner: Control control })
            {
                // Resumes the control's rendering process and trigger a redraw.
                NativeMethods.SendMessageW(control.Handle, NativeMethods.WM_SETREDRAW, NativeMethods.TRUE, IntPtr.Zero);
                control.Refresh();
            }
        }

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

        /// <summary>
        ///  Gets or sets whether the ToolStrip grip button is enabled.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool GripEnabled
        {
            get => _gripButton.Enabled;
            set => _gripButton.Enabled = value;
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

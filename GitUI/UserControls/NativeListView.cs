using System;

namespace GitUI.UserControls
{
    public class NativeListView : System.Windows.Forms.ListView
    {
        internal static event EventHandler BeginCreateHandle;
        internal static event EventHandler EndCreateHandle;

        protected override void CreateHandle()
        {
            BeginCreateHandle?.Invoke(this, EventArgs.Empty);
            base.CreateHandle();
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
            EndCreateHandle?.Invoke(this, EventArgs.Empty);
        }
    }
}

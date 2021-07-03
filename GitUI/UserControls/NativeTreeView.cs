using Windows.Win32;
using Windows.Win32.Foundation;

namespace GitUI.UserControls
{
    public class NativeTreeView : System.Windows.Forms.TreeView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            PInvoke.SetWindowTheme((HWND)Handle, "explorer", null);
        }
    }
}

namespace GitUI.UserControls
{
    public class NativeListView : System.Windows.Forms.ListView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
        }
    }
}

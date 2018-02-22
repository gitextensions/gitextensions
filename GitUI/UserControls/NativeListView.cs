namespace GitUI.UserControls
{
    public class NativeListView : System.Windows.Forms.ListView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
        }
    }
}

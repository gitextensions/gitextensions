namespace GitUI.UserControls
{
    public class NativeTreeView : System.Windows.Forms.TreeView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
        }
    }
}

namespace GitUI.UserControls
{
    public class NativeTreeView : TreeView
    {
        public NativeTreeView()
        {
            DoubleBuffered = true;
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
        }
    }
}

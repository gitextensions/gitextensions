namespace GitUI.UserControls
{
    public class NativeTreeView : TreeView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
        }

        public new bool DoubleBuffered
        {
            get => base.DoubleBuffered;
            set => base.DoubleBuffered = value;
        }
    }
}

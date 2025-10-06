namespace GitUI.UserControls;

public class NativeTreeView : TreeView
{
    public NativeTreeView()
    {
        DoubleBuffered = true;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (!Application.IsDarkModeEnabled)
        {
            // explorer style selection painting in left panel
            // Not needed in dark mode, this is the same for "DarkMode_Explorer"
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
        }
    }
}

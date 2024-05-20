using System.Reflection;

namespace GitExtUtils.GitUI;

public static class PropertyGridExtensions
{
    private static readonly FieldInfo? _gridViewField = typeof(PropertyGrid).GetField("_gridView", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? _moveSplitterToMethod =
        typeof(PropertyGrid).Assembly.GetType("System.Windows.Forms.PropertyGridInternal.PropertyGridView")
        ?.GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void SetLabelColumnWidth(this PropertyGrid grid, int width)
    {
        object view = _gridViewField?.GetValue(grid);
        _moveSplitterToMethod?.Invoke(view, [width]);
    }
}

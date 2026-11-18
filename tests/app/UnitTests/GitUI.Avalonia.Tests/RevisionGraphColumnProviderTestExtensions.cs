using System.Reflection;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitExtensionsTests;

// parity-scaffolding: observe retained-cache diagnostics without adding shipping accessor members.
internal static class RevisionGraphColumnProviderTestExtensions
{
    private static readonly FieldInfo Cache = typeof(RevisionGraphColumnProvider).GetField("_graphRenderCache", BindingFlags.Instance | BindingFlags.NonPublic)!;

    internal static VisibleRowRange GetCachedVisibleRange(this RevisionGraphColumnProvider.TestAccessor accessor)
        => ReadCacheProperty<VisibleRowRange>(accessor, "Range");

    internal static int GetLastRenderedRow(this RevisionGraphColumnProvider.TestAccessor accessor)
        => ReadCacheProperty<int>(accessor, "LastRenderedRow");

    private static T ReadCacheProperty<T>(RevisionGraphColumnProvider.TestAccessor accessor, string name)
    {
        object cache = Cache.GetValue(accessor.RevisionGraphColumnProvider)!;
        return (T)(cache.GetType().GetProperty(name)!.GetValue(cache)
            ?? throw new MissingMemberException(cache.GetType().FullName, name));
    }
}

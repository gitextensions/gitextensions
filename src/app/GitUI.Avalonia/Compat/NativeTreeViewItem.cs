using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GitUI.Compat;

/// <summary>
///  Supplies the repository tree's cancellable notification before IsExpanded takes effect.
/// </summary>
internal sealed class NativeTreeViewItem : TreeViewItem
{
    internal static readonly RoutedEvent<ExpansionChangingEventArgs> ExpansionChangingEvent =
        RoutedEvent.Register<NativeTreeViewItem, ExpansionChangingEventArgs>("ExpansionChanging", RoutingStrategies.Bubble);

    static NativeTreeViewItem()
    {
        IsExpandedProperty.OverrideMetadata<NativeTreeViewItem>(new StyledPropertyMetadata<bool>(coerce: static (owner, value) =>
        {
            NativeTreeViewItem item = (NativeTreeViewItem)owner;
            if (item.ItemCount == 0 || item.IsExpanded == value)
            {
                return value;
            }

            ExpansionChangingEventArgs args = new();
            item.RaiseEvent(args);
            return args.Cancel ? item.IsExpanded : value;
        }));
    }

    protected override Type StyleKeyOverride => typeof(TreeViewItem);

    internal sealed class ExpansionChangingEventArgs() : RoutedEventArgs(ExpansionChangingEvent)
    {
        public bool Cancel { get; set; }
    }
}

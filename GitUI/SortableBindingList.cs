using System.ComponentModel;
using System.Linq.Expressions;

namespace GitUI
{
    /// <summary>
    /// Custom sortable list, used to support user-defined sorting in <see cref="DataGridView"/>.
    /// </summary>
    /// <remarks>The class is abstract so that it is not used directly.<br/>Instead, it is expected to be derived, with a static constructor adding sortable properties through the <see cref="AddSortableProperty{TValue}"/> method</remarks>
    public abstract class SortableBindingList<T> : BindingList<T>
    {
        public static Dictionary<string, Comparison<T>> PropertyComparers { get; } = new();

        private List<T> Elements => (List<T>)Items;

        protected override bool SupportsSortingCore => true;

        public void AddRange(IEnumerable<T> items)
        {
            Elements.AddRange(items);

            // NOTE: adding items via wrapper's AddRange doesn't generate ListChanged event, so DataGridView doesn't update itself
            // There are two solutions:
            //  0. Add items one by one using direct this.Add method (without IList<T> wrapper).
            //     Too many ListChanged events will be generated (one per item), too many updates for gridview. Bad performance.
            //  1. Batch add items through Items wrapper's AddRange method.
            //     One reset event will be generated, one batch update for gridview. Ugly but fast code.
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void ApplySortCore(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
        {
            Elements.Sort(CreateComparison(propertyDescriptor, direction == ListSortDirection.Descending));
        }

        /// <summary>
        /// Adds custom property comparer.
        /// </summary>
        /// <typeparam name="TValue">Property type.</typeparam>
        /// <param name="expr">Property to sort by.</param>
        /// <param name="propertyComparer">Property values comparer.</param>
        protected internal static void AddSortableProperty<TValue>(Expression<Func<T, TValue>> expr, Comparison<T> propertyComparer)
        {
            PropertyComparers[((MemberExpression)expr.Body).Member.Name] = propertyComparer;
        }

        /// <summary>
        /// Creates a comparison to sort branches by specified property.
        /// </summary>
        /// <param name="propertyDescriptor">Property to sort by.</param>
        /// <param name="isReversedComparing">Use reversed sorting order.</param>
        public static Comparison<T> CreateComparison(PropertyDescriptor propertyDescriptor, bool isReversedComparing)
        {
            if (PropertyComparers.TryGetValue(propertyDescriptor.Name, out Comparison<T> comparer))
            {
                return isReversedComparing ? (x, y) => comparer(y, x) : comparer;
            }

            throw new NotSupportedException($"Custom sort by {propertyDescriptor.Name} property is not supported.");
        }
    }
}

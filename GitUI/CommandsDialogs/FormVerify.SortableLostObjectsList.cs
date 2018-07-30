using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace GitUI.CommandsDialogs
{
    partial class FormVerify
    {
        private sealed class SortableLostObjectsList : BindingList<LostObject>
        {
            public void AddRange(IEnumerable<LostObject> lostObjects)
            {
                LostObjects.AddRange(lostObjects);

                // NOTE: adding items via wrapper's AddRange doesn't generate ListChanged event, so DataGridView doesn't update itself
                // There are two solutions:
                //  0. Add items one by one using direct this.Add method (without IList<T> wrapper).
                //     Too many ListChanged events will be generated (one per item), too many updates for gridview. Bad performance.
                //  1. Batch add items through Items wrapper's AddRange method.
                //     One reset event will be generated, one batch update for gridview. Ugly but fast code.
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }

            protected override bool SupportsSortingCore => true;

            protected override void ApplySortCore(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
            {
                LostObjects.Sort(LostObjectsComparer.Create(propertyDescriptor, direction == ListSortDirection.Descending));
            }

            private List<LostObject> LostObjects => (List<LostObject>)Items;

            private static class LostObjectsComparer
            {
                private static readonly Dictionary<string, Comparison<LostObject>> PropertyComparers = new Dictionary<string, Comparison<LostObject>>();

                static LostObjectsComparer()
                {
                    AddSortableProperty(lostObject => lostObject.RawType, (x, y) => string.Compare(x.RawType, y.RawType, StringComparison.Ordinal));
                    AddSortableProperty(lostObject => lostObject.ObjectId, (x, y) => x.ObjectId.CompareTo(y.ObjectId));
                    AddSortableProperty(lostObject => lostObject.Author, (x, y) => string.Compare(x.Author, y.Author, StringComparison.CurrentCulture));
                    AddSortableProperty(lostObject => lostObject.Date, (x, y) => x.Date.HasValue && y.Date.HasValue
                        ? DateTime.Compare(x.Date.Value, y.Date.Value)
                        : Comparer<bool>.Default.Compare(x.Date.HasValue, y.Date.HasValue));
                    AddSortableProperty(lostObject => lostObject.Subject, (x, y) => string.Compare(x.Subject, y.Subject, StringComparison.CurrentCulture));
                }

                /// <summary>
                /// Creates a comparer to sort lostObjects by specified property.
                /// </summary>
                /// <param name="propertyDescriptor">Property to sort by.</param>
                /// <param name="isReversedComparing">Use reversed sorting order.</param>
                public static Comparison<LostObject> Create(PropertyDescriptor propertyDescriptor, bool isReversedComparing)
                {
                    if (PropertyComparers.TryGetValue(propertyDescriptor.Name, out var comparer))
                    {
                        return isReversedComparing ? (x, y) => comparer(y, x) : comparer;
                    }

                    throw new NotSupportedException(string.Format("Custom sort by {0} property is not supported.", propertyDescriptor.Name));
                }

                /// <summary>
                /// Adds custom property comparer.
                /// </summary>
                /// <typeparam name="T">Property type.</typeparam>
                /// <param name="expr">Property to sort by.</param>
                /// <param name="propertyComparer">Property values comparer.</param>
                private static void AddSortableProperty<T>(Expression<Func<LostObject, T>> expr, Comparison<LostObject> propertyComparer)
                {
                    PropertyComparers[((MemberExpression)expr.Body).Member.Name] = propertyComparer;
                }
            }
        }
    }
}

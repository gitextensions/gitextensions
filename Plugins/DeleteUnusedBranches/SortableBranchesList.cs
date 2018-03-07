using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace DeleteUnusedBranches
{
    /// <summary>
    /// Custom sortable binding branches list, use for support user-defined sorting in <see cref="DataGridView"/>.
    /// </summary>
    internal sealed class SortableBranchesList : BindingList<Branch>
    {
        public void AddRange(IEnumerable<Branch> branches)
        {
            Branches.AddRange(branches);
        }

        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
        {
            Branches.Sort(BranchesComparer.Create(propertyDescriptor, direction == ListSortDirection.Descending));
        }

        private List<Branch> Branches => (List<Branch>)Items;

        private static class BranchesComparer
        {
            private static readonly Dictionary<string, Comparison<Branch>> PropertyComparers = new Dictionary<string, Comparison<Branch>>();

            static BranchesComparer()
            {
                AddSortableProperty(branch => branch.Date, (x, y) => DateTime.Compare(x.Date, y.Date));
                AddSortableProperty(branch => branch.Name, (x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
                AddSortableProperty(branch => branch.Author, (x, y) => string.Compare(x.Author, y.Author, StringComparison.CurrentCulture));
            }

            /// <summary>
            /// Creates a comparer to sort branches by specified property.
            /// </summary>
            /// <param name="propertyDescriptor">Property to sort by.</param>
            /// <param name="isReversedComparing">Use reversed sorting order.</param>
            public static Comparison<Branch> Create(PropertyDescriptor propertyDescriptor, bool isReversedComparing)
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
            private static void AddSortableProperty<T>(Expression<Func<Branch, T>> expr, Comparison<Branch> propertyComparer)
            {
                PropertyComparers[((MemberExpression)expr.Body).Member.Name] = propertyComparer;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace FindLargeFiles
{
    /// <summary>
    /// Custom sortable binding branches list, use for support user-defined sorting in <see cref="DataGridView"/>.
    /// </summary>
    internal sealed class SortableObjectsList : BindingList<GitObject>
    {
        public void AddRange(IEnumerable<GitObject> objects)
        {
            GitObjects.AddRange(objects);
        }

        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
        {
            GitObjects.Sort(GitObjectsComparer.Create(propertyDescriptor, direction == ListSortDirection.Descending));
        }

        private List<GitObject> GitObjects => (List<GitObject>)Items;

        private static class GitObjectsComparer
        {
            private static readonly Dictionary<string, Comparison<GitObject>> PropertyComparers = new Dictionary<string, Comparison<GitObject>>();

            static GitObjectsComparer()
            {
                AddSortableProperty(branch => branch.LastCommitDate, (x, y) => DateTime.Compare(x.LastCommitDate, y.LastCommitDate));
                AddSortableProperty(branch => branch.Path, (x, y) => string.Compare(x.Path, y.Path, StringComparison.CurrentCulture));
                AddSortableProperty(branch => branch.SHA, (x, y) => string.Compare(x.SHA, y.SHA));
                AddSortableProperty(branch => branch.CommitCount, (x, y) => x.CommitCount.CompareTo(y.CommitCount));
                AddSortableProperty(branch => branch.Size, (x, y) => x.SizeInBytes.CompareTo(y.SizeInBytes));
                AddSortableProperty(branch => branch.CompressedSize, (x, y) => x.CompressedSizeInBytes.CompareTo(y.CompressedSizeInBytes));
            }

            /// <summary>
            /// Creates a comparer to sort branches by specified property.
            /// </summary>
            /// <param name="propertyDescriptor">Property to sort by.</param>
            /// <param name="isReversedComparing">Use reversed sorting order.</param>
            public static Comparison<GitObject> Create(PropertyDescriptor propertyDescriptor, bool isReversedComparing)
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
            private static void AddSortableProperty<T>(Expression<Func<GitObject, T>> expr, Comparison<GitObject> propertyComparer)
            {
                PropertyComparers[((MemberExpression)expr.Body).Member.Name] = propertyComparer;
            }
        }
    }
}

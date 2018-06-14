using System;
using System.Linq;
using System.Windows.Forms;

namespace GitUI
{
    public static class TableLayoutPanelExtensions
    {
        /// <summary>
        /// Adjusts the width of the required column to the largest value from the supplied set.
        /// </summary>
        /// <param name="table">The table to adjust.</param>
        /// <param name="columnIndex">The index of the column to resize.</param>
        /// <param name="widths">A set of available of widths to select the largest value from.</param>
        public static void AdjustWidthToSize(this TableLayoutPanel table, int columnIndex, params float[] widths)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (table.ColumnCount < 1)
            {
                throw new ArgumentException("The table must have at least one column");
            }

            if (columnIndex < 0 || columnIndex >= table.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, $"Column index must be within [0, {table.ColumnCount - 1}] range");
            }

            if (widths == null)
            {
                throw new ArgumentNullException(nameof(widths));
            }

            if (widths.Length < 1)
            {
                throw new ArgumentException("At least one width is required", nameof(widths));
            }

            table.ColumnStyles[columnIndex].SizeType = SizeType.Absolute;
            table.ColumnStyles[columnIndex].Width = widths.Max();
        }
    }
}
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
        /// <param name="controls">A set of controls to choose the widest from.</param>
        public static void AdjustWidthToSize(this TableLayoutPanel table, int columnIndex, params Control[] controls)
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

            if (controls == null)
            {
                throw new ArgumentNullException(nameof(controls));
            }

            if (controls.Length < 1)
            {
                throw new ArgumentException("At least one control is required", nameof(controls));
            }

            var requiredWidth = controls.Max(c => c.Margin.Left + c.Width + c.Margin.Right);
            table.ColumnStyles[columnIndex].SizeType = SizeType.Absolute;
            table.ColumnStyles[columnIndex].Width = requiredWidth;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GitUI.CommandsDialogs
{
    public sealed class DataGridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        private bool _wasAttached;
        private bool _selfChanging;

        /// <summary>
        /// Relative check box location (from cellbounds).
        /// </summary>
        private Rectangle _checkBoxArea;
        private CheckState _checkedState = CheckState.Indeterminate;

        public void AttachTo(DataGridViewCheckBoxColumn owningColumn)
        {
            if (_wasAttached)
            {
                throw new InvalidOperationException("This cell has already been attached to a column.");
            }

            if (DataGridView != null)
            {
                owningColumn.HeaderCell = this;
                owningColumn.HeaderText = string.Empty;

                DataGridView.CurrentCellDirtyStateChanged += OnCurrentCellDirtyStateChanged;
                DataGridView.Rows.CollectionChanged += OnCollectionChanged;
                UpdateCheckedState();
                _wasAttached = true;
            }
        }

        public void Detach()
        {
            if (_wasAttached)
            {
                DataGridView.CurrentCellDirtyStateChanged -= OnCurrentCellDirtyStateChanged;
                DataGridView.Rows.CollectionChanged -= OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            UpdateCheckedState();
        }

        private void OnCurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (DataGridView.CurrentCell.ColumnIndex != OwningColumn.Index || _selfChanging)
            {
                return;
            }

            DataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            UpdateCheckedState();
        }

        private void UpdateCheckedState()
        {
            var cellValues = Cells
                .Select(cell => cell.Value)
                .Cast<bool?>()
                .Distinct()
                .ToList();
            CheckedState = cellValues.Count == 1
                ? cellValues.Single() == true ? CheckState.Checked : CheckState.Unchecked
                : CheckState.Indeterminate;
        }

        private IEnumerable<DataGridViewCheckBoxCell> Cells
        {
            get
            {
                return DataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Select(row => row.Cells[OwningColumn.Index])
                    .Cast<DataGridViewCheckBoxCell>();
            }
        }

        private CheckState CheckedState
        {
            get { return _checkedState; }
            set
            {
                if (value == _checkedState)
                {
                    return;
                }

                _checkedState = value;
                DataGridView.InvalidateCell(this);
            }
        }

        private CheckBoxState CheckBoxState
        {
            get
            {
                switch (CheckedState)
                {
                    case CheckState.Unchecked:
                        return CheckBoxState.UncheckedNormal;
                    case CheckState.Checked:
                        return CheckBoxState.CheckedNormal;
                    case CheckState.Indeterminate:
                        return CheckBoxState.MixedNormal;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState,
            object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            var glyphSize = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
            var relativeLocation = new Point((cellBounds.Width / 2) - (glyphSize.Width / 2), (cellBounds.Height / 2) - (glyphSize.Height / 2));
            var absoluteLocation = new Point(cellBounds.Location.X + relativeLocation.X, cellBounds.Location.Y + relativeLocation.Y);

            _checkBoxArea = new Rectangle(relativeLocation, glyphSize);
            CheckBoxRenderer.DrawCheckBox(graphics, absoluteLocation, CheckBoxState);
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (_checkBoxArea.Contains(e.X, e.Y))
            {
                var newStateIsChecked = CheckedState != CheckState.Checked;
                CheckedState = newStateIsChecked ? CheckState.Checked : CheckState.Unchecked;
                _selfChanging = true;
                foreach (var cell in Cells)
                {
                    if (cell == DataGridView.CurrentCell)
                    {
                        // workaround for updating current cell
                        DataGridView.CurrentCell = null;
                    }

                    cell.Value = newStateIsChecked;
                }

                _selfChanging = false;
            }

            base.OnMouseClick(e);
        }
    }
}

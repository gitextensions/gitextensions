using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DeleteUnusedBranches
{
    public class DataGridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        private Point _checkBoxLocation;
        private Size _checkBoxSize;
        private Point _cellLocation = new Point();
        private CheckBoxState _cbState = CheckBoxState.UncheckedNormal;
        private bool _checked = false;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                DataGridView.InvalidateCell(this);
            }
        }

        public DataGridViewCheckBoxHeaderCell()
        {
        }

        protected override void Paint(Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates dataGridViewElementState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                dataGridViewElementState, value,
                formattedValue, errorText, cellStyle,
                advancedBorderStyle, paintParts);

            Point p = new Point();
            Size s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
            p.X = cellBounds.Location.X + (cellBounds.Width / 2) - (s.Width / 2);
            p.Y = cellBounds.Location.Y + (cellBounds.Height / 2) - (s.Height / 2);
            _cellLocation = cellBounds.Location;
            _checkBoxLocation = p;
            _checkBoxSize = s;

            if (_checked)
            {
                _cbState = CheckBoxState.CheckedNormal;
            }
            else
            {
                _cbState = CheckBoxState.UncheckedNormal;
            }

            CheckBoxRenderer.DrawCheckBox(graphics, _checkBoxLocation, _cbState);
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            Point p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
            if (p.X >= _checkBoxLocation.X &&
                p.X <= _checkBoxLocation.X + _checkBoxSize.Width &&
                p.Y >= _checkBoxLocation.Y &&
                p.Y <= _checkBoxLocation.Y + _checkBoxSize.Height)
            {
                Checked = !Checked;
                if (CheckBoxClicked != null)
                {
                    CheckBoxHeaderCellEventArgs args = new CheckBoxHeaderCellEventArgs
                    {
                        Checked = Checked
                    };
                    OnCheckBoxClicked(args);
                    DataGridView.InvalidateCell(this);
                }
            }

            base.OnMouseClick(e);
        }

        protected virtual void OnCheckBoxClicked(CheckBoxHeaderCellEventArgs e)
        {
            CheckBoxClicked?.Invoke(this, e);
        }

        public event EventHandler<CheckBoxHeaderCellEventArgs> CheckBoxClicked;
    }
}

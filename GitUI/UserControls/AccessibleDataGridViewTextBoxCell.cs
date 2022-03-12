using System.Windows.Forms;

namespace GitUI.UserControls
{
    /// <summary>
    /// A <cref>DataGridViewTextBoxCell</cref> with <cref>CreateAccessibilityInstance</cref> overridden
    /// in order to suppress the <cref>Name</cref> "Message Row nnn", which is redundant
    /// because the row and column numbers are provided in dedicated accessibility properties.
    /// </summary>
    public class AccessibleDataGridViewTextBoxCell : DataGridViewTextBoxCell
    {
        protected class DataGridViewTextBoxCellUnnamedAccessibleObject : DataGridViewTextBoxCellAccessibleObject
        {
            public DataGridViewTextBoxCellUnnamedAccessibleObject(DataGridViewCell? owner) : base(owner)
            {
            }

            public override string Name
                => string.Empty;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewTextBoxCellUnnamedAccessibleObject(this);
        }
    }
}

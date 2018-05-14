using System.Windows.Forms;

namespace GitUI.UserControls
{
    /// <inheritdoc />
    internal class ListViewGroupMouseEventArgs : MouseEventArgs
    {
        public ListViewGroupMouseEventArgs(MouseButtons button, ListViewGroup group, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta)
        {
            Group = group;
        }

        public ListViewGroup Group { get; }
    }
}
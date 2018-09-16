using System.Windows.Forms;

namespace GitUI.UserControls
{
    /// <inheritdoc />
    internal class ListViewGroupMouseEventArgs : MouseEventArgs
    {
        public ListViewGroupMouseEventArgs(MouseButtons button, ListViewGroupHitInfo groupHitInfo, int clicks, int delta)
            : base(button, clicks, groupHitInfo.Location.X, groupHitInfo.Location.Y, delta)
        {
            GroupInfo = groupHitInfo;
        }

        public ListViewGroupHitInfo GroupInfo { get; }
        public bool Handled { get; set; }
    }
}
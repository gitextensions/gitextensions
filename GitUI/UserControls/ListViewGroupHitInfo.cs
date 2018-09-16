using System.Drawing;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    public class ListViewGroupHitInfo
    {
        public ListViewGroupHitInfo(ListViewGroup @group, bool isCollapseButton, Point location)
        {
            Group = @group;
            IsCollapseButton = isCollapseButton;
            Location = location;
        }

        public ListViewGroup Group { get; }
        public bool IsCollapseButton { get; }
        public Point Location { get; }
    }
}
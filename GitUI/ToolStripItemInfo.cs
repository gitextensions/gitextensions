using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// Data class to populate data for a ToolStripMenuItem on a non-UI thread and then convert to a ToolStripMenuItem on the UI thread.
    /// This allows more work to be done on a background thread and additionally facilitates use of AddRange() which is MUCH quicker
    /// than adding individual items.
    /// </summary>
    class ToolStripItemInfo
    {
        public string Text { get; set; }
        public Image Image { get; set; }
        public object Tag { get; set; }
        public EventHandler ClickHandler { get; set; }
        public bool BoldFont { get; set; }

        /// <summary>
        /// Controls whether a ToolStripSeparator is created. If true, all other fields are ignored.
        /// </summary>
        public bool IsSeparator { get; set; }


        public ToolStripItemInfo() { }

        public ToolStripItemInfo(string text)
        {
            Text = text;
        }

        public ToolStripItemInfo(string text, object tag, EventHandler clickHandler)
        {
            Text = text;
            Tag = tag;
            ClickHandler = clickHandler;
        }

        public ToolStripItem createToolStripItem(int width)
        {
            if (IsSeparator)
                return new ToolStripSeparator();
            else
            {
                var menuItem = new ToolStripMenuItem(Text, Image, ClickHandler);
                menuItem.Tag = Tag;
                menuItem.Width = width;
                if (BoldFont)
                    menuItem.Font = new Font(menuItem.Font, FontStyle.Bold);
                return menuItem;
            }
        }
    }
}

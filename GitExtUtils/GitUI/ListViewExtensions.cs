using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public static class ListViewExtensions
    {
        public static IEnumerable<ListViewItem> SelectedItems(this ListView listView) =>
            listView.SelectedItems.Cast<ListViewItem>();

        public static IEnumerable<ListViewItem> Items(this ListView listView) =>
            listView.Items.Cast<ListViewItem>();

        public static IEnumerable<ListViewGroup> Groups(this ListView listView) =>
            listView.Groups.Cast<ListViewGroup>();

        public static T Tag<T>(this ListViewItem item) =>
            (T)item.Tag;

        public static T Tag<T>(this ListViewGroup grp) =>
            (T)grp.Tag;

        public static Image Image(this ListViewItem item)
        {
            if (item.ImageList == null || item.ImageIndex == -1)
            {
                return null;
            }

            return item.ImageList.Images[item.ImageIndex];
        }

        public static IEnumerable<T> ItemTags<T>(this ListView listView) =>
            listView.Items().Select(Tag<T>);

        public static IEnumerable<T> SelectedItemTags<T>(this ListView listView) =>
            listView.SelectedItems().Select(Tag<T>);

        /// <summary>
        /// <para>For practical purposes: The last <see cref="ListViewItem"/> added to selection.</para>
        /// <para>Actually: Focused item if selected, otherwise last item in <see cref="SelectedItems"/> list</para>
        /// </summary>
        public static ListViewItem LastSelectedItem(this ListView listView)
        {
            if (listView.FocusedItem?.Selected == true)
            {
                return listView.FocusedItem;
            }

            if (listView.SelectedItems.Count == 0)
            {
                return null;
            }

            return listView.SelectedItems[listView.SelectedItems.Count - 1];
        }
    }
}